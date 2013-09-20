using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using Tao.OpenGl;

using Qbes.Client.Logic.Configuration;
using Qbes.Client.Logic.Constants;
using Qbes.Client.Logic.Exceptions;
using Qbes.Client.Logic.GameProviders;
using Qbes.Client.Logic.Networking;
using Qbes.Common.Logic;
using Qbes.Common.Logic.Configuration;
using Qbes.Common.Logic.Constants;
using Qbes.Common.Logic.Extensions;
using Qbes.Common.Logic.Networking;
using Qbes.Common.Logic.Networking.Constants;

namespace Qbes.Client.Logic
{
   /// <summary>
   /// The client world manager is essentialy the game manager for the client
   /// which takes care of user input and rendering.
   /// </summary>
   public sealed partial class ClientWorldManager : WorldManager<ClientArea, ClientSegment, ClientBox>
   {
      #region Constants
      private const string SkinsDir = "Skins";
      #endregion

      #region Static fields
      private static readonly ClientWorldManager _Instance = new ClientWorldManager();
      #endregion

      #region Fields
      private Dictionary<int, List<ClientArea>> _PartialTerrainData = new Dictionary<int, List<ClientArea>>();
      private Dictionary<int, List<int>> _PartialTerrainDataIds = new Dictionary<int, List<int>>();
      private Dictionary<int, List<ClientSegment>> _SegmentsForVisRefresh = new Dictionary<int, List<ClientSegment>>();
      private Thread _MultiPlayerTerrainThread;
      private ManualResetEvent _MultiPlayerInitialDataWait = new ManualResetEvent(false);
      private ManualResetEvent _MultiPlayerTerrainDataWait = new ManualResetEvent(false);
      private ManualResetEvent _StartWait = new ManualResetEvent(false);

      private ClientConnection _Client;
      private Player _Player = new Player();
      private byte[] _SkinHash = new byte[16];
      private Point3D _EyeLocation;
      private bool _WasMoving = false;

      private int _Height = 720;
      private Surface _Screen;
      private int _Width = 1280;
      private bool _FirstTextureLoad = true;

      private bool _MouseGrab = false;
      private float _RenderDistance = 36864.0f; // 192m
      private bool _ShowHud = true;
      private float[] _SkyColor = new float[4];
      private bool _Fog = true;
      
      private int _Frame = 0;
      private int _PendingTerrainRequests = 0;
      private int _CurrentTime = 0;
      private int _DiagTime = 0;
      private int _LastMovedSentTime = 0;
      private bool _Tick = false;

      private object _MultiplayerTerrainRequestsLock = new object();
      private object _PendingTerrainRequestsLock = new object();
      private object _TickLock = new object();

      private LoadRequest _MultiplayerTerrainRequest = new LoadRequest();
      private Queue<ClientBox> _PooledBoxes = new Queue<ClientBox>();
      private Queue<ClientSegment> _PooledSegments = new Queue<ClientSegment>();
      private Point3D _TempPoint = new Point3D();

      private bool _ChatBoxActive = false;
      private List<string> _ChatHistory = new List<string>();
      private string _CurrentChatMessage = string.Empty;
      #endregion

      #region Constructors
      private ClientWorldManager()
      {
         // read configuration
         Configuration = ConfigurationManager.ReadConfiguration<ClientConfiguration>();
         InitializeConfiguration(Configuration);

         _RenderDistance = Configuration.Video.VisibilityRange * Configuration.Video.VisibilityRange;
         _Fog = Configuration.Video.Fog;
         _SkyColor = Configuration.Video.SkyColor.ToArray();
         _Width = Configuration.Video.ResolutionWidth;
         _Height = Configuration.Video.ResolutionHeight;

         // load skin
         byte[] skin = File.ReadAllBytes(Path.Combine("..", "..", SkinsDir, Configuration.Customization.SkinFileName));
         _SkinHash = skin.GetMd5Checksum();
         SkinManager.SaveSkin(ref _SkinHash, ref skin);

         // set singleplayer provider
         if (Configuration.Network.AutoConnect)
         {
            IsSinglePlayer = false;
            WorldHelper.SetMessagingProviders(MultiPlayerProvider.ClientToServerProvider, MultiPlayerProvider.ServerToClientProvider);
         }
         else
         {
            IsSinglePlayer = true;
            WorldHelper.SetMessagingProviders(SinglePlayerProvider.ClientToServerProvider, SinglePlayerProvider.ServerToClientProvider);
            // dummy client connection
            _Client = new ClientConnection(true);
         }

         // set delegates
         WorldHelper.SetGetPooledBoxDelegate(GetPooledBox);
         WorldHelper.SetGetPooledSegmentDelegate(GetPooledSegment);
         WorldHelper.SetGetSegmentDelegate(GetSegment);

         // Sets keyboard events
         Events.KeyboardDown += new EventHandler<KeyboardEventArgs>(KeyDown);
         Events.KeyboardUp += new EventHandler<KeyboardEventArgs>(KeyUp);
         Events.MouseButtonDown += new EventHandler<MouseButtonEventArgs>(MouseButtonDown);
         Events.MouseButtonUp += new EventHandler<MouseButtonEventArgs>(MouseButtonUp);
         Events.MouseMotion += new EventHandler<MouseMotionEventArgs>(MouseMotion);
         Events.TargetFps = 1000;

         // Sets Window icon and title
         this.WindowAttributes();

         // Creates SDL.NET Surface to hold an OpenGL scene
         _Screen = Video.SetVideoMode(_Width, _Height, true, true);
         CenterMouseCursor();
      }

      static ClientWorldManager()
      {
         // empty
      }
      #endregion

      internal void AddChatMessage(string message)
      {
         _ChatHistory.Add(message);
      }

      internal void AddSegmentForVisibilityRefresh(ref int messageId, ClientSegment clientSegment)
      {
         _SegmentsForVisRefresh[messageId].Add(clientSegment);
      }

      internal bool CachePartialTerrainData(int terrainMessageId, List<ClientArea> partialData, byte current, byte total)
      {
         if (!_PartialTerrainData.ContainsKey(terrainMessageId))
         {
            _PartialTerrainData.Add(terrainMessageId, new List<ClientArea>());
            _PartialTerrainDataIds.Add(terrainMessageId, new List<int>());
         }

         _PartialTerrainData[terrainMessageId].AddRange(partialData);
         List<int> ids = _PartialTerrainDataIds[terrainMessageId];
         if (ids.Contains(current))
         {
            // now this is a serious issue
            throw new DuplicateTerrainMessageException();
         }
         ids.Add(current);

         return (ids.Count == total);
      }

      private void CenterMouseCursor()
      {
         Events.Remove(EventMask.MouseMotion);
         Mouse.MousePosition = new Point(_Width / 2, _Height / 2);
         Events.Retrieve(EventMask.MouseMotion);
      }

      /// <summary>
      /// Instructs the manager to dispose no longer needed areas.
      /// </summary>
      protected override void CleanAreas()
      {
         List<Tuple<int, int>> columns = GetEntityColumns(_Player);

         // flag nonlisted areas for removal
         List<ClientArea> areas = new List<ClientArea>(Areas.Values);
         foreach (ClientArea area in areas)
         {
            if (!area.IsInitialized ||
                columns.Contains(new Tuple<int, int>(area.X, area.Z)))
            {
               // this area has not been initialize yet or is an active area
               continue;
            }

            if (area.Key < 0)
            {
               // now this is a problem...
               DiagnosticsManager.WriteMessage("WARNING: attempt to remove an area without a key!");
               continue;
            }

            area.IsFlaggedForRemoval = true;
            lock (CollectionsLock)
            {
               // lock the collections to prevent conflicts
               int segmentCount = area.GetSegmentCount();
               int boxesCount = 0;

               // save changed
               if (area.IsChanged)
               {
                  SaveArea(area);
               }

               for (int i = 0; i < segmentCount; i++)
               {
                  ClientSegment segment = area.GetSegment<ClientSegment>(ref i);
                  boxesCount += segment.GetBoxCount();
                  foreach (ClientBox box in segment.GetBoxesSynchronized())
                  {
                     _PooledBoxes.Enqueue(box);
                  }
                  segment.Unload();
                  Segments.Remove(segment.Key);

                  _PooledSegments.Enqueue(segment);
               }
               area.Unload();
               Areas.Remove(area.Key);

               DiagnosticsManager.BoxesLoaded -= boxesCount;
               DiagnosticsManager.SegmentsLoaded -= segmentCount;
               DiagnosticsManager.AreasLoaded--;
            }
         }
      }

      internal void CleanAreas(ref int messageId)
      {
         // clean areas only once for each whole terrain batch
         if (!_PartialTerrainData.ContainsKey(messageId))
         {
            CleanAreas();
         }
      }

      internal ClientConnection Client
      {
         get
         {
            return _Client;
         }
      }

      internal int ColumnX
      {
         get
         {
            return _Player.CurrentColumnX;
         }
      }

      internal int ColumnZ
      {
         get
         {
            return _Player.CurrentColumnZ;
         }
      }

      internal ClientConfiguration Configuration { get; private set; }

      private Box GetPooledBox()
      {
         if (_PooledBoxes.Count > 0)
         {
            return _PooledBoxes.Dequeue() ?? new ClientBox();
         }

         return new ClientBox();
      }

      private Segment GetPooledSegment()
      {
         if (_PooledSegments.Count > 0)
         {
            return _PooledSegments.Dequeue() ?? new ClientSegment();
         }

         return new ClientSegment();
      }

      /// <summary>
      /// Gets a segment based on key.
      /// </summary>
      /// <param name="key">Segment key</param>
      /// <returns>Segment by key</returns>
      protected override Segment GetSegment(ref int key)
      {
         if (!Segments.ContainsKey(key))
         {
            return null;
         }
         return Segments[key];
      }

      internal ClientSegment GetSegment(int key)
      {
         lock (CollectionsLock)
         {
            if (Segments.ContainsKey(key))
            {
               return Segments[key];
            }

            return null;
         }
      }

      /// <summary>
      /// Looks up the neighbouring segments for given segment.
      /// </summary>
      /// <param name="segment">Segment</param>
      /// <returns>Neighbouring segments</returns>
      /// <remarks>Non-existing neighbours are stored as
      /// ClientSegment.DummyNeighbour</remarks>
      public override ClientSegment[] GetSegmentsNeighbours(ClientSegment segment)
      {
         ClientSegment[] result = base.GetSegmentsNeighbours(segment);

         for (int i = 0; i < 6; i++)
         {
            if (result[i] == null)
            {
               //int key = -1;

               //switch (i)
               //{
               //   case Sides.BackX:
               //      key = Segment.GenerateKey(0, segment.Y, segment.Z;
               //      break;
               //   case Sides.BackZ:
               //      key = Segment.GenerateKey(segment.X, segment.Y, 0);
               //      break;
               //   case Sides.FrontX:
               //      key = Segment.GenerateKey(WorldHelper.SizeX - 8, segment.Y, segment.Z);
               //      break;
               //   case Sides.FrontZ:
               //      key = Segment.GenerateKey(segment.X, segment.Y, WorldHelper.SizeZ - 8);
               //      break;
               //   default:
               //      break;
               //}

               //if (key < 0 || !Segments.ContainsKey(key))
               //{
               result[i] = ClientSegment.DummyNeighbour;
               //}
               //else
               //{
               //   result[i] = Segments[key];
               //}
            }
         }

         return result;
      }

      private void HandlePlayerMovement()
      {
         IsMoved = _Player.HandleMovement();
         if ((IsMoved && _CurrentTime - _LastMovedSentTime > UpdateIntervals.EntityMovingInterval) ||
             (!IsMoved && _WasMoving))
         {
            _LastMovedSentTime = Environment.TickCount;
            WorldHelper.ClientToServerProvider.PlayerMovedNotification(_Client.Connection, _Player.Location, _Player.RotationLeft, _Player.RotationUp);
         }
         _WasMoving = IsMoved;
      }

      /// <summary>
      /// Gets the singleton instance.
      /// </summary>
      public static ClientWorldManager Instance
      {
         get
         {
            return _Instance;
         }
      }

      internal bool IsMoved { get; private set; }

      internal bool IsSinglePlayer { get; private set; }

      //private void LevelGround(int level)
      //{
      //   for (int x = ColumnX; x < ColumnX + 64; x++)
      //   {
      //      for (int y = level; y < 256; y++)
      //      {
      //         for (int z = ColumnZ; z < ColumnZ + 64; z++)
      //         {
      //            int segmentX = x - x % 8;
      //            int segmentY = y - y % 8;
      //            int segmentZ = z - z % 8;
      //            Segment segment = GetSegment(ref segmentX, ref segmentY, ref segmentZ);
      //            if (segment.GetBoxCount() == 0)
      //            {
      //               continue;
      //            }

      //            segment.PlaceOrRemoveBlock(false, x, y, z, 0);
      //         }
      //      }
      //   }
      //}

      internal void LoadPlayerColumns(int messageId)
      {
         LoadPlayerColumns(messageId, Player, Player.CurrentColumnX, Player.CurrentColumnZ);
      }

      internal Point3D Location
      {
         get
         {
            return _Player.Location;
         }
      }

      internal ushort MultiPlayerEntityID { get; set; }

      private void MultiPlayerTerrainLoader()
      {
         while (true)
         {
            int terrainMessageId = -1;
            lock (_MultiplayerTerrainRequestsLock)
            {
               if (_MultiplayerTerrainRequest.MessageId > 0)
               {
                  terrainMessageId = _MultiplayerTerrainRequest.MessageId;
                  _MultiplayerTerrainRequest.MessageId = -1;
               }
            }

            if (terrainMessageId > 0)
            {
               WorldHelper.ClientToServerProvider.TerrainDataRequest(_Client.Connection, terrainMessageId, Location, _Player.RotationLeft, _Player.RotationUp);
            }
            else
            {
               Thread.Sleep(1000);
               continue;
            }

            _MultiPlayerTerrainDataWait.WaitOne();
            _MultiPlayerTerrainDataWait.Reset();
         }
      }

      private void NextMaterial(bool next)
      {
         int material = _Player.SelectedMaterial;

         if (next)
         {
            material++;
            if (material > Material.LastPlaceableMaterial)
            {
               material = Material.FirstPlaceableMaterial;
            }
         }
         else
         {
            material--;
            if (material < Material.FirstPlaceableMaterial)
            {
               material = Material.LastPlaceableMaterial;
            }
         }

         _Player.SelectedMaterial = (ushort)material;
      }

      internal void OnAreasLoaded(int messageId)
      {
         OnAreasLoaded(messageId, _PartialTerrainData[messageId]);
         _PartialTerrainData.Remove(messageId);
         _PartialTerrainDataIds.Remove(messageId);
         _MultiPlayerTerrainDataWait.Set();
      }

      internal void OnAreasLoaded(int messageId, List<ClientArea> areas)
      {
         List<ClientSegment> segments = new List<ClientSegment>(areas.Count * 512);
         foreach (ClientArea area in areas)
         {
            List<ClientSegment> areaSegments = area.GetSegments();
            segments.AddRange(areaSegments);
            Areas.Add(area.Key, area);
            foreach (ClientSegment segment in areaSegments)
            {
               Segments.Add(segment.Key, segment);
            }
         }

         LoadRequest request = new LoadRequest()
         {
            Entity = _Player,
            MessageId = messageId
         };
         OnAreasLoaded(request, areas, segments);

         if (_Player.CurrentSegment == null)
         {
            // prepare the player
            _Player.HandleMovement();
         }
      }

      /// <summary>
      /// Runs visibility checks after new segments get loaded.
      /// </summary>
      /// <param name="loadRequest">Load request instance</param>
      /// <param name="areas">New areas</param>
      /// <param name="segments">New segments</param>
      protected override void OnAreasLoaded(LoadRequest loadRequest,
                                            List<ClientArea> areas,
                                            List<ClientSegment> segments)
      {
         lock (_PendingTerrainRequestsLock)
         {
            DiagnosticsManager.AreasLoaded += areas.Count;
            DiagnosticsManager.SegmentsLoaded += segments.Count;

            PrepareClientSegments(loadRequest.MessageId, segments);
            foreach (ClientArea area in areas)
            {
               area.IsReady = true;
            }

            _PendingTerrainRequests--;
         }

         _StartWait.Set();
      }

      /// <summary>
      /// Reinitialies the openGL renderer.
      /// </summary>
      protected override void OnReset()
      {
         Reshape();
      }

      internal Player Player
      {
         get
         {
            return _Player;
         }
         private set
         {
            if (_Player != null)
            {
               // unsubscribe from old event
               _Player.OnColumnChanged -= Player_OnColumnChanged;
            }

            _Player = value;
            _Player.OnColumnChanged += new EventHandler(Player_OnColumnChanged);

            _MultiPlayerInitialDataWait.Set();
         }
      }

      private void Player_OnColumnChanged(object sender, EventArgs e)
      {
         lock (_PendingTerrainRequestsLock)
         {
            // increment the number of pending requests
            _PendingTerrainRequests++;
         }

         // load the player columns
         if (IsSinglePlayer)
         {
            WorldHelper.ClientToServerProvider.TerrainDataRequest(_Client.Connection, GetNextMessageId(), Player.Location, Player.RotationLeft, Player.RotationUp);
         }
         else
         {
            lock (_MultiplayerTerrainRequestsLock)
            {
               _MultiplayerTerrainRequest.ColumnX = Player.CurrentColumnX;
               _MultiplayerTerrainRequest.ColumnZ = Player.CurrentColumnZ;
               _MultiplayerTerrainRequest.MessageId = GetNextMessageId();
            }
         }
      }

      internal int PooledBoxCount
      {
         get
         {
            return _PooledBoxes.Count;
         }
      }

      internal int PooledSegmentCount
      {
         get
         {
            return _PooledSegments.Count;
         }
      }

      private void PrepareClientSegments(int messageId, List<ClientSegment> segments)
      {
#if DIAG
         int time = Environment.TickCount;
#endif
         foreach (ClientSegment segment in segments)
         {
            segment.CheckEnclosed();
            DiagnosticsManager.BoxesLoaded += segment.GetBoxCount();
         }
#if DIAG
         DiagnosticsManager.WriteMessage("Segment enclosed check done in {0} ms",
                                         (Environment.TickCount - time));
#endif

#if DIAG
         time = Environment.TickCount;
#endif
         _SegmentsForVisRefresh.Add(messageId, new List<ClientSegment>());
         foreach (ClientSegment segment in segments)
         {
            segment.CheckHidden(ref messageId);
            segment.SortBoxesByMaterial();
         }

         // update visibility for segments that have new neighbours
         foreach (ClientSegment segment in _SegmentsForVisRefresh[messageId])
         {
            segment.CheckHidden(ref messageId);
         }
         _SegmentsForVisRefresh.Remove(messageId);
#if DIAG
         DiagnosticsManager.WriteMessage("Hidden segment/face elimination done in {0} ms",
                                         (Environment.TickCount - time));
#endif
      }

      internal float RenderDistance
      {
         get
         {
            return _RenderDistance;
         }
      }

      internal void RequestInitialTerrainData()
      {
         if (IsSinglePlayer)
         {
            return;
         }

         Task task = new Task(() =>
         {
            // wait for player to load
            _MultiPlayerInitialDataWait.WaitOne();
            _MultiPlayerTerrainDataWait.Set();

            // explicitely fire the OnColumnChanged event handler
            Player_OnColumnChanged(this, EventArgs.Empty);
         });
         task.Start();
      }

      internal void ResetEntityTextureNames()
      {
         List<Entity> entities = new List<Entity>(Entities.Values);
         foreach (Entity entity in entities)
         {
            entity.SkinTextureName = -1;
         }
      }

      private void Reshape()
      {
         if (!_FirstTextureLoad)
         {
            TextureManager.Instance.UnloadTextures(true);
         }

         InitGL();
      }

      internal float RotationX
      {
         get
         {
            return _Player.RotationUp;
         }
      }

      internal float RotationY
      {
         get
         {
            return _Player.RotationLeft;
         }
      }

      internal void SetPlayer(Entity entity)
      {
         if (!(entity is Player))
         {
            throw new NotPlayerEntityException();
         }

         Player = (Player)entity;
      }

      internal byte[] SkinHash
      {
         get
         {
            return _SkinHash;
         }
      }

      /// <summary>
      /// Starts listening to the input events.
      /// </summary>
      public void Start()
      {
         // no need to re-init OpenGL as this was already done by OnReset
         // Reshape();

         if (IsSinglePlayer)
         {
            Load(false);
            // lookup player
            foreach (Entity entity in Entities.Values)
            {
               if (entity is Player)
               {
                  Player = entity as Player;
                  break;
               }
            }
#if DIAG
            DiagnosticsManager.WriteMessage("Retrieved player entity");
#endif
         }
         else
         {
            _MultiPlayerTerrainThread = new Thread(MultiPlayerTerrainLoader);
            _MultiPlayerTerrainThread.Priority = ThreadPriority.Lowest;
            _MultiPlayerTerrainThread.Start();
            _Client = new ClientConnection();
            _Client.Start(Configuration.MessagingConfigurationNode, Configuration.Network.GetRemoteEndPointUdp());
         }

         _StartWait.WaitOne();

#if DIAG
         DiagnosticsManager.WriteMessage("Client ready");
#endif

         Events.Run();
      }

      internal void Stop()
      {
         if (IsSinglePlayer)
         {
            // save changed areas
            Save(false);
         }
         else if (_Client != null)
         {
            // stop the multiplayer client
            _MultiPlayerTerrainThread.Abort();
            _Client.Stop();
         }

         // save configuration
         ConfigurationManager.SaveConfiguration(Configuration);

         // stop load thread
         StopWorkerThreads();
         // Will stop the app loop
         Events.QuitApplication();
      }

      private void StopTimer()
      {
         Events.Tick -= Tick;
         bool wait = true;
         while (wait)
         {
            lock (_TickLock)
            {
               wait = _Tick;
            }

            if (wait)
            {
#if DIAG
               Console.WriteLine("Toggle fullscreen waiting for draw to complete...");
#endif
               Thread.Sleep(100);
            }
         }
      }

      internal Point3D TempPoint
      {
         get
         {
            return _TempPoint;
         }
      }

      private void Tick(object sender, TickEventArgs e)
      {
         lock (_TickLock)
         {
            _Tick = true;
         }

         DrawGLScene();
         Video.GLSwapBuffers();

         lock (_TickLock)
         {
            _Tick = false;
         }
      }

      private void WindowAttributes()
      {
         Video.WindowIcon();
         Video.WindowCaption = string.Format("Qbes v{0}", Assembly.GetExecutingAssembly().ImageRuntimeVersion);
      }
   }
}
