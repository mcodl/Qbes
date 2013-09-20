using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using Lidgren.Network;

using NetworkCommsDotNet;
using ConnectionTcp = NetworkCommsDotNet.Connection;

using Qbes.Common.Logic;
using Qbes.Common.Logic.Networking;
using Connection = Qbes.Common.Logic.Networking.Connection;

namespace Qbes.Server.Logic.Networking
{
   internal sealed class ConnectionList
   {
      #region Private fields
      private Dictionary<ushort, Connection> _EntityIdConnections = new Dictionary<ushort, Connection>();
      private Dictionary<long, Connection> _RemoteIdConnections = new Dictionary<long, Connection>();
      private Dictionary<ConnectionTcp, Connection> _TcpConnections = new Dictionary<ConnectionTcp, Connection>();
      #endregion

      internal Connection this[ushort entityId]
      {
         get
         {
            if (_EntityIdConnections.ContainsKey(entityId))
            {
               return _EntityIdConnections[entityId];
            }

            return null;
         }
      }

      internal Connection this[long remoteId]
      {
         get
         {
            if (_RemoteIdConnections.ContainsKey(remoteId))
            {
               return _RemoteIdConnections[remoteId];
            }

            return null;
         }
      }

      internal Connection this[ConnectionTcp connectionTcp]
      {
         get
         {
            if (_TcpConnections.ContainsKey(connectionTcp))
            {
               return _TcpConnections[connectionTcp];
            }

            return null;
         }
      }

      internal void AddConnection(NetConnection connection, CompletedReceivedMultipartMessage callback)
      {
         Connection newConnection = new Connection(ServerWorldManager.Instance.Configuration.MessagingConfigurationNode, connection, callback);
         _RemoteIdConnections.Add(connection.RemoteUniqueIdentifier, newConnection);
      }

      internal List<Connection> GetAuthenticatedPlayers(ushort? excludeEntityId)
      {
         List<Connection> result = new List<Connection>();

         foreach (Connection connection in new List<Connection>(_EntityIdConnections.Values))
         {
            if (excludeEntityId.HasValue && connection.Player.ID == excludeEntityId.Value)
            {
               continue;
            }
            result.Add(connection);
         }

         return result;
      }

      internal Player GetPlayer(long remoteId)
      {
         return _RemoteIdConnections[remoteId].Player;
      }

      internal Player GetPlayer(NetConnection connection)
      {
         return GetPlayer(connection.RemoteUniqueIdentifier);
      }

      internal bool IsPlayerConnected(ushort playerId)
      {
         return _EntityIdConnections.ContainsKey(playerId);
      }

      internal void RemoveConnection(long remoteId)
      {
         Connection toRemove = _RemoteIdConnections[remoteId];
         if (toRemove.TcpConnection != null)
         {
            _TcpConnections.Remove(toRemove.TcpConnection);
         }
         _RemoteIdConnections.Remove(remoteId);

         if (toRemove.Player != null)
         {
            _EntityIdConnections.Remove(toRemove.Player.ID);
         }
      }

      internal void RemoveConnection(NetConnection connection)
      {
         RemoveConnection(connection.RemoteUniqueIdentifier);
      }

      internal void SetPlayer(long remoteId, Player player)
      {
         Connection connection = _RemoteIdConnections[remoteId];
         connection.Player = player;

         _EntityIdConnections[player.ID] = connection;
      }

      internal void SetPlayer(Connection connection, Player player)
      {
         SetPlayer(connection.NetConnection.RemoteUniqueIdentifier, player);
      }

      internal void SetPlayerConnectionTcp(ushort playerId, ConnectionTcp connection)
      {
         this[playerId].TcpConnection = connection;
         _TcpConnections[connection] = this[playerId];
      }
   }
}
