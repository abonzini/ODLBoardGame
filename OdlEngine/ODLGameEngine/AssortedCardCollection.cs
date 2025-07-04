using Newtonsoft.Json;
using System.Text.Json;

namespace ODLGameEngine
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AssortedCardCollection : ICloneable
    {
        [JsonProperty]
        private SortedList<int, int> _cardHistogram = new SortedList<int, int>();
        [JsonProperty]
        private int _size = 0;
        public int CardCount { get { return _size; } }
        /// <summary>
        /// Adds card to hand
        /// </summary>
        /// <param name="card">Which card to add</param>
        /// <param name="howMany">Optional parameter, how many will be added</param>
        public void InsertToCollection(int card, int howMany = 1)
        {
            if (howMany <= 0) return;
            if (_cardHistogram.TryGetValue(card, out int value))
            {
                _cardHistogram[card] += howMany;
            }
            else
            {
                _cardHistogram.Add(card, howMany);
            }
            _size++;
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
        public void RemoveFromCollection(int card)
        {
            _cardHistogram[card]--;
            _size--;
            if (_cardHistogram[card] == 0)
            {
                _cardHistogram.Remove(card);
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
            AssortedCardCollection newCollection = (AssortedCardCollection)MemberwiseClone();
            newCollection._cardHistogram = new SortedList<int, int>();
            foreach (KeyValuePair<int, int> kvp in _cardHistogram)
            {
                newCollection._cardHistogram[kvp.Key] = kvp.Value;
            }
            return newCollection;
        }
    }
}
