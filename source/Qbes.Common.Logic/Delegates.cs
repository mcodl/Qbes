using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Networking;

namespace Qbes.Common.Logic
{
   internal delegate void CheckWallCollisionsDelegate(Vector3D moveVector, ref float halfSizeX, ref float halfSizeY, ref float[] data);

   /// <summary>
   /// This delegate is used to inform receiver that incomming multipart message
   /// has been assembled and is ready to be processed.
   /// </summary>
   /// <param name="connection">Connection object</param>
   /// <param name="messageCode">Message code</param>
   /// <param name="data">Message data</param>
   public delegate void CompletedReceivedMultipartMessage(Connection connection, ref byte messageCode, ref byte[] data);

   /// <summary>
   /// This delegate is used to retrieve entities near a specific point.
   /// </summary>
   /// <param name="point">Point of interest</param>
   /// <param name="distanceSquare">Distance squared</param>
   /// <returns>List with entities near a specific point within given distance
   /// square</returns>
   public delegate List<Entity> GetEntitiesDelegate(Point3D point, float distanceSquare);

   /// <summary>
   /// This delegate is used to retrieve pooled box.
   /// </summary>
   /// <returns>Box from the pool or null</returns>
   public delegate Box GetPooledBoxDelegate();

   /// <summary>
   /// This delegate is used to retrieve pooled segment.
   /// </summary>
   /// <returns>Segment from the pool or null</returns>
   public delegate Segment GetPooledSegmentDelegate();

   /// <summary>
   /// This delegate is used to retrieve segment by key.
   /// </summary>
   /// <param name="key">Segment key</param>
   /// <returns>Segment by key</returns>
   public delegate Segment GetSegmentDelegate(ref int key);

   [Obsolete]
   internal delegate Segment[] GetSegmentNeighboursDelegate(Segment segment);

   internal delegate Segment[] GetSegmentNeighboursForCollisionsDelegate(Segment segment, Point3D location);
}
