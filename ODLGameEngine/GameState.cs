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
    /// Lanes
    /// </summary>
    public enum LaneID
    {
        NO_LANE,
        LANE_PLAINS,
        LANE_FOREST,
        LANE_MOUNTAIN
    }

    /// <summary>
    /// Contains all data about a game state, copyable and small, no methods.
    /// The "moving parts" (actions, full board stste, decks, hands) are stored elsewhere
    /// Can be serialized to clients to render full game state.
    /// Can be sent to AI to make game decisions.
    /// </summary>
    public class GameState
    {
        public PlayerId currentPlayer { get; set; } = PlayerId.OMNISCIENT;

        public PlayerState[] playerStates { get; set; } = [new PlayerState(), new PlayerState()];
    }

    /// <summary>
    /// State of a player, to be contained in game state
    /// </summary>
    public class PlayerState
    {
        public int hp { get; set; } = 0;
        public int gold { get; set; } = 0;
        public int nBuildings { get; set; } = 0;
        public int nUnits { get; set; } = 0;
        public int handSize { get; set; } = 0;
        public int deckSize { get; set; } = 0;
        public string handInfo { get; set; } = "";
        public string deckInfo { get; set; } = "";
    }
}
