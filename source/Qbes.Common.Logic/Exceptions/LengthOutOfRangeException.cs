using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Exceptions
{
   /// <summary>
   /// This exception is thrown when given length is out of allowed range.
   /// </summary>
   public sealed class LengthOutOfRangeException : ApplicationException
   {
      #region Constants
      private const string ErrorMessageFormat = "Given length is out of range: {0} (allowed: {1})";
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a new exception instance.
      /// </summary>
      /// <param name="length">Requested length</param>
      /// <param name="allowed">Allowed length</param>
      public LengthOutOfRangeException(int length, int allowed)
         : base(string.Format(ErrorMessageFormat, length, allowed))
      {
         // empty
      }
      #endregion
   }
}
