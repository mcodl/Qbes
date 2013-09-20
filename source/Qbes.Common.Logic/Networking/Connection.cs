using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using Lidgren.Network;

using NetworkCommsDotNet;
using ConnectionTcp = NetworkCommsDotNet.Connection;

using Decoder = SevenZip.Compression.LZMA.Decoder;
using Encoder = SevenZip.Compression.LZMA.Encoder;

using Qbes.Common.Logic.Configuration;
using Qbes.Common.Logic.Networking.Constants;

namespace Qbes.Common.Logic.Networking
{
   /// <summary>
   /// Instances of this class wrap a NetConnection instance with additional
   /// data needed for the Qbes client/server architecture.
   /// </summary>
   public sealed class Connection
   {
      #region Constants
      /// <summary>
      /// Message header length.
      /// </summary>
      public const int MessageHeaderLength = 5;
      private const int MultipartHeaderLength = 8;
      /// <summary>
      /// Multipart message header length.
      /// </summary>
      public const int MultipartMessageHeaderLength = 13;
      private const int OffsetMultipartData = 13;
      private const int OffsetMultipartMessageID = 5;
      private const int OffsetMultipartPartCount = 11;
      private const int OffsetMultipartPartIndex = 9;
      /// <summary>
      /// Name of the TCP messages.
      /// </summary>
      public const string TcpByteMessageName = "Bytes";
      #endregion

      #region Static fields
      private static uint _NextMultipartMessageId;
      private static object _NextMultipartMessageIdLock = new object();
      #endregion

      #region Private fields
      private CompletedReceivedMultipartMessage _CompletedReceivedMultipartCallback;
      private NetConnection _Connection;
      private int _MaxMessageSize;
      private Dictionary<uint, List<MultipartMessage>> _MultipartMessageCache = new Dictionary<uint, List<MultipartMessage>>();
      private object _MultipartMessageLock = new object();
      private Player _PlayerEntity;
      private IPEndPoint _RemoteEndPoint;
      private object _SendLock = new object();
      private uint _WaitingMultipartId;
      private object _WaitingMultipartIdLock = new object();
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a new connection instance.
      /// </summary>
      /// <param name="messagingSettings">Qbes messaging configuration
      /// node</param>
      /// <param name="connection">NetConnection object</param>
      /// <param name="callback">Callback for completed received multipart
      /// messages</param>
      public Connection(MessagingConfiguration messagingSettings, NetConnection connection, CompletedReceivedMultipartMessage callback)
      {
         _MaxMessageSize = messagingSettings.MaxMessageSize;
         _Connection = connection;
         _RemoteEndPoint = connection.RemoteEndPoint;
         _CompletedReceivedMultipartCallback = callback;
      }
      #endregion

      /// <summary>
      /// Closes the connections.
      /// </summary>
      /// <param name="message">Message</param>
      public void Disconnect(string message)
      {
         NetConnection.Disconnect(message);
         if (TcpConnection != null)
         {
            TcpConnection.CloseConnection(false);
            NetworkComms.Shutdown();
         }
      }

      [Obsolete]
      private static uint GetNextMultipartMessageID()
      {
         lock (_NextMultipartMessageIdLock)
         {
            return ++_NextMultipartMessageId;
         }
      }

      /// <summary>
      /// Gets the NetConnection object.
      /// </summary>
      public NetConnection NetConnection
      {
         get
         {
            return _Connection;
         }
      }

      /// <summary>
      /// Gets or sets the player entity.
      /// </summary>
      public Player Player
      {
         get
         {
            return _PlayerEntity;
         }
         set
         {
            _PlayerEntity = value;
         }
      }

      /// <summary>
      /// Prepares a byte array with a message code and length header.
      /// </summary>
      /// <param name="messageCode">Message code</param>
      /// <param name="length">Length of the actual message</param>
      /// <returns>Byte array for the message</returns>
      /// <remarks>Remember that first 3 bytes are occupied by the code and
      /// length header</remarks>
      public static byte[] PrepareByteArray(byte messageCode, int length)
      {
         byte[] data = new byte[length + MessageHeaderLength];
         data[0] = messageCode;
         BitConverter.GetBytes(length).CopyTo(data, 1);

         return data;
      }

      [Obsolete]
      internal List<MultipartMessage> PrepareMultipartMessages(ref byte[] data)
      {
         List<MultipartMessage> result = new List<MultipartMessage>();

         // prepare streams
         MemoryStream inData = new MemoryStream(data);
         inData.Position = 0;
         MemoryStream outData = new MemoryStream();

         // compress the data
         Encoder compressor = new Encoder();
         compressor.WriteCoderProperties(outData);
         outData.Write(BitConverter.GetBytes(inData.Length), 0, 8);
         compressor.Code(inData, outData, inData.Length, -1, null);

         outData.Position = 0;

         // close the in stream as it is no longer needed
         inData.Close();

         // assemble multipart messages
         int maxMessageSize = _MaxMessageSize - MultipartMessageHeaderLength;
         ushort partCount = Convert.ToUInt16(Math.Ceiling((double)outData.Length / (double)maxMessageSize));
         uint messageId = GetNextMultipartMessageID();
         _WaitingMultipartId = messageId;
         for (ushort partIndex = 0; partIndex < partCount; partIndex++)
         {
            int currentOffset = partIndex * maxMessageSize;
            int bytesLeft = (int)outData.Length - currentOffset;
            int currentSize = (bytesLeft > maxMessageSize ? maxMessageSize : bytesLeft);

            byte[] partData = new byte[currentSize];
            outData.Read(partData, 0, currentSize);

            result.Add(new MultipartMessage()
            {
               Data = partData,
               MessageID = messageId,
               PartCount = partCount,
               PartIndex = partIndex
            });
         }

         // close the out data stream
         outData.Close();

         // return messages
         return result;
      }

      /// <summary>
      /// Processes compressed message.
      /// </summary>
      /// <param name="compressed">Compressed message</param>
      public void ProcessCompressedMessage(ref byte[] compressed)
      {
         // write the parts into a stream
         MemoryStream inData = new MemoryStream(compressed);
         inData.Position = 0;

         // decompress the stream
         MemoryStream outData = new MemoryStream();
         Decoder decompressor = new Decoder();

         // Read the decoder properties
         byte[] properties = new byte[5];
         inData.Read(properties, 0, 5);

         // Read in the decompress file size.
         byte[] dataLengthBytes = new byte[8];
         inData.Read(dataLengthBytes, 0, 8);
         long dataLength = BitConverter.ToInt64(dataLengthBytes, 0);

         decompressor.SetDecoderProperties(properties);
         decompressor.Code(inData, outData, inData.Length, dataLength, null);
         outData.Position = 0;

         // extract the actual message data
         byte messageCode = (byte)outData.ReadByte();
         byte[] messageLengthData = new byte[4];
         outData.Read(messageLengthData, 0, 4);
         int messageLength = BitConverter.ToInt32(messageLengthData, 0);
         byte[] messageData = new byte[messageLength];
         outData.Read(messageData, 0, messageLength);

         Console.WriteLine("> !COMPRESSED-TCP! {0} -> {1} bytes", inData.Length, outData.Length);

         // close the streams
         inData.Close();
         outData.Close();

         // fire the completed multipart message callback
         _CompletedReceivedMultipartCallback(this, ref messageCode, ref messageData);
      }

      /// <summary>
      /// Processes a received part from a multipart compressed message.
      /// </summary>
      /// <param name="data">Partial data</param>
      [Obsolete]
      public void ProcessMultipartMessage(ref byte[] data)
      {
         // create the multipart message
         byte[] partData = new byte[data.Length - MultipartHeaderLength];
         for (int i = 0; i < partData.Length; i++)
         {
            partData[i] = data[i + MultipartHeaderLength];
         }

         MultipartMessage message = new MultipartMessage()
         {
            Data = partData,
            MessageID = BitConverter.ToUInt32(data, OffsetMultipartMessageID - MessageHeaderLength),
            PartIndex = BitConverter.ToUInt16(data, OffsetMultipartPartIndex - MessageHeaderLength),
            PartCount = BitConverter.ToUInt16(data, OffsetMultipartPartCount - MessageHeaderLength)
         };

         Console.WriteLine("< !MULTIPART-COMPRESSED!: {0} ({1} / {2})", message.MessageID, message.PartIndex + 1, message.PartCount);

         // lock this crucial part to preven conflicts
         lock (_MultipartMessageLock)
         {
            // add the message to the list
            if (!_MultipartMessageCache.ContainsKey(message.MessageID))
            {
               _MultipartMessageCache.Add(message.MessageID, new List<MultipartMessage>());
            }
            List<MultipartMessage> messages = _MultipartMessageCache[message.MessageID];
            messages.Add(message);

            if (messages.Count == message.PartCount)
            {
               // all parts have arrived
               // sort the list
               messages.Sort();

               // write the parts into a stream
               MemoryStream inData = new MemoryStream();
               foreach (MultipartMessage part in messages)
               {
                  inData.Write(part.Data, 0, part.Data.Length);
               }
               inData.Position = 0;

               // decompress the stream
               MemoryStream outData = new MemoryStream();
               Decoder decompressor = new Decoder();

               // Read the decoder properties
               byte[] properties = new byte[5];
               inData.Read(properties, 0, 5);

               // Read in the decompress file size.
               byte[] dataLengthBytes = new byte[8];
               inData.Read(dataLengthBytes, 0, 8);
               long dataLength = BitConverter.ToInt64(dataLengthBytes, 0);

               decompressor.SetDecoderProperties(properties);
               decompressor.Code(inData, outData, inData.Length, dataLength, null);
               outData.Position = 0;

               // extract the actual message data
               byte messageCode = (byte)outData.ReadByte();
               byte[] messageLengthData = new byte[4];
               outData.Read(messageLengthData, 0, 4);
               int messageLength = BitConverter.ToInt32(messageLengthData, 0);
               byte[] messageData = new byte[messageLength];
               outData.Read(messageData, 0, messageLength);

               // close the streams
               inData.Close();
               outData.Close();

               // fire the completed multipart message callback
               _CompletedReceivedMultipartCallback(this, ref messageCode, ref messageData);
            }
         }
      }

      /// <summary>
      /// Gets the lock intended for syncing message sending.
      /// </summary>
      public object SendLock
      {
         get
         {
            return _SendLock;
         }
      }

      /// <summary>
      /// Gets a remote endpoint.
      /// </summary>
      public IPEndPoint RemoteEndPoint
      {
         get
         {
            return _RemoteEndPoint;
         }
      }

      private void SendCompressedMessage(ref byte[] uncompressed)
      {
         if (TcpConnection == null)
         {
            Console.WriteLine("- TCP connection closed, can't send large message");
            return;
         }

         // prepare streams
         MemoryStream inData = new MemoryStream(uncompressed);
         inData.Position = 0;
         MemoryStream outData = new MemoryStream();

         // compress the data
         Encoder compressor = new Encoder();
         compressor.WriteCoderProperties(outData);
         outData.Write(BitConverter.GetBytes(inData.Length), 0, 8);
         compressor.Code(inData, outData, inData.Length, -1, null);

         outData.Position = 0;

         Console.WriteLine("< !COMPRESSED-TCP! {0} -> {1} bytes", inData.Length, outData.Length);

         // close the in stream as it is no longer needed
         inData.Close();

         if (TcpConnection != null)
         {
            TcpConnection.SendObject(TcpByteMessageName, outData.ToArray());
         }

         outData.Close();
      }

      /// <summary>
      /// Send a message through this connection.
      /// </summary>
      /// <param name="data">Binary data</param>
      /// <param name="channel">Channel</param>
      public void SendMessage(ref byte[] data, int channel)
      {
         SendMessage(ref data, NetDeliveryMethod.ReliableOrdered, channel);
      }

      /// <summary>
      /// Send a message through this connection.
      /// </summary>
      /// <param name="data">Binary data</param>
      /// <param name="method">UDP delivery method (ignored if sent over TCP
      /// because of message size)</param>
      /// <param name="channel">Channel</param>
      public void SendMessage(ref byte[] data, NetDeliveryMethod method, int channel)
      {
         if (data.Length > _MaxMessageSize)
         {
            SendCompressedMessage(ref data);

            return;
         }

         NetOutgoingMessage message = NetConnection.Peer.CreateMessage(data.Length);
         message.LengthBytes = data.Length;
         message.Data = data;

         NetConnection.SendMessage(message, method, channel);
      }

      /// <summary>
      /// Gets or sets the TCP connection.
      /// </summary>
      public ConnectionTcp TcpConnection { get; set; }
   }
}
