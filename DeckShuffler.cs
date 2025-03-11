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
		for (int i = 0; i < n - 1; i++)
		{
			int k = rng.Next(i, n);

			// Swap the cards
			Card tempCard = deck[k];
			deck[k] = deck[i];
			deck[i] = tempCard;

			// Update the grid locations of the swapped cards
			Vector2 tempLocation = deck[k].m_currentGridLocation;
			deck[k].UpdateLocation(deck[i].m_currentGridLocation);
			deck[i].UpdateLocation(tempLocation);
		}
	}
}
