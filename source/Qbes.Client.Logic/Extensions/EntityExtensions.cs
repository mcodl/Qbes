using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tao.OpenGl;

using Qbes.Common.Logic;

namespace Qbes.Client.Logic.Extensions
{
   internal static class EntityExtensions
   {
      internal static void Draw(this Entity toDraw, Player player)
      {
         if (toDraw.ID == player.ID)
         {
            // don't draw self
            return;
         }

         Point3D shifted = ClientWorldManager.Instance.TempPoint;
         shifted.Set(toDraw.Location);

         // apply possible shift corrections
         if (player.Location.X - shifted.X > WorldHelper.HalfSizeX)
         {
            shifted.X += WorldHelper.SizeX;
         }
         else if (shifted.X - player.Location.X > WorldHelper.HalfSizeX)
         {
            shifted.X -= WorldHelper.SizeX;
         }

         if (player.Location.Z - shifted.Z > WorldHelper.HalfSizeZ)
         {
            shifted.Z += WorldHelper.SizeZ;
         }
         else if (shifted.Z - player.Location.Z > WorldHelper.HalfSizeZ)
         {
            shifted.Z -= WorldHelper.SizeZ;
         }

         if (player.Location.GetDistanceSquare(shifted) > ClientWorldManager.Instance.RenderDistance)
         {
            // don't draw too distant entities
            return;
         }

         if (toDraw is Player && toDraw.SkinTextureName <= 0)
         {
            // attempt to get texture
            toDraw.SkinTextureName = TextureManager.Instance.GetSkinTextureName(((Player)toDraw).SkinChecksum, true, Gl.GL_QUADS);
         }

         // calculate points for drawing
         Point3D[] boxPoints = new Point3D[]
         {
            new Point3D(shifted.X - toDraw.HalfSizeX, shifted.Y, shifted.Z - toDraw.HalfSizeX),
            new Point3D(shifted.X - toDraw.HalfSizeX, shifted.Y, shifted.Z + toDraw.HalfSizeX),
            new Point3D(shifted.X + toDraw.HalfSizeX, shifted.Y, shifted.Z - toDraw.HalfSizeX),
            new Point3D(shifted.X + toDraw.HalfSizeX, shifted.Y, shifted.Z + toDraw.HalfSizeX),
         };

         foreach (Point3D boxPoint in boxPoints)
         {
            boxPoint.Rotate(shifted, toDraw.RotationLeft, 0.0f);
         }

         float x1a = boxPoints[0].X;
         float x1b = boxPoints[1].X;
         float x2a = boxPoints[2].X;
         float x2b = boxPoints[3].X;
         float y1 = shifted.Y - toDraw.HalfSizeY;
         float y2 = shifted.Y + toDraw.HalfSizeY;
         float z1a = boxPoints[0].Z;
         float z1b = boxPoints[2].Z;
         float z2a = boxPoints[1].Z;
         float z2b = boxPoints[3].Z;

         // draw the entity
         if (toDraw.SkinTextureName > 0)
         {
            TextureManager.Instance.ChangeTextureForQuads(toDraw.SkinTextureName);

            // left side
            Gl.glNormal3f(-1.0f, 0.0f, 0.0f);
            Gl.glTexCoord2f(0.25f, 0.796296f);
            Gl.glVertex3f(x1a, y1, z1a);
            Gl.glTexCoord2f(0.25f, 0.203704f);
            Gl.glVertex3f(x1a, y2, z1a);
            Gl.glTexCoord2f(0.0f, 0.203704f);
            Gl.glVertex3f(x1b, y2, z2a);
            Gl.glTexCoord2f(0.0f, 0.796296f);
            Gl.glVertex3f(x1b, y1, z2a);

            // bottom side
            Gl.glNormal3f(0.0f, -1.0f, 0.0f);
            Gl.glTexCoord2f(0.5f, 0.796296f);
            Gl.glVertex3f(x1a, y1, z1a);
            Gl.glTexCoord2f(0.25f, 0.796296f);
            Gl.glVertex3f(x2a, y1, z1b);
            Gl.glTexCoord2f(0.25f, 1.0f);
            Gl.glVertex3f(x2b, y1, z2b);
            Gl.glTexCoord2f(0.5f, 1.0f);
            Gl.glVertex3f(x1b, y1, z2a);

            // front side
            Gl.glNormal3f(0.0f, 0.0f, -1.0f);
            Gl.glTexCoord2f(0.5f, 0.796296f);
            Gl.glVertex3f(x1a, y1, z1a);
            Gl.glTexCoord2f(0.5f, 0.203704f);
            Gl.glVertex3f(x1a, y2, z1a);
            Gl.glTexCoord2f(0.25f, 0.203704f);
            Gl.glVertex3f(x2a, y2, z1b);
            Gl.glTexCoord2f(0.25f, 0.796296f);
            Gl.glVertex3f(x2a, y1, z1b);

            // right side
            Gl.glNormal3f(1.0f, 0.0f, 0.0f);
            Gl.glTexCoord2f(0.5f, 0.796296f);
            Gl.glVertex3f(x2a, y1, z1b);
            Gl.glTexCoord2f(0.5f, 0.203704f);
            Gl.glVertex3f(x2a, y2, z1b);
            Gl.glTexCoord2f(0.75f, 0.203704f);
            Gl.glVertex3f(x2b, y2, z2b);
            Gl.glTexCoord2f(0.75f, 0.796296f);
            Gl.glVertex3f(x2b, y1, z2b);

            // top side
            Gl.glNormal3f(0.0f, 1.0f, 0.0f);
            Gl.glTexCoord2f(0.5f, 0.203704f);
            Gl.glVertex3f(x1a, y2, z1a);
            Gl.glTexCoord2f(0.25f, 0.203704f);
            Gl.glVertex3f(x2a, y2, z1b);
            Gl.glTexCoord2f(0.25f, 0.0f);
            Gl.glVertex3f(x2b, y2, z2b);
            Gl.glTexCoord2f(0.5f, 0.0f);
            Gl.glVertex3f(x1b, y2, z2a);

            // back
            Gl.glNormal3f(0.0f, 0.0f, 1.0f);
            Gl.glTexCoord2f(0.75f, 0.796296f);
            Gl.glVertex3f(x1b, y1, z2a);
            Gl.glTexCoord2f(0.75f, 0.203704f);
            Gl.glVertex3f(x1b, y2, z2a);
            Gl.glTexCoord2f(1.0f, 0.203704f);
            Gl.glVertex3f(x2b, y2, z2b);
            Gl.glTexCoord2f(1.0f, 0.796296f);
            Gl.glVertex3f(x2b, y1, z2b);
         }
         else
         {
            // texture not loaded yet
            Gl.glDisable(Gl.GL_TEXTURE_2D);

            // left side
            Gl.glNormal3f(-1.0f, 0.0f, 0.0f);
            Gl.glVertex3f(x1a, y1, z1a);
            Gl.glVertex3f(x1a, y2, z1a);
            Gl.glVertex3f(x1b, y2, z2a);
            Gl.glVertex3f(x1b, y1, z2a);

            // bottom side
            Gl.glNormal3f(0.0f, -1.0f, 0.0f);
            Gl.glVertex3f(x1a, y1, z1a);
            Gl.glVertex3f(x2a, y1, z1b);
            Gl.glVertex3f(x2b, y1, z2b);
            Gl.glVertex3f(x1b, y1, z2a);

            // front side
            Gl.glNormal3f(0.0f, 0.0f, -1.0f);
            Gl.glVertex3f(x1a, y1, z1a);
            Gl.glVertex3f(x1a, y2, z1a);
            Gl.glVertex3f(x2a, y2, z1b);
            Gl.glVertex3f(x2a, y1, z1b);

            // right side
            Gl.glNormal3f(1.0f, 0.0f, 0.0f);
            Gl.glVertex3f(x2a, y1, z1b);
            Gl.glVertex3f(x2a, y2, z1b);
            Gl.glVertex3f(x2b, y2, z2b);
            Gl.glVertex3f(x2b, y1, z2b);

            // top side
            Gl.glNormal3f(0.0f, 1.0f, 0.0f);
            Gl.glVertex3f(x1a, y2, z1a);
            Gl.glVertex3f(x2a, y2, z1b);
            Gl.glVertex3f(x2b, y2, z2b);
            Gl.glVertex3f(x1b, y2, z2a);

            // back
            Gl.glNormal3f(0.0f, 0.0f, 1.0f);
            Gl.glVertex3f(x1b, y1, z2a);
            Gl.glVertex3f(x1b, y2, z2a);
            Gl.glVertex3f(x2b, y2, z2b);
            Gl.glVertex3f(x2b, y1, z2b);

            Gl.glEnable(Gl.GL_TEXTURE_2D);
         }
      }
   }
}
