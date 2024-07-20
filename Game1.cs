using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Platformer2D;

namespace Alchellmy;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Vector2 _baseScreenSize = new(800, 480);
    private Matrix _globalTransformation;
    private int _backBufferWidth, _backBufferHeight;
    private GamePadState _gamePadState;
    private KeyboardState _keyboardState;
    private Player _player;
    private Level _level;
    private GameState _currentGameState;
    private SpriteFont _font;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            IsFullScreen = false
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _currentGameState = GameState.StartMenu;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        ScalePresentationArea();
        _player = new Player(Services, new Vector2(400, 480));
        _level = new Level(Services, "Content/Levels/Level1.txt");
        _font = Content.Load<SpriteFont>("Sprites/GlobalFont");
    }

    // Work out how much we need to scale our graphics to fill the screen
    public void ScalePresentationArea()
    {
        _backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        _backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        float horScaling = _backBufferWidth / _baseScreenSize.X;
        float verScaling = _backBufferHeight / _baseScreenSize.Y;
        Vector3 screenScalingFactor = new(horScaling, verScaling, 1);
        _globalTransformation = Matrix.CreateScale(screenScalingFactor);
        Debug.WriteLine("Screen Size - Width[" + GraphicsDevice.PresentationParameters.BackBufferWidth + "] Height [" + GraphicsDevice.PresentationParameters.BackBufferHeight + "]");
    }

    protected override void Update(GameTime gameTime)
    {
        switch (_currentGameState)
        {
            case GameState.StartMenu:
                // Update logic for the start menu (if any)
                HandleInput(gameTime);
                base.Update(gameTime);
                break;

            case GameState.Playing:

                // Confirm the screen has not been resized by the user
                if (_backBufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
                        _backBufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
                {
                    ScalePresentationArea();
                }

                HandleInput(gameTime);
                _player.Update(gameTime, Keyboard.GetState(), GamePad.GetState(PlayerIndex.One), _level);
                base.Update(gameTime);
                break;

            case GameState.Paused:
                // Update logic for the pause menu (if any)
                HandleInput(gameTime);
                base.Update(gameTime);
                break;
        }
    }

    private void HandleInput(GameTime gameTime)
    {
        // get all of our input states
        _keyboardState = Keyboard.GetState();
        _gamePadState = GamePad.GetState(PlayerIndex.One);

        switch (_currentGameState)
        {
            case GameState.StartMenu:
                if (_keyboardState.IsKeyDown(Keys.Enter) || _gamePadState.IsButtonDown(Buttons.Start))
                {
                    _currentGameState = GameState.Playing;
                }
                break;

            case GameState.Playing:
                if (_keyboardState.IsKeyDown(Keys.P) || _gamePadState.IsButtonDown(Buttons.Start))
                {
                    _currentGameState = GameState.Paused;
                }
                break;

            case GameState.Paused:
                if (_keyboardState.IsKeyDown(Keys.Enter) || _gamePadState.IsButtonDown(Buttons.Start))
                {
                    _currentGameState = GameState.Playing;
                }
                else if (_keyboardState.IsKeyDown(Keys.Escape))
                {
                    Exit(); // Exit the game
                }
                break;
        }
    }


    protected override void Draw(GameTime gameTime)
    {
        _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, _globalTransformation);

        switch (_currentGameState)
        {
            case GameState.StartMenu:
                DrawStartMenu(gameTime);
                break;

            case GameState.Playing:
                _level.Draw(gameTime, _spriteBatch);
                _player.Draw(gameTime, _spriteBatch);
                break;

            case GameState.Paused:
                _level.Draw(gameTime, _spriteBatch);
                _player.Draw(gameTime, _spriteBatch);
                DrawPauseMenu(gameTime);
                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected void DrawStartMenu(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        string message = "Press Enter or Start to Play!";
        Vector2 size = _font.MeasureString(message);
        _spriteBatch.DrawString(_font, message, _baseScreenSize / 2 - size / 2, Color.White);
    }

    protected void DrawPauseMenu(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        string message = "Game Paused\nPress Enter or Start to Resume";
        Vector2 size = _font.MeasureString(message);
        _spriteBatch.DrawString(_font, message, _baseScreenSize / 2 - size / 2, Color.White);
    }
}