using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qbes.Common.Logic.Exceptions;

namespace Qbes.Common.Logic.DataStructures
{
   /// <summary>
   /// This specialized class uses a long variable to store an 8x8 binary
   /// matrix.
   /// </summary>
   public sealed class BinaryArrayInLong
   {
      #region Constants
      /// <summary>
      /// Gets the maximum array length that can be stored.
      /// </summary>
      public const int LengthLimit = 64;
      private const long LongOne = 1;
      #endregion

      #region Fields
      private long _Array;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default instance with all values set to false.
      /// </summary>
      public BinaryArrayInLong()
      {
         // empty array
      }

      /// <summary>
      /// Creates an instance with predefined values.
      /// </summary>
      /// <param name="initialData">Initial data array</param>
      public BinaryArrayInLong(bool[] initialData)
      {
         UpdateArray(initialData);
      }
      #endregion

      /// <summary>
      /// Gets or set the bool value on a specified index.
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>Bool value on the specified index</returns>
      public bool this[int index]
      {
         get
         {
            return ((_Array & (LongOne << index)) != 0);
         }
         set
         {
            if (value)
            {
               _Array |= LongOne << index;
            }
            else
            {
               _Array &= (long)~(LongOne << index);
            }
         }
      }

      /// <summary>
      /// Gets current long value.
      /// </summary>
      /// <returns>Current value</returns>
      public long GetValue()
      {
         return _Array;
      }

      /// <summary>
      /// Resets the array to 0 value.
      /// </summary>
      public void Reset()
      {
         _Array = 0;
      }

      /// <summary>
      /// Updates the array with new data.
      /// </summary>
      /// <param name="newData">New data array</param>
      public void UpdateArray(bool[] newData)
      {
         if (newData.Length > LengthLimit)
         {
            throw new LengthOutOfRangeException(newData.Length, LengthLimit);
         }

         _Array = 0;

         for (int i = 0; i < newData.Length; i++)
         {
            if (newData[i])
            {
               _Array += LongOne << i;
            }
         }
      }
   }
}
