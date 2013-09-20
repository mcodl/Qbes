using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic;
using Qbes.Server.Logic.Exceptions;

namespace Qbes.Server.Logic
{
   /// <summary>
   /// ServerSegment adds multiplayer related functionality to the Segment base
   /// class.
   /// </summary>
   public sealed class ServerSegment : Segment
   {
      #region Private fields
      private ServerArea _Area;
      private List<ServerBox> _Boxes = new List<ServerBox>();
      #endregion

      /// <summary>
      /// Adds a cube as a box.
      /// </summary>
      /// <param name="cube">Cube to add as a box</param>
      protected override void AddBox(Cube cube)
      {
         AddBox(new ServerBox(cube, this));
      }

      /// <summary>
      /// Adds a box to this segment.
      /// </summary>
      /// <typeparam name="TBox">Box type type</typeparam>
      /// <param name="box">Box to add</param>
      public override void AddBox<TBox>(TBox box)
      {
         if (!(box is ServerBox))
         {
            throw new NotServerBoxException();
         }

         _Boxes.Add(box as ServerBox);
         StoreBoxToExistMatrix(box);
      }

      /// <summary>
      /// Gets or sets (protected) the wrapping area.
      /// </summary>
      public override Area Area
      {
         get
         {
            return base.Area;
         }
         protected set
         {
            base.Area = value;
            ServerArea = (ServerArea)value;
         }
      }

      /// <summary>
      /// Clears the held boxes.
      /// </summary>
      protected override void ClearBoxes()
      {
         _Boxes.Clear();
      }

      /// <summary>
      /// Gets the owner area of this segment.
      /// </summary>
      /// <returns>Owner area of this segment</returns>
      public override Area GetArea()
      {
         return ServerArea;
      }

      /// <summary>
      /// Gets a box at the specified index.
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>Box at specified index</returns>
      public override TBox GetBox<TBox>(ref int index)
      {
         return _Boxes[index] as TBox;
      }

      /// <summary>
      /// Gets the current box count.
      /// </summary>
      /// <returns>Current box count</returns>
      public override int GetBoxCount()
      {
         return _Boxes.Count;
      }

      /// <summary>
      /// Gets the current box count.
      /// </summary>
      /// <returns>Current box count</returns>
      protected override List<Box> GetBoxes()
      {
         List<Box> result = new List<Box>(_Boxes.Count);

         result.AddRange(_Boxes.Cast<Box>());

         return result;
      }

      /// <summary>
      /// Merges the boxes.
      /// </summary>
      protected override void MergeBoxes()
      {
         Box.MergeBoxes(_Boxes);
      }

      /// <summary>
      /// Runs any postprocessing needed after a segment is changed.
      /// </summary>
      protected override void OnChanged()
      {
         // empty
      }

      /// <summary>
      /// Runs any postprocessing needed after a segment is initialized.
      /// </summary>
      protected override void OnInitialized()
      {
         // empty
      }

      internal ServerArea ServerArea
      {
         get
         {
            return _Area;
         }
         private set
         {
            _Area = value;
         }
      }

      /// <summary>
      /// Removes all references held by this segment.
      /// </summary>
      public override void Unload()
      {
         base.Unload();

         _Boxes.ForEach(b => b.Unload());
         _Boxes.Clear();
      }
   }
}
