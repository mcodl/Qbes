using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Qbes.Server.Logic.Configuration
{
   /// <summary>
   /// Instances of this class are used as part of web server configuration
   /// and contain handler type for a source address.
   /// </summary>
   public sealed class WebPageNode
   {
      #region Constructors
      /// <summary>
      /// Creates a default web page node instance.
      /// </summary>
      public WebPageNode()
      {
         // empty
      }

      /// <summary>
      /// Creates a specific web page node instance.
      /// </summary>
      /// <param name="handlerType"></param>
      /// <param name="addresses"></param>
      public WebPageNode(Type handlerType, params string[] addresses)
      {
         HandlerType = handlerType.AssemblyQualifiedName;
         Addresses = addresses;
      }
      #endregion

      /// <summary>
      /// Gets or sets an array with addresses applying for this handler.
      /// </summary>
      [XmlArray("Addresses")]
      [XmlArrayItem("Address")]
      public string[] Addresses { get; set; }

      /// <summary>
      /// Gets or sets the handler type.
      /// </summary>
      [XmlElement("HandlerType")]
      public string HandlerType { get; set; }
   }
}
