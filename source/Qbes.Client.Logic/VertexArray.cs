using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Tao.OpenGl;

using Qbes.Common.Logic.Constants;

namespace Qbes.Client.Logic
{
   [Obsolete("Vertex arrays proved to hinder performance")]
   internal sealed class VertexArray
   {
      #region Constants
      private const int BufferCount = 384;
      private const int Count = 4;
      private const int First = 0;
      private const int NormalsDataIndex = 20;
      private const int PositionDataIndex = 0;
      private const int Stride = 0;
      private const int TexCoordPointerSize = 2;
      private const int TextureDataIndex = 12;
      private const int TotalByteDataSize = TotalDataSize * 4;
      private const int TotalDataSize = 32;
      private const int VertexPointerSize = 3;
      #endregion

      #region Static fields
      private static VertexArray[] _Arrays = new VertexArray[BufferCount];
      private static int[] _BufferNames = new int[BufferCount];
      private static readonly IntPtr _NormalDataPointer = new IntPtr(NormalsDataIndex);
      private static readonly IntPtr _PositionDataPointer = new IntPtr(PositionDataIndex);
      private static readonly IntPtr _TextureDataPointer = new IntPtr(TextureDataIndex);
      #endregion

      #region Fields
      private float[] _Data;
      #endregion

      #region Constructors
      private VertexArray(int xSize, int ySize, int zSize, int side)
      {
         switch (side)
         {
            case Sides.FrontX:
               _Data = new float[]
               {
                  // front X vertices
                  0.0f, 0.0f, 0.0f,
                  0.0f, ySize, 0.0f,
                  0.0f, ySize, zSize,
                  0.0f, 0.0f, zSize,
                  // front X texcoords
                  0.0f, 0.0f,
                  0.0f, ySize,
                  zSize, ySize,
                  zSize, 0.0f,
                  // front X normals
                  -1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f
               };
               break;
            case Sides.FrontY:
               _Data = new float[]
               {
                  // front Y vertices
                  0.0f, 0.0f, 0.0f,
                  xSize, 0.0f, 0.0f,
                  xSize, 0.0f, zSize,
                  0.0f, 0.0f, zSize,
                  // front Y texcoords
                  0.0f, 0.0f,
                  0.0f, xSize,
                  zSize, xSize,
                  zSize, 0.0f,
                  // front Y normals
                  0.0f, -1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, -1.0f, 0.0f
               };
               break;
            case Sides.FrontZ:
               _Data = new float[]
               {
                  // front Z vertices
                  0.0f, 0.0f, 0.0f,
                  0.0f, ySize, 0.0f,
                  xSize, ySize, 0.0f,
                  xSize, 0.0f, 0.0f,
                  // front Z texcoords
                  0.0f, 0.0f,
                  0.0f, ySize,
                  xSize, ySize,
                  xSize, 0.0f,
                  // front Z normals
                  0.0f, 0.0f, -1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, -1.0f
               };
               break;
            case Sides.BackX:
               _Data = new float[]
               {
                  // back X vertices
                  0.0f, 0.0f, 0.0f,
                  0.0f, -ySize, 0.0f,
                  0.0f, -ySize, -zSize,
                  0.0f, 0.0f, -zSize,
                  // back X texcoords
                  0.0f, 0.0f,
                  0.0f, ySize,
                  zSize, ySize,
                  zSize, 0.0f,
                  // back X normals
                  1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f
               };
               break;
            case Sides.BackY:
               _Data = new float[]
               {
                  // back Y vertices
                  0.0f, 0.0f, 0.0f,
                  -xSize, 0.0f, 0.0f,
                  -xSize, 0.0f, -zSize,
                  0.0f, 0.0f, -zSize,
                  // back Y texcoords
                  0.0f, 0.0f,
                  0.0f, xSize,
                  zSize, xSize,
                  zSize, 0.0f,
                  // back Y normals
                  0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f
               };
               break;
            case Sides.BackZ:
               _Data = new float[]
               {
                  // back Z vertices
                  0.0f, 0.0f, 0.0f,
                  0.0f, -ySize, 0.0f,
                  -xSize, -ySize, 0.0f,
                  -xSize, 0.0f, 0.0f,
                  // back Z texcoords
                  0.0f, 0.0f,
                  0.0f, ySize,
                  xSize, ySize,
                  xSize, 0.0f,
                  // back Z normals
                  0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f
               };
               break;
         }
      }
      #endregion

      internal static void DrawVertexArray(int bufferName)
      {
         Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, bufferName);

         Gl.glVertexPointer(VertexPointerSize, Gl.GL_FLOAT, Stride, _PositionDataPointer);
         Gl.glTexCoordPointer(TexCoordPointerSize, Gl.GL_FLOAT, Stride, _TextureDataPointer);
         Gl.glNormalPointer(Gl.GL_FLOAT, Stride, _NormalDataPointer);

         Gl.glDrawArrays(Gl.GL_QUADS, First, Count);
      }

      internal static void DrawVertexArray(ref int xSize, ref int ySize, ref int zSize, ref int side)
      {
         int index = GetLocation(ref xSize, ref ySize, ref zSize, ref side);

         Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _BufferNames[index]);

         Gl.glVertexPointer(VertexPointerSize, Gl.GL_FLOAT, Stride, _PositionDataPointer);
         Gl.glTexCoordPointer(TexCoordPointerSize, Gl.GL_FLOAT, Stride, _TextureDataPointer);
         Gl.glNormalPointer(Gl.GL_FLOAT, Stride, _NormalDataPointer);

         Gl.glDrawArrays(Gl.GL_QUADS, First, Count);
      }

      internal static int GetBufferName(int xSize, int ySize, int zSize, int side)
      {
         return _BufferNames[GetLocation(ref xSize, ref ySize, ref zSize, ref side)];
      }

      internal static int GetBufferName(float xSize, float ySize, float zSize, int side)
      {
         return GetBufferName(Convert.ToInt32(xSize), Convert.ToInt32(ySize),
                              Convert.ToInt32(zSize), side);
      }

      private static int GetLocation(ref int xSize, ref int ySize, ref int zSize, ref int side)
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

      internal static void InitializeBuffers()
      {
         // prepare buffers
         Gl.glGenBuffers(BufferCount, _BufferNames);

         // fill in buffers for X sides
         int staticX = 0;
         for (int side = Sides.FrontX; side <= Sides.BackX; side += 3)
         {
            for (int y = 1; y <= 8; y++)
            {
               for (int z = 1; z <= 8; z++)
               {
                  int index = GetLocation(ref staticX, ref y, ref z, ref side);
                  _Arrays[index] = new VertexArray(staticX, y, z, side);
                  Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _BufferNames[index]);

                  Gl.glBufferData(Gl.GL_ARRAY_BUFFER, new IntPtr(TotalByteDataSize),
                                  _Arrays[index]._Data, Gl.GL_STREAM_DRAW);
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
                  int index = GetLocation(ref x, ref staticY, ref z, ref side);
                  _Arrays[index] = new VertexArray(x, staticY, z, side);
                  Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _BufferNames[index]);

                  Gl.glBufferData(Gl.GL_ARRAY_BUFFER, new IntPtr(TotalByteDataSize),
                                  _Arrays[index]._Data, Gl.GL_STREAM_DRAW);
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
                  int index = GetLocation(ref x, ref y, ref staticZ, ref side);
                  _Arrays[index] = new VertexArray(x, y, staticZ, side);
                  Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, _BufferNames[index]);

                  Gl.glBufferData(Gl.GL_ARRAY_BUFFER, new IntPtr(TotalByteDataSize),
                                  _Arrays[index]._Data, Gl.GL_STREAM_DRAW);
               }
            }
         }
      }
   }
}
