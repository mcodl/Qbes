using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic;
using Qbes.Server.Logic.Exceptions;

namespace Qbes.Server.Logic
{
   /// <summary>
   /// ServerArea adds multiplayer related functionality to the Area base class.
   /// </summary>
   public sealed class ServerArea : Area
   {
      #region Private fields
      private List<ServerSegment> _Segments = new List<ServerSegment>();
      #endregion

      /// <summary>
      /// Adds a segment to this area.
      /// </summary>
      /// <typeparam name="TSegment">Segment type</typeparam>
      /// <param name="segment">Segment to add</param>
      public override void AddSegment<TSegment>(TSegment segment)
      {
         if (!(segment is ServerSegment))
         {
            throw new NotServerSegmentException();
         }

         _Segments.Add(segment as ServerSegment);
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

      /// <summary>
      /// Removes all references held by this area.
      /// </summary>
      public override void Unload()
      {
         base.Unload();

         _Segments.Clear();
         _Segments = null;
      }
   }
}
