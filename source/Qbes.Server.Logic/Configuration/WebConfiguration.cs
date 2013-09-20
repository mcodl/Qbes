using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Qbes.Server.Logic.Networking.Web.Pages;

namespace Qbes.Server.Logic.Configuration
{
   /// <summary>
   /// WebConfiguration contains properties with setting for the WebHost
   /// compoment.
   /// </summary>
   public sealed class WebConfiguration
   {
      #region Constants
      /// <summary>
      /// Gets the default enabled state.
      /// </summary>
      public const bool DefaultEnabled = false;
      /// <summary>
      /// Gets the default web server port.
      /// </summary>
      public const int DefaultWebServerPort = 800;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default web configuration node instance.
      /// </summary>
      public WebConfiguration()
      {
         Enabled = DefaultEnabled;
         WebServerPort = DefaultWebServerPort;

         // reconstruct default handlers
         SetDefaultHandlers();
      }
      #endregion

      /// <summary>
      /// Gets or sets whether the web server is enabled.
      /// </summary>
      [XmlElement("Enabled")]
      public bool Enabled { get; set; }

      /// <summary>
      /// Gets or sets page handlers.
      /// </summary>
      [XmlArray("Handlers")]
      [XmlArrayItem("Handler")]
      public WebPageNode[] Handlers { get; set; }

      private void SetDefaultHandlers()
      {
         List<WebPageNode> defaultHandlers = new List<WebPageNode>();

         defaultHandlers.Add(new WebPageNode(typeof(Home), "/", "/index.htm", "/index.html"));

         Handlers = defaultHandlers.ToArray();
      }

      /// <summary>
      /// Gets or sets the web server port.
      /// </summary>
      [XmlElement("WebServerPort")]
      public int WebServerPort { get; set; }
   }
}
