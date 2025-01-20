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
        public string Tag { get; set; } = "";
        public int BuildingInTile { get; set; } = 0;
        public SortedSet<int> UnitsInTile { get; set; } = new SortedSet<int>();
        public int[] PlayerUnitCount { get; set; } = [0, 0];
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
        public LaneID Id {get; set;} = LaneID.NO_LANE;
        public int Len { get; set; } = 0;
        public List<Tile> Tiles { get; set; }
        public int[] PlayerUnitCount { get; set; } = [0, 0];
        public Lane(int n)
        {
            Len = n;
            Tiles = new List<Tile>(n);
        }

        public IEnumerable<Tile> GetTiles(PlayerId player, bool ascending = true) /// Returns tile one by one relative to desired player
        {
            int start, end, increment; 
            // calculate order depending if I want ascending or descending
            if(PlayerId.PLAYER_2 != player ^ !ascending) // All use the reference of 0 = beginning except player 2
            {
                start = 0;
                end = Len-1;
                increment = 1;
            }
            else
            {
                start = Len-1;
                end = 0;
                increment = -1;
            }

            for (int i = start; increment * i <= increment * end; i += increment)
            {
                yield return Tiles[i];
            }
        }

        public Tile GetTile(PlayerId player, int index)
        {
            if(0 <= index && index < Len)
            {
                throw new IndexOutOfRangeException("Desired index out of bounds for this lane");
            }
            // If player is p2, reverse the desired index
            if(PlayerId.PLAYER_2 == player)
            {
                index = Len - 1 - index;
            }
            return Tiles[index];
        }

        public int GetLastTile(PlayerId player) /// Returns the edge of the tile (to decide if advance or damage castle)
        {
            if (player != PlayerId.PLAYER_2) return Len - 1;
            else return 0;
        }
    }

    /// <summary>
    /// Contains board content (state) as well as a method to serialize exactly whats going on
    /// </summary>
    public class Board
    {
        public Lane PlainsLane { get; set; } = new Lane(GameConstants.PLAINS_TILES_NUMBER);
        public Lane ForestLane { get; set; } = new Lane(GameConstants.FOREST_TILES_NUMBER);
        public Lane MountainLane { get; set; } = new Lane(GameConstants.MOUNTAIN_TILES_NUMBER);
        // Units
        public SortedList<int, Unit>[] PlayerUnits { get; set; } = [new SortedList<int, Unit>(), new SortedList<int, Unit>()];
        public SortedList<int, Building>[] PlayerBuildings { get; set; } = [new SortedList<int, Building>(), new SortedList<int, Building>()];
        public SortedList<int, Unit>[] DeadUnits { get; set; } = [new SortedList<int, Unit>(), new SortedList<int, Unit>()];
        public SortedList<int, Building>[] DeadBuildings { get; set; } = [new SortedList<int, Building>(), new SortedList<int, Building>()];
        // Methods
        public int LaneCount { get; set; } = 3;
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
                LaneID.LANE_PLAINS => PlainsLane,
                LaneID.LANE_FOREST => ForestLane,
                LaneID.LANE_MOUNTAIN => MountainLane,
                _ => throw new Exception("Unrecognized lane requested"),
            };
        }
    }
}
