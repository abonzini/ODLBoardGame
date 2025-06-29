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
        public void InsertToCollection(int card)
        {
            if (_cardHistogram.TryGetValue(card, out int value))
            {
                _cardHistogram[card] = ++value;
            }
            else
            {
                _cardHistogram.Add(card, 1);
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
