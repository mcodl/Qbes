using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.TerrainGeneration
{
   internal sealed class TerrainGenSegment : Segment
   {
      #region Private fields
      private List<TerrainGenBox> _Boxes = new List<TerrainGenBox>();
      #endregion

      #region Constructors
      internal TerrainGenSegment(int segmentX, int segmentY, int segmentZ, Area area)
         : base(segmentX, segmentY, segmentZ, area)
      {
         // empty
      }
      #endregion

      protected override void AddBox(Cube cube)
      {
         AddBox(new TerrainGenBox(cube, this));
      }

      public override void AddBox<TBox>(TBox box)
      {
         if (!(box is TerrainGenBox))
         {
            throw new ApplicationException();
         }

         _Boxes.Add(box as TerrainGenBox);
      }

      protected override void ClearBoxes()
      {
         _Boxes.Clear();
      }

      public override TBox GetBox<TBox>(ref int index)
      {
         return _Boxes[index] as TBox;
      }

      public override int GetBoxCount()
      {
         return _Boxes.Count;
      }

      protected override List<Box> GetBoxes()
      {
         return _Boxes.Cast<Box>().ToList();
      }

      protected override void MergeBoxes()
      {
         Box.MergeBoxes(_Boxes);
      }
   }
}
