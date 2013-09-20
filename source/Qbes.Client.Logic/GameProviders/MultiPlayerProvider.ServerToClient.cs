using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic;
using Qbes.Common.Logic.Extensions;
using Qbes.Common.Logic.Networking;
using Qbes.Common.Logic.Networking.Constants;

namespace Qbes.Client.Logic.GameProviders
{
   internal sealed partial class MultiPlayerProvider
   {
      internal sealed class ServerToClient : IServerToClient
      {
         #region Private fields
         private Dictionary<int, List<DelayedSegmentUpdate>> _DelayedSegmentUpdates = new Dictionary<int, List<DelayedSegmentUpdate>>();
         private object _EntityCreateUpdateLock = new object();
         private object _TerrainUpdateLock = new object();
         #endregion

         public void ChatMessage(Connection connection, string message)
         {
            Console.WriteLine("> ChatMessage: {0}", message);

            ClientWorldManager.Instance.AddChatMessage(message);
         }

         public void DisconnectingNotification(Connection connection, string reason)
         {
            Console.WriteLine("> DisconnectingNotification: {0}", reason);
         }

         public void EntityCreateOrUpdateNotification(Connection connection, Entity entity)
         {
            Console.WriteLine("> EntityCreateOrUpdateNotification: {0}", entity.ID);

            lock (_EntityCreateUpdateLock)
            {
               if (entity.ID == ClientWorldManager.Instance.MultiPlayerEntityID)
               {
                  ClientWorldManager.Instance.SetPlayer(entity);
               }
               ClientWorldManager.Instance.Entities[entity.ID] = entity;

               if (entity is Player)
               {
                  // check if skin is in cache
                  Player receivedPlayer = (Player)entity;
                  if (entity.ID != ClientWorldManager.Instance.MultiPlayerEntityID)
                  {
                     byte[] skinHash = (receivedPlayer).SkinChecksumBytes;
                     if (!SkinManager.IsSkinOnDisk(ref skinHash))
                     {
                        // skin not found, request it
                        WorldHelper.ClientToServerProvider.RequestFile(connection, FileDataExchangeTypes.Skin, skinHash);
                     }
                  }
               }
            }
         }

         public void EntityDeleteNotification(Connection connection, ushort entityId)
         {
            Console.WriteLine("> EntityDeleteNotification: {0}", entityId);

            if (ClientWorldManager.Instance.Entities.ContainsKey(entityId))
            {
               ClientWorldManager.Instance.Entities.Remove(entityId);
            }
         }

         public void EntityMovedNotification(Connection connection, ushort entityId, Point3D location, float rotationLeft, float rotationUp, int movedTime)
         {
            //Console.WriteLine("> EntityMovedNotification: ID {0}, Pos {1}, L {2:0.0}, U {3:0.0}", entityId, location, rotationLeft, rotationUp);

            Entity entity = null;
            if (ClientWorldManager.Instance.Entities.TryGetValue(entityId, out entity))
            {
               entity.SetLocation(location, rotationLeft, rotationUp, false, movedTime);
            }
         }

         public void EntityMovingNotification(Connection connection, ushort entityId, sbyte moveX, sbyte moveY, sbyte moveZ, float rotationLeft, float rotationUp)
         {
            throw new NotImplementedException();
         }

         public void LoginResponse(Connection connection, byte result, ushort entityID, string additionalInfo)
         {
            Console.WriteLine("> LoginResponse: {0} - {1} (2)", entityID, result, additionalInfo);

            switch (result)
            {
               case LoginResults.Banned:
                  Console.WriteLine("You are banned from this server: " + (additionalInfo ?? "no additional info"));
                  ClientWorldManager.Instance.Stop();
                  break;
               case LoginResults.InvalidCredentials:
                  Console.WriteLine("Invalid credentials");
                  ClientWorldManager.Instance.Stop();
                  break;
               case LoginResults.Successful:
                  Console.WriteLine("Login successful");
                  ClientWorldManager.Instance.MultiPlayerEntityID = entityID;
                  ClientWorldManager.Instance.Client.SendTcpPairMessage(entityID.ToString());
                  break;
               case LoginResults.VersionMismatch:
                  Console.WriteLine(additionalInfo);
                  ClientWorldManager.Instance.Stop();
                  break;
            }
         }

         public void PlaceOrRemoveCubeNotification(Connection connection, bool place, Segment segment, int x, int y, int z, ushort materialId, uint version)
         {
            //Console.WriteLine("> PlaceOrRemoveCubeNotification");

            if (version > segment.Version)
            {
               segment.PlaceOrRemoveBlockAndSetVersion(place, x, y, z, materialId, version);
            }
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
                  string reason = Encoding.ASCII.GetString(data, 1, data[0]);

                  DisconnectingNotification(connection, reason);
                  break;
               case MessageCodes.EntityCreateUpdate:
                  Entity entity = Entity.CreateFromByteArray(data);

                  EntityCreateOrUpdateNotification(connection, entity);
                  break;
               case MessageCodes.EntityDelete:
                  ushort deleteEntityId = BitConverter.ToUInt16(data, 0);

                  EntityDeleteNotification(connection, deleteEntityId);
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
                  byte result = data[0];
                  ushort entityId = BitConverter.ToUInt16(data, 1);
                  byte additionalInfoLength = data[3];
                  string additionalInfo = null;
                  if (additionalInfoLength > 0)
                  {
                     additionalInfo = Encoding.ASCII.GetString(data, 4, additionalInfoLength);
                  }

                  LoginResponse(connection, result, entityId, additionalInfo);
                  break;
               case MessageCodes.Moved:
                  ushort movedEntityId = BitConverter.ToUInt16(data, 0);
                  Point3D movedLocation = new Point3D();
                  movedLocation.InitializeFromByteArray(ref data, 2);
                  float movedRotationLeft = BitConverter.ToSingle(data, Point3D.SerializedSize + 2);
                  float movedRotationUp = BitConverter.ToSingle(data, Point3D.SerializedSize + 6);
                  int movedTime = BitConverter.ToInt32(data, Point3D.SerializedSize + 10);

                  EntityMovedNotification(connection, movedEntityId, movedLocation, movedRotationLeft, movedRotationUp, movedTime);
                  break;
               case MessageCodes.Moving:
                  break;
               //case MessageCodes.MultipartCompressed:
               //   connection.ProcessMultipartMessage(ref data);
               //   break;
               case MessageCodes.PlaceOrRemoveCube:
                  bool place = (data[0] > 0);
                  int key = BitConverter.ToInt32(data, 1);
                  int x = BitConverter.ToInt32(data, 5);
                  int y = BitConverter.ToInt32(data, 9);
                  int z = BitConverter.ToInt32(data, 13);
                  ushort materialId = BitConverter.ToUInt16(data, 17);
                  uint version = BitConverter.ToUInt32(data, 19);

                  lock (_TerrainUpdateLock)
                  {
                     ClientSegment segment = ClientWorldManager.Instance.GetSegment(key);
                     if (segment != null)
                     {
                        PlaceOrRemoveCubeNotification(connection, place, segment, x, y, z, materialId, version);
                     }
                     else
                     {
                        if (!_DelayedSegmentUpdates.ContainsKey(key))
                        {
                           _DelayedSegmentUpdates.Add(key, new List<DelayedSegmentUpdate>());
                        }
                        _DelayedSegmentUpdates[key].Add(new DelayedSegmentUpdate()
                        {
                           MaterialID = materialId,
                           Place = place,
                           SegmentKey = key,
                           Version = version,
                           X = x,
                           Y = y,
                           Z = z
                        });
                     }
                  }
                  break;
               case MessageCodes.Terrain:
                  int terrainMessageId = BitConverter.ToInt32(data, 0);
                  byte current = data[4];
                  byte total = data[5];
                  int areaCount = data[6];

                  //Console.WriteLine("Received terrain data: terrainMessageId = {0}, current = {1}, total = {2}, areaCount = {3}", terrainMessageId, current, total, areaCount);

                  // clean areas prior to deserialization
                  lock (_TerrainUpdateLock)
                  {
                     ClientWorldManager.Instance.CleanAreas(ref terrainMessageId);
                  }

                  // deserialize the terrain
                  List<ClientArea> areas = new List<ClientArea>(areaCount);
                  int offset = 7;
                  for (int i = 0; i < areaCount; i++)
                  {
                     ClientArea area = new ClientArea();
                     area.InitializeFromByteArray<ClientSegment, ClientBox>(data, ref offset);
                     areas.Add(area);
                  }

                  TerrainDataResponse(connection, terrainMessageId, areas, current, total);
                  break;
            }
         }

         public void TerrainDataResponse<TArea>(Connection connection, int terrainMessageId, List<TArea> areas, byte current, byte total) where TArea : Area
         {
            Console.WriteLine("> TerrainDataResponse: {0} ({1} / {2})", terrainMessageId, current, total);

            lock (_TerrainUpdateLock)
            {
               bool last = ClientWorldManager.Instance.CachePartialTerrainData(terrainMessageId, areas.Cast<ClientArea>().ToList(), current, total);

               if (last)
               {
                  ClientWorldManager.Instance.OnAreasLoaded(terrainMessageId);

                  // check delayed terrain updates
                  List<int> updated = new List<int>();
                  foreach (var segmentUpdates in _DelayedSegmentUpdates)
                  {
                     Segment s = ClientWorldManager.Instance.GetSegment(segmentUpdates.Key);
                     if (s != null)
                     {
                        updated.Add(s.Key);
                        foreach (DelayedSegmentUpdate update in segmentUpdates.Value)
                        {
                           PlaceOrRemoveCubeNotification(connection, update.Place, s, update.X, update.Y, update.Z, update.MaterialID, update.Version);
                        }
                     }
                  }

                  // delete processed updates
                  foreach (int key in updated)
                  {
                     _DelayedSegmentUpdates.Remove(key);
                  }
               }
            }
         }

         #region IFileDataExchange
         public void AddFile(Connection connection, byte exchangeType, byte[] hash, byte[] data)
         {
            string path = FileDataExchangeUtils.CacheFile(ref exchangeType, ref hash, ref data);
            string hashString = hash.GetHexaString();

            Console.WriteLine("> AddFile: {0} ({1})", exchangeType, hashString);
         }

         public void CheckFileExistRequest(Connection connection, byte exchangeType, byte[] hash)
         {
            bool exists = FileDataExchangeUtils.CheckCache(ref exchangeType, ref hash);
            string hashString = hash.GetHexaString();

            Console.WriteLine("> CheckFileExistRequest: {0} ({1})", exchangeType, hashString);

            WorldHelper.ClientToServerProvider.CheckFileExistResponse(connection, exchangeType, hash, exists);
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

            byte[] fileData = FileDataExchangeUtils.GetCachedFile(ref exchangeType, ref hash);
            FileDataExchangeUtils.SendAddFileMessage(connection, ref exchangeType, ref hash, ref fileData);
         }
         #endregion
      }

      private sealed class DelayedSegmentUpdate
      {
         internal ushort MaterialID { get; set; }

         internal bool Place { get; set; }

         internal int SegmentKey { get; set; }

         internal uint Version { get; set; }

         internal int X { get; set; }

         internal int Y { get; set; }

         internal int Z { get; set; }
      }
   }
}
