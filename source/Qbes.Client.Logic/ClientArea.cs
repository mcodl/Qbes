using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Client.Logic.Exceptions;
using Qbes.Common.Logic;
using Qbes.Common.Logic.DataStructures;

namespace Qbes.Client.Logic
{
   /// <summary>
   /// Client area adds rendering related logic to the Area class.
   /// </summary>
   public sealed class ClientArea : Area
   {
      #region Constants
      private const float AreaCloseDistance = 6500.0f;
      private const float AreaCloseDistanceAreaEliminationAngle = (float)(0.7 * Math.PI); // 126°
      private const float AreaCloseDistanceSegmentAutoAcceptAngle = (float)(0.3 * Math.PI); // 54°
      private const float AreaCloseDistanceSegmentEliminationAngle = (float)(0.45 * Math.PI); // 81°
      private const float AwayDistanceAreaEliminationAngle = (float)(0.5 * Math.PI); // 90°
      private const float AwayDistanceSegmentAutoAcceptAngle = (float)(0.22 * Math.PI); // 39,6°
      private const float AwayDistanceSegmentEliminationAngle = (float)(0.3 * Math.PI); // 54°
      private const float DiagonalSize = 12288.0f;
      private const int FlagsAngleIndex = 0;
      private const int FlagsDistanceIndex = 1;
      private const int FlagsForRemovalIndex = 2;
      private const int FlagsReadyIndex = 3;
      private const float SegmentAreaCloseDistance = 3888.0f;
      internal const float SegmentCloseDistance = 128.0f;
      #endregion

      #region Fields
      private BinaryArrayInByte _Flags = new BinaryArrayInByte();
      private List<ClientSegment> _Segments = new List<ClientSegment>();
      private int _ShiftX;
      private int _ShiftZ;
      private List<ClientSegment> _VisibleSegments = new List<ClientSegment>();
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default client area instance.
      /// </summary>
      public ClientArea()
         : base()
      {
         // empty
      }

      /// <summary>
      /// Creates a new area with given coordinates of its front bottom left
      /// corner.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      public ClientArea(int x, int y, int z)
         : base(x, y, z)
      {
         // empty
      }
      #endregion

      /// <summary>
      /// Adds a segment to this area.
      /// </summary>
      /// <typeparam name="TSegment">Segment type</typeparam>
      /// <param name="segment">Segment to add</param>
      public override void AddSegment<TSegment>(TSegment segment)
      {
         if (!(segment is ClientSegment))
         {
            throw new NotClientSegmentException();
         }

         _Segments.Add(segment as ClientSegment);
      }

      /// <summary>
      /// Gets or sets (private) the CalculateSegmentAngle helper.
      /// </summary>
      public bool CalculateSegmentAngle
      {
         get
         {
            return _Flags[FlagsAngleIndex];
         }
         private set
         {
            _Flags[FlagsAngleIndex] = value;
         }
      }

      /// <summary>
      /// Gets or sets (private) the CalculateSegmentDistance helper.
      /// </summary>
      public bool CalculateSegmentDistance
      {
         get
         {
            return _Flags[FlagsDistanceIndex];
         }
         private set
         {
            _Flags[FlagsDistanceIndex] = value;
         }
      }

#if DIAG
      internal bool CloseElimination { get; private set; }
#endif

      /// <summary>
      /// Gets or sets the Distance helper.
      /// </summary>
      public float Distance { get; set; }

      internal void Draw(Point3D location, Point3D direction)
      {
         Point3D centerPoint = ClientWorldManager.Instance.TempPoint;
         centerPoint.Set(CenterPoint);

         // check shift
         if (location.X + WorldHelper.HalfSizeX < CenterPoint.X)
         {
            _ShiftX = -WorldHelper.SizeX;
            centerPoint.X += _ShiftX;
         }
         else if (location.X - WorldHelper.HalfSizeX > CenterPoint.X)
         {
            _ShiftX = WorldHelper.SizeX;
            centerPoint.X += _ShiftX;
         }
         else if (_ShiftX != 0)
         {
            _ShiftX = 0;
         }

         if (location.Z + WorldHelper.HalfSizeZ < CenterPoint.Z)
         {
            _ShiftZ = -WorldHelper.SizeZ;
            centerPoint.Z += _ShiftZ;
         }
         else if (location.Z - WorldHelper.HalfSizeZ > CenterPoint.Z)
         {
            _ShiftZ = WorldHelper.SizeZ;
            centerPoint.Z += _ShiftZ;
         }
         else if (_ShiftZ != 0)
         {
            _ShiftZ = 0;
         }

         Distance = location.GetDistanceSquare(centerPoint);
         double angle = Point3D.GetAngleRadians(location, direction, centerPoint);

         if (ClientWorldManager.Instance.ColumnX == X &&
             ClientWorldManager.Instance.ColumnZ == Z &&
             Math.Abs(ClientWorldManager.Instance.Location.Y - centerPoint.Y) <= 32.0f)
         {
            // current area (don't check segment angles as player may be inside one)
            CalculateSegmentAngle = false;
         }
         else if (Distance <= AreaCloseDistance)
         {
            // close area
#if DIAG
            CloseElimination = true;
#endif
            bool closeDistance = (Distance <= SegmentAreaCloseDistance);
            // area viewport elimination with large angle tolerance applied only
            // at larger than close distances
            if (!closeDistance &&
                angle > AreaCloseDistanceAreaEliminationAngle)
            {
#if DIAG
               DiagnosticsManager.EliminatedAreasClose++;
#endif
               return;
            }
#if DIAG
            DiagnosticsManager.PassedAreasClose++;
#endif
            CalculateSegmentAngle = (closeDistance || angle > AreaCloseDistanceSegmentAutoAcceptAngle);
            SegmentAngleTolerance = AreaCloseDistanceSegmentEliminationAngle;
         }
         else
         {
            // basic distance elimination
#if DIAG
            CloseElimination = false;
#endif
            if (Distance >= ClientWorldManager.Instance.RenderDistance + DiagonalSize)
            {
#if DIAG
               DiagnosticsManager.EliminatedAreasBasicDistance++;
#endif
               return;
            }

            // area viewport elimination with 72° angle tolerance
            if (angle > AwayDistanceAreaEliminationAngle)
            {
#if DIAG
               DiagnosticsManager.EliminatedAreasBasicAngle++;
#endif
               return;
            }
#if DIAG
            DiagnosticsManager.PassedAreasBasic++;
#endif
            CalculateSegmentAngle = (angle > AwayDistanceSegmentAutoAcceptAngle);
            SegmentAngleTolerance = AwayDistanceSegmentEliminationAngle;
         }

         CalculateSegmentDistance = (Distance > (ClientWorldManager.Instance.RenderDistance - DiagonalSize)) || (Distance < SegmentAreaCloseDistance);

         // render segments
         foreach (ClientSegment segment in _Segments)
         {
            if (segment.IsHidden)
            {
               continue;
            }

            segment.Draw(location, direction, ref _ShiftX, ref _ShiftZ);
         }

#if DIAG
         DiagnosticsManager.AreasRendered++;
#endif
      }

      /// <summary>
      /// Gets a segment at the specified index.
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>Segment at specified index</returns>
      public override TSegment GetSegment<TSegment>(ref int index)
      {
         return _Segments[index] as TSegment;
      }

      /// <summary>
      /// Gets the current segment count.
      /// </summary>
      /// <returns>Current segment count</returns>
      public override int GetSegmentCount()
      {
         return _Segments.Count;
      }

      internal List<ClientSegment> GetSegments()
      {
         return _Segments;
      }

      internal bool IsFlaggedForRemoval
      {
         get
         {
            return _Flags[FlagsForRemovalIndex];
         }
         set
         {
            _Flags[FlagsForRemovalIndex] = value;
         }
      }

      /// <summary>
      /// Gets or sets whether this client area is ready for rendering
      /// </summary>
      internal bool IsReady
      {
         get
         {
            return _Flags[FlagsReadyIndex];
         }
         set
         {
            _Flags[FlagsReadyIndex] = value;
         }
      }

      internal float SegmentAngleTolerance { get; private set; }

      /// <summary>
      /// Removes all references held by this area.
      /// </summary>
      public override void Unload()
      {
         base.Unload();

         _Segments.Clear();
         _Segments = null;
         //_VisibleSegments.Clear();
         //_VisibleSegments = null;
      }

      /// <summary>
      /// Gets the visible segments collection.
      /// </summary>
      [Obsolete("Removed for sync issues")]
      internal List<ClientSegment> VisibleSegments
      {
         get
         {
            return _VisibleSegments;
         }
      }
   }
}
