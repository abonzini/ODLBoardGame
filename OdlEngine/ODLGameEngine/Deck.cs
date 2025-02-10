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
        public int DeckSize { get; set; } = 0;
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
                DeckSize++;
            }
        }
        /// <summary>
        /// Initializes deck copying a list from somewhere
        /// </summary>
        /// <param name="cardsList">A lsit with the cards</param>
        public void InitializeDeck(List<int> cardsList)
        {
            Cards.Clear();
            // Now I add string to the deck
            foreach (int card in cardsList)
            {
                if (!CardHistogram.TryGetValue(card, out int value))
                {
                    value = 0;
                    CardHistogram[card] = value;
                }
                CardHistogram[card] = ++value;
                Cards.Add(card);
                DeckSize++;
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
        /// Removes card in specific location of deck (default last)
        /// </summary>
        /// <param name="position">Posiiton to remove (default last)</param>
        /// <returns>The card ID that was just popped</returns>
        public int PopCard(int position = -1)
        {
            int card;
            if(position == -1)
            {
                card = Cards.Last(); // Get card
                Cards.RemoveAt(DeckSize - 1); // Pop it
            }
            else
            {
                card = Cards[position]; // Get card
                Cards.RemoveAt(position); // Pop it
            }
            
            CardHistogram[card]--;
            DeckSize--;
            return card; // Return what was in the last position
        }
        /// <summary>
        /// Adds card back into last place
        /// </summary>
        /// <param name="card">The card to add to top of deck</param>
        public void InsertCard(int card, int position=-1)
        {
            if(position == -1)
            {
                position = DeckSize;
            }
            Cards.Insert(position, card);
            if (!CardHistogram.TryGetValue(card, out int value))
            {
                value = 0;
                CardHistogram[card] = value;
            }
            CardHistogram[card] = ++value;
            DeckSize++;
        }

        /// <summary>
        /// Swaps the cards specified in a deck. Useful dor Fischer-Yates Shuffling
        /// </summary>
        /// <param name="pos1">Position 1</param>
        /// <param name="pos2">Position 2</param>
        public void SwapCards(int pos1, int pos2)
        {
            int aux;
            if (pos1 != pos2)
            {
                aux = Cards[pos1];
                Cards[pos1] = Cards[pos2];
                Cards[pos2] = aux;
            }
        }
        /// <summary>
        /// Looks at next card drawn
        /// </summary>
        /// <returns>The next card in deck (doesn't remove)</returns>
        public int Peep()
        {
            return Cards.Last();
        }
        public override string ToString()
        {
            return GetDeckHistogramString();
        }
    }
}
