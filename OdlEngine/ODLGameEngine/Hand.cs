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
        public List<int> CardsInHand { get; set; } = new List<int>();
        public int HandSize { get; set; } = 0;
        /// <summary>
        /// Inserts a card to the hand
        /// </summary>
        /// <param name="card">Card to insert</param>
        /// <param name="i">Position to insert at, default is to beginning of</param>
        public void InsertCard(int card, int i = -1)
        {
            if(i == -1)
            {
                i = HandSize;
            }
            CardsInHand.Insert(i, card);
            HandSize++;
        }
        /// <summary>
        /// Removes the card at specific position (pops). Used when playing a card, and when undoing a draw
        /// </summary>
        /// <param name="i">Position, default removes the last drawn</param>
        /// <returns>The card that was removed</returns>
        public int RemoveCardAt(int i=-1)
        {
            if (i == -1)
            {
                i = HandSize - 1;
            }
            int card = CardsInHand[i];
            CardsInHand.RemoveAt(i);
            HandSize--;
            return card;
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
