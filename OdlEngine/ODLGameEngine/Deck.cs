using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ODLGameEngine
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Deck : ICloneable
    {
        private bool _dirtyHash = true;
        private int _hash;
        [JsonProperty]
        private readonly List<int> _cards = new List<int>(30);
        [JsonProperty]
        private readonly Dictionary<int, int> _cardHistogram = new Dictionary<int, int>();
        [JsonProperty]
        private int _deckSize = 0;
        public int DeckSize { get { return _deckSize; } }
        /// <summary>
        /// Initializes deck given csv string of cards sequence
        /// </summary>
        /// <param name="deckString">A csv string with each int id of the cards</param>
        public void InitializeDeck(string deckString)
        {
            _cards.Clear();

            // Now I add string to the deck
            string[] cardStrings = deckString.Split(',');
            foreach(string card in cardStrings)
            {
                int cardId = int.Parse(card);
                if (!_cardHistogram.TryGetValue(cardId, out int value))
                {
                    value = 0;
                    _cardHistogram[cardId] = value;
                }
                _cardHistogram[cardId] = ++value;
                _cards.Add(cardId);
                _deckSize++;
            }
            _dirtyHash = true;
        }
        /// <summary>
        /// Initializes deck copying a list from somewhere
        /// </summary>
        /// <param name="cardsList">A lsit with the cards</param>
        public void InitializeDeck(List<int> cardsList)
        {
            _cards.Clear();
            // Now I add string to the deck
            foreach (int card in cardsList)
            {
                if (!_cardHistogram.TryGetValue(card, out int value))
                {
                    value = 0;
                    _cardHistogram[card] = value;
                }
                _cardHistogram[card] = ++value;
                _cards.Add(card);
                _deckSize++;
            }
            _dirtyHash = true;
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
            retString = System.Text.Json.JsonSerializer.Serialize(_cardHistogram, options);
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
                card = _cards.Last(); // Get card
                _cards.RemoveAt(DeckSize - 1); // Pop it
            }
            else
            {
                card = _cards[position]; // Get card
                _cards.RemoveAt(position); // Pop it
            }
            
            _cardHistogram[card]--;
            _deckSize--;
            _dirtyHash = true;
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
            _cards.Insert(position, card);
            if (!_cardHistogram.TryGetValue(card, out int value))
            {
                value = 0;
                _cardHistogram[card] = value;
            }
            _cardHistogram[card] = ++value;
            _deckSize++;
            _dirtyHash = true;
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
                aux = _cards[pos1];
                _cards[pos1] = _cards[pos2];
                _cards[pos2] = aux;
            }
            _dirtyHash = true;
        }
        /// <summary>
        /// Looks at next card drawn
        /// </summary>
        /// /// <param name="i">Position to peep, last by default</param>
        /// <returns>The next card in deck (doesn't remove)</returns>
        public int PeepAt(int i = -1)
        {
            if(i == -1)
            {
                return _cards.Last();
            }
            else
            {
                return _cards[i];
            }
        }
        /// <summary>
        /// Check how many copies of a specific card in deck
        /// </summary>
        /// <param name="card">Which card</param>
        /// <returns>How many</returns>
        public int CheckAmount(int card)
        {
            if(_cardHistogram.TryGetValue(card,out int value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }
        public override int GetHashCode()
        {
            if (_dirtyHash) // Recalculates only when dirty
            {
                HashCode hash = new HashCode();
                foreach (int card in _cards)
                {
                    hash.Add(card);
                } // Histogram is not needed as it would be just the sum of _cards anyway (correlated!)
                _hash = hash.ToHashCode();
                _dirtyHash = false; // Currently updated hash
            }
            return _hash;
        }
        public bool IsHashDirty()
        { return _dirtyHash; }
        public override string ToString()
        {
            return GetDeckHistogramString();
        }
        public object Clone()
        {
            Deck newDeck = new Deck();
            newDeck.InitializeDeck(_cards);
            return newDeck;
        }
    }
}
