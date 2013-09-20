using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Exceptions
{
   /// <summary>
   /// This static class provides the means to log exceptions.
   /// </summary>
   public static class ExceptionHandler
   {
      #region Constants
      private const string InnerExceptionSeparator = "--- INNER EXCEPTION ---";
      #endregion

      #region Static fields
      private static readonly string _Path = Path.Combine("..", "..", "Logs");
      private static readonly string _PathFormat = Path.Combine(_Path, "{0}.txt");
      #endregion

      /// <summary>
      /// Logs an exception into a file.
      /// </summary>
      /// <param name="ex">Exception to log</param>
      public static void LogException(Exception ex)
      {
         if (!Directory.Exists(_Path))
         {
            Directory.CreateDirectory(_Path);
         }

         Console.WriteLine("Exception: " + ex.GetType().ToString());

         string filePath = string.Format(_PathFormat, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + Guid.NewGuid().ToString());
         StringBuilder sb = new StringBuilder();
         WriteExceptionData(ex, sb);
         File.WriteAllText(filePath, sb.ToString());
      }

      private static void WriteExceptionData(Exception ex, StringBuilder sb)
      {
         sb.AppendLine(ex.GetType().ToString());
         sb.AppendLine();
         sb.AppendLine(ex.Message);
         sb.AppendLine("Source: " + ex.Source);
         sb.AppendLine();
         sb.AppendLine(ex.StackTrace);

         if (ex.InnerException != null)
         {
            sb.AppendLine();
            sb.AppendLine(InnerExceptionSeparator);
            sb.AppendLine();
            WriteExceptionData(ex.InnerException, sb);
         }
      }
   }
}
