using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Lidgren.Network;

using NetworkCommsDotNet;
using ConnectionTcp = NetworkCommsDotNet.Connection;

using Qbes.Common.Logic;
using Qbes.Common.Logic.Configuration;
using Qbes.Common.Logic.Exceptions;
using Qbes.Common.Logic.Networking;
using Connection = Qbes.Common.Logic.Networking.Connection;
using Qbes.Common.Logic.Networking.Constants;
using Qbes.Server.Logic.Configuration;
using Qbes.Server.Logic.Networking.Web;

namespace Qbes.Server.Logic.Networking
{
   internal sealed class ServerManager
   {
      #region Static fields
      private static readonly ServerManager _Instance = new ServerManager();
      #endregion

      #region Private fields
      private ConnectionList _Clients = new ConnectionList();
      private readonly ClientToServer _ClientToServer = new ClientToServer();
      private Dictionary<ushort, ManualResetEvent> _ClientWaits = new Dictionary<ushort, ManualResetEvent>();
      private NetServer _Server;
      private readonly ServerToClient _ServerToClient = new ServerToClient();
      private WebHost _WebServer;
      #endregion

      #region Constructors
      private ServerManager()
      {
         WorldHelper.SetMessagingProviders(_ClientToServer, _ServerToClient);

         SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

         NetPeerConfiguration peerConfig = ServerWorldManager.Instance.Configuration.Network.CreateNetPeerConfigurationUdp();
         peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.DebugMessage, true);
         peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.ErrorMessage, true);
         peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.VerboseDebugMessage, true);
         peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.WarningMessage, true);

         _Server = new NetServer(peerConfig);
         _Server.RegisterReceivedCallback(new SendOrPostCallback(ReceivedMessage));

         _WebServer = new WebHost(ServerWorldManager.Instance.Configuration.Network.Web);
      }

      static ServerManager()
      {
         // empty
      }
      #endregion

      internal ClientToServer ClientToServerProvider
      {
         get
         {
            return _ClientToServer;
         }
      }

      internal void Flush()
      {
         _Server.FlushSendQueue();
      }

      internal List<Connection> GetAuthenticatedPlayers()
      {
         return GetAuthenticatedPlayers(null);
      }

      internal List<Connection> GetAuthenticatedPlayers(ushort? excludeEntityId)
      {
         return _Clients.GetAuthenticatedPlayers(excludeEntityId);
      }

      internal Connection GetPlayerConnection(ushort entityId)
      {
         return _Clients[entityId];
      }

      internal static ServerManager Instance
      {
         get
         {
            return _Instance;
         }
      }

      internal bool IsPlayerConnected(ushort playerId)
      {
         return _Clients.IsPlayerConnected(playerId);
      }

      private void ReceivedMessage(object peer)
      {
         NetIncomingMessage message;
         while ((message = _Server.ReadMessage()) != null)
         {
            try
            {
               // handle incoming message
               switch (message.MessageType)
               {
                  case NetIncomingMessageType.DebugMessage:
                  case NetIncomingMessageType.ErrorMessage:
                  case NetIncomingMessageType.WarningMessage:
                  case NetIncomingMessageType.VerboseDebugMessage:
                     Console.WriteLine(message.ReadString());
                     break;
                  case NetIncomingMessageType.StatusChanged:
                     NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
                     if (status == NetConnectionStatus.Connected)
                     {
                        _Clients.AddConnection(message.SenderConnection, _ClientToServer.ProcessReceivedMessage);
                        Console.WriteLine("New connection: " + message.SenderEndPoint.ToString());
                     }
                     else if (status == NetConnectionStatus.Disconnected)
                     {
                        Player disconnectingPlayer = _Clients.GetPlayer(message.SenderConnection);
                        _Clients.RemoveConnection(message.SenderConnection);
                        Console.WriteLine("Disconnected: " + message.SenderEndPoint.ToString());

                        if (disconnectingPlayer != null)
                        {
                           ServerWorldManager.Instance.UnloadPlayer(disconnectingPlayer);

                           if (_ClientWaits.ContainsKey(disconnectingPlayer.ID))
                           {
                              _ClientWaits[disconnectingPlayer.ID].Set();
                              _ClientWaits[disconnectingPlayer.ID].Close();
                              _ClientWaits.Remove(disconnectingPlayer.ID);
                           }

                           // inform all other about disconnected player
                           foreach (Connection other in GetAuthenticatedPlayers(disconnectingPlayer.ID))
                           {
                              ServerToClientProvider.EntityDeleteNotification(other, disconnectingPlayer.ID);
                           }
                        }
                     }
                     break;
                  case NetIncomingMessageType.Data:
                     if (message.LengthBytes < 3)
                     {
                        break;
                     }
                     Connection connection = _Clients[message.SenderConnection.RemoteUniqueIdentifier];
                     byte messageCode = message.ReadByte();
                     int length = BitConverter.ToInt32(message.ReadBytes(4), 0);
                     byte[] data = message.ReadBytes(length);

                     _ClientToServer.ProcessReceivedMessage(connection, ref messageCode, ref data);
                     break;
                  default:
                     //Output("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
                     break;
               }
            }
            catch (Exception ex)
            {
               ExceptionHandler.LogException(ex);
            }
         }
      }

      private void ReceivedMessageTcpBytes(PacketHeader header, ConnectionTcp connection, byte[] message)
      {
         HandledTask task = new HandledTask(() =>
            {
            _Clients[connection].ProcessCompressedMessage(ref message);
            });
         task.Start();
      }

      private void ReceivedMessageTcpString(PacketHeader header, ConnectionTcp connection, string message)
      {
         try
         {
            Console.WriteLine("Paired TCP connection for {0}: {1}", message, connection.ConnectionInfo.RemoteEndPoint);
            _Clients.SetPlayerConnectionTcp(Convert.ToUInt16(message), connection);
            connection.SendObject("Message", "Connected");
         }
         catch (Exception ex)
         {
            ExceptionHandler.LogException(ex);
         }
      }

      internal ServerToClient ServerToClientProvider
      {
         get
         {
            return _ServerToClient;
         }
      }

      internal void SetPlayerConnection(Connection connection, Player player)
      {
         // check if there's a previous session active
         Connection previous = _Clients[player.ID];
         if (previous != null)
         {
            Console.WriteLine("Disconnecting previous active session for player: " + player.PlayerName);

            _ClientWaits[player.ID] = new ManualResetEvent(false);
            ServerToClientProvider.DisconnectingNotification(previous, "New player session has been started");
            _Clients.RemoveConnection(player.ID);
            previous.Disconnect("New player session has been started");
            _ClientWaits[player.ID].WaitOne();
         }

         _Clients.SetPlayer(connection, player);
      }

      internal void Start()
      {
         // first start TCP server
         NetworkComms.AppendGlobalIncomingPacketHandler<byte[]>(Connection.TcpByteMessageName, ReceivedMessageTcpBytes);
         NetworkComms.AppendGlobalIncomingPacketHandler<string>("Message", ReceivedMessageTcpString);
         NetworkComms.DefaultListenPort = ServerWorldManager.Instance.Configuration.Network.PortTcp;
         TCPConnection.DefaultMSPerKBSendTimeout = ServerWorldManager.Instance.Configuration.Network.InitialMSPerKBSendTimeout;
         TCPConnection.StartListening(false);

         // then start UDP server
         _Server.Start();

         // finally start HTTP server
         _WebServer.Start();
      }

      internal void Stop()
      {
         _Server.Shutdown("Server is shutting down...");
         NetworkComms.Shutdown();
         _WebServer.Stop();
      }
   }
}
