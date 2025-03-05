using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AddictionSolitaire;

public class Game1 : Game
{
	private int SCREEN_WIDTH = 1920;
	private int SCREEN_HEIGHT = 1080;
	private GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;
	private Texture2D _deckSpriteSheet;
	private SpriteFont _font;
	private List<Card> m_deck;
	public Game1()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
		_graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
		_graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
		_graphics.ApplyChanges();

		//Load the deck into the list
		for (var rank = 0; rank <= 12; rank++)
		{
			foreach (var suit in new List<string> { "SPADES", "DIAMONDS", "CLUBS", "HEARTS" })
			{
				Debug.WriteLine($"{rank} of {suit}");
			}
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(GraphicsDevice);
		_deckSpriteSheet = Content.Load<Texture2D>("deck");
		_font = Content.Load<SpriteFont>("font");
	}

	protected override void Update(GameTime gameTime)
	{
		if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			Exit();
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);
		_spriteBatch.Begin();

		for (var x = 0; x < 4; x++)
		{
			for (int y = 0; y < 13; y++)
			{
				_spriteBatch.Draw(_deckSpriteSheet, new Rectangle(48 * x, 72 * y, 48, 72), new Rectangle(32 * x, 48 * y, 32, 48), Color.White);
			}
		}
		_spriteBatch.End();
		base.Draw(gameTime);
		Window.Title = $@"{Mouse.GetState().Position.ToString()}   {IsMousePressed()}";
	}

	private static bool IsMousePressed()
	{
		return Mouse.GetState().LeftButton == ButtonState.Pressed;
	}
}


public struct Card
{
	public Card(Rectangle r1, Rectangle r2)
	{
		m_PositionRect = r1;
		m_SpritesheetBlitRect = r2;
	}
	public Rectangle m_PositionRect;
	public Rectangle m_SpritesheetBlitRect;
}