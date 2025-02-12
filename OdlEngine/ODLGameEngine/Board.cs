using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class Tile
    {
        public int BuildingInTile { get; set; } = 0;
        public SortedSet<int> UnitsInTile { get; set; } = new SortedSet<int>();
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
    public class Lane /// Player 0 goes from 0 -> N-1 and vice versa. Absolute truth is always w.r.t. player 0
    {
        public LaneID Id {get; set;} = LaneID.NO_LANE;
        public int Len { get; set; } = 0;
        public List<Tile> Tiles { get; set; }
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

        public override string ToString()
        {
            return Id.ToString() + $", P1: {PlayerUnitCount[0]} P2: {PlayerUnitCount[1]}";
        }
    }

    /// <summary>
    /// Contains board content (state) as well as a method to serialize exactly whats going on
    /// </summary>
    public class Board
    {
        public Lane PlainsLane { get; set; } = new Lane(LaneID.PLAINS, GameConstants.PLAINS_TILES_NUMBER);
        public Lane ForestLane { get; set; } = new Lane(LaneID.FOREST, GameConstants.FOREST_TILES_NUMBER);
        public Lane MountainLane { get; set; } = new Lane(LaneID.MOUNTAIN, GameConstants.MOUNTAIN_TILES_NUMBER);
        // Units
        public SortedList<int, Unit> PlayerUnits { get; set; } = new SortedList<int, Unit>();
        public SortedList<int, Building> PlayerBuildings { get; set; } = new SortedList<int, Building>();
        public SortedList<int, Unit> DeadUnits { get; set; } = new SortedList<int, Unit>();
        public SortedList<int, Building> DeadBuildings { get; set; } = new SortedList<int, Building>();
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
    }
}
