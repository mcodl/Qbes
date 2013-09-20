using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Qbes.Common.Logic.Configuration;

namespace Qbes.Client.Logic.Configuration
{
   /// <summary>
   /// Base node of the configuration hierarchy.
   /// </summary>
   [Serializable]
   public sealed class ClientConfiguration : IQbesConfiguration
   {
      #region Constructors
      /// <summary>
      /// Creates a configuration node instance.
      /// </summary>
      public ClientConfiguration()
      {
         AutoSave = new AutoSaveConfiguration();
         Customization = new CustomizationConfiguration();
         Input = new InputConfiguration();
         Network = new NetworkConfiguration();
         Video = new VideoConfiguration();
      }
      #endregion

      /// <summary>
      /// Gets or sets the autosave settings.
      /// </summary>
      [XmlElement("AutoSave")]
      public AutoSaveConfiguration AutoSave { get; set; }

      /// <summary>
      /// Gets the autosave configuration node.
      /// </summary>
      [XmlIgnore]
      public AutoSaveConfiguration AutoSaveConfigurationNode
      {
         get
         {
            return AutoSave;
         }
      }

      /// <summary>
      /// Gets or sets the customization configuration node.
      /// </summary>
      [XmlElement("Customization")]
      public CustomizationConfiguration Customization { get; set; }

      /// <summary>
      /// Gets or sets the input configuration node.
      /// </summary>
      [XmlElement("Input")]
      public InputConfiguration Input { get; set; }

      /// <summary>
      /// Gets the messaging configuration node.
      /// </summary>
      [XmlIgnore]
      public MessagingConfiguration MessagingConfigurationNode
      {
         get
         {
            return Network.Messaging;
         }
      }

      /// <summary>
      /// Gets or sets the network configuration node.
      /// </summary>
      [XmlElement("Network")]
      public NetworkConfiguration Network { get; set; }

      /// <summary>
      /// Gets or sets the video configuration node.
      /// </summary>
      [XmlElement("Video")]
      public VideoConfiguration Video { get; set; }
   }
}
