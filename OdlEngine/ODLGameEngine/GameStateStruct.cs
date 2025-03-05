using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Linq;

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
    /// Hashable stuff can give you a hash which attempts to guarantee hash uniqueness and so on
    /// </summary>
    public interface IHashable
    {
        public int GetHash();
    }

    /// <summary>
    /// Contains all data about a game state, copyable and small, no methods.
    /// The "moving parts" (actions, full board stste, decks, hands) are stored elsewhere
    /// Can be serialized to clients to render full game state.
    /// Can be sent to AI to make game decisions.
    /// The amount of info given to players and spectators is limited depending on their privileges to avoid cheating
    /// With this, a game state can be completely retrieved and any game can be started from any point (as well as from scratch)
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class GameStateStruct : IHashable
    {
        [JsonProperty]
        public States CurrentState { get; set; } = States.START;
        [JsonProperty]
        public int StateHash { get { return GetHash(); } }
        [JsonProperty]
        public int Seed { get; set; } = 0;
        [JsonProperty]
        public int NextUnitIndex { get; set; } = 0;
        [JsonProperty]
        public CurrentPlayer CurrentPlayer { get; set; } = CurrentPlayer.OMNISCIENT;
        [JsonProperty]
        public PlayerState[] PlayerStates { get; set; } = [new PlayerState(), new PlayerState()];
        [JsonProperty]
        public Board BoardState { get; set; } = new Board();

        public int GetHash()
        {
            HashCode hash = new HashCode();
            hash.Add(CurrentState);
            hash.Add(Seed);
            hash.Add(NextUnitIndex);
            hash.Add(CurrentPlayer);
            hash.Add(PlayerStates[0].GetHash());
            hash.Add(PlayerStates[1].GetHash());
            hash.Add(BoardState.GetHash());
            return hash.ToHashCode();
        }
    }

    /// <summary>
    /// State of a player, to be contained in game state
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PlayerState : IHashable
    {
        /// <summary>
        /// Name of player
        /// </summary>
        [JsonProperty] 
        public string Name { get; set; } = "";
        /// <summary>
        /// Class (for later class-specific mechanics)
        /// </summary>
        [JsonProperty]
        public PlayerClassType PlayerClass { get; set; } = PlayerClassType.BASE;

        /// <summary>
        /// Current Hp
        /// </summary>
        [JsonProperty]
        public int Hp { get; set; } = 0;
        /// <summary>
        /// Gold
        /// </summary>
        [JsonProperty]
        public int Gold { get; set; } = 0;
        /// <summary>
        /// If player can rush this turn
        /// </summary>
        [JsonProperty]
        public bool RushAvailable { get; set; } = true;
        /// <summary>
        /// Number of buildings the player has
        /// </summary>
        [JsonProperty]
        public int NBuildings { get; set; } = 0;
        /// <summary>
        /// Number of units the player has
        /// </summary>
        [JsonProperty]
        public int NUnits { get; set; } = 0;
        /// <summary>
        /// Player Hand
        /// </summary>
        [JsonProperty]
        public AssortedCardCollection Hand { get; set; } = new AssortedCardCollection();
        /// <summary>
        /// Player Deck
        /// </summary>
        [JsonProperty]
        public Deck Deck { get; set; } = new Deck();
        /// <summary>
        /// Player Discard Pile
        /// </summary>
        [JsonProperty]
        public AssortedCardCollection DiscardPile { get; set; } = new AssortedCardCollection();

        public int GetHash()
        {
            HashCode hash = new HashCode();
            hash.Add(Name);
            hash.Add(PlayerClass);
            hash.Add(Hp);
            hash.Add(Gold);
            hash.Add(RushAvailable);
            hash.Add(Hand.GetHash());
            hash.Add(Deck.GetHash());
            hash.Add(DiscardPile.GetHash());
            return hash.ToHashCode();
        }
    }
}