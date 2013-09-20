using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Qbes.Client.Logic.Configuration
{
   /// <summary>
   /// Input configuration contains all settings related to mouse and keyboard.
   /// </summary>
   public sealed class InputConfiguration
   {
      #region Constants
      /// <summary>
      /// Default mouse sensitivity.
      /// </summary>
      public const float DefaultMouseSensitity = 1.0f;
      #endregion

      #region Constructor
      /// <summary>
      /// Creates a configuration node instance.
      /// </summary>
      public InputConfiguration()
      {
         MouseSensitivity = DefaultMouseSensitity;
      }
      #endregion

      /// <summary>
      /// Gets or sets the mouse sensitivity.
      /// </summary>
      [XmlElement("MouseSensitivity")]
      public float MouseSensitivity { get; set; }
   }
}
