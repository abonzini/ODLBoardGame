using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    /// <summary>
    /// Defines a player who plays the game
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Name of player
        /// </summary>
        public string name { get; set; } = "";
        /// <summary>
        /// Class the player is using
        /// </summary>
        public PlayerClassType playerClass { get; set; } = PlayerClassType.BASE;
        /// <summary>
        /// The decks the player is starting with
        /// </summary>
        public List<int> initialDecklist { get; set; } = new List<int>();
    }
}
