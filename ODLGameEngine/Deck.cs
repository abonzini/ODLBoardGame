using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class Deck
    {
        public const int MAX_CARDS_IN_DECK = 60;
        int[] cardsInDeck = new int[MAX_CARDS_IN_DECK];
        Dictionary<int, int> cardHistogram = new Dictionary<int, int>();
        int cardCount = 0;
        /// <summary>
        /// Initializes deck given csv string of cards sequence
        /// </summary>
        /// <param name="deckString">A csv string with each int id of the cards</param>
        public void InitializeDeck(string deckString)
        {
            Array.Clear(cardsInDeck);
            cardCount = 0;

            // Now I add string to the deck
            string[] cardStrings = deckString.Split(',');
            int cardsToAdd = (cardStrings.Count() > MAX_CARDS_IN_DECK) ? MAX_CARDS_IN_DECK : cardStrings.Count();
            for(int i = 0; i < cardsToAdd; i++)
            {
                int cardId = int.Parse(cardStrings[i]);
                if (!cardHistogram.ContainsKey(cardId))
                {
                    cardHistogram[cardId] = 0;
                }
                cardHistogram[cardId]++;
                cardsInDeck[i] = int.Parse(cardStrings[i]);
                cardCount++;
            }
        }
        /// <summary>
        /// Request deck string for current deck
        /// </summary>
        /// <returns> Deck JSON string with count </returns>
        public string GetDeckString()
        {
            string retString = string.Empty;
            var options = new JsonSerializerOptions // Serializing options for nice format...
            {
                WriteIndented = true
            };
            retString = JsonSerializer.Serialize(cardHistogram, options);
            return retString;
        }
        /// <summary>
        /// Gets the remaining number of cards in deck
        /// </summary>
        /// <returns>Number of cards in deck</returns>
        public int GetCardNumber()
        {
            return cardCount;
        }
        /// <summary>
        /// Gets last card of deck
        /// </summary>
        /// <returns>The card ID that was jsut popped</returns>
        public int PopCard()
        {
            if(cardCount > 0)
            {
                int card = cardsInDeck[cardCount-1];
                cardCount--; // One less card
                cardHistogram[card]--;
                return card; // Return what was in the last position
            }
            else
            {
                throw new Exception("No cards in deck!");
            }
        }
        /// <summary>
        /// Adds card back into last place
        /// </summary>
        /// <param name="card">The card to add to top of deck</param>
        public void InsertCard(int card)
        {
            if(cardCount < MAX_CARDS_IN_DECK)
            {
                cardsInDeck[cardCount] = card;
                if (!cardHistogram.ContainsKey(card))
                {
                    cardHistogram[card] = 0;
                }
                cardHistogram[card]++;
                cardCount++;
            }
            else
            {
                throw new Exception("Deck full!");
            }    
        }
        /// <summary>
        /// Shuffles deck
        /// </summary>
        /// <param name="randomSeed">Random seed to perform the shuffle</param>
        public void ShuffleDeck(int randomSeed)
        {
            Random rng = new Random(randomSeed);
            rng.Shuffle<int>(cardsInDeck.AsSpan<int>().Slice(0, cardCount));
        }

        public override string ToString()
        {
            return GetDeckString();
        }
    }
}
