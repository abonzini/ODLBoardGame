using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public enum CurrentPlayer
    {
        PLAYER_1,
        PLAYER_2,
        SPECTATOR, // Meant for a spectator, lowest knowledge
        OMNISCIENT, // Highest knowledge
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
        public States CurrentState {  get; set; } = States.START;
        public string StateHash { get; set; } = "";
        public int Seed { get; set; } = 0;
        public int PlaceableTotalCount { get; set; } = 0;
        public CurrentPlayer CurrentPlayer { get; set; } = CurrentPlayer.OMNISCIENT;
        public PlayerState[] PlayerStates { get; set; } = [new PlayerState(), new PlayerState()];
        public Board BoardState { get; set; } = new Board();
    }

    /// <summary>
    /// State of a player, to be contained in game state
    /// </summary>
    public class PlayerState
    {
        public string Name { get; set; } = "";
        public PlayerClassType PlayerClass { get; set; } = PlayerClassType.BASE;
        public int Hp { get; set; } = 0;
        public int Gold { get; set; } = 0;
        public bool RushActive { get; set; } = true;
        public int NBuildings { get; set; } = 0;
        public int NUnits { get; set; } = 0;
        public Hand Hand { get; set; } = new Hand();
        public Deck Deck { get; set; } = new Deck();
        public List<int> DiscardPile { get; set; } = new List<int>();
    }
}