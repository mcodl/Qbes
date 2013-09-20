using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;

using Qbes.Common.Logic.Configuration;

namespace Qbes.Client.Logic.Configuration
{
   /// <summary>
   /// Instances of this class hold the information about network configuration.
   /// </summary>
   public sealed class NetworkConfiguration
   {
      #region Constants
      /// <summary>
      /// Default autoconnect state.
      /// </summary>
      public const bool DefaultAutoConnect = false;
      /// <summary>
      /// Default IP address.
      /// </summary>
      public const string DefaultIpAddress = "127.0.0.1";
      /// <summary>
      /// Default port for TCP.
      /// </summary>
      public const int DefaultPortTcp = 6666;
      /// <summary>
      /// Default port for UDP.
      /// </summary>
      public const int DefaultPortUdp = 6666;
      #endregion

      #region Constructor
      /// <summary>
      /// Creates a default configuration instance.
      /// </summary>
      public NetworkConfiguration()
      {
         AutoConnect = DefaultAutoConnect;
         IpAddress = DefaultIpAddress;
         Messaging = new MessagingConfiguration();
         PlayerName = Environment.UserName;
         PortUdp = DefaultPortUdp;
         PortTcp = DefaultPortTcp;
      }
      #endregion

      /// <summary>
      /// Gets or sets whether the client should connect to the server immediately.
      /// </summary>
      [XmlElement("AutoConnect")]
      public bool AutoConnect { get; set; }

      internal IPEndPoint GetRemoteEndPointUdp()
      {
         return new IPEndPoint(IPAddress.Parse(IpAddress), PortUdp);
      }

      /// <summary>
      /// Gets or sets the last used server address.
      /// </summary>
      [XmlElement("IpAddress")]
      public string IpAddress { get; set; }

      /// <summary>
      /// Gets or sets the messaging configuration node.
      /// </summary>
      [XmlElement("Messaging")]
      public MessagingConfiguration Messaging { get; set; }

      /// <summary>
      /// Gets or sets the player name.
      /// </summary>
      [XmlElement("PlayerName")]
      public string PlayerName { get; set; }

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
   }
}
