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
    /// Contains all data about a game state, copyable and small, no methods.
    /// The "moving parts" (actions, full board stste, decks, hands) are stored elsewhere
    /// Can be serialized to clients to render full game state.
    /// Can be sent to AI to make game decisions.
    /// The amount of info given to players and spectators is limited depending on their privileges to avoid cheating
    /// With this, a game state can be completely retrieved and any game can be started from any point (as well as from scratch)
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class GameStateStruct
    {
        [JsonProperty]
        public States CurrentState { get; set; } = States.START;
        [JsonProperty]
        public int StateHash { get { return GetHashCode(); } }
        [JsonProperty]
        public int Seed { get; set; } = 0;
        [JsonProperty]
        public int NextUniqueIndex { get; set; } = 2; // 2 because 0 and 1 are reserved for players
        [JsonProperty]
        public CurrentPlayer CurrentPlayer { get; set; } = CurrentPlayer.OMNISCIENT;
        [JsonProperty]
        public PlayerState[] PlayerStates { get; set; } = [new PlayerState(), new PlayerState()];
        [JsonProperty]
        public Board BoardState { get; set; } = new Board();
        [JsonProperty]
        public Dictionary<TriggerType, SortedSet<int>> Triggers { get; set; } = new Dictionary<TriggerType, SortedSet<int>>();
        // Entities
        [JsonProperty]
        public readonly SortedList<int, LivingEntity> EntityData = new SortedList<int, LivingEntity>();
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(CurrentState);
            hash.Add(Seed);
            hash.Add(NextUniqueIndex);
            hash.Add(CurrentPlayer);
            hash.Add(BoardState.GetHashCode());
            foreach (KeyValuePair<int, LivingEntity> kvp in EntityData)
            {
                hash.Add(kvp.Key);
                hash.Add(kvp.Value.GetHashCode());
            }
            foreach (KeyValuePair< TriggerType, SortedSet<int>> trigger in Triggers)
            {
                hash.Add(trigger.Key);
                foreach(int entity in  trigger.Value)
                {
                    hash.Add(entity);
                }
            }
            return hash.ToHashCode();
        }
    }
}