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
        public List<int> deck { get; set; } = new List<int>(30);
        public Dictionary<int, int> cardHistogram { get; set; } = new Dictionary<int, int>();
        /// <summary>
        /// Initializes deck given csv string of cards sequence
        /// </summary>
        /// <param name="deckString">A csv string with each int id of the cards</param>
        public void InitializeDeck(string deckString)
        {
            deck.Clear();

            // Now I add string to the deck
            string[] cardStrings = deckString.Split(',');
            foreach(string card in cardStrings)
            {
                int cardId = int.Parse(card);
                if (!cardHistogram.ContainsKey(cardId))
                {
                    cardHistogram[cardId] = 0;
                }
                cardHistogram[cardId]++;
                deck.Add(cardId);
            }
        }
        /// <summary>
        /// Request deck string for current deck
        /// </summary>
        /// <returns> Deck JSON string with count </returns>
        public string GetDeckHistogramString()
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
            return deck.Count;
        }
        /// <summary>
        /// Gets last card of deck
        /// </summary>
        /// <returns>The card ID that was jsut popped</returns>
        public int PopCard()
        {
            if(deck.Count > 0)
            {
                int card = deck.Last(); // Get card
                deck.RemoveAt(deck.Count - 1); // Pop it
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
        public void InsertCard(int position, int card)
        {
            deck.Insert(position, card);
            if (!cardHistogram.ContainsKey(card))
            {
                cardHistogram[card] = 0;
            }
            cardHistogram[card]++;
        }
        
        public override string ToString()
        {
            return GetDeckHistogramString();
        }
    }
}
