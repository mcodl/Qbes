using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lidgren.Network;

using Qbes.Common.Logic;
using Qbes.Common.Logic.Networking;
using Qbes.Common.Logic.Networking.Constants;

namespace Qbes.Server.Logic.Networking
{
   internal sealed class ServerToClient : IServerToClient
   {
      public void ChatMessage(Connection connection, string message)
      {
         Console.WriteLine("< ChatMessage: {0}", message);

         byte[] chatData = Encoding.ASCII.GetBytes(message);
         byte[] data = Connection.PrepareByteArray(MessageCodes.Chat, message.Length + 1);

         data[5] = (byte)chatData.Length;
         chatData.CopyTo(data, 6);

         connection.SendMessage(ref data, NetworkChannels.ChatChannel);
      }

      public void DisconnectingNotification(Connection connection, string reason)
      {
         Console.WriteLine("< DisconnectingNotification: {0}", reason);

         byte[] reasonData = Encoding.ASCII.GetBytes(reason);
         byte[] data = Connection.PrepareByteArray(MessageCodes.Disconnecting, reasonData.Length + 1);

         data[5] = (byte)reasonData.Length;
         reasonData.CopyTo(data, 6);

         connection.SendMessage(ref data, NetworkChannels.ConnectingDisconnectingChannel);
      }

      public void EntityCreateOrUpdateNotification(Connection connection, Entity entity)
      {
         Console.WriteLine("< EntityCreateOrUpdateNotification: {0}", entity.ID);

         byte[] entityData = entity.Serialize();
         byte[] data = Connection.PrepareByteArray(MessageCodes.EntityCreateUpdate, entityData.Length);

         entityData.CopyTo(data, 5);

         connection.SendMessage(ref data, NetworkChannels.EntityMessagesChannel);
      }

      public void EntityDeleteNotification(Connection connection, ushort entityId)
      {
         Console.WriteLine("< EntityDeleteNotification: {0}", entityId);

         byte[] data = Connection.PrepareByteArray(MessageCodes.EntityDelete, 2);

         BitConverter.GetBytes(entityId).CopyTo(data, 5);

         connection.SendMessage(ref data, NetworkChannels.EntityMessagesChannel);
      }

      public void EntityMovedNotification(Connection connection, ushort entityId, Point3D location, float rotationLeft, float rotationUp, int movedTime)
      {
         //Console.WriteLine("< EntityMovedNotification: ID {0}, Pos {1}, L {2:0.0}, U {3:0.0}", entityId, location, rotationLeft, rotationUp);

         byte[] data = Connection.PrepareByteArray(MessageCodes.Moved, Point3D.SerializedSize + 14);

         BitConverter.GetBytes(entityId).CopyTo(data, 5);
         location.Serialize(ref data, 7);
         BitConverter.GetBytes(rotationLeft).CopyTo(data, Point3D.SerializedSize + 7);
         BitConverter.GetBytes(rotationUp).CopyTo(data, Point3D.SerializedSize + 11);
         BitConverter.GetBytes(movedTime).CopyTo(data, Point3D.SerializedSize + 15);

         connection.SendMessage(ref data, NetDeliveryMethod.ReliableUnordered, NetworkChannels.EntityMessagesChannel);
      }

      public void EntityMovingNotification(Connection connection, ushort entityId, sbyte moveX, sbyte moveY, sbyte moveZ, float rotationLeft, float rotationUp)
      {
         throw new NotImplementedException();
      }

      public void LoginResponse(Connection connection, byte result, ushort entityID, string additionalInfo)
      {
         Console.WriteLine("< LoginResponse: {0} - {1} (2)", entityID, result, additionalInfo);

         int additionalInfoLength = 0;
         byte[] additionalInfoData = null;
         if (!string.IsNullOrEmpty(additionalInfo))
         {
            additionalInfoData = Encoding.ASCII.GetBytes(additionalInfo);
            additionalInfoLength = additionalInfoData.Length;
         }
         byte[] data = Connection.PrepareByteArray(MessageCodes.Login, additionalInfoLength + 4);

         data[5] = result;
         BitConverter.GetBytes(entityID).CopyTo(data, 6);
         data[8] = (byte)additionalInfoLength;
         if (additionalInfoData != null)
         {
            additionalInfoData.CopyTo(data, 9);
         }

         connection.SendMessage(ref data, NetworkChannels.ConnectingDisconnectingChannel);
      }

      public void PlaceOrRemoveCubeNotification(Connection connection, bool place, Segment segment, int x, int y, int z, ushort materialId, uint version)
      {
         //Console.WriteLine("< PlaceOrRemoveCubeNotification");

         byte[] data = Connection.PrepareByteArray(MessageCodes.PlaceOrRemoveCube, 23);

         data[5] = (byte)(place ? 1 : 0);
         BitConverter.GetBytes(segment.Key).CopyTo(data, 6);
         BitConverter.GetBytes(x).CopyTo(data, 10);
         BitConverter.GetBytes(y).CopyTo(data, 14);
         BitConverter.GetBytes(z).CopyTo(data, 18);
         BitConverter.GetBytes(materialId).CopyTo(data, 22);
         BitConverter.GetBytes(version).CopyTo(data, 24);

         connection.SendMessage(ref data, NetworkChannels.TerrainDataChannel);
      }

      public void TerrainDataResponse<TArea>(Connection connection, int terrainMessageId, List<TArea> areas, byte current, byte total) where TArea : Area
      {
         Console.WriteLine("< TerrainDataResponse: {0} ({1} / {2})", terrainMessageId, current, total);

         int areasCount = areas.Count;
         int length = 9;
         List<byte[]> areaDatas = new List<byte[]>();
         foreach (TArea area in areas)
         {
            areaDatas.Add(area.Serialize());
            length += areaDatas.Last().Length;
         }

         byte[] data = Connection.PrepareByteArray(MessageCodes.Terrain, length);
         BitConverter.GetBytes(terrainMessageId).CopyTo(data, 5);
         data[9] = current;
         data[10] = total;
         data[11] = (byte)areasCount;
         int offset = 12;
         foreach (byte[] areaData in areaDatas)
         {
            areaData.CopyTo(data, offset);
            offset += areaData.Length;
         }

         //Console.WriteLine("Sending terrain data: entity ID = {0}, terrainMessageId = {1}, current = {2}, total = {3}, areaCount = {4}", connection.Player.ID, terrainMessageId, current, total, areasCount);

         connection.SendMessage(ref data, NetworkChannels.TerrainDataChannel);
      }

      #region IFileDataExchange
      public void AddFile(Connection connection, byte exchangeType, byte[] hash, byte[] data)
      {
         FileDataExchangeUtils.SendAddFileMessage(connection, ref exchangeType, ref hash, ref data);
      }

      public void CheckFileExistRequest(Connection connection, byte exchangeType, byte[] hash)
      {
         FileDataExchangeUtils.SendCheckFileExistRequest(connection, ref exchangeType, ref hash);
      }

      public void CheckFileExistResponse(Connection connection, byte exchangeType, byte[] hash, bool exists)
      {
         FileDataExchangeUtils.SendCheckFileExistResponse(connection, ref exchangeType, ref hash, ref exists);
      }

      public void RequestFile(Connection connection, byte exchangeType, byte[] hash)
      {
         FileDataExchangeUtils.SendRequestFile(connection, ref exchangeType, ref hash);
      }
      #endregion
   }
}
