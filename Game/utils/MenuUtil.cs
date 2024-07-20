using Microsoft.Xna.Framework.Input;

namespace Platformer2D 
{
  static class MenuUtil
  {
    public static int GetMenuButtonValue(KeyboardState keyboardState, GamePadState gamePadState) {
      if (keyboardState.IsKeyDown(Keys.Enter)) 
      {
        return (int)Keys.Enter;
      }
      else if (gamePadState.IsButtonDown(Buttons.Start))
      {
        return (int)Buttons.Start;
      }
      return (int)Buttons.None;
    }
    public static bool IsMenuButtonDown(int btnValue) {
      if (btnValue == (int)Buttons.None) return false;
      return btnValue == ((int)Keys.Enter) || btnValue == ((int)Buttons.Start);
    }
    public static bool IsMenuButtonUp(int btnValue) {
      return btnValue == (int)Buttons.None;
    }
  }
}