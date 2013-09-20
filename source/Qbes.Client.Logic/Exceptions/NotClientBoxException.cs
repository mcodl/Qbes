using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Client.Logic.Exceptions
{
   internal sealed class NotClientBoxException : ApplicationException
   {
      #region Constants
      private const string ErrorMessage = "Given box instance is not of ClientBox type";
      #endregion

      #region Constructors
      internal NotClientBoxException()
         : base(ErrorMessage)
      {
      }
      #endregion
   }
}
