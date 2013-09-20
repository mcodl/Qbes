using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Networking.Constants
{
   /// <summary>
   /// This static class contains constants that define smallest possible update
   /// intervals for repeatedly sent messages.
   /// </summary>
   public static class UpdateIntervals
   {
      /// <summary>
      /// Defines a window of miliseconds between individual moving updates.
      /// </summary>
      public const int EntityMovingInterval = 33;
   }
}
