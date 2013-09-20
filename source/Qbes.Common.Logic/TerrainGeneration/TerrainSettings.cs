using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.TerrainGeneration
{
   internal sealed class TerrainSettings
   {
      #region Constants
      private const float DefaultErosionLevel = 16.0f;
      private const int DefaultErosionPasses = 8;
      private const bool DefaultMirror = true;
      private const float DefaultNoise = 4.0f;
      private const float DefaultPerturbD = 32.0f;
      private const float DefaultPerturbF = 32.0f;
      private const float DefaultTerrainMaxHeight = 127.0f;
      private const float DefaultTerrainMinHeight = 63.0f;
      private const float ForestErosionLevel = 12.0f;
      private const int ForestErosionPasses = 1;
      private const bool ForestMirror = false;
      private const float ForestNoise = 8.0f;
      private const float ForestPerturbD = 16.0f;
      private const float ForestPerturbF = 16.0f;
      private const float ForestTerrainMaxHeight = 255.0f;
      private const float ForestTerrainMinHeight = 0.0f;
      private const float StoneErosionLevel = 4.0f;
      private const int StoneErosionPasses = 4;
      private const bool StoneMirror = false;
      private const float StoneNoise = 2.0f;
      private const float StonePerturbD = 16.0f;
      private const float StonePerturbF = 16.0f;
      private const float StoneTerrainMaxHeight = 95.0f;
      private const float StoneTerrainMinHeight = 31.0f;
      private const float ValleyErosionLevel = 24.0f;
      private const int ValleyErosionPasses = 8;
      private const bool ValleyMirror = false;
      private const float ValleyNoise = 8.0f;
      private const float ValleyPerturbD = 4.0f;
      private const float ValleyPerturbF = 4.0f;
      private const float ValleyTerrainMaxHeight = 255.0f;
      private const float ValleyTerrainMinHeight = 31.0f;
      #endregion

      #region Constructors
      internal TerrainSettings()
         : this (DefaultNoise, DefaultPerturbF, DefaultPerturbD,
                 DefaultErosionPasses, DefaultErosionLevel,
                 DefaultTerrainMinHeight, DefaultTerrainMaxHeight,
                 DefaultMirror)
      {
         // empty
      }

      internal TerrainSettings(float noise, float perturbF, float perturbD,
                               int erosionPasses, float erosionLevel,
                               float terrainMinHeight, float terrainMaxHeight,
                               bool mirror)
      {
         ErosionLevel = erosionLevel;
         ErosionPasses = erosionPasses;
         Mirror = mirror;
         Noise = noise;
         PerturbD = perturbD;
         PerturbF = perturbF;
         TerrainMaxHeight = terrainMaxHeight;
         TerrainMinHeight = terrainMinHeight;
      }
      #endregion

      internal static TerrainSettings CreateGrassMapSettings()
      {
         return new TerrainSettings();
      }

      internal static TerrainSettings CreateForestMapSettings()
      {
         return new TerrainSettings(ForestNoise, ForestPerturbF, ForestPerturbD,
                                    ForestErosionPasses, ForestErosionLevel,
                                    ForestTerrainMinHeight, ForestTerrainMaxHeight,
                                    ForestMirror);
      }

      internal static TerrainSettings CreateStoneMapSettings()
      {
         return new TerrainSettings(StoneNoise, StonePerturbF, StonePerturbD,
                                    StoneErosionPasses, StoneErosionLevel,
                                    StoneTerrainMinHeight, StoneTerrainMaxHeight,
                                    StoneMirror);
      }

      internal static TerrainSettings CreateValleyMapSettings()
      {
         return new TerrainSettings(ValleyNoise, ValleyPerturbF, ValleyPerturbD,
                                    ValleyErosionPasses, ValleyErosionLevel,
                                    ValleyTerrainMinHeight, ValleyTerrainMaxHeight,
                                    ValleyMirror);
      }

      internal float ErosionLevel { get; private set; }

      internal int ErosionPasses { get; private set; }

      internal bool Mirror { get; private set; }

      internal float Noise { get; private set; }

      internal float PerturbD { get; private set; }

      internal float PerturbF { get; private set; }

      internal float TerrainMaxHeight { get; private set; }

      internal float TerrainMinHeight { get; private set; }
   }
}
