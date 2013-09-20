using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Qbes.Common.Logic.Networking;

namespace Qbes.Common.Logic.Exceptions
{
   /// <summary>
   /// Handled task is wrapping the standard .Net Task with exception handling
   /// and optionally also a lock.
   /// </summary>
   public sealed class HandledTask : IDisposable
   {
      #region Private fields
      private Task _Task;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a new handled task.
      /// </summary>
      /// <param name="action">Task action</param>
      public HandledTask(Action action)
      {
         _Task = new Task(() =>
         {
            try
            {
               action();
            }
            catch (Exception ex)
            {
               ExceptionHandler.LogException(ex);
            }
         });
      }

      /// <summary>
      /// Creates a new synchronized handled task.
      /// </summary>
      /// <param name="action">Task action</param>
      /// <param name="lockObject">Lock object</param>
      [Obsolete]
      public HandledTask(Action action, Object lockObject)
         : this(() => {
            lock (lockObject)
            {
               action();
            }
         })
      {
         // empty
      }
      #endregion

      /// <summary>
      /// Releases the held task.
      /// </summary>
      public void Dispose()
      {
         _Task.Dispose();
         _Task = null;
      }

      /// <summary>
      /// Starts the task.
      /// </summary>
      public void Start()
      {
         _Task.Start();
      }
   }
}
