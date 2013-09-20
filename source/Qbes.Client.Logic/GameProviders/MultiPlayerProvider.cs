using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qbes.Common.Logic.Networking;

namespace Qbes.Client.Logic.GameProviders
{
   internal sealed partial class MultiPlayerProvider
   {
      #region Static fields
      private static ClientToServer _ClientToServerProvider = new MultiPlayerProvider.ClientToServer();
      private static ServerToClient _ServerToClientProvider = new MultiPlayerProvider.ServerToClient();
      #endregion

      internal static ClientToServer ClientToServerProvider
      {
         get
         {
            return _ClientToServerProvider;
         }
      }

      internal static ServerToClient ServerToClientProvider
      {
         get
         {
            return _ServerToClientProvider;
         }
      }
   }
}
