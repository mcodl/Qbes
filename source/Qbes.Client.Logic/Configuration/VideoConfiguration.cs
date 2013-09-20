using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Qbes.Common.Logic.Configuration;

namespace Qbes.Client.Logic.Configuration
{
   /// <summary>
   /// The video configuration node contains all settings related to display
   /// settings.
   /// </summary>
   public sealed class VideoConfiguration
   {
      #region Constants
      /// <summary>
      /// Default fog setting.
      /// </summary>
      public const bool DefaultFog = true;
      /// <summary>
      /// Default resolution height.
      /// </summary>
      public const int DefaultResolutionHeight = 720;
      /// <summary>
      /// Default resolution width.
      /// </summary>
      public const int DefaultResolutionWidth = 1280;
      /// <summary>
      /// Default visibility range.
      /// </summary>
      public const int DefaultVisibilityRange = 192;
      #endregion

      #region Static fields
      /// <summary>
      /// Default sky color.
      /// </summary>
      public static readonly ColorNode DefaultSkyColor = new ColorNode(0.77f, 0.87f, 0.97f, 1.0f);
      #endregion

      #region Constructor
      /// <summary>
      /// Creates a configuration node instance.
      /// </summary>
      public VideoConfiguration()
      {
         Fog = DefaultFog;
         ResolutionHeight = DefaultResolutionHeight;
         ResolutionWidth = DefaultResolutionWidth;
         SkyColor = DefaultSkyColor;
         VisibilityRange = DefaultVisibilityRange;
      }
      #endregion

      /// <summary>
      /// Gets or sets whether fog is enabled.
      /// </summary>
      [XmlElement("Fog")]
      public bool Fog { get; set; }

      /// <summary>
      /// Gets or sets the resolution height.
      /// </summary>
      [XmlElement("ResolutionHeight")]
      public int ResolutionHeight { get; set; }

      /// <summary>
      /// Gets or sets the resolution width.
      /// </summary>
      [XmlElement("ResolutionWidth")]
      public int ResolutionWidth { get; set; }

      /// <summary>
      /// Gets or sets the sky color.
      /// </summary>
      [XmlElement("SkyColor")]
      public ColorNode SkyColor { get; set; }

      /// <summary>
      /// Gets or sets the visibility range in meters.
      /// </summary>
      [XmlElement("VisibilityRange")]
      public int VisibilityRange { get; set; }
   }
}
