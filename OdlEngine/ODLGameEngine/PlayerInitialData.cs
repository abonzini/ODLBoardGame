namespace ODLGameEngine
{
    /// <summary>
    /// Defines a player who plays the game
    /// </summary>
    public class PlayerInitialData
    {
        /// <summary>
        /// Name of player
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// Class the player is using
        /// </summary>
        public PlayerTribe PlayerClass { get; set; } = PlayerTribe.BASE;
        /// <summary>
        /// The decks the player is starting with
        /// </summary>
        public List<int> InitialDecklist { get; set; } = new List<int>();
    }
}
