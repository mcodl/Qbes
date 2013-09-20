using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Extensions;
using Qbes.Common.Logic.Networking.Constants;

namespace Qbes.Common.Logic.Networking
{
   /// <summary>
   /// Contains common file data exchange logic.
   /// </summary>
   public static class FileDataExchangeUtils
   {
      /// <summary>
      /// Adds a file to appropriate cache.
      /// </summary>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="fileHash">File checksum array</param>
      /// <param name="fileData">File data array</param>
      public static string CacheFile(ref byte exchangeType, ref byte[] fileHash, ref byte[] fileData)
      {
         switch (exchangeType)
         {
            case FileDataExchangeTypes.Skin:
               return SkinManager.SaveSkin(ref fileHash, ref fileData);
            default:
               // TODO: log and handle properly
               return string.Empty;
         }
      }

      /// <summary>
      /// Checks whether entry is in a cache.
      /// </summary>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="fileHash">File checksum array</param>
      /// <returns>True if in cache</returns>
      public static bool CheckCache(ref byte exchangeType, ref byte[] fileHash)
      {
         switch (exchangeType)
         {
            case FileDataExchangeTypes.Skin:
               return SkinManager.IsSkinOnDisk(ref fileHash);
            default:
               // TODO: log and handle properly
               return true;
         }
      }

      /// <summary>
      /// Gets cached file.
      /// </summary>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="fileHash">File checksum array</param>
      /// <returns>Cached file</returns>
      public static byte[] GetCachedFile(ref byte exchangeType, ref byte[] fileHash)
      {
         switch (exchangeType)
         {
            case FileDataExchangeTypes.Skin:
               return SkinManager.GetSkin(ref fileHash);
            default:
               // TODO: log and handle properly
               return null;
         }
      }

      private static void GetExchangeTypeAndHash(ref byte[] receivedData, out byte exchangeType, out byte[] hash)
      {
         exchangeType = receivedData[0];

         hash = new byte[16];
         for (int i = 0; i < 16; i++)
         {
            hash[i] = receivedData[i + 1];
         }
      }

      /// <summary>
      /// Parses received add file message.
      /// </summary>
      /// <param name="receivedData">Received data</param>
      /// <param name="exchangeType">Parsed exchange type</param>
      /// <param name="hash">Parsed file checksum array</param>
      /// <param name="fileData">Parsed file data array</param>
      public static void ParseAddFileMessage(ref byte[] receivedData, out byte exchangeType, out byte[] hash, out byte[] fileData)
      {
         GetExchangeTypeAndHash(ref receivedData, out exchangeType, out hash);

         int fileDataSize = BitConverter.ToInt32(receivedData, 17);
         fileData = new byte[fileDataSize];
         for (int i = 0; i < fileDataSize; i++)
         {
            fileData[i] = receivedData[i + 21];
         }
      }

      /// <summary>
      /// Parses a request to check whether a local file exists on the other side.
      /// </summary>
      /// <param name="receivedData">Received data</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      public static void ParseCheckFileExistRequest(ref byte[] receivedData, out byte exchangeType, out byte[] hash)
      {
         GetExchangeTypeAndHash(ref receivedData, out exchangeType, out hash);
      }

      /// <summary>
      /// Parses a response to check request.
      /// </summary>
      /// <param name="receivedData">Received data</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      /// <param name="exists">True if exists</param>
      public static void ParseCheckFileExistResponse(ref byte[] receivedData, out byte exchangeType, out byte[] hash, out bool exists)
      {
         GetExchangeTypeAndHash(ref receivedData, out exchangeType, out hash);

         exists = (receivedData[17] > 0);
      }

      /// <summary>
      /// Parses a file request.
      /// </summary>
      /// <param name="receivedData">Received data</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      public static void ParseFileRequest(ref byte[] receivedData, out byte exchangeType, out byte[] hash)
      {
         GetExchangeTypeAndHash(ref receivedData, out exchangeType, out hash);
      }

      /// <summary>
      /// Sends an add file message to the other side.
      /// </summary>
      /// <param name="connection">Connection object</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      /// <param name="fileData">File data</param>
      public static void SendAddFileMessage(Connection connection, ref byte exchangeType, ref byte[] hash, ref byte[] fileData)
      {
         string hashString = hash.GetHexaString();

         Console.WriteLine("< AddFile: {0} ({1})", exchangeType, hashString);

         byte[] data = Connection.PrepareByteArray(MessageCodes.FileDataExchangeAdd, fileData.Length + 21);

         data[5] = exchangeType;
         hash.CopyTo(data, 6);
         BitConverter.GetBytes(fileData.Length).CopyTo(data, 22);
         fileData.CopyTo(data, 26);

         connection.SendMessage(ref data, NetworkChannels.FileDataExchangeChannel);
      }

      /// <summary>
      /// Sends a request to check whether a local file exists on the other side.
      /// </summary>
      /// <param name="connection">Connection object</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      public static void SendCheckFileExistRequest(Connection connection, ref byte exchangeType, ref byte[] hash)
      {
         string hashString = hash.GetHexaString();

         Console.WriteLine("< CheckFileExistRequest: {0} ({1})", exchangeType, hashString);

         byte[] data = Connection.PrepareByteArray(MessageCodes.FileDataExchangeRequest, 17);

         data[5] = exchangeType;
         hash.CopyTo(data, 6);

         connection.SendMessage(ref data, NetworkChannels.FileDataExchangeChannel);
      }

      /// <summary>
      /// Sends a response to check request.
      /// </summary>
      /// <param name="connection">Connection object</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      /// <param name="exists">True if exists</param>
      public static void SendCheckFileExistResponse(Connection connection, ref byte exchangeType, ref byte[] hash, ref bool exists)
      {
         string hashString = hash.GetHexaString();

         Console.WriteLine("< CheckFileExistResponse: {0} - {1} ({2})", exchangeType, exists, hashString);

         byte[] data = Connection.PrepareByteArray(MessageCodes.FileDataExchangeResponse, 18);

         data[5] = exchangeType;
         hash.CopyTo(data, 6);
         data[22] = (byte)(exists ? 1 : 0);

         connection.SendMessage(ref data, NetworkChannels.FileDataExchangeChannel);
      }

      /// <summary>
      /// Sends a file request.
      /// </summary>
      /// <param name="connection">Connection object</param>
      /// <param name="exchangeType">Exchange type</param>
      /// <param name="hash">Content checksum</param>
      public static void SendRequestFile(Connection connection, ref byte exchangeType, ref byte[] hash)
      {
         string hashString = hash.GetHexaString();

         Console.WriteLine("< RequestFile: {0} ({1})", exchangeType, hashString);

         byte[] data = Connection.PrepareByteArray(MessageCodes.FileDataRequest, 18);

         data[5] = exchangeType;
         hash.CopyTo(data, 6);

         connection.SendMessage(ref data, NetworkChannels.FileDataExchangeChannel);
      }
   }
}
