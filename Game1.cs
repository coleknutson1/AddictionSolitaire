using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

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
	private Card _currentlySelectedCard;
	private MouseState _previousMouseState = Mouse.GetState();
	private Vector2 _currentOffset;

	public Game1()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
		_graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
		_graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
		_graphics.ApplyChanges();
		//Load the deck into the list
		m_deck = new List<Card>();
		for (var rank = 0; rank <= 12; rank++)
		{
			int i = 0;
			foreach (var suit in new List<string> { "SPADES", "DIAMONDS", "CLUBS", "HEARTS" })
			{
				m_deck.Add(new Card(new Rectangle(48 * i, 72 * rank, 48, 72), new Rectangle(32 * i, 48 * rank, 32, 48), suit, rank));
				i++;
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

		if (IsMouseJustPressed())
		{
			var mouse_pos = Mouse.GetState().Position;
			foreach(var c in m_deck)
			{
				if (c.m_PositionRect.Intersects(new Rectangle(mouse_pos,Point.Zero)))
				{
					_currentlySelectedCard = c;
					_currentOffset = new Vector2(_currentlySelectedCard.m_PositionRect.Left, _currentlySelectedCard.m_PositionRect.Top) - mouse_pos.ToVector2();
					break;
				}
			}
		}
		else if (Mouse.GetState().LeftButton == ButtonState.Released)
		{
			_currentlySelectedCard = null;
		}
		else if(_currentlySelectedCard is not null) //Dragging card
		{
			_currentlySelectedCard.m_PositionRect = HandleCardPosition();
		}
			base.Update(gameTime);
		_previousMouseState = Mouse.GetState();
	}

	private Rectangle HandleCardPosition()
	{
		return new Rectangle(Mouse.GetState().Position+_currentOffset.ToPoint(), new Point(48, 72));
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);
		_spriteBatch.Begin();

		foreach (var c in m_deck)
		{
			_spriteBatch.Draw(_deckSpriteSheet, c.m_PositionRect, c.m_SpritesheetBlitRect, Color.White);
		}
		_spriteBatch.End();
		base.Draw(gameTime);
		Window.Title = $@"{Mouse.GetState().Position.ToString()}";
	}

	private bool IsMouseJustPressed()
	{
		return Mouse.GetState().LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
	}
}


public record Card
{
	public Card(Rectangle r1, Rectangle r2, string suit, int rank)
	{
		m_PositionRect = r1;
		m_SpritesheetBlitRect = r2;
		m_suit = suit;
		m_rank = rank;
	}
	public Rectangle m_PositionRect;
	public Rectangle m_SpritesheetBlitRect;
	public string m_suit;
	public int m_rank;
}