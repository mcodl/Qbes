using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Client.Logic.Constants;
using Qbes.Client.Logic.Exceptions;
using Qbes.Common.Logic;
using Qbes.Common.Logic.Constants;
using Qbes.Common.Logic.DataStructures;

namespace Qbes.Client.Logic
{
   /// <summary>
   /// Client segment adds rendering related logic to the Segment class.
   /// </summary>
   public sealed class ClientSegment : Segment
   {
      #region Constants
      private const int FlagsEnclosedIndex = 6;
      private const int FlagsHiddenIndex = 7;
      #endregion

      #region Static fields
      private static readonly ClientSegment _DummyNeighbour;
      #endregion

      #region Fields
      private List<ClientBox> _Boxes = new List<ClientBox>();
      private BinaryArrayInByte _Flags = new BinaryArrayInByte();
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default client segment instance.
      /// </summary>
      public ClientSegment()
         : base()
      {
         _Flags[FlagsHiddenIndex] = true;
      }

      static ClientSegment()
      {
         _DummyNeighbour = new ClientSegment();
         _DummyNeighbour.IsEnclosed = true;
         _DummyNeighbour.IsHidden = true;
         _DummyNeighbour._Flags = new BinaryArrayInByte(new bool[] { true, true, true, true, true, true, false, true });
      }
      #endregion

      /// <summary>
      /// Adds a cube as a box.
      /// </summary>
      /// <param name="cube">Cube to add as a box</param>
      protected override void AddBox(Cube cube)
      {
         AddBox(new ClientBox(cube, this));
      }

      /// <summary>
      /// Adds a box to this segment.
      /// </summary>
      /// <typeparam name="TBox">Box type type</typeparam>
      /// <param name="box">Box to add</param>
      public override void AddBox<TBox>(TBox box)
      {
         if (!(box is ClientBox))
         {
            throw new NotClientBoxException();
         }

         _Boxes.Add(box as ClientBox);
         StoreBoxToExistMatrix(box);
      }

      /// <summary>
      /// Gets or sets (protected) the wrapper area.
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
            ClientArea = (ClientArea)value;
         }
      }

      /// <summary>
      /// Checks whether this segment is enclosed.
      /// </summary>
      internal void CheckEnclosed()
      {
         if (_Boxes.Count == 0)
         {
            // empty segment can't be enclosed
            for (int i = 0; i < 6; i++)
            {
               _Flags[i] = false;
            }
            IsEnclosed = false;
            return;
         }

         int remaining = 296;
         int[] sideBlockRemaining = new int[] { 64, 64, 64, 64, 64, 64 };

         for (int i = 0; i < 8; i++)
         {
            for (int j = 0; j < 8; j++)
            {
               if (VisMatrix.Get(7, i, j))
               {
                  remaining--;
                  sideBlockRemaining[Sides.FrontX]--;
               }
               if (VisMatrix.Get(i, 7, j))
               {
                  remaining--;
                  sideBlockRemaining[Sides.FrontY]--;
               }
               if (VisMatrix.Get(i, j, 7))
               {
                  remaining--;
                  sideBlockRemaining[Sides.FrontZ]--;
               }
               if (VisMatrix.Get(0, i, j))
               {
                  remaining--;
                  sideBlockRemaining[Sides.BackX]--;
               }
               if (VisMatrix.Get(i, 0, j))
               {
                  remaining--;
                  sideBlockRemaining[Sides.BackY]--;
               }
               if (VisMatrix.Get(i, j, 0))
               {
                  remaining--;
                  sideBlockRemaining[Sides.BackZ]--;
               }
            }
         }

         IsEnclosed = (remaining <= 0);
         bool[] neighbourBlocks = new bool[6];
         for (int i = 0; i < 6; i++)
         {
            _Flags[i] = (sideBlockRemaining[i] == 0);
         }
      }

      /// <summary>
      /// Checks whether this segment is hidden. If that's not the case then all
      /// box faces are checked if they're not hidden.
      /// </summary>
      /// <param name="messageId">Message ID</param>
      internal void CheckHidden(ref int messageId)
      {
         // get neighbouring segments
         ClientSegment[] neighbours = ClientWorldManager.Instance.GetSegmentsNeighbours(this);

         // schedule updates of neighbours first (if neccessary)
         if (!ClientArea.IsReady &&
             messageId != SpecialMessageIDs.SegmentRefreshFromBlockPlacement)
         {
            for (int i = 0; i < neighbours.Length; i++)
            {
               if (neighbours[i] != _DummyNeighbour &&
                   neighbours[i].ClientArea.IsReady)
               {
                  // schedule the segment for visibility refresh
                  ClientWorldManager.Instance.AddSegmentForVisibilityRefresh(ref messageId, neighbours[i]);
               }
            }
         }

         // if the segment has no boxes, then it is practically hidden
         if (_Boxes.Count == 0)
         {
            IsHidden = true;
            return;
         }

         // check if this segment is hidden or not
         bool hidden = true;
         for (int i = 0; i < neighbours.Length; i++)
         {
            if (!neighbours[i]._Flags[i])
            {
               hidden = false;
               break;
            }
         }
         IsHidden = hidden;

         // check hidden faces in segment's boxes if not hidden
         if (hidden)
         {
            return;
         }

         int hiddenFaces = 0;

         foreach (ClientBox b in _Boxes)
         {
            // prepare variables
            int x1 = Convert.ToInt32(b.X1 - b.Segment.X);
            int x2 = Convert.ToInt32(b.X2 - b.Segment.X) - 1;
            int y1 = Convert.ToInt32(b.Y1 - b.Segment.Y);
            int y2 = Convert.ToInt32(b.Y2 - b.Segment.Y) - 1;
            int z1 = Convert.ToInt32(b.Z1 - b.Segment.Z);
            int z2 = Convert.ToInt32(b.Z2 - b.Segment.Z) - 1;
         
            // first check if it isn't on the border
            if (x1 == 0 && !CheckHidden(b, Sides.FrontX, ref neighbours, ref hiddenFaces))
            {
               CheckHiddenX(b, Sides.FrontX, neighbours[Sides.FrontX].VisMatrix, 7, ref y1, ref y2, ref z1, ref z2, ref hiddenFaces);
            }
            else if (x1 > 0)
            {
               CheckHiddenX(b, Sides.FrontX, VisMatrix, x1 - 1, ref y1, ref y2, ref z1, ref z2, ref hiddenFaces);
            }

            if (y1 == 0 && !CheckHidden(b, Sides.FrontY, ref neighbours, ref hiddenFaces))
            {
               CheckHiddenY(b, Sides.FrontY, neighbours[Sides.FrontY].VisMatrix, 7, ref x1, ref x2, ref z1, ref z2, ref hiddenFaces);
            }
            else if (y1 > 0)
            {
               CheckHiddenY(b, Sides.FrontY, VisMatrix, y1 - 1, ref x1, ref x2, ref z1, ref z2, ref hiddenFaces);
            }

            if (z1 == 0 && !CheckHidden(b, Sides.FrontZ, ref neighbours, ref hiddenFaces))
            {
               CheckHiddenZ(b, Sides.FrontZ, neighbours[Sides.FrontZ].VisMatrix, 7, ref x1, ref x2, ref y1, ref y2, ref hiddenFaces);
            }
            else if (z1 > 0)
            {
               CheckHiddenZ(b, Sides.FrontZ, VisMatrix, z1 - 1, ref x1, ref x2, ref y1, ref y2, ref hiddenFaces);
            }

            if (x2 == 7 && !CheckHidden(b, Sides.BackX, ref neighbours, ref hiddenFaces))
            {
               CheckHiddenX(b, Sides.BackX, neighbours[Sides.BackX].VisMatrix, 0, ref y1, ref y2, ref z1, ref z2, ref hiddenFaces);
            }
            else if (x2 < 7)
            {
               CheckHiddenX(b, Sides.BackX, VisMatrix, x2 + 1, ref y1, ref y2, ref z1, ref z2, ref hiddenFaces);
            }

            if (y2 == 7 && !CheckHidden(b, Sides.BackY, ref neighbours, ref hiddenFaces))
            {
               CheckHiddenY(b, Sides.BackY, neighbours[Sides.BackY].VisMatrix, 0, ref x1, ref x2, ref z1, ref z2, ref hiddenFaces);
            }
            else if (y2 < 7)
            {
               CheckHiddenY(b, Sides.BackY, VisMatrix, y2 + 1, ref x1, ref x2, ref z1, ref z2, ref hiddenFaces);
            }

            if (z2 == 7 && !CheckHidden(b, Sides.BackZ, ref neighbours, ref hiddenFaces))
            {
               CheckHiddenZ(b, Sides.BackZ, neighbours[Sides.BackZ].VisMatrix, 0, ref x1, ref x2, ref y1, ref y2, ref hiddenFaces);
            }
            else if (z2 < 7)
            {
               CheckHiddenZ(b, Sides.BackZ, VisMatrix, z2 + 1, ref x1, ref x2, ref y1, ref y2, ref hiddenFaces);
            }

            if (hiddenFaces == 6)
            {
               // all sides are hidden
               b.HiddenSides[Sides.All] = true;
            }

            hiddenFaces = 0;
         }
      }

      /// <summary>
      /// Checks whether the box side is blocked by the neighbour's blocking
      /// side.
      /// </summary>
      /// <param name="box">Box</param>
      /// <param name="side">Side</param>
      /// <param name="neighbours">Neighbours array</param>
      /// <param name="hiddenFaces">Hidden sides reference</param>
      /// <returns>True if the box side is blocked</returns>
      private bool CheckHidden(ClientBox box, int side, ref ClientSegment[] neighbours, ref int hiddenFaces)
      {
         if (neighbours[side]._Flags[side])
         {
            hiddenFaces++;
            box.HiddenSides[side] = true;

            return true;
         }

         return false;
      }

      /// <summary>
      /// Does a check for either FrontX or BackX
      /// </summary>
      /// <param name="box">Box</param>
      /// <param name="side">Side</param>
      /// <param name="visMatrix">Visibility matrix</param>
      /// <param name="x">X coordinate</param>
      /// <param name="y1">Y1 coordinate</param>
      /// <param name="y2">Y2 coordinate</param>
      /// <param name="z1">Z1 coordinate</param>
      /// <param name="z2">Z2 coordinate</param>
      /// <param name="hiddenFaces">Hidden sides reference</param>
      private static void CheckHiddenX(ClientBox box, int side, BinaryMatrix3D visMatrix, int x, ref int y1, ref int y2, ref int z1, ref int z2, ref int hiddenFaces)
      {
         box.HiddenSides[side] = true;
         for (int y = y1; y <= y2; y++)
         {
            for (int z = z1; z <= z2; z++)
            {
               if (!visMatrix.Get(ref x, ref y, ref z))
               {
                  box.HiddenSides[side] = false;
                  break;
               }
            }

            if (!box.HiddenSides[side])
            {
               break;
            }
         }

         if (box.HiddenSides[side])
         {
            hiddenFaces++;
         }
      }

      /// <summary>
      /// Does a check for either FrontY or BackY
      /// </summary>
      /// <param name="box">Box</param>
      /// <param name="side">Side</param>
      /// <param name="visMatrix">Visibility matrix</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="x1">X1 coordinate</param>
      /// <param name="x2">X2 coordinate</param>
      /// <param name="z1">Z1 coordinate</param>
      /// <param name="z2">Z2 coordinate</param>
      /// <param name="hiddenFaces">Hidden sides reference</param>
      private static void CheckHiddenY(ClientBox box, int side, BinaryMatrix3D visMatrix, int y, ref int x1, ref int x2, ref int z1, ref int z2, ref int hiddenFaces)
      {
         box.HiddenSides[side] = true;
         for (int x = x1; x <= x2; x++)
         {
            for (int z = z1; z <= z2; z++)
            {
               if (!visMatrix.Get(ref x, ref y, ref z))
               {
                  box.HiddenSides[side] = false;
                  break;
               }
            }

            if (!box.HiddenSides[side])
            {
               break;
            }
         }

         if (box.HiddenSides[side])
         {
            hiddenFaces++;
         }
      }

      /// <summary>
      /// Does a check for either FrontZ or BackZ
      /// </summary>
      /// <param name="box">Box</param>
      /// <param name="side">Side</param>
      /// <param name="visMatrix">Visibility matrix</param>
      /// <param name="z">Z coordinate</param>
      /// <param name="x1">X1 coordinate</param>
      /// <param name="x2">X2 coordinate</param>
      /// <param name="y1">Y1 coordinate</param>
      /// <param name="y2">Y2 coordinate</param>
      /// <param name="hiddenFaces">Hidden sides reference</param>
      private static void CheckHiddenZ(ClientBox box, int side, BinaryMatrix3D visMatrix, int z, ref int x1, ref int x2, ref int y1, ref int y2, ref int hiddenFaces)
      {
         box.HiddenSides[side] = true;
         for (int x = x1; x <= x2; x++)
         {
            for (int y = y1; y <= y2; y++)
            {
               if (!visMatrix.Get(ref x, ref y, ref z))
               {
                  box.HiddenSides[side] = false;
                  break;
               }
            }

            if (!box.HiddenSides[side])
            {
               break;
            }
         }

         if (box.HiddenSides[side])
         {
            hiddenFaces++;
         }
      }

      /// <summary>
      /// Clears the held boxes.
      /// </summary>
      protected override void ClearBoxes()
      {
         DiagnosticsManager.BoxesLoaded -= _Boxes.Count;
         _Boxes.Clear();
      }

      /// <summary>
      /// Gets or sets (private) the wrapper area.
      /// </summary>
      public ClientArea ClientArea { get; private set; }

      internal void Draw(Point3D location, Point3D direction, ref int shiftX, ref int shiftZ)
      {
         Point3D centerPoint = ClientWorldManager.Instance.TempPoint;
         centerPoint.Set(CenterPoint, shiftX, 0.0f, shiftZ);
         bool autoAccept = false;

         if (ClientArea.CalculateSegmentDistance)             
         {
            float dist = location.GetDistanceSquare(centerPoint);
            if (dist >= ClientWorldManager.Instance.RenderDistance)
            {
#if DIAG
               if (ClientArea.CloseElimination)
               {
                  DiagnosticsManager.EliminatedSegmentsCloseDistance++;
               }
               else
               {
                  DiagnosticsManager.EliminatedSegmentsBasicDistance++;
               }
#endif
               return;
            }
            else if (dist <= ClientArea.SegmentCloseDistance)
            {
               autoAccept = true;
            }
         }

         if (!autoAccept &&
             ClientArea.CalculateSegmentAngle &&
             Point3D.GetAngleRadians(location, direction, centerPoint) > ClientArea.SegmentAngleTolerance)
         {
#if DIAG
            if (ClientArea.CloseElimination)
            {
               DiagnosticsManager.EliminatedSegmentsCloseAngle++;
            }
            else
            {
               DiagnosticsManager.EliminatedSegmentsBasicAngle++;
            }
#endif
            return;
         }

         foreach (var box in GetBoxesSynchronized().Cast<ClientBox>())
         {
            if (box.HiddenSides[Sides.All])
            {
               continue;
            }
            box.Draw(location, ref shiftX, ref shiftZ);
         }

#if DIAG
         if (ClientArea.CloseElimination)
         {
            DiagnosticsManager.PassedSegmentsClose++;
         }
         else
         {
            DiagnosticsManager.PassedSegmentsBasic++;
         }
         DiagnosticsManager.SegmentsRendered++;
#endif
      }

      /// <summary>
      /// Gets a dummy neighbour for segments that are on the edge of the map.
      /// </summary>
      public static ClientSegment DummyNeighbour
      {
         get
         {
            return _DummyNeighbour;
         }
      }

      /// <summary>
      /// Gets the owner area of this segment.
      /// </summary>
      /// <returns>Owner area of this segment</returns>
      public override Area GetArea()
      {
         return ClientArea;
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
      /// Gets or sets (private) whether the segment forms an enclosed box and
      /// thus nothing inside is visible from the outside (if so outer walls may
      /// still be visible though).
      /// </summary>
      internal bool IsEnclosed
      {
         get
         {
            return _Flags[FlagsEnclosedIndex];
         }
         private set
         {
            _Flags[FlagsEnclosedIndex] = value;
         }
      }

      /// <summary>
      /// Gets or sets whether the segment is hidden by other segments (if so
      /// then outer segment walls aren't visible).
      /// </summary>
      internal bool IsHidden
      {
         get
         {
            return _Flags[FlagsHiddenIndex];
         }
         private set
         {
            //if (_Flags[FlagsHiddenIndex] && !value)
            //{
            //   ClientArea.VisibleSegments.Add(this);
            //}
            //else if (!_Flags[FlagsHiddenIndex] && value)
            //{
            //   ClientArea.VisibleSegments.Remove(this);
            //}

            _Flags[FlagsHiddenIndex] = value;
         }
      }

      /// <summary>
      /// Merges the boxes.
      /// </summary>
      protected override void MergeBoxes()
      {
         Box.MergeBoxes(_Boxes);
      }

      /// <summary>
      /// Recalculates the visibility.
      /// </summary>
      protected override void OnChanged()
      {
         // adjust loaded box count
         DiagnosticsManager.BoxesLoaded += _Boxes.Count;

         // recheck enclosed
         CheckEnclosed();

         // check hidden
         int messageID = SpecialMessageIDs.SegmentRefreshFromBlockPlacement;
         CheckHidden(ref messageID);
         SortBoxesByMaterial();
         
         ClientSegment[] neihbours = ClientWorldManager.Instance.GetSegmentsNeighbours(this);
         foreach (ClientSegment neighbour in neihbours)
         {
            if (neighbour == DummyNeighbour)
            {
               continue;
            }

            neighbour.CheckHidden(ref messageID);
         }
      }

      /// <summary>
      /// Initializes the flags properly.
      /// </summary>
      protected override void OnInitialized()
      {
         _Flags[FlagsHiddenIndex] = true;
      }

      internal void SortBoxesByMaterial()
      {
         // sort boxes by material ID to help reduce texture switching
         _Boxes.Sort((a, b) =>
         {
            return a.MaterialId.CompareTo(b.MaterialId);
         });
      }

      /// <summary>
      /// Removes all references held by this segment.
      /// </summary>
      public override void Unload()
      {
         base.Unload();

         _Boxes.ForEach(b => b.Unload());
         _Boxes.Clear();
         _Flags.Reset();
      }
   }
}
