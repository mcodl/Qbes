using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic;

namespace Qbes.Client.Logic.Extensions
{
   internal static class PlayerExtensions
   {
      #region Constants
      private const float HighlightEliminationDistance = 512.0f;
      #endregion

      internal static float[] GetHighlightCoordinates(this Player player)
      {
         float[] result = new float[12];

         Segment playerSegment = player.CurrentSegment;
         List<Box> checkBoxes = new List<Box>(playerSegment.GetBoxesSynchronized());

         for (int x = playerSegment.X - 8; x <= playerSegment.X + 8; x += 8)
         {
            for (int y = playerSegment.Y - 8; y <= playerSegment.Y + 8; y += 8)
            {
               for (int z = playerSegment.Z - 8; z <= playerSegment.Z + 8; z += 8)
               {
                  if (x == playerSegment.X && y == playerSegment.Y && z == playerSegment.Z)
                  {
                     continue;
                  }

                  checkBoxes.AddRange(ClientWorldManager.Instance.GetSegment(ref x, ref y, ref z).GetBoxesSynchronized());
               }
            }
         }

         Point3D intersection = new Point3D(float.MaxValue, float.MaxValue, float.MaxValue);
         foreach (Box box in checkBoxes)
         {
            if (box.CenterPoint.GetDistanceSquare(player.Location) > HighlightEliminationDistance)
            {
               // too far to even consider
               continue;
            }

            // TODO: calculate interesection
         }

         return result;
      }
   }
}
