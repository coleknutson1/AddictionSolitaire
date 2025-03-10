using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

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
	private Card _currentlyHighlightedCard;

	//Initialize FPS Class
	private FrameCounter _frameCounter = new FrameCounter();

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

	private void PlaceCorrectCardAtSelectedEmptySpot()
	{
		// Return if in first slot of row
		if (_deck.IndexOf(_currentlySelectedCard) % 14 == 0) { return; }

		// Card before (cb) current empty spot
		var _cb_index = _deck.IndexOf(_currentlySelectedCard) - 1;
		if (_cb_index < 0)
			return; // Just do nothing as they were need to pick a two (IMPROVEMENT: unless they are down to the last two?)

		// Get card before (cb) and swap card.
		var _cb = _deck[_cb_index];

		// Swap Card (sc)
		var _sc = _deck.FirstOrDefault(x => x.m_suit == _cb.m_suit && x.m_rank == _cb.m_rank + 1);

		if (_sc == null) //If empty precedes the current selected empty
			return;

		// Get the index of currently selected and swap card.
		var _cs_index = _deck.IndexOf(_currentlySelectedCard);
		var _sc_index = _deck.IndexOf(_sc);

		// The two cards need to exchange rank/suit values but retain their index as that is how we sort.
		var _temp_rank = _currentlySelectedCard.m_rank;
		var _temp_suit = _currentlySelectedCard.m_suit;
		var _temp_spritesheetBlitRect = _currentlySelectedCard.m_SpritesheetBlitRect;
		var _temp_gridLocation = _currentlySelectedCard.m_currentGridLocation;
		var _temp_isEmpty = _currentlySelectedCard.m_isEmpty;

		_deck[_cs_index].UpdateRankAndSuit(_sc.m_rank, _sc.m_suit, _sc.m_SpritesheetBlitRect, _sc.m_currentGridLocation, _sc.m_isEmpty);
		_deck[_sc_index].UpdateRankAndSuit(_temp_rank, _temp_suit, _temp_spritesheetBlitRect, _temp_gridLocation, _temp_isEmpty);
	}

	private bool IsMouseJustPressed() =>
		Mouse.GetState().LeftButton == ButtonState.Pressed &&
		_previousMouseState.LeftButton == ButtonState.Released;

	protected override void Update(GameTime gameTime)
	{
		//We need to brute force check on which card is hovered over for highlighting the matching card spot
		//Needs optimization. n=52
		_currentlySelectedCard = null;
		var mousePos = Mouse.GetState().Position;
		foreach (var card in _deck)
		{
			if (card.m_PositionRect.Intersects(new Rectangle(mousePos, Point.Zero)))
			{
				_currentlySelectedCard = card;
				_currentOffset = new Vector2(
					_currentlySelectedCard.m_PositionRect.Left,
					_currentlySelectedCard.m_PositionRect.Top
				) - mousePos.ToVector2();

				//Get the index of the card that is 1 lower and of the same suit than our currently selected. Plus one as that is a valid spot for it.
				var _index_of_valid_card_move = _deck.IndexOf(
					_deck.FirstOrDefault(x => x.m_suit == _currentlySelectedCard.m_suit
					&& x.m_rank == _currentlySelectedCard.m_rank - 1)) + 1;

				//If it would be an out of bounds, just break prematurely
				if (_index_of_valid_card_move >= _deck.Count)
					break;

				//Immediately find the slot that swap card that corresponds to currently selected.
				var _cardSlotForValidCardMove = _deck[_index_of_valid_card_move];

				//If the valid card move is empty, then we know it's definitely a valid move to be made.
				_currentlyHighlightedCard = _cardSlotForValidCardMove.m_isEmpty ? _cardSlotForValidCardMove : null;
				break;
			}
		}

		//Handle the mouse interactions
		if (IsMouseJustPressed())
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

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.ForestGreen);

		_spriteBatch.Begin();
		var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

		_frameCounter.Update(deltaTime);

		var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);
		foreach (var card in _deck)
		{
			if (card == _currentlyHighlightedCard)
			{
				_spriteBatch.Draw(_deckSpriteSheet, card.m_PositionRect, card.m_SpritesheetBlitRect, new Color(new Vector4(221, 245, 66, .5f)));
			}
			else
			{
				_spriteBatch.Draw(_deckSpriteSheet, card.m_PositionRect, card.m_SpritesheetBlitRect, Color.White);
			}
			//_spriteBatch.DrawString(
			//	_font,
			//	_deck.IndexOf(card).ToString(),
			//	new Vector2(card.m_PositionRect.X + 10, card.m_PositionRect.Y + 64),
			//	Color.Blue
			//);
		}

		_spriteBatch.End();
		base.Draw(gameTime);

		var currentlyHighlightedText = _currentlyHighlightedCard == null ? "" : $"{_currentlyHighlightedCard.m_suit}:{_currentlyHighlightedCard.m_rank}";
		Window.Title = $"Card Game -  - {fps}";
	}



}