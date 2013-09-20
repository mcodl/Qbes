using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Constants;
using Qbes.Common.Logic.DataStructures;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// Boxes are wrapping cubes of the same material (if they can be grouped)
   /// into larger objects.
   /// </summary>
   public class Box
   {
      #region Constants
      private const int BinaryDataFlagIndex = 0;
      private const int BinaryDataMaterialIdIndex = 8;
      private const int BinaryDataMergedIndex = 24;
      private const int OffsetCoords = 0;
      private const int OffsetFlag = 3;
      private const int OffsetMaterial = 4;
      //private const int OldOffsetFlag = 8;
      //private const int OldOffsetMaterial = 6;
      private const int OffsetX = 0;
      private const int OffsetY = 1;
      private const int OffsetZ = 2;
      //private const int OffsetX1 = 0;
      //private const int OffsetX2 = 1;
      //private const int OffsetY1 = 2;
      //private const int OffsetY2 = 3;
      //private const int OffsetZ1 = 4;
      //private const int OffsetZ2 = 5;
      ///// <summary>
      ///// The length in bytes of serialized box data.
      ///// </summary>
      //public const int OldSerializedSize = 9;
      /// <summary>
      /// The length in bytes of serialized box data.
      /// </summary>
      public const int SerializedSize = 6;
      private const int SizesXIndex = 0;
      private const int SizesYIndex = 8;
      private const int SizesZIndex = 16;
      private const float StairWalkSize = 0.3f;
      #endregion

      #region Static fields
      private static readonly Box _FloorBox = new Box()
      {
         X1 = -65536,
         X2 = 65536,
         Y1 = -1,
         Y2 = 0,
         Z1 = -65536,
         Z2 = 65536,
      };
      #endregion

      #region Fields
      private DataInInt _BinaryData = new DataInInt();
      private Point3D _CenterPoint;
      private DataInInt _Sizes = new DataInInt();
      private float _X1, _X2, _Y1, _Y2, _Z1, _Z2;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default box instance.
      /// </summary>
      public Box()
      {
         // empty
      }

      /// <summary>
      /// Creates a box from given cube and segment owner.
      /// </summary>
      /// <param name="cube">Base cube</param>
      /// <param name="segment">Segment owner</param>
      public Box(Cube cube, Segment segment)
      {
         MaterialId = cube.MaterialId;
         Segment = segment;
         X1 = cube.X;
         X2 = cube.X + 1;
         Y1 = cube.Y;
         Y2 = cube.Y + 1;
         Z1 = cube.Z;
         Z2 = cube.Z + 1;

         UpdateSidePoints();
      }
      #endregion

      /// <summary>
      /// Gets the center point.
      /// </summary>
      public Point3D CenterPoint
      {
         get
         {
            return _CenterPoint;
         }
      }

      private static void CheckIntersection(Vector3D line, Tuple<int, Point3D> intersect, int side, Intersection currentResult)
      {
         if (intersect.Item1 == 1)
         {
            float newDistance = line.Point1.GetDistanceSquare(intersect.Item2);
            if (newDistance < currentResult.Distance)
            {
               if (currentResult == null)
               {
                  currentResult = new Intersection();
               }

               currentResult.Distance = newDistance;
               currentResult.Side = side;
               currentResult.IntersectionPoint = intersect.Item2;
            }
         }
      }

      internal float CheckFloorCeilingCollisions(Vector3D moveVector,
                                                ref float halfSizeX, ref float halfSizeY,
                                                ref bool falling)
      {
         float x1 = X1 - halfSizeX;
         float x2 = X2 + halfSizeX;
         float z1 = Z1 - halfSizeX;
         float z2 = Z2 + halfSizeX;

         // check floor
         float y = Y2 + halfSizeY;
         Tuple<int, Point3D> result = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(x1, y, z1),
            new Point3D(x2, y, z1),
            new Point3D(x2, y, z2),
            new Point3D(x1, y, z2)),
            moveVector);
         if (result.Item1 == 1)
         {
            // interesects and less than half is inside the box
            float diff = result.Item2.Y - moveVector.Point2.Y;
            if (diff > ErrorTolerances.FloatEpsilon && diff < halfSizeY)
            {
               falling = false;
               return diff;
            }
         }
         else if (result.Item1 == 0)
         {
            // check ceiling
            y = Y1 - halfSizeY;
            result = Point3D.GetRectangleIntersection(Tuple.Create(
               new Point3D(x1, y, z1),
               new Point3D(x2, y, z1),
               new Point3D(x2, y, z2),
               new Point3D(x1, y, z2)),
               moveVector);
            if (result.Item1 == 1)
            {
               float diff = moveVector.Point2.Y - result.Item2.Y;
               if (diff > ErrorTolerances.FloatEpsilon && diff < halfSizeY)
               {
                  // intersects and less than half is inside the box
                  return -diff;
               }
            }
         }

         return 0;
      }

      internal void CheckSuffocation(Point3D location, ref float halfSizeX,
                                     ref float halfSizeY, ref bool _Suffocating)
      {
         if (_Suffocating)
         {
            // already suffocating
            return;
         }
         else if (location.X > X1 - halfSizeX && location.X < X2 + halfSizeX &&
                  location.Y > Y1 - halfSizeY && location.Y < Y2 + halfSizeY &&
                  location.Z > Z1 - halfSizeX && location.Z < Z2 + halfSizeX)
         {
            // at least part of the entity is in terrain and thus being suffocated
            _Suffocating = true;
         }
      }

      [Obsolete("Replaced with CollisionWall class")]
      internal void CheckWallCollisions(Vector3D moveVector,
                                        ref float halfSizeX, ref float halfSizeY,
                                        ref float[] data)
      {
         float x1 = X1 - halfSizeX;
         float x2 = X2 + halfSizeX;
         float y1 = Y1 - halfSizeY;
         float y2 = Y2 + halfSizeY;
         float z1 = Z1 - halfSizeX;
         float z2 = Z2 + halfSizeX;

         // check front X wall
         Tuple<int, Point3D> result = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(x1, y1, z1),
            new Point3D(x1, y1, z2),
            new Point3D(x1, y2, z2),
            new Point3D(x1, y2, z1)),
            moveVector);
         if (result.Item1 == 1)
         {
            float dist = result.Item2.GetDistanceSquare(moveVector.Point1);
            if (dist < data[0])
            {
               float diff = moveVector.Point2.X - result.Item2.X;
               if (diff > ErrorTolerances.FloatEpsilon)
               {
                  data[0] = dist;
                  data[1] = -diff;
                  data[2] = 0;
               }
            }
         }

         // check back X wall
         result = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(x2, y1, z1),
            new Point3D(x2, y1, z2),
            new Point3D(x2, y2, z2),
            new Point3D(x2, y2, z1)),
            moveVector);
         if (result.Item1 == 1)
         {
            float dist = result.Item2.GetDistanceSquare(moveVector.Point1);
            if (dist < data[0])
            {
               float diff = result.Item2.X - moveVector.Point2.X;
               if (diff > ErrorTolerances.FloatEpsilon)
               {
                  data[0] = dist;
                  data[1] = diff;
                  data[2] = 0;
               }
            }
         }

         // check front Z wall
         result = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(x1, y1, z1),
            new Point3D(x1, y2, z1),
            new Point3D(x2, y2, z1),
            new Point3D(x2, y1, z1)),
            moveVector);
         if (result.Item1 == 1)
         {
            float dist = result.Item2.GetDistanceSquare(moveVector.Point1);
            if (dist < data[0])
            {
               float diff = moveVector.Point2.Z - result.Item2.Z;
               if (diff > ErrorTolerances.FloatEpsilon)
               {
                  data[0] = dist;
                  data[1] = 0;
                  data[2] = -diff;
               }
            }
         }

         // check back Z wall
         result = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(x1, y1, z2),
            new Point3D(x1, y2, z2),
            new Point3D(x2, y2, z2),
            new Point3D(x2, y1, z2)),
            moveVector);
         if (result.Item1 == 1)
         {
            float dist = result.Item2.GetDistanceSquare(moveVector.Point1);
            if (dist < data[0])
            {
               float diff = result.Item2.Z - moveVector.Point2.Z;
               if (diff > ErrorTolerances.FloatEpsilon)
               {
                  data[0] = dist;
                  data[1] = 0;
                  data[2] = diff;
               }
            }
         }
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

         if (obj == null || !(obj is Box))
         {
            return false;
         }

         Box other = (Box)obj;

         return (X1 == other.X1 && X2 == other.X2 &&
                 Y1 == other.Y1 && Y2 == other.Y2 &&
                 Z1 == other.Z1 && Z2 == other.Z2 &&
                 MaterialId == other.MaterialId);
      }

      /// <summary>
      /// Gets or sets the box flag.
      /// </summary>
      public byte Flag
      {
         get
         {
            return _BinaryData.GetByte(BinaryDataFlagIndex);
         }
         private set
         {
            _BinaryData.StoreByte(value, BinaryDataFlagIndex);
         }
      }

      internal static Box FloorBox
      {
         get
         {
            return _FloorBox;
         }
      }

      /// <summary>
      /// Gets the cube list that make this blob.
      /// </summary>
      /// <returns>Cube list that make this blob</returns>
      public List<Cube> GetCubesList()
      {
         List<Cube> result = new List<Cube>();

         for (float x = X1; x < X2; x++)
         {
            for (float y = Y1; y < Y2; y++)
            {
               for (float z = Z1; z < Z2; z++)
               {
                  result.Add(new Cube(x, y, z, MaterialId));
               }
            }
         }

         return result;
      }

      /// <summary>
      /// Gets the hash code.
      /// </summary>
      /// <returns>Hash code</returns>
      public override int GetHashCode()
      {
         return (X1 + X2 + Y1 + Y2 + Z1 + Z2 + MaterialId).GetHashCode();
      }

      /// <summary>
      /// Gets the intersection of a given line with this box (if any).
      /// </summary>
      /// <param name="line">Line</param>
      /// <param name="shiftX">Shift X</param>
      /// <param name="shiftZ">Shift Z</param>
      /// <returns>Intersection with given line</returns>
      public Intersection GetIntersection(Vector3D line, int shiftX, int shiftZ)
      {
         Intersection result = new Intersection();

         // check front X
         Tuple<int, Point3D> intersect = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(X1 + shiftX, Y1, Z1 + shiftZ),
            new Point3D(X1 + shiftX, Y1, Z2 + shiftZ),
            new Point3D(X1 + shiftX, Y2, Z2 + shiftZ),
            new Point3D(X1 + shiftX, Y2, Z1 + shiftZ)),
            line);
         CheckIntersection(line, intersect, Sides.FrontX, result);

         // check back X
         intersect = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(X2 + shiftX, Y1, Z1 + shiftZ),
            new Point3D(X2 + shiftX, Y1, Z2 + shiftZ),
            new Point3D(X2 + shiftX, Y2, Z2 + shiftZ),
            new Point3D(X2 + shiftX, Y2, Z1 + shiftZ)),
            line);
         CheckIntersection(line, intersect, Sides.BackX, result);

         // check front Y
         intersect = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(X1 + shiftX, Y1, Z1 + shiftZ),
            new Point3D(X1 + shiftX, Y1, Z2 + shiftZ),
            new Point3D(X2 + shiftX, Y1, Z2 + shiftZ),
            new Point3D(X2 + shiftX, Y1, Z1 + shiftZ)),
            line);
         CheckIntersection(line, intersect, Sides.FrontY, result);

         // check back Y
         intersect = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(X1 + shiftX, Y2, Z1 + shiftZ),
            new Point3D(X1 + shiftX, Y2, Z2 + shiftZ),
            new Point3D(X2 + shiftX, Y2, Z2 + shiftZ),
            new Point3D(X2 + shiftX, Y2, Z1 + shiftZ)),
            line);
         CheckIntersection(line, intersect, Sides.BackY, result);

         // check front Z
         intersect = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(X1 + shiftX, Y1, Z1 + shiftZ),
            new Point3D(X1 + shiftX, Y2, Z1 + shiftZ),
            new Point3D(X2 + shiftX, Y2, Z1 + shiftZ),
            new Point3D(X2 + shiftX, Y1, Z1 + shiftZ)),
            line);
         CheckIntersection(line, intersect, Sides.FrontZ, result);

         // check back Z
         intersect = Point3D.GetRectangleIntersection(Tuple.Create(
            new Point3D(X1 + shiftX, Y1, Z2 + shiftZ),
            new Point3D(X1 + shiftX, Y2, Z2 + shiftZ),
            new Point3D(X2 + shiftX, Y2, Z2 + shiftZ),
            new Point3D(X2 + shiftX, Y1, Z2 + shiftZ)),
            line);
         CheckIntersection(line, intersect, Sides.BackZ, result);

         return result;
      }

      private Point3D GetSidePoint(int side)
      {
         Point3D helperPoint = new Point3D(CenterPoint);

         switch (side)
         {
            case Sides.FrontX:
               helperPoint.X = X1;
               break;
            case Sides.FrontY:
               helperPoint.Y = Y1;
               break;
            case Sides.FrontZ:
               helperPoint.Z = Z1;
               break;
            case Sides.BackX:
               helperPoint.X = X2;
               break;
            case Sides.BackY:
               helperPoint.Y = Y2;
               break;
            case Sides.BackZ:
               helperPoint.Z = Z2;
               break;
         }

         return helperPoint;
      }

      /// <summary>
      /// Initializes this box from serialized data.
      /// </summary>
      /// <param name="data">Array with serialized data</param>
      /// <param name="offset">Offset</param>
      /// <param name="segment">Segment reference</param>
      internal void InitializeFromByteArray(ref byte[] data, int offset, Segment segment)
      {
         // reset previous data
         Segment = segment;

         byte xData = data[OffsetX + offset];
         X1 = Segment.X + (xData % 8);
         X2 = X1 + (xData - xData % 8) / 8;
         byte yData = data[OffsetY + offset];
         Y1 = Segment.Y + (yData % 8);
         Y2 = Y1 + (yData - yData % 8) / 8;
         byte zData = data[OffsetZ + offset];
         Z1 = Segment.Z + (zData % 8);
         Z2 = Z1 + (zData - zData % 8) / 8;

         Flag = data[OffsetFlag + offset];
         MaterialId = data[OffsetMaterial + offset];

         UpdateSidePoints();

         OnInitialized();
      }

      /// <summary>
      /// Gets or sets (private) the material ID.
      /// </summary>
      public ushort MaterialId
      {
         get
         {
            return _BinaryData.GetUShort(BinaryDataMaterialIdIndex);
         }
         private set
         {
            _BinaryData.StoreUShort(value, BinaryDataMaterialIdIndex);
         }
      }

      private void Merge(Box b, ref int axis)
      {
         if ((MaterialId == b.MaterialId) &&
             ((axis == 0 && Z1 == b.Z1 && Z2 == b.Z2 && Y1 == b.Y1 && Y2 == b.Y2 && (X1 == b.X2 || X2 == b.X1)) ||
              (axis == 1 && X1 == b.X1 && X2 == b.X2 && Y1 == b.Y1 && Y2 == b.Y2 && (Z1 == b.Z2 || Z2 == b.Z1)) ||
              (axis == 2 && X1 == b.X1 && X2 == b.X2 && Z1 == b.Z1 && Z2 == b.Z2 && (Y1 == b.Y2 || Y2 == b.Y1))))
         {
            X1 = Math.Min(X1, b.X1);
            X2 = Math.Max(X2, b.X2);
            Y1 = Math.Min(Y1, b.Y1);
            Y2 = Math.Max(Y2, b.Y2);
            Z1 = Math.Min(Z1, b.Z1);
            Z2 = Math.Max(Z2, b.Z2);

            b.Merged = true;
         }
      }

      /// <summary>
      /// Merges given boxes into largest boxes possible.
      /// </summary>
      /// <param name="boxes">Boxes collection</param>
      /// <typeparam name="TBox">Box type</typeparam>
      public static void MergeBoxes<TBox>(List<TBox> boxes)
         where TBox : Box
      {
         // Run merging 3 times (once for all axis)
         for (int axis = 0; axis < 3; axis++)
         {
            MergeBoxes(boxes, ref axis);
         }

         // update side points
         boxes.ForEach(x =>
         {
            x.UpdateSidePoints();
         });
      }

      internal static void MergeBoxes<TBox>(List<TBox> boxes, int axis)
         where TBox : Box
      {
         MergeBoxes(boxes, ref axis);
      }

      internal static void MergeBoxes<TBox>(List<TBox> boxes, ref int axis)
         where TBox : Box
      {
         foreach (var currentBox in boxes)
         {
            if (currentBox.Merged)
            {
               continue;
            }

            foreach (var box in boxes)
            {
               if (currentBox == box ||
                   box.Merged ||
                   currentBox.MaterialId != box.MaterialId)
               {
                  continue;
               }

               currentBox.Merge(box, ref axis);
            }
         }

         // remove boxes that merged into others
         boxes.RemoveAll(x =>
         {
            return x.Merged;
         });
      }

      /// <summary>
      /// Gets or sets (private) whether this blob has been merged into another
      /// blob.
      /// </summary>
      public bool Merged
      {
         get
         {
            return _BinaryData.GetBool(BinaryDataMergedIndex);
         }
         private set
         {
            _BinaryData.StoreBool(value, BinaryDataMergedIndex);
         }
      }

      /// <summary>
      /// This method is called when the box initialization is complete
      /// </summary>
      protected virtual void OnInitialized()
      {
         // empty
      }

      /// <summary>
      /// Gets or sets (private) the wrapping segment.
      /// </summary>
      public Segment Segment { get; private set; }

      /// <summary>
      /// Serializes box data into given byte array.
      /// </summary>
      /// <param name="data">Target byte array</param>
      /// <param name="offset">Current cursor position</param>
      internal void Serialize(ref byte[] data, ref int offset)
      {
         data[OffsetX + offset] = (byte)(Convert.ToByte(X1 - Segment.X) + (Convert.ToByte(X2 - X1) * 8));
         data[OffsetY + offset] = (byte)(Convert.ToByte(Y1 - Segment.Y) + (Convert.ToByte(Y2 - Y1) * 8));
         data[OffsetZ + offset] = (byte)(Convert.ToByte(Z1 - Segment.Z) + (Convert.ToByte(Z2 - Z1) * 8));
         data[OffsetFlag + offset] = Flag;
         BitConverter.GetBytes(MaterialId).CopyTo(data, OffsetMaterial + offset);

         // move the offset
         offset += SerializedSize;
      }

      /// <summary>
      /// Gets or sets (private) the X size.
      /// </summary>
      protected byte SizeX
      {
         get
         {
            return _Sizes.GetByte(SizesXIndex);
         }
         private set
         {
            _Sizes.StoreByte(value, SizesXIndex);
         }
      }

      /// <summary>
      /// Gets or sets (private) the Y size.
      /// </summary>
      protected byte SizeY
      {
         get
         {
            return _Sizes.GetByte(SizesYIndex);
         }
         private set
         {
            _Sizes.StoreByte(value, SizesYIndex);
         }
      }

      /// <summary>
      /// Gets or sets (private) the Z size.
      /// </summary>
      protected byte SizeZ
      {
         get
         {
            return _Sizes.GetByte(SizesZIndex);
         }
         private set
         {
            _Sizes.StoreByte(value, SizesZIndex);
         }
      }

      /// <summary>
      /// Removes all references from this box.
      /// </summary>
      public void Unload()
      {
         _BinaryData = new DataInInt();
         _Sizes = new DataInInt();
         Segment = null;
      }

      /// <summary>
      /// Updates side and center points.
      /// </summary>
      protected void UpdateSidePoints()
      {
         if (X2 - X1 < 1 || X2 - X1 > 8 ||
             Y2 - Y1 < 1 || Y2 - Y1 > 8 ||
             Z2 - Z1 < 1 || Z2 - Z1 > 8)
         {
            Console.WriteLine("- Invalid box data: F = [ {0} ; {1} ; {2} ], B = [ {3} ; {4} ; {5} ] in segment [ {6} ; {7} ; {8} ]", X1, Y1, Z1, X2, Y2, Z2, Segment.X, Segment.Y, Segment.Z);
         }

         SizeX = Convert.ToByte(X2 - X1);
         SizeY = Convert.ToByte(Y2 - Y1);
         SizeZ = Convert.ToByte(Z2 - Z1);

         float centerX = X1 + (float)SizeX / 2.0f;
         float centerY = Y1 + (float)SizeY / 2.0f;
         float centerZ = Z1 + (float)SizeZ / 2.0f;

         _CenterPoint = new Point3D(centerX, centerY, centerZ);
      }

      /// <summary>
      /// Gets or sets (private) the X1 coordinate.
      /// </summary>
      public float X1
      {
         get
         {
            return _X1;
         }
         private set
         {
            _X1 = value;
         }
      }

      /// <summary>
      /// Gets or sets (private) the X2 coordinate.
      /// </summary>
      public float X2
      {
         get
         {
            return _X2;
         }
         private set
         {
            _X2 = value;
         }
      }

      /// <summary>
      /// Gets or sets (private) the Y1 coordinate.
      /// </summary>
      public float Y1
      {
         get
         {
            return _Y1;
         }
         private set
         {
            _Y1 = value;
         }
      }

      /// <summary>
      /// Gets or sets (private) the Y2 coordinate.
      /// </summary>
      public float Y2
      {
         get
         {
            return _Y2;
         }
         private set
         {
            _Y2 = value;
         }
      }

      /// <summary>
      /// Gets or sets (private) the Z1 coordinate.
      /// </summary>
      public float Z1
      {
         get
         {
            return _Z1;
         }
         private set
         {
            _Z1 = value;
         }
      }

      /// <summary>
      /// Gets or sets (private) the Z2 coordinate.
      /// </summary>
      public float Z2
      {
         get
         {
            return _Z2;
         }
         private set
         {
            _Z2 = value;
         }
      }
   }
}
