using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// Cubes are the smallest pieces in the world. They are merged into blobs in
   /// order to reduce the resulting object count.
   /// </summary>
   public sealed class Cube
   {
      #region Constructors
      /// <summary>
      /// Creates a new cube with given coordinates of its front bottom left
      /// corner and material ID.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <param name="materialId">Material ID</param>
      public Cube(int x, int y, int z, ushort materialId)
      {
         X = x;
         Y = y;
         Z = z;
         MaterialId = materialId;
      }

      /// <summary>
      /// Creates a new cube with given coordinates of its front bottom left
      /// corner and material ID.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <param name="materialId">Material ID</param>
      public Cube(float x, float y, float z, ushort materialId)
         : this(Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(z), materialId)
      {
         // empty
      }
      #endregion

      [Obsolete("Doesn't consider client box creation")]
      internal Box CreateBox(Segment segment)
      {
         return new Box(this, segment);
      }

      /// <summary>
      /// Gets or sets (private) the material ID.
      /// </summary>
      public ushort MaterialId { get; private set; }

      /// <summary>
      /// Gets or sets (private) the X coordinate.
      /// </summary>
      public int X { get; private set; }

      /// <summary>
      /// Gets or sets (private) the X coordinate.
      /// </summary>
      public int Y { get; private set; }

      /// <summary>
      /// Gets or sets (private) the X coordinate.
      /// </summary>
      public int Z { get; private set; }
   }
}
