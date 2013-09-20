using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Networking.Constants
{
   /// <summary>
   /// Network channels are defined in this static class as constants.
   /// </summary>
   public static class NetworkChannels
   {
      /// <summary>
      /// Chat channel number.
      /// </summary>
      public const int ChatChannel = 5;
      /// <summary>
      /// Channel number for messages related to connecting and disconnecting
      /// players.
      /// </summary>
      public const int ConnectingDisconnectingChannel = 4;
      /// <summary>
      /// Entity messages channel number.
      /// </summary>
      public const int EntityMessagesChannel = 3;
      /// <summary>
      /// File data exchange channel number.
      /// </summary>
      public const int FileDataExchangeChannel = 1;
      /// <summary>
      /// Multipart compressed channel number.
      /// </summary>
      public const int MultipartChannel = 0;
      /// <summary>
      /// Terrain data channel number.
      /// </summary>
      public const int TerrainDataChannel = 2;
   }
}
