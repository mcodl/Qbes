using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Qbes.Common.Logic.Configuration
{
   /// <summary>
   /// This configuration node contains info about autosave settings.
   /// </summary>
   public sealed class AutoSaveConfiguration
   {
      #region Constants
      /// <summary>
      /// Default autosave interval in seconds.
      /// </summary>
      public const int DefaultAutoSaveInterval = 60;
      /// <summary>
      /// Default setting for whether autosave is enabled.
      /// </summary>
      public const bool DefaultEnableAutoSave = true;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default instance.
      /// </summary>
      public AutoSaveConfiguration()
      {
         AutoSaveInterval = DefaultAutoSaveInterval;
         EnableAutoSave = DefaultEnableAutoSave;
      }
      #endregion

      /// <summary>
      /// Gets or sets the autosave interval in seconds.
      /// </summary>
      [XmlElement("AutoSaveInterval")]
      public int AutoSaveInterval { get; set; }

      /// <summary>
      /// Gets or sets whether autosave is enabled.
      /// </summary>
      [XmlElement("EnableAutoSave")]
      public bool EnableAutoSave { get; set; }
   }
}
