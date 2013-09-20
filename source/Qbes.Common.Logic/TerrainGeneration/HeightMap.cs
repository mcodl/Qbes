using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.TerrainGeneration
{
   internal sealed class HeightMap
   {
      #region Constants
      private const float DefaultErosionLevel = 16.0f;
      private const int DefaultErosionPasses = 8;
      private const float DefaultNoise = 4.0f;
      private const float DefaultPerturbD = 32.0f;
      private const float DefaultPerturbF = 32.0f;
      private const float DefaultTerrainMaxHeight = 127.0f;
      private const float DefaultTerrainMinHeight = 63.0f;
      private const int MaxHeight = 256;
      #endregion

      #region Private fields
      private int _HalfSize;
      private float[,] _Heights;
      private PerlinNoise _Perlin;
      private int _Size;
      private TerrainSettings _Settings;
      #endregion

      #region Constructors
      internal HeightMap(int seed, int size, TerrainSettings settings)
      {
         _HalfSize = size / 2;
         _Size = size;
         _Heights = new float[_Size, _Size];
         _Perlin = new PerlinNoise(seed);
         _Settings = settings;

         GenerateTerrain();
      }
      #endregion

      internal float this[int x, int z]
      {
         get
         {
            return _Heights[x, z];
         }
         set
         {
            _Heights[x, z] = value;
         }
      }

      private void AddPerlinNoise()
      {
         float noise = _Settings.Noise;

         for (int x = 0; x < _Size; x++)
         {
            for (int z = 0; z < _Size; z++)
            {
               _Heights[x, z] += Convert.ToSingle(_Perlin.Noise(noise * x / (float)_Size, noise * z / (float)_Size, 0));
            }
         }

         if (_Settings.Mirror)
         {
            // now take the middle part and move it to 0 up to half size
            int sourceStart = _Size / 4;
            for (int x = 0; x < _HalfSize; x++)
            {
               for (int z = 0; z < _HalfSize; z++)
               {
                  _Heights[x, z] = _Heights[x + sourceStart, z + sourceStart];
               }
            }
         }
      }

      private void AdjustHeight()
      {
         float minHeight = _Settings.TerrainMinHeight;
         float maxHeight = _Settings.TerrainMaxHeight;

         for (int x = 0; x < _Size; x++)
         {
            for (int z = 0; z < _Size; z++)
            {
               float height = _Heights[x, z] + 0.5f;
               height *= maxHeight;
               float diff = maxHeight - height;
               diff *= (minHeight / maxHeight);

               _Heights[x, z] = height + diff;

               if (_Heights[x, z] < 0)
               {
                  _Heights[x, z] = 0;
               }
               else if (_Heights[x, z] > MaxHeight - 1)
               {
                  _Heights[x, z] = MaxHeight - 1;
               }
            }
         }
      }

      internal void Blend(HeightMap otherMap, int fromLevel)
      {
         for (int x = 0; x < _Size; x++)
         {
            for (int z = 0; z < _Size; z++)
            {
               float height = _Heights[x, z];
               float otherHeight = otherMap[x, z];
               if (height <= fromLevel)
               {
                  _Heights[x, z] = Convert.ToSingle(Math.Min(height, otherHeight));
               }
            }
         }

         Smoothen();
      }

      private void Erode()
      {
         float smoothness = _Settings.ErosionLevel;

         for (int e = 0; e < _Settings.ErosionPasses; e++)
         {
            for (int x = 1; x < _Size - 1; x++)
            {
               for (int z = 1; z < _Size - 1; z++)
               {
                  float dMax = 0.0f;
                  int[] match = { 0, 0 };

                  for (int u = -1; u <= 1; u++)
                  {
                     for (int v = -1; v <= 1; v++)
                     {
                        if (Math.Abs(u) + Math.Abs(v) > 0)
                        {
                           float dI = _Heights[x, z] - _Heights[x + u, z + v];
                           if (dI > dMax)
                           {
                              dMax = dI;
                              match[0] = u; match[1] = v;
                           }
                        }
                     }
                  }

                  if (0 < dMax && dMax <= (smoothness / (float)_Size))
                  {
                     float dH = 0.5f * dMax;
                     _Heights[x, z] -= dH;
                     _Heights[x + match[0], z + match[1]] += dH;
                  }
               }
            }
         }
      }

      private void GenerateTerrain()
      {
         // create initial terrain for quarter of the map
         AddPerlinNoise();

         // mirror the terrain to remaining parts
         MirrorTerrain();

         // perturbate the terrain
         Perturb();

         // apply erosion
         Erode();

         // smoothen the terrain
         Smoothen();

         // adjust final heightmap
         AdjustHeight();
      }

      private void MirrorTerrain()
      {
         if (_Settings.Mirror)
         {
            for (int x = _HalfSize; x < _Size; x++)
            {
               for (int z = 0; z < _HalfSize; z++)
               {
                  _Heights[x, z] = _Heights[_HalfSize - (x - _HalfSize) - 1, z];
               }
            }

            for (int x = 0; x < _Size; x++)
            {
               for (int z = _HalfSize; z < _Size; z++)
               {
                  _Heights[x, z] = _Heights[x, _HalfSize - (z - _HalfSize) - 1];
               }
            }
         }
      }

      private void Perturb()
      {
         float f = _Settings.PerturbF;
         float d = _Settings.PerturbD;

         int u, v;
         float[,] temp = new float[_Size, _Size];
         for (int x = 0; x < _Size; x++)
         {
            for (int z = 0; z < _Size; z++)
            {
               u = x + (int)(_Perlin.Noise(f * x / (float)_Size, f * z / (float)_Size, 0) * d);
               v = z + (int)(_Perlin.Noise(f * x / (float)_Size, f * z / (float)_Size, 1) * d);

               if (u < 0)
               {
                  u = 0;
               }
               else if (u >= _Size)
               {
                  u = _Size - 1;
               }

               if (v < 0)
               {
                  v = 0;
               }
               else if (v >= _Size)
               {
                  v = _Size - 1;
               }
               temp[x, z] = _Heights[u, v];
            }
         }
         _Heights = temp;
      }

      private void Smoothen()
      {
         for (int x = 0; x < _Size; ++x)
         {
            for (int z = 0; z < _Size; ++z)
            {
               float total = 0.0f;

               for (int u = -1; u <= 1; u++)
               {
                  for (int v = -1; v <= 1; v++)
                  {
                     int xU = x + u;
                     if (xU < 0)
                     {
                        xU = _Size - 1;
                     }
                     else if (xU >= _Size)
                     {
                        xU = 0;
                     }
                     int zV = z + v;
                     if (zV < 0)
                     {
                        zV = _Size - 1;
                     }
                     else if (zV >= _Size)
                     {
                        zV = 0;
                     }
                     total += _Heights[xU, zV];
                  }
               }

               _Heights[x, z] = total / 9.0f;
            }
         }
      }
   }
}
