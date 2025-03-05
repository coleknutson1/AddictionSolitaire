using static AddictionSolitaire.Game1;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

public class DeckShuffler
{
	private static Random rng = new Random();

	public static void Shuffle(List<Card> deck)
	{
		int n = deck.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);

			// Swap the cards
			Card tempCard = deck[k];
			deck[k] = deck[n];
			deck[n] = tempCard;

			// Update the grid locations of the swapped cards
			Vector2 tempLocation = deck[k].m_currentGridLocation;
			deck[k].UpdateLocation(deck[n].m_currentGridLocation);
			deck[n].UpdateLocation(tempLocation);
		}
	}
}