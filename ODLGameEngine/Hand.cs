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
        public List<int> cardsInHand { get; set; } = new List<int>();

        /// <summary>
        /// Get hand size of player
        /// </summary>
        /// <returns>Number of cards in their hand</returns>
        public int GetHandSize()
        {
            return cardsInHand.Count;
        }
        /// <summary>
        /// Inserts a card to the hand
        /// </summary>
        /// <param name="card">Card to insert</param>
        /// <param name="i">Position to insert at</param>
        public void InsertCard(int card, int i)
        {
            if(i >= 0 && i<=cardsInHand.Count)
            {
                cardsInHand.Insert(i, card);
            }
            else
            {
                throw new Exception("Card inserted has to be contiguous to other cards!");
            }
        }
        /// <summary>
        /// Removes the card at specific position (pops). Used when playing a card, and when undoing a draw
        /// </summary>
        /// <param name="i">Position</param>
        /// <returns>The card that was removed</returns>
        public int RemoveCardAt(int i)
        {
            if(i>=0 && i < cardsInHand.Count)
            {
                int card = cardsInHand[i];
                cardsInHand.RemoveAt(i);
                return card;
            }
            else
            {
                throw new Exception("Card removed from hand not in range!");
            }
        }

        public override string ToString()
        {
            string retString = string.Empty;
            var options = new JsonSerializerOptions // Serializing options for nice format...
            {
                WriteIndented = true
            };
            retString = JsonSerializer.Serialize(cardsInHand, options);
            return retString;
        }
    }
}
