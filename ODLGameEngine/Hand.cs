using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class Hand
    {
        List<Card> cardsInHand = new List<Card>();
        /// <summary>
        /// Returns a hand string listing cards in hand
        /// </summary>
        /// <returns>Csv list of cards in hand</returns>
        public string GetHandString()
        {
            string retString = string.Empty;
            for (int i = 0; i < cardsInHand.Count; i++)
            {
                retString += cardsInHand[i].id;
                if (i < cardsInHand.Count - 1) // Add csv until last card
                {
                    retString += ",";
                }
            }
            return retString;
        }
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
        public void insertCard(Card card, int i)
        {
            if(i > 0 && i<=cardsInHand.Count)
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
        public Card removeCardAt(int i)
        {
            Card card = cardsInHand[i];
            cardsInHand.RemoveAt(i);
            return card;
        }
    }
}
