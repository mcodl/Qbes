using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic;
using Qbes.Common.Logic.Networking;

namespace Qbes.Client.Logic.GameProviders
{
   internal sealed partial class SinglePlayerProvider
   {
      internal sealed class ClientToServer : IClientToServer
      {
         public void ChatMessage(Connection connection, string message)
         {
            // empty
         }

         public void DisconnectingNotification(Connection connection)
         {
            // empty
         }

         public void LoginRequest(Connection connection, string name, string password, string version, byte[] skinHash)
         {
            // empty
         }

         public void PlaceOrRemoveCubeNotification(Connection connection, bool place, Point3D location, float rotationLeft, float rotationUp, ushort materialId)
         {
            ClientWorldManager.Instance.Player.PlaceOrRemoveBlock(place);
         }

         public void PlayerMovedNotification(Connection connection, Point3D location, float rotationLeft, float rotationUp)
         {
            // empty
         }

         public void PlayerMovingNotification(Connection connection, sbyte moveX, sbyte moveY, sbyte moveZ, float rotationLeft, float rotationUp)
         {
            // empty
         }

         public void TerrainDataRequest(Connection connection, int terrainMessageId, Point3D location, float rotationLeft, float rotationUp)
         {
            ClientWorldManager.Instance.LoadPlayerColumns(terrainMessageId);
         }

         #region IFileDataExchange
         public void AddFile(Connection connection, byte exchangeType, byte[] hash, byte[] data)
         {
            // empty
         }

         public void CheckFileExistRequest(Connection connection, byte exchangeType, byte[] hash)
         {
            // empty
         }

         public void CheckFileExistResponse(Connection connection, byte exchangeType, byte[] hash, bool exists)
         {
            // empty
         }

         public void RequestFile(Connection connection, byte exchangeType, byte[] hash)
         {
            // empty
         }
         #endregion
      }
   }
}
