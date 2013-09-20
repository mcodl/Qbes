using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Client.Logic.Exceptions
{
   internal sealed class DuplicateTerrainMessageException : ApplicationException
   {
      #region Constants
      private const string ErrorMessage = "Received terrain message with same part number";
      #endregion

      #region Constructors
      internal DuplicateTerrainMessageException()
         : base(ErrorMessage)
      {
      }
      #endregion
   }
}
