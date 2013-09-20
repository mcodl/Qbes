using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Constants;
using Qbes.Common.Logic.DataStructures;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// Segments are parts of areas and are used as wrappers for actual world
   /// cubes which are merged into blobs.
   /// </summary>
   public abstract class Segment : IComparable<Segment>
   {
      #region Constants
      private const int HeaderSize = 6;
      //private const int OldHeaderSize = 9;
      private const int OffsetBoxData = 6;
      //private const int OldOffsetBoxData = 9;
      private const int OffsetDataLength = 0;
      private const int OffsetVersion = 2;
      //private const int OldOffsetVersion = 5;
      private const int OffsetX = 2;
      private const int OffsetY = 3;
      private const int OffsetZ = 4;
      #endregion

      #region Fields
      //private List<Box> _Boxes = new List<Box>();
      private List<CollisionWall> _CollisionWalls;
      private List<Cube> _Cubes = new List<Cube>();
      private int _Key = -1;
      private object _Lock = new object();
      private uint _Version = 0;
      private BinaryMatrix3D _VisMatrix = new BinaryMatrix3D();
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default segment instance.
      /// </summary>
      public Segment()
      {
         // empty
      }

      /// <summary>
      /// Creates a new segment with given coordinates of its front bottom left
      /// corner.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <param name="area">Wrapping area</param>
      public Segment(int x, int y, int z, Area area)
      {
         X = x;
         Y = y;
         Z = z;

         InitializeFields(area);
         area.AddSegment(this);
      }
      #endregion

      /// <summary>
      /// Adds a cube as a box.
      /// </summary>
      /// <param name="cube">Cube to add as a box</param>
      protected abstract void AddBox(Cube cube);

      /// <summary>
      /// Adds a box to this segment.
      /// </summary>
      /// <typeparam name="TBox">Box type type</typeparam>
      /// <param name="box">Box to add</param>
      public abstract void AddBox<TBox>(TBox box)
         where TBox : Box;

      /// <summary>
      /// Adds a cube to the segment.
      /// </summary>
      /// <param name="cube">Cube to add</param>
      /// <returns>True if the cube was added</returns>
      internal bool AddCube(Cube cube)
      {
         if (_VisMatrix.Get(cube.X % 8, cube.Y % 8, cube.Z % 8))
         {
            return false;
         }

         _Cubes.Add(cube);
         StoreCubeToExistMatrix(cube);

         return true;
      }

      /// <summary>
      /// Gets or sets (protected) the wrapper area.
      /// </summary>
      public virtual Area Area { get; protected set; }

      /// <summary>
      /// Gets or sets (private) the segment's center point.
      /// </summary>
      public Point3D CenterPoint { get; private set; }

      /// <summary>
      /// Clears the held boxes.
      /// </summary>
      protected abstract void ClearBoxes();

      /// <summary>
      /// Compares this segment's coordinates to those of another segment.
      /// </summary>
      /// <param name="other">Segment to compare with</param>
      /// <returns></returns>
      public int CompareTo(Segment other)
      {
         int result = X.CompareTo(other.X);
         if (result == 0)
         {
            result = Y.CompareTo(other.Y);
            if (result == 0)
            {
               result = Z.CompareTo(other.Z);
            }
         }

         return result;
      }

      /// <summary>
      /// Reconstructs all boxes.
      /// </summary>
      public void ConstructBoxes()
      {
         if (GetBoxCount() == 0)
         {
            // construct initial boxes
            foreach (Cube cube in _Cubes)
            {
               AddBox(cube);
            }
         }

         MergeBoxes();
      }

      /// <summary>
      /// Gets whether this object is the same as the given object.
      /// </summary>
      /// <param name="obj">Other object to check equality with</param>
      /// <returns>True if equal</returns>
      public override bool Equals(object obj)
      {
         if (object.ReferenceEquals(this, obj))
         {
            return true;
         }

         if (obj == null || !(obj is Segment))
         {
            return false;
         }

         Segment other = (Segment)obj;
         return (X == other.X && Y == other.Y && Z == other.Z);
      }

      /// <summary>
      /// Generates a segment key based on segment's coordinates.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <returns>Segment key</returns>
      public static int GenerateKey(int x, int y, int z)
      {
         // adjust shift
         while (x >= WorldHelper.SizeX)
         {
            x -= WorldHelper.SizeX;
         }
         while (x < 0)
         {
            x += WorldHelper.SizeX;
         }
         while (z >= WorldHelper.SizeZ)
         {
            z -= WorldHelper.SizeZ;
         }
         while (z < 0)
         {
            z += WorldHelper.SizeZ;
         }

         // assemble the key from its parts
         x /= 8;
         y /= 8;
         z /= 8;
         return x + (y * 8192) + (z * 262144);
      }

      /// <summary>
      /// Gets the owner area of this segment.
      /// </summary>
      /// <returns>Owner area of this segment</returns>
      public virtual Area GetArea()
      {
         return Area;
      }

      /// <summary>
      /// Gets a box at the specified index.
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>Box at specified index</returns>
      public abstract TBox GetBox<TBox>(ref int index)
         where TBox : Box;

      /// <summary>
      /// Gets the current box count.
      /// </summary>
      /// <returns>Current box count</returns>
      public abstract int GetBoxCount();

      /// <summary>
      /// Gets a list with all boxes.
      /// </summary>
      /// <returns>List with boxes</returns>
      protected abstract List<Box> GetBoxes();

      /// <summary>
      /// Gets a list with all boxes in a thread-safe manner.
      /// </summary>
      /// <returns>List with boxes</returns>
      public List<Box> GetBoxesSynchronized()
      {
         lock (_Lock)
         {
            return GetBoxes();
         }
      }

      internal List<CollisionWall> GetCollisionWalls()
      {
         lock (_Lock)
         {
            if (_CollisionWalls == null)
            {
               _CollisionWalls = new List<CollisionWall>();
               Segment neighbour = null;

               for (int x = 0; x < 8; x++)
               {
                  for (int y = 0; y < 8; y++)
                  {
                     for (int z = 0; z < 8; z++)
                     {
                        if (!_VisMatrix.Get(ref x, ref y, ref z))
                        {
                           // skip empty space
                           continue;
                        }

                        Box box = new Box(new Cube(X + x, Y + y, Z + z, 0), this);

                        // check front X wall
                        if (x == 0)
                        {
                           neighbour = WorldHelper.GetSegment(X - 8, Y, Z);
                           if (neighbour != null && !neighbour._VisMatrix.Get(7, y, z))
                           {
                              _CollisionWalls.Add(new CollisionWall(box, Sides.FrontX));
                           }
                        }
                        else if (!_VisMatrix.Get(x - 1, y, z))
                        {
                           _CollisionWalls.Add(new CollisionWall(box, Sides.FrontX));
                        }

                        // check back X wall
                        if (x == 7)
                        {
                           neighbour = WorldHelper.GetSegment(X + 8, Y, Z);
                           if (neighbour != null && !neighbour._VisMatrix.Get(0, y, z))
                           {
                              _CollisionWalls.Add(new CollisionWall(box, Sides.BackX));
                           }
                        }
                        else if (!_VisMatrix.Get(x + 1, y, z))
                        {
                           _CollisionWalls.Add(new CollisionWall(box, Sides.BackX));
                        }

                        // check front Z wall
                        if (z == 0)
                        {
                           neighbour = WorldHelper.GetSegment(X, Y, Z - 8);
                           if (neighbour != null && !neighbour._VisMatrix.Get(x, y, 7))
                           {
                              _CollisionWalls.Add(new CollisionWall(box, Sides.FrontZ));
                           }
                        }
                        else if (!_VisMatrix.Get(x, y, z - 1))
                        {
                           _CollisionWalls.Add(new CollisionWall(box, Sides.FrontZ));
                        }

                        // check back Z wall
                        if (z == 7)
                        {
                           neighbour = WorldHelper.GetSegment(X, Y, Z + 8);
                           if (neighbour != null && !neighbour._VisMatrix.Get(x, y, 0))
                           {
                              _CollisionWalls.Add(new CollisionWall(box, Sides.BackZ));
                           }
                        }
                        else if (!_VisMatrix.Get(x, y, z + 1))
                        {
                           _CollisionWalls.Add(new CollisionWall(box, Sides.BackZ));
                        }
                     }
                  }
               }
            }

            return _CollisionWalls;
         }
      }

      /// <summary>
      /// Gets the hash code.
      /// </summary>
      /// <returns>Hash code</returns>
      public override int GetHashCode()
      {
         return Key;
      }

      private void InitializeFields(Area area)
      {
         CenterPoint = new Point3D(X + 4, Y + 4, Z + 4);
         _Key = GenerateKey(X, Y, Z);

         Area = area;
      }

      /// <summary>
      /// Initializes this segment from binary data. Note that this will also
      /// load the segment's boxes.
      /// </summary>
      /// <param name="data">Array with serialized data</param>
      /// <param name="offset">Offset</param>
      /// <param name="area">Wrapping area</param>
      /// <param name="x">X diff coordinate</param>
      /// <param name="y">z diff coordinate</param>
      /// <param name="z">Z diff coordinate</param>
      internal void InitializeFromByteArray<TBox>(ref byte[] data, ref int offset, Area area, ref int x, ref int y, ref int z)
         where TBox : Box, new()
      {
         short dataLength = BitConverter.ToInt16(data, offset + OffsetDataLength);

         X = x + area.X;
         Y = y + area.Y;
         Z = z + area.Z;
         _Version = BitConverter.ToUInt32(data, offset + OffsetVersion);

         InitializeFields(area);

         for (int i = OffsetBoxData; i < dataLength; i += Box.SerializedSize)
         {
            TBox box = (TBox)WorldHelper.GetPooledBox();
            box.InitializeFromByteArray(ref data, offset + i, this);
            AddBox(box);
         }

         offset += dataLength;

         OnInitialized();
      }

      /// <summary>
      /// Gets the segment's key.
      /// </summary>
      public int Key
      {
         get
         {
            return _Key;
         }
      }

      /// <summary>
      /// Merges the held boxes.
      /// </summary>
      protected abstract void MergeBoxes();

      /// <summary>
      /// Fired when this segment is changed.
      /// </summary>
      protected virtual void OnChanged()
      {
         // empty
      }

      /// <summary>
      /// Fired when the initialization from byte array is complete.
      /// </summary>
      protected virtual void OnInitialized()
      {
         // empty
      }

      private bool PlaceOrRemoveBlock(bool place, int x, int y, int z, ushort materialID)
      {
         // construct cubes
         _Cubes.Clear();
         foreach (Box box in GetBoxes())
         {
            _Cubes.AddRange(box.GetCubesList());
         }

         if (place)
         {
            // add the new cube
            if (!AddCube(new Cube(x, y, z, materialID)))
            {
               return false;
            }
         }
         else
         {
            // remove old cube from vis matrix and cubes
            _VisMatrix.Set(x % 8, y % 8, z % 8, false);
            _Cubes.RemoveAll(c => c.X == x && c.Y == y && c.Z == z);
         }

         // reconstruct boxes
         ClearBoxes();
         ConstructBoxes();
         _Cubes.Clear();

         // reset collision walls
         _CollisionWalls = null;

         // send signal that segment changed
         Area.IsChanged = true;
         OnChanged();

         return true;
      }
      
      /// <summary>
      /// Places or removes a block.
      /// </summary>
      /// <param name="place">True to place, false to remove</param>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <param name="materialID">Material ID (used only for placing)</param>
      /// <param name="version">New version</param>
      public void PlaceOrRemoveBlockAndSetVersion(bool place, int x, int y, int z, ushort materialID, uint version)
      {
         lock (_Lock)
         {
            PlaceOrRemoveBlock(place, x, y, z, materialID);
            _Version = version;
         }
      }

      /// <summary>
      /// Places or removes a block.
      /// </summary>
      /// <param name="place">True to place, false to remove</param>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <param name="materialID">Material ID (used only for placing)</param>
      /// <returns>Current version</returns>
      public uint PlaceOrRemoveBlockAndUpdateVersion(bool place, int x, int y, int z, ushort materialID)
      {
         lock (_Lock)
         {
            if (PlaceOrRemoveBlock(place, x, y, z, materialID))
            {
               _Version++;
            }

            return _Version;
         }
      }

      /// <summary>
      /// Serializes box data into given byte array.
      /// </summary>
      /// <param name="data">Target byte array</param>
      /// <param name="offset">Current cursor position</param>
      internal void Serialize(ref byte[] data, ref int offset)
      {
         Area area = GetArea();

         BitConverter.GetBytes(SerializedSize).CopyTo(data, offset);
         //data[OffsetX + offset] = (byte)(X - area.X);
         //data[OffsetY + offset] = (byte)(Y - area.Y);
         //data[OffsetZ + offset] = (byte)(Z - area.Z);

         // write the boxes
         lock (_Lock)
         {
            BitConverter.GetBytes(_Version).CopyTo(data, offset + OffsetVersion);
            offset += OffsetBoxData;
            foreach (Box box in GetBoxes())
            {
               box.Serialize(ref data, ref offset);
            }
         }
      }

      /// <summary>
      /// Gets the size of serialized data.
      /// </summary>
      public short SerializedSize
      {
         get
         {
            return (short)(HeaderSize + Box.SerializedSize * GetBoxCount());
         }
      }

      /// <summary>
      /// Stores given box into the visibility matrix.
      /// </summary>
      /// <param name="box">Box to store</param>
      protected void StoreBoxToExistMatrix(Box box)
      {
         foreach (Cube cube in box.GetCubesList())
         {
            StoreCubeToExistMatrix(cube);
         }
      }

      private void StoreCubeToExistMatrix(Cube cube)
      {
         _VisMatrix.Set(cube.X % 8, cube.Y % 8, cube.Z % 8, true);
      }

      /// <summary>
      /// Removes all references held by this segment.
      /// </summary>
      public virtual void Unload()
      {
         Area = null;
         //_Boxes.ForEach(b => b.Unload());
         //_Boxes.Clear();
         if (_CollisionWalls != null)
         {
            _CollisionWalls.ForEach(c => c.Unload());
            _CollisionWalls = null;
         }
         _Cubes.Clear();
         _VisMatrix.Reset();
      }

      /// <summary>
      /// Gets the current segment version.
      /// </summary>
      public uint Version
      {
         get
         {
            lock (_Lock)
            {
               return _Version;
            }
         }
      }

      /// <summary>
      /// Gets the vis matrix.
      /// </summary>
      public BinaryMatrix3D VisMatrix
      {
         get
         {
            return _VisMatrix;
         }
      }

      /// <summary>
      /// Gets or sets (private) the X coordinate.
      /// </summary>
      public int X { get; private set; }

      /// <summary>
      /// Gets or sets (private) the Y coordinate.
      /// </summary>
      public int Y { get; private set; }

      /// <summary>
      /// Gets or sets (private) the Z coordinate.
      /// </summary>
      public int Z { get; private set; }
   }
}
