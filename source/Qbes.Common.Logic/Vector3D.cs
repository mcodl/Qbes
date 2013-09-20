using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// Vector3D class is used to represent vectors in 3D space using two points.
   /// </summary>
   public sealed class Vector3D
   {
      #region Constructors
      /// <summary>
      /// Creates a default vector at origin with directions [1; 0; 0].
      /// </summary>
      public Vector3D()
         : this(new Point3D(), new Point3D(1.0f, 0.0f, 0.0f))
      {
         // empty
      }

      /// <summary>
      /// Creates a vector using given points.
      /// </summary>
      /// <param name="point1">First point</param>
      /// <param name="point2">Second point</param>
      public Vector3D(Point3D point1, Point3D point2)
      {
         Point1 = point1;
         Point2 = point2;
      }
      #endregion

      /// <summary>
      /// Gets or sets the first point.
      /// </summary>
      public Point3D Point1 { get; set; }

      /// <summary>
      /// Gets or sets the second point.
      /// </summary>
      public Point3D Point2 { get; set; }
   }
}
