using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Extensions;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// Skin manager is used to cache player skins.
   /// </summary>
   public sealed class SkinManager
   {
      #region Constants
      /// <summary>
      /// Cached skin file extension.
      /// </summary>
      public const string SkinFileExtension = ".skin";
      #endregion

      #region Static fields
      private static object _Lock = new object();
      #endregion

      /// <summary>
      /// Get skin texture.
      /// </summary>
      /// <param name="hash">Skin hash</param>
      /// <returns>Skin texture</returns>
      /// <remarks>Skin needs to be already present on the disk, otherwise
      /// exception is thrown</remarks>
      public static byte[] GetSkin(ref byte[] hash)
      {
         lock (_Lock)
         {
            return File.ReadAllBytes(GetSkinFileName(ref hash));
         }
      }

      /// <summary>
      /// Get skin texture.
      /// </summary>
      /// <param name="hashString">Skin hash string</param>
      /// <returns>Skin texture</returns>
      /// <remarks>Skin needs to be already present on the disk, otherwise
      /// exception is thrown</remarks>
      public static byte[] GetSkin(string hashString)
      {
         lock (_Lock)
         {
            return File.ReadAllBytes(GetSkinFileName(hashString));
         }
      }

      /// <summary>
      /// Gets the path to cached skin.
      /// </summary>
      /// <param name="hash">Skin hash</param>
      /// <returns>Path to cached skin</returns>
      public static string GetSkinFileName(ref byte[] hash)
      {
         return GetSkinFileName(hash.GetHexaString());
      }

      /// <summary>
      /// Gets the path to cached skin.
      /// </summary>
      /// <param name="hash">Skin hash string</param>
      /// <returns>Path to cached skin</returns>
      public static string GetSkinFileName(string hash)
      {
         return Path.Combine(WorldHelper.CachePath, hash + SkinFileExtension);
      }

      /// <summary>
      /// Check skin existence on disk.
      /// </summary>
      /// <param name="hash">Skin hash</param>
      /// <returns>True if skin is stored locally and hash matches</returns>
      public static bool IsSkinOnDisk(ref byte[] hash)
      {
         return IsSkinOnDisk(hash.GetHexaString());
      }

      /// <summary>
      /// Check skin existence on disk.
      /// </summary>
      /// <param name="hashString">Skin hash string</param>
      /// <returns>True if skin is stored locally and hash matches</returns>
      public static bool IsSkinOnDisk(string hashString)
      {
         string filename = GetSkinFileName(hashString);

         lock (_Lock)
         {
            bool exists = File.Exists(filename);

            if (!exists)
            {
               Console.WriteLine("- IsSkinOnDisk: {0} - doesn't exist", hashString);

               // skin not on disk at all
               return false;
            }

            byte[] skinData = GetSkin(hashString);
            byte[] localHash = skinData.GetMd5Checksum();

            // skin is on disk, compare hashes and return results
            bool hashMatch = (hashString == localHash.GetHexaString());
            if (!hashMatch)
            {
               Console.WriteLine("- IsSkinOnDisk: {0} - hash mismatch", hashString);

               // delete local as checksums do not match
               File.Delete(filename);
            }
            else
            {
               Console.WriteLine("- IsSkinOnDisk: {0} - OK", hashString);
            }

            return hashMatch;
         }
      }

      /// <summary>
      /// Saves the skin to the cache.
      /// </summary>
      /// <param name="hash">Skin hash</param>
      /// <param name="skinData">Skin texture</param>
      public static string SaveSkin(ref byte[] hash, ref byte[] skinData)
      {
         string filePath = GetSkinFileName(ref hash);

         lock (_Lock)
         {
            File.WriteAllBytes(filePath, skinData);
         }

         return filePath;
      }
   }
}
