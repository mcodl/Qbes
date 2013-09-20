using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Networking
{
   internal sealed class MultipartMessage : IComparable<MultipartMessage>
   {
      internal byte[] Data { get; set; }

      internal uint MessageID { get; set; }

      internal ushort PartCount { get; set; }

      internal ushort PartIndex { get; set; }

      public int CompareTo(MultipartMessage other)
      {
         return PartIndex.CompareTo(other.PartIndex);
      }
   }
}
