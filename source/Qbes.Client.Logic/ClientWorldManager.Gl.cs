using System;
using System.Collections.Generic;
using System.Drawing;

using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using Tao.OpenGl;

using Qbes.Client.Logic.Extensions;
using Qbes.Common.Logic;
using Qbes.Common.Logic.Constants;
using Qbes.Common.Logic.Extensions;

namespace Qbes.Client.Logic
{
   public sealed partial class ClientWorldManager
   {
      private void DrawGLScene()
      {
         if (Player == null)
         {
            return;
         }

         // Clear Screen And Depth Buffer
         Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
         // Reset The Current Modelview Matrix
         Gl.glLoadIdentity();

         // select all areas
         List<ClientArea> areas = new List<ClientArea>();
         bool loaded = false;
         do
         {
            try
            {
               areas.AddRange(Areas.Values);
               loaded = true;
            }
            catch (Exception ex)
            {
               areas.Clear();
               DiagnosticsManager.WriteMessage("WARNING: Exception while selecting areas " + ex.Message);
            }
         } while (!loaded);

         HandlePlayerMovement();

         // set viewport
         _EyeLocation = new Point3D(_Player.Location, 0.0f, Player.EyeHeightFromCenter, 0.0f);
         Glu.gluLookAt(_EyeLocation.X, _EyeLocation.Y, _EyeLocation.Z,
                       _Player.NewDirection.X, _Player.NewDirection.Y, _Player.NewDirection.Z,
                       0.0, 512.0, 0.0);

         Gl.glEnable(Gl.GL_DEPTH_TEST);
         Gl.glEnable(Gl.GL_LIGHTING);
         Gl.glEnable(Gl.GL_LIGHT0);
         Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[] { -512.0f, 512.0f, -512.0f, 0.0f });

         Gl.glEnable(Gl.GL_TEXTURE_2D);
         Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);

         Gl.glColor3f(1.0f, 1.0f, 1.0f);

         // draw terrain
         Gl.glBegin(Gl.GL_QUADS);
         //Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
         //Gl.glEnableClientState(Gl.GL_COLOR_ARRAY);

         foreach (ClientArea area in areas)
         {
            if (area != null && area.IsReady && !area.IsFlaggedForRemoval /* &&
                area.VisibleSegments.Count > 0 */ )
            {
               area.Draw(_EyeLocation, _Player.NewDirection);
            }
         }

         Gl.glEnd();
         //Gl.glDisableClientState(Gl.GL_COLOR_ARRAY);
         //Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);

         // draw entities
         Gl.glBegin(Gl.GL_QUADS);

         foreach (Entity entity in new List<Entity>(Entities.Values))
         {
            if (entity != null)
            {
               entity.Draw(Player);
            }
         }

         Gl.glEnd();

         Gl.glDisable(Gl.GL_LIGHTING);
         Gl.glDisable(Gl.GL_LIGHT0);
         Gl.glDisable(Gl.GL_DEPTH_TEST);

         Gl.glBegin(Gl.GL_QUADS);

         // higlight surface under crosshair

         Gl.glEnd();

         if (_ShowHud)
         {
            // draw HUD
            DrawHud();
         }

         Gl.glFlush();
         //Gl.glFinish();

         _Frame++;
         if (_Frame == 50)
         {
            _Frame = 0;
            int endTime = Environment.TickCount;
            DiagnosticsManager.WriteStatistics(endTime - _DiagTime);
            _DiagTime = endTime;
         }
         _CurrentTime = Environment.TickCount;

#if DIAG
         DiagnosticsManager.GlGetErrorResult = Gl.glGetError();
         DiagnosticsManager.ResetRendered();
#endif
      }

      private void DrawHud()
      {
         Gl.glLoadIdentity();
         Gl.glOrtho(-100, 100, -100, 100, 0, 5);

         Gl.glDisable(Gl.GL_TEXTURE_2D);

         Gl.glColor3fv(Configuration.Customization.HudColor.ToArray());
         Gl.glLineWidth(2.0f);

         // draw crosshair
         Gl.glBegin(Gl.GL_LINES);

         Gl.glVertex3f(5.0f, 0.0f, 0.0f);
         Gl.glVertex3f(3.0f, 0.0f, 0.0f);
         Gl.glVertex3f(-5.0f, 0.0f, 0.0f);
         Gl.glVertex3f(-3.0f, 0.0f, 0.0f);
         Gl.glVertex3f(0.0f, 5.0f, 0.0f);
         Gl.glVertex3f(0.0f, 3.0f, 0.0f);
         Gl.glVertex3f(0.0f, -5.0f, 0.0f);
         Gl.glVertex3f(0.0f, -3.0f, 0.0f);

         Gl.glEnd();

         // draw selected material frame
         Gl.glBegin(Gl.GL_QUADS);

         Gl.glVertex3f(95.25f, -44.75f, 0.0f);
         Gl.glVertex3f(84.75f, -44.75f, 0.0f);
         Gl.glVertex3f(84.75f, -55.25f, 0.0f);
         Gl.glVertex3f(95.25f, -55.25f, 0.0f);

         Gl.glEnd();

         // draw selected material top face
         Gl.glEnable(Gl.GL_TEXTURE_2D);
         Gl.glColor3f(1.0f, 1.0f, 1.0f);

         Gl.glBegin(Gl.GL_QUADS);

         TextureManager.Instance.ChangeTextureForQuads(Material.GetMaterial(Player.SelectedMaterial)[Sides.BackY]);
         Gl.glTexCoord2f(1.0f, 0.0f);
         Gl.glVertex3f(95.0f, -45.0f, 0.0f);
         Gl.glTexCoord2f(0.0f, 0.0f);
         Gl.glVertex3f(85.0f, -45.0f, 0.0f);
         Gl.glTexCoord2f(0.0f, 1.0f);
         Gl.glVertex3f(85.0f, -55.0f, 0.0f);
         Gl.glTexCoord2f(1.0f, 1.0f);
         Gl.glVertex3f(95.0f, -55.0f, 0.0f);

         Gl.glEnd();

         // draw chat
         FontInfo font = TextureManager.Instance.GetFont("consolas");
         float x = -95.0f;
         float y = -55.0f;
         float maxXchat = -10.0f;
         float maxXtextBox = 80.0f;
         float size = 1.5f;

         // draw player's chat message editor line
         Gl.glColor3fv(Configuration.Customization.HudColor.ToArray());

         if (_ChatBoxActive)
         {
            DrawText(x - 3.0f, y, maxXtextBox, size, font, "> ");
         }
         DrawText(x, y, maxXtextBox, size, font, _CurrentChatMessage);

         // draw last 10 messages
         Gl.glColor3f(1.0f, 1.0f, 1.0f);
         for (int i = _ChatHistory.Count - 1; i >= 0 && i >= _ChatHistory.Count - 6; i--)
         {
            y += MeasureTextLines(x, maxXchat, size, _ChatHistory[i].Length) * size * font.HeightRatio;
            DrawText(x, y, maxXchat, size, font, _ChatHistory[i]);
         }

         Gl.glDisable(Gl.GL_TEXTURE_2D);
      }

      private void DrawText(float x, float y, float maxX, float size, FontInfo font, string text)
      {
         // text = text.RemoveDiacritics();
         float height = size * font.HeightRatio;

         Gl.glBegin(Gl.GL_QUADS);
         TextureManager.Instance.ChangeTextureForQuads(font.TextureName);

         float currentX = x;
         float currentY = y;

         for (int i = 0; i < text.Length; i++)
         {
            char c = text[i];
            if (c < 32 || c > 127)
            {
               c = ' ';
            }
            float[] texCoords = font.GetTexCoords(c);

            Gl.glTexCoord2f(texCoords[0], texCoords[3]);
            Gl.glVertex3f(currentX, currentY, 0.0f);
            Gl.glTexCoord2f(texCoords[2], texCoords[3]);
            Gl.glVertex3f(currentX + size, currentY, 0.0f);
            Gl.glTexCoord2f(texCoords[2], texCoords[1]);
            Gl.glVertex3f(currentX + size, currentY + height, 0.0f);
            Gl.glTexCoord2f(texCoords[0], texCoords[1]);
            Gl.glVertex3f(currentX, currentY + height, 0.0f);

            currentX += size;
            if (currentX >= maxX)
            {
               currentY -= height;
               currentX = x;
            }
         }

         Gl.glEnd();
      }

      private void InitGL()
      {
         Console.WriteLine("Initializing OpenGL");

         // Reset The Current Viewport
         Gl.glViewport(0, 0, _Width, _Height);
         // Select The Projection Matrix
         Gl.glMatrixMode(Gl.GL_PROJECTION);
         // Reset The Projection Matrix
         Gl.glLoadIdentity();
         // Calculate The Aspect Ratio Of The Window
         Glu.gluPerspective(60.0F, (_Width / (float)_Height), 0.1F, RenderDistance);
         // Select The Modelview Matrix
         Gl.glMatrixMode(Gl.GL_MODELVIEW);
         // Reset The Modelview Matrix
         Gl.glLoadIdentity();

         // load textures
         if (_FirstTextureLoad)
         {
            TextureManager.Instance.LoadFonts();
            TextureManager.Instance.LoadTerrainTextures();
            TextureManager.Instance.LoadSkinTexture(_SkinHash.GetHexaString(), SkinManager.GetSkinFileName(ref _SkinHash));
            _FirstTextureLoad = false;
         }

         // Enable Smooth Shading
         Gl.glShadeModel(Gl.GL_SMOOTH);

         // sky color
         Gl.glClearColor(_SkyColor[0], _SkyColor[1], _SkyColor[2], _SkyColor[3]);

         // Depth Buffer Setup
         Gl.glClearDepth(1.0f);

         // Set light
         Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, new float[] { 0.8f, 0.8f, 0.8f, 1.0f });
         Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT, new float[] { 0.8f, 0.8f, 0.8f, 1.0f });
         Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SPECULAR, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
         Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SHININESS, new float[] { 0.0f });
         Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, new float[] { 0.8f, 0.8f, 0.8f, 1.0f });
         Gl.glLightModeli(Gl.GL_LIGHT_MODEL_LOCAL_VIEWER, Gl.GL_TRUE);

         // set fog
         Gl.glFogi(Gl.GL_FOG_MODE, Gl.GL_LINEAR);
         Gl.glFogfv(Gl.GL_FOG_COLOR, _SkyColor);
         Gl.glFogf(Gl.GL_FOG_DENSITY, 0.05f);
         Gl.glHint(Gl.GL_FOG_HINT, Gl.GL_DONT_CARE);
         Gl.glFogf(Gl.GL_FOG_START, 1.0f);
         Gl.glFogf(Gl.GL_FOG_END, Convert.ToSingle(Math.Sqrt(RenderDistance)));

         if (_Fog)
         {
            Gl.glEnable(Gl.GL_FOG);
         }
         else
         {
            Gl.glDisable(Gl.GL_FOG);
         }

         // enable color material
         Gl.glEnable(Gl.GL_COLOR_MATERIAL);

         // generic material behaviour
         Gl.glColorMaterial(Gl.GL_FRONT, Gl.GL_DIFFUSE);

         // depth testing function
         Gl.glDepthFunc(Gl.GL_LEQUAL);

         // perspective correction setting
         Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_FASTEST);

         // disable dithering
         Gl.glDisable(Gl.GL_DITHER);

         Console.WriteLine("OpenGL initialized");

         _DiagTime = Environment.TickCount;

         // Sets the ticker to update OpenGL Context
         Events.Tick += new EventHandler<TickEventArgs>(Tick);
      }

      private int MeasureTextLines(float x, float maxX, float size, int textLength)
      {
         int lines = 1;

         // calculate lines
         float lineWidth = Math.Abs(maxX - x);
         float textWidth = textLength * size;
         lines += Convert.ToInt32(Math.Floor((double)textWidth / (double)lineWidth));

         return lines;
      }

      private void ToggleFog()
      {
         _Fog = !_Fog;
         if (_Fog)
         {
            Gl.glEnable(Gl.GL_FOG);
         }
         else
         {
            Gl.glDisable(Gl.GL_FOG);
         }
      }
   }
}
