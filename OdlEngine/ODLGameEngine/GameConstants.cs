namespace ODLGameEngine
{
    public static class GameConstants
    {
        // Sizes
        public const int BOARD_NUMBER_OF_LANES = 3;
        public const int PLAINS_NUMBER_OF_TILES = 4;
        public const int FOREST_NUMBER_OF_TILES = 6;
        public const int MOUNTAIN_NUMBER_OF_TILES = 8;
        public const int BOARD_NUMBER_OF_TILES = PLAINS_NUMBER_OF_TILES + FOREST_NUMBER_OF_TILES + MOUNTAIN_NUMBER_OF_TILES;
        // May need to be moved to player class if different between them or sth
        public const int STARTING_HP = 20;
        public const int STARTING_GOLD = 5;
        public const int STARTING_CARDS = 4;
        public const int DECK_SIZE = 30;
        public const int DRAW_PHASE_CARDS_DRAWN = 1;
        public const int DRAW_PHASE_GOLD_OBTAINED = 2;
        public const int DEFAULT_ACTIVE_POWER_ID = 1;
        public const int DECKOUT_DAMAGE = 5;
    }

}
