using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.DataStructures
{
   /// <summary>
   /// BinaryMatrix3D is a specialized class that stores a 3D matrix with 8x8x8
   /// dimensions.
   /// </summary>
   public sealed class BinaryMatrix3D
   {
      #region Fields
      private BinaryArrayInLong[] _Matrix = new BinaryArrayInLong[8];
      #endregion

      #region Constructors
      /// <summary>
      /// Creates an empty binary matrix.
      /// </summary>
      public BinaryMatrix3D()
      {
         for (int i = 0; i < 8; i++)
         {
            _Matrix[i] = new BinaryArrayInLong();
         }
      }

      /// <summary>
      /// Creates a matrix with given data
      /// </summary>
      /// <param name="initialData">Initial data in form of a 3D array</param>
      public BinaryMatrix3D(bool[][][] initialData)
         : this()
      {
         UpdateMatrix(initialData);
      }
      #endregion

      private static byte CalculatePosition(ref int y, ref int z)
      {
         return (byte)(y * 8 + z);
      }

      /// <summary>
      /// Gets the value at specified coordinates.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <returns>Value at specified coordinates</returns>
      public bool Get(int x, int y, int z)
      {
         return _Matrix[x][CalculatePosition(ref y, ref z)];
      }

      /// <summary>
      /// Gets the value at specified coordinates.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <returns>Value at specified coordinates</returns>
      public bool Get(ref int x, ref int y, ref int z)
      {
         return _Matrix[x][CalculatePosition(ref y, ref z)];
      }

      /// <summary>
      /// Resets the matrix to 0 value.
      /// </summary>
      public void Reset()
      {
         for (int i = 0; i < 8; i++)
         {
            _Matrix[i].Reset();
         }
      }

      /// <summary>
      /// Sets the value at specified coordinates.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <param name="newValue">New value</param>
      public void Set(int x, int y, int z, bool newValue)
      {
         _Matrix[x][CalculatePosition(ref y, ref z)] = newValue;
      }

      /// <summary>
      /// Updates the array with new data.
      /// </summary>
      /// <param name="newData">New data array</param>
      public void UpdateMatrix(bool[][][] newData)
      {
         Reset();

         for (int x = 0; x < 8; x++)
         {
            bool[] array = new bool[64];

            for (int y = 0; y < 8; y++)
            {
               for (int z = 0; z < 8; z++)
               {
                  if (newData[x][y][z])
                  {
                     byte position = CalculatePosition(ref y, ref z);
                     array[position] = true;
                  }
               }
            }

            _Matrix[x] = new BinaryArrayInLong(array);
         }
      }
   }
}
