using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Exceptions;

namespace Qbes.Common.Logic.DataStructures
{
   /// <summary>
   /// Instance of these specialized classes contain a single byte that is
   /// presented as a bit array.
   /// </summary>
   public sealed class BinaryArrayInByte
   {
      #region Constants
      /// <summary>
      /// Gets the maximum array length that can be stored.
      /// </summary>
      public const int LengthLimit = 8;
      #endregion

      #region Fields
      private byte _Array;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default instance with all values set to false.
      /// </summary>
      public BinaryArrayInByte()
      {
         // empty array
      }

      /// <summary>
      /// Creates an instance with predefined values.
      /// </summary>
      /// <param name="initialData">Initial data array</param>
      public BinaryArrayInByte(bool[] initialData)
      {
         UpdateArray(initialData);
      }
      #endregion

      /// <summary>
      /// Gets or sets the bool value on a specified index.
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>Bool value on the specified index</returns>
      public bool this[int index]
      {
         get
         {
            return ((_Array & (1 << index)) != 0);
         }
         set
         {
            if (value)
            {
               _Array |= (byte)(1 << index);
            }
            else
            {
               _Array &= (byte)~(1 << index);
            }
         }
      }

      /// <summary>
      /// Gets the current byte value.
      /// </summary>
      /// <returns>Current value</returns>
      public byte GetValue()
      {
         return _Array;
      }

      /// <summary>
      /// Sets the held array value to 0.
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
               _Array |= (byte)(1 << i);
            }
         }
      }
   }
}
