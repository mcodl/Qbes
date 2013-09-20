using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Client.Diagnostics
{
   public struct DiagnosticData
   {
      #region World statistics
      public int? AreasLoaded { get; set; }
      
      public int? AreasRendered { get; set; }
      
      public int? BoxesLoaded { get; set; }
      
      public int? BoxesPooled { get; set; }
      
      public int? BoxesRendered { get; set; }
      
      public int? FacesLoaded { get; set; }
      
      public int? FacesRendered { get; set; }

      public int? SegmentsLoaded { get; set; }
      
      public int? SegmentsPooled { get; set; }
      
      public int? SegmentsRendered { get; set; }
      #endregion

      #region Player data
      public int? CurrentSegmentX { get; set; }

      public int? CurrentSegmentY { get; set; }

      public int? CurrentSegmentZ { get; set; }

      public int? DiagonalNeighbourX { get; set; }

      public int? DiagonalNeighbourY { get; set; }

      public int? DiagonalNeighbourZ { get; set; }

      public float? PlayerDirectionLeft { get; set; }

      public float? PlayerDirectionUp { get; set; }

      public float? PlayerX { get; set; }

      public float? PlayerY { get; set; }

      public float? PlayerZ { get; set; }

      public float? PreviousCheckpointX { get; set; }

      public float? PreviousCheckpointY { get; set; }

      public float? PreviousCheckpointZ { get; set; }
      #endregion

      #region Viewport elimination
      public int? AwayAreasEliminatedAngle { get; set; }

      public int? AwayAreasEliminatedDistance { get; set; }

      public int? AwayAreasPassed { get; set; }

      public int? AwaySegmentsEliminatedAngle { get; set; }

      public int? AwaySegmentsEliminatedDistance { get; set; }

      public int? AwaySegmentsPassed { get; set; }

      public int? CloseAreasEliminated { get; set; }

      public int? CloseAreasPassed { get; set; }

      public int? CloseSegmentsEliminatedAngle { get; set; }

      public int? CloseSegmentsEliminatedDistance { get; set; }

      public int? CloseSegmentsPassed { get; set; }
      #endregion

      #region Performance
      public int? FiftyFramesIn { get; set; }

      public int? GlBindTextureCalls { get; set; }

      public int? GlGetErrorResult { get; set; }

      public int? UsedMemory { get; set; }
      #endregion

      #region Point/vector operations
      public int? Angle { get; set; }

      public int? ArithmethicOperators { get; set; }

      public int? Clone { get; set; }

      public int? ComparativeOperators { get; set; }

      public int? Distance { get; set; }

      public int? Intersect { get; set; }

      public int? Rotation { get; set; }
      #endregion
   }
}
