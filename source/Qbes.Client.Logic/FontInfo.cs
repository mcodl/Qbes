using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Client.Logic
{
   internal sealed class FontInfo
   {
      #region Constants
      private const float ColPercent = 0.0625f;
      #endregion

      #region Fields
      private float[] _Paddings = new float[4];
      #endregion

      #region Constructors
      internal FontInfo(string fontName, float heightRatio, float rowSize,
                        int[] pixelPaddings, int textureName)
      {
         FontName = fontName;
         HeightRatio = heightRatio;
         RowSize = rowSize;
         TextureName = textureName;

         float rowPercent = ColPercent * HeightRatio;
         _Paddings[0] = (float)pixelPaddings[0] / 256;
         _Paddings[1] = (float)pixelPaddings[1] / 256;
         _Paddings[2] = (float)pixelPaddings[2] / 256;
         _Paddings[3] = (float)pixelPaddings[3] / 256;
      }
      #endregion

      internal string FontName { get; private set; }

      internal float[] GetTexCoords(char c)
      {
         int index = (int)c - 32;
         int row = Convert.ToInt32(Math.Floor((double)index / 16));
         int col = index % 16;

         float rowPercent = ColPercent * HeightRatio;

         return new float[]
         {
            ColPercent * col + _Paddings[1], 
            rowPercent * row + _Paddings[0],
            ColPercent * col + ColPercent - _Paddings[3],
            rowPercent * row + rowPercent - _Paddings[2]
         };
      }

      internal float HeightRatio { get; private set; }

      internal float RowSize { get; private set; }

      internal int TextureName { get; private set; }
   }
}
