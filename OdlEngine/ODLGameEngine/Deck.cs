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
        public List<int> Cards { get; set; } = new List<int>(30);
        public Dictionary<int, int> CardHistogram { get; set; } = new Dictionary<int, int>();
        /// <summary>
        /// Initializes deck given csv string of cards sequence
        /// </summary>
        /// <param name="deckString">A csv string with each int id of the cards</param>
        public void InitializeDeck(string deckString)
        {
            Cards.Clear();

            // Now I add string to the deck
            string[] cardStrings = deckString.Split(',');
            foreach(string card in cardStrings)
            {
                int cardId = int.Parse(card);
                if (!CardHistogram.TryGetValue(cardId, out int value))
                {
                    value = 0;
                    CardHistogram[cardId] = value;
                }
                CardHistogram[cardId] = ++value;
                Cards.Add(cardId);
            }
        }
        /// <summary>
        /// Request deck string for current deck
        /// </summary>
        /// <returns> Deck JSON string with count </returns>
        public string GetDeckHistogramString()
        {
            string retString;
            var options = new JsonSerializerOptions // Serializing options for nice format...
            {
                WriteIndented = true
            };
            retString = JsonSerializer.Serialize(CardHistogram, options);
            return retString;
        }
        /// <summary>
        /// Gets the remaining number of cards in deck
        /// </summary>
        /// <returns>Number of cards in deck</returns>
        public int GetCardNumber()
        {
            return Cards.Count;
        }
        /// <summary>
        /// Gets last card of deck
        /// </summary>
        /// <returns>The card ID that was just popped</returns>
        public int PopCard(int position = -1)
        {
            int card;
            if(position == -1)
            {
                card = Cards.Last(); // Get card
                Cards.RemoveAt(Cards.Count - 1); // Pop it
            }
            else
            {
                card = Cards[position]; // Get card
                Cards.RemoveAt(position); // Pop it
            }
            
            CardHistogram[card]--;
            return card; // Return what was in the last position
        }
        /// <summary>
        /// Adds card back into last place
        /// </summary>
        /// <param name="card">The card to add to top of deck</param>
        public void InsertCard(int position, int card)
        {
            Cards.Insert(position, card);
            if (!CardHistogram.TryGetValue(card, out int value))
            {
                value = 0;
                CardHistogram[card] = value;
            }
            CardHistogram[card] = ++value;
        }
        
        public override string ToString()
        {
            return GetDeckHistogramString();
        }
    }
}
