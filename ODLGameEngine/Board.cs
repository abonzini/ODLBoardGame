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
        string _tag = "";
        Building _buildingInTile = null; // I guess will be a single element list? (leaving space for flexibility)
        SortedList<int, Unit> _unitsInTile = new SortedList<int, Unit>();
        int[] _playersUnits = [0, 0];
    }
    public class Lane /// Player 0 goes from 0 -> N-1 and vice versa. Absolute truth is always w.r.t. player 0
    {
        int _len;
        List<Tile> _tiles;
        int[] _playersUnits = [0, 0];
        public Lane(int n)
        {
            _len = n;
            _tiles = new List<Tile>(n);
            _playersUnits = [0, 0]; // Players start w no units here
        }

        IEnumerable<Tile> GetTiles(PlayerId player, bool ascending = true) /// Returns tile one by one relative to desired player
        {
            int start, end, increment; 
            // calculate order depending if I want ascending or descending
            if(PlayerId.PLAYER_2 != player ^ !ascending) // All use the reference of 0 = beginning except player 2
            {
                start = 0;
                end = _len-1;
                increment = 1;
            }
            else
            {
                start = _len-1;
                end = 0;
                increment = -1;
            }

            for (int i = start; increment * i <= increment * end; i += increment)
            {
                yield return _tiles[i];
            }
        }

        Tile GetTile(PlayerId player, int index)
        {
            if(0 <= index && index < _len)
            {
                throw new IndexOutOfRangeException("Desired index out of bounds for this lane");
            }
            // If player is p2, reverse the desired index
            if(PlayerId.PLAYER_2 == player)
            {
                index = _len - 1 - index;
            }
            return _tiles[index];
        }

        int GetLastTile(PlayerId player) /// Returns the edge of the tile (to decide if advance or damage castle)
        {
            if (player != PlayerId.PLAYER_2) return _len - 1;
            else return 0;
        }
    }
}
