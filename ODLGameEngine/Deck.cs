using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class Deck
    {
        public const int MAX_CARDS_IN_DECK = 60;
        int[] cardsInDeck = new int[MAX_CARDS_IN_DECK];
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
                cardsInDeck[i] = int.Parse(cardStrings[i]);
                cardCount++;
            }
        }
        /// <summary>
        /// Request deck string for current deck
        /// </summary>
        /// <returns>Deck string as it stands </returns>
        public string GetDeckString()
        {
            string retString = string.Empty;
            for(int i = 0; i < cardCount; i++)
            {
                retString += cardsInDeck[i];
                if(i < cardCount-1) // Add csv until last card
                {
                    retString += ",";
                }
            }
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
                cardCount--; // One less card
                return cardsInDeck[cardCount]; // Return what was in the last position
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
        public void RestoreCard(int card)
        {
            if(cardCount < MAX_CARDS_IN_DECK)
            {
                cardsInDeck[cardCount] = card;
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
            rng.Shuffle<int>(cardsInDeck);
        }
        /// <summary>
        /// Adds new card to deck (has to shuffle deck after)
        /// </summary>
        /// <param name="card">Card to add</param>
        /// <param name="randomSeed">Seed to shuffle</param>
        public void InsertCard(int card, int randomSeed)
        {
            RestoreCard(card);
            ShuffleDeck(randomSeed);
        }
    }
}
