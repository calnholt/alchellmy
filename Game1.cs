﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    private MouseState _mouseState;
    private Player _player;
    private Level _level;
    private GameState _currentGameState;
    private SpriteFont _font;
    private List<MenuButton> _menuButtons;
    private Texture2D _menuButtonTexture;
		private int _previousMenuBtn = (int)Buttons.None;

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

        LoadGUI();
    }

    protected void LoadGUI() 
    {
        _font = Content.Load<SpriteFont>("Sprites/GlobalFont");
        _menuButtonTexture = new Texture2D(GraphicsDevice, 1, 1);
        _menuButtonTexture.SetData(new[] { Color.Gray });
        _menuButtons = new List<MenuButton>
        {
            new MenuButton(_menuButtonTexture, _font, "Start", new Vector2(350, 200), Color.DarkGray, Keys.Enter, () => _currentGameState = GameState.Playing, GUIConstants.MENU_START_BUTTON, GUIConstants.MENU_START_BUTTON),
            new MenuButton(_menuButtonTexture, _font, "Resume", new Vector2(350, 200), Color.DarkGray, Keys.P, () => _currentGameState = GameState.Playing, GUIConstants.MENU_RESUME_BUTTON, GUIConstants.MENU_RESUME_BUTTON),
            new MenuButton(_menuButtonTexture, _font, "Exit", new Vector2(350, 300), Color.DarkGray, Keys.Escape, () => Exit(),  GUIConstants.MENU_EXIT_BUTTON, GUIConstants.MENU_EXIT_BUTTON)
        };
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
      
                //GUI Code will refactor out.
                foreach (var button in _menuButtons)
                {
                    button.Update(gameTime, Mouse.GetState(), Keyboard.GetState());
                }

                base.Update(gameTime);

                break;

            case GameState.Playing:
                // Refactor to GUI code? Confirm the screen has not been resized by the user
                if (_backBufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
                        _backBufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
                {
                    ScalePresentationArea();
                }

                // handle input after scaling the window and gui code?
                HandleInput(gameTime);

                _player.Update(gameTime, Keyboard.GetState(), GamePad.GetState(PlayerIndex.One), _level);

                base.Update(gameTime);

                break;

            case GameState.Paused:
                HandleInput(gameTime);

                //GUI Code will refactor out.
                foreach (var button in _menuButtons)
                {
                    button.Update(gameTime, Mouse.GetState(), Keyboard.GetState());
                }

                base.Update(gameTime);
                
                break;
        }
    }

    private void HandleInput(GameTime gameTime)
    {
        // get all of our input states
        _keyboardState = Keyboard.GetState();
        _gamePadState = GamePad.GetState(PlayerIndex.One);
				var currentMenuBtn = MenuUtil.GetMenuButtonValue(_keyboardState, _gamePadState);
				var isMenuButtonDown = MenuUtil.IsMenuButtonDown(currentMenuBtn);
				var isMenuButtonUp = MenuUtil.IsMenuButtonUp(currentMenuBtn);

 				// Exit the game
				if (_keyboardState.IsKeyDown(Keys.Escape))
				{
					Exit();
				}

				if (isMenuButtonDown && _previousMenuBtn != currentMenuBtn) 
				{
					HandleMenuToggle();
					_previousMenuBtn = currentMenuBtn;
				}
				else if (isMenuButtonUp) 
				{
					_previousMenuBtn = (int)Buttons.None;
				}
    }

		private void HandleMenuToggle() {
				switch (_currentGameState)
        {
            case GameState.StartMenu:
                {
                    _currentGameState = GameState.Playing;
										break;
                }
            case GameState.Playing:
                {
                    _currentGameState = GameState.Paused;
										break;
                }

            case GameState.Paused:
								{
										_currentGameState = GameState.Playing;
										break;
								}
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
                DrawPauseMenu(gameTime);
                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected void DrawStartMenu(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        var buttons = _menuButtons
            .Where(btn => btn.Code != GUIConstants.MENU_RESUME_BUTTON)
            .ToList();

        foreach (var button in buttons)
        {
            button.Draw(_spriteBatch);
        }
    }

    protected void DrawPauseMenu(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        var buttons = _menuButtons
            .Where(btn => btn.Code != GUIConstants.MENU_START_BUTTON)
            .ToList();

        foreach (var button in buttons)
        {
            button.Draw(_spriteBatch);
        }
    }
}