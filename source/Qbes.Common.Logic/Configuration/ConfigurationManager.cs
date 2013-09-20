using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Qbes.Common.Logic.Configuration
{
   /// <summary>
   /// The configuration manager provides the basic functions to access and
   /// modify the configuration file.
   /// </summary>
   public static class ConfigurationManager
   {
      #region Static fields
      private static readonly string _Path = Path.Combine("..", "..", "Config");
      private static readonly string _PathFormat = Path.Combine(_Path, "{0}.xml");
      #endregion

      private static XmlSerializer GetTypeData<T>(out Type configType, out string filePath)
         where T : new()
      {
         configType = typeof(T);
         filePath = string.Format(_PathFormat, configType.FullName);

         return new XmlSerializer(configType);
      }

      /// <summary>
      /// Reads the configuration file for that type. If the file doesn't exist
      /// than it creates a new file with default settings.
      /// </summary>
      /// <typeparam name="T">Configuration type</typeparam>
      /// <returns>Parsed configuration file</returns>
      public static T ReadConfiguration<T>()
         where T : new()
      {
         Type configType;
         string filePath;
         XmlSerializer xs = GetTypeData<T>(out configType, out filePath);

         if (!File.Exists(filePath))
         {
            SaveConfiguration<T>(new T());
         }

         using (StreamReader reader = new StreamReader(filePath))
         {
            return (T)xs.Deserialize(reader);
         }
      }

      /// <summary>
      /// Saves the given configuration object into its configuration file.
      /// </summary>
      /// <typeparam name="T">Configuration type</typeparam>
      /// <param name="configuration">Object with config data</param>
      public static void SaveConfiguration<T>(T configuration)
         where T : new()
      {
         if (!Directory.Exists(_Path))
         {
            Directory.CreateDirectory(_Path);
         }

         Type configType;
         string filePath;
         XmlSerializer xs = GetTypeData<T>(out configType, out filePath);

         using (StreamWriter writer = new StreamWriter(filePath))
         {
            xs.Serialize(writer, configuration);
         }
      }
   }
}
