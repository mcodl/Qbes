using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Qbes.Common.Logic.Configuration;
using Qbes.Common.Logic.Constants;
using Qbes.Common.Logic.Exceptions;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// The World class is the highest in the world hierarchy that wraps the
   /// areas. Its main purpose is to manage the world map and entities.
   /// </summary>
   /// <typeparam name="TArea">Area type</typeparam>
   /// <typeparam name="TSegment">Segment type</typeparam>
   /// <typeparam name="TBox">Box type</typeparam>
   public abstract class WorldManager<TArea, TSegment, TBox>
      where TArea : Area, new()
      where TSegment : Segment, new()
      where TBox : Box, new()
   {
      #region Constants
      private const string DirAreas = "areas";
      private const string DirEntities = "entities";
      private const string ExtensionArea = ".area";
      private const string ExtensionEntity = ".entity";
      private const int LoadThreadWait = 100;
      private const string SubDirPlayers = "players";
      private const string WorldInfoFileName = "info.world";
      private const int WorldInfoNameLineIndex = 0;
      private const int WorldInfoXSizeLineIndex = 1;
      private const int WorldInfoZSizeLineIndex = 2;
      #endregion

      #region Static fields
      private static readonly string _WorldsPath = Path.Combine("..", "..", "Worlds");
      #endregion

      #region Fields
      private Dictionary<int, TArea> _Areas = new Dictionary<int,TArea>();
      private Thread _AutoSaveThread;
      private ManualResetEvent _AutoSaveWait = new ManualResetEvent(false);
      private bool _AutoSaving = false;
      private object _AutoSavingLock = new object();
      private object _CollectionsLock = new object();
      private IQbesConfiguration _Configuration;
      private Dictionary<ushort, Entity> _Entities = new Dictionary<ushort, Entity>();
      private bool _Initialized = false;
      private Queue<LoadRequest> _LoadQueue = new Queue<LoadRequest>();
      private Thread _LoadThread;
      private int _MessageId = 0;
      private object _MessageIdLock = new object();
      private string _Name;
      private string _Path;
      private Dictionary<int, TSegment> _Segments = new Dictionary<int,TSegment>();
      private bool _Stopping = false;
      private string _Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
      #endregion

      #region Constructors
      /// <summary>
      /// Initializes the base part of the world manager.
      /// </summary>
      protected WorldManager()
      {
         WorldHelper.SetGetEntitiesDelegate(GetEntities);
         WorldHelper.SetGetSegmentNeighboursDelegate(GetSegmentNeighboursForCollisions);

         // create cache dir if doesn't exist
         if (!Directory.Exists(WorldHelper.CachePath))
         {
            Directory.CreateDirectory(WorldHelper.CachePath);
         }
      }
      #endregion

      /// <summary>
      /// Gets the world's areas collection.
      /// </summary>
      protected Dictionary<int, TArea> Areas
      {
         get
         {
            return _Areas;
         }
      }

      private void AutoSaveWorker()
      {
         int interval = _Configuration.AutoSaveConfigurationNode.AutoSaveInterval * 1000;

         _AutoSaveWait.WaitOne(interval);
         _AutoSaveWait.Reset();

         while (_Initialized && !_Stopping)
         {
            lock (_AutoSavingLock)
            {
               _AutoSaving = true;
            }
            Console.WriteLine("Autosaving...");
            Save(false);
            lock (_AutoSavingLock)
            {
               _AutoSaving = false;
            }
            _AutoSaveWait.WaitOne(interval);
            _AutoSaveWait.Reset();
         }
      }

      /// <summary>
      /// Instructs the manager to dispose no longer needed areas.
      /// </summary>
      protected abstract void CleanAreas();

      /// <summary>
      /// Gets the collections lock object.
      /// </summary>
      protected object CollectionsLock
      {
         get
         {
            return _CollectionsLock;
         }
      }

      /// <summary>
      /// Creates a new world and saves it.
      /// </summary>
      /// <param name="areasX">Number of areas on the X axis</param>
      /// <param name="areasZ">Number of areas on the Z axis</param>
      public void CreateWorld(byte areasX, byte areasZ)
      {
         CreateWorld(areasX, areasZ, Environment.TickCount);
      }

      /// <summary>
      /// Creates a new world and saves it.
      /// </summary>
      /// <param name="areasX">Number of areas on the X axis</param>
      /// <param name="areasZ">Number of areas on the Z axis</param>
      /// <param name="seed">Random generator seed</param>
      public void CreateWorld(byte areasX, byte areasZ, int seed)
      {
         SaveDirectoryStructure();
         WorldGenerator.CreateProceduralWorld(areasX, areasZ, seed, this);
      }

      /// <summary>
      /// Gets a collection with entities.
      /// </summary>
      public Dictionary<ushort, Entity> Entities
      {
         get
         {
            return _Entities;
         }
      }

      private List<Entity> GetEntities(Point3D point, float radiusSquare)
      {
         List<Entity> result = _Entities.Where(e => e.Value.Location.GetDistanceSquare(point) <= radiusSquare).Select(e => e.Value).ToList();

         return result;
      }

      private string GetFullAreasDirectoryPath()
      {
         return GetFullWorldPath() + DirAreas + Path.DirectorySeparatorChar;
      }

      private string GetFullEntitiesDirectoryPath()
      {
         return GetFullWorldPath() + DirEntities + Path.DirectorySeparatorChar;
      }

      private string GetFullWorldPath()
      {
         return _Path + Path.DirectorySeparatorChar;
      }

      /// <summary>
      /// Gets next message ID.
      /// </summary>
      /// <returns>Next message ID</returns>
      protected int GetNextMessageId()
      {
         lock (_MessageIdLock)
         {
            return (++_MessageId);
         }
      }

      /// <summary>
      /// Gets the columns around the given entity.
      /// </summary>
      /// <param name="entity">Entity</param>
      /// <returns>List with columns around the entity</returns>
      protected List<Tuple<int, int>> GetEntityColumns(Entity entity)
      {
         return GetEntityColumns(entity.CurrentColumnX, entity.CurrentColumnZ);
      }

      /// <summary>
      /// Gets the columns around the given entity.
      /// </summary>
      /// <param name="currentColumnX">Current X column</param>
      /// <param name="currentColumnZ">Current Z column</param>
      /// <returns>List with columns around the entity</returns>
      protected List<Tuple<int, int>> GetEntityColumns(int currentColumnX, int currentColumnZ)
      {
         List<Tuple<int, int>> result = new List<Tuple<int, int>>();

         int columnsLoaded = 0;
         int playerAreaX = currentColumnX;
         int playerAreaZ = currentColumnZ;
         int dir = 0;
         int length = 1;

         while (columnsLoaded < 49)
         {
            for (int i = 0; i < length; i++)
            {
               if (playerAreaX >= 0 && playerAreaX < WorldHelper.SizeX &&
                   playerAreaZ >= 0 && playerAreaZ < WorldHelper.SizeZ)
               {
                  result.Add(new Tuple<int, int>(playerAreaX, playerAreaZ));
               }
               else
               {
                  // shift needed
                  int shiftedAreaX = playerAreaX;
                  if (shiftedAreaX < 0)
                  {
                     shiftedAreaX += WorldHelper.SizeX;
                  }
                  else if (shiftedAreaX >= WorldHelper.SizeX)
                  {
                     shiftedAreaX -= WorldHelper.SizeX;
                  }

                  int shiftedAreaZ = playerAreaZ;
                  if (shiftedAreaZ < 0)
                  {
                     shiftedAreaZ += WorldHelper.SizeZ;
                  }
                  else if (shiftedAreaZ >= WorldHelper.SizeZ)
                  {
                     shiftedAreaZ -= WorldHelper.SizeZ;
                  }

                  result.Add(new Tuple<int, int>(shiftedAreaX, shiftedAreaZ));
               }
               columnsLoaded++;

               switch (dir)
               {
                  case 0:
                     playerAreaX += 64;
                     break;
                  case 1:
                     playerAreaZ += 64;
                     break;
                  case 2:
                     playerAreaX -= 64;
                     break;
                  case 3:
                     playerAreaZ -= 64;
                     break;
               }
            }

            dir--;
            if (dir < 0)
            {
               // reset direction to 3
               dir = 3;
            }
            if (dir % 2 == 0)
            {
               // extend length
               length++;
            }
         }

         return result;
      }

      /// <summary>
      /// Gets a segment based on given coordinates.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <returns>Segment at given coordinates</returns>
      public Segment GetSegment(ref int x, ref int y, ref int z)
      {
         int key = Segment.GenerateKey(x, y, z);
         return GetSegment(ref key);
      }

      /// <summary>
      /// Gets a segment based on key.
      /// </summary>
      /// <param name="key">Segment key</param>
      /// <returns>Segment by key</returns>
      protected abstract Segment GetSegment(ref int key);

      /// <summary>
      /// Gets a segment which contains given coordinates.
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      /// <param name="z">Z coordinate</param>
      /// <returns>Segment which contains given coordinate</returns>
      public Segment GetSegmentBasedOnInsidePoint(int x, int y, int z)
      {
         x -= (x % 8);
         y -= (y % 8);
         z -= (z % 8);
         return GetSegment(ref x, ref y, ref z);
      }

      [Obsolete]
      private Segment[] GetSegmentsNeighbours(Segment segment)
      {
         return GetSegmentsNeighbours(segment as TSegment);
      }

      /// <summary>
      /// Looks up the neighbouring segments for given segment.
      /// </summary>
      /// <param name="segment">Segment</param>
      /// <returns>Neighbouring segments</returns>
      /// <remarks>Non-existing neighbours are stored as NULLs</remarks>
      public virtual TSegment[] GetSegmentsNeighbours(TSegment segment)
      {
         TSegment[] neighbours = new TSegment[6];

         int xM = segment.X - 8;
         //if (xM < 0)
         //{
         //   xM += WorldHelper.SizeX;
         //}
         int xP = segment.X + 8;
         //if (xP > WorldHelper.SizeX)
         //{
         //   xP -= WorldHelper.SizeX;
         //}
         int zM = segment.Z - 8;
         //if (zM < 0)
         //{
         //   zM += WorldHelper.SizeZ;
         //}
         int zP = segment.Z + 8;
         //if (zP > WorldHelper.SizeZ)
         //{
         //   zP -= WorldHelper.SizeZ;
         //}

         Segments.TryGetValue(Segment.GenerateKey(xM, segment.Y, segment.Z),
                              out neighbours[Sides.FrontX]);
         Segments.TryGetValue(Segment.GenerateKey(segment.X, (segment.Y - 8), segment.Z),
                              out neighbours[Sides.FrontY]);
         Segments.TryGetValue(Segment.GenerateKey(segment.X, segment.Y, zM),
                              out neighbours[Sides.FrontZ]);
         Segments.TryGetValue(Segment.GenerateKey(xP, segment.Y, segment.Z),
                              out neighbours[Sides.BackX]);
         Segments.TryGetValue(Segment.GenerateKey(segment.X, (segment.Y + 8), segment.Z),
                              out neighbours[Sides.BackY]);
         Segments.TryGetValue(Segment.GenerateKey(segment.X, segment.Y, zP),
                              out neighbours[Sides.BackZ]);

         return neighbours;
      }

      private Segment[] GetSegmentNeighboursForCollisions(Segment segment, Point3D location)
      {
         TSegment[] neighbours = new TSegment[7];

         int xS = segment.X;
         if (location.X - xS < 4)
         {
            xS -= 8;
            //if (xS < 0)
            //{
            //   xS += WorldHelper.SizeX;
            //}
         }
         else
         {
            xS += 8;
            //if (xS > WorldHelper.SizeX)
            //{
            //   xS -= WorldHelper.SizeX;
            //}
         }

         int yS = segment.Y;
         if (location.Y - yS < 4)
         {
            yS -= 8;
         }
         else
         {
            yS += 8;
         }

         int zS = segment.Z;
         if (location.Z - zS < 4)
         {
            zS -= 8;
            //if (zS < 0)
            //{
            //   zS += WorldHelper.SizeZ;
            //}
         }
         else
         {
            zS += 8;
            //if (zS > WorldHelper.SizeZ)
            //{
            //   zS -= WorldHelper.SizeZ;
            //}
         }

         Segments.TryGetValue(Segment.GenerateKey(xS, segment.Y, segment.Z),
                              out neighbours[0]);
         Segments.TryGetValue(Segment.GenerateKey(segment.X, yS, segment.Z),
                              out neighbours[1]);
         Segments.TryGetValue(Segment.GenerateKey(segment.X, segment.Y, zS),
                              out neighbours[2]);
         Segments.TryGetValue(Segment.GenerateKey(xS, yS, segment.Z),
                              out neighbours[3]);
         Segments.TryGetValue(Segment.GenerateKey(segment.X, yS, zS),
                              out neighbours[4]);
         Segments.TryGetValue(Segment.GenerateKey(xS, segment.Y, zS),
                              out neighbours[5]);
         Segments.TryGetValue(Segment.GenerateKey(xS, yS, zS),
                              out neighbours[6]);

         return neighbours;
      }

      /// <summary>
      /// Initializes the common client/server configuration.
      /// </summary>
      /// <param name="configuration">Configuration instance</param>
      protected void InitializeConfiguration(IQbesConfiguration configuration)
      {
         _Configuration = configuration;
      }

      /// <summary>
      /// Gets whether the world is initialized.
      /// </summary>
      public bool IsInitialized
      {
         get
         {
            return _Initialized;
         }
      }

      /// <summary>
      /// Lods the world from data files.
      /// </summary>
      /// <param name="server">Determines whether this is a server load</param>
      public void Load(bool server)
      {
         if (_Initialized)
         {
            throw new WorldAlreadyInitializedException();
         }

         // first load world info file
         string worldPath = GetFullWorldPath();
         string[] worldInfo = File.ReadAllLines(worldPath + WorldInfoFileName);

         _Name = worldInfo[WorldInfoNameLineIndex];
         WorldHelper.SetWorldSize(Convert.ToInt32(worldInfo[WorldInfoXSizeLineIndex]),
                                  Convert.ToInt32(worldInfo[WorldInfoZSizeLineIndex]));

         // load entities
         DirectoryInfo entityDir = new DirectoryInfo(GetFullEntitiesDirectoryPath());
         foreach (FileInfo entityFile in entityDir.GetFiles("*" + ExtensionEntity))
         {
            Entity entity = Entity.CreateFromByteArray(File.ReadAllBytes(entityFile.FullName));
            _Entities.Add(entity.ID, entity);
            if (!server)
            {
               LoadEntityColumns(new LoadRequest()
               {
                  MessageId = -entity.ID,
                  Entity = entity
               });
               entity.InitializeTerrainDependencies();
            }
         }

         _Initialized = true;

         if (_Configuration.AutoSaveConfigurationNode.EnableAutoSave)
         {
            _AutoSaveWait.Reset();
            _AutoSaveThread = new Thread(AutoSaveWorker);
            _AutoSaveThread.Start();
         }
      }

      private void LoadColumn(int x, int z, List<TArea> areas, List<TSegment> segments)
      {
         lock (CollectionsLock)
         {
            // lock the collections to prevent collisions
            if (_Areas.ContainsKey(Area.GenerateKey(x, 0, z)))
            {
               // already loaded or is being loaded
               return;
            }

            for (int y = 0; y < 256; y += 64)
            {
               TArea area = new TArea();
               _Areas[Area.GenerateKey(x, y, z)] = area;
            }
         }

         for (int y = 0; y < 256; y += 64)
         {
            string fileKey = x + "_" + y + "_" + z;

            TArea area = _Areas[Area.GenerateKey(x, y, z)];

            string fullName = GetFullAreasDirectoryPath() + fileKey + ExtensionArea;
            area.InitializeFromByteArray<TSegment, TBox>(File.ReadAllBytes(fullName));
            for (int i = 0; i < area.GetSegmentCount(); i++)
            {
               TSegment segment = area.GetSegment<TSegment>(ref i);

               // integrity test
               int cubeCount = 0;
               for (int bIndex = 0; bIndex < segment.GetBoxCount(); bIndex++)
               {
                  TBox box = segment.GetBox<TBox>(ref bIndex);
                  cubeCount += Convert.ToInt32((box.X2 - box.X1) * (box.Y2 - box.Y1) * (box.Z2 - box.Z1));
               }
               if (cubeCount > 512)
               {
                  throw new CubeCountHighException(cubeCount);
               }
               lock (CollectionsLock)
               {
                  // lock the collections to prevent collisions
                  _Segments[segment.Key] = segment;
               }
               segments.Add(segment);
            }
            areas.Add(area);
         }
      }

      private void LoadEntityColumns(LoadRequest request)
      {
         // load new areas and segments
         List<TArea> areas = new List<TArea>();
         List<TSegment> segments = new List<TSegment>();

         foreach (Tuple<int, int> column in GetEntityColumns(request.ColumnX, request.ColumnZ))
         {
            LoadColumn(column.Item1, column.Item2, areas, segments);
         }

         OnAreasLoaded(request, areas, segments);
      }

      /// <summary>
      /// Loads areas for the player.
      /// </summary>
      /// <param name="messageId">Message ID</param>
      /// <param name="player">Player</param>
      /// <param name="columnX">Current X column</param>
      /// <param name="columnZ">Current Z column</param>
      protected void LoadPlayerColumns(int messageId, Player player, int columnX, int columnZ)
      {
         _LoadQueue.Enqueue(new LoadRequest()
         {
            ColumnX = columnX,
            ColumnZ = columnZ,
            Entity = player,
            MessageId = messageId
         });
      }

      /// <summary>
      /// Loads the whole map into the memory.
      /// </summary>
      /// <remarks>Use this only with server as client instances use a lot of
      /// memory.</remarks>
      protected void LoadWholeMap()
      {
         List<TArea> areas = new List<TArea>();
         List<TSegment> segments = new List<TSegment>();

         for (int columnX = 0; columnX < WorldHelper.SizeX; columnX += 64)
         {
            for (int columnZ = 0; columnZ < WorldHelper.SizeZ; columnZ += 64)
            {
               LoadColumn(columnX, columnZ, areas, segments);
            }
         }

         LoadRequest request = new LoadRequest()
         {
            MessageId = int.MinValue
         };
         OnAreasLoaded(request, areas, segments);
      }

      private void LoadWorker()
      {
         while (!_Stopping)
         {
            if (_LoadQueue.Count > 0)
            {
               LoadRequest request = _LoadQueue.Dequeue();
               if (request != null)
               {
                  // clean no longer needed areas
                  CleanAreas();

                  LoadEntityColumns(request);
               }
            }

            Thread.Sleep(LoadThreadWait);
         }
      }

      /// <summary>
      /// Initiated when new areas have been loaded
      /// </summary>
      /// <param name="loadRequest">Load request instance</param>
      /// <param name="areas">New areas</param>
      /// <param name="segments">New segments</param>
      protected abstract void OnAreasLoaded(LoadRequest loadRequest, List<TArea> areas, List<TSegment> segments);

      /// <summary>
      /// Initiated when Reset method is called.
      /// </summary>
      protected abstract void OnReset();

      /// <summary>
      /// Resets the world instance.
      /// </summary>
      /// <param name="name">World name</param>
      /// <param name="dirName">Name in the world directory</param>
      public void Reset(string name, string dirName)
      {
         Console.WriteLine("Loading world...");

         _Initialized = false;

         _Name = name;
         _Path = Path.Combine(_WorldsPath, dirName);
         WorldHelper.SetWorldPath(_Path);

         if (_Areas != null)
         {
            _Areas.Clear();
         }
         _Areas = new Dictionary<int, TArea>();

         if (_Segments != null)
         {
            _Segments.Clear();
         }
         _Segments = new Dictionary<int, TSegment>();

         if (_Entities != null)
         {
            _Entities.Clear();
         }
         _Entities = new Dictionary<ushort, Entity>();

         if (_LoadThread != null)
         {
            _LoadThread.Abort();
            _LoadThread.Join();
         }
         _LoadThread = new Thread(LoadWorker);
         _LoadThread.Priority = ThreadPriority.BelowNormal;
         _LoadThread.Start();

         StopAutoSave();

         if (_LoadQueue != null)
         {
            _LoadQueue.Clear();
         }
         _LoadQueue = new Queue<LoadRequest>();

         OnReset();
      }

      /// <summary>
      /// Saves the world.
      /// </summary>
      /// <param name="full">Determines whether full or differential save will
      /// be made</param>
      public void Save(bool full)
      {
         int time = Environment.TickCount;

         string fullAreasPath = GetFullAreasDirectoryPath();

         if (full)
         {
            // create initial directory structure
            SaveDirectoryStructure();
         }

         // write the areas
         List<Area> areas = new List<Area>();
         lock (CollectionsLock)
         {
            areas.AddRange(_Areas.Values);
         }

         foreach (Area area in areas)
         {
            if (full || area.IsChanged)
            {
               // save area either when full save or if changed
               SaveArea(area);
            }
         }

         // write entities
         foreach (Entity entity in _Entities.Values)
         {
            SaveEntity(entity);
         }

         Console.WriteLine("Saved in {0} ms", (Environment.TickCount - time));
      }

      /// <summary>
      /// Saves given area.
      /// </summary>
      /// <param name="area">Area to save</param>
      public void SaveArea(Area area)
      {
         string fullAreasPath = GetFullAreasDirectoryPath();
         string fileKey = area.X + "_" + area.Y + "_" + area.Z;
         File.WriteAllBytes(fullAreasPath + fileKey + ExtensionArea,
                            area.Serialize());
      }

      private void SaveDirectoryStructure()
      {
         string fullAreasPath = GetFullAreasDirectoryPath();
         if (!Directory.Exists(fullAreasPath))
         {
            Directory.CreateDirectory(fullAreasPath);
         }
      }

      private void SaveEntity(Entity entity)
      {
         string fullEntitiesPath = GetFullEntitiesDirectoryPath();
         string fileKey = entity.ID.ToString();
         File.WriteAllBytes(fullEntitiesPath + fileKey + ExtensionEntity,
                            entity.Serialize());
      }

      /// <summary>
      /// Gets the world's segments collection.
      /// </summary>
      protected Dictionary<int, TSegment> Segments
      {
         get
         {
            return _Segments;
         }
      }

      private void StopAutoSave()
      {
         if (_Configuration.AutoSaveConfigurationNode.EnableAutoSave)
         {
            _AutoSaveWait.Set();
            while (true)
            {
               lock (_AutoSavingLock)
               {
                  if (!_AutoSaving)
                  {
                     break;
                  }
               }
               Thread.Sleep(1);
            }
         }
      }

      /// <summary>
      /// Sets the stopping flag to true.
      /// </summary>
      protected void StopWorkerThreads()
      {
         _Stopping = true;
         StopAutoSave();
      }

      /// <summary>
      /// Gets the program version.
      /// </summary>
      public string Version
      {
         get
         {
            return _Version;
         }
      }

      /// <summary>
      /// Gets the path to the current world.
      /// </summary>
      public string WorldPath
      {
         get
         {
            return _Path;
         }
      }

      #region LoadRequest entity class
      /// <summary>
      /// Instances of the LoadRequest class are used to request terrain loads.
      /// </summary>
      protected sealed class LoadRequest
      {
         /// <summary>
         /// Gets or sets the entity's X column at the time of the request.
         /// </summary>
         public int ColumnX { get; set; }

         /// <summary>
         /// Gets or sets the entity's Z column at the time of the request.
         /// </summary>
         public int ColumnZ { get; set; }

         /// <summary>
         /// Gets or sets the entity.
         /// </summary>
         public Entity Entity { get; set; }

         /// <summary>
         /// Gets or sets the terrain message ID.
         /// </summary>
         public int MessageId { get; set; }
      }
      #endregion
   }
}
