using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Qbes.Common.Logic.Extensions
{
   /// <summary>
   /// This static class contains extensions for byte and byte arrays.
   /// </summary>
   public static class ByteExtensions
   {
      #region Static fields
      private static readonly MD5 _Md5 = MD5CryptoServiceProvider.Create();
      #endregion

      /// <summary>
      /// Gets a hexa string from the byte array.
      /// </summary>
      /// <param name="data">Array to get a hexa string from</param>
      /// <returns>Hexa string from the byte array</returns>
      public static string GetHexaString(this byte[] data)
      {
         StringBuilder sb = new StringBuilder(data.Length * 2);

         for (int i = 0; i < data.Length; i++)
         {
            sb.Append(data[i].ToString("x2").ToUpper());
         }

         return sb.ToString();
      }

      /// <summary>
      /// Gets an MD5 hash from the byte array.
      /// </summary>
      /// <param name="data">Array to calculate checksum for</param>
      /// <returns>MD5 hash from the byte array</returns>
      public static byte[] GetMd5Checksum(this byte[] data)
      {
         return _Md5.ComputeHash(data);
      }
   }
}
