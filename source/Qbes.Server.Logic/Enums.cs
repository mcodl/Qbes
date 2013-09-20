using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Server.Logic
{
   [Obsolete]
   internal enum ClientAreaState : byte
   {
      Loading = 1,
      Loaded = 2,
      NotLoaded = 0,
   }
}
