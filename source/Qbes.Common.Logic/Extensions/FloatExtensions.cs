using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Constants;

namespace Qbes.Common.Logic.Extensions
{
   /// <summary>
   /// Float extensions.
   /// </summary>
   public static class FloatExtensions
   {
      /// <summary>
      /// Checks equality with allowed error.
      /// </summary>
      /// <param name="this">This float</param>
      /// <param name="what">Another float</param>
      /// <returns>True if equals with allowed error</returns>
      public static bool EqEps(this float @this, float what)
      {
         return ((@this < 0 ? -@this : @this) - what) < ErrorTolerances.FloatEpsilon;
      }
   }
}
