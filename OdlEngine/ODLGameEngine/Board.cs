using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ODLGameEngine
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Tile : IHashable
    {
        [JsonProperty]
        public int BuildingInTile { get; set; } = 0;
        [JsonProperty]
        public SortedSet<int> UnitsInTile { get; set; } = new SortedSet<int>();
        [JsonProperty]
        public int[] PlayerUnitCount { get; set; } = [0, 0];

        public int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(BuildingInTile);
            hash.Add(PlayerUnitCount[0]);
            hash.Add(PlayerUnitCount[1]);
            foreach(int unit in UnitsInTile)
            {
                hash.Add(unit);
            }
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"P1: {PlayerUnitCount[0]} P2: {PlayerUnitCount[1]}";
        }
    }
    /// <summary>
    /// Lanes
    /// </summary>
    public enum LaneID
    {
        NO_LANE,
        PLAINS,
        FOREST,
        MOUNTAIN
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class Lane : IHashable /// Player 0 goes from 0 -> N-1 and vice versa. Absolute truth is always w.r.t. player 0
    {
        [JsonProperty]
        public LaneID Id {get; set;} = LaneID.NO_LANE;
        [JsonProperty] 
        public int Len { get; set; } = 0;
        [JsonProperty]
        public List<Tile> Tiles { get; set; }
        [JsonProperty]
        public int[] PlayerUnitCount { get; set; } = [0, 0];
        public Lane(LaneID id, int n)
        {
            Id = id;
            Len = n;
            Tiles = new List<Tile>(n);
            for(int i = 0; i < Len; i++)
            {
                Tiles.Add(new Tile());
            }
        }
        /// <summary>
        /// Gets the tile instance given the index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The actual instance of a tile to place units</returns>
        public Tile GetTileAbsolute(int index)
        {
            return Tiles[index];
        }
        /// <summary>
        /// Gets tile relative to player, so 1,2,3,4 etc from player's POV. Negative indices work like in python, starting from beginning
        /// </summary>
        /// <param name="index">Tile index</param>
        /// <param name="player">Relative to what?</param>
        /// <returns></returns>
        public Tile GetTileRelative(int index, int player)
        {
            if(index < 0) // Pyhton notation, need to add n, so that -1 -> n-1 and -n becomes 0
            {
                index += Len;
            }
            if(player == 1) // Next, for player, I need to flip, so that first is last and vice versa. This involves n-1 complement
            {
                index = Len - 1 - index;
            }
            // Now that I got the right index, return correct value...
            return GetTileAbsolute(index);
        }
        /// <summary>
        /// Returns the index of the first tile, but relative to a player
        /// </summary>
        /// <param name="player">Player</param>
        /// <returns>Tile index</returns>
        public int GetFirstTileCoord(int player) /// Returns the first tile of the lane (to spawn units)
        {
            return (player != 0) ? Len - 1 : 0;
        }
        /// <summary>
        /// Returns the index of the last tile, but relative to a player
        /// </summary>
        /// <param name="player">Player</param>
        /// <returns>Tile index</returns>
        public int GetLastTileCoord(int player) /// Returns the edge of the lane (to decide if advance or damage castle)
        {
            return (player != 1) ? Len - 1 : 0;
        }
        public static int GetAdvanceDirection(int player)
        {
            return (player == 0) ? 1 : -1;
        }
        public override string ToString()
        {
            return Id.ToString() + $", P1: {PlayerUnitCount[0]} P2: {PlayerUnitCount[1]}";
        }

        public int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            hash.Add(Len); // So that different lanes have different hashes even if empty
            hash.Add(PlayerUnitCount[0]);
            hash.Add(PlayerUnitCount[1]);
            foreach (Tile tile in Tiles)
            {
                hash.Add(tile.GetGameStateHash());
            }
            return hash.ToHashCode();
        }
    }

    /// <summary>
    /// Contains board content (state) as well as a method to serialize exactly whats going on.
    /// Lanes for now do not contain and shoudl not contain info that isn't found elsewhere.
    /// E.g. lanes/tiles know their units, but units also know their coords, so the hash can only be dependent on the unit to change the game state.
    /// Board however needs to be hashable as it's the root entity that stores and controls unit info.
    /// Lanes and tiles however need to be serialized as json to be quickly rendered into a game UI. They're the only ones that are serialized but not hashed!
    /// This may change in the future if there's extra tile modifiers that are stored in a tile but not on Board directly, in which change we may change the whole hashing scheme
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Board : IHashable
    {
        [JsonProperty]
        public Lane PlainsLane { get; set; } = new Lane(LaneID.PLAINS, GameConstants.PLAINS_TILES_NUMBER);
        [JsonProperty]
        public Lane ForestLane { get; set; } = new Lane(LaneID.FOREST, GameConstants.FOREST_TILES_NUMBER);
        [JsonProperty]
        public Lane MountainLane { get; set; } = new Lane(LaneID.MOUNTAIN, GameConstants.MOUNTAIN_TILES_NUMBER);
        // Units
        [JsonProperty]
        public readonly SortedList<int, Unit> Units = new SortedList<int, Unit>();
        [JsonProperty]
        public readonly SortedList<int, Building> Buildings = new SortedList<int, Building>();
        // Methods
        public Lane GetLane(int i)
        {
            return i switch
            {
                0 => PlainsLane,
                1 => ForestLane,
                2 => MountainLane,
                _ => throw new IndexOutOfRangeException("Chosen lane higher than lane count"),
            };
        }
        public Lane GetLane(LaneID laneID)
        {
            return laneID switch
            {
                LaneID.NO_LANE => null,
                LaneID.PLAINS => PlainsLane,
                LaneID.FOREST => ForestLane,
                LaneID.MOUNTAIN => MountainLane,
                _ => throw new Exception("Unrecognized lane requested"),
            };
        }
        public Lane GetLane(CardTargets laneTarget)
        {
            return laneTarget switch
            {
                CardTargets.PLAINS => PlainsLane,
                CardTargets.FOREST => ForestLane,
                CardTargets.MOUNTAIN => MountainLane,
                _ => throw new Exception("Unrecognized lane requested"),
            };
        }
        public virtual int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            foreach (KeyValuePair<int, Unit> kvp in Units)
            {
                hash.Add(kvp.Key);
                hash.Add(kvp.Value.GetGameStateHash());
            }
            foreach (KeyValuePair<int, Building> kvp in Buildings)
            {
                hash.Add(kvp.Key);
                hash.Add(kvp.Value.GetGameStateHash());
            }
            return hash.ToHashCode();
        }
    }
}
