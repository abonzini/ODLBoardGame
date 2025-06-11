namespace ODLGameEngine
{
    public partial class GameStateMachine // General handler of buildings
    {
        /// <summary>
        /// Constructs a building, given a player owner and the constructionContext that has all the data necessary for building to appear
        /// </summary>
        /// <param name="player">Player who'll own the building</param>
        /// <param name="playContext">Playability context</param>
        /// <returns>The initialised building</returns>
        public Building BUILDING_ConstructBuilding(int player, PlayContext playContext)
        {
            // Clone building
            Building newSpawnedBuilding = (Building)playContext.Actor.Clone();
            newSpawnedBuilding.Owner = player; // Building owner
            // Add to board
            LIVINGENTITY_InitializeEntity(newSpawnedBuilding);
            // Places building in new coordinate
            Unit buildingUnit = (Unit)DetailedState.EntityData[playContext.PlayedTarget]; // Building target will be the builder unit
            LIVINGENTITY_InsertInTile(newSpawnedBuilding, buildingUnit.TileCoordinate);
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            LIVINGENTITY_CheckIfUnitAlive(newSpawnedBuilding);
            // TODO: Construction events
            //ConstructionContext constructionContext = new ConstructionContext();
            //constructionContext.Affected = newSpawnedBuilding; // Construction context updated with actual building
            // Etc
            return newSpawnedBuilding;
        }
    }
}
