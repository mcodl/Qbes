using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Networking.Constants
{
   /// <summary>
   /// This class contains constants for possible login results.
   /// </summary>
   public static class LoginResults
   {
      /// <summary>
      /// Player or IP address banned code.
      /// </summary>
      public const byte Banned = 2;
      /// <summary>
      /// Wrong name and/or password code.
      /// </summary>
      public const byte InvalidCredentials = 1;
      /// <summary>
      /// Successful login code.
      /// </summary>
      public const byte Successful = 0;
      /// <summary>
      /// Version mismatch code.
      /// </summary>
      public const byte VersionMismatch = 3;
   }
}
