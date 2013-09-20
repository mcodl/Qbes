using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Extensions
{
   /// <summary>
   /// This static class contains extensions for System.String class.
   /// </summary>
   public static class StringExtensions
   {
      /// <summary>
      /// Strips away all accents and diacritics from a string.
      /// </summary>
      /// <param name="stIn">Input string</param>
      /// <returns>String without accents and diacritics</returns>
      public static string RemoveDiacritics(this string stIn)
      {
         string stFormD = stIn.Normalize(NormalizationForm.FormD);
         StringBuilder sb = new StringBuilder();

         for (int i = 0; i < stFormD.Length; i++)
         {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[i]);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
               sb.Append(stFormD[i]);
            }
         }

         return sb.ToString().Normalize(NormalizationForm.FormC);
      }
   }
}
