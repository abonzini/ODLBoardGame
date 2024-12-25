using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class GameStateManager
    {
        // TODO FIRST:
        // State machine of game state manager, actions in order and keep flow, accept commands only in specific situations
        // TODO
        // Contorls whole thing, contains data (hp, gold, hand#, contains hands too)
        // Allows cards to operate as needed, giving itself as base state but only allowing readonly and requesting actions back to game state, receive results
        // Game state also contains info og hidden
        // Request game/hand state as needed in json mode or similar for unity client
        /* Finally, will contain AI methods that will be useful such as
            - Get possible actions (play, pass, whatever)
            - Get game state (& an absolute state hash independent of order)
            - Reversion of game state back to prev action
         */

        // TODO technical
        // OrderedList of existing units (coordinates, buff tracker, player owner, etc), similar graveyard for units, will be picked-placed as corresponding when requested by game state
    }
}
