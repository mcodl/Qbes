using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Qbes.Common.Logic.Configuration
{
   /// <summary>
   /// Instances of this class are used to hold settings about a color.
   /// </summary>
   public sealed class ColorNode
   {
      #region Constructors
      /// <summary>
      /// Creates a default non-transparent black color.
      /// </summary>
      public ColorNode()
      {
         // empty
      }

      /// <summary>
      /// Creates a non-transparent color based on channel values.
      /// </summary>
      /// <param name="red">Red portion</param>
      /// <param name="green">Green portion</param>
      /// <param name="blue">Blue portion</param>
      public ColorNode(float red, float green, float blue)
         : this (red, green, blue, 0.0f)
      {
      }

      /// <summary>
      /// Creates a color based on channel values.
      /// </summary>
      /// <param name="red">Red portion</param>
      /// <param name="green">Green portion</param>
      /// <param name="blue">Blue portion</param>
      /// <param name="alpha">Alpha value</param>
      public ColorNode(float red, float green, float blue, float alpha)
      {
         Red = red;
         Green = green;
         Blue = blue;
         Alpha = alpha;
      }
      #endregion

      /// <summary>
      /// Gets or sets the alpha level.
      /// </summary>
      [XmlAttribute("a")]
      public float Alpha { get; set; }

      /// <summary>
      /// Gets or sets the blue portion of the color.
      /// </summary>
      [XmlAttribute("b")]
      public float Blue { get; set; }

      /// <summary>
      /// Gets or sets the green portion of the color.
      /// </summary>
      [XmlAttribute("g")]
      public float Green { get; set; }

      /// <summary>
      /// Gets or sets the red portion of the color.
      /// </summary>
      [XmlAttribute("r")]
      public float Red { get; set; }

      /// <summary>
      /// Returns the color as a float array.
      /// </summary>
      /// <returns>Color in float array as { Red, Green, Blue, Alpha }</returns>
      public float[] ToArray()
      {
         return new float[] { Red, Green, Blue, Alpha };
      }
   }
}
