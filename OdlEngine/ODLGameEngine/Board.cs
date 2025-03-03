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
    public class Tile
    {
        [JsonProperty]
        public int BuildingInTile { get; set; } = 0;
        [JsonProperty]
        public SortedSet<int> UnitsInTile { get; set; } = new SortedSet<int>();
        [JsonProperty]
        public int[] PlayerUnitCount { get; set; } = [0, 0];
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
    public class Lane /// Player 0 goes from 0 -> N-1 and vice versa. Absolute truth is always w.r.t. player 0
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
        public Tile GetTile(int index)
        {
            return Tiles[index];
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
        protected bool _dirtyHash = true; // To see if hash needs to be recalculated
        protected int _hash;
        [JsonProperty]
        public Lane PlainsLane { get; set; } = new Lane(LaneID.PLAINS, GameConstants.PLAINS_TILES_NUMBER);
        [JsonProperty]
        public Lane ForestLane { get; set; } = new Lane(LaneID.FOREST, GameConstants.FOREST_TILES_NUMBER);
        [JsonProperty]
        public Lane MountainLane { get; set; } = new Lane(LaneID.MOUNTAIN, GameConstants.MOUNTAIN_TILES_NUMBER);
        // Units
        [JsonProperty]
        private readonly SortedList<int, Unit> _units = new SortedList<int, Unit>();
        [JsonProperty]
        private readonly SortedList<int, Building> _buildings = new SortedList<int, Building>();
        // Methods
        /// <summary>
        /// Returns the unit dictionary, can retrieve both the current and dead units. Can also be used when just looking for somethign without editing
        /// </summary>
        /// <param name="edit">Whether a unit may be modified by this process (may need to update hash)</param>
        /// <param name="alive">Do I want the live units or the dead (past) units?</param>
        /// <returns>The corresponding sorted list to do operations with</returns>
        public SortedList<int, Unit> GetUnitContainer(bool edit = true)
        {
            if (edit) _dirtyHash = true;
            return _units;
        }
        /// <summary>
        /// Returns the building dictionary, can retrieve both the current and dead building. Can also be used when just looking for somethign without editing
        /// </summary>
        /// <param name="edit">Whether a building may be modified by this process (may need to update hash)</param>
        /// <param name="alive">Do I want the live building or the dead (past) building?</param>
        /// <returns>The corresponding sorted list to do operations with</returns>
        public SortedList<int, Building> GetBuildingContainer(bool edit = true)
        {
            if (edit) _dirtyHash = true;
            return _buildings;
        }
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
        public virtual int GetHash()
        {
            if (_dirtyHash) // Recalculates only when dirty
            {
                HashCode hash = new HashCode();
                foreach (KeyValuePair<int, Unit> kvp in _units)
                {
                    hash.Add(kvp.Key);
                    hash.Add(kvp.Value.GetHash());
                }
                foreach (KeyValuePair<int, Building> kvp in _buildings)
                {
                    hash.Add(kvp.Key);
                    hash.Add(kvp.Value.GetHash());
                }
                _hash = hash.ToHashCode();
                _dirtyHash = false; // Currently updated hash
            }
            return _hash;
        }
        public bool IsHashDirty()
        {
            return _dirtyHash;
        }
    }
}
