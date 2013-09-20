using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Tao.OpenGl;

using Qbes.Common.Logic;
using Qbes.Common.Logic.Constants;

namespace Qbes.Client.Logic
{
   internal sealed class TextureManager
   {
      #region Constants
      private const int PixelWidth = 3;
      #endregion

      #region Static fields
      private static readonly string _FontsPath = Path.Combine("..", "..", "Fonts");
      private static readonly TextureManager _Instance = new TextureManager();
      private static readonly string _TexturePath = Path.Combine("..", "..", "Textures");
      private static readonly string _TexturesInfoPath = Path.Combine(_TexturePath, "textures.txt");
      #endregion

      #region Fields
      private int _CurrentTexture = 0;
      private Bitmap _Image;
      private object _LoadLock = new object();
      private Dictionary<string, FontInfo> _FontTextureMap = new Dictionary<string, FontInfo>();
      private Dictionary<int, int[]> _MaterialTextureMap = new Dictionary<int, int[]>();
      private Dictionary<string, int> _SkinTextureMap = new Dictionary<string, int>();
      private List<int> _Textures = new List<int>();
      #endregion

      #region Constructors
      private TextureManager()
      {
         // empty
      }

      static TextureManager()
      {
         // empty
      }
      #endregion

      private void BindTexture(int textureName, string path, int width, int height)
      {
         Console.WriteLine("Loading texture: {0} from {1}", textureName, path);

         IntPtr ptr = default(IntPtr);
         //int[] data = GetBitMapArray(path, width, height, out ptr);

         _Image = new Bitmap(Image.FromFile(path));
         BitmapData bitmap = _Image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
         byte[] pixelData = new byte[bitmap.Stride];
         int[] colorData = new int[width * height];
         // GCHandle pin = GCHandle.Alloc(colorData, GCHandleType.Pinned);

         for (int scanline = 0; scanline < bitmap.Height; scanline++)
         {
            Marshal.Copy(bitmap.Scan0 + (scanline * bitmap.Stride), pixelData, 0, bitmap.Stride);
            for (int pixeloffset = 0; pixeloffset < bitmap.Width; pixeloffset++)
            {
               colorData[(scanline * width) + pixeloffset] =
                     (pixelData[pixeloffset * PixelWidth + 2]) +
                     (pixelData[pixeloffset * PixelWidth + 1] << 8) +
                     (pixelData[pixeloffset * PixelWidth] << 16);
            }
         }
         ptr = bitmap.Scan0;
         // ptr = pin.AddrOfPinnedObject();

         Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureName);
         Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, 3, width, height, 0, Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, ptr);
         Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
         Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
         Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
         Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
         _Textures.Add(textureName);

         _Image.UnlockBits(bitmap);
         _Image.Dispose();
         // pin.Free();

         Console.WriteLine("Texture loaded: {0} from {1}", textureName, path);
      }

      internal void ChangeTextureForArrays(ref int texture)
      {
         if (texture != _CurrentTexture)
         {
            Gl.glDisableClientState(Gl.GL_COLOR_ARRAY);
            Gl.glDisableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            Gl.glDisableClientState(Gl.GL_NORMAL_ARRAY);

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texture);

            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
            Gl.glEnableClientState(Gl.GL_NORMAL_ARRAY);

            _CurrentTexture = texture;

#if DIAG
            DiagnosticsManager.GlBindTextureCalls++;
#endif
         }
      }

      internal void ChangeTextureForDisplayLists(ref int texture)
      {
         if (texture != _CurrentTexture)
         {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texture);
            _CurrentTexture = texture;

#if DIAG
            DiagnosticsManager.GlBindTextureCalls++;
#endif
         }
      }

      internal void ChangeTextureForQuads(int texture)
      {
         if (texture != _CurrentTexture)
         {
            Gl.glEnd();

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texture);

            Gl.glBegin(Gl.GL_QUADS);

            _CurrentTexture = texture;

#if DIAG
            DiagnosticsManager.GlBindTextureCalls++;
#endif
         }
      }

      private int[] GetBitMapArray(string textureFile, int width, int height, out IntPtr ptr)
      {
         _Image = new Bitmap(Image.FromFile(textureFile));
         BitmapData data = _Image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
         byte[] pixelData = new byte[data.Stride];
         int[] result = new int[width * height];

         try
         {
            for (int scanline = 0; scanline < data.Height; scanline++)
            {
               Marshal.Copy(data.Scan0 + (scanline * data.Stride), pixelData, 0, data.Stride);
               for (int pixeloffset = 0; pixeloffset < data.Width; pixeloffset++)
               {
                  result[(scanline * width) + pixeloffset] =
                      (pixelData[pixeloffset * PixelWidth + 2]) +
                      (pixelData[pixeloffset * PixelWidth + 1] << 8) +
                      (pixelData[pixeloffset * PixelWidth] << 16);
               }
            }
            ptr = data.Scan0;
         }
         finally
         {
            _Image.UnlockBits(data);
         }

         return result;
      }

      internal FontInfo GetFont(string key)
      {
         return _FontTextureMap[key];
      }

      private int[] GetMaterialTextureNames(int materialID)
      {
         if (!_MaterialTextureMap.ContainsKey(materialID))
         {
            return _MaterialTextureMap.Values.First();
         }

         return _MaterialTextureMap[materialID];
      }

      internal int GetSkinTextureName(string key, bool loadIfMissing, int beginConst)
      {
         int name = -1;
         _SkinTextureMap.TryGetValue(key, out name);

         if (name <= 0)
         {
            Console.WriteLine("Missing texture for {0}, attempting load...", key);
            if (SkinManager.IsSkinOnDisk(key))
            {
               Gl.glEnd();
               Gl.glDisable(Gl.GL_TEXTURE_2D);
               name = LoadSkinTexture(key, SkinManager.GetSkinFileName(key));
               Gl.glEnable(Gl.GL_TEXTURE_2D);
               Gl.glBegin(beginConst);
            }
         }

         return name;
      }

      internal static TextureManager Instance
      {
         get
         {
            return _Instance;
         }
      }

      internal void LoadFonts()
      {
         DirectoryInfo fontsDir = new DirectoryInfo(_FontsPath);
         FileInfo[] fonts = fontsDir.GetFiles("*.jpg");
         int[] fontTextureNames = new int[fonts.Length];

         lock (_LoadLock)
         {
            Gl.glGenTextures(fontTextureNames.Length, fontTextureNames);

            for (int i = 0; i < fontTextureNames.Length; i++)
            {
               string path = fonts[i].FullName;
               string name = fonts[i].Name;

               // load texture into opengl
               BindTexture(fontTextureNames[i], path, 256, 256);
               float heightRatio = 0.0f;
               float rowSize = 0.0f;
               int[] paddings = new int[4];

               foreach (string fontInfoData in File.ReadAllLines(path + ".info"))
               {
                  string[] frags = fontInfoData.Split('=');
                  switch (frags[0])
                  {
                     case "heightRatio":
                        heightRatio = float.Parse(frags[1], CultureInfo.InvariantCulture);
                        break;
                     case "padding":
                        string[] paddingData = frags[1].Split(';');
                        for (int p = 0; p < 4; p++)
                        {
                           paddings[p] = Convert.ToInt32(paddingData[p]);
                        }
                        break;
                     case "rowSize":
                        rowSize = float.Parse(frags[1], CultureInfo.InvariantCulture);
                        break;
                  }
               }

               // store texture by name
               FontInfo fontInfo = new FontInfo(name, heightRatio, rowSize, paddings, fontTextureNames[i]);
               _FontTextureMap.Add(name.Substring(0, name.IndexOf(".jpg")), fontInfo);
            }

            _Textures.AddRange(fontTextureNames);
         }
      }

      internal int LoadSkinTexture(string key, string path)
      {
         if (_SkinTextureMap.ContainsKey(key))
         {
            return _SkinTextureMap[key];
         }
         else
         {
            int[] skinTexture = new int[1];

            lock (_LoadLock)
            {
               Gl.glGenTextures(1, skinTexture);
               int error = Gl.glGetError();

               BindTexture(skinTexture[0], path, 256, 256);
            }

            _SkinTextureMap[key] = skinTexture[0];

            return skinTexture[0];
         }
      }

      internal void LoadTerrainTextures()
      {
         int[] terrainTextureNames;

         Dictionary<string, int> textureNameMap = new Dictionary<string, int>();
         DirectoryInfo dir = new DirectoryInfo(_TexturePath);
         FileInfo[] textureFiles = dir.GetFiles("*.jpg");
         terrainTextureNames = new int[textureFiles.Length];

         lock (_LoadLock)
         {
            Gl.glGenTextures(textureFiles.Length, terrainTextureNames);

            for (int i = 0; i < textureFiles.Length; i++)
            {
               string path = textureFiles[i].FullName;
               string name = textureFiles[i].Name;

               // load texture into opengl
               BindTexture(terrainTextureNames[i], path, 128, 128);

               // store texture by name
               textureNameMap.Add(name.Substring(0, name.IndexOf(".jpg")), terrainTextureNames[i]);
            }
         }

         // fill the material texture map collection based on textures.txt file
         string[] materialDefinitions = File.ReadAllLines(_TexturesInfoPath);
         foreach (string line in materialDefinitions)
         {
            string[] data = line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            ushort materialID = ushort.Parse(data[0]);
            int[] textureIDs = new int[6];
            for (int i = 0; i < 6; i++)
            {
               textureIDs[i] = textureNameMap[data[i + 1]];
            }
            _MaterialTextureMap.Add(materialID, textureIDs);
         }

         // set materials' texture names
         foreach (Material material in Material.GetMaterials())
         {
            if (material == null)
            {
               break;
            }
            material.SetTextureNames(GetMaterialTextureNames(material.ID));
         }
      }

      internal void UnloadTextures(bool reload)
      {
         Console.WriteLine("Unloading textures...");

         if (_Textures.Count == 0)
         {
            // no textures loaded
            return;
         }

         List<string> skinsToReload = new List<string>();

         if (reload)
         {
            skinsToReload.AddRange(_SkinTextureMap.Keys);
         }

         _FontTextureMap.Clear();
         _MaterialTextureMap.Clear();
         _SkinTextureMap.Clear();

         // unload all textures
         Gl.glDeleteTextures(_Textures.Count, _Textures.ToArray());
         _Textures.Clear();

         Console.WriteLine("Textures unloaded");

         if (reload)
         {
            Console.WriteLine("Reloading textures...");

            // reset all entity texture names
            ClientWorldManager.Instance.ResetEntityTextureNames();

            // reload textures
            LoadFonts();
            LoadTerrainTextures();
            foreach (string key in skinsToReload)
            {
               string path = SkinManager.GetSkinFileName(key);
               LoadSkinTexture(key, path);
            }

            Console.WriteLine("Textures reloaded");
         }
      }
   }
}
