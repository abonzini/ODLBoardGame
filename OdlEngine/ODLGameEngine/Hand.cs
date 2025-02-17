using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class Hand
    {
        public Dictionary<int,int> CardsInHand { get; set; } = new Dictionary<int, int>();
        public int HandSize { get; set; } = 0;
        /// <summary>
        /// Adds card to hand
        /// </summary>
        /// <param name="card">Which card to add</param>
        public void InsertCard(int card)
        {
            if (CardsInHand.TryGetValue(card, out int value))
            {
                CardsInHand[card] = ++value;
            }
            else
            {
                CardsInHand.Add(card, 1);
            }
            HandSize++;
        }
        /// <summary>
        /// Removes card from hand
        /// </summary>
        /// <param name="card">Card to remove</param>
        public void RemoveCard(int card)
        {
            CardsInHand[card]--;
            HandSize--;
            if (CardsInHand[card] == 0)
            {
                CardsInHand.Remove(card);
            }
        }

        public override string ToString()
        {
            string retString;
            var options = new JsonSerializerOptions // Serializing options for nice format...
            {
                WriteIndented = true
            };
            retString = JsonSerializer.Serialize(CardsInHand, options);
            return retString;
        }
    }
}
