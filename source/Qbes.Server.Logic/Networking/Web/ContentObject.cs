using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Qbes.Server.Logic.Networking.Web
{
   /// <summary>
   /// Content objects derive from DynamicObject so that they can seamlessly
   /// store data that are used by WebPage implementations.
   /// </summary>
   public sealed class ContentObject : DynamicObject
   {
      #region Private fields
      private string _Name;
      private Dictionary<string, object> _Objects = new Dictionary<string, object>();
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a new content object instance.
      /// </summary>
      /// <param name="name">Content object name</param>
      public ContentObject(string name)
      {
         _Name = name;
      }
      #endregion

      internal IEnumerable<string> GetKeys()
      {
         return _Objects.Keys;
      }

      /// <summary>
      /// Gets an object by key (property name).
      /// </summary>
      /// <param name="key">Key (property name)</param>
      /// <returns>Object or null if not found</returns>
      public object Get(string key)
      {
         object result = null;
         return _Objects.TryGetValue(key, out result);
      }

      /// <summary>
      /// Gets the content object name.
      /// </summary>
      public string Name
      {
         get
         {
            return _Name;
         }
      }

      /// <summary>
      /// Stores an object by given key (property name).
      /// </summary>
      /// <param name="key">Key (property name)</param>
      /// <param name="value">Object to store</param>
      public void Set(string key, object value)
      {
         _Objects[key] = value;
      }

      /// <summary>
      /// Tries to retrieve an object by given member binder.
      /// </summary>
      /// <param name="binder">Member binder</param>
      /// <param name="result">Output object</param>
      /// <returns>True if successful</returns>
      public override bool TryGetMember(GetMemberBinder binder, out object result)
      {
         result = Get(binder.Name);
         return _Objects.ContainsKey(binder.Name);
      }

      /// <summary>
      /// Tries to store an object by given member binder.
      /// </summary>
      /// <param name="binder">Member binder</param>
      /// <param name="value">Object to store</param>
      /// <returns>True if successful</returns>
      public override bool TrySetMember(SetMemberBinder binder, object value)
      {
         Set(binder.Name, value);
         return true;
      }
   }
}
