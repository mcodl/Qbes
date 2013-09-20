using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Qbes.Server.Logic.Networking.Web.Pages
{
   internal sealed class Home : WebPage
   {
      public override string Header
      {
         get
         {
            return "Home";
         }
      }

      public override void ProcessRequest(HttpListenerRequest request, HttpListenerResponse response)
      {
         WriteResponse(response, ProcessTemplate("Home.xml"));
      }
   }
}
