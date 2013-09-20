using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Networking.Constants
{
   /// <summary>
   /// This class contains constants with message codes that are needed for
   /// recognition of messages in their binary format.
   /// </summary>
   public static class MessageCodes
   {
      /// <summary>
      /// Chat message code.
      /// </summary>
      public const int Chat = 16;
      /// <summary>
      /// Disconnecting code.
      /// </summary>
      public const byte Disconnecting = 7;
      /// <summary>
      /// Entity creation or update code.
      /// </summary>
      public const byte EntityCreateUpdate = 8;
      /// <summary>
      /// Entity deletion code.
      /// </summary>
      public const byte EntityDelete = 9;
      /// <summary>
      /// File data exchange code.
      /// </summary>
      public const byte FileDataExchangeAdd = 11;
      /// <summary>
      /// File data exchange code.
      /// </summary>
      public const byte FileDataExchangeRequest = 12;
      /// <summary>
      /// File data exchange code.
      /// </summary>
      public const byte FileDataExchangeResponse = 13;
      /// <summary>
      /// File data request code.
      /// </summary>
      public const byte FileDataRequest = 14;
      /// <summary>
      /// General "no" response code.
      /// </summary>
      public const byte GeneralNo = 1;
      /// <summary>
      /// General "yes" response code.
      /// </summary>
      public const byte GeneralYes = 0;
      /// <summary>
      /// Login message code.
      /// </summary>
      public const byte Login = 2;
      /// <summary>
      /// Entity moved message code.
      /// </summary>
      public const byte Moved = 5;
      /// <summary>
      /// Entity moving message code.
      /// </summary>
      public const byte Moving = 4;
      /// <summary>
      /// Multipart compressed message code.
      /// </summary>
      [Obsolete]
      public const byte MultipartCompressed = 10;
      /// <summary>
      /// Multipart compressed confirmation code.
      /// </summary>
      [Obsolete]
      public const byte MultipartConfirmation = 15;
      /// <summary>
      /// Cube placement/removal code.
      /// </summary>
      public const byte PlaceOrRemoveCube = 6;
      /// <summary>
      /// Terrain message code.
      /// </summary>
      public const byte Terrain = 3;
   }
}
