using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;

using Lidgren.Network;

using Qbes.Common.Logic.Configuration;

namespace Qbes.Server.Logic.Configuration
{
   /// <summary>
   /// Instances of this class contain network-specific configuration that is
   /// used to setup the server.
   /// </summary>
   public sealed class NetworkConfiguration
   {
      #region Constants
      /// <summary>
      /// Default milliseconds per KB write timeout before connection specific
      /// values become available. 
      /// </summary>
      public const int DefaultInitialMSPerKBSendTimeout = 1000;
      /// <summary>
      /// Default max connections count.
      /// </summary>
      public const int DefaultMaxConnections = 16;
      /// <summary>
      /// Default max area count of a single terrain data batch.
      /// </summary>
      public const int DefaultMaxTerrainBatchSize = 28;
      /// <summary>
      /// Default port for TCP.
      /// </summary>
      public const int DefaultPortTcp = 6666;
      /// <summary>
      /// Default port for UDP.
      /// </summary>
      public const int DefaultPortUdp = 6666;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates an instance with default settings.
      /// </summary>
      public NetworkConfiguration()
      {
         InitialMSPerKBSendTimeout = DefaultInitialMSPerKBSendTimeout;
         MaxConnections = DefaultMaxConnections;
         MaxTerrainBatchSize = DefaultMaxTerrainBatchSize;
         Messaging = new MessagingConfiguration();
         PortUdp = DefaultPortUdp;
         PortTcp = DefaultPortTcp;
         Web = new WebConfiguration();
      }
      #endregion

      internal NetPeerConfiguration CreateNetPeerConfigurationUdp()
      {
         NetPeerConfiguration config = new NetPeerConfiguration("Qbes");
         config.MaximumConnections = MaxConnections;
         config.Port = PortUdp;

         return config;
      }

      /// <summary>
      /// Gets or sets initial milliseconds per KB write timeout before
      /// connection specific values become available. 
      /// </summary>
      [XmlElement("InitialMSPerKBSendTimeout")]
      public int InitialMSPerKBSendTimeout { get; set; }

      /// <summary>
      /// Gets or sets the max connections count.
      /// </summary>
      [XmlElement("MaxConnections")]
      public int MaxConnections { get; set; }

      /// <summary>
      /// Gets or sets the max area count of a single terrain data batch.
      /// </summary>
      [XmlElement("MaxTerrainBatchSize")]
      public int MaxTerrainBatchSize { get; set; }

      /// <summary>
      /// Gets or sets the messaging configuration node.
      /// </summary>
      [XmlElement("Messaging")]
      public MessagingConfiguration Messaging { get; set; }

      /// <summary>
      /// Gets or sets the TCP server port.
      /// </summary>
      [XmlElement("PortTcp")]
      public int PortTcp { get; set; }

      /// <summary>
      /// Gets or sets the UDP server port.
      /// </summary>
      [XmlElement("PortUdp")]
      public int PortUdp { get; set; }

      /// <summary>
      /// Gets or sets the web configuration node.
      /// </summary>
      [XmlElement("Web")]
      public WebConfiguration Web { get; set; }
   }
}
