using Microsoft.Xna.Framework.Input;

namespace Alchellmy;

public struct JumpState
{
  public const float JumpControlPower = 0.14f;
  public const float JumpLaunchVelocity = -2500.0f;
  public const float MaxJumpTime = 0.7f;
  public const int BUTTON_UP = (int)Buttons.None;
  public bool IsJumping = false;
  public bool WasJumping = false;
  public float JumpTime = 0.0f;
  public JumpState()
  {
  }

  public bool IsInput(KeyboardState keyboardState, GamePadState gamePadState)
  {
    return (
            gamePadState.IsButtonDown(Buttons.A) ||
            keyboardState.IsKeyDown(Keys.Space) ||
            keyboardState.IsKeyDown(Keys.Up) ||
            keyboardState.IsKeyDown(Keys.W)
          );
  }

}