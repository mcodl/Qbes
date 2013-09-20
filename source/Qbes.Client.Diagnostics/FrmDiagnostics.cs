using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Qbes.Client.Diagnostics
{
   public partial class FrmDiagnostics : Form
   {
      public FrmDiagnostics()
      {
         CheckForIllegalCrossThreadCalls = false;
         InitializeComponent();
      }

      private void Update(Label label, string newValue)
      {
         if (label.Text == newValue)
         {
            label.ForeColor = Color.Black;
            return;
         }
         else
         {
            label.ForeColor = Color.DarkGreen;
         }

         label.Text = newValue;
      }

      public void UpdateCounters(DiagnosticData data)
      {
         #region World statistics
         if (data.AreasLoaded.HasValue)
         {
            Update(lblLoadedAreas, data.AreasLoaded.Value.ToString());
         }
         if (data.AreasRendered.HasValue)
         {
            Update(lblRenderedAreas, data.AreasRendered.Value.ToString());
            Update(lblPercentAreas, ((double)data.AreasRendered / (double)data.AreasLoaded * 100.0d).ToString("0.0"));
         }
         if (data.BoxesLoaded.HasValue)
         {
            Update(lblLoadedBoxes, data.BoxesLoaded.Value.ToString());
         }
         if (data.BoxesPooled.HasValue)
         {
            Update(lblPoolBoxes, data.BoxesPooled.Value.ToString());
         }
         if (data.BoxesRendered.HasValue)
         {
            Update(lblRenderedBoxes, data.BoxesRendered.Value.ToString());
            Update(lblPercentBoxes, ((double)data.BoxesRendered / (double)data.BoxesLoaded * 100.0d).ToString("0.0"));
         }
         if (data.FacesLoaded.HasValue)
         {
            Update(lblLoadedFaces, data.FacesLoaded.Value.ToString());
         }
         if (data.FacesRendered.HasValue)
         {
            Update(lblRenderedFaces, data.FacesRendered.Value.ToString());
            Update(lblPercentFaces, ((double)data.FacesRendered / (double)data.FacesLoaded * 100.0d).ToString("0.0"));
         }
         if (data.SegmentsLoaded.HasValue)
         {
            Update(lblLoadedSegments, data.SegmentsLoaded.Value.ToString());
         }
         if (data.SegmentsPooled.HasValue)
         {
            Update(lblPoolSegments, data.SegmentsPooled.Value.ToString());
         }
         if (data.SegmentsRendered.HasValue)
         {
            Update(lblRenderedSegments, data.SegmentsRendered.Value.ToString());
            Update(lblPercentSegments, ((double)data.SegmentsRendered / (double)data.SegmentsLoaded * 100.0d).ToString("0.0"));
         }
         #endregion

         #region Player data
         if (data.CurrentSegmentX.HasValue)
         {
            Update(lblCurrentSegmentX, data.CurrentSegmentX.Value.ToString());
         }
         if (data.CurrentSegmentY.HasValue)
         {
            Update(lblCurrentSegmentY, data.CurrentSegmentY.Value.ToString());
         }
         if (data.CurrentSegmentZ.HasValue)
         {
            Update(lblCurrentSegmentZ, data.CurrentSegmentZ.Value.ToString());
         }
         if (data.DiagonalNeighbourX.HasValue)
         {
            Update(lblDiagonalNeighbourX, data.DiagonalNeighbourX.Value.ToString());
         }
         if (data.DiagonalNeighbourY.HasValue)
         {
            Update(lblDiagonalNeighbourY, data.DiagonalNeighbourY.Value.ToString());
         }
         if (data.DiagonalNeighbourZ.HasValue)
         {
            Update(lblDiagonalNeighbourZ, data.DiagonalNeighbourZ.Value.ToString());
         }
         if (data.PlayerDirectionLeft.HasValue)
         {
            Update(lblLeftRight, data.PlayerDirectionLeft.Value.ToString("0.0") + "°");
         }
         if (data.PlayerDirectionUp.HasValue)
         {
            Update(lblUpDown, data.PlayerDirectionUp.Value.ToString("0.0") + "°");
         }
         if (data.PlayerX.HasValue)
         {
            Update(lblX, data.PlayerX.Value.ToString("0.0"));
         }
         if (data.PlayerY.HasValue)
         {
            Update(lblY, data.PlayerY.Value.ToString("0.0"));
         }
         if (data.PlayerZ.HasValue)
         {
            Update(lblZ, data.PlayerZ.Value.ToString("0.0"));
         }
         if (data.PreviousCheckpointX.HasValue)
         {
            Update(lblPreviousCheckpointX, data.PreviousCheckpointX.Value.ToString("0.0"));
         }
         if (data.PreviousCheckpointY.HasValue)
         {
            Update(lblPreviousCheckpointY, data.PreviousCheckpointY.Value.ToString("0.0"));
         }
         if (data.PreviousCheckpointZ.HasValue)
         {
            Update(lblPreviousCheckpointZ, data.PreviousCheckpointZ.Value.ToString("0.0"));
         }
         #endregion

         #region Viewport elimination
         if (data.AwayAreasEliminatedAngle.HasValue)
         {
            Update(lblEliminatedAreasAwayAngle, data.AwayAreasEliminatedAngle.Value.ToString());
         }
         if (data.AwayAreasEliminatedDistance.HasValue)
         {
            Update(lblEliminatedAreasAwayDistance, data.AwayAreasEliminatedDistance.Value.ToString());
         }
         if (data.AwayAreasPassed.HasValue)
         {
            Update(lblPassedAreasAway, data.AwayAreasPassed.Value.ToString());
         }
         if (data.AwaySegmentsEliminatedAngle.HasValue)
         {
            Update(lblEliminatedSegmentsAwayAngle, data.AwaySegmentsEliminatedAngle.Value.ToString());
         }
         if (data.AwaySegmentsEliminatedDistance.HasValue)
         {
            Update(lblEliminatedSegmentsAwayDistance, data.AwaySegmentsEliminatedDistance.Value.ToString());
         }
         if (data.AwaySegmentsPassed.HasValue)
         {
            Update(lblPassedSegmentsAway, data.AwaySegmentsPassed.Value.ToString());
         }
         if (data.CloseAreasEliminated.HasValue)
         {
            Update(lblEliminatedAreasClose, data.CloseAreasEliminated.Value.ToString());
         }
         if (data.CloseAreasPassed.HasValue)
         {
            Update(lblPassedAreasClose, data.CloseAreasPassed.Value.ToString());
         }
         if (data.CloseSegmentsEliminatedAngle.HasValue)
         {
            Update(lblEliminatedSegmentsCloseAngle, data.CloseSegmentsEliminatedAngle.Value.ToString());
         }
         if (data.CloseSegmentsEliminatedDistance.HasValue)
         {
            Update(lblEliminatedSegmentsCloseDistance, data.CloseSegmentsEliminatedDistance.Value.ToString());
         }
         if (data.CloseSegmentsPassed.HasValue)
         {
            Update(lblPassedSegmentsClose, data.CloseSegmentsPassed.Value.ToString());
         }
         #endregion

         #region Performance
         if (data.FiftyFramesIn.HasValue)
         {
            Update(lbl50FramesIn, data.FiftyFramesIn.Value.ToString());
            Update(lblAverageFPS, (1000.0d / ((double)data.FiftyFramesIn / 50.0d)).ToString("0.0"));
         }
         if (data.GlBindTextureCalls.HasValue)
         {
            Update(lblGlBindTextureCalls, data.GlBindTextureCalls.Value.ToString());
         }
         if (data.GlGetErrorResult.HasValue)
         {
            Update(lblGlGetErrorResult, data.GlGetErrorResult.Value.ToString());
         }
         if (data.UsedMemory.HasValue)
         {
            Update(lblUsedMemory, data.UsedMemory.Value.ToString() + " MB");
         }
         #endregion

         #region Point/vector operations
         if (data.Angle.HasValue)
         {
            Update(lblPointAngle, data.Angle.Value.ToString());
         }
         if (data.ArithmethicOperators.HasValue)
         {
            Update(lblPointArithmethicOperator, data.ArithmethicOperators.Value.ToString());
         }
         if (data.Clone.HasValue)
         {
            Update(lblPointClone, data.Clone.Value.ToString());
         }
         if (data.ComparativeOperators.HasValue)
         {
            Update(lblPointComparisonOperator, data.ComparativeOperators.Value.ToString());
         }
         if (data.Distance.HasValue)
         {
            Update(lblPointDistance, data.Distance.Value.ToString());
         }
         if (data.Intersect.HasValue)
         {
            Update(lblPointIntersect, data.Intersect.Value.ToString());
         }
         if (data.Rotation.HasValue)
         {
            Update(lblPointRotation, data.Rotation.Value.ToString());
         }
         #endregion
      }

      public void WriteMessage(string message)
      {
         txtLog.Text += DateTime.Now.ToString("HH:mm:ss") + "\t" + message + Environment.NewLine;
         txtLog.Select(txtLog.Text.Length - 1, 0);
      }
   }
}
