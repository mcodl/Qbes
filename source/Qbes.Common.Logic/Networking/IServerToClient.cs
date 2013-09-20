using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Networking
{
   /// <summary>
   /// This interface contains method declarations that the server will be
   /// issuing to the client.
   /// </summary>
   public interface IServerToClient : IFileDataExchange
   {
      /// <summary>
      /// Chat message resent from server to client.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="message">Chat message</param>
      void ChatMessage(Connection connection, string message);

      /// <summary>
      /// Message to client that the player will be disconnected.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="reason">Reason</param>
      void DisconnectingNotification(Connection connection, string reason);

      /// <summary>
      /// Message to client with either a new entity or update to an existing
      /// entity.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="entity">Entity object</param>
      void EntityCreateOrUpdateNotification(Connection connection, Entity entity);

      /// <summary>
      /// Message to client that entity has been deleted.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="entityId">Entity ID</param>
      void EntityDeleteNotification(Connection connection, ushort entityId);

      /// <summary>
      /// Entity moved notification.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="entityId">Entity ID</param>
      /// <param name="location">Current location</param>
      /// <param name="rotationLeft">Rotation left</param>
      /// <param name="rotationUp">Rotation up</param>
      /// <param name="movedTime">Move time</param>
      void EntityMovedNotification(Connection connection, ushort entityId, Point3D location, float rotationLeft, float rotationUp, int movedTime);

      /// <summary>
      /// Entity moving notification.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="entityId">Entity ID</param>
      /// <param name="moveX">Move X</param>
      /// <param name="moveY">Move Y</param>
      /// <param name="moveZ">Move Z</param>
      /// <param name="rotationLeft">Rotation left</param>
      /// <param name="rotationUp">Rotation up (only aim direction)</param>
      void EntityMovingNotification(Connection connection, ushort entityId, sbyte moveX, sbyte moveY, sbyte moveZ, float rotationLeft, float rotationUp);

      /// <summary>
      /// Server's response to client's login request.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="result">Login result code (see LoginResults)</param>
      /// <param name="entityID">Player's entity ID</param>
      /// <param name="additionalInfo">Any additional info</param>
      void LoginResponse(Connection connection, byte result, ushort entityID, string additionalInfo);

      /// <summary>
      /// Cube placement/removal notification.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="place">True to place, false to remove a cube</param>
      /// <param name="segment">Target segment</param>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <param name="version">Segment version with this change</param>
      /// <param name="materialId">Material ID</param>
      void PlaceOrRemoveCubeNotification(Connection connection, bool place, Segment segment, int x, int y, int z, ushort materialId, uint version);

      /// <summary>
      /// Server's response to client's terrain request.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <typeparam name="TArea">Area type</typeparam>
      /// <param name="terrainMessageId">Client's terrain request ID</param>
      /// <param name="areas">New areas for client</param>
      /// <param name="current">Current terrain part</param>
      /// <param name="total">Total parts count</param>
      void TerrainDataResponse<TArea>(Connection connection, int terrainMessageId, List<TArea> areas, byte current, byte total)
         where TArea : Area;
   }
}
