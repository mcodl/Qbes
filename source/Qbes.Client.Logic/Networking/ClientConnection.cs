using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using Lidgren.Network;

using NetworkCommsDotNet;
using ConnectionTcp = NetworkCommsDotNet.Connection;

using Qbes.Client.Logic.GameProviders;
using Qbes.Common.Logic;
using Qbes.Common.Logic.Configuration;
using Qbes.Common.Logic.Exceptions;
using Qbes.Common.Logic.Networking;
using Connection = Qbes.Common.Logic.Networking.Connection;

namespace Qbes.Client.Logic.Networking
{
   internal sealed class ClientConnection
   {
      #region Private fields
      private NetClient _Client;
      private ConnectionTcp _ClientTcp;
      private object _CompressedLock = new object();
      private Connection _Connection;
      #endregion

      #region Constructors
      internal ClientConnection()
         : this(false)
      {
      }

      internal ClientConnection(bool dummy)
      {
         if (!dummy)
         {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            NetPeerConfiguration peerConfig = new NetPeerConfiguration("Qbes");
            peerConfig.AutoFlushSendQueue = true;
            peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.DebugMessage, true);
            peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.ErrorMessage, true);
            peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.VerboseDebugMessage, true);
            peerConfig.SetMessageTypeEnabled(NetIncomingMessageType.WarningMessage, true);
            _Client = new NetClient(peerConfig);
            _Client.RegisterReceivedCallback(new SendOrPostCallback(ReceivedMessage));
         }
      }
      #endregion

      internal Connection Connection
      {
         get
         {
            return _Connection;
         }
      }

      private void ReceivedMessage(object peer)
      {
         NetIncomingMessage message;
         while ((message = _Client.ReadMessage()) != null)
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
                     DiagnosticsManager.WriteMessage(message.ReadString());
                     break;
                  case NetIncomingMessageType.StatusChanged:
                     NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
                     if (status == NetConnectionStatus.Connected)
                     {
                        DiagnosticsManager.WriteMessage("New connection: " + message.SenderEndPoint.ToString());

                        // login
                        WorldHelper.ClientToServerProvider.LoginRequest(_Connection, ClientWorldManager.Instance.Configuration.Network.PlayerName, "<EMPTY>", ClientWorldManager.Instance.Version, ClientWorldManager.Instance.SkinHash);
                     }
                     else if (status == NetConnectionStatus.Disconnected)
                     {
                        DiagnosticsManager.WriteMessage("Disconnected: " + message.SenderEndPoint.ToString());
                        ClientWorldManager.Instance.Stop();
                     }
                     break;
                  case NetIncomingMessageType.Data:
                     if (message.LengthBytes < 3)
                     {
                        break;
                     }
                     byte messageCode = message.ReadByte();
                     int length = BitConverter.ToInt32(message.ReadBytes(4), 0);
                     byte[] data = message.ReadBytes(length);

                     MultiPlayerProvider.ServerToClientProvider.ProcessReceivedMessage(_Connection, ref messageCode, ref data);
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
               lock (_CompressedLock)
               {
                  _Connection.ProcessCompressedMessage(ref message);
               }
            });
         task.Start();
      }

      private void ReceivedMessageTcpString(PacketHeader header, ConnectionTcp connection, string message)
      {
         try
         {
            _ClientTcp = connection;
            Console.WriteLine("Server connection pair confirmation received: {0}", connection.ConnectionInfo.RemoteEndPoint);
            ClientWorldManager.Instance.RequestInitialTerrainData();
         }
         catch (Exception ex)
         {
            ExceptionHandler.LogException(ex);
         }
      }

      internal void SendTcpPairMessage(string entityID)
      {
         _ClientTcp.SendObject("Message", entityID);
      }

      internal void Start(MessagingConfiguration messagingSettings, IPEndPoint remoteEndPoint)
      {
         NetworkComms.AppendGlobalIncomingPacketHandler<byte[]>(Connection.TcpByteMessageName, ReceivedMessageTcpBytes);
         NetworkComms.AppendGlobalIncomingPacketHandler<string>("Message", ReceivedMessageTcpString);
         ConnectionInfo connectionInfo = new ConnectionInfo(ClientWorldManager.Instance.Configuration.Network.IpAddress, ClientWorldManager.Instance.Configuration.Network.PortTcp);
         _ClientTcp = TCPConnection.GetConnection(connectionInfo, true);

         _Client.Start();
         _Connection = new Connection(messagingSettings, _Client.Connect(remoteEndPoint), MultiPlayerProvider.ServerToClientProvider.ProcessReceivedMessage);
         _Connection.TcpConnection = _ClientTcp;
      }

      internal void Stop()
      {
         MultiPlayerProvider.ClientToServerProvider.DisconnectingNotification(_Connection);
         _Client.Disconnect("Disconnecting");
         if (_ClientTcp != null)
         {
            _ClientTcp.CloseConnection(false);
         }
      }
   }
}
