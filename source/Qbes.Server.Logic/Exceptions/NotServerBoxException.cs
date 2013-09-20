using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Server.Logic.Exceptions
{
   internal sealed class NotServerBoxException : ApplicationException
   {
      #region Constants
      private const string ErrorMessage = "Given box instance is not of ServerBox type";
      #endregion

      #region Constructors
      internal NotServerBoxException()
         : base(ErrorMessage)
      {
      }
      #endregion
   }
}
