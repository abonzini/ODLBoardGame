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
    public class AssortedCardCollection
    {
        private int _hash;
        private bool _dirtyHash = true;
        [JsonProperty]
        private readonly SortedList<int,int> _cards = new SortedList<int, int>();
        [JsonProperty] 
        private int _size = 0;
        public int CardCount { get { return _size; } }
        /// <summary>
        /// Adds card to hand
        /// </summary>
        /// <param name="card">Which card to add</param>
        public void InsertCard(int card)
        {
            if (_cards.TryGetValue(card, out int value))
            {
                _cards[card] = ++value;
            }
            else
            {
                _cards.Add(card, 1);
            }
            _size++;
            _dirtyHash = true;
        }
        /// <summary>
        /// Removes card from hand
        /// </summary>
        /// <param name="card">Card to remove</param>
        public void RemoveCard(int card)
        {
            _cards[card]--;
            _size--;
            if (_cards[card] == 0)
            {
                _cards.Remove(card);
            }
            _dirtyHash = true;
        }
        /// <summary>
        /// Checks if hand contains a specific card
        /// </summary>
        /// <param name="card"></param>
        /// <returns>If contained</returns>
        public bool HasCard(int card)
        {
            return _cards.ContainsKey(card);
        }
        /// <summary>
        /// Returns how many copies of a card here
        /// </summary>
        /// <param name="card"></param>
        /// <returns>Amount</returns>
        public int CheckAmount(int card)
        {
            return _cards[card];
        }
        public override int GetHashCode()
        {
            if (_dirtyHash) // Recalculates only when dirty
            {
                HashCode hash = new HashCode();
                foreach(KeyValuePair<int, int> kvp in _cards)
                {
                    hash.Add(kvp.Key);
                    hash.Add(kvp.Value);
                } // HandSize not needed as it is just the sum of kpv.value anyway
                _hash = hash.ToHashCode();
                _dirtyHash = false; // Currently updated hash
            }
            return _hash;
        }
        public bool IsHashDirty()
        { return _dirtyHash; }
        public override string ToString()
        {
            string retString;
            var options = new JsonSerializerOptions // Serializing options for nice format...
            {
                WriteIndented = true
            };
            retString = System.Text.Json.JsonSerializer.Serialize(_cards, options);
            return retString;
        }
    }
}
