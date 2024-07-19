using System;
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

	public Game1()
	{
		_graphics = new GraphicsDeviceManager(this)
		{
			IsFullScreen = false
		};
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
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
		// Confirm the screen has not been resized by the user
		if (_backBufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
				_backBufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
		{
			ScalePresentationArea();
		}
		HandleInput(gameTime);
		_player.Update(gameTime, Keyboard.GetState(), GamePad.GetState(PlayerIndex.One));
		base.Update(gameTime);
	}

	private void HandleInput(GameTime gameTime)
	{
		// get all of our input states
		_keyboardState = Keyboard.GetState();
		_gamePadState = GamePad.GetState(PlayerIndex.One);
	}


	protected override void Draw(GameTime gameTime)
	{
		_graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
		_spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, _globalTransformation);
		_player.Draw(gameTime, _spriteBatch);
		_spriteBatch.End();
		base.Draw(gameTime);
	}
}
