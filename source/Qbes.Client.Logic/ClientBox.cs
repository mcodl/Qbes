using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tao.OpenGl;

using Qbes.Common.Logic;
using Qbes.Common.Logic.Constants;
using Qbes.Common.Logic.DataStructures;

namespace Qbes.Client.Logic
{
   /// <summary>
   /// Client box adds rendering related logic to the Box class.
   /// </summary>
   public sealed class ClientBox : Box
   {
      #region Static fields
      private static Point3D _HelperPoint = new Point3D();
      private static byte[] _Indices = new byte[]
      {
            0, 1, 2, 3, // front X
            0, 4, 7, 3, // front Y
            0, 1, 5, 4, // front Z
            4, 5, 6, 7, // back X
            1, 5, 6, 2, // back Y
            3, 2, 6, 7  // back Z
      };
      #endregion

      #region Fields
      private BinaryArrayInByte _HiddenSides = new BinaryArrayInByte();
      private Material _Material;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default client box instance.
      /// </summary>
      public ClientBox()
         : base()
      {
         // UpdateSidePoints();
      }

      /// <summary>
      /// Creates a box from given cube and segment owner.
      /// </summary>
      /// <param name="cube">Base cube</param>
      /// <param name="segment">Segment owner</param>
      public ClientBox(Cube cube, ClientSegment segment)
         : base(cube, segment)
      {
         OnInitialized();
      }
      #endregion

      internal void Draw(Point3D location, ref int shiftX, ref int shiftZ)
      {
         _HelperPoint.X = CenterPoint.X + shiftX;
         _HelperPoint.Y = CenterPoint.Y;
         _HelperPoint.Z = CenterPoint.Z + shiftZ;
         float centerDistance = location.GetDistanceSquare(_HelperPoint);

         // Gl.glColor4f(_Material.Red, _Material.Green, _Material.Blue, _Material.Alpha);

         if (!_HiddenSides[Sides.FrontX])
         {
            _HelperPoint.X = X1 + shiftX;
            if (centerDistance > location.GetDistanceSquare(_HelperPoint))
            {
               TextureManager.Instance.ChangeTextureForQuads(_Material[Sides.FrontX]);

               Gl.glNormal3f(-1.0f, 0.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, 0.0f);
               Gl.glVertex3f(X1 + shiftX, Y1, Z1 + shiftZ);
               Gl.glTexCoord2f(0.0f, SizeY);
               Gl.glVertex3f(X1 + shiftX, Y2, Z1 + shiftZ);
               Gl.glTexCoord2f(SizeZ, SizeY);
               Gl.glVertex3f(X1 + shiftX, Y2, Z2 + shiftZ);
               Gl.glTexCoord2f(SizeZ, 0.0f);
               Gl.glVertex3f(X1 + shiftX, Y1, Z2 + shiftZ);

#if DIAG
               DiagnosticsManager.FacesRendered++;
#endif
            }
            _HelperPoint.X = CenterPoint.X + shiftX;
         }

         if (!_HiddenSides[Sides.FrontY])
         {
            _HelperPoint.Y = Y1;
            if (centerDistance > location.GetDistanceSquare(_HelperPoint))
            {
               TextureManager.Instance.ChangeTextureForQuads(_Material[Sides.FrontY]);

               Gl.glNormal3f(0.0f, -1.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, 0.0f);
               Gl.glVertex3f(X1 + shiftX, Y1, Z1 + shiftZ);
               Gl.glTexCoord2f(0.0f, SizeX);
               Gl.glVertex3f(X2 + shiftX, Y1, Z1 + shiftZ);
               Gl.glTexCoord2f(SizeZ, SizeX);
               Gl.glVertex3f(X2 + shiftX, Y1, Z2 + shiftZ);
               Gl.glTexCoord2f(SizeZ, 0.0f);
               Gl.glVertex3f(X1 + shiftX, Y1, Z2 + shiftZ);

#if DIAG
               DiagnosticsManager.FacesRendered++;
#endif
            }
            _HelperPoint.Y = CenterPoint.Y;
         }

         if (!_HiddenSides[Sides.FrontZ])
         {
            _HelperPoint.Z = Z1 + shiftZ;
            if (centerDistance > location.GetDistanceSquare(_HelperPoint))
            {
               TextureManager.Instance.ChangeTextureForQuads(_Material[Sides.FrontZ]);

               Gl.glNormal3f(0.0f, 0.0f, -1.0f);
               Gl.glTexCoord2f(0.0f, 0.0f);
               Gl.glVertex3f(X1 + shiftX, Y1, Z1 + shiftZ);
               Gl.glTexCoord2f(0.0f, SizeY);
               Gl.glVertex3f(X1 + shiftX, Y2, Z1 + shiftZ);
               Gl.glTexCoord2f(SizeX, SizeY);
               Gl.glVertex3f(X2 + shiftX, Y2, Z1 + shiftZ);
               Gl.glTexCoord2f(SizeX, 0.0f);
               Gl.glVertex3f(X2 + shiftX, Y1, Z1 + shiftZ);

#if DIAG
               DiagnosticsManager.FacesRendered++;
#endif
            }
            _HelperPoint.Z = CenterPoint.Z + shiftZ;
         }

         if (!_HiddenSides[Sides.BackX])
         {
            _HelperPoint.X = X2 + shiftX;
            if (centerDistance > location.GetDistanceSquare(_HelperPoint))
            {
               TextureManager.Instance.ChangeTextureForQuads(_Material[Sides.BackX]);

               Gl.glNormal3f(1.0f, 0.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, 0.0f);
               Gl.glVertex3f(X2 + shiftX, Y1, Z1 + shiftZ);
               Gl.glTexCoord2f(0.0f, SizeY);
               Gl.glVertex3f(X2 + shiftX, Y2, Z1 + shiftZ);
               Gl.glTexCoord2f(SizeZ, SizeY);
               Gl.glVertex3f(X2 + shiftX, Y2, Z2 + shiftZ);
               Gl.glTexCoord2f(SizeZ, 0.0f);
               Gl.glVertex3f(X2 + shiftX, Y1, Z2 + shiftZ);

#if DIAG
               DiagnosticsManager.FacesRendered++;
#endif
            }
            _HelperPoint.X = CenterPoint.X + shiftX;
         }

         if (!_HiddenSides[Sides.BackY])
         {
            _HelperPoint.Y = Y2;
            if (centerDistance > location.GetDistanceSquare(_HelperPoint))
            {
               TextureManager.Instance.ChangeTextureForQuads(_Material[Sides.BackY]);

               Gl.glNormal3f(0.0f, 1.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, 0.0f);
               Gl.glVertex3f(X1 + shiftX, Y2, Z1 + shiftZ);
               Gl.glTexCoord2f(0.0f, SizeX);
               Gl.glVertex3f(X2 + shiftX, Y2, Z1 + shiftZ);
               Gl.glTexCoord2f(SizeZ, SizeX);
               Gl.glVertex3f(X2 + shiftX, Y2, Z2 + shiftZ);
               Gl.glTexCoord2f(SizeZ, 0.0f);
               Gl.glVertex3f(X1 + shiftX, Y2, Z2 + shiftZ);

#if DIAG
               DiagnosticsManager.FacesRendered++;
#endif
            }
            _HelperPoint.Y = CenterPoint.Y;
         }

         if (!_HiddenSides[Sides.BackZ])
         {
            _HelperPoint.Z = Z2 + shiftZ;
            if (centerDistance > location.GetDistanceSquare(_HelperPoint))
            {
               TextureManager.Instance.ChangeTextureForQuads(_Material[Sides.BackZ]);

               Gl.glNormal3f(0.0f, 0.0f, 1.0f);
               Gl.glTexCoord2f(0.0f, 0.0f);
               Gl.glVertex3f(X1 + shiftX, Y1, Z2 + shiftZ);
               Gl.glTexCoord2f(0.0f, SizeY);
               Gl.glVertex3f(X1 + shiftX, Y2, Z2 + shiftZ);
               Gl.glTexCoord2f(SizeX, SizeY);
               Gl.glVertex3f(X2 + shiftX, Y2, Z2 + shiftZ);
               Gl.glTexCoord2f(SizeX, 0.0f);
               Gl.glVertex3f(X2 + shiftX, Y1, Z2 + shiftZ);

#if DIAG
               DiagnosticsManager.FacesRendered++;
#endif
            }
            // no need to adjust, this is the last one
         }

#if DIAG
         DiagnosticsManager.BoxesRendered++;
#endif
      }

      internal void DrawRangeElements(Point3D location, ref int shiftX, ref int shiftZ)
      {
         float[] vertices = new float[]
         {
            X1 + shiftX, Y1, Z1 + shiftZ,
            X1 + shiftX, Y2, Z1 + shiftZ,
            X1 + shiftX, Y2, Z2 + shiftZ,
            X1 + shiftX, Y1, Z2 + shiftZ,
            X2 + shiftX, Y1, Z1 + shiftZ,
            X2 + shiftX, Y2, Z1 + shiftZ,
            X2 + shiftX, Y2, Z2 + shiftZ,
            X2 + shiftX, Y1, Z2 + shiftZ
         };
         float[] colors = new float[]
         {
            1, 1, 1,   1, 1, 0,   1, 0, 0,
            1, 1, 1,   1, 0, 1,   0, 0, 1,
            1, 1, 1,   0, 1, 1,   0, 1, 0,
            1, 1, 0,   0, 1, 0,   0, 0, 0,
            1, 0, 0,   1, 0, 0,   1, 1, 1,
            0, 0, 0,   0, 0, 1,   1, 0, 1,
            0, 0, 1,   0, 0, 0,   0, 1, 0,
            0, 1, 1,   1, 1, 1,   1, 1, 0,
         };

         Gl.glColorPointer(3, Gl.GL_FLOAT, 0, colors);
         Gl.glVertexPointer(3, Gl.GL_FLOAT, 0, vertices);

         Gl.glDrawRangeElements(Gl.GL_QUADS, 0, 23, 24, Gl.GL_UNSIGNED_BYTE, _Indices);
      }

      internal BinaryArrayInByte HiddenSides
      {
         get
         {
            return _HiddenSides;
         }
      }

      /// <summary>
      /// This method is called when the box initialization is complete
      /// </summary>
      protected override void OnInitialized()
      {
         _HiddenSides = new BinaryArrayInByte();
         _Material = Material.GetMaterial(MaterialId);
      }
   }
}
