using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic;
using Qbes.Common.Logic.Exceptions;
using Qbes.Common.Logic.Extensions;
using Qbes.Common.Logic.Networking;
using Qbes.Common.Logic.Networking.Constants;

namespace Qbes.Server.Logic.Networking
{
   internal sealed class ClientToServer : IClientToServer
   {
      public void ChatMessage(Connection connection, string message)
      {
         Console.WriteLine("> ChatMessage: {0} - {1}", connection.Player.ID, message);
         message = connection.Player.PlayerName + ": " + message;

         // resend to all other players
         ushort senderId = connection.Player.ID;
         foreach (Connection other in ServerManager.Instance.GetAuthenticatedPlayers(senderId))
         {
            ServerManager.Instance.ServerToClientProvider.ChatMessage(other, message);
         }
      }

      public void DisconnectingNotification(Connection connection)
      {
         Console.WriteLine("> DisconnectingNotification: {0} ", connection.Player.PlayerName);
      }

      public void LoginRequest(Connection connection, string name, string password, string version, byte[] skinHash)
      {
         Console.WriteLine("> LoginRequest: {0} - {1} ({2})", name, version, skinHash.GetHexaString());
         
         bool success = false;

         if (version != ServerWorldManager.Instance.Version)
         {
            // incorrect client version
            string message = "Login failed (incorrect client version: " + version + ", expected: " + ServerWorldManager.Instance.Version + ")";
            Console.WriteLine(message);
            ServerManager.Instance.ServerToClientProvider.LoginResponse(connection, LoginResults.VersionMismatch, 0, message);
         }
         else if (ServerWorldManager.Instance.Configuration.Security.BannedPlayers.Contains(name))
         {
            // banned player
            Console.WriteLine("Login failed (player banned)");
            ServerManager.Instance.ServerToClientProvider.LoginResponse(connection, LoginResults.Banned, 0, "Banned player");
         }
         else if (ServerWorldManager.Instance.Configuration.Security.BannedIpAddresses.Contains(connection.NetConnection.RemoteEndPoint.Address.ToString()))
         {
            // banned IP address
            Console.WriteLine("Login failed (IP address banned)");
            ServerManager.Instance.ServerToClientProvider.LoginResponse(connection, LoginResults.Banned, 0, "Banned IP address");
         }
         else if (ServerWorldManager.Instance.Configuration.Security.RequiresAuthentication)
         {
            // TODO: authenticate
            success = true;
         }
         else
         {
            success = true;
         }

         if (success)
         {
            Console.WriteLine("Login successful");

            // retrieve entity ID of the player
            ushort entityId = ServerWorldManager.Instance.GetPlayerId(name);

            // check player's skin existence in cache
            if (!SkinManager.IsSkinOnDisk(ref skinHash))
            {
               ServerManager.Instance.ServerToClientProvider.RequestFile(connection, FileDataExchangeTypes.Skin, skinHash);
            }

            // check if authenticated already
            Player newPlayer = (Player)ServerWorldManager.Instance.PrepareConnectedPlayer(entityId);
            newPlayer.SetSkinHash(skinHash);
            ServerManager.Instance.SetPlayerConnection(connection, newPlayer);
            ServerManager.Instance.ServerToClientProvider.LoginResponse(connection, LoginResults.Successful, entityId, "Login successful");

            // send all entities in the game to that player
            foreach (Entity entity in ServerWorldManager.Instance.Entities.Values)
            {
               if (entity.ID != entityId &&
                   !ServerWorldManager.Instance.IsPlayerConnected(entity as Player))
               {
                  continue;
               }
               ServerManager.Instance.ServerToClientProvider.EntityCreateOrUpdateNotification(connection, entity);
            }

            // send the newly logged in entity to all players
            foreach (Connection other in ServerManager.Instance.GetAuthenticatedPlayers(entityId))
            {
               ServerManager.Instance.ServerToClientProvider.EntityCreateOrUpdateNotification(other, newPlayer);
            }
         }

         // flush the send queue
         ServerManager.Instance.Flush();
      }

      public void PlaceOrRemoveCubeNotification(Connection connection, bool place, Point3D location, float rotationLeft, float rotationUp, ushort materialId)
      {
         //Console.WriteLine("> PlaceOrRemoveCubeNotification");

         // adjust player's location and heading
         PlayerMovedNotification(connection, location, rotationLeft, rotationUp);

         // set material
         connection.Player.SelectedMaterial = materialId;

         int x = -1;
         int y = -1;
         int z = -1;
         uint version = 0;
         if (!connection.Player.PlaceOrRemoveBlock(place, out x, out y, out z, out version))
         {
            // ignore
            return;
         }

         // distribute info about changed terrain
         Segment segment = ServerWorldManager.Instance.GetSegmentBasedOnInsidePoint(x, y, z);
         foreach (Connection other in ServerManager.Instance.GetAuthenticatedPlayers())
         {
            if (ServerWorldManager.Instance.IsPlayersArea(other.Player, segment.Area.Key))
            {
               ServerManager.Instance.ServerToClientProvider.PlaceOrRemoveCubeNotification(other, place, segment, x, y, z, connection.Player.GetSelectedMaterial(), version);
            }
         }
      }

      public void PlayerMovedNotification(Connection connection, Point3D location, float rotationLeft, float rotationUp)
      {
         //Console.WriteLine("> PlayerMovedNotification: Pos {0}, L {1:0.0}, U {2:0.0}", location, rotationLeft, rotationUp);
         
         connection.Player.SetLocation(location, rotationLeft, rotationUp, true, Environment.TickCount);

         foreach (Connection other in ServerManager.Instance.GetAuthenticatedPlayers(connection.Player.ID))
         {
            ServerManager.Instance.ServerToClientProvider.EntityMovedNotification(other, connection.Player.ID, location, rotationLeft, rotationUp, connection.Player.LastLocationUpdate);
         }
      }

      public void PlayerMovingNotification(Connection connection, sbyte moveX, sbyte moveY, sbyte moveZ, float rotationLeft, float rotationUp)
      {
         throw new NotImplementedException();
      }

      public void ProcessReceivedMessage(Connection connection, ref byte messageCode, ref byte[] data)
      {
         switch (messageCode)
         {
            case MessageCodes.Chat:
               string chatMessage = Encoding.ASCII.GetString(data, 1, data[0]);
               ChatMessage(connection, chatMessage);
               break;
            case MessageCodes.Disconnecting:
               DisconnectingNotification(connection);
               break;
            case MessageCodes.FileDataExchangeAdd:
               byte addExchangeType = 0;
               byte[] addHash = null;
               byte[] addFileData = null;
               FileDataExchangeUtils.ParseAddFileMessage(ref data, out addExchangeType, out addHash, out addFileData);

               AddFile(connection, addExchangeType, addHash, addFileData);
               break;
            case MessageCodes.FileDataExchangeRequest:
               byte requestExchangeType = 0;
               byte[] requestHash = null;
               FileDataExchangeUtils.ParseCheckFileExistRequest(ref data, out requestExchangeType, out requestHash);

               CheckFileExistRequest(connection, requestExchangeType, requestHash);
               break;
            case MessageCodes.FileDataExchangeResponse:
               byte responseExchangeType = 0;
               byte[] responseHash = null;
               bool responseExists = false;
               FileDataExchangeUtils.ParseCheckFileExistResponse(ref data, out responseExchangeType, out responseHash, out responseExists);

               CheckFileExistResponse(connection, responseExchangeType, responseHash, responseExists);
               break;
            case MessageCodes.FileDataRequest:
               byte fileExchangeType = 0;
               byte[] fileHash = null;

               FileDataExchangeUtils.ParseFileRequest(ref data, out fileExchangeType, out fileHash);

               RequestFile(connection, fileExchangeType, fileHash);
               break;
            case MessageCodes.GeneralNo:
               break;
            case MessageCodes.GeneralYes:
               break;
            case MessageCodes.Login:
               int offset = 0;

               int nameLength = data[offset];
               offset++;
               string name = Encoding.ASCII.GetString(data, offset, nameLength);
               offset += nameLength;

               int passwordLength = data[offset];
               offset++;
               string password = Encoding.ASCII.GetString(data, offset, passwordLength);
               offset += passwordLength;

               int versionLength = data[offset];
               offset++;
               string version = Encoding.ASCII.GetString(data, offset, versionLength);
               offset += versionLength;

               byte[] skinHash = new byte[16];
               for (int i = 0; i < 16; i++)
               {
                  skinHash[i] = data[offset + i];
               }

               LoginRequest(connection, name, password, version, skinHash);
               break;
            case MessageCodes.Moved:
               Point3D movedLocation = new Point3D();
               movedLocation.InitializeFromByteArray(ref data, 0);
               float rotationLeft = BitConverter.ToSingle(data, Point3D.SerializedSize);
               float rotationUp = BitConverter.ToSingle(data, Point3D.SerializedSize + 4);

               PlayerMovedNotification(connection, movedLocation, rotationLeft, rotationUp);
               break;
            case MessageCodes.Moving:
               break;
            //case MessageCodes.MultipartCompressed:
            //   connection.ProcessMultipartMessage(ref data);
            //   break;
            case MessageCodes.PlaceOrRemoveCube:
               bool place = (data[0] > 0);
               Point3D placeLocation = new Point3D();
               placeLocation.InitializeFromByteArray(ref data, 1);
               float placeRotationLeft = BitConverter.ToSingle(data, Point3D.SerializedSize + 1);
               float placeRotationUp = BitConverter.ToSingle(data, Point3D.SerializedSize + 5);
               ushort placeMaterialId = BitConverter.ToUInt16(data, Point3D.SerializedSize + 9);

               PlaceOrRemoveCubeNotification(connection, place, placeLocation, placeRotationLeft, placeRotationUp, placeMaterialId);
               break;
            case MessageCodes.Terrain:
               int terrainMessageId = BitConverter.ToInt32(data, 0);
               Point3D terrainLocation = new Point3D();
               terrainLocation.InitializeFromByteArray(ref data, 4);
               float terrainRotationLeft = BitConverter.ToSingle(data, Point3D.SerializedSize + 4);
               float terrainRotationUp = BitConverter.ToSingle(data, Point3D.SerializedSize + 8);

               TerrainDataRequest(connection, terrainMessageId, terrainLocation, terrainRotationLeft, terrainRotationUp);
               break;
         }
      }

      public void TerrainDataRequest(Connection connection, int terrainMessageId, Point3D location, float rotationLeft, float rotationUp)
      {
         Console.WriteLine("> TerrainDataRequest: {0}", terrainMessageId);

         PlayerMovedNotification(connection, location, rotationLeft, rotationUp);

         ServerWorldManager.Instance.LoadClientColumns(terrainMessageId, connection.Player);
      }

      #region IFileDataExchange
      public void AddFile(Connection connection, byte exchangeType, byte[] hash, byte[] data)
      {
         string hashString = hash.GetHexaString();

         Console.WriteLine("> AddFile: {0} ({1})", exchangeType, hashString);

         FileDataExchangeUtils.CacheFile(ref exchangeType, ref hash, ref data);

         // distribute to connected clients
         foreach (Connection other in ServerManager.Instance.GetAuthenticatedPlayers())
         {
            ServerManager.Instance.ServerToClientProvider.CheckFileExistRequest(other, exchangeType, hash);
         }
      }

      public void CheckFileExistRequest(Connection connection, byte exchangeType, byte[] hash)
      {
         string hashString = hash.GetHexaString();

         Console.WriteLine("> CheckFileExistRequest: {0} ({1})", exchangeType, hashString);

         bool exists = FileDataExchangeUtils.CheckCache(ref exchangeType, ref hash);

         ServerManager.Instance.ServerToClientProvider.CheckFileExistResponse(connection, exchangeType, hash, exists);
      }

      public void CheckFileExistResponse(Connection connection, byte exchangeType, byte[] hash, bool exists)
      {
         string hashString = hash.GetHexaString();

         Console.WriteLine("> CheckFileExistResponse: {0} - {1} ({2})", exchangeType, exists, hashString);

         if (exists)
         {
            return;
         }

         byte[] fileData = FileDataExchangeUtils.GetCachedFile(ref exchangeType, ref hash);
         FileDataExchangeUtils.SendAddFileMessage(connection, ref exchangeType, ref hash, ref fileData);
      }

      public void RequestFile(Connection connection, byte exchangeType, byte[] hash)
      {
         string hashString = hash.GetHexaString();

         Console.WriteLine("> RequestFile: {0} ({1})", exchangeType, hashString);

         if (FileDataExchangeUtils.CheckCache(ref exchangeType, ref hash))
         {
            byte[] fileData = FileDataExchangeUtils.GetCachedFile(ref exchangeType, ref hash);
            FileDataExchangeUtils.SendAddFileMessage(connection, ref exchangeType, ref hash, ref fileData);
         }
      }
      #endregion
   }
}
