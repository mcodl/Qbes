using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Constants;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// Instances of Material class represent different materials in the world.
   /// </summary>
   public sealed class Material
   {
      #region Static fields
      private static Material[] _Materials = new Material[19];
      #endregion

      #region Fields
      private ushort _ID;
      private int[] _TextureNames;
      #endregion

      #region Constructors
      private Material(ushort id, int durability, byte stability)
      {
         _ID = id;
         Durability = durability;
         Stability = stability;
      }

      [Obsolete("Material colors are no longer needed")]
      private Material(ushort id, float red, float green, float blue, float alpha, int durability)
      {
         _ID = id;
         Red = red;
         Green = green;
         Blue = blue;
         Alpha = alpha;
         Durability = durability;
      }

      static Material()
      {
         _Materials[Materials.Air] = new Material(Materials.Air, 0, 0); // air
         _Materials[Materials.Water] = new Material(Materials.Water, 0, 0); // water
         _Materials[Materials.Stone] = new Material(Materials.Stone, 100, 20); // stone
         _Materials[Materials.Sand] = new Material(Materials.Sand, 20, 4); // sand
         _Materials[Materials.Badland] = new Material(Materials.Badland, 50, 8); // badland
         _Materials[Materials.Mud] = new Material(Materials.Mud, 30, 8); // mud
         _Materials[Materials.Metal] = new Material(Materials.Metal, 200, 28); // metal
         _Materials[Materials.Leaves] = new Material(Materials.Leaves, 10, 4); // leaves
         _Materials[Materials.Treewood] = new Material(Materials.Treewood, 40, 24); // treewood
         _Materials[Materials.Grass] = new Material(Materials.Grass, 30, 8); // grass
         _Materials[Materials.Coal] = new Material(Materials.Coal, 100, 12); // coal
         _Materials[Materials.Plastic] = new Material(Materials.Plastic, 150, 16); // plastic
         _Materials[Materials.Snow] = new Material(Materials.Snow, 5, 4); // snow
         _Materials[Materials.Asphalt] = new Material(Materials.Asphalt, 250, 28); // asphalt
         _Materials[Materials.Bush] = new Material(Materials.Bush, 12, 4); // leaves
         _Materials[Materials.Pavement] = new Material(Materials.Pavement, 200, 12); // pavement
         _Materials[Materials.Tiles] = new Material(Materials.Tiles, 200, 16); // tiles
         _Materials[Materials.WoodenPlanksSmooth] = new Material(Materials.WoodenPlanksSmooth, 70, 32); // smooth wooden planks
         _Materials[Materials.WoodenPlanksRough] = new Material(Materials.WoodenPlanksRough, 60, 28); // rough wooden planks
      }
      #endregion

      /// <summary>
      /// Gets texture name for a side.
      /// </summary>
      /// <param name="side">Side</param>
      /// <returns>Texture name for given side</returns>
      public int this[int side]
      {
         get
         {
            return _TextureNames[side];
         }
      }

      /// <summary>
      /// Alpha level.
      /// </summary>
      [Obsolete("Material colors are no longer needed")]
      public float Alpha { get; private set; }

      /// <summary>
      /// Blue color level.
      /// </summary>
      [Obsolete("Material colors are no longer needed")]
      public float Blue { get; private set; }

      /// <summary>
      /// Material durability.
      /// </summary>
      public int Durability { get; private set; }

      /// <summary>
      /// Gets the first material available for placing.
      /// </summary>
      public static ushort FirstPlaceableMaterial
      {
         get
         {
            return 2;
         }
      }

      /// <summary>
      /// Gets a desired material by ID.
      /// </summary>
      /// <param name="material">Material ID</param>
      /// <returns>Material by ID</returns>
      public static Material GetMaterial(ushort material)
      {
         return _Materials[material];
      }

      /// <summary>
      /// Gets a collection with all available materials.
      /// </summary>
      /// <returns>Collection with all available materials</returns>
      public static List<Material> GetMaterials()
      {
         return new List<Material>(_Materials);
      }

      /// <summary>
      /// Gets the material's ID.
      /// </summary>
      public int ID
      {
         get
         {
            return _ID;
         }
      }

      /// <summary>
      /// Gets the last material available for placing.
      /// </summary>
      public static ushort LastPlaceableMaterial
      {
         get
         {
            return 18;
         }
      }

      /// <summary>
      /// Red color level.
      /// </summary>
      [Obsolete("Material colors are no longer needed")]
      public float Red { get; private set; }

      /// <summary>
      /// Green color level.
      /// </summary>
      [Obsolete("Material colors are no longer needed")]
      public float Green { get; private set; }

      /// <summary>
      /// Set texture names.
      /// </summary>
      /// <param name="textureNames">Texture name array</param>
      public void SetTextureNames(int[] textureNames)
      {
         _TextureNames = textureNames;
      }

      /// <summary>
      /// Gets or sets (private) material stability.
      /// </summary>
      public byte Stability { get; private set; }
   }
}
