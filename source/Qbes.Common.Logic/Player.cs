using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Qbes.Common.Logic.Constants;
using Qbes.Common.Logic.Extensions;

namespace Qbes.Common.Logic
{
   /// <summary>
   /// Instances of this class represent human controlled players.
   /// </summary>
   public sealed class Player : Entity
   {
      #region Constants
      /// <summary>
      /// Player entities can move.
      /// </summary>
      public const bool CanMove = true;
      /// <summary>
      /// Player entities can place blocks.
      /// </summary>
      public const bool CanPlaceBlocks = true;
      /// <summary>
      /// Height of the eyes from the center point.
      /// </summary>
      public const float EyeHeightFromCenter = 0.75f;
      private const int FixedSize = 19;
      private const float PlayerHalfSizeX = 0.325f;
      private const float PlayerHalfSizeY = 0.95f;
      private const int OffsetNameLength = 18;
      private const int OffsetName = 19;
      private const int OffsetSelectedMaterial = 0;
      private const int OffsetSkinHash = 2;
      /// <summary>
      /// Placeholder for default skin checksum should it not be stored.
      /// </summary>
      public const string PlaceholderSkinChecksum = "00000000000000000000000000000000";
      #endregion

      #region Private fields
      private string _PlayerName;
      private byte[] _PlayerNameBytes;
      private string _SkinChecksum = PlaceholderSkinChecksum;
      private byte[] _SkinChecksumBytes = new byte[16];
      #endregion

      #region Constructors
      /// <summary>
      /// Creates a default player instance.
      /// </summary>
      public Player()
         : base(EntityTypes.Player, PlayerHalfSizeX, PlayerHalfSizeY,
                EyeHeightFromCenter, CanMove, CanPlaceBlocks)
      {
         SelectedMaterial = Materials.Stone;
      }

      /// <summary>
      /// Creates a player at given location.
      /// </summary>
      /// <param name="location">Player location</param>
      /// <param name="name">Player name</param>
      public Player(Point3D location, string name)
         : this()
      {
         Location = location;
         PlayerName = name;
      }
      #endregion

      /// <summary>
      /// Gets the selected material.
      /// </summary>
      /// <returns>Selected material ID</returns>
      public override ushort GetSelectedMaterial()
      {
         return SelectedMaterial;
      }

      /// <summary>
      /// Gets the serialized size of player's data.
      /// </summary>
      /// <returns>Serialized size of player's data</returns>
      protected override int GetSerializedSize()
      {
         return PlayerSerializedSize;
      }

      /// <summary>
      /// Initializes this player from serialized data.
      /// </summary>
      /// <param name="data">Array with serialized data</param>
      /// <param name="offset">Offset</param>
      protected override void InitializeFromByteArray(ref byte[] data, int offset)
      {
         SelectedMaterial = BitConverter.ToUInt16(data, offset + OffsetSelectedMaterial);

         _SkinChecksumBytes = new byte[16];
         for (int i = 0; i < 16; i++)
         {
            _SkinChecksumBytes[i] = data[i + offset + OffsetSkinHash];
         }
         _SkinChecksum = _SkinChecksumBytes.GetHexaString();

         int nameLength = data[offset + OffsetNameLength];
         PlayerName = Encoding.ASCII.GetString(data, offset + OffsetName, nameLength);
      }

      /// <summary>
      /// Gets or sets the player name.
      /// </summary>
      public string PlayerName
      {
         get
         {
            return _PlayerName;
         }
         set
         {
            _PlayerName = value;
            _PlayerNameBytes = Encoding.ASCII.GetBytes(value);
         }
      }

      /// <summary>
      /// Gets the serialized size of the player portion.
      /// </summary>
      public int PlayerSerializedSize
      {
         get
         {
            return FixedSize + _PlayerNameBytes.Length;
         }
      }

      /// <summary>
      /// Gets or sets the selected material ID.
      /// </summary>
      public ushort SelectedMaterial { get; set; }

      /// <summary>
      /// Serializes the derived portion into given array at given offset.
      /// </summary>
      /// <param name="data">Target byte array</param>
      /// <param name="offset">Offset</param>
      protected override void Serialize(ref byte[] data, int offset)
      {
         BitConverter.GetBytes(SelectedMaterial).CopyTo(data, offset + OffsetSelectedMaterial);

         if (_SkinChecksumBytes != null)
         {
            _SkinChecksumBytes.CopyTo(data, offset + OffsetSkinHash);
         }

         data[offset + OffsetNameLength] = (byte)_PlayerNameBytes.Length;
         _PlayerNameBytes.CopyTo(data, offset + OffsetName);
      }

      /// <summary>
      /// Sets the skin hash.
      /// </summary>
      /// <param name="skinHash">New skin hash</param>
      public void SetSkinHash(byte[] skinHash)
      {
         _SkinChecksum = skinHash.GetHexaString();
         _SkinChecksumBytes = skinHash;
      }

      /// <summary>
      /// Gets skin checksum in a form of hexastring.
      /// </summary>
      public string SkinChecksum
      {
         get
         {
            return _SkinChecksum;
         }
      }

      /// <summary>
      /// Gets skin checksum in a form of byte array.
      /// </summary>
      public byte[] SkinChecksumBytes
      {
         get
         {
            return _SkinChecksumBytes;
         }
      }
   }
}
