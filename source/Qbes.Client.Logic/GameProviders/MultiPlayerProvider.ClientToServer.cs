using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lidgren.Network;

using Qbes.Common.Logic;
using Qbes.Common.Logic.Extensions;
using Qbes.Common.Logic.Networking;
using Qbes.Common.Logic.Networking.Constants;

namespace Qbes.Client.Logic.GameProviders
{
   internal sealed partial class MultiPlayerProvider
   {
      internal sealed class ClientToServer : IClientToServer
      {
         public void ChatMessage(Connection connection, string message)
         {
            if (message.Length > 255)
            {
               message = message.Substring(0, 255);
            }

            Console.WriteLine("< ChatMessage: {0}", message);

            byte[] chatData = Encoding.ASCII.GetBytes(message);
            byte[] data = Connection.PrepareByteArray(MessageCodes.Chat, message.Length + 1);

            data[5] = (byte)chatData.Length;
            chatData.CopyTo(data, 6);

            connection.SendMessage(ref data, NetworkChannels.ChatChannel);
         }

         public void DisconnectingNotification(Connection connection)
         {
            Console.WriteLine("< DisconnectingNotification");

            byte[] data = Connection.PrepareByteArray(MessageCodes.Disconnecting, 0);

            connection.SendMessage(ref data, NetworkChannels.ConnectingDisconnectingChannel);
         }

         public void LoginRequest(Connection connection, string name, string password, string version, byte[] skinHash)
         {
            Console.WriteLine("< LoginRequest: {0} - {1} ({2})", name, version, skinHash.GetHexaString());

            byte[] nameData = Encoding.ASCII.GetBytes(name);
            byte[] passwordData = Encoding.ASCII.GetBytes(password);
            byte[] versionData = Encoding.ASCII.GetBytes(version);
            byte[] data = Connection.PrepareByteArray(MessageCodes.Login, nameData.Length + passwordData.Length + versionData.Length + 19);

            int offset = 5;

            data[offset] = (byte)nameData.Length;
            offset++;
            nameData.CopyTo(data, offset);
            offset += nameData.Length;

            data[offset] = (byte)passwordData.Length;
            offset++;
            passwordData.CopyTo(data, offset);
            offset += passwordData.Length;

            data[offset] = (byte)versionData.Length;
            offset++;
            versionData.CopyTo(data, offset);
            offset += versionData.Length;

            skinHash.CopyTo(data, offset);

            connection.SendMessage(ref data, NetworkChannels.ConnectingDisconnectingChannel);
         }

         public void PlaceOrRemoveCubeNotification(Connection connection, bool place, Point3D location, float rotationLeft, float rotationUp, ushort materialId)
         {
            //Console.WriteLine("< PlaceOrRemoveCubeNotification");

            byte[] data = Connection.PrepareByteArray(MessageCodes.PlaceOrRemoveCube, Point3D.SerializedSize + 11);

            data[5] = (byte)(place ? 1 : 0);
            location.Serialize(ref data, 6);
            BitConverter.GetBytes(rotationLeft).CopyTo(data, Point3D.SerializedSize + 6);
            BitConverter.GetBytes(rotationUp).CopyTo(data, Point3D.SerializedSize + 10);
            BitConverter.GetBytes(materialId).CopyTo(data, Point3D.SerializedSize + 14);

            connection.SendMessage(ref data, NetworkChannels.TerrainDataChannel);
         }

         public void PlayerMovedNotification(Connection connection, Point3D location, float rotationLeft, float rotationUp)
         {
            //Console.WriteLine("< PlayerMovedNotification: Pos {0}, L {1:0.0}, U {2:0.0}", location, rotationLeft, rotationUp);

            byte[] data = Connection.PrepareByteArray(MessageCodes.Moved, Point3D.SerializedSize + 8);

            location.Serialize(ref data, 5);
            BitConverter.GetBytes(rotationLeft).CopyTo(data, Point3D.SerializedSize + 5);
            BitConverter.GetBytes(rotationUp).CopyTo(data, Point3D.SerializedSize + 9);

            connection.SendMessage(ref data, NetDeliveryMethod.ReliableSequenced, NetworkChannels.EntityMessagesChannel);
         }

         public void PlayerMovingNotification(Connection connection, sbyte moveX, sbyte moveY, sbyte moveZ, float rotationLeft, float rotationUp)
         {
            throw new NotImplementedException();
         }

         public void TerrainDataRequest(Connection connection, int terrainMessageId, Point3D location, float rotationLeft, float rotationUp)
         {
            Console.WriteLine("< TerrainDataRequest: {0}", terrainMessageId);

            byte[] data = Connection.PrepareByteArray(MessageCodes.Terrain, Point3D.SerializedSize + 12);

            BitConverter.GetBytes(terrainMessageId).CopyTo(data, 5);
            location.Serialize(ref data, 9);
            BitConverter.GetBytes(rotationLeft).CopyTo(data, Point3D.SerializedSize + 9);
            BitConverter.GetBytes(rotationUp).CopyTo(data, Point3D.SerializedSize + 13);

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
}
