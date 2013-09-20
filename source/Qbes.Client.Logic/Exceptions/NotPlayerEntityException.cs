using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Client.Logic.Exceptions
{
   internal sealed class NotPlayerEntityException : Exception
   {
      #region Constants
      private const string ErrorMessage = "Given entity is not of player type";
      #endregion

      #region Constructors
      internal NotPlayerEntityException()
         : base(ErrorMessage)
      {
         // empty
      }
      #endregion
   }
}
