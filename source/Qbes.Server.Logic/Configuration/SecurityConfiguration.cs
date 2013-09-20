using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Qbes.Server.Logic.Configuration
{
   /// <summary>
   /// Instances of this class are used to hold settings regarding server
   /// security.
   /// </summary>
   public sealed class SecurityConfiguration
   {
      #region Constants
      /// <summary>
      /// Server doesn't require authentication by default.
      /// </summary>
      public const bool DefaultRequiresAuthentication = false;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default instance.
      /// </summary>
      public SecurityConfiguration()
      {
         BannedIpAddresses = new List<string>();
         BannedPlayers = new List<string>();
         RequiresAuthentication = DefaultRequiresAuthentication;
      }
      #endregion

      /// <summary>
      /// Gets or sets the list containing names of banned IP addresses.
      /// </summary>
      [XmlElement("BannedIpAddresses")]
      public List<string> BannedIpAddresses { get; set; }

      /// <summary>
      /// Gets or sets the list containing names of banned players.
      /// </summary>
      [XmlElement("BannedPlayers")]
      public List<string> BannedPlayers { get; set; }

      /// <summary>
      /// Gets or sets whether the server requires authentication.
      /// </summary>
      [XmlElement("RequiresAuthentication")]
      public bool RequiresAuthentication { get; set; }
   }
}
