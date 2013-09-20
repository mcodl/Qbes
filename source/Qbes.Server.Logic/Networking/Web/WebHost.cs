using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using Qbes.Common.Logic.Exceptions;
using Qbes.Server.Logic.Configuration;

namespace Qbes.Server.Logic.Networking.Web
{
   internal sealed class WebHost
   {
      #region Static fields
      private static readonly string _BasePath = Path.Combine("..", "..");
      #endregion

      #region Private fields
      private bool _Active;
      private WebConfiguration _Configuration;
      private bool _Enabled;
      private HttpListener _HttpServer = new HttpListener();
      private Dictionary<string, Type> _Pages = new Dictionary<string, Type>();
      private Thread _ProcessingThread;
      #endregion

      #region Constructors
      internal WebHost(WebConfiguration configuration)
      {
         _Configuration = configuration;
         _Enabled = _Configuration.Enabled;
         _HttpServer.Prefixes.Add(string.Format("http://*:{0}/", _Configuration.WebServerPort));

         // read pages configuration
         foreach (WebPageNode node in configuration.Handlers)
         {
            Type type = Type.GetType(node.HandlerType);
            foreach (string address in node.Addresses)
            {
               _Pages.Add(address, type);
            }
         }
      }
      #endregion

      private void ProcessRequest(IAsyncResult result)
      {
         HttpListener listener = (HttpListener)result.AsyncState;

         // call EndGetContext to complete the asynchronous operation.
         HttpListenerContext context = listener.EndGetContext(result);

         try
         {
            // find a response handler
            Type responseType = null;
            if (_Pages.TryGetValue(context.Request.Url.LocalPath, out responseType))
            {
               WebPage page = (WebPage)Activator.CreateInstance(responseType);
               page.ProcessRequest(context.Request, context.Response);
            }
            else
            {
               // not response handler found, try static content
               string filePath = _BasePath + context.Request.Url.LocalPath.Replace('/', Path.DirectorySeparatorChar);
               if (File.Exists(filePath))
               {
                  string contentType = "text/plain";
                  bool text = true;

                  // attempt to determine content type
                  if (filePath.Contains("."))
                  {
                     string ext = filePath.Substring(filePath.LastIndexOf(".") + 1);

                     switch (ext)
                     {
                        case "css":
                           contentType = "text/css";
                           break;
                        case "gif":
                           contentType = "image/gif";
                           text = false;
                           break;
                        case "htm":
                        case "html":
                           contentType = "text/html";
                           break;
                        case "jpeg":
                        case "jpg":
                           contentType = "image/jpeg";
                           text = false;
                           break;
                        case "js":
                           contentType = "application/x-javascript";
                           break;
                        case "png":
                           contentType = "image/png";
                           text = false;
                           break;
                     }
                  }

                  // write response as either text or binary
                  if (text)
                  {
                     WriteResponse(context.Response, contentType, File.ReadAllText(filePath));
                  }
                  else
                  {
                     WriteResponse(context.Response, contentType, File.ReadAllBytes(filePath));
                  }
               }
               else
               {
                  // static content not found, return 404
                  context.Response.StatusCode = (int)HttpStatusCode.NotFound;
               }
            }
         }
         catch (Exception ex)
         {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ExceptionHandler.LogException(ex);
         }

         // close the output stream
         context.Response.Close();
      }

      private void ProcessRequests()
      {
         _Active = true;

         while (_Active)
         {
            try
            {
               IAsyncResult result = _HttpServer.BeginGetContext(ProcessRequest, _HttpServer);
               result.AsyncWaitHandle.WaitOne();
            }
            catch (Exception ex)
            {
               ExceptionHandler.LogException(ex);
            }
         }
      }

      internal void Start()
      {
         if (!_Enabled)
         {
            return;
         }

         try
         {
            _HttpServer.Start();
         }
         catch (HttpListenerException ex)
         {
            _Enabled = false;
            Console.WriteLine("Failed to start HTTP server: " + ex.Message);
            Console.WriteLine("Try running the server as admin");
            Console.WriteLine("The server will continue to work normally but the web interface won't be available");
            return;
         }

         _ProcessingThread = new Thread(ProcessRequests);
         _ProcessingThread.Priority = ThreadPriority.Lowest;
         _ProcessingThread.Start();
      }

      internal void Stop()
      {
         if (_Enabled)
         {
            _HttpServer.Stop();
            _Active = false;
            _ProcessingThread.Join();
            _HttpServer.Close();
         }
      }

      internal static void WriteResponse(HttpListenerResponse response, string responseText)
      {
         WriteResponse(response, "text/html", responseText);
      }

      internal static void WriteResponse(HttpListenerResponse response, string contentType, string responseText)
      {
         response.ContentEncoding = Encoding.UTF8;
         WriteResponse(response, contentType, Encoding.UTF8.GetBytes(responseText));
      }

      internal static void WriteResponse(HttpListenerResponse response, string contentType, byte[] data)
      {
         response.ContentType = contentType;
         response.ContentLength64 = data.Length;
         response.OutputStream.Write(data, 0, data.Length);
      }
   }
}
