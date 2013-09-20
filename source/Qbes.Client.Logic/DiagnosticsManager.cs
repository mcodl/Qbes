using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Qbes.Client.Diagnostics;
using Qbes.Common.Logic;

namespace Qbes.Client.Logic
{
   internal static class DiagnosticsManager
   {
      #region Constants
      [Obsolete("Replaced with Qbes.Client.Diagnostics")]
      private const string ClearBox =
         "                                                                                ";
#if DIAG
      [Obsolete("Replaced with Qbes.Client.Diagnostics")]
      private const string OutputFormat =
         "\tAreas\tSgmts\tBoxes\tFaces\t\t\tA\tB\n" +
         "Loaded\t{0}\t{1}\t{2}\t{3}\t\tElimin.\t{21}\tA:{24} D:{23}\n" +
         "Rendrd\t{4}\t{5}\t{6}\t{7}\t\tPassed.\t{22}\t{25}\n" +
         "%\t{8:0.0}\t{9:0.0}\t{10:0.0}\t{11:0.0}\n" +
         "\t\t\t\t\t\tglBindTexture calls: {20}\n" +
         "X\tY\tZ\tDirection\t\t50 frames in {17} ms\n" +
         "{12:0.0}\t{13:0.0}\t{14:0.0}\t{15:0.0}° {16:0.0}°\t\taverage FPS {18:0.0}\n\n" +
         "Memory {19} MB\tPoint/vector operations A:{26} D:{27}";
#else
      [Obsolete("Replaced with Qbes.Client.Diagnostics")]
      private const string OutputFormat =
         "\tAreas\tSgmts\tBoxes\tFaces\n" +
         "Loaded\t{0}\t{1}\t{2}\t{3}\n\n" +
         "X\tY\tZ\tDirection\n" +
         "{4:0.0}\t{5:0.0}\t{6:0.0}\t{7:0.0}° {8:0.0}°\n\n" +
         "50 frames in {9} ms\t\taverage FPS {10:0.0}\n\n" +
         "Memory {11} MB";
#endif
      #endregion

      #region Static fields
      [Obsolete("Replaced with Qbes.Client.Diagnostics")]
      private static object _ConsoleLock = new object();
      private static FrmDiagnostics _Form;
      [Obsolete("Replaced with Qbes.Client.Diagnostics")]
      private static readonly List<string> _Messages = new List<string>();
      [Obsolete("Replaced with Qbes.Client.Diagnostics")]
      private static readonly ClientWorldManager _World = ClientWorldManager.Instance;
      #endregion

      #region Static constructors
      static DiagnosticsManager()
      {
         //for (int i = 0; i < 10; i++)
         //{
         //   _Messages.Add(string.Empty);
         //}
      }
      #endregion

      internal static int AreasLoaded { get; set; }

#if DIAG
      internal static int AreasRendered { get; set; }
#endif

      internal static int BoxesLoaded { get; set; }

#if DIAG
      internal static int BoxesRendered { get; set; }
#endif

#if DIAG
      internal static int EliminatedAreasBasicAngle { get; set; }

      internal static int EliminatedAreasBasicDistance { get; set; }

      internal static int EliminatedAreasClose { get; set; }

      internal static int EliminatedSegmentsBasicAngle { get; set; }

      internal static int EliminatedSegmentsBasicDistance { get; set; }

      internal static int EliminatedSegmentsCloseAngle { get; set; }

      internal static int EliminatedSegmentsCloseDistance { get; set; }
#endif

      internal static int FacesLoaded
      {
         get
         {
            return BoxesLoaded * 6;
         }
      }

#if DIAG
      internal static int FacesRendered { get; set; }

      internal static int GlBindTextureCalls { get; set; }

      internal static int GlGetErrorResult { get; set; }
#endif

#if DIAG
      internal static int PassedAreasClose { get; set; }

      internal static int PassedAreasBasic { get; set; }

      internal static int PassedSegmentsClose { get; set; }

      internal static int PassedSegmentsBasic { get; set; }
#endif

#if DIAG
      internal static void ResetRendered()
      {
         AreasRendered = 0;
         SegmentsRendered = 0;
         BoxesRendered = 0;
         FacesRendered = 0;

         GlBindTextureCalls = 0;

         EliminatedAreasClose = 0;
         PassedAreasClose = 0;

         EliminatedAreasBasicAngle = 0;
         EliminatedAreasBasicDistance = 0;
         PassedAreasBasic = 0;

         EliminatedSegmentsCloseAngle = 0;
         EliminatedSegmentsCloseDistance = 0;
         PassedSegmentsClose = 0;

         EliminatedSegmentsBasicAngle = 0;
         EliminatedSegmentsBasicDistance = 0;
         PassedSegmentsBasic = 0;

         Point3D.ResetOperationCounters();
      }
#endif

      internal static int SegmentsLoaded { get; set; }

#if DIAG
      internal static int SegmentsRendered { get; set; }
#endif

      internal static void ShowHideDiagnosticsWindow()
      {
         if (_Form == null)
         {
            _Form = new FrmDiagnostics();
            _Form.Show();
         }
         else
         {
            _Form.Close();
            _Form.Dispose();
            _Form = null;
         }
      }

      internal static void WriteMessage(string message)
      {
         //lock (_ConsoleLock)
         //{
         //   if (message.Length > 70)
         //   {
         //      message = message.Substring(0, 67) + "...";
         //   }
         //   _Messages.Add(DateTime.Now.ToString("HH:mm:ss") + " " + message);

         //   while (_Messages.Count > 10)
         //   {
         //      _Messages.RemoveAt(0);
         //   }

         //   for (int i = _Messages.Count - 1; i >= 0; i--)
         //   {
         //      Console.CursorLeft = 0;
         //      Console.CursorTop = i + 12;
         //      Console.Write(ClearBox);

         //      Console.CursorLeft = 0;
         //      Console.CursorTop = i + 12;
         //      Console.WriteLine(_Messages[i]);
         //   }
         //}

         if (_Form != null)
         {
            _Form.WriteMessage(message);
         }
         Console.WriteLine(message);
      }

      internal static void WriteMessage(string message, params object[] args)
      {
         WriteMessage(string.Format(message, args));
      }

      internal static void WriteStatistics(int time)
      {
//         lock (_ConsoleLock)
//         {
//            Console.CursorLeft = 0;
//            Console.CursorTop = 0;

//            for (int i = 0; i < 8; i++)
//            {
//               Console.Write(ClearBox);
//            }
//         }

//         double avgFps = 1000.0d / ((double)time / 50.0d);
//         int memory = Convert.ToInt32((double)Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024);

//         lock (_ConsoleLock)
//         {
//            Console.CursorLeft = 0;
//            Console.CursorTop = 0;

//#if DIAG
//            double areasPct = (double)AreasRendered / (double)AreasLoaded * 100.0d;
//            double sgmtsPct = (double)SegmentsRendered / (double)SegmentsLoaded * 100.0d;
//            double boxesPct = (double)BoxesRendered / (double)BoxesLoaded * 100.0d;
//            double facesPct = (double)FacesRendered / (double)FacesLoaded * 100.0d;
//            Console.WriteLine(OutputFormat,
//                              AreasLoaded, SegmentsLoaded, BoxesLoaded, FacesLoaded,
//                              AreasRendered, SegmentsRendered, BoxesRendered, FacesRendered + (FacesRendered > 50000 ? "!" : ""),
//                              areasPct, sgmtsPct, boxesPct, facesPct,
//                              _World.Location.X, _World.Location.Y, _World.Location.Z,
//                              _World.RotationY, _World.RotationX,
//                              time, avgFps,
//                              memory,
//                              GlBindTextureCalls,
//                              EliminatedAreaCloseDistance, PassedAreaCloseDistance,
//                              EliminatedBasicDistance, EliminatedBasicAngle, PassedBasic,
//                              Point3D.AngleOperations, Point3D.DistanceOperations);
//#else
//            Console.WriteLine(OutputFormat,
//                              AreasLoaded, SegmentsLoaded, BoxesLoaded, FacesLoaded,
//                              _World.Location.X, _World.Location.Y, _World.Location.Z,
//                              _World.RotationY, _World.RotationX,
//                              time, avgFps,
//                              memory);
//#endif
//         }

         DiagnosticData data = new DiagnosticData();

         #region World statistics
         data.AreasLoaded = AreasLoaded;
#if DIAG
         data.AreasRendered = AreasRendered;
#endif
         data.BoxesLoaded = BoxesLoaded;
#if DIAG
         data.BoxesPooled = ClientWorldManager.Instance.PooledBoxCount;
         data.BoxesRendered = BoxesRendered;
#endif
         data.FacesLoaded = FacesLoaded;
#if DIAG
         data.FacesRendered = FacesRendered;
#endif
         data.SegmentsLoaded = SegmentsLoaded;
#if DIAG
         data.SegmentsPooled = ClientWorldManager.Instance.PooledSegmentCount;
         data.SegmentsRendered = SegmentsRendered;
#endif
         #endregion

         #region Player data
         if (ClientWorldManager.Instance.Player.CurrentSegment != null)
         {
            data.CurrentSegmentX = ClientWorldManager.Instance.Player.CurrentSegment.X;
            data.CurrentSegmentY = ClientWorldManager.Instance.Player.CurrentSegment.Y;
            data.CurrentSegmentZ = ClientWorldManager.Instance.Player.CurrentSegment.Z;
            Segment diagonalNeighbour = ClientWorldManager.Instance.Player.GetCurrentSegmentNeighbour(6);
            data.DiagonalNeighbourX = diagonalNeighbour.X;
            data.DiagonalNeighbourY = diagonalNeighbour.Y;
            data.DiagonalNeighbourZ = diagonalNeighbour.Z;
         }
         data.PlayerDirectionLeft = ClientWorldManager.Instance.RotationY;
         data.PlayerDirectionUp = ClientWorldManager.Instance.RotationX;
         data.PlayerX = ClientWorldManager.Instance.Location.X;
         data.PlayerY = ClientWorldManager.Instance.Location.Y;
         data.PlayerZ = ClientWorldManager.Instance.Location.Z;
         Point3D checkpoint = ClientWorldManager.Instance.Player.PreviousCheckPoint;
         data.PreviousCheckpointX = checkpoint.X;
         data.PreviousCheckpointY = checkpoint.Y;
         data.PreviousCheckpointZ = checkpoint.Z;
         #endregion

         #region Viewport elimination
#if DIAG
         data.AwayAreasEliminatedAngle = EliminatedAreasBasicAngle;
         data.AwayAreasEliminatedDistance = EliminatedAreasBasicDistance;
         data.AwayAreasPassed = PassedAreasBasic;
         data.AwaySegmentsEliminatedAngle = EliminatedSegmentsBasicAngle;
         data.AwaySegmentsEliminatedDistance = EliminatedSegmentsBasicDistance;
         data.AwaySegmentsPassed = PassedSegmentsBasic;
         data.CloseAreasEliminated = EliminatedAreasClose;
         data.CloseAreasPassed = PassedAreasClose;
         data.CloseSegmentsEliminatedAngle = EliminatedSegmentsCloseAngle;
         data.CloseSegmentsEliminatedDistance = EliminatedSegmentsCloseDistance;
         data.CloseSegmentsPassed = PassedSegmentsClose;
#endif
         #endregion

         #region Performance
         data.FiftyFramesIn = time;
#if DIAG
         data.GlBindTextureCalls = GlBindTextureCalls;
         data.GlGetErrorResult = GlGetErrorResult;
#endif
         data.UsedMemory = Convert.ToInt32((double)Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024);
         #endregion

         #region Point/vector operations
#if DIAG
         data.Angle = Point3D.AngleCounter;
         data.ArithmethicOperators = Point3D.ArithmethicOperatorsCounter;
         data.Clone = Point3D.CloneCounter;
         data.ComparativeOperators = Point3D.ComparativeOperatorsCounter;
         data.Distance = Point3D.DistanceCounter;
         data.Intersect = Point3D.IntersectCounter;
         data.Rotation = Point3D.RotationCounter;
#endif
         #endregion

         if (_Form != null)
         {
            _Form.UpdateCounters(data);
         }
      }
   }
}
