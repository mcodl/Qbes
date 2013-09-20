using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Exceptions
{
   /// <summary>
   /// This exception is used when the segment's cube count is higher then 512.
   /// </summary>
   public sealed class CubeCountHighException : ApplicationException
   {
      #region Constants
      private const string ErrorMessageFormat = "Cube count in segment higher then 512: {0}";
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a new exception instance.
      /// </summary>
      public CubeCountHighException(int cubeCount)
         : base(string.Format(ErrorMessageFormat, cubeCount))
      {
      }
      #endregion
   }
}
