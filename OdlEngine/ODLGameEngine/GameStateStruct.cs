using Newtonsoft.Json;
using System.Text.Json;

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
        START,
        ACTION_PHASE,
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
        public int TurnCounter { get; set; } = 1; // Non hashed as Turn number doesnt change two states being different (transpositions). It is stored however for other effects and MinMax depth. Begins at turn 1
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
        public Player[] PlayerStates { get; set; } = new Player[2];
        [JsonProperty]
        public Board BoardState { get; set; } = new Board();
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
            return hash.ToHashCode();
        }
    }
}