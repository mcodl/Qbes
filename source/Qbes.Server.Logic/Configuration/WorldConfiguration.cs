using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Qbes.Common.Logic;

namespace Qbes.Server.Logic.Configuration
{
   /// <summary>
   /// Instances of this configuration class are used to hold settings regarding
   /// the world and gameplay.
   /// </summary>
   public sealed class WorldConfiguration
   {
      #region Constants
      /// <summary>
      /// Default setting for whether the whole map should be loaded.
      /// </summary>
      public const bool DefaultLoadWholeMap = true;
      /// <summary>
      /// Default X coordinate for new player's spawn point.
      /// </summary>
      public const float DefaultNewPlayerSpawnX = 96.0f;
      /// <summary>
      /// Default Y coordinate for new player's spawn point.
      /// </summary>
      public const float DefaultNewPlayerSpawnY = 96.0f;
      /// <summary>
      /// Default Z coordinate for new player's spawn point.
      /// </summary>
      public const float DefaultNewPlayerSpawnZ = 96.0f;
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default instance.
      /// </summary>
      public WorldConfiguration()
      {
         LoadWholeMap = DefaultLoadWholeMap;
         NewPlayerSpawnPoint = new Point3D(DefaultNewPlayerSpawnX,
                                           DefaultNewPlayerSpawnY,
                                           DefaultNewPlayerSpawnZ);
      }
      #endregion

      /// <summary>
      /// Gets or sets whether the whole map should be loaded.
      /// </summary>
      [XmlElement("LoadWholeMap")]
      public bool LoadWholeMap { get; set; }

      /// <summary>
      /// Gets or sets the new player's spawn location.
      /// </summary>
      [XmlElement("NewPlayerSpawnPoint")]
      public Point3D NewPlayerSpawnPoint { get; set; }
   }
}
