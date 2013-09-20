using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Constants;
using Qbes.Common.Logic.Exceptions;
using Qbes.Common.Logic.TerrainGeneration;

namespace Qbes.Common.Logic
{
   internal static class WorldGenerator
   {
      [Obsolete]
      internal static void CreateFlatWorld<TArea, TSegment, TBox>(byte areasX, byte areasZ, int seed, WorldManager<TArea, TSegment, TBox> worldManager)
         where TArea : Area, new()
         where TSegment : Segment, new()
         where TBox : Box, new()
      {
         Console.WriteLine("Creating new world...");

         Dictionary<int, Area> areas = new Dictionary<int, Area>();
         Dictionary<int, Segment> segments = new Dictionary<int, Segment>();

         int totalAreas = areasX * areasZ * 4;
         int sizeX = areasX * 64;
         int sizeY = 256;
         int sizeZ = areasZ * 64;

         #region Create cubes by adding layers
         int cubes = 0;
         int boxes = 0;

         // fill layers
         Console.WriteLine("Generating map...");
         int areasGenerated = 0;
         ushort materialId = Materials.Grass;

         for (int areaX = 0; areaX < sizeX; areaX += 64)
         {
            for (int areaZ = 0; areaZ < sizeZ; areaZ += 64)
            {
               #region Prepare areas
               for (int areaY = 0; areaY < sizeY; areaY += 64)
               {
                  Area area = new TerrainGenArea(areaX, areaY, areaZ);
                  areas.Add(area.Key, area);
               }
               #endregion

               #region Prepare segments
               for (int segmentX = areaX; segmentX < areaX + 64; segmentX += 8)
               {
                  for (int segmentY = 0; segmentY < 256; segmentY += 8)
                  {
                     for (int segmentZ = areaZ; segmentZ < areaZ + 64; segmentZ += 8)
                     {
                        int ownerX = (segmentX - segmentX % 64);
                        int ownerY = (segmentY - segmentY % 64);
                        int ownerZ = (segmentZ - segmentZ % 64);
                        Area area = areas[Area.GenerateKey(ownerX, ownerY, ownerZ)];

                        Segment segment = new TerrainGenSegment(segmentX, segmentY, segmentZ, area);

                        segments.Add(segment.Key, segment);
                     }
                  }
               }
               #endregion

               // generate boxes
               for (int x = 0; x < 64; x++)
               {
                  for (int z = 0; z < 64; z++)
                  {
                     for (int y = 0; y < 92; y++)
                     {
                        Cube cube = new Cube(x + areaX, y, z + areaZ, materialId);

                        int segmentX = (cube.X - cube.X % 8);
                        int segmentY = (cube.Y - cube.Y % 8);
                        int segmentZ = (cube.Z - cube.Z % 8);

                        if (segments[Segment.GenerateKey(segmentX, segmentY, segmentZ)].AddCube(cube))
                        {
                           cubes++;
                        }
                     }
                  }
               }

               // form boxes
               foreach (Segment segment in segments.Values)
               {
                  segment.ConstructBoxes();
                  boxes += segment.GetBoxCount();

                  // segment integrity check
                  int cubeCount = 0;
                  for (int i = 0; i < segment.GetBoxCount(); i++)
                  {
                     Box box = segment.GetBox<Box>(ref i);
                     cubeCount += Convert.ToInt32((box.X2 - box.X1) * (box.Y2 - box.Y1) * (box.Z2 - box.Z1));
                  }
                  if (cubeCount > 512)
                  {
                     throw new CubeCountHighException(cubeCount);
                  }
               }

               // save and unload the area column
               for (int areaY = 0; areaY < 256; areaY += 64)
               {
                  Area area = areas[Area.GenerateKey(areaX, areaY, areaZ)];
                  worldManager.SaveArea(area);
               }

               areas.Clear();
               segments.Clear();

               areasGenerated += 4;
               Console.WriteLine("\t{0}\t{1}", areasGenerated, totalAreas);
            }
         }
         Console.WriteLine("Created cubes: " + cubes);
         Console.WriteLine("Box count: " + boxes);
         #endregion
      }

      [Obsolete]
      internal static void CreatePesimisticWorld<TArea, TSegment, TBox>(byte areasX, byte areasZ, int seed, WorldManager<TArea, TSegment, TBox> worldManager)
         where TArea : Area, new()
         where TSegment : Segment, new()
         where TBox : Box, new()
      {
         Console.WriteLine("Creating new world...");

         Random r = (seed >= 0 ? new Random(seed) : new Random());

         Dictionary<int, Area> areas = new Dictionary<int, Area>();
         Dictionary<int, Segment> segments = new Dictionary<int, Segment>();

         int totalAreas = areasX * areasZ * 4;
         int sizeX = areasX * 64;
         int sizeY = 256;
         int sizeZ = areasZ * 64;

         #region Create cubes by adding layers
         int cubes = 0;
         int boxes = 0;

         // fill layers
         Console.WriteLine("Generating map...");
         int areasGenerated = 0;
         bool chasm = false;
         ushort materialIdBase, materialId;

         for (int areaX = 0; areaX < sizeX; areaX += 64)
         {
            for (int areaZ = 0; areaZ < sizeZ; areaZ += 64)
            {
               chasm = (areaX == areaZ);

               #region Prepare areas
               for (int areaY = 0; areaY < sizeY; areaY += 64)
               {
                  Area area = new TerrainGenArea(areaX, areaY, areaZ);
                  areas.Add(area.Key, area);
               }
               #endregion

               #region Prepare segments
               for (int segmentX = areaX; segmentX < areaX + 64; segmentX += 8)
               {
                  for (int segmentY = 0; segmentY < 256; segmentY += 8)
                  {
                     for (int segmentZ = areaZ; segmentZ < areaZ + 64; segmentZ += 8)
                     {
                        int ownerX = (segmentX - segmentX % 64);
                        int ownerY = (segmentY - segmentY % 64);
                        int ownerZ = (segmentZ - segmentZ % 64);
                        Area area = areas[Area.GenerateKey(ownerX, ownerY, ownerZ)];

                        Segment segment = new TerrainGenSegment(segmentX, segmentY, segmentZ, area);

                        segments.Add(segment.Key, segment);
                     }
                  }
               }
               #endregion

               // current heightmap
               byte[][] heightMap = new byte[64][];
               for (int x = 0; x < 64; x++)
               {
                  heightMap[x] = new byte[64];
               }

               // generate boxes
               materialIdBase = (byte)(r.Next(7) + 2);
               for (int x = 0; x < 64; x++)
               {
                  for (int z = 0; z < 64; z++)
                  {
                     if (chasm && x - 4 <= z && x + 4 >= z)
                     {
                        // skip to make chasm
                        continue;
                     }

                     for (int layer = 0; layer < 6; layer++)
                     {
                        materialId = (byte)(materialIdBase + layer % 4);

                        byte height = (byte)(layer * 16 + r.Next(4) + 12);
                        for (byte y = heightMap[x][z]; y < height; y++)
                        {
                           if ((x >= 29 && x <= 35 && y >= 29 && y <= 35) ||
                               (z >= 33 && z <= 39 && y >= 33 && y <= 39) ||
                               (x >= 31 && x <= 37 && z >= 31 && z <= 37))
                           {
                              // skip to make tunnels
                              continue;
                           }

                           Cube cube = new Cube(x + areaX, y, z + areaZ, materialId);

                           int segmentX = (cube.X - cube.X % 8);
                           int segmentY = (cube.Y - cube.Y % 8);
                           int segmentZ = (cube.Z - cube.Z % 8);

                           if (segments[Segment.GenerateKey(segmentX, segmentY, segmentZ)].AddCube(cube))
                           {
                              cubes++;
                           }
                        }
                        heightMap[x][z] = height;
                     }
                  }
               }

               // form boxes
               foreach (Segment segment in segments.Values)
               {
                  segment.ConstructBoxes();
                  boxes += segment.GetBoxCount();

                  // segment integrity check
                  int cubeCount = 0;
                  for (int i = 0; i < segment.GetBoxCount(); i++)
                  {
                     Box box = segment.GetBox<Box>(ref i);
                     cubeCount += Convert.ToInt32((box.X2 - box.X1) * (box.Y2 - box.Y1) * (box.Z2 - box.Z1));
                  }
                  if (cubeCount > 512)
                  {
                     throw new CubeCountHighException(cubeCount);
                  }
               }

               // save and unload the area column
               for (int areaY = 0; areaY < 256; areaY += 64)
               {
                  Area area = areas[Area.GenerateKey(areaX, areaY, areaZ)];
                  worldManager.SaveArea(area);
               }

               areas.Clear();
               segments.Clear();

               areasGenerated += 4;
               Console.WriteLine("\t{0}\t{1}", areasGenerated, totalAreas);
            }
         }
         Console.WriteLine("Created cubes: " + cubes);
         Console.WriteLine("Box count: " + boxes);
         #endregion
      }

      internal static void CreateProceduralWorld<TArea, TSegment, TBox>(byte areasX, byte areasZ, int seed, WorldManager<TArea, TSegment, TBox> worldManager)
         where TArea : Area, new()
         where TSegment : Segment, new()
         where TBox : Box, new()
      {
         Random r = new Random(seed);

         Console.WriteLine("Creating new world...");
         Console.WriteLine("Generating heightmaps...");

         const int valleyFromLevel = 95;
         const int forestFromLevel = 159;

         int mapSize = areasX * 64;
         HeightMap grassMap = new HeightMap(seed, mapSize, TerrainSettings.CreateGrassMapSettings());
         HeightMap stoneMap = new HeightMap(seed, mapSize, TerrainSettings.CreateStoneMapSettings());
         HeightMap valleyMap = new HeightMap(seed, mapSize, TerrainSettings.CreateValleyMapSettings());

         // blend valley map with grass map
         grassMap.Blend(valleyMap, valleyFromLevel);

         // now create forest height map
         HeightMap forestMap = new HeightMap(seed, mapSize, TerrainSettings.CreateForestMapSettings());

         Console.WriteLine("Storing terrain data...");

         Dictionary<int, Area> areas = new Dictionary<int, Area>();
         Dictionary<int, Segment> segments = new Dictionary<int, Segment>();

         int totalAreas = areasX * areasZ * 4;
         int sizeX = areasX * 64;
         int sizeY = 256;
         int sizeZ = areasZ * 64;

         #region Create cubes by adding layers
         int cubes = 0;
         int boxes = 0;
         int trees = 0;
         int potentialTreesObstructed = 0;
         int potentialTreesRandom = 0;

         // fill layers
         int areasGenerated = 0;
         for (int areaX = 0; areaX < sizeX; areaX += 64)
         {
            for (int areaZ = 0; areaZ < sizeZ; areaZ += 64)
            {
               #region Prepare areas
               for (int areaY = 0; areaY < sizeY; areaY += 64)
               {
                  Area area = new TerrainGenArea(areaX, areaY, areaZ);
                  areas.Add(area.Key, area);
               }
               #endregion

               #region Prepare segments
               for (int segmentX = areaX; segmentX < areaX + 64; segmentX += 8)
               {
                  for (int segmentY = 0; segmentY < 256; segmentY += 8)
                  {
                     for (int segmentZ = areaZ; segmentZ < areaZ + 64; segmentZ += 8)
                     {
                        int ownerX = (segmentX - segmentX % 64);
                        int ownerY = (segmentY - segmentY % 64);
                        int ownerZ = (segmentZ - segmentZ % 64);
                        Area area = areas[Area.GenerateKey(ownerX, ownerY, ownerZ)];

                        Segment segment = new TerrainGenSegment(segmentX, segmentY, segmentZ, area);

                        segments.Add(segment.Key, segment);
                     }
                  }
               }
               #endregion

               // generate boxes
               for (int x = 0; x < 64; x++)
               {
                  int currentX = x + areaX;
                  int segmentX = (currentX - currentX % 8);

                  for (int z = 0; z < 64; z++)
                  {
                     int currentZ = z + areaZ;
                     int segmentZ = (currentZ - currentZ % 8);
                     int grassHeight = Convert.ToInt32(grassMap[currentX, currentZ]);
                     int stoneHeight = Convert.ToInt32(stoneMap[currentX, currentZ]);
                     int maxHeight = Convert.ToInt32(Math.Max(grassHeight, stoneHeight));

                     ushort materialId = Materials.Stone;

                     int y = 0;
                     for (y = 0; y <= maxHeight; y++)
                     {
                        // mud or grass
                        if (y < grassHeight)
                        {
                           materialId = Materials.Mud;
                        }
                        if (y == grassHeight ||
                            (y == maxHeight && materialId == Materials.Mud))
                        {
                           materialId = Materials.Grass;
                        }

                        // possible stone override
                        if (y < stoneHeight)
                        {
                           materialId = Materials.Stone;
                        }

                        Cube cube = new Cube(currentX, y, currentZ, materialId);

                        int segmentY = (cube.Y - cube.Y % 8);

                        if (segments[Segment.GenerateKey(segmentX, segmentY, segmentZ)].AddCube(cube))
                        {
                           cubes++;
                        }
                     }

                     // check if tree should be added on top of grass
                     float forestLevel = forestMap[currentX, currentZ];
                     if (materialId == Materials.Grass &&
                         x > 0 && x < 63 && y < 224 && z > 0 && z < 63 &&
                         forestLevel >= forestFromLevel)
                     {
                        // check if there is nothing
                        bool obstructed = false;
                        for (int nX = currentX - 1; nX <= currentX + 1; nX++)
                        {
                           for (int nZ = currentZ - 1; nZ <= currentZ + 1; nZ++)
                           {
                              if (segments[Segment.GenerateKey(nX, y, nZ)].VisMatrix.Get(nX % 8, y % 8, nZ % 8))
                              {
                                 obstructed = true;
                                 break;
                              }
                           }

                           if (obstructed)
                           {
                              break;
                           }
                        }

                        if (obstructed)
                        {
                           potentialTreesObstructed++;
                        }

                        if (!obstructed && r.Next(0, 2048) < forestLevel)
                        {
                           // generate a tree
                           int treeHeight = r.Next(3, 7);
                           int leavesSpread = r.Next(1, 3);
                           if (treeHeight < 5 || x == 1 || z == 1 || x == 62 || z == 62)
                           {
                              leavesSpread = 1;
                           }

                           // start with trunk
                           materialId = Materials.Treewood;
                           for (int tY = y; tY < y + treeHeight; tY++)
                           {
                              Cube cube = new Cube(currentX, tY, currentZ, materialId);
                              int segmentY = (cube.Y - cube.Y % 8);
                              if (segments[Segment.GenerateKey(segmentX, segmentY, segmentZ)].AddCube(cube))
                              {
                                 cubes++;
                              }
                           }

                           // now surround with leaves
                           materialId = Materials.Leaves;
                           for (int lY = y + 2; lY < y + treeHeight + 1; lY++)
                           {
                              if (lY == y + treeHeight)
                              {
                                 leavesSpread--;
                              }

                              for (int lX = currentX - leavesSpread; lX <= currentX + leavesSpread; lX++)
                              {
                                 for (int lZ = currentZ - leavesSpread; lZ <= currentZ + leavesSpread; lZ++)
                                 {
                                    Cube cube = new Cube(lX, lY, lZ, materialId);
                                    int segmentY = (cube.Y - cube.Y % 8);
                                    if (segments[Segment.GenerateKey(lX - lX % 8, segmentY, lZ - lZ % 8)].AddCube(cube))
                                    {
                                       cubes++;
                                    }
                                 }
                              }
                           }

                           trees++;
                        }
                        else
                        {
                           potentialTreesRandom++;
                        }
                     }
                  }
               }

               // form boxes
               foreach (Segment segment in segments.Values)
               {
                  segment.ConstructBoxes();
                  boxes += segment.GetBoxCount();

                  // segment integrity check
                  int cubeCount = 0;
                  for (int i = 0; i < segment.GetBoxCount(); i++)
                  {
                     Box box = segment.GetBox<Box>(ref i);
                     cubeCount += Convert.ToInt32((box.X2 - box.X1) * (box.Y2 - box.Y1) * (box.Z2 - box.Z1));
                  }
                  if (cubeCount > 512)
                  {
                     throw new CubeCountHighException(cubeCount);
                  }
               }

               // save and unload the area column
               for (int areaY = 0; areaY < 256; areaY += 64)
               {
                  Area area = areas[Area.GenerateKey(areaX, areaY, areaZ)];
                  ((TerrainGenArea)area).SortSegments();
                  worldManager.SaveArea(area);
               }

               areas.Clear();
               segments.Clear();

               areasGenerated += 4;
               Console.WriteLine("\t{0}\t{1}", areasGenerated, totalAreas);
            }
         }
         Console.WriteLine("Created cubes: " + cubes);
         Console.WriteLine("Box count: " + boxes);
         Console.WriteLine("Tree count: " + trees + ", remove potential obstructed: " + potentialTreesObstructed + ", by random: " + potentialTreesRandom);
         #endregion
      }
   }
}
