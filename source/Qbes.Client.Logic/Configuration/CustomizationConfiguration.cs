using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Qbes.Common.Logic.Configuration;

namespace Qbes.Client.Logic.Configuration
{
   /// <summary>
   /// Instances of this class are used to hold player's customization.
   /// </summary>
   public sealed class CustomizationConfiguration
   {
      #region Constants
      /// <summary>
      /// Default skin file name.
      /// </summary>
      public const string DefaultSkinFileName = "default.jpg";
      #endregion

      #region Static fields
      /// <summary>
      /// Default HUD color.
      /// </summary>
      public static readonly ColorNode DefaultHudColor = new ColorNode(1.0f, 0.0f, 0.0f, 1.0f);
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default configuration instance.
      /// </summary>
      public CustomizationConfiguration()
      {
         HudColor = DefaultHudColor;
         SkinFileName = DefaultSkinFileName;
      }
      #endregion

      /// <summary>
      /// Gets or sets the HUD color.
      /// </summary>
      [XmlElement("HudColor")]
      public ColorNode HudColor { get; set; }

      /// <summary>
      /// Gets or sets skin file name.
      /// </summary>
      [XmlElement("SkinFileName")]
      public string SkinFileName { get; set; }
   }
}
