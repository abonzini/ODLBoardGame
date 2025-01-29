using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public static class GameConstants
    {
        public const int BOARD_LANES_NUMBER = 3;
        public const int PLAINS_TILES_NUMBER = 4;
        public const int FOREST_TILES_NUMBER = 6;
        public const int MOUNTAIN_TILES_NUMBER = 8;
        // May need to be moved to player class if different between them or sth
        public const int STARTING_HP = 30;
        public const int STARTING_GOLD = 5;
        public const int STARTING_CARDS = 4;
        public const int DECK_SIZE = 30;
        public const int DRAW_PHASE_CARDS_DRAWN = 1;
        public const int DRAW_PHASE_GOLD_OBTAINED = 2;
    }

}
