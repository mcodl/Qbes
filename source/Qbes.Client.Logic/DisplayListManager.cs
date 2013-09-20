using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Constants;

using Tao.OpenGl;

namespace Qbes.Client.Logic
{
   [Obsolete("Display lists proved to hinder performance")]
   internal sealed class DisplayListManager
   {
      #region Constants
      private const int ListCount = 384;
      #endregion

      #region Static fields
      private static readonly DisplayListManager _Instance = new DisplayListManager();
      #endregion

      #region Fields
      private int[] _ListNames = new int[ListCount];
      #endregion

      #region Constructors
      private DisplayListManager()
      {
         // empty
      }

      static DisplayListManager()
      {
         // empty
      }
      #endregion

      private void CompileList(int xSize, int ySize, int zSize, int side)
      {
         Gl.glNewList(GetListName(xSize, ySize, zSize, side), Gl.GL_COMPILE);
         Gl.glBegin(Gl.GL_QUADS);

         switch (side)
         {
            case Sides.FrontX:
               Gl.glNormal3f(-1.0f, 0.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(0.0f, 0.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, ySize); Gl.glVertex3f(0.0f, ySize, 0.0f);
               Gl.glTexCoord2f(zSize, ySize); Gl.glVertex3f(0.0f, ySize, zSize);
               Gl.glTexCoord2f(zSize, 0.0f); Gl.glVertex3f(0.0f, 0.0f, zSize);
               break;
            case Sides.FrontY:
               Gl.glNormal3f(0.0f, -1.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(0.0f, 0.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, xSize); Gl.glVertex3f(xSize, 0.0f, 0.0f);
               Gl.glTexCoord2f(zSize, xSize); Gl.glVertex3f(xSize, 0.0f, zSize);
               Gl.glTexCoord2f(zSize, 0.0f); Gl.glVertex3f(0.0f, 0.0f, zSize);
               break;
            case Sides.FrontZ:
               Gl.glNormal3f(0.0f, 0.0f, -1.0f);
               Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(0.0f, 0.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, ySize); Gl.glVertex3f(0.0f, ySize, 0.0f);
               Gl.glTexCoord2f(xSize, ySize); Gl.glVertex3f(xSize, ySize, 0.0f);
               Gl.glTexCoord2f(xSize, 0.0f); Gl.glVertex3f(xSize, 0.0f, 0.0f);
               break;
            case Sides.BackX:
               Gl.glNormal3f(1.0f, 0.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(0.0f, 0.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, ySize); Gl.glVertex3f(0.0f, -ySize, 0.0f);
               Gl.glTexCoord2f(zSize, ySize); Gl.glVertex3f(0.0f, -ySize, -zSize);
               Gl.glTexCoord2f(zSize, 0.0f); Gl.glVertex3f(0.0f, 0.0f, -zSize);
               break;
            case Sides.BackY:
               Gl.glNormal3f(0.0f, 1.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(0.0f, 0.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, xSize); Gl.glVertex3f(-xSize, 0.0f, 0.0f);
               Gl.glTexCoord2f(zSize, xSize); Gl.glVertex3f(-xSize, 0.0f, -zSize);
               Gl.glTexCoord2f(zSize, 0.0f); Gl.glVertex3f(0.0f, 0.0f, -zSize);
               break;
            case Sides.BackZ:
               Gl.glNormal3f(0.0f, 0.0f, 1.0f);
               Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(0.0f, 0.0f, 0.0f);
               Gl.glTexCoord2f(0.0f, ySize); Gl.glVertex3f(0.0f, -ySize, 0.0f);
               Gl.glTexCoord2f(xSize, ySize); Gl.glVertex3f(-xSize, -ySize, 0.0f);
               Gl.glTexCoord2f(xSize, 0.0f); Gl.glVertex3f(-xSize, 0.0f, 0.0f);
               break;
         }

         Gl.glEnd();
         Gl.glEndList();
      }


      private int GetIndex(ref int xSize, ref int ySize, ref int zSize, ref int side)
      {
         int position = side * 64;

         switch (side)
         {
            case Sides.BackX:
            case Sides.FrontX:
               position += (ySize - 1) * 8 + zSize - 1;
               break;
            case Sides.BackY:
            case Sides.FrontY:
               position += (xSize - 1) * 8 + zSize - 1;
               break;
            case Sides.BackZ:
            case Sides.FrontZ:
               position += (xSize - 1) * 8 + ySize - 1;
               break;
         }

         return position;
      }

      internal int GetListName(int xSize, int ySize, int zSize, int side)
      {
         return _ListNames[GetIndex(ref xSize, ref ySize, ref zSize, ref side)];
      }

      internal int GetListName(float xSize, float ySize, float zSize, int side)
      {
         return GetListName(Convert.ToInt32(xSize), Convert.ToInt32(ySize),
                            Convert.ToInt32(zSize), side);
      }

      internal void InitializeLists()
      {
         // prepare buffers
         int name = Gl.glGenLists(ListCount);
         for (int i = 0; i < ListCount; i++)
         {
            _ListNames[i] = name;
            name++;
         }

         // fill in buffers for X sides
         int staticX = 0;
         for (int side = Sides.FrontX; side <= Sides.BackX; side += 3)
         {
            for (int y = 1; y <= 8; y++)
            {
               for (int z = 1; z <= 8; z++)
               {
                  CompileList(staticX, y, z, side);
               }
            }
         }

         // fill in buffers for Y sides
         int staticY = 0;
         for (int side = Sides.FrontY; side <= Sides.BackY; side += 3)
         {
            for (int x = 1; x <= 8; x++)
            {
               for (int z = 1; z <= 8; z++)
               {
                  CompileList(x, staticY, z, side);
               }
            }
         }

         // fill in buffers for Z sides
         int staticZ = 0;
         for (int side = Sides.FrontZ; side <= Sides.BackZ; side += 3)
         {
            for (int x = 1; x <= 8; x++)
            {
               for (int y = 1; y <= 8; y++)
               {
                  CompileList(x, y, staticZ, side);
               }
            }
         }
      }

      internal static DisplayListManager Instance
      {
         get
         {
            return _Instance;
         }
      }
   }
}
