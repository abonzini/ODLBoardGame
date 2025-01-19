using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public enum PlayerId
    {
        SPECTATOR, // Meant for a spectator, lowest knowledge
        PLAYER_1,
        PLAYER_2,
        OMNISCIENT // Highest knowledge
    }
    /// <summary>
    /// What state the machine is
    /// </summary>
    public enum States
    {
        /// <summary>
        /// State machine just created, reccomended to setup before continuing
        /// </summary>
        START,
        /// <summary>
        /// Initialization for Player 1
        /// </summary>
        P1_INIT,
        /// <summary>
        /// Initialization for Player 2
        /// </summary>
        P2_INIT,
        /// <summary>
        /// Start of a brand new game, shuffling and such needed
        /// </summary>
        DRAW_PHASE,
        /// <summary>
        /// Action phase, players select their actions
        /// </summary>
        ACTION_PHASE,
        /// <summary>
        /// End of turn activities
        /// </summary>
        EOT,
        /// <summary>
        /// If end of game is triggered
        /// </summary>
        EOG
    }

    /// <summary>
    /// Contains all data about a game state, copyable and small, no methods.
    /// The "moving parts" (actions, full board stste, decks, hands) are stored elsewhere
    /// Can be serialized to clients to render full game state.
    /// Can be sent to AI to make game decisions.
    /// The amount of info given to players and spectators is limited depending on their privileges to avoid cheating
    /// With this, a game state can be completely retrieved and any game can be started from any point (as well as from scratch)
    /// </summary>
    public class GameStateStruct
    {
        public States currentState {  get; set; } = States.START;
        public string stateHash { get; set; } = "";
        public int seed { get; set; } = 0;
        public PlayerId currentPlayer { get; set; } = PlayerId.OMNISCIENT;
        public PlayerState[] playerStates { get; set; } = [new PlayerState(), new PlayerState()];
        public Board boardState { get; set; } = new Board();
    }

    /// <summary>
    /// State of a player, to be contained in game state
    /// </summary>
    public class PlayerState
    {
        public string name { get; set; } = "";
        public PlayerClassType playerClass { get; set; } = PlayerClassType.BASE;
        public int hp { get; set; } = 0;
        public int gold { get; set; } = 0;
        public bool rushActive { get; set; } = true;
        public int nBuildings { get; set; } = 0;
        public int nUnits { get; set; } = 0;
        public Hand hand { get; set; } = new Hand();
        public Deck deck { get; set; } = new Deck();
    }
}