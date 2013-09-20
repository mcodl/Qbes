using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Configuration;
using Qbes.Common.Logic.Networking;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// WorldHelper static class contains information about the currently loaded
   /// world.
   /// </summary>
   public static class WorldHelper
   {
      #region
      /// <summary>
      /// Gets the name of the cache directory.
      /// </summary>
      public const string CacheDirName = "Cache";
      #endregion

      #region Static fields
      [Obsolete]
      private static AutoSaveConfiguration _AutoSaveConfiguration;
      private static string _CachePath = Path.Combine("..", "..", CacheDirName);
      private static IClientToServer _ClientToServerProvider;
      private static GetEntitiesDelegate _GetEntitiesDelegate;
      private static GetPooledBoxDelegate _GetPooledBoxDelegate;
      private static GetPooledSegmentDelegate _GetPooledSegmentDelegate;
      private static GetSegmentDelegate _GetSegmentDelegate;
      private static GetSegmentNeighboursForCollisionsDelegate _GetSegmentNeighboursDelegate;
      private static int _HalfSizeX = 256;
      private static int _HalfSizeZ = 256;
      [Obsolete]
      private static MessagingConfiguration _MessagingConfiguration;
      private static IServerToClient _ServerToClientProvider;
      private static int _SizeX = 512;
      private static int _SizeZ = 512;
      private static string _WorldPath;
      #endregion

      [Obsolete]
      internal static AutoSaveConfiguration AutoSaveConfiguration
      {
         get
         {
            return _AutoSaveConfiguration;
         }
      }

      /// <summary>
      /// Gets the cache path.
      /// </summary>
      public static string CachePath
      {
         get
         {
            return _CachePath;
         }
      }

      /// <summary>
      /// Gets the client to server messaging provider.
      /// </summary>
      public static IClientToServer ClientToServerProvider
      {
         get
         {
            return _ClientToServerProvider;
         }
      }

      internal static List<Entity> GetEntities(Point3D point, float distanceSquare)
      {
         return _GetEntitiesDelegate(point, distanceSquare);
      }

      internal static Box GetPooledBox()
      {
         return _GetPooledBoxDelegate();
      }

      internal static Segment GetPooledSegment()
      {
         return _GetPooledSegmentDelegate();
      }

      internal static Segment GetSegment(int key)
      {
         return _GetSegmentDelegate(ref key);
      }

      internal static Segment GetSegment(int x, int y, int z)
      {
         int key = Segment.GenerateKey(x, y, z);
         return _GetSegmentDelegate(ref key);
      }

      internal static Segment[] GetSegmentNeighboursForCollisions(Segment segment, Point3D location)
      {
         return _GetSegmentNeighboursDelegate(segment, location);
      }

      /// <summary>
      /// Gets the half X size.
      /// </summary>
      public static int HalfSizeX
      {
         get
         {
            return _HalfSizeX;
         }
      }

      /// <summary>
      /// Gets the half Z size.
      /// </summary>
      public static int HalfSizeZ
      {
         get
         {
            return _HalfSizeZ;
         }
      }

      [Obsolete]
      internal static MessagingConfiguration MessagingConfiguration
      {
         get
         {
            return _MessagingConfiguration;
         }
      }

      /// <summary>
      /// Gets the server to client messaging provider
      /// </summary>
      public static IServerToClient ServerToClientProvider
      {
         get
         {
            return _ServerToClientProvider;
         }
      }

      /// <summary>
      /// Sets the autosave configuration node.
      /// </summary>
      /// <param name="config">Autosave configuration node</param>
      [Obsolete]
      public static void SetAutoSaveConfiguration(AutoSaveConfiguration config)
      {
         _AutoSaveConfiguration = config;
      }

      internal static void SetGetEntitiesDelegate(GetEntitiesDelegate getEntitiesDelegate)
      {
         _GetEntitiesDelegate = getEntitiesDelegate;
      }

      /// <summary>
      /// Sets the GetPooledBoxDelegate.
      /// </summary>
      /// <param name="getPooledBoxDelegate">New GetPooledBoxDelegate</param>
      public static void SetGetPooledBoxDelegate(GetPooledBoxDelegate getPooledBoxDelegate)
      {
         _GetPooledBoxDelegate = getPooledBoxDelegate;
      }

      /// <summary>
      /// Sets the GetPooledSegmentDelegate.
      /// </summary>
      /// <param name="getPooledSegmentDelegate">New GetPooledSegmentDelegate</param>
      public static void SetGetPooledSegmentDelegate(GetPooledSegmentDelegate getPooledSegmentDelegate)
      {
         _GetPooledSegmentDelegate = getPooledSegmentDelegate;
      }

      /// <summary>
      /// Sets the GetSegmentDelegate.
      /// </summary>
      /// <param name="getSegmentDelegate">New GetSegmentDelegate</param>
      public static void SetGetSegmentDelegate(GetSegmentDelegate getSegmentDelegate)
      {
         _GetSegmentDelegate = getSegmentDelegate;
      }

      internal static void SetGetSegmentNeighboursDelegate(GetSegmentNeighboursForCollisionsDelegate getSegmentNeighboursDelegate)
      {
         _GetSegmentNeighboursDelegate = getSegmentNeighboursDelegate;
      }

      /// <summary>
      /// Sets the messaging configuration node.
      /// </summary>
      /// <param name="config">Messaging configuration</param>
      [Obsolete]
      public static void SetMessagingConfiguration(MessagingConfiguration config)
      {
         _MessagingConfiguration = config;
      }

      /// <summary>
      /// Sets the messaging providers.
      /// </summary>
      /// <param name="clientToServerProvider">New client to server messaging
      /// provider</param>
      /// <param name="serverToClientProvider">New server to client messaging
      /// provider</param>
      public static void SetMessagingProviders(IClientToServer clientToServerProvider, IServerToClient serverToClientProvider)
      {
         _ClientToServerProvider = clientToServerProvider;
         _ServerToClientProvider = serverToClientProvider;
      }

      internal static void SetWorldPath(string path)
      {
         _WorldPath = path;
      }

      internal static void SetWorldSize(int sizeX, int sizeZ)
      {
         _HalfSizeX = sizeX / 2;
         _HalfSizeZ = sizeZ / 2;
         _SizeX = sizeX;
         _SizeZ = sizeZ;
      }

      /// <summary>
      /// Gets the X size.
      /// </summary>
      public static int SizeX
      {
         get
         {
            return _SizeX;
         }
      }

      /// <summary>
      /// Gets the Z size.
      /// </summary>
      public static int SizeZ
      {
         get
         {
            return _SizeZ;
         }
      }

      internal static string WorldPath
      {
         get
         {
            return _WorldPath;
         }
      }
   }
}
