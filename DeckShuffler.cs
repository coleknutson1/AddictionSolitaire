using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class DeckShuffler
{
	private static Random rng = new Random();
	public static bool firstRun = true;
	public static void Shuffle(List<Card> deck)
	{
		int n = deck.Count;
		HashSet<int> exemptIndices = new HashSet<int>();
		exemptIndices.Clear();
		if (!firstRun)
		{
			for (int i = 0; i < n - 1; i++)
			{
				Console.WriteLine("Do the first corrected shuffle");
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

		//CHATGPT, can you please just write me a shuffler for the list "List<Card> deck" and make sure it follow off the logic below
		//CHATGPT, can you please just write me a shuffler for the list "List<Card> deck" and make sure it follow off the logic below
		List<int> nonExemptCards = new List<int>();
		for (int i = 0; i < deck.Count; i++)
		{
			if (!exemptIndices.Contains(i))
			{
				nonExemptCards.Add(i);
			}
		}

		// Fisher-Yates shuffle for non-exempt cards
		for (int i = nonExemptCards.Count - 1; i > 0; i--)
		{
			int j = rng.Next(i + 1);
			// Swap the values within the Card objects instead of the indices
			Card tempCard = new Card(deck[nonExemptCards[i]].m_SpritesheetBlitRect, deck[nonExemptCards[i]].m_suit, deck[nonExemptCards[i]].m_rank, deck[nonExemptCards[i]].m_isEmpty, deck[nonExemptCards[i]].m_currentGridLocation);
			deck[nonExemptCards[i]].UpdateRankAndSuit(deck[nonExemptCards[j]].m_rank, deck[nonExemptCards[j]].m_suit, deck[nonExemptCards[j]].m_SpritesheetBlitRect, deck[nonExemptCards[j]].m_currentGridLocation, deck[nonExemptCards[j]].m_isEmpty);
			deck[nonExemptCards[j]].UpdateRankAndSuit(tempCard.m_rank, tempCard.m_suit, tempCard.m_SpritesheetBlitRect, tempCard.m_currentGridLocation, tempCard.m_isEmpty);
		}
	}
}
