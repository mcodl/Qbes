using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Qbes.Common.Logic;
using Qbes.Common.Logic.Configuration;
using Qbes.Common.Logic.Exceptions;
using Qbes.Common.Logic.Networking;
using Qbes.Server.Logic.Configuration;
using Qbes.Server.Logic.Networking;

namespace Qbes.Server.Logic
{
   /// <summary>
   /// The server manager takes care of the multiplayer game by managing player
   /// connections and network messages.
   /// </summary>
   public sealed class ServerWorldManager : WorldManager<ServerArea, ServerSegment, ServerBox>
   {
      #region Constants
      private const string CommandBroadcast = "broadcast";
      private const string CommandExit = "exit";
      private const string CommandKick = "kick";
      private const string CommandPlayers = "players";
      private const string CommandSave = "save";
      private const string CommandSaveAll = "save-all";
      private const string PlayersListFormat = "{0}, {1}, {2}";
      private const string PlayersListHeader = "Endpoint, ID, Name";
      #endregion

      #region Static fields
      private static readonly ServerWorldManager _Instance = new ServerWorldManager();
      #endregion

      #region Private fields
      private bool _Running = true;

      private Dictionary<ushort, List<int>> _ClientAreas = new Dictionary<ushort, List<int>>();
      private object _ClientLock = new object();

      private Queue<ServerBox> _PooledBoxes = new Queue<ServerBox>();
      private Queue<ServerSegment> _PooledSegments = new Queue<ServerSegment>();

      private ServerConfiguration _Configuration;
      #endregion

      #region Constructors
      private ServerWorldManager()
      {
         _Configuration = ConfigurationManager.ReadConfiguration<ServerConfiguration>();
         // save the configuration to update with possibly new contents
         ConfigurationManager.SaveConfiguration(_Configuration);

         InitializeConfiguration(Configuration);

         // set delegates
         WorldHelper.SetGetPooledBoxDelegate(GetPooledBox);
         WorldHelper.SetGetPooledSegmentDelegate(GetPooledSegment);
         WorldHelper.SetGetSegmentDelegate(GetSegment);
      }

      static ServerWorldManager()
      {
         // empty
      }
      #endregion

      /// <summary>
      /// Instructs the manager to dispose no longer needed areas.
      /// </summary>
      protected override void CleanAreas()
      {
         if (Configuration.World.LoadWholeMap)
         {
            // don't clean when whole map is loaded
            return;
         }

         List<int> areas = new List<int>();
         lock (CollectionsLock)
         {
            areas.AddRange(Areas.Keys);
         }

         List<int> clientAreas = new List<int>();
         lock (_ClientLock)
         {
            clientAreas.AddRange(_ClientAreas.SelectMany(ca => ca.Value));
         }

         foreach (int key in clientAreas)
         {
            if (areas.Contains(key))
            {
               areas.Remove(key);
            }
         }

         // only no longer areas are left now
         foreach (int toRemove in areas)
         {
            ServerArea area = Areas[toRemove];
            if (!area.IsInitialized || area.Key < 0)
            {
               // skip not loaded area
               continue;
            }

            // save changed
            if (area.IsChanged)
            {
               SaveArea(area);
            }

            // pool and unload
            lock (CollectionsLock)
            {
               int segmentCount = area.GetSegmentCount();
               for (int i = 0; i < segmentCount; i++)
               {
                  ServerSegment segment = area.GetSegment<ServerSegment>(ref i);
                  foreach (ServerBox box in segment.GetBoxesSynchronized())
                  {
                     _PooledBoxes.Enqueue(box);
                  }
                  segment.Unload();
                  Segments.Remove(segment.Key);

                  _PooledSegments.Enqueue(segment);
               }
               area.Unload();
               Areas.Remove(area.Key);
            }
         }
      }

      internal ServerConfiguration Configuration
      {
         get
         {
            return _Configuration;
         }
      }

      internal ushort GetPlayerId(string name)
      {
         List<Player> players = new List<Player>(Entities.Values.Where(e => e is Player).Cast<Player>());
         Player player = players.FirstOrDefault(p => p.PlayerName == name);
         if (player == null)
         {
            // create and save new player with given name
            player = new Player(Configuration.World.NewPlayerSpawnPoint, name);
            Entities.Add(player.ID, player);
            Save(false);
         }

         return player.ID;
      }

      private Box GetPooledBox()
      {
         if (_PooledBoxes.Count > 0)
         {
            return _PooledBoxes.Dequeue() ?? new ServerBox();
         }

         return new ServerBox();
      }

      private Segment GetPooledSegment()
      {
         if (_PooledSegments.Count > 0)
         {
            return _PooledSegments.Dequeue() ?? new ServerSegment();
         }

         return new ServerSegment();
      }

      /// <summary>
      /// Gets a segment based on key.
      /// </summary>
      /// <param name="key">Segment key</param>
      /// <returns>Segment by key</returns>
      protected override Segment GetSegment(ref int key)
      {
         lock (CollectionsLock)
         {
            if (!Segments.ContainsKey(key))
            {
               return null;
            }
            return Segments[key];
         }
      }

      /// <summary>
      /// Gets the singleton isntance.
      /// </summary>
      public static ServerWorldManager Instance
      {
         get
         {
            return _Instance;
         }
      }

      internal bool IsPlayersArea(Player player, int key)
      {
         lock (_ClientLock)
         {
            if (_ClientAreas.ContainsKey(player.ID))
            {
               return _ClientAreas[player.ID].Contains(key);
            }
         }

         return false;
      }

      internal bool IsPlayerConnected(Player player)
      {
         if (player == null)
         {
            return false;
         }

         lock (_ClientLock)
         {
            return ServerManager.Instance.IsPlayerConnected(player.ID);
         }
      }

      internal void LoadClientColumns(int messageId, Player player)
      {
         lock (_ClientLock)
         {
            if (!_ClientAreas.ContainsKey(player.ID))
            {
               _ClientAreas.Add(player.ID, new List<int>());
            }
         }

         LoadPlayerColumns(messageId, player, player.CurrentColumnX, player.CurrentColumnZ);
      }

      private void KickPlayer(string name)
      {
         Connection connection = ServerManager.Instance.GetAuthenticatedPlayers().FirstOrDefault(c => c.Player.PlayerName == name);
         if (connection != null)
         {
            ServerManager.Instance.ServerToClientProvider.DisconnectingNotification(connection, "You have been kicked from the server!");
            connection.NetConnection.Disconnect("You have been kicked from the server!");

            Console.WriteLine("Player kicked: " + name);
         }
         else
         {
            Console.WriteLine("Player not found: " + name);
         }
      }

      private void ListPlayers()
      {
         Console.WriteLine(PlayersListHeader);
         foreach (var connection in ServerManager.Instance.GetAuthenticatedPlayers())
         {
            Console.WriteLine(PlayersListFormat, connection.NetConnection.RemoteEndPoint, connection.Player.ID, connection.Player.PlayerName);
         }
      }

      /// <summary>
      /// Runs any postprocessing that is needed after server loads new areas.
      /// </summary>
      /// <param name="loadRequest">Load request instance</param>
      /// <param name="areas">New areas</param>
      /// <param name="segments">New segments</param>
      protected override void OnAreasLoaded(LoadRequest loadRequest, List<ServerArea> areas, List<ServerSegment> segments)
      {
         if (loadRequest.MessageId < 0 || loadRequest.Entity == null)
         {
            return;
         }

         // the newly loaded areas/segments aren't usable as they could have been
         // covered by other players' terrain
         areas.Clear();
         segments.Clear();

         // construct player's area keys
         List<int> areaKeys = new List<int>();
         foreach (Tuple<int, int> column in GetEntityColumns(loadRequest.ColumnX, loadRequest.ColumnZ))
         {
            for (int y = 0; y < 256; y += 64)
            {
               areaKeys.Add(Area.GenerateKey(column.Item1, y, column.Item2));
            }
         }

         // retrieve player connection
         Connection connection = ServerManager.Instance.GetPlayerConnection(loadRequest.Entity.ID);
         if (connection == null)
         {
            // no longer online
            return;
         }

         // process terrain response in a synced and handled task
         HandledTask task = new HandledTask(() =>
         {
            // compare the new keys with current ones
            List<int> currentKeys = new List<int>();
            lock (_ClientLock)
            {
               currentKeys = _ClientAreas[loadRequest.Entity.ID];
            }

            lock (CollectionsLock)
            {
               foreach (int key in areaKeys.Where(a => !currentKeys.Contains(a)))
               {
                  areas.Add(Areas[key]);
               }
            }

            lock (_ClientLock)
            {
               _ClientAreas[loadRequest.Entity.ID] = areaKeys;
            }

            if (areas.Count == 0)
            {
               // there is actually nothing to send
               ServerManager.Instance.ServerToClientProvider.TerrainDataResponse(connection, loadRequest.MessageId, new List<ServerArea>(), (byte)0, (byte)0);
            }
            else
            {
               // send the terrain data to the requesting client
               int batchSize = Configuration.Network.MaxTerrainBatchSize;
               byte current = 1;
               byte total = (byte)Math.Ceiling((float)areas.Count / (float)batchSize);
               for (int i = 0; i < areas.Count; i += batchSize)
               {
                  List<ServerArea> partial = areas.Skip(i).Take(batchSize).ToList();
                  ServerManager.Instance.ServerToClientProvider.TerrainDataResponse(connection, loadRequest.MessageId, partial, current, total);
                  current++;
               }
            }
         });
         task.Start();
      }

      /// <summary>
      /// Runs any postprocessing that is needed after a reset.
      /// </summary>
      protected override void OnReset()
      {
         _ClientAreas.Clear();
      }

      private void ReadConsoleCommand()
      {
         string command = Console.ReadLine().Trim();
         if (string.IsNullOrEmpty(command))
         {
            return;
         }

         string[] commandData = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
         try
         {
            switch (commandData[0].ToLower())
            {
               case CommandBroadcast:
                  string message = "<SERVER>: " + command.Substring(command.IndexOf(' ') + 1);
                  foreach (Connection receiver in ServerManager.Instance.GetAuthenticatedPlayers())
                  {
                     ServerManager.Instance.ServerToClientProvider.ChatMessage(receiver, message);
                  }
                  break;
               case CommandExit:
                  _Running = false;
                  ServerManager.Instance.Stop();
                  Save(false);
                  StopWorkerThreads();
                  break;
               case CommandPlayers:
                  ListPlayers();
                  break;
               case CommandSave:
                  Save(false);
                  break;
               case CommandSaveAll:
                  Save(true);
                  break;
               case CommandKick:
                  KickPlayer(commandData[1]);
                  break;
               default:
                  Console.WriteLine("Unknown command: " + command);
                  break;
            }
         }
         catch (Exception ex)
         {
            ExceptionHandler.LogException(ex);
         }
      }

      /// <summary>
      /// Starts the server.
      /// </summary>
      public void Start()
      {
         // load the world
         Load(true);
         if (Configuration.World.LoadWholeMap)
         {
            LoadWholeMap();
         }

         // start the network server
         Console.WriteLine("Initializing server socket...");
         ServerManager.Instance.Start();

         Console.WriteLine("Server started");

         do
         {
            // read console commands
            ReadConsoleCommand();
         } while (_Running);
      }

      internal void UnloadPlayer(Player disconnectingPlayer)
      {
         lock (_ClientLock)
         {
            if (!_ClientAreas.ContainsKey(disconnectingPlayer.ID))
            {
               return;
            }
            _ClientAreas.Remove(disconnectingPlayer.ID);
         }
         CleanAreas();
      }

      internal Entity PrepareConnectedPlayer(ushort entityId)
      {
         lock (_ClientLock)
         {
            _ClientAreas[entityId] = new List<int>();
         }

         return Entities[entityId];
      }
   }
}
