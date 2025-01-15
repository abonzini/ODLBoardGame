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
        public string tag { get; set; } = "";
        public int buildingInTile { get; set; } = 0;
        public SortedSet<int> unitsInTile { get; set; } = new SortedSet<int>();
        public int[] playerUnitCount { get; set; } = [0, 0];
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
    public class Lane /// Player 0 goes from 0 -> N-1 and vice versa. Absolute truth is always w.r.t. player 0
    {
        public LaneID id {get; set;} = LaneID.NO_LANE;
        public int len { get; set; } = 0;
        public List<Tile> tiles { get; set; }
        public int[] playerUnitCount { get; set; } = [0, 0];
        public Lane(int n)
        {
            len = n;
            tiles = new List<Tile>(n);
        }

        public IEnumerable<Tile> GetTiles(PlayerId player, bool ascending = true) /// Returns tile one by one relative to desired player
        {
            int start, end, increment; 
            // calculate order depending if I want ascending or descending
            if(PlayerId.PLAYER_2 != player ^ !ascending) // All use the reference of 0 = beginning except player 2
            {
                start = 0;
                end = len-1;
                increment = 1;
            }
            else
            {
                start = len-1;
                end = 0;
                increment = -1;
            }

            for (int i = start; increment * i <= increment * end; i += increment)
            {
                yield return tiles[i];
            }
        }

        public Tile GetTile(PlayerId player, int index)
        {
            if(0 <= index && index < len)
            {
                throw new IndexOutOfRangeException("Desired index out of bounds for this lane");
            }
            // If player is p2, reverse the desired index
            if(PlayerId.PLAYER_2 == player)
            {
                index = len - 1 - index;
            }
            return tiles[index];
        }

        public int GetLastTile(PlayerId player) /// Returns the edge of the tile (to decide if advance or damage castle)
        {
            if (player != PlayerId.PLAYER_2) return len - 1;
            else return 0;
        }
    }

    /// <summary>
    /// Contains board content (state) as well as a method to serialize exactly whats going on
    /// </summary>
    public class Board
    {
        public Lane plainsLane { get; set; } = new Lane(GameConstants.PLAINS_TILES_NUMBER);
        public Lane forestLane { get; set; } = new Lane(GameConstants.FOREST_TILES_NUMBER);
        public Lane mountainLane { get; set; } = new Lane(GameConstants.MOUNTAIN_TILES_NUMBER);
        // Units
        public SortedList<int, Unit>[] playerUnits { get; set; } = [new SortedList<int, Unit>(), new SortedList<int, Unit>()];
        public SortedList<int, Building>[] playerBuildings { get; set; } = [new SortedList<int, Building>(), new SortedList<int, Building>()];
        public SortedList<int, Unit>[] deadUnits { get; set; } = [new SortedList<int, Unit>(), new SortedList<int, Unit>()];
        public SortedList<int, Building>[] deadBuildings { get; set; } = [new SortedList<int, Building>(), new SortedList<int, Building>()];
        // Methods
        public int laneCount { get; set; } = 3;
        public Lane GetLane(int i)
        {
            switch(i)
            {
                case 0: return plainsLane;
                case 1: return forestLane;
                case 2: return mountainLane;
                default: throw new IndexOutOfRangeException("Chosen lane higher than lane count");
            }
        }
        public Lane GetLane(LaneID laneID)
        {
            switch (laneID)
            {
                case LaneID.NO_LANE: return null;
                case LaneID.LANE_PLAINS: return plainsLane;
                case LaneID.LANE_FOREST: return forestLane;
                case LaneID.LANE_MOUNTAIN: return mountainLane;
                default: throw new Exception("Unrecognized lane requested");
            }
        }
    }
}
