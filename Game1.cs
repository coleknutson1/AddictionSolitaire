using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace AddictionSolitaire;

public class Game1 : Game
{
	// Screen and card constants
	private const int ScreenWidth = 1920;
	private const int ScreenHeight = 1080;
	private const float CardScale = 2.25f;
	public static int CardWidth { get; private set; } = 32;
	public static int CardHeight { get; private set; } = 48;
	private readonly Vector2 TableStartPosition = new(150, 150);

	// Game components
	private GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;
	private Texture2D _deckSpriteSheet;
	private SpriteFont _font;

	// Game state
	private List<Card> _deck;
	private Card _currentlySelectedCard;
	private MouseState _previousMouseState = Mouse.GetState();
	private Vector2 _currentOffset;

	public Game1()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;

		ConfigureGraphics();
		InitializeDeck();
	}

	private void ConfigureGraphics()
	{
		_graphics.PreferredBackBufferWidth = ScreenWidth;
		_graphics.PreferredBackBufferHeight = ScreenHeight;
		_graphics.ApplyChanges();

		// Scale card dimensions
		CardWidth = (int)(CardWidth * CardScale);
		CardHeight = (int)(CardWidth * CardScale);
	}

	private void InitializeDeck()
	{
		_deck = new List<Card>();
		string[] suits = { "SPADES", "DIAMONDS", "CLUBS", "HEARTS" };

		for (int rank = 0; rank <= 13; rank++)
		{
			for (int suitIndex = 0; suitIndex < suits.Length; suitIndex++)
			{
				_deck.Add(new Card(
					new Rectangle(32 * suitIndex, 48 * rank, 32, 48),
					suits[suitIndex],
					rank,
					rank == 0,
					new Vector2(rank, suitIndex)
				));
			}
		}

		// Shuffle Cards
		DeckShuffler.Shuffle(_deck);
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
		// Exit condition
		if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
			Keyboard.GetState().IsKeyDown(Keys.Escape))
			Exit();

		HandleMouseInteraction();

		base.Update(gameTime);
		_previousMouseState = Mouse.GetState();
	}

	private void HandleMouseInteraction()
	{
		if (IsMouseJustPressed())
		{
			var mousePos = Mouse.GetState().Position;
			foreach (var card in _deck)
			{
				if (card.m_PositionRect.Intersects(new Rectangle(mousePos, Point.Zero)) && !card.m_isEmpty)
				{
					_currentlySelectedCard = card;
					_currentOffset = new Vector2(
						_currentlySelectedCard.m_PositionRect.Left,
						_currentlySelectedCard.m_PositionRect.Top
					) - mousePos.ToVector2();
					break;
				}
			}
		}
		else if (Mouse.GetState().LeftButton == ButtonState.Released)
		{
			_currentlySelectedCard = null;
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.ForestGreen);

		_spriteBatch.Begin();

		foreach (var card in _deck)
		{
			_spriteBatch.Draw(_deckSpriteSheet, card.m_PositionRect, card.m_SpritesheetBlitRect, Color.White);
			_spriteBatch.DrawString(
				_font,
				card.m_currentGridLocation.ToString(),
				new Vector2(card.m_PositionRect.X, card.m_PositionRect.Y + 64),
				Color.Blue
			);
		}

		_spriteBatch.End();
		base.Draw(gameTime);

		Window.Title = $"Card Game - {TableStartPosition}";
	}

	private bool IsMouseJustPressed() =>
		Mouse.GetState().LeftButton == ButtonState.Pressed &&
		_previousMouseState.LeftButton == ButtonState.Released;

	public record Card
	{
		public Card(Rectangle spritesheetRect, string suit, int rank, bool isEmpty, Vector2 gridLocation)
		{
			m_SpritesheetBlitRect = spritesheetRect;
			m_suit = suit;
			m_rank = rank;
			m_isEmpty = isEmpty;
			m_currentGridLocation = gridLocation;
			UpdatePositionRect();
		}

		public void UpdateLocation(Vector2 newLocation)
		{
			m_currentGridLocation = newLocation;
			UpdatePositionRect();
		}

		private void UpdatePositionRect()
		{
			m_PositionRect = new Rectangle(
				Game1.CardWidth * (int)m_currentGridLocation.X,
				Game1.CardHeight * (int)m_currentGridLocation.Y,
				Game1.CardWidth,
				Game1.CardHeight
			);
		}

		public Rectangle m_PositionRect { get; private set; }
		public bool m_isEmpty { get; }
		public Rectangle m_SpritesheetBlitRect { get; }
		public string m_suit { get; }
		public int m_rank { get; }
		public Vector2 m_currentGridLocation { get; private set; }
	}
}