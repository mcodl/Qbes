using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Constants;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// Instances of this class carry the information about intersections.
   /// </summary>
   public sealed class Intersection
   {
      #region Constructors
      /// <summary>
      /// Creates a default intersection.
      /// </summary>
      public Intersection()
         : this(float.MaxValue, null, Sides.All)
      {
         // empty
      }

      /// <summary>
      /// Creates an intersection from given parameters.
      /// </summary>
      /// <param name="distance">Distance to intersection</param>
      /// <param name="intersectionPoint">Point of intersection</param>
      /// <param name="side">Side of intersection</param>
      public Intersection(float distance, Point3D intersectionPoint, int side)
      {
         Distance = distance;
         IntersectionPoint = intersectionPoint;
         Side = side;
      }
      #endregion

      /// <summary>
      /// Gets or sets the distance.
      /// </summary>
      public float Distance { get; set; }

      /// <summary>
      /// Gets or sets the intersection point.
      /// </summary>
      public Point3D IntersectionPoint { get; set; }

      /// <summary>
      /// Gets or sets the side of the intersection.
      /// </summary>
      public int Side { get; set; }
   }
}
