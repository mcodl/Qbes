using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Qbes.Common.Logic.Configuration
{
   /// <summary>
   /// Instances of this class contain settings for network messaging.
   /// </summary>
   public sealed class MessagingConfiguration
   {
      #region Constants
      /// <summary>
      /// Default maximum size of a message in bytes.
      /// </summary>
      public const int DefaultMaxMessageSize = 1400;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default instance.
      /// </summary>
      public MessagingConfiguration()
      {
         MaxMessageSize = DefaultMaxMessageSize;
      }
      #endregion

      /// <summary>
      /// Gets or sets the maximum size of a message in bytes (including
      /// headers).
      /// </summary>
      [XmlElement("MaxMessageSize")]
      public int MaxMessageSize { get; set; }
   }
}
