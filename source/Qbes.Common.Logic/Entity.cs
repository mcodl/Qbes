using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Constants;
using Qbes.Common.Logic.Exceptions;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// The entity class is a common base for players, NPCs and mobs.
   /// </summary>
   public abstract class Entity
   {
      #region Constants
      private const float BlockEntityCollisionSelectRange = 25.0f;
      /// <summary>
      /// Range at which the entity can still place or remove blocks.
      /// </summary>
      public const float BlockInteractionRange = 16.0f;
      private const int OffsetEntityType = 0;
      private const int OffsetID = 1;
      private const int OffsetLocation = 3;
      private const int OffsetRotationLeft = 15;
      private const int OffsetRotationUp = 19;
      /// <summary>
      /// Serialized size of entity without inheritor's data.
      /// </summary>
      public const int SerializedSize = 23;
      #endregion

      #region Events
      /// <summary>
      /// This event is fired when the entity changes its column.
      /// </summary>
      public event EventHandler OnColumnChanged;
      #endregion

      #region Static fields
      private static readonly Point3D _Direction = new Point3D(0.0f, 0.0f, -128.0f);
      private static ushort _NextId = 1;
      private static object _NextIdLock = new object();
      private static float _ZeroUpDownRotation = 0.0f;
      #endregion

      #region Fields
      private bool _CanMove;
      private bool _CanPlaceBlocks;
      private int _CurrentColumnX;
      private int _CurrentColumnZ;
      private Segment _CurrentSegment;
      private Segment[] _CurrentSegmentNeighbours;
      private int _CurrentTime;
      private byte _EntityType;
      private float _EyeHeightFromCenter;
      private bool _Falling;
      private float _HalfSizeX;
      private float _HalfSizeY;
      private ushort _ID;
      private int _LastMoveTime;
      private float _NewRotationLeft;
      private float _NewRotationUp;
      private Point3D _PreviousCheckPoint = new Point3D();
      private float _RotationLeft;
      private float _RotationUp;
      private int _SkinTextureName = -1;
      private bool _Suffocating;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a new entity.
      /// </summary>
      /// <param name="entityType">Entity type</param>
      /// <param name="halfSizeX">Half size X</param>
      /// <param name="halfSizeY">Half size Y</param>
      /// <param name="eyeHeightFromCenter">Height of the eyes from the entity's
      /// center point</param>
      /// <param name="canMove">Determines whether entity can move</param>
      /// <param name="canPlaceBlocks">Determines whether entity can place
      /// blocks</param>
      protected Entity(byte entityType, float halfSizeX, float halfSizeY,
                       float eyeHeightFromCenter, bool canMove,
                       bool canPlaceBlocks)
      {
         _EntityType = entityType;
         _HalfSizeX = halfSizeX;
         _HalfSizeY = halfSizeY;
         _EyeHeightFromCenter = eyeHeightFromCenter;
         _CanMove = canMove;
         _CanPlaceBlocks = canPlaceBlocks;

         lock (_NextIdLock)
         {
            _ID = (_NextId++);
         }
         _LastMoveTime = Environment.TickCount;
      }
      #endregion

      /// <summary>
      /// Gets the base direction vector.
      /// </summary>
      public static Point3D BaseDirectionVector
      {
         get
         {
            return _Direction;
         }
      }

      private void CheckSegmentChanged()
      {
         int segmentX = Convert.ToInt32(Math.Truncate(Location.X));
         segmentX -= segmentX % 8;
         if (segmentX >= WorldHelper.SizeX)
         {
            segmentX -= WorldHelper.SizeX;
         }
         else if (segmentX < 0)
         {
            segmentX += WorldHelper.SizeX;
         }
         int segmentY = Convert.ToInt32(Math.Truncate(Location.Y));
         segmentY -= segmentY % 8;
         int segmentZ = Convert.ToInt32(Math.Truncate(Location.Z));
         segmentZ -= segmentZ % 8;
         if (segmentZ >= WorldHelper.SizeZ)
         {
            segmentZ -= WorldHelper.SizeZ;
         }
         else if (segmentZ < 0)
         {
            segmentZ += WorldHelper.SizeZ;
         }

         bool refreshNeighbours = false;
         // check if segment needs to be changed
         if (_CurrentSegment == null ||
             !(_CurrentSegment.X == segmentX &&
               _CurrentSegment.Y == segmentY &&
               _CurrentSegment.Z == segmentZ))
         {
            _CurrentSegment = WorldHelper.GetSegment(segmentX, segmentY, segmentZ);
            refreshNeighbours = true;
         }

         Point3D inBorderCheckPoint = new Point3D(Location.X - _CurrentSegment.X,
                                                  Location.Y - _CurrentSegment.Y,
                                                  Location.Z - _CurrentSegment.Z);
         bool xRefresh = (inBorderCheckPoint.X < 4 != _PreviousCheckPoint.X < 4);
         bool yRefresh = (inBorderCheckPoint.Y < 4 != _PreviousCheckPoint.Y < 4);
         bool zRefresh = (inBorderCheckPoint.Z < 4 != _PreviousCheckPoint.Z < 4);
         // check if neighbours are to be refreshed
         if (refreshNeighbours || xRefresh || yRefresh || zRefresh)
         {
            _CurrentSegmentNeighbours = WorldHelper.GetSegmentNeighboursForCollisions(_CurrentSegment, Location);
         }

         _PreviousCheckPoint = inBorderCheckPoint;

         // check if column area isn't changed
         int x = Convert.ToInt32(Math.Truncate(Location.X));
         x -= x % 64;
         int z = Convert.ToInt32(Math.Truncate(Location.Z));
         z -= z % 64;

         if (CurrentColumnX != x || CurrentColumnZ != z)
         {
            _CurrentColumnX = x;
            _CurrentColumnZ = z;

            if (OnColumnChanged != null)
            {
               OnColumnChanged(this, EventArgs.Empty);
            }
         }
      }

      /// <summary>
      /// Initializes this entity from serialized data.
      /// </summary>
      /// <param name="data">Array with serialized data</param>
      public static Entity CreateFromByteArray(byte[] data)
      {
         switch(data[OffsetEntityType])
         {
            case EntityTypes.Player:
               Player player = new Player();
               player.InitializeFromByteArray(ref data);
               return player;
            default:
               throw new UnknownEntityTypeException(data[OffsetEntityType]);
         }
      }

      /// <summary>
      /// Gets the current X column.
      /// </summary>
      public int CurrentColumnX
      {
         get
         {
            return _CurrentColumnX;
         }
      }

      /// <summary>
      /// Gets the current Z column.
      /// </summary>
      public int CurrentColumnZ
      {
         get
         {
            return _CurrentColumnZ;
         }
      }

      /// <summary>
      /// Gets the current segment.
      /// </summary>
      public Segment CurrentSegment
      {
         get
         {
            return _CurrentSegment;
         }
      }

      /// <summary>
      /// Gets the current segment's neighbour at given index.
      /// </summary>
      /// <param name="index">Neighbour index</param>
      /// <returns>Neighbour at given index</returns>
      public Segment GetCurrentSegmentNeighbour(int index)
      {
         return _CurrentSegmentNeighbours[index];
      }

      /// <summary>
      /// Gets the selected material.
      /// </summary>
      /// <returns>Selected material ID</returns>
      public abstract ushort GetSelectedMaterial();

      /// <summary>
      /// Gets the serialized length in bytes of the deriving portion.
      /// </summary>
      /// <returns>Serialized length in bytes</returns>
      protected abstract int GetSerializedSize();

      /// <summary>
      /// Gets the half os X size.
      /// </summary>
      public float HalfSizeX
      {
         get
         {
            return _HalfSizeX;
         }
      }

      /// <summary>
      /// Gets the half os Y size.
      /// </summary>
      public float HalfSizeY
      {
         get
         {
            return _HalfSizeY;
         }
      }

      /// <summary>
      /// Handles the entity movement in the world.
      /// </summary>
      /// <returns>True if moved</returns>
      public bool HandleMovement()
      {
         bool moved = false;

         if (!_CanMove)
         {
            return moved;
         }

         // calculate movement
         _CurrentTime = Environment.TickCount;
         if (MoveX != 0 || MoveY != 0 || MoveZ != 0)
         {
            moved = true;

            if (_CurrentSegment == null)
            {
               CheckSegmentChanged();
            }

            double distance = (_CurrentTime - _LastMoveTime) * Speed.EntityBaseSpeed;
            Point3D move = new Point3D(MoveX * distance, MoveY * distance, MoveZ * distance);
            move.Rotate(ref _RotationLeft, ref _ZeroUpDownRotation);
            Point3D original = new Point3D(Location);
            Location += move;

            // check collisions and adjust location if needed
            Vector3D moveVector = new Vector3D(original, Location);
            List<Box> boxesForCheck = new List<Box>(_CurrentSegment.GetBoxesSynchronized());
            foreach (Segment neighbour in _CurrentSegmentNeighbours)
            {
               if (neighbour != null)
               {
                  boxesForCheck.AddRange(neighbour.GetBoxesSynchronized());
               }
            }

            // floor/ceiling collisions
            _Falling = true;
            foreach (Box box in boxesForCheck)
            {
               Location.Y += box.CheckFloorCeilingCollisions(moveVector, ref _HalfSizeX,
                                                             ref _HalfSizeY, ref _Falling);
            }
            // world floor/ceiling check
            if (Location.Y - _HalfSizeY < 0.0f)
            {
               Location.Y = _HalfSizeY;
            }
            else if (Location.Y + _HalfSizeY > 255.0f)
            {
               Location.Y = 255.0f - _HalfSizeY;
            }

            // assemble collision walls
            List<CollisionWall> walls = new List<CollisionWall>(_CurrentSegment.GetCollisionWalls());
            foreach (Segment neighbour in _CurrentSegmentNeighbours)
            {
               if (neighbour != null)
               {
                  walls.AddRange(neighbour.GetCollisionWalls());
               }
            }
            // walls.RemoveAll(w => w.GetBoxCenterPointDistanceSquare(Location) > 4.0f);
            // check wall collisions
            float[] xData = new float[] { 512.0f, 0.0f };
            float[] zData = new float[] { 512.0f, 0.0f };
            foreach (CollisionWall wall in walls)
            {
               if (wall.Side == Sides.FrontX || wall.Side == Sides.BackX)
               {
                  wall.CheckWallCollisions(moveVector, ref _HalfSizeX, ref _HalfSizeY, ref xData);
               }
               if (wall.Side == Sides.FrontZ || wall.Side == Sides.BackZ)
               {
                  wall.CheckWallCollisions(moveVector, ref _HalfSizeX, ref _HalfSizeY, ref zData);
               }
            }
            // apply shift based on wall collisons
            Location.X += xData[1];
            Location.Z += zData[1];

            // check suffocation
            _Suffocating = false;
            foreach (Box box in boxesForCheck)
            {
               box.CheckSuffocation(Location, ref _HalfSizeX, ref _HalfSizeY,
                                    ref _Suffocating);
            }

            // check move over map edge
            if (Location.X < 0)
            {
               Location.X += WorldHelper.SizeX;
            }
            else if (Location.X >= WorldHelper.SizeX)
            {
               Location.X -= WorldHelper.SizeX;
            }

            if (Location.Z < 0)
            {
               Location.Z += WorldHelper.SizeZ;
            }
            else if (Location.Z >= WorldHelper.SizeZ)
            {
               Location.Z -= WorldHelper.SizeZ;
            }

            // check if segment changed
            CheckSegmentChanged();
         }

         // calculate new direction
         moved |= HandleRotation();

         // update last moved time
         _LastMoveTime = _CurrentTime;

         return moved;
      }

      private bool HandleRotation()
      {
         bool rotated = false;

         if (_RotationUp != NewRotationUp || _RotationLeft != NewRotationLeft)
         {
            rotated = true;

            _RotationUp = NewRotationUp;
            _RotationLeft = NewRotationLeft;
         }

         NewDirection = Location + _Direction;
         NewDirection.Rotate(Location, ref _RotationLeft, ref _RotationUp);

         return rotated;
      }

      /// <summary>
      /// Gets the entity ID.
      /// </summary>
      public ushort ID
      {
         get
         {
            return _ID;
         }
      }

      internal void InitializeTerrainDependencies()
      {
         CheckSegmentChanged();
      }

      private void InitializeFromByteArray(ref byte[] data)
      {
         // initialize the base part
         Location = new Point3D();
         _EntityType = data[OffsetEntityType];
         _ID = BitConverter.ToUInt16(data, OffsetID);
         Location.InitializeFromByteArray(ref data, OffsetLocation);
         _RotationLeft = BitConverter.ToSingle(data, OffsetRotationLeft);
         _NewRotationLeft = _RotationLeft;
         _RotationUp = BitConverter.ToSingle(data, OffsetRotationUp);
         _NewRotationUp = _RotationUp;

         int x = Convert.ToInt32(Location.X);
         int z = Convert.ToInt32(Location.Z);
         _CurrentColumnX = x - x % 64;
         _CurrentColumnZ = z - z % 64;

         lock (_NextIdLock)
         {
            if (_NextId <= ID)
            {
               _NextIdLock = ID + 1;
            }
         }

         // initialize the deriving part
         InitializeFromByteArray(ref data, SerializedSize);
      }

      /// <summary>
      /// Initializes this entity from serialized data.
      /// </summary>
      /// <param name="data">Array with serialized data</param>
      /// <param name="offset">Offset</param>
      protected abstract void InitializeFromByteArray(ref byte[] data, int offset);

      /// <summary>
      /// Gets or sets (private) the last network update.
      /// </summary>
      public int LastLocationUpdate { get; private set; }

      /// <summary>
      /// Gets or sets (protected) current location in the world.
      /// </summary>
      /// <remarks>This position is actualy only of the eyes</remarks>
      public Point3D Location { get; protected set; }

      /// <summary>
      /// Gets or sets the X move value (-1, 0, 1).
      /// </summary>
      public sbyte MoveX { get; set; }

      /// <summary>
      /// Gets or sets the Y move value (-1, 0, 1).
      /// </summary>
      public sbyte MoveY { get; set; }

      /// <summary>
      /// Gets or sets the Z move value (-1, 0, 1).
      /// </summary>
      public sbyte MoveZ { get; set; }

      /// <summary>
      /// Gets or sets (private) new direction.
      /// </summary>
      public Point3D NewDirection { get; private set; }

      /// <summary>
      /// Gets or sets new left/right rotation.
      /// </summary>
      public float NewRotationLeft
      {
         get
         {
            return _NewRotationLeft;
         }
         set
         {
            while (value >= 360.0f)
            {
               value -= 360.0f;
            }
            while (value < 0)
            {
               value += 360.0f;
            }

            _NewRotationLeft = value;
         }
      }

      /// <summary>
      /// Gets or sets new up/down rotation
      /// </summary>
      public float NewRotationUp
      {
         get
         {
            return _NewRotationUp;
         }
         set
         {
            if (value >= 85.0f)
            {
               value = 85.0f;
            }
            else if (value <= -85.0f)
            {
               value = -85.0f;
            }

            _NewRotationUp = value;
         }
      }

      /// <summary>
      /// Places or removes a block.
      /// </summary>
      /// <param name="place">True to place block</param>
      public void PlaceOrRemoveBlock(bool place)
      {
         int x = 0;
         int y = 0;
         int z = 0;
         uint version = 0;
         PlaceOrRemoveBlock(place, out x, out y, out z, out version);
      }

      /// <summary>
      /// Places or removes a block.
      /// </summary>
      /// <param name="place">True to place block</param>
      /// <param name="x">X coordinate of the placed/removed block</param>
      /// <param name="y">Y coordinate of the placed/removed block</param>
      /// <param name="z">Z coordinate of the placed/removed block</param>
      /// <param name="version">Current segment version</param>
      /// <returns>True if block has been placed/removed</returns>
      public bool PlaceOrRemoveBlock(bool place, out int x, out int y, out int z, out uint version)
      {
         x = -1;
         y = -1;
         z = -1;
         version = 0;

         if (!_CanPlaceBlocks)
         {
            return false;
         }

         // check closest intersection
         Point3D eyeLocation = new Point3D(Location, 0.0f, _EyeHeightFromCenter, 0.0f);
         Vector3D line = new Vector3D(eyeLocation, NewDirection);
         Intersection closest = new Intersection();
         List<Tuple<Box, int, int>> boxes = new List<Tuple<Box, int, int>>();

         // first add current segment's boxes which have 0 shift
         foreach (Box box in _CurrentSegment.GetBoxesSynchronized())
         {
            boxes.Add(Tuple.Create(box, 0, 0));
         }
         // now add other boxes and determine possible shift
         foreach (Segment neighbour in _CurrentSegmentNeighbours)
         {
            int shiftX = 0;
            if (_CurrentSegment.X - neighbour.X > WorldHelper.HalfSizeX)
            {
               shiftX = WorldHelper.SizeX;
            }
            else if (neighbour.X - _CurrentSegment.X > WorldHelper.HalfSizeX)
            {
               shiftX = -WorldHelper.SizeX;
            }

            int shiftZ = 0;
            if (_CurrentSegment.Z - neighbour.Z > WorldHelper.HalfSizeZ)
            {
               shiftZ = WorldHelper.SizeZ;
            }
            else if (neighbour.Z - _CurrentSegment.Z > WorldHelper.HalfSizeZ)
            {
               shiftZ = -WorldHelper.SizeZ;
            }

            foreach (Box box in neighbour.GetBoxesSynchronized())
            {
               boxes.Add(Tuple.Create(box, shiftX, shiftZ));
            }
         }

         if (eyeLocation.Y * eyeLocation.Y < BlockInteractionRange)
         {
            boxes.Add(Tuple.Create(Box.FloorBox, 0, 0));
         }

         int reverseShiftX = 0;
         int reverseShiftZ = 0;
         foreach (Tuple<Box, int, int> box in boxes)
         {
            Intersection intersection = box.Item1.GetIntersection(line, box.Item2, box.Item3);
            if (intersection.Distance < closest.Distance)
            {
               closest = intersection;
               reverseShiftX = -box.Item2;
               reverseShiftZ = -box.Item3;
            }
         }

         // check if there is a close intersection at all
         if (closest.IntersectionPoint == null || closest.Distance > BlockInteractionRange)
         {
            // not close enough or no point at all
            return false;
         }

         // adjust the reverse shift by accounting for reverse number truncating
         // when below 0
         if (closest.IntersectionPoint.X < 0 && closest.IntersectionPoint.X % 1 != 0)
         {
            reverseShiftX--;
         }
         if (closest.IntersectionPoint.Z < 0 && closest.IntersectionPoint.Z % 1 != 0)
         {
            reverseShiftZ--;
         }
         // calculate cube location without the shift
         x = Convert.ToInt32(Math.Truncate(closest.IntersectionPoint.X)) + reverseShiftX;
         y = Convert.ToInt32(Math.Truncate(closest.IntersectionPoint.Y));
         z = Convert.ToInt32(Math.Truncate(closest.IntersectionPoint.Z)) + reverseShiftZ;

         if (place)
         {
            // correct the location based on intersection side
            switch (closest.Side)
            {
               case Sides.FrontX:
                  x--;
                  break;
               case Sides.FrontY:
                  y--;
                  break;
               case Sides.FrontZ:
                  z--;
                  break;
            }
         }
         else
         {
            // correct the location based on intersection side
            switch (closest.Side)
            {
               case Sides.BackX:
                  x--;
                  break;
               case Sides.BackY:
                  y--;
                  break;
               case Sides.BackZ:
                  z--;
                  break;
            }
         }

         // check for floor/ceiling limits
         if (y < 0 || y > byte.MaxValue)
         {
            return false;
         }

         // adjust possible shift over the edge of the world after correction
         if (x < 0)
         {
            x += WorldHelper.SizeX;
         }
         else if (x >= WorldHelper.SizeX)
         {
            x -= WorldHelper.SizeX;
         }

         if (z < 0)
         {
            z += WorldHelper.SizeZ;
         }
         else if (z >= WorldHelper.SizeZ)
         {
            z -= WorldHelper.SizeZ;
         }

         // check if placing the block will clip into an entity
         if (place)
         {
            Box box = new Box(new Cube(x, y, z, 0), null);
            List<Entity> entities = WorldHelper.GetEntities(box.CenterPoint, BlockEntityCollisionSelectRange);

            bool collides = false;
            foreach (Entity entity in entities)
            {
               box.CheckSuffocation(entity.Location, ref entity._HalfSizeX, ref entity._HalfSizeY, ref collides);
               if (collides)
               {
                  // at least one entity collides with the newly placed box
                  return false;
               }
            }
         }

         // retrieve the destination segment and place or remove the block
         Segment destinationSegment = WorldHelper.GetSegment(x, y, z);
         version = destinationSegment.PlaceOrRemoveBlockAndUpdateVersion(place, x, y, z, GetSelectedMaterial());

         return true;
      }

      /// <summary>
      /// Gets the previous segment changed checkpoint.
      /// </summary>
      public Point3D PreviousCheckPoint
      {
         get
         {
            return _PreviousCheckPoint;
         }
      }

      /// <summary>
      /// Gets current left/right rotation.
      /// </summary>
      public float RotationLeft
      {
         get
         {
            return _RotationLeft;
         }
      }

      /// <summary>
      /// Gets current up/down rotation
      /// </summary>
      public float RotationUp
      {
         get
         {
            return _RotationUp;
         }
      }

      /// <summary>
      /// Serializes entity into a byte array.
      /// </summary>
      /// <returns>Byte array with serialized entity</returns>
      public byte[] Serialize()
      {
         // prepare target byte array
         byte[] data = new byte[SerializedSize + GetSerializedSize()];

         // serialize portion of this base type
         data[OffsetEntityType] = _EntityType;
         BitConverter.GetBytes(ID).CopyTo(data, OffsetID);
         Location.Serialize(ref data, OffsetLocation);
         BitConverter.GetBytes(RotationLeft).CopyTo(data, OffsetRotationLeft);
         BitConverter.GetBytes(RotationUp).CopyTo(data, OffsetRotationUp);

         // add the portion from the derivative type
         Serialize(ref data, SerializedSize);

         // return the result
         return data;
      }

      /// <summary>
      /// Serializes the derived portion into given array at given offset.
      /// </summary>
      /// <param name="data">Target byte array</param>
      /// <param name="offset">Offset</param>
      protected abstract void Serialize(ref byte[] data, int offset);

      /// <summary>
      /// Set the entity location and direction.
      /// </summary>
      /// <param name="location">Current location</param>
      /// <param name="rotationLeft">Rotation left</param>
      /// <param name="rotationUp">Rotation up</param>
      /// <param name="handleMovement">Determines whether movement should be
      /// handled</param>
      /// <param name="movedTime">Moved time</param>
      public void SetLocation(Point3D location, float rotationLeft, float rotationUp, bool handleMovement, int movedTime)
      {
         if (movedTime < LastLocationUpdate)
         {
            return;
         }

         LastLocationUpdate = movedTime;
         Location = location;
         NewRotationLeft = rotationLeft;
         NewRotationUp = rotationUp;
         if (handleMovement)
         {
            CheckSegmentChanged();
            HandleMovement();
         }
         else
         {
            HandleRotation();
         }
      }

      /// <summary>
      /// Gets the texture name for this entity.
      /// </summary>
      /// <remarks>Needed only by client</remarks>
      public int SkinTextureName
      {
         get
         {
            return _SkinTextureName;
         }
         set
         {
            _SkinTextureName = value;
         }
      }
   }
}
