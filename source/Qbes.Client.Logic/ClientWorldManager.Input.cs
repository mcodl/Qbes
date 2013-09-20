using System;
using System.Drawing;
using System.Threading;

using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using Qbes.Common.Logic;

namespace Qbes.Client.Logic
{
   public sealed partial class ClientWorldManager
   {
      private string HandleTextKey(KeyboardEventArgs e)
      {
         if (e.Key == Key.Space)
         {
            return " ";
         }

         bool capsMod = e.Mod.HasFlag(ModifierKeys.Caps);
         bool shiftMod = (e.Mod.HasFlag(ModifierKeys.LeftShift) || e.Mod.HasFlag(ModifierKeys.RightShift));
         bool textShift = ((capsMod && !shiftMod) || (!capsMod && shiftMod));

         string c = e.KeyboardCharacter;

         // text shift applies only to alpha characters
         if ((int)e.Key >= 97 && (int)e.Key <= 122)
         {
            return (textShift ? c.ToUpper() : c);
         }

         if (shiftMod)
         {
            // other characters are influenced by shift and ignore caps
            if ((int)e.Key == 44 || (int)e.Key == 46 || (int)e.Key == 47)
            {
               return ((char)(e.Key + 16)).ToString();
            }
            else if ((int)e.Key >= 91 && (int)e.Key <= 93)
            {
               return ((char)(e.Key + 32)).ToString();
            }
            else
            {
               switch (e.Key)
               {
                  case Key.One:
                     return "!";
                  case Key.Two:
                     return "@";
                  case Key.Three:
                     return "#";
                  case Key.Four:
                     return "$";
                  case Key.Five:
                     return "%";
                  case Key.Six:
                     return "^";
                  case Key.Seven:
                     return "&";
                  case Key.Eight:
                     return "*";
                  case Key.Nine:
                     return "(";
                  case Key.Zero:
                     return ")";
                  case Key.Minus:
                     return "_";
                  case Key.Equals:
                     return "+";
                  case Key.Semicolon:
                     return ":";
                  case Key.Quote:
                     return "\"";
                  case Key.BackQuote:
                     return "~";
               }
            }
         }

         return c;
      }

      private void KeyDown(object sender, KeyboardEventArgs e)
      {
         if (_ChatBoxActive)
         {
            if (e.Key == Key.Backspace && _CurrentChatMessage.Length > 0)
            {
               _CurrentChatMessage = _CurrentChatMessage.Substring(0, _CurrentChatMessage.Length - 1);
            }
            else if (e.Key == Key.Escape)
            {
               _ChatBoxActive = false;
               _CurrentChatMessage = string.Empty;
            }
            else if (e.Key == Key.Return)
            {
               _ChatBoxActive = false;
               if (!string.IsNullOrEmpty(_CurrentChatMessage))
               {
                  _ChatHistory.Add(Player.PlayerName + ": " + _CurrentChatMessage);
                  WorldHelper.ClientToServerProvider.ChatMessage(Client.Connection, _CurrentChatMessage);
                  _CurrentChatMessage = string.Empty;
               }
            }
            else if ((int)e.Key >= 32 &&
                     (int)e.Key <= 126 &&
                     _CurrentChatMessage.Length < 200)
            {
               _CurrentChatMessage += HandleTextKey(e);
            }
         }
         else
         {
            switch (e.Key)
            {
               case Key.W:
                  _Player.MoveZ = -1;
                  break;
               case Key.S:
                  _Player.MoveZ = 1;
                  break;
               case Key.A:
                  _Player.MoveX = -1;
                  break;
               case Key.D:
                  _Player.MoveX = 1;
                  break;
               case Key.G:
                  _MouseGrab = !_MouseGrab;
                  Mouse.ShowCursor = !_MouseGrab;
#if DIAG
               DiagnosticsManager.WriteMessage("Mousegrab set to {0}", _MouseGrab);
#endif
                  break;
               case Key.F:
                  ToggleFog();
                  break;
               case Key.R:
                  NextMaterial(false);
                  break;
               case Key.T:
                  NextMaterial(true);
                  break;
               case Key.One:
               case Key.Two:
               case Key.Three:
               case Key.Four:
               case Key.Five:
               case Key.Six:
               case Key.Seven:
               case Key.Eight:
               case Key.Nine:
               case Key.Zero:
                  _Player.SelectedMaterial = (ushort)((int)e.Key - 46);
                  break;
               case Key.Space:
                  _Player.MoveY = 1;
                  break;
               case Key.LeftControl:
                  _Player.MoveY = -1;
                  break;
               case Key.Return:
                  _ChatBoxActive = true;
                  break;
               case Key.Escape:
                  StopTimer();
                  TextureManager.Instance.UnloadTextures(false);

                  if (IsSinglePlayer)
                  {
                     Stop();
                  }
                  else
                  {
                     WorldHelper.ClientToServerProvider.DisconnectingNotification(_Client.Connection);
                     _Client.Connection.Disconnect("Player is quitting the client");
                  }
                  break;
               case Key.F1:
                  _ShowHud = !_ShowHud;
                  break;
               case Key.F3:
                  DiagnosticsManager.ShowHideDiagnosticsWindow();
                  break;
               case Key.F6:
                  Save(false);
                  break;
               case Key.F9:
                  // load
                  break;
               case Key.F11:
                  StopTimer();

                  // Toggle fullscreen
                  if (!_Screen.FullScreen)
                  {
                     _Screen = Video.SetVideoMode(_Width, _Height, true, true, true);
                     WindowAttributes();
                  }
                  else
                  {
                     _Screen = Video.SetVideoMode(_Width, _Height, true, true);
                     WindowAttributes();
                  }
                  Reshape();

#if DIAG
                  DiagnosticsManager.WriteMessage("Fullscreen set to {0}", _Screen.FullScreen);
#endif
                  break;
            }
         }
      }

      private void KeyUp(object sender, KeyboardEventArgs e)
      {
         if (_ChatBoxActive)
         {
            return;
         }

         switch (e.Key)
         {
            case Key.W:
            case Key.S:
               _Player.MoveZ = 0;
               break;
            case Key.A:
            case Key.D:
               _Player.MoveX = 0;
               break;
            case Key.Q:
               break;
            case Key.R:
               break;
            case Key.Space:
            case Key.LeftControl:
               _Player.MoveY = 0;
               break;
         }
      }

      private void MouseButtonDown(object sender, MouseButtonEventArgs e)
      {

      }

      private void MouseButtonUp(object sender, MouseButtonEventArgs e)
      {
         switch (e.Button)
         {
            case MouseButton.PrimaryButton:
               WorldHelper.ClientToServerProvider.PlaceOrRemoveCubeNotification(_Client.Connection, true, Player.Location, Player.RotationLeft, Player.RotationUp, Player.SelectedMaterial);
               break;
            case MouseButton.SecondaryButton:
               WorldHelper.ClientToServerProvider.PlaceOrRemoveCubeNotification(_Client.Connection, false, Player.Location, Player.RotationLeft, Player.RotationUp, Player.SelectedMaterial);
               break;
            case MouseButton.WheelDown:
               NextMaterial(false);
               break;
            case MouseButton.WheelUp:
               NextMaterial(true);
               break;
         }
      }

      private void MouseMotion(object sender, MouseMotionEventArgs e)
      {
         _Player.NewRotationLeft -= e.RelativeX * Configuration.Input.MouseSensitivity;
         _Player.NewRotationUp += e.RelativeY * Configuration.Input.MouseSensitivity;

         if (_MouseGrab)
         {
            CenterMouseCursor();
         }
      }
   }
}
