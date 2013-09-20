using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Client.Logic.Exceptions
{
   internal sealed class NotClientSegmentException : ApplicationException
   {
      #region Constants
      private const string ErrorMessage = "Given segment instance is not of ClientSegment type";
      #endregion

      #region Constructors
      internal NotClientSegmentException()
         : base(ErrorMessage)
      {
      }
      #endregion
   }
}
