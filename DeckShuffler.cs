using static AddictionSolitaire.Game1;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

public class DeckShuffler
{
	private static Random rng = new Random();
	private static bool firstRun = true;
	public static void Shuffle(List<Card> deck)
	{
		int n = deck.Count;
		HashSet<int> exemptIndices = new HashSet<int>();

		if (!firstRun)
		{
			for (int i = 0; i < n - 1; i++)
			{
				//Loop over the deck in sequential order
				for (int index = 0; index < deck.Count; index++)
				{
					//First, see if any of the first column values have ranks of zero
					if (index % 13 == 0)
					{
						var next_card_index = index + 1;
						var current_index = index;
						if (deck[index].m_rank == 0)
						{
							exemptIndices.Add(index);
							//If they do, check the next index and see if it has an m_rank that is one greater than previous and of the same m_suit
							while (true)
							{
								if (deck[current_index].m_rank == deck[next_card_index].m_rank - 1 && deck[current_index].m_suit == deck[next_card_index].m_suit)
								{
									exemptIndices.Add(next_card_index);
									current_index = next_card_index;
									next_card_index = next_card_index + 1;
								}
								else
								{
									break;
								}
							}
						}

					}
				}
			}
		}
		firstRun = false;
		for (int i = 0; i < n - 1; i++)
		{
			if (exemptIndices.Contains(i))
			{
				continue;
			}

			int k;
			do
			{
				k = rng.Next(i, n);
			} while (exemptIndices.Contains(k));

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
