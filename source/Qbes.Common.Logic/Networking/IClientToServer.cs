using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Networking
{
   /// <summary>
   /// This interface contains method declarations that the client will be
   /// issuing to the server.
   /// </summary>
   public interface IClientToServer : IFileDataExchange
   {
      /// <summary>
      /// Chat message from client to the server.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="message">Chat message</param>
      void ChatMessage(Connection connection, string message);

      /// <summary>
      /// Message to server that the player is disconnecting.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      void DisconnectingNotification(Connection connection);

      /// <summary>
      /// Login message to the server with name and password.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="name">Player name</param>
      /// <param name="password">Player password (already hashed)</param>
      /// <param name="version">Client version</param>
      /// <param name="skinHash">Skin hash</param>
      void LoginRequest(Connection connection, string name, string password, string version, byte[] skinHash);

      /// <summary>
      /// Cube placement/removal notification.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="place">True to place, false to remove a cube</param>
      /// <param name="location">Current location</param>
      /// <param name="rotationLeft">Rotation left</param>
      /// <param name="rotationUp">Rotation up</param>
      /// <param name="materialId">Material ID</param>
      void PlaceOrRemoveCubeNotification(Connection connection, bool place, Point3D location, float rotationLeft, float rotationUp, ushort materialId);

      /// <summary>
      /// Player moved notification.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="location">Current location</param>
      /// <param name="rotationLeft">Rotation left</param>
      /// <param name="rotationUp">Rotation up</param>
      void PlayerMovedNotification(Connection connection, Point3D location, float rotationLeft, float rotationUp);

      /// <summary>
      /// Player moving notification.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="moveX">Move X</param>
      /// <param name="moveY">Move Y</param>
      /// <param name="moveZ">Move Z</param>
      /// <param name="rotationLeft">Rotation left</param>
      /// <param name="rotationUp">Rotation up (only aim direction)</param>
      void PlayerMovingNotification(Connection connection, sbyte moveX, sbyte moveY, sbyte moveZ, float rotationLeft, float rotationUp);

      /// <summary>
      /// Request for terrain data with player's area columns.
      /// </summary>
      /// <param name="connection">Client to server connection</param>
      /// <param name="terrainMessageId">Client's terrain request ID</param>
      /// <param name="location">Current location</param>
      /// <param name="rotationLeft">Rotation left</param>
      /// <param name="rotationUp">Rotation up</param>
      void TerrainDataRequest(Connection connection, int terrainMessageId, Point3D location, float rotationLeft, float rotationUp);
   }
}
