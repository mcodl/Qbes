using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Constants
{
   /// <summary>
   /// This class contains special constants used to account for possible errors
   /// int float precision computation.
   /// </summary>
   public static class ErrorTolerances
   {
      #region Constants
      /// <summary>
      /// Float epsilon contains the error tolerance for single float precision.
      /// </summary>
      public const float FloatEpsilon = 0.00000000001f;
      #endregion
   }
}
