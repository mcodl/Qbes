using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.TerrainGeneration
{
   internal sealed class TerrainGenBox : Box
   {
      #region Constructors
      internal TerrainGenBox()
      {
         // empty
      }

      internal TerrainGenBox(Cube cube, Segment segment)
         : base(cube, segment)
      {
         // empty
      }
      #endregion
   }
}
