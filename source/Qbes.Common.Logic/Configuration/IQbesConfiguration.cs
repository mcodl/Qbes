using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Configuration
{
   /// <summary>
   /// This interface defines common client and server configuration nodes,
   /// </summary>
   public interface IQbesConfiguration
   {
      /// <summary>
      /// Gets the autosave configuration node.
      /// </summary>
      AutoSaveConfiguration AutoSaveConfigurationNode { get; }

      /// <summary>
      /// Gets the messaging configuration node.
      /// </summary>
      MessagingConfiguration MessagingConfigurationNode { get; }
   }
}
