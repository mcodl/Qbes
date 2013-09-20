using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Qbes.Common.Logic.Extensions;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// Instances of this class are used to represent points or vectors in 3D
   /// world.
   /// </summary>
   /// <remarks>Big thanks to Miroslav Hédl for the line/triangle intersection
   /// code</remarks>
   public sealed class Point3D
   {
      #region Constants
      private const int OffsetX = 0;
      private const int OffsetY = 4;
      private const int OffsetZ = 8;
      /// <summary>
      /// The length in bytes of serialized point data.
      /// </summary>
      public const int SerializedSize = 12;
      #endregion

      #region Static fields
#if DIAG
      private static int _AngleCounter = 0;
      private static int _ArithmethicOperatorsCounter = 0;
      private static int _CloneCounter = 0;
      private static int _ComparativeOperatorsCounter = 0;
      private static int _DistanceCounter = 0;
      private static int _IntersectCounter = 0;
      private static int _RotationCounter = 0;
#endif
      private static readonly Point3D _One = new Point3D(1, 1, 1);
      private static readonly Point3D _Origin = new Point3D(0, 0, 0);
      private static readonly double _Pi = Math.PI / 180.0d;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default point at the origin.
      /// </summary>
      public Point3D()
      {
         // empty
      }

      /// <summary>
      /// Creates a new point based on given point.
      /// </summary>
      /// <param name="point">Base point</param>
      public Point3D(Point3D point)
      {
         X = point.X;
         Y = point.Y;
         Z = point.Z;

#if DIAG
         _CloneCounter++;
#endif
      }

      /// <summary>
      /// Creates a new point with given coordinates.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      public Point3D(float x, float y, float z)
      {
         X = x;
         Y = y;
         Z = z;
      }

      /// <summary>
      /// Creates a new point with given coordinates.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      public Point3D(double x, double y, double z)
      {
         X = Convert.ToSingle(x);
         Y = Convert.ToSingle(y);
         Z = Convert.ToSingle(z);
      }

      /// <summary>
      /// Creates a new point based on given point and coordinate shifts.
      /// </summary>
      /// <param name="point">Base point</param>
      /// <param name="shiftX">X coordinate shift</param>
      /// <param name="shiftY">Y coordinate shift</param>
      /// <param name="shiftZ">Z coordinate shift</param>
      public Point3D(Point3D point, float shiftX, float shiftY, float shiftZ)
         : this(point)
      {
         X += shiftX;
         Y += shiftY;
         Z += shiftZ;

#if DIAG
         _CloneCounter++;
#endif
      }
      #endregion

#if DIAG
      /// <summary>
      /// Gets the count of angle operations.
      /// </summary>
      public static int AngleCounter
      {
         get
         {
            return _AngleCounter;
         }
      }

      /// <summary>
      /// Gets the count of arithmethic operator operations.
      /// </summary>
      public static int ArithmethicOperatorsCounter
      {
         get
         {
            return _ArithmethicOperatorsCounter;
         }
      }

      /// <summary>
      /// Gets the count of clone constructor operations.
      /// </summary>
      public static int CloneCounter
      {
         get
         {
            return _CloneCounter;
         }
      }

      /// <summary>
      /// Gets the count of comparative operator operations.
      /// </summary>
      public static int ComparativeOperatorsCounter
      {
         get
         {
            return _ComparativeOperatorsCounter;
         }
      }
#endif

      /// <summary>
      /// Gets a cross product between two points.
      /// </summary>
      /// <param name="point1">Vector 1</param>
      /// <param name="point2">Vector 2</param>
      /// <returns>Cross product</returns>
      public static Point3D CrossProduct(Point3D point1, Point3D point2)
      {
         return new Point3D(point1.Y * point2.Z - point1.Z * point2.Y,
                            point1.Z * point2.X - point1.X * point2.Z,
                            point1.X * point2.Y - point1.Y * point2.X);
      }

#if DIAG
      /// <summary>
      /// Gets the distance operations counter.
      /// </summary>
      public static int DistanceCounter
      {
         get
         {
            return _DistanceCounter;
         }
      }
#endif

      /// <summary>
      /// Gets a dot product from this and given vector.
      /// </summary>
      /// <param name="other">Other vector</param>
      /// <returns>Dot product</returns>
      public float DotProduct(Point3D other)
      {
         return X * other.X + Y * other.Y + Z * other.Z;
      }

      /// <summary>
      /// Gets whether this object is the same as the given object.
      /// </summary>
      /// <param name="obj">Other object to check equality with</param>
      /// <returns>True if equal</returns>
      public override bool Equals(object obj)
      {
         if (object.ReferenceEquals(this, obj))
         {
            return true;
         }

         if (obj == null || !(obj is Point3D))
         {
            return false;
         }

         Point3D other = (Point3D)obj;
         return (this == other);
      }

      /// <summary>
      /// Gets an angle between two vectors.
      /// </summary>
      /// <param name="origin">Origin</param>
      /// <param name="vector1">Vector 1</param>
      /// <param name="vector2">Vector 2</param>
      /// <returns>Angle between two vectors</returns>
      public static double GetAngle(Point3D origin, Point3D vector1, Point3D vector2)
      {
         // this is extremely unlikely to happen so lets just ignore this case
         //if ((vector1.X == 0.0f && vector1.Y == 0.0f && vector1.Z == 0.0f) ||
         //    (vector2.X == 0.0f && vector2.Y == 0.0f && vector2.Z == 0.0f))
         //{
         //   // unable to calculate so return -1
         //   return -1;
         //}

         Point3D a = (vector1 - origin);
         a.Normalize();
         Point3D b = (vector2 - origin);
         b.Normalize();

#if DIAG
         _AngleCounter++;
#endif

         return Math.Acos(a.X * b.X + a.Y * b.Y + a.Z * b.Z) / Math.PI;
      }

      /// <summary>
      /// Gets an angle between two vectors.
      /// </summary>
      /// <param name="origin">Origin</param>
      /// <param name="vector1">Vector 1</param>
      /// <param name="vector2">Vector 2</param>
      /// <returns>Angle between two vectors which is not divided by
      /// Math.PI</returns>
      public static double GetAngleRadians(Point3D origin, Point3D vector1, Point3D vector2)
      {
         // this is extremely unlikely to happen so lets just ignore this case
         //if ((vector1.X == 0.0f && vector1.Y == 0.0f && vector1.Z == 0.0f) ||
         //    (vector2.X == 0.0f && vector2.Y == 0.0f && vector2.Z == 0.0f))
         //{
         //   // unable to calculate so return -1
         //   return -1;
         //}

         Point3D a = (vector1 - origin);
         a.Normalize();
         Point3D b = (vector2 - origin);
         b.Normalize();

#if DIAG
         _AngleCounter++;
#endif

         return Math.Acos(a.X * b.X + a.Y * b.Y + a.Z * b.Z);
      }

      /// <summary>
      /// Gets the distance from [0;0;0] origin.
      /// </summary>
      /// <returns>Distance from [0;0;0] origin</returns>
      public double GetDistance()
      {
#if DIAG
         _DistanceCounter++;
#endif

         return Math.Sqrt(X * X + Y * Y + Z * Z);
      }

      /// <summary>
      /// Gets the distance between this and given point.
      /// </summary>
      /// <param name="point2">Second point</param>
      /// <returns>Distance between this and given point</returns>
      public double GetDistance(Point3D point2)
      {
#if DIAG
         _DistanceCounter++;
#endif

         return Math.Sqrt(Square(X - point2.X) + Square(Y - point2.Y) + Square(Z - point2.Z));
      }

      /// <summary>
      /// Gets the distance square from [0;0;0] origin.
      /// </summary>
      /// <returns>Distance square from [0;0;0] origin</returns>
      public float GetDistanceSquare()
      {
#if DIAG
         _DistanceCounter++;
#endif

         return X * X + Y * Y + Z * Z;
      }

      /// <summary>
      /// Gets the distance square between this and given point.
      /// </summary>
      /// <param name="other">Other point</param>
      /// <returns>Distance square between this and given point</returns>
      public float GetDistanceSquare(Point3D other)
      {
#if DIAG
         _DistanceCounter++;
#endif

         return Square(X - other.X) + Square(Y - other.Y) + Square(Z - other.Z);
      }

      /// <summary>
      /// Gets the hash code.
      /// </summary>
      /// <returns>Hash code</returns>
      public override int GetHashCode()
      {
         return (X + Y + Z).GetHashCode();
      }

      /// <summary>
      /// Gets the normal vector of a given plane.
      /// </summary>
      /// <param name="plane">Plane defined by 3 points</param>
      /// <returns>Normal vector</returns>
      public static Point3D GetPlanesNormal(Tuple<Point3D, Point3D, Point3D> plane)
      {
         return Point3D.CrossProduct(plane.Item1 - plane.Item2, plane.Item1 - plane.Item3);
      }
      
      /// <summary>
      /// Gets the line fragment intersection with a rectangle (if any).
      /// </summary>
      /// <param name="rectangle">Rectangle defined by 4 points (either in
      /// clockwise or counterclockwise direction)</param>
      /// <param name="line">Line fragment defined by 2 points</param>
      /// <returns>First item = 0 --> no intersetion; first item = 1 --> one
      /// intersection in the second item; first item = 2 --> line is on the
      /// plane</returns>
      public static Tuple<int, Point3D> GetRectangleIntersection(Tuple<Point3D, Point3D, Point3D, Point3D> rectangle, Vector3D line)
      {
         var result = GetTriangleIntersection(Tuple.Create(rectangle.Item1, rectangle.Item2, rectangle.Item3), line);
         if (result.Item1 == 0)
         {
            result = GetTriangleIntersection(Tuple.Create(rectangle.Item1, rectangle.Item4, rectangle.Item3), line);
         }

         return result;
      }

      /// <summary>
      /// Creates a new point that is rotated around origin by given angles.
      /// </summary>
      /// <param name="leftRightAngle">Left/right rotation angle</param>
      /// <param name="upDownAngle">Up/down rotation angle</param>
      /// <returns>New point that is rotated around origin by given angles</returns>
      [Obsolete("Use Rotate(...) methods instead")]
      public Point3D GetRotated(float leftRightAngle, float upDownAngle)
      {
         return GetRotatedUpDown(ref upDownAngle).GetRotatedLeftRight(ref leftRightAngle);
      }

      /// <summary>
      /// Gets a point rotated around the Y axis by given angle.
      /// </summary>
      /// <param name="angle">Left/right rotation angle</param>
      /// <returns>Point rotated around the Y axis by given angle</returns>
      [Obsolete("Use Rotate(...) methods instead")]
      public Point3D GetRotatedLeftRight(ref float angle)
      {
         double cDegrees = angle * _Pi;
         double cosDegrees = Math.Cos(cDegrees);
         double sinDegrees = Math.Sin(cDegrees);

         double x = (X * cosDegrees) + (Z * sinDegrees);
         double z = (X * -sinDegrees) + (Z * cosDegrees);

         return new Point3D(x, Y, z);
      }

      /// <summary>
      /// Gets a point rotated around the X axis by given angle.
      /// </summary>
      /// <param name="angle">Up/down rotation angle</param>
      /// <returns>Point rotated around the X axis by given angle</returns>
      [Obsolete("Use Rotate(...) methods instead")]
      public Point3D GetRotatedUpDown(ref float angle)
      {
         double cDegrees = angle * _Pi;
         double cosDegrees = Math.Cos(cDegrees);
         double sinDegrees = Math.Sin(cDegrees);

         double y = (Y * cosDegrees) + (Z * sinDegrees);
         double z = (Y * -sinDegrees) + (Z * cosDegrees);

         return new Point3D(X, y, z);
      }

      /// <summary>
      /// Gets the line fragment intersection with a triangle (if any).
      /// </summary>
      /// <param name="triangle">Triangle defined by 3 points</param>
      /// <param name="line">Line fragment defined by 2 points</param>
      /// <returns>First item = 0 --> no intersetion; first item = 1 --> one
      /// intersection in the second item; first item = 2 --> line is on the
      /// plane</returns>
      public static Tuple<int, Point3D> GetTriangleIntersection(Tuple<Point3D, Point3D, Point3D> triangle, Vector3D line)
      {
#if DIAG
         _IntersectCounter++;
#endif

         Point3D vU1U2 = line.Point2 - line.Point1;
         Point3D vU1P1 = triangle.Item2 - line.Point1;
         Point3D norm = GetPlanesNormal(triangle);

         float lenNormU1U2 = norm.DotProduct(vU1U2);
         float lenNormU1P1 = norm.DotProduct(vU1P1);

         // normala dotProduct usecka  =>  kdyz je to 0, jsou kolme, normala je kolma na plochu, tedy usecka a plocha jsou paralelni
         if (lenNormU1U2.EqEps(0))
         {
            if (lenNormU1P1.EqEps(0))
            {
               // usecka lezi v rovine definovane trojuhelnikem.... detekuj si prunik usecky strojuhelnikem sam :-)
               return Tuple.Create<int, Point3D>(2, null);
            }
            else
            {
               // usecka je paralelni s rovinou, takze nic se nikdy neprotne
               return Tuple.Create<int, Point3D>(0, null);
            }
         }

         // 0 zacatek, 1 konec usecky... uzavreny interval [0;1] == na usecce :-)
         float position = lenNormU1P1 / lenNormU1U2;

         if (0 <= position && position <= 1)
         {
            // ha!, usecka protina rovinu definovanou trojuhelnikem.... ted jen zjistit jestli ten bod je v tom trojuhelniku
            // měla by platit rovnost pro všechny 4 body roviny:
            // NBx = ZjistiNormaluPlochuDefinovanouTremiBodyVole(plocha)
            // NBx.Item1.DotProduct( [ plocha.Item1 ...až plocha.Item3 a bodPruniku] ) == NBx.Item3
            // jestli ne, mám něco blbě, což by nebylo nic zvláštního :)
            Point3D intersection = line.Point1 + vU1U2 * position;
            if (IsPointInTriangle(intersection, triangle))
            {
               return Tuple.Create(1, intersection);
            }
            return Tuple.Create<int, Point3D>(0, null);
         }
         // usecka NEprotina plochu, ale kdybys ji prodlouzil (kdyby to byla primka), tak by rovinu protnula v bode (usecka.Item1 + vU1U2 * pozice)
         return Tuple.Create<int, Point3D>(0, null);
      }

      /// <summary>
      /// Initializes this point from serialized data.
      /// </summary>
      /// <param name="data">Array with serialized data</param>
      /// <param name="offset">Offset</param>
      public void InitializeFromByteArray(ref byte[] data, int offset)
      {
         X = BitConverter.ToSingle(data, offset + OffsetX);
         Y = BitConverter.ToSingle(data, offset + OffsetY);
         Z = BitConverter.ToSingle(data, offset + OffsetZ);
      }

#if DIAG
      /// <summary>
      /// Gets the intersection operations counter.
      /// </summary>
      public static int IntersectCounter
      {
         get
         {
            return _IntersectCounter;
         }
      }
#endif

      /// <summary>
      /// Gets whether a given point is inside a triangle
      /// </summary>
      /// <param name="p">Point</param>
      /// <param name="triangle">Triangle defined by 3 points</param>
      /// <returns>True if the point is inside given triangle</returns>
      public static bool IsPointInTriangle(Point3D p, Tuple<Point3D, Point3D, Point3D> triangle)
      {
         // taken from http://www.blackpawn.com/texts/pointinpoly/default.html  

         // Compute vectors
         Point3D a = triangle.Item1;
         Point3D b = triangle.Item2;
         Point3D c = triangle.Item3;

         Point3D v0 = c - a;
         Point3D v1 = b - a;
         Point3D v2 = p - a;

         // Compute dot products
         float dot00 = v0.DotProduct(v0);
         float dot01 = v0.DotProduct(v1);
         float dot02 = v0.DotProduct(v2);
         float dot11 = v1.DotProduct(v1);
         float dot12 = v1.DotProduct(v2);

         // Compute barycentric coordinates
         double invDenom = 1F / (dot00 * dot11 - dot01 * dot01);
         double u = (dot11 * dot02 - dot01 * dot12) * invDenom;
         double v = (dot00 * dot12 - dot01 * dot02) * invDenom;

         // Check if point is in triangle
         return (u >= 0) && (v >= 0) && (u + v <= 1);
      }

      private void Normalize()
      {
         float size = Convert.ToSingle(GetDistance());

         X /= size;
         Y /= size;
         Z /= size;
      }

#if DIAG
      /// <summary>
      /// Resets the angle operations counter to zero.
      /// </summary>
      public static void ResetOperationCounters()
      {
         _AngleCounter = 0;
         _ArithmethicOperatorsCounter = 0;
         _CloneCounter = 0;
         _ComparativeOperatorsCounter = 0;
         _DistanceCounter = 0;
         _IntersectCounter = 0;
         _RotationCounter = 0;
      }
#endif

      /// <summary>
      /// Rotates the point.
      /// </summary>
      /// <param name="leftRightAngle">Left/right rotation angle</param>
      /// <param name="upDownAngle">Up/down rotation angle</param>
      public void Rotate(float leftRightAngle, float upDownAngle)
      {
         Rotate(_Origin, ref leftRightAngle, ref upDownAngle);
      }

      /// <summary>
      /// Rotates the point.
      /// </summary>
      /// <param name="leftRightAngle">Left/right rotation angle</param>
      /// <param name="upDownAngle">Up/down rotation angle</param>
      public void Rotate(ref float leftRightAngle, ref float upDownAngle)
      {
         Rotate(_Origin, ref leftRightAngle, ref upDownAngle);
      }

      /// <summary>
      /// Rotates the point around given origin by given angles.
      /// </summary>
      /// <param name="origin">Origin point to rotate around</param>
      /// <param name="leftRightAngle">Left/right rotation angle</param>
      /// <param name="upDownAngle">Up/down rotation angle</param>
      public void Rotate(Point3D origin, float leftRightAngle, float upDownAngle)
      {
         Rotate(origin, ref leftRightAngle, ref upDownAngle);
      }

      /// <summary>
      /// Rotates the point around given origin by given angles.
      /// </summary>
      /// <param name="origin">Origin point to rotate around</param>
      /// <param name="leftRightAngle">Left/right rotation angle</param>
      /// <param name="upDownAngle">Up/down rotation angle</param>
      public void Rotate(Point3D origin, ref float leftRightAngle,
                         ref float upDownAngle)
      {
#if DIAG
         _RotationCounter++;
#endif

         double x = (X -= origin.X);
         double y = (Y -= origin.Y);
         double z = (Z -= origin.Z);

         double upDownDegrees = upDownAngle * _Pi;
         double upDownCosDegrees = Math.Cos(upDownDegrees);
         double upDownSinDegrees = Math.Sin(upDownDegrees);

         double leftRightDegrees = leftRightAngle * _Pi;
         double leftRightCosDegrees = Math.Cos(leftRightDegrees);
         double leftRightSinDegrees = Math.Sin(leftRightDegrees);

         y = (Y * upDownCosDegrees) + (Z * upDownSinDegrees);
         z = (Y * -upDownSinDegrees) + (Z * upDownCosDegrees);
         x = (X * leftRightCosDegrees) + (z * leftRightSinDegrees);
         z = (X * -leftRightSinDegrees) + (z * leftRightCosDegrees);

         X = Convert.ToSingle(x) + origin.X;
         Y = Convert.ToSingle(y) + origin.Y;
         Z = Convert.ToSingle(z) + origin.Z;
      }

#if DIAG
      /// <summary>
      /// Gets the rotation operations counter.
      /// </summary>
      public static int RotationCounter
      {
         get
         {
            return _RotationCounter;
         }
      }
#endif

      /// <summary>
      /// Serializes point data into given byte array.
      /// </summary>
      /// <param name="data">Target byte array</param>
      /// <param name="offset">Current cursor position</param>
      public void Serialize(ref byte[] data, int offset)
      {
         BitConverter.GetBytes(X).CopyTo(data, offset + OffsetX);
         BitConverter.GetBytes(Y).CopyTo(data, offset + OffsetY);
         BitConverter.GetBytes(Z).CopyTo(data, offset + OffsetZ);
      }

      /// <summary>
      /// Sets this point's coordinates to the given point's coordinates.
      /// </summary>
      /// <param name="point">Base point</param>
      public void Set(Point3D point)
      {
         X = point.X;
         Y = point.Y;
         Z = point.Z;
      }

      /// <summary>
      /// Sets this point's coordinates to the given coordinates.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      public void Set(float x, float y, float z)
      {
         X = x;
         Y = y;
         Z = z;
      }

      /// <summary>
      /// Sets this point's coordinates to the given point's coordinates and
      /// shift.
      /// </summary>
      /// <param name="point">Base point</param>
      /// <param name="shiftX">X shift</param>
      /// <param name="shiftY">Y shift</param>
      /// <param name="shiftZ">Z shift</param>
      public void Set(Point3D point, float shiftX, float shiftY, float shiftZ)
      {
         Set(point.X + shiftX, point.Y + shiftY, point.Z + shiftZ);
      }

      private static float Square(float x)
      {
         return x * x;
      }

      /// <summary>
      /// Gets a string representation of the point's coordinates.
      /// </summary>
      /// <returns>String with coordinates</returns>
      public override string ToString()
      {
         return "[ " + X + " ; " + Y + " ; " + Z + " ]";

      }

      /// <summary>
      /// Gets or sets the X coordinate.
      /// </summary>
      [XmlAttribute("x")]
      public float X { get; set; }

      /// <summary>
      /// Gets or sets the Y coordinate.
      /// </summary>
      [XmlAttribute("y")]
      public float Y { get; set; }

      /// <summary>
      /// Gets or sets the Z coordinate.
      /// </summary>
      [XmlAttribute("z")]
      public float Z { get; set; }

      #region Operators
      /// <summary>
      /// Creates a new point by adding point2 to point1.
      /// </summary>
      /// <param name="point1">Point 1</param>
      /// <param name="point2">Point 2</param>
      /// <returns>New point by adding point2 to point1</returns>
      public static Point3D operator +(Point3D point1, Point3D point2)
      {
#if DIAG
         _ArithmethicOperatorsCounter++;
#endif

         return new Point3D(point1.X + point2.X,
                            point1.Y + point2.Y,
                            point1.Z + point2.Z);
      }

      /// <summary>
      /// Creates a negated point from the given point.
      /// </summary>
      /// <param name="p">Point</param>
      /// <returns>Negated point</returns>
      public static Point3D operator -(Point3D p)
      {
#if DIAG
         _ArithmethicOperatorsCounter++;
#endif

         return new Point3D(-p.X, -p.Y, -p.Z);
      }

      /// <summary>
      /// Creates a new point by substracting point2 from point1.
      /// </summary>
      /// <param name="point1">Point 1</param>
      /// <param name="point2">Point 2</param>
      /// <returns>New point by substracting point2 from point1</returns>
      public static Point3D operator -(Point3D point1, Point3D point2)
      {
#if DIAG
         _ArithmethicOperatorsCounter++;
#endif

         return new Point3D(point1.X - point2.X,
                            point1.Y - point2.Y,
                            point1.Z - point2.Z);
      }

      /// <summary>
      /// Creates a new point by multiplying given point with a scalar.
      /// </summary>
      /// <param name="p">Point</param>
      /// <param name="scalar">Scalar value</param>
      /// <returns>Multiplied point</returns>
      public static Point3D operator *(Point3D p, float scalar)
      {
#if DIAG
         _ArithmethicOperatorsCounter++;
#endif

         return new Point3D(p.X * scalar, p.Y * scalar, p.Z * scalar);
      }

      /// <summary>
      /// Creates a new point by dividing given point with a scalar.
      /// </summary>
      /// <param name="p">Point</param>
      /// <param name="scalar">Scalar value</param>
      /// <returns>Divided point</returns>
      public static Point3D operator /(Point3D p, float scalar)
      {
#if DIAG
         _ArithmethicOperatorsCounter++;
#endif

         return new Point3D(p.X / scalar, p.Y / scalar, p.Z / scalar);
      }

      /// <summary>
      /// Comparison operator.
      /// </summary>
      /// <param name="point1">Point 1</param>
      /// <param name="point2">Point 2</param>
      /// <returns>True if equals</returns>
      public static bool operator ==(Point3D point1, Point3D point2)
      {
#if DIAG
         _ComparativeOperatorsCounter++;
#endif

         if (object.ReferenceEquals(point1, null))
         {
            return object.ReferenceEquals(point2, null);
         }
         else if (object.ReferenceEquals(point2, null))
         {
            return object.ReferenceEquals(point1, null);
         }

         return (point1.X == point2.X && point1.Y == point2.Y && point1.Z == point2.Z);
      }

      /// <summary>
      /// Comparison operator.
      /// </summary>
      /// <param name="point1">Point 1</param>
      /// <param name="point2">Point 2</param>
      /// <returns>True if not equals</returns>
      public static bool operator !=(Point3D point1, Point3D point2)
      {
#if DIAG
         _ComparativeOperatorsCounter++;
#endif

         return !(point1 == point2);
      }
      #endregion
   }
}
