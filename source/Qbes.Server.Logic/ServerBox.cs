using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic;

namespace Qbes.Server.Logic
{
   /// <summary>
   /// ServerBox adds multiplayer related functionality to the Box base class.
   /// </summary>
   public sealed class ServerBox : Box
   {
      #region Constructors
      /// <summary>
      /// Creates a default server box instance.
      /// </summary>
      public ServerBox()
         : base()
      {

      }

      /// <summary>
      /// Creates a box from given cube and segment owner.
      /// </summary>
      /// <param name="cube">Base cube</param>
      /// <param name="segment">Segment owner</param>
      public ServerBox(Cube cube, ServerSegment segment)
         : base(cube, segment)
      {
         OnInitialized();
      }
      #endregion

      /// <summary>
      /// Runs any needed postprocessing when the server box is loaded.
      /// </summary>
      protected override void OnInitialized()
      {
         // empty
      }
   }
}
