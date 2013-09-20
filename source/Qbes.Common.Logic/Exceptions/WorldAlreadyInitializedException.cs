using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qbes.Common.Logic.Exceptions
{
   /// <summary>
   /// This exception is used for the error when the world has been already
   /// initialized.
   /// </summary>
   public sealed class WorldAlreadyInitializedException : ApplicationException
   {
      #region Constants
      private const string ErrorMessage = "The world is already initialized";
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a new exception instance.
      /// </summary>
      public WorldAlreadyInitializedException()
         : base(ErrorMessage)
      {
      }
      #endregion
   }
}
