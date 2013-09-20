using System;
using System.Collections.Generic;
using System.Diagnostics;

using Qbes.Client.Logic;
using Qbes.Common.Logic;

namespace Qbes.Client
{
   class Program
   {
      [STAThread]
      public static void Main()
      {
         Console.WriteLine("Starting client...");

         ClientWorldManager.Instance.Reset("ProcTest", "ProcTest");
         //ClientWorldManager.Instance.CreateWorld(8, 8);
         ClientWorldManager.Instance.Start();
      }
   }
}
