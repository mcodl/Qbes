using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Exceptions
{
   internal sealed class UnknownEntityTypeException : ApplicationException
   {
      #region Constants
      private const string ErrorMessageFormat = "Unknown entity type: {0}";
      #endregion

      #region Constructors
      internal UnknownEntityTypeException(byte entityType)
         : base(string.Format(ErrorMessageFormat, entityType))
      {
         // empty
      }
      #endregion
   }
}
