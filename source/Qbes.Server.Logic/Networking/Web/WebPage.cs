using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace Qbes.Server.Logic.Networking.Web
{
   /// <summary>
   /// WebPage is a common base for web page response processing.
   /// </summary>
   public abstract class WebPage
   {
      #region Constants
      private const string KeyFormat = "{{${0}.{1}}}";
      private const string TemplateIdentifierFormat = "{{@Template={0}}}";
      #endregion

      #region Static fields
      private static readonly string _TemplatePath = Path.Combine("..", "..", "Templates");
      #endregion

      private string BuildTemplate(string templateName)
      {
         string template = null;

         if (templateName.EndsWith(".xml"))
         {
            // read parent XML template
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(GetTemplate(templateName));

            string masterTemplateName = xml.DocumentElement.Attributes["masterTemplateName"].Value;
            template = BuildTemplate(masterTemplateName);

            // replace placeholder with actual contents
            foreach (XmlNode node in xml.SelectNodes("//Template/Content"))
            {
               string contentName = node.Attributes["name"].Value;
               string templatePlaceholder = string.Format(TemplateIdentifierFormat, contentName);

               template.Replace(templatePlaceholder, node.InnerXml);
            }
         }
         else
         {
            // reached top level, just read and apply the template
            template = GetTemplate(templateName);
         }

         return template;
      }

      private string GetTemplate(string templateName)
      {
         return File.ReadAllText(Path.Combine(_TemplatePath, templateName));
      }

      /// <summary>
      /// Gets the page header.
      /// </summary>
      public abstract string Header { get; }

      /// <summary>
      /// Processes given template.
      /// </summary>
      /// <param name="templateName">Template name</param>
      /// <returns>Text with final markup</returns>
      protected string ProcessTemplate(string templateName)
      {
         string template = BuildTemplate(templateName);

         // replace Page placeholders
         template = template
            .Replace("{#Page.Header}", Header);

         return template;
      }

      /// <summary>
      /// Processes given template and objects into a final markup.
      /// </summary>
      /// <param name="templateName">Template name</param>
      /// <param name="contentObjects">List with content objects</param>
      /// <returns>Text with final markup</returns>
      protected string ProcessTemplate(string templateName, List<ContentObject> contentObjects)
      {
         // process page normally
         string content = ProcessTemplate(templateName);

         // go through all content objects and apply replacements
         foreach (ContentObject obj in contentObjects)
         {
            foreach (string key in obj.GetKeys())
            {
               content.Replace(string.Format(KeyFormat, obj.Name, key), obj.Get(key).ToString());
            }
         }

         return content;
      }

      /// <summary>
      /// Proccesses a request and sends an appropriate response.
      /// </summary>
      /// <param name="request">HTTP request</param>
      /// <param name="response">HTTP response</param>
      public abstract void ProcessRequest(HttpListenerRequest request, HttpListenerResponse response);

      /// <summary>
      /// Writes given text to the HTTP response.
      /// </summary>
      /// <param name="response">HTTP response</param>
      /// <param name="responseText">Response text</param>
      protected void WriteResponse(HttpListenerResponse response, string responseText)
      {
         WebHost.WriteResponse(response, responseText);
      }
   }
}
