using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Constants;

namespace Qbes.Common.Logic
{
   internal sealed class CollisionWall
   {
      #region Constants
      internal const float CheckDistanceSquare = 9.0f;
      #endregion

      #region Private fields
      private Box _Box;
      private CheckWallCollisionsDelegate _CheckDelegate;
      private int _Side;
      #endregion

      #region Constructors
      internal CollisionWall(Box box, int side)
      {
         _Box = box;
         _Side = side;

         switch (side)
         {
            case Sides.BackX:
               _CheckDelegate = CheckBackXCollisions;
               break;
            case Sides.BackZ:
               _CheckDelegate = CheckBackZCollisions;
               break;
            case Sides.FrontX:
               _CheckDelegate = CheckFrontXCollisions;
               break;
            case Sides.FrontZ:
               _CheckDelegate = CheckFrontZCollisions;
               break;
            default:
               throw new ArgumentException("Invalid side for wall collision object: " + side);
         }
      }
      #endregion

      private void CheckBackXCollisions(Vector3D moveVector,
                                        ref float halfSizeX, ref float halfSizeY,
                                        ref float[] data)
      {
         float x1, x2, y1, y2, z1, z2;
         GetPoints(halfSizeX, halfSizeY, out x1, out x2, out y1, out y2, out z1, out z2);

         if (moveVector.Point1.X < x2)
         {
            return;
         }

         if (_Box.CenterPoint.Z - moveVector.Point1.Z > WorldHelper.HalfSizeZ)
         {
            z1 -= WorldHelper.SizeZ;
            z2 -= WorldHelper.SizeZ;
         }
         else if (moveVector.Point1.Z - _Box.CenterPoint.Z > WorldHelper.HalfSizeZ)
         {
            z1 += WorldHelper.SizeZ;
            z2 += WorldHelper.SizeZ;
         }

         Tuple<int, Point3D> result = Point3D.GetRectangleIntersection(Tuple.Create(
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
               }
            }
         }
      }

      private void CheckBackZCollisions(Vector3D moveVector,
                                        ref float halfSizeX, ref float halfSizeY,
                                        ref float[] data)
      {
         float x1, x2, y1, y2, z1, z2;
         GetPoints(halfSizeX, halfSizeY, out x1, out x2, out y1, out y2, out z1, out z2);

         if (moveVector.Point1.Z < z2)
         {
            return;
         }

         if (_Box.CenterPoint.X - moveVector.Point1.X > WorldHelper.HalfSizeX)
         {
            x1 -= WorldHelper.SizeX;
            x2 -= WorldHelper.SizeX;
         }
         else if (moveVector.Point1.X - _Box.CenterPoint.X > WorldHelper.HalfSizeX)
         {
            x1 += WorldHelper.SizeX;
            x2 += WorldHelper.SizeX;
         }

         Tuple<int, Point3D> result = Point3D.GetRectangleIntersection(Tuple.Create(
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
                  data[1] = diff;
               }
            }
         }
      }

      private void CheckFrontXCollisions(Vector3D moveVector,
                                         ref float halfSizeX, ref float halfSizeY,
                                         ref float[] data)
      {
         float x1, x2, y1, y2, z1, z2;
         GetPoints(halfSizeX, halfSizeY, out x1, out x2, out y1, out y2, out z1, out z2);

         if (moveVector.Point1.X > x1)
         {
            return;
         }

         if (_Box.CenterPoint.Z - moveVector.Point1.Z > WorldHelper.HalfSizeZ)
         {
            z1 -= WorldHelper.SizeZ;
            z2 -= WorldHelper.SizeZ;
         }
         else if (moveVector.Point1.Z - _Box.CenterPoint.Z > WorldHelper.HalfSizeZ)
         {
            z1 += WorldHelper.SizeZ;
            z2 += WorldHelper.SizeZ;
         }

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
               }
            }
         }
      }

      private void CheckFrontZCollisions(Vector3D moveVector,
                                         ref float halfSizeX, ref float halfSizeY,
                                         ref float[] data)
      {
         float x1, x2, y1, y2, z1, z2;
         GetPoints(halfSizeX, halfSizeY, out x1, out x2, out y1, out y2, out z1, out z2);

         if (moveVector.Point1.Z > z1)
         {
            return;
         }

         if (_Box.CenterPoint.X - moveVector.Point1.X > WorldHelper.HalfSizeX)
         {
            x1 -= WorldHelper.SizeX;
            x2 -= WorldHelper.SizeX;
         }
         else if (moveVector.Point1.X - _Box.CenterPoint.X > WorldHelper.HalfSizeX)
         {
            x1 += WorldHelper.SizeX;
            x2 += WorldHelper.SizeX;
         }

         Tuple<int, Point3D> result = Point3D.GetRectangleIntersection(Tuple.Create(
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
                  data[1] = -diff;
               }
            }
         }
      }

      internal void CheckWallCollisions(Vector3D moveVector,
                                        ref float halfSizeX, ref float halfSizeY,
                                        ref float[] data)
      {
         if (moveVector.Point2.GetDistanceSquare(_Box.CenterPoint) > CheckDistanceSquare)
         {
            return;
         }

         _CheckDelegate(moveVector, ref halfSizeX, ref halfSizeY, ref data);
      }

      internal float GetBoxCenterPointDistanceSquare(Point3D point)
      {
         Point3D center = new Point3D(_Box.CenterPoint);

         switch (_Side)
         {
            case Sides.BackX:
               if (center.X - point.X > WorldHelper.HalfSizeX)
               {
                  center.X -= WorldHelper.SizeX;
               }
               break;
            case Sides.BackZ:
               if (center.Z - point.Z > WorldHelper.HalfSizeX)
               {
                  center.Z -= WorldHelper.SizeZ;
               }
               break;
            case Sides.FrontX:
               if (point.X - center.X > WorldHelper.HalfSizeX)
               {
                  center.X += WorldHelper.SizeX;
               }
               break;
            case Sides.FrontZ:
               if (point.Z - center.Z > WorldHelper.HalfSizeX)
               {
                  center.Z += WorldHelper.SizeZ;
               }
               break;
            default:
               throw new InvalidOperationException("Collision wall with invalid side: " + _Side);
         }

         return point.GetDistanceSquare(center);
      }

      private void GetPoints(float halfSizeX, float halfSizeY, out float x1, out float x2, out float y1, out float y2, out float z1, out float z2)
      {
         x1 = _Box.X1 - halfSizeX;
         x2 = _Box.X2 + halfSizeX;
         y1 = _Box.Y1 - halfSizeY;
         y2 = _Box.Y2 + halfSizeY;
         z1 = _Box.Z1 - halfSizeX;
         z2 = _Box.Z2 + halfSizeX;
      }

      internal int Side
      {
         get
         {
            return _Side;
         }
      }

      internal void Unload()
      {
         _Box = null;
      }
   }
}
