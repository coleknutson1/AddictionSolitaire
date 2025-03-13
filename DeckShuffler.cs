using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class DeckShuffler
{
	private static Random rng = new Random();
	public static bool firstRun = true;
	public static void Shuffle(HashSet<Card> deck)
	{
		int n = deck.Count;
		HashSet<int> exemptIndices = new HashSet<int>();
		exemptIndices.Clear();
		if (!firstRun)
		{
			var deckList = deck.ToList();
			for (int i = 0; i < n - 1; i++)
			{
				Console.WriteLine("Do the first corrected shuffle");
				//Loop over the deck in sequential order
				for (int index = 0; index < deckList.Count; index++)
				{
					//First, see if any of the first column values have ranks of zero
					if (index % 13 == 0)
					{
						var next_card_index = index + 1;
						var current_index = index;
						if (deckList[index].m_rank == 0)
						{
							exemptIndices.Add(index);
							//If they do, check the next index and see if it has an m_rank that is one greater than previous and of the same m_suit
							while (true)
							{
								if (deckList[current_index].m_rank == deckList[next_card_index].m_rank - 1 && deckList[current_index].m_suit == deckList[next_card_index].m_suit)
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
		var deckListForShuffle = deck.ToList();
		List<int> nonExemptCards = new List<int>();
		for (int i = 0; i < deckListForShuffle.Count; i++)
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
			Card tempCard = new Card(deckListForShuffle[nonExemptCards[i]].m_SpritesheetBlitRect, deckListForShuffle[nonExemptCards[i]].m_suit, deckListForShuffle[nonExemptCards[i]].m_rank, deckListForShuffle[nonExemptCards[i]].m_isEmpty, deckListForShuffle[nonExemptCards[i]].m_currentGridLocation);
			deckListForShuffle[nonExemptCards[i]].UpdateRankAndSuit(deckListForShuffle[nonExemptCards[j]].m_rank, deckListForShuffle[nonExemptCards[j]].m_suit, deckListForShuffle[nonExemptCards[j]].m_SpritesheetBlitRect, deckListForShuffle[nonExemptCards[j]].m_currentGridLocation, deckListForShuffle[nonExemptCards[j]].m_isEmpty);
			deckListForShuffle[nonExemptCards[j]].UpdateRankAndSuit(tempCard.m_rank, tempCard.m_suit, tempCard.m_SpritesheetBlitRect, tempCard.m_currentGridLocation, tempCard.m_isEmpty);
		}

		// Update the original HashSet with the shuffled list
		deck.Clear();
		foreach (var card in deckListForShuffle)
		{
			deck.Add(card);
		}
	}
}
