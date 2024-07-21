using System;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alchellmy
{
  /// <summary>
  /// Our fearless adventurer!
  /// </summary>
  class Player
  {
    // Animations
    private Animation idleAnimation;
    private Animation runAnimation;
    private Animation jumpAnimation;
    private Animation celebrateAnimation;
    private Animation dieAnimation;
    private SpriteEffects flip = SpriteEffects.None;
    private AnimationPlayer sprite;

    // Sounds
    // private SoundEffect killedSound;
    // private SoundEffect jumpSound;
    // private SoundEffect fallSound;

    public bool IsAlive
    {
      get { return isAlive; }
    }
    bool isAlive;

    // Physics state
    public Vector2 Position
    {
      get { return position; }
      set { position = value; }
    }
    Vector2 position;

    private float previousBottom;

    public Vector2 Velocity
    {
      get { return velocity; }
      set { velocity = value; }
    }
    Vector2 velocity;

    // Constants for controlling horizontal movement
    private const float MoveAcceleration = 30000.0f;
    private const float MaxMoveSpeed = 100000.0f;
    private const float GroundDragFactor = 0.48f;
    private const float AirDragFactor = 0.48f;

    
    private const float GravityAcceleration = 2400.0f;
    private const float MaxFallSpeed = 550.0f;

    // Input configuration
    private const float MoveStickScale = 1.0f;
    

    /// <summary>
    /// Gets whether or not the player's feet are on the ground.
    /// </summary>
    public bool IsOnGround
    {
      get { return isOnGround; }
    }
    bool isOnGround = true;

    /// <summary>
    /// Current user movement input.
    /// </summary>
    private float movement;

    // Jumping state

    private bool isFacingRight = true;
    private JumpState _jumpState = new();
    private DashState _dashState = new();

    private Rectangle localBounds;
    /// <summary>
    /// Gets a rectangle which bounds this player in world space.
    /// </summary>
    public Rectangle BoundingRectangle
    {
      get
      {
        int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
        int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;
        return new Rectangle(left, top, localBounds.Width, localBounds.Height);
      }
    }

    /// <summary>
    /// Constructors a new player.
    /// </summary>
    public Player(IServiceProvider serviceProvider, Vector2 position)
    {
      LoadContent(serviceProvider);
      Reset(position);
    }

    /// <summary>
    /// Loads the player sprite sheet and sounds.
    /// </summary>
    public void LoadContent(IServiceProvider serviceProvider)
    {
      var content = new ContentManager(serviceProvider, "Content");
      // Load animated textures.
      idleAnimation = new Animation(content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
      runAnimation = new Animation(content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
      jumpAnimation = new Animation(content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
      celebrateAnimation = new Animation(content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
      dieAnimation = new Animation(content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);

      // Calculate bounds within texture size.            
      int width = (int)(idleAnimation.FrameWidth * 0.4);
      int left = (idleAnimation.FrameWidth - width) / 2;
      int height = (int)(idleAnimation.FrameHeight * 0.8);
      int top = idleAnimation.FrameHeight - height;
      localBounds = new Rectangle(left, top, width, height);

      // Load sounds.            
      // killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
      // jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
      // fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
    }

    /// <summary>
    /// Resets the player to life.
    /// </summary>
    /// <param name="position">The position to come to life at.</param>
    public void Reset(Vector2 position)
    {
      Position = position;
      Velocity = Vector2.Zero;
      isAlive = true;
      sprite.PlayAnimation(idleAnimation);
    }

    /// <summary>
    /// Handles input, performs physics, and animates the player sprite.
    /// </summary>
    /// <remarks>
    /// We pass in all of the input states so that our game is only polling the hardware
    /// once per frame. We also pass the game's orientation because when using the accelerometer,
    /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
    /// </remarks>
    public void Update(
        GameTime gameTime,
        KeyboardState keyboardState,
        GamePadState gamePadState,
        Level level)
    {
      float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
      GetInput(keyboardState, gamePadState);
      ApplyPhysics(gameTime, level);

      if (_dashState.IsUpdateCooldown())
      {
        _dashState.CooldownTime += elapsed;
      }

      if (IsAlive && IsOnGround)
      {
        if (_dashState.State == DashStateEnum.PreventDash) {
          _dashState.State = DashStateEnum.NotDashing;
        }
        if (Math.Abs(Velocity.X) - 0.02f > 0)
        {
          sprite.PlayAnimation(runAnimation);
        }
        else
        {
          sprite.PlayAnimation(idleAnimation);
        }
      }
    }

    /// <summary>
    /// Gets player horizontal movement and jump commands from input.
    /// </summary>
    private void GetInput(
        KeyboardState keyboardState,
        GamePadState gamePadState)
    {
      bool isDashingInput = _dashState.IsInput(keyboardState, gamePadState);
      if (isDashingInput)
      {
        _dashState.State = IsOnGround ? DashStateEnum.GroundDashing : DashStateEnum.MidairDashing;
        _dashState.BtnKey = gamePadState.IsButtonDown(Buttons.RightShoulder) ? (int)Buttons.RightShoulder : (int)Keys.J;
      }
      else
      {
        // Get analog horizontal movement.
        movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

        // Ignore small movements to prevent running in place.
        if (Math.Abs(movement) < 0.5f)
          movement = 0.0f;
        // If any digital horizontal movement input is found, override the analog movement.
        if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
            keyboardState.IsKeyDown(Keys.Left) ||
            keyboardState.IsKeyDown(Keys.A))
        {
          movement = -1.0f;
          isFacingRight = false;
        }
        else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                keyboardState.IsKeyDown(Keys.Right) ||
                keyboardState.IsKeyDown(Keys.D))
        {
          movement = 1.0f;
          isFacingRight = true;
        }
      }

      _jumpState.IsJumping = _jumpState.IsInput(keyboardState, gamePadState);
      if (_dashState.IsInputUp(keyboardState, gamePadState))
      {
        _dashState.BtnKey = DashState.BUTTON_UP;
      }

    }

    /// <summary>
    /// Updates the player's velocity and position based on input, gravity, etc.
    /// </summary>
    public void ApplyPhysics(GameTime gameTime, Level level)
    {
      float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
      Vector2 previousPosition = Position;

      if (_dashState.IsDashing())
      {
        velocity.X = DoDash(gameTime);
      }
      else
      {
        // Base velocity is a combination of horizontal movement control and
        // acceleration downward due to gravity.
        velocity.X += movement * MoveAcceleration * elapsed;
        velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
        velocity.Y = DoJump(velocity.Y, gameTime);
        // Apply pseudo-drag horizontally.
        if (IsOnGround)
          velocity.X *= GroundDragFactor;
        else
          velocity.X *= AirDragFactor;

        // Prevent the player from running faster than his top speed.            
        velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
      }

      // Apply velocity.
      position.X += velocity.X * elapsed;
      if (!_dashState.IsDashing())
      {
        position.Y += velocity.Y * elapsed;
      }
      Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

      // If the player is now colliding with the level, separate them.
      HandleCollisions(level);

      // If the collision stopped us from moving, reset the velocity to zero.
      if (Position.X == previousPosition.X)
        velocity.X = 0;

      if (Position.Y == previousPosition.Y)
      {
        velocity.Y = 0;
        // cancel jump if overhead collision
        _jumpState.JumpTime = 0.0f;
        _jumpState.IsJumping = false;
      }
    }

    /// <summary>
    /// Calculates the Y velocity accounting for jumping and
    /// animates accordingly.
    /// </summary>
    /// <remarks>
    /// During the ascent of a jump, the Y velocity is completely
    /// overridden by a power curve. During the descent, gravity takes
    /// over. The jump velocity is controlled by the jumpTime field
    /// which measures time into the ascent of the current jump.
    /// </remarks>
    /// <param name="velocityY">
    /// The player's current velocity along the Y axis.
    /// </param>
    /// <returns>
    /// A new Y velocity if beginning or continuing a jump.
    /// Otherwise, the existing Y velocity.
    /// </returns>
    private float DoJump(float velocityY, GameTime gameTime)
    {
      // If the player wants to jump
      if (_jumpState.IsJumping && !_dashState.IsDashing())
      {
        // Begin or continue a jump
        if ((!_jumpState.WasJumping && IsOnGround) || _jumpState.JumpTime > 0.0f)
        {
          if (_jumpState.JumpTime == 0.0f)
          {
            // jumpSound.Play();
          }
          _jumpState.JumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
          sprite.PlayAnimation(jumpAnimation);
        }

        // If we are in the ascent of the jump
        if (0.0f < _jumpState.JumpTime && _jumpState.JumpTime <= JumpState.MaxJumpTime)
        {
          // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
          velocityY = JumpState.JumpLaunchVelocity * (1.0f - (float)Math.Pow(_jumpState.JumpTime / JumpState.MaxJumpTime, JumpState.JumpControlPower));
        }
        else
        {
          // Reached the apex of the jump
          _jumpState.JumpTime = 0.0f;
        }
      }
      else
      {
        // Continues not jumping or cancels a jump in progress
        _jumpState.JumpTime = 0.0f;
      }
      _jumpState.WasJumping = _jumpState.IsJumping;

      return velocityY;
    }

    private float DoDash(GameTime gameTime)
    {
      _dashState.Time += (float)gameTime.ElapsedGameTime.TotalSeconds;
      if (_dashState.Time >= DashState.DURATION)
      {
        _dashState.State = _dashState.State == DashStateEnum.MidairDashing ? DashStateEnum.PreventDash : DashStateEnum.NotDashing;
        _dashState.Time = 0.0f;
        _dashState.CooldownTime = 0.0f;
        return velocity.X;
      }
      return velocity.X + DashState.VELOCITY * (isFacingRight ? 1 : -1);
    }

    /// <summary>
    /// Detects and resolves collisions between the player and the level's tiles.
    /// </summary>
    /// <param name="level">
    /// The level.
    /// </param>
    private void HandleCollisions(Level level)
    {
      // Get the player's bounding rectangle and find neighboring tiles.
      Rectangle bounds = BoundingRectangle;
      int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
      int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
      int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
      int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

      // Reset flag to search for ground collision.
      isOnGround = false;

      // For each potentially colliding tile,
      for (int y = topTile; y <= bottomTile; ++y)
      {
        for (int x = leftTile; x <= rightTile; ++x)
        {
          // If this tile is collidable,
          TileCollision collision = level.GetCollision(x, y);
          if (collision != TileCollision.Passable)
          {
            // Determine collision depth (with direction) and magnitude.
            Rectangle tileBounds = level.GetBounds(x, y);
            Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
            if (depth != Vector2.Zero)
            {
              float absDepthX = Math.Abs(depth.X);
              float absDepthY = Math.Abs(depth.Y);

              // Resolve the collision along the shallow axis.
              if (absDepthY < absDepthX || collision == TileCollision.Platform)
              {
                // If we crossed the top of a tile, we are on the ground.
                if (previousBottom <= tileBounds.Top)
                  isOnGround = true;

                // Ignore platforms, unless we are on the ground.
                if (collision == TileCollision.Impassable || IsOnGround)
                {
                  // Resolve the collision along the Y axis.
                  Position = new Vector2(Position.X, Position.Y + depth.Y);

                  // Perform further collisions with the new bounds.
                  bounds = BoundingRectangle;
                }
              }
              else if (collision == TileCollision.Impassable)
              {
                _dashState.State = DashStateEnum.NotDashing;
                // Resolve the collision along the X axis.
                Position = new Vector2(Position.X + depth.X, Position.Y);

                // Perform further collisions with the new bounds.
                bounds = BoundingRectangle;
              }
            }
          }
        }
      }

      // Save the new bounds bottom.
      previousBottom = bounds.Bottom;
    }

    /// <summary>
    /// Draws the animated player.
    /// </summary>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      // Flip the sprite to face the way we are moving.
      if (Velocity.X > 0)
        flip = SpriteEffects.FlipHorizontally;
      else if (Velocity.X < 0)
        flip = SpriteEffects.None;

      // Draw that sprite.
      sprite.Draw(gameTime, spriteBatch, Position, flip, _dashState.IsDashing() ? Color.Black : Color.White);
    }
  }
}
