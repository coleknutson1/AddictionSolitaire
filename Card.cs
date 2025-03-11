using AddictionSolitaire;
using Microsoft.Xna.Framework;

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
		const int padding = 5; // Adjust the padding value as needed
		const int offsetX = 170; // Adjust the offset value as needed
		const int offsetY = 100; // Adjust the offset value as needed
		m_PositionRect = new Rectangle(
			(Game1.CardWidth + padding) * (int)m_currentGridLocation.X + offsetX,
			(Game1.CardHeight + padding) * (int)m_currentGridLocation.Y + offsetY,
			Game1.CardWidth,
			Game1.CardHeight
		);
	}

	public Rectangle m_PositionRect { get; set; }
	public bool m_isEmpty { get; set; }
	public Rectangle m_SpritesheetBlitRect { get; set; }
	public string m_suit { get; set; }
	public int m_rank { get; set; }
	public Vector2 m_currentGridLocation { get; set; }

	public void UpdateRankAndSuit(int newRank, string newSuit, Rectangle newSpritesheetBlit, Vector2 currentGridLocation, bool temp_isEmpty)
	{
		m_rank = newRank;
		m_suit = newSuit;
		m_SpritesheetBlitRect = newSpritesheetBlit;
		m_currentGridLocation = currentGridLocation;
		m_isEmpty = temp_isEmpty;
	}

	public override string ToString()
	{
		// Return whatever string representation you want
		return $"{m_rank} of {m_suit}";
	}
}
