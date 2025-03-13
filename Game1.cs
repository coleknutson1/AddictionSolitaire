#region TODO
//Switch to Hashsets
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddictionSolitaire;

public class Game1 : Game
{
	// Screen and card constants
	private const int SCREEN_WIDTH = 640;
	private const int SCREEN_HEIGHT = 360;
	private const float CARD_SCALE = 2f;
	private const int SCREEN_SCALE = 2;
	private Rectangle BLACK_RECT = new Rectangle(64, 576, 32, 48);
	public static int CardWidth { get; private set; } = 32;
	public static int CardHeight { get; private set; } = 48;
	private readonly Vector2 TableStartPosition = new(150, 150);

	// Game components and content to load
	private GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;
	private Texture2D _deckSpriteSheet;
	private SpriteFont _font;
	private SoundEffect _cardFlick;
	private Texture2D _shuffleButton;
	private Texture2D _background;

	// Game state
	private List<Card> _deck;
	private Card _currentlySelectedCard;
	private MouseState _previousMouseState = Mouse.GetState();
	private KeyboardState _previousKeyboardState = Keyboard.GetState();
	private Card _currentlyHighlightedCard;
	private Card _pocketCard;

	//Initialize FPS Class
	private FrameCounter _frameCounter = new FrameCounter();

	//Card strobe vars
	private int _i_strobe = 0;
	private bool _strobe_on = true;
	private Rectangle _shuffleButtonRectangle;
	private const int LENGTH_OF_STROBE = 5; //frames
	private const int LENGTH_OF_STROBE_MAX = LENGTH_OF_STROBE * 2; //reset

	#region Initialization
	public Game1()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
		_shuffleButtonRectangle = new Rectangle(new Point(0, 0), new Point(128, 128));
		ConfigureGraphics();
		InitializeDeck();
	}

	private void ConfigureGraphics()
	{
		// In your game initialization
		_graphics.PreferredBackBufferWidth = SCREEN_WIDTH * SCREEN_SCALE;
		_graphics.PreferredBackBufferHeight = SCREEN_HEIGHT * SCREEN_SCALE;
		_graphics.ApplyChanges();

		// Scale card dimensions
		CardWidth = (int)(CardWidth * CARD_SCALE);
		CardHeight = (int)(CardWidth * CARD_SCALE);
	}

	private void InitializeDeck()
	{
		_deck = new List<Card>();
		string[] suits = { "SPADES", "DIAMONDS", "CLUBS", "HEARTS" };
		for (int suitIndex = 0; suitIndex < suits.Length; suitIndex++)
		{
			for (int rank = 0; rank <= 12; rank++)
			{
				_deck.Add(new Card(
					new Rectangle(32 * suitIndex, 48 * rank, 32, 48),
					suits[suitIndex],
					rank,
					rank == 12,
					new Vector2(rank, suitIndex)
				));
			}
		}
		foreach (var mt in _deck.Where(x => x.m_rank == 12))
		{
			mt.m_rank = 99;
		}
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
		_shuffleButton = Content.Load<Texture2D>("shuffle");
		_background = Content.Load<Texture2D>("tabletop");
	}

	#endregion

	#region InputHandlers
	private bool HandleMouseHighlightingAndLogic()
	{
		var mousePos = Mouse.GetState().Position;

		//Loop over deck in order to find the card with the mouse over it. TODO: Optimize
		foreach (var card in _deck)
		{
			//If we are hovering over a card
			if (card.m_PositionRect.Intersects(new Rectangle(mousePos, Point.Zero)))
			{
				//Update global variable
				_currentlySelectedCard = card;

				//Hovering over an empty slot, therefore highlight the valid card slot (if applicable).
				if (_currentlySelectedCard.m_isEmpty)
				{
					if (_deck.IndexOf(_currentlySelectedCard) % 13 == 0)
						return false;

					var _card_before_currently_selected = _deck[_deck.IndexOf(_currentlySelectedCard) - 1];
					_currentlyHighlightedCard = _deck.FirstOrDefault(x => x.m_rank == _card_before_currently_selected.m_rank + 1 && x.m_suit == _card_before_currently_selected.m_suit);
					return true;
				}

				//If they are hovering over a 2, show them the first available left-most column (if applicable).
				else if (_currentlySelectedCard.m_rank == 0)
				{
					_currentlyHighlightedCard = _deck.Where((x, index) => x.m_isEmpty && index % 13 == 0)?.FirstOrDefault();
					return true;
				}

				//Otherwise search for an available slot for currently selected card.
				var _index_of_valid_card_move = _deck.IndexOf(
								_deck.FirstOrDefault(x => x.m_suit == _currentlySelectedCard.m_suit
								&& x.m_rank == _currentlySelectedCard.m_rank - 1)) + 1;

				//Don't go out of bounds!
				if (_index_of_valid_card_move >= _deck.Count)
					return false;

				_currentlyHighlightedCard = _deck[_index_of_valid_card_move].m_isEmpty ? _deck[_index_of_valid_card_move] : null;
				return true;
			}
		}


		return false;
	}

	private bool PlaceCorrectCardAtSelectedEmptySpot()
	{
		if (_currentlySelectedCard == null || _currentlyHighlightedCard == null)
			return false;

		// Get the currently selected and highlighted cards.
		var selectedCard = _currentlySelectedCard;
		var highlightedCard = _currentlyHighlightedCard;

		// The two cards need to exchange rank/suit values but retain their index as that is how we sort.
		var _temp_rank = selectedCard.m_rank;
		var _temp_suit = selectedCard.m_suit;
		var _temp_spritesheetBlitRect = selectedCard.m_SpritesheetBlitRect;
		var _temp_gridLocation = selectedCard.m_currentGridLocation;
		var _temp_isEmpty = selectedCard.m_isEmpty;

		// Play sound of updating
		_cardFlick.Play();
		selectedCard.UpdateRankAndSuit(highlightedCard.m_rank, highlightedCard.m_suit, highlightedCard.m_SpritesheetBlitRect, highlightedCard.m_currentGridLocation, highlightedCard.m_isEmpty);
		highlightedCard.UpdateRankAndSuit(_temp_rank, _temp_suit, _temp_spritesheetBlitRect, _temp_gridLocation, _temp_isEmpty);

		return true;
	}


	private bool IsMouseJustPressed(bool isLeftPressed = true) =>
	(isLeftPressed && Mouse.GetState().LeftButton == ButtonState.Pressed &&
	_previousMouseState.LeftButton == ButtonState.Released) || (
	!isLeftPressed && Mouse.GetState().RightButton == ButtonState.Pressed &&
	_previousMouseState.RightButton == ButtonState.Released);

	private void HandleFullscreen()
	{
		var keyboardState = Keyboard.GetState();
		if (keyboardState.IsKeyDown(Keys.F11) && _previousKeyboardState.IsKeyUp(Keys.F11))
		{
			_graphics.IsFullScreen = !_graphics.IsFullScreen;
			_graphics.ApplyChanges();
		}
		_previousKeyboardState = keyboardState;
	}
	#endregion

	#region PocketCard
	/// <summary>
	/// Place the card we are right-clicking into the pocket slot.
	/// </summary>
	/// <returns>Boolean value evaluating if we actually placed a card in the pocket slot.</returns>
	/// 
	private bool PlaceCardInPocketSlot()
	{
		//Retrieve index of currently selected and bail if null or if there is already a card in _pocketCard.
		var indexOfCurrentlySelectedCard = _deck.FindIndex(x => x == _currentlySelectedCard);
		if (indexOfCurrentlySelectedCard == -1 || _currentlySelectedCard.m_isEmpty || _pocketCard != null)
		{
			return false;
		}

		//Place currently selected into pocket slot. Then pick currently selected from deck and empty it.
		_pocketCard = new Card();
		_pocketCard.UpdateRankAndSuit(_currentlySelectedCard.m_rank, _currentlySelectedCard.m_suit, _currentlySelectedCard.m_SpritesheetBlitRect, _currentlySelectedCard.m_currentGridLocation, _currentlySelectedCard.m_isEmpty);
		_pocketCard.m_PositionRect = GetPocketCardRectangle();
		var _temp_location = new Vector2(_pocketCard.m_PositionRect.X, _pocketCard.m_PositionRect.Y);

		_deck[indexOfCurrentlySelectedCard]
			.UpdateRankAndSuit(99, null, BLACK_RECT, _temp_location, true);


		return true;
	}
	
	private bool HandlePocketCardClick()
	{
		//If pocket card is null or if we aren't clicking it, return false.
		if (_pocketCard == null || !_pocketCard.m_PositionRect.Intersects(new Rectangle(Mouse.GetState().Position, Point.Zero)))
		{
			return false;
		}
		
		//If we have a highlighted card, then we know we can place it there, so handle that.
		if(_currentlyHighlightedCard != null)
		{
			_currentlyHighlightedCard.UpdateRankAndSuit(_pocketCard.m_rank, _pocketCard.m_suit, _pocketCard.m_SpritesheetBlitRect, _currentlyHighlightedCard.m_currentGridLocation, false);
			_pocketCard = new Card();
		}

		return true;
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

	private static Rectangle GetPocketCardRectangle()
	{
		return new Rectangle(SCREEN_WIDTH * SCREEN_SCALE - 100, SCREEN_HEIGHT * SCREEN_SCALE - 300, Game1.CardWidth, Game1.CardHeight);
	}
	#endregion

	#region Update/Draw
	protected override void Update(GameTime gameTime)
	{
		HandleFullscreen();
		HandleStrobe();

		_currentlySelectedCard = _currentlyHighlightedCard = null;
		HandleMouseHighlightingAndLogic();

		//Check if mouse is pressed and if they are clicking the pocket slot. If they aren't, see if they are clicking a valid card.
		if (IsMouseJustPressed() && !HandlePocketCardClick())
		{
			PlaceCorrectCardAtSelectedEmptySpot();
		}

		//If they right click a card, set it as the pocket card and set its slot to empty.
		else if (IsMouseJustPressed(false))
		{
			PlaceCardInPocketSlot();
		}

		//Handle shuffle.
		else if (IsMouseJustPressed() && (_shuffleButtonRectangle.Intersects(new Rectangle(Mouse.GetState().Position, Point.Zero))))
		{
			DeckShuffler.Shuffle(_deck);
		}

		// Exit Game
		if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
			Keyboard.GetState().IsKeyDown(Keys.Escape))
			Exit();

		base.Update(gameTime);
		_previousMouseState = Mouse.GetState();
		_previousKeyboardState = Keyboard.GetState();
	}


	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.ForestGreen);
		// In your Draw method
		_spriteBatch.Begin(
			SpriteSortMode.Deferred,
			BlendState.AlphaBlend,
			SamplerState.PointClamp, // This is the key for pixel-perfect scaling
			null,
			null,
			null,
			null); // Optional scaling matrix
		_spriteBatch.Draw(_background, new Rectangle(0, 0, SCREEN_WIDTH * SCREEN_SCALE, SCREEN_HEIGHT * SCREEN_SCALE), Color.White);
		var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_frameCounter.Update(deltaTime);

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

		//If pocket card is empty, draw black quad with count of pocket uses left in it.
		if (_pocketCard != null)
		{
			_spriteBatch.Draw(_deckSpriteSheet, GetPocketCardRectangle(), _pocketCard.m_SpritesheetBlitRect, Color.White);
		}
		else
		{
			_spriteBatch.Draw(_deckSpriteSheet, GetPocketCardRectangle(), BLACK_RECT, Color.White);
		}

		_spriteBatch.Draw(_shuffleButton, _shuffleButtonRectangle, Color.White);
		_spriteBatch.End();
		base.Draw(gameTime);

		var currentlyHighlightedText = _currentlyHighlightedCard == null ? "" : $"{_currentlyHighlightedCard.m_suit}:{_currentlyHighlightedCard.m_rank}";
		Window.Title = $"Addiction Solitaire - FPS: {((int)_frameCounter.AverageFramesPerSecond)}";
	}
	#endregion
}
