using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    // TODO: Action/event class (step?)
    public class GameStateMachine
    {
        States currentState = States.START;
        GameStateClass detailedState = new GameStateClass(); // State info, will work over this to advance game
        CardFinder cardDb = null; // Card db to be used
        Player[] players = [null, null]; // Both players, this should be never null for a new game

        /// <summary>
        /// Sets card db for this game simulator
        /// </summary>
        /// <param name="cardDb">The card db used, either a new one, fully load, or even weird dummy card db</param>
        public void SetCardDb(CardFinder cardDb)
        {
            this.cardDb = cardDb;
        }
    }
}
