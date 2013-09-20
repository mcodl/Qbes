using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.TerrainGeneration
{
   internal sealed class TerrainGenArea : Area
   {
      #region Private fields
      private List<TerrainGenSegment> _Segments = new List<TerrainGenSegment>();
      #endregion

      #region Constructors
      internal TerrainGenArea(int areaX, int areaY, int areaZ)
         : base(areaX, areaY, areaZ)
      {
         // empty
      }
      #endregion

      public override void AddSegment<TSegment>(TSegment segment)
      {
         if (!(segment is TerrainGenSegment))
         {
            throw new ApplicationException();
         }

         _Segments.Add(segment as TerrainGenSegment);
      }

      public override TSegment GetSegment<TSegment>(ref int index)
      {
         return _Segments[index] as TSegment;
      }

      public override int GetSegmentCount()
      {
         return _Segments.Count;
      }

      internal void SortSegments()
      {
         _Segments.Sort();
      }
   }
}
