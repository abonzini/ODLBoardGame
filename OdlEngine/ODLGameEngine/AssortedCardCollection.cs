using Newtonsoft.Json;
using System.Text.Json;

namespace ODLGameEngine
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AssortedCardCollection : ICloneable
    {
        // Aux comparer to sort in descending order
        static readonly Comparer<int> descendingComparer = Comparer<int>.Create((x, y) => y.CompareTo(x));
        [JsonProperty]
        private readonly SortedList<int, int> _cardHistogram = new SortedList<int, int>();
        [JsonProperty]
        private int _size = 0;
        // Bit of a weird one, this holds the number of counts in every card (in descending order), made it public because I didn't want to deal with it.
        // Contains all cards sorted by count kind of like an inverse histogram. Useful for wildcard discovery calculations
        public SortedDictionary<int, HashSet<int>> CountHistogram = new SortedDictionary<int, HashSet<int>>(descendingComparer);
        // Methods and stuff
        public AssortedCardCollection() { } // Normal constructor
        public AssortedCardCollection(List<int> Deck) // Helper constructor for tests
        {
            foreach (int card in Deck)
            {
                AddToCollection(card);
            }
        }
        public int CardCount { get { return _size; } }
        /// <summary>
        /// Adds card to hand
        /// </summary>
        /// <param name="card">Which card to add</param>
        /// <param name="howMany">Optional parameter, how many will be added</param>
        public void AddToCollection(int card, int howMany = 1)
        {
            int newAmount;
            if (howMany <= 0) return;
            if (_cardHistogram.TryGetValue(card, out int value))
            {
                newAmount = value + howMany;
                _cardHistogram[card] = newAmount;
            }
            else
            {
                newAmount = howMany;
                _cardHistogram.Add(card, howMany);
            }
            ModifyCardCount(card, value, newAmount);
            _size += howMany;
        }
        /// <summary>
        /// Returns how manny cards here
        /// </summary>
        /// <returns>The number of cards</returns>
        public int GetSize()
        {
            return _size;
        }
        /// <summary>
        /// Resets the card histogram
        /// </summary>
        protected void ResetHistogram()
        {
            _cardHistogram.Clear();
        }
        /// <summary>
        /// Removes card from hand
        /// </summary>
        /// <param name="card">Card to remove</param>
        /// <param name="howMany">Optional parameter, how many will be removed</param>
        public void RemoveFromCollection(int card, int howMany = 1)
        {
            if (howMany <= 0) return;
            if (_cardHistogram.TryGetValue(card, out int nCards)) // Check if the card is even present in this collection
            {
                int nRemovedCards = Math.Min(nCards, howMany); // Remove only as many as I could
                int newAmount = nCards - nRemovedCards;
                _cardHistogram[card] = nCards - nRemovedCards;
                _size -= nRemovedCards;
                if (newAmount == 0)
                {
                    _cardHistogram.Remove(card);
                }
                ModifyCardCount(card, nCards, newAmount);
            }
        }
        /// <summary>
        /// Checks if hand contains a specific card
        /// </summary>
        /// <param name="card"></param>
        /// <returns>If contained</returns>
        public bool HasCardInCollection(int card)
        {
            return _cardHistogram.ContainsKey(card);
        }
        /// <summary>
        /// Returns how many copies of a card here
        /// </summary>
        /// <param name="card"></param>
        /// <returns>Amount</returns>
        public int CheckAmountInCollection(int card)
        {
            if (!_cardHistogram.TryGetValue(card, out int amount))
            {
                amount = 0;
            }
            return amount;
        }
        /// <summary>
        /// Yield returns elements in the histogram one by one, i.e. exposes the histogram but only for checking
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<int, int>> GetCards()
        {
            foreach (KeyValuePair<int, int> nextCard in _cardHistogram)
            {
                yield return nextCard;
            }
        }
        /// <summary>
        /// VERY DANGEROUS! This alters the deck size without actually changing the real contents of the deck
        /// Only to be used when no cards whatsoever will be drawn from deck
        /// Otherwise there may be a big disaster
        /// </summary>
        /// <param name="amount">How much to change the deck size by (i.e. +- 1 to add/remove cards)</param>
        public void HYPOTHETICAL_ChangeCount(int amount)
        {
            _size += amount;
        }
        /// <summary>
        /// For the CountHistogram, given a card, a rudimentary "remove from old, add to new"
        /// </summary>
        /// <param name="card">The card whose count changed</param>
        /// <param name="oldCount">Old count</param>
        /// <param name="newCount">New count</param>
        private void ModifyCardCount(int card, int oldCount, int newCount)
        {
            if (oldCount > 0) // For the old number
            {
                HashSet<int> cards = CountHistogram[oldCount];
                cards.Remove(card); // Remove card from here
                if (cards.Count == 0) // Remove count if it was the last card with this quantity
                {
                    CountHistogram.Remove(oldCount);
                }
            }
            if (newCount > 0)
            {
                if (!CountHistogram.TryGetValue(newCount, out HashSet<int> value))
                {
                    CountHistogram[newCount] = new HashSet<int>([card]);
                }
                else
                {
                    value.Add(card);
                }
            }
        }
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            foreach (KeyValuePair<int, int> kvp in _cardHistogram)
            {
                hash.Add(kvp.Key);
                hash.Add(kvp.Value);
            }
            return hash.ToHashCode();
        }
        public override string ToString()
        {
            string retString;
            var options = new JsonSerializerOptions // Serializing options for nice format...
            {
                WriteIndented = true
            };
            retString = System.Text.Json.JsonSerializer.Serialize(_cardHistogram, options);
            return retString;
        }
        public virtual object Clone()
        {
            AssortedCardCollection newCollection = new AssortedCardCollection();
            foreach (KeyValuePair<int, int> kvp in _cardHistogram)
            {
                newCollection.AddToCollection(kvp.Key, kvp.Value);
            }
            return newCollection;
        }
    }
}
