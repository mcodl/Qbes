using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Qbes.Common.Logic.Configuration;

namespace Qbes.Server.Logic.Configuration
{
   /// <summary>
   /// The server configuration is used to hold all the settings available for
   /// the server.
   /// </summary>
   public sealed class ServerConfiguration : IQbesConfiguration
   {
      #region Constructors
      /// <summary>
      /// Creates an instance with default settings.
      /// </summary>
      public ServerConfiguration()
      {
         AutoSave = new AutoSaveConfiguration();
         Network = new NetworkConfiguration();
         Security = new SecurityConfiguration();
         World = new WorldConfiguration();
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
      /// Gets or sets the network settings.
      /// </summary>
      [XmlElement("Network")]
      public NetworkConfiguration Network { get; set; }

      /// <summary>
      /// Gets or sets the security settings.
      /// </summary>
      [XmlElement("Security")]
      public SecurityConfiguration Security { get; set; }

      /// <summary>
      /// Gets or sets the world and gameplay settings.
      /// </summary>
      [XmlElement("World")]
      public WorldConfiguration World { get; set; }
   }
}
