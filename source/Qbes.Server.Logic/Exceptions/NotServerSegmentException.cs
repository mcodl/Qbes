using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Server.Logic.Exceptions
{
   internal sealed class NotServerSegmentException : ApplicationException
   {
      #region Constants
      private const string ErrorMessage = "Given segment instance is not of ServerSegment type";
      #endregion

      #region Constructors
      internal NotServerSegmentException()
         : base(ErrorMessage)
      {
      }
      #endregion
   }
}
