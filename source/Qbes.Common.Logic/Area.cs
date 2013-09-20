using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// Area is the largest wrapper object in the world. Its size is 64 * 64 * 64
   /// and contains segments (up to 512).
   /// </summary>
   public abstract class Area
   {
      #region Constants
      private const int HeaderSize = 9;
      private const int OffsetSegmentData = 9;
      private const int OffsetDataLength = 0;
      private const int OffsetX = 4;
      private const int OffsetY = 6;
      private const int OffsetZ = 7;
      #endregion

      #region Fields
      private int _Key = -1;
      //private List<Segment> _Segments = new List<Segment>();
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default area instance.
      /// </summary>
      public Area()
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
      public Area(int x, int y, int z)
      {
         X = x;
         Y = y;
         Z = z;
         InitializeFields(x, y, z);
      }
      #endregion

      /// <summary>
      /// Adds a segment to this area.
      /// </summary>
      /// <typeparam name="TSegment">Segment type</typeparam>
      /// <param name="segment">Segment to add</param>
      public abstract void AddSegment<TSegment>(TSegment segment)
         where TSegment : Segment;

      /// <summary>
      /// Gets or sets (private) the area's center point.
      /// </summary>
      public Point3D CenterPoint { get; private set; }

      /// <summary>
      /// Generates an area key based on area's coordinates.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <returns>Area key</returns>
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
         x /= 64;
         y /= 64;
         z /= 64;
         return x + (y * 1024) + (z * 4096);
      }

      /// <summary>
      /// Gets the area hashcode.
      /// </summary>
      /// <returns>Area hashcode</returns>
      public override int GetHashCode()
      {
         return Key;
      }

      /// <summary>
      /// Gets a segment at the specified index.
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>Segment at specified index</returns>
      public abstract TSegment GetSegment<TSegment>(ref int index)
         where TSegment : Segment;

      /// <summary>
      /// Gets the current segment count.
      /// </summary>
      /// <returns>Current segment count</returns>
      public abstract int GetSegmentCount();

      private void InitializeFields(int x, int y, int z)
      {
         CenterPoint = new Point3D(x + 32, y + 32, z + 32);
         _Key = GenerateKey(X, Y, Z);
         IsInitialized = true;
      }

      /// <summary>
      /// Creates an area from binary data. Note that this will also load the
      /// segments and their boxes.
      /// </summary>
      /// <param name="data">Array with serialized data</param>
      public void InitializeFromByteArray<TSegment, TBox>(byte[] data)
         where TSegment : Segment, new()
         where TBox : Box, new()
      {
         int offset = 0;
         InitializeFromByteArray<TSegment, TBox>(data, ref offset);
      }

      /// <summary>
      /// Creates an area from binary data. Note that this will also load the
      /// segments and their boxes.
      /// </summary>
      /// <param name="data">Array with serialized data</param>
      /// <param name="offset">Offset</param>
      public void InitializeFromByteArray<TSegment, TBox>(byte[] data, ref int offset)
         where TSegment : Segment, new()
         where TBox : Box, new()
      {
         int length = BitConverter.ToInt32(data, offset);

         X = BitConverter.ToUInt16(data, offset + OffsetX) * 64;
         Y = data[offset + OffsetY] * 64;
         Z = BitConverter.ToUInt16(data, offset + OffsetZ) * 64;

         // read segments
         int segmentOffset = offset + OffsetSegmentData;
         int x = 0;
         int y = 0;
         int z = 0;
         while (segmentOffset < offset + length)
         {
            TSegment segment = (TSegment)WorldHelper.GetPooledSegment();
            segment.InitializeFromByteArray<TBox>(ref data, ref segmentOffset, this, ref x, ref y, ref z);
            AddSegment(segment);

            z += 8;
            if (z == 64)
            {
               z = 0;
               y += 8;
               if (y == 64)
               {
                  y = 0;
                  x += 8;
               }
            }
         }

         offset += length;

         InitializeFields(X, Y, Z);
      }

      /// <summary>
      /// Gets or sets whether this area has been changed.
      /// </summary>
      public bool IsChanged { get; set; }

      /// <summary>
      /// Gets or sets (private) if this area is initialized.
      /// </summary>
      public bool IsInitialized { get; private set; }

      /// <summary>
      /// Gets the area key.
      /// </summary>
      public int Key
      {
         get
         {
            return _Key;
         }
      }

      /// <summary>
      /// Gets a byte array with serialized area.
      /// </summary>
      /// <returns></returns>
      public byte[] Serialize()
      {
         // calculate total size
         int size = HeaderSize;
         for (int i = 0; i < GetSegmentCount(); i++)
         {
            Segment segment = GetSegment<Segment>(ref i);
            size += segment.SerializedSize;
         }

         byte[] data = new byte[size];

         // write area header
         BitConverter.GetBytes(size).CopyTo(data, OffsetDataLength);
         ushort x = (ushort)(X / 64);
         BitConverter.GetBytes(x).CopyTo(data, OffsetX);
         byte y = (byte)(Y / 64);
         data[OffsetY] = y;
         ushort z = (ushort)(Z / 64);
         BitConverter.GetBytes(z).CopyTo(data, OffsetZ);

         // set as not changed
         IsChanged = false;

         // write other segments
         int offset = OffsetSegmentData;
         for (int i = 0; i < GetSegmentCount(); i++)
         {
            Segment segment = GetSegment<Segment>(ref i);
            segment.Serialize(ref data, ref offset);
         }

         return data;
      }

      /// <summary>
      /// Removes all references held by this area.
      /// </summary>
      public virtual void Unload()
      {
         // empty
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
