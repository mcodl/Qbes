using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic;

namespace Qbes.UnitTests.Helpers
{
   internal static class WorldSizeHelper
   {
      internal static void SetWorldSize()
      {
         WorldHelper.SetWorldSize(65536, 65536);
      }
   }
}
