using Microsoft.Xna.Framework.Input;

namespace Platformer2D;

public enum DashStateEnum
{
  NotDashing,
  GroundDashing,
  MidairDashing,
  PreventDash,
}

public struct DashState
{
  public const float DURATION = 0.15f;
  public const float COOLDOWN_DURATION = 0.15f;
  public const float VELOCITY = 200.0f;
  public const int BUTTON_UP = (int)Buttons.None;
  public DashStateEnum State;
  public float Time = 0.0f;
  public float CooldownTime = COOLDOWN_DURATION;
  public int BtnKey = BUTTON_UP;

  public DashState()
  {
    State = DashStateEnum.NotDashing;
    Time = 0.0f;

  }
  public bool IsInput(KeyboardState keyboardState, GamePadState gamePadState)
  {
    if (State == DashStateEnum.PreventDash) return false;
    return CooldownTime >= COOLDOWN_DURATION &&
        (
            gamePadState.IsButtonDown(Buttons.RightShoulder) ||
            keyboardState.IsKeyDown(Keys.J)
        ) &&
        State == DashStateEnum.NotDashing &&
        BtnKey == BUTTON_UP;
  }

  public readonly bool IsDashing()
  {
    return State == DashStateEnum.GroundDashing ||
        State == DashStateEnum.MidairDashing;
  }

  public bool IsUpdateCooldown()
  {
    return CooldownTime < COOLDOWN_DURATION && !IsDashing();
  }
  public bool IsDashInputUp(KeyboardState keyboardState, GamePadState gamePadState)
  {
    if (BtnKey == (int)Buttons.RightShoulder)
    {
      return gamePadState.IsButtonUp(Buttons.RightShoulder);
    }
    if (BtnKey == (int)Keys.J)
    {
      return keyboardState.IsKeyUp(Keys.J);
    }
    return false;
  }
}