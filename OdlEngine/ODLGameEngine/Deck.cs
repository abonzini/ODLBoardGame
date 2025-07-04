using Newtonsoft.Json;
using System.Text.Json;

namespace ODLGameEngine
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Deck : AssortedCardCollection
    {
        [JsonProperty]
        private List<int> _orderedCards = new List<int>(30);
        public int DeckSize { get { return GetSize(); } }
        /// <summary>
        /// Initializes deck given csv string of cards sequence
        /// </summary>
        /// <param name="deckString">A csv string with each int id of the cards</param>
        public void InitializeDeck(string deckString)
        {
            _orderedCards.Clear();
            ResetHistogram();

            // Now I add string to the deck
            string[] cardStrings = deckString.Split(',');
            foreach (string card in cardStrings)
            {
                int cardId = int.Parse(card);
                _orderedCards.Add(cardId);
                InsertToCollection(cardId);
            }
        }
        /// <summary>
        /// Initializes deck copying a list from somewhere
        /// </summary>
        /// <param name="cardsList">A lsit with the cards</param>
        public void InitializeDeck(List<int> cardsList)
        {
            _orderedCards.Clear();
            ResetHistogram();
            // Now I add string to the deck
            foreach (int card in cardsList)
            {
                _orderedCards.Add(card);
                InsertToCollection(card);
            }
        }
        /// <summary>
        /// Copies contents in deck from an iteration of a card collection
        /// </summary>
        /// <param name="cardCollection">The assorted collection to copy</param>
        public void InitializeDeck(AssortedCardCollection cardCollection)
        {
            _orderedCards.Clear();
            ResetHistogram();
            // Now I add string to the deck
            foreach (KeyValuePair<int, int> cardCount in cardCollection.GetCards())
            {
                for (int i = 0; i < cardCount.Value; i++) // Add N cards
                {
                    _orderedCards.Add(cardCount.Key);
                    InsertToCollection(cardCount.Key);
                }
            }
        }
        /// <summary>
        /// Removes card in specific location of deck (default last)
        /// </summary>
        /// <param name="position">Posiiton to remove (default last)</param>
        /// <returns>The card ID that was just popped</returns>
        public int PopCard(int position = -1)
        {
            int card;
            if (position == -1)
            {
                card = _orderedCards.Last(); // Get card
                _orderedCards.RemoveAt(DeckSize - 1); // Pop it
            }
            else
            {
                card = _orderedCards[position]; // Get card
                _orderedCards.RemoveAt(position); // Pop it
            }

            RemoveFromCollection(card); // Removes from histogram
            return card; // Return what was in the last position
        }
        /// <summary>
        /// Adds card back into last place
        /// </summary>
        /// <param name="card">The card to add to top of deck</param>
        public void InsertCard(int card, int position = -1)
        {
            if (position == -1)
            {
                position = DeckSize;
            }
            _orderedCards.Insert(position, card);
            InsertToCollection(card);
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
                aux = _orderedCards[pos1];
                _orderedCards[pos1] = _orderedCards[pos2];
                _orderedCards[pos2] = aux;
            }
        }
        /// <summary>
        /// Looks at next card drawn
        /// </summary>
        /// /// <param name="i">Position to peep, last by default</param>
        /// <returns>The next card in deck (doesn't remove)</returns>
        public int PeepAt(int i = -1)
        {
            if (i == -1)
            {
                return _orderedCards.Last();
            }
            else
            {
                return _orderedCards[i];
            }
        }
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GetSize()); // Note: Although size is included in the next part, we need to serialize deck size for hypothetical scenarios where deck shrinks but not actually drawing cards (adds wildcards to hand for the minmax)
            foreach (int card in _orderedCards)
            {
                hash.Add(card);
            }
            return hash.ToHashCode();
        }
        public override string ToString()
        {
            return base.ToString();
        }
        public override object Clone()
        {
            Deck newDeck = (Deck)base.Clone();
            newDeck._orderedCards = new List<int>();
            foreach (int card in _orderedCards)
            {
                newDeck._orderedCards.Add(card);
            }
            return newDeck;
        }
    }
}
