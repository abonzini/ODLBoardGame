using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class DiscardPile
    {
        public Dictionary<int, int> CardsInPile { get; set; } = new Dictionary<int, int>();
        public int PileSize { get; set; } = 0;
        /// <summary>
        /// Adds card to pile
        /// </summary>
        /// <param name="card">Which card to add</param>
        public void InsertCard(int card)
        {
            if (CardsInPile.TryGetValue(card, out int value))
            {
                CardsInPile[card] = ++value;
            }
            else
            {
                CardsInPile.Add(card, 1);
            }
            PileSize++;
        }
        /// <summary>
        /// Removes card from pile
        /// </summary>
        /// <param name="card">Card to remove</param>
        public void RemoveCard(int card)
        {
            CardsInPile[card]--;
            PileSize--;
            if (CardsInPile[card] == 0)
            {
                CardsInPile.Remove(card);
            }
        }

        public override string ToString()
        {
            string retString;
            var options = new JsonSerializerOptions // Serializing options for nice format...
            {
                WriteIndented = true
            };
            retString = JsonSerializer.Serialize(CardsInPile, options);
            return retString;
        }
    }
}
