using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Networking;

namespace Qbes.Client.Logic.GameProviders
{
   internal sealed partial class SinglePlayerProvider
   {
      #region Static fields
      private static IClientToServer _ClientToServerProvider = new SinglePlayerProvider.ClientToServer();
      private static IServerToClient _ServerToClientProvider = new SinglePlayerProvider.ServerToClient();
      #endregion

      internal static IClientToServer ClientToServerProvider
      {
         get
         {
            return _ClientToServerProvider;
         }
      }

      internal static IServerToClient ServerToClientProvider
      {
         get
         {
            return _ServerToClientProvider;
         }
      }
   }
}
