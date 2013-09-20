using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.DataStructures
{
   /// <summary>
   /// Unlike the binary storage structures this can store other values into
   /// itself that are up to 16 bits long. The total size can't exceed 32 bits.
   /// </summary>
   public sealed class DataInInt
   {
      #region Fields
      private int _Store;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a new instance.
      /// </summary>
      public DataInInt()
      {
         // empty
      }
      #endregion

      /// <summary>
      /// Erases a part of the store.
      /// </summary>
      /// <param name="index">Index</param>
      /// <param name="length">Length</param>
      public void Erase(int index, int length)
      {
         for (int i = 0; i < length; i++)
         {
            _Store &= ~(1 << (index + i));
         }
      }

      /// <summary>
      /// Gets a bool value.
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>Bool value on given index</returns>
      public bool GetBool(int index)
      {
         return ((_Store & (1 << index)) != 0);
      }

      /// <summary>
      /// Gets a byte value.
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>Byte value on given index</returns>
      public byte GetByte(int index)
      {
         return (byte)(_Store >> index);
      }

      /// <summary>
      /// Gets a ushort value.
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>UShort value on given index</returns>
      public ushort GetUShort(int index)
      {
         return (ushort)(_Store >> index);
      }

      /// <summary>
      /// Store a bool value.
      /// </summary>
      /// <param name="value">Value</param>
      /// <param name="index">Index</param>
      public void StoreBool(bool value, int index)
      {
         Erase(index, 1);

         if (value)
         {
            _Store |= 1 << index;
         }
      }

      /// <summary>
      /// Stores a byte value.
      /// </summary>
      /// <param name="value">Value</param>
      /// <param name="index">Index</param>
      public void StoreByte(byte value, int index)
      {
         Erase(index, 8);

         _Store += (int)value << index;
      }

      /// <summary>
      /// Stores a ushort value.
      /// </summary>
      /// <param name="value">Value</param>
      /// <param name="index">Index</param>
      public void StoreUShort(ushort value, int index)
      {
         Erase(index, 16);

         _Store += (int)value << index;
      }
   }
}
