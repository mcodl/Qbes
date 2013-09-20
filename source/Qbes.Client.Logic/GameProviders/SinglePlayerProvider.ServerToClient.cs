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
      internal sealed class ServerToClient : IServerToClient
      {
         public void ChatMessage(Connection connection, string message)
         {
            // empty
         }

         public void DisconnectingNotification(Connection connection, string reason)
         {
            // empty
         }

         public void EntityCreateOrUpdateNotification(Connection connection, Entity entity)
         {
            // empty
         }

         public void EntityDeleteNotification(Connection connection, ushort entityId)
         {
            // empty
         }

         public void EntityMovedNotification(Connection connection, ushort entityId, Point3D location, float rotationLeft, float rotationUp, int movedTime)
         {
            // empty
         }

         public void EntityMovingNotification(Connection connection, ushort entityId, sbyte moveX, sbyte moveY, sbyte moveZ, float rotationLeft, float rotationUp)
         {
            // empty
         }

         public void LoginResponse(Connection connection, byte result, ushort entityID, string additionalInfo)
         {
            // empty
         }

         public void PlaceOrRemoveCubeNotification(Connection connection, bool place, Segment segment, int x, int y, int z, ushort materialId, uint version)
         {
            // empty
         }

         public void TerrainDataResponse<TArea>(Connection connection, int terrainMessageId, List<TArea> areas, byte current, byte total) where TArea : Area
         {
            // empty
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
