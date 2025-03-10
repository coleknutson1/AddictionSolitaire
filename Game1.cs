#region TODO
//Switch to Hashsets
#endregion

using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

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

	// Game components and content to load
	private GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;
	private Texture2D _deckSpriteSheet;
	private SpriteFont _font;
	private SoundEffect _cardFlick;

	// Game state
	private List<Card> _deck;
	private Card _currentlySelectedCard;
	private MouseState _previousMouseState = Mouse.GetState();
	private Card _currentlyHighlightedCard;

	//Initialize FPS Class
	private FrameCounter _frameCounter = new FrameCounter();

	//Card strobe vars
	private int _i_strobe = 0;
	private bool _strobe_on = true;
	private const int LENGTH_OF_STROBE = 5; //frames
	private const int LENGTH_OF_STROBE_MAX = LENGTH_OF_STROBE * 2; //reset

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
		for (int suitIndex = 0; suitIndex < suits.Length; suitIndex++)
		{
			for (int rank = 0; rank <= 13; rank++)
			{

				_deck.Add(new Card(
					new Rectangle(32 * suitIndex, 48 * rank, 32, 48),
					suits[suitIndex],
					rank,
					rank == 13,
					new Vector2(rank, suitIndex)
				));
			}
		}
		foreach(var mt in _deck.Where(x=>x.m_rank == 13))
		{
			mt.m_rank = 99;
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
		_cardFlick = Content.Load<SoundEffect>("flick");
	}

	private void PlaceCorrectCardAtSelectedEmptySpot()
	{
		// Get the index of currently selected and swap card.
		var _cs_index = _deck.IndexOf(_currentlySelectedCard);
		var _sc_index = _deck.IndexOf(_currentlyHighlightedCard);

		// The two cards need to exchange rank/suit values but retain their index as that is how we sort.
		var _temp_rank = _currentlySelectedCard.m_rank;
		var _temp_suit = _currentlySelectedCard.m_suit;
		var _temp_spritesheetBlitRect = _currentlySelectedCard.m_SpritesheetBlitRect;
		var _temp_gridLocation = _currentlySelectedCard.m_currentGridLocation;
		var _temp_isEmpty = _currentlySelectedCard.m_isEmpty;

		//Play sound of updating
		_cardFlick.Play();
		_deck[_cs_index].UpdateRankAndSuit(_currentlyHighlightedCard.m_rank, _currentlyHighlightedCard.m_suit, _currentlyHighlightedCard.m_SpritesheetBlitRect, _currentlyHighlightedCard.m_currentGridLocation, _currentlyHighlightedCard.m_isEmpty);
		_deck[_sc_index].UpdateRankAndSuit(_temp_rank, _temp_suit, _temp_spritesheetBlitRect, _temp_gridLocation, _temp_isEmpty);
	}

	/// <summary>
	/// Helper function to check if the latest mouse state is different from previous cycle's.
	/// </summary>
	/// <returns></returns>
	private bool IsMouseJustPressed() =>
		Mouse.GetState().LeftButton == ButtonState.Pressed &&
		_previousMouseState.LeftButton == ButtonState.Released;

	protected override void Update(GameTime gameTime)
	{
		HandleStrobe();
		//We need to brute force check on which card is hovered over for highlighting the matching card spot
		//Needs optimization. n=52
		_currentlySelectedCard = _currentlyHighlightedCard = null;
		HandleMouseHighlightingAndLogic();

		//Handle the mouse interactions
		if (_currentlyHighlightedCard != null && _currentlySelectedCard != null && IsMouseJustPressed())
		{
			PlaceCorrectCardAtSelectedEmptySpot();
		}

		// Exit Game
		if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
			Keyboard.GetState().IsKeyDown(Keys.Escape))
			Exit();

		base.Update(gameTime);
		_previousMouseState = Mouse.GetState();
	}

	private void HandleStrobe()
	{
		if (_currentlyHighlightedCard == null)
		{
			_i_strobe = 0;
			return;
		}
		_i_strobe++;
		if (_i_strobe == LENGTH_OF_STROBE)
		{
			_strobe_on = false;
		}
		if (_i_strobe == LENGTH_OF_STROBE_MAX)
		{
			_strobe_on = true;
			_i_strobe = 0;
		}
	}

	/// <summary>
	/// See if we are hovering over a card and, if applicable, show a valid spot for that card.
	/// </summary>
	private void HandleMouseHighlightingAndLogic()
	{
		var mousePos = Mouse.GetState().Position;
		foreach (var card in _deck)
		{
			if (card.m_PositionRect.Intersects(new Rectangle(mousePos, Point.Zero)))
			{
				_currentlySelectedCard = card;
				
				//TODO: If they have multiple first rank slots open for aces we need to allow them to pick their ace.

				//If the hover over an empty slot, check the card before, then get rank+1 and same suit of that card.
				if (_currentlySelectedCard.m_isEmpty)
				{
					//If in first column, we should not follow this logic.
					if (_deck.IndexOf(_currentlySelectedCard) % 14 == 0)
						break;
					var _card_before_currently_selected = _deck[_deck.IndexOf(_currentlySelectedCard) - 1];
					_currentlyHighlightedCard = _deck.FirstOrDefault(x => x.m_rank == _card_before_currently_selected.m_rank + 1 && x.m_suit == _card_before_currently_selected.m_suit);
					break;
				}

				//Get the index of the card that is 1 lower and of the same suit than our currently selected. Plus one as that is a valid spot for it.
				var _index_of_valid_card_move = _deck.IndexOf(
					_deck.FirstOrDefault(x => x.m_suit == _currentlySelectedCard.m_suit
					&& x.m_rank == _currentlySelectedCard.m_rank - 1)) + 1;

				//If it would be an out of bounds, just break prematurely
				if (_index_of_valid_card_move >= _deck.Count)
					break;

				//Immediately find the slot that swap card that corresponds to currently selected IF it's empty.
				_currentlyHighlightedCard = _deck[_index_of_valid_card_move].m_isEmpty ? _deck[_index_of_valid_card_move] : null;
				break;
			}
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.ForestGreen);

		_spriteBatch.Begin();
		var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_frameCounter.Update(deltaTime);
		var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);

		// Draw out every card.
		foreach (var card in _deck)
		{

			// Hint highlighting.
			if (card == _currentlyHighlightedCard && _strobe_on)
			{
				_spriteBatch.Draw(_deckSpriteSheet, card.m_PositionRect, card.m_SpritesheetBlitRect, new Color(new Vector4(221, 100, 66, .5f)));
			}
			else
			{
				_spriteBatch.Draw(_deckSpriteSheet, card.m_PositionRect, card.m_SpritesheetBlitRect, Color.White);
			}
		}

		_spriteBatch.End();
		base.Draw(gameTime);

		var currentlyHighlightedText = _currentlyHighlightedCard == null ? "" : $"{_currentlyHighlightedCard.m_suit}:{_currentlyHighlightedCard.m_rank}";
		Window.Title = $"Card Game -  - {fps}";
	}
}