using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Server.Logic;

namespace Qbes.Server
{
   class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine("Starting server...");

         //ServerWorldManager.Instance.Reset("Hybrid", "Hybrid");
         //ServerWorldManager.Instance.CreateWorld(8, 8);
         ServerWorldManager.Instance.Reset("Hybrid", "Hybrid");
         ServerWorldManager.Instance.Start();
      }
   }
}
