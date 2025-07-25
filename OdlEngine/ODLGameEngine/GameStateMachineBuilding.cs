namespace ODLGameEngine
{
    public partial class GameStateMachine // General handler of buildings
    {
        /// <summary>
        /// Constructs a building, given a player owner and the constructionContext that has all the data necessary for building to appear
        /// </summary>
        /// <param name="player">Player who'll own the building</param>
        /// <param name="constructionContext">Construction context to separate unit target from actual tile</param>
        /// <returns>The initialised building</returns>
        public Building BUILDING_ConstructBuilding(int player, ConstructionContext constructionContext)
        {
            // Clone building
            Building newSpawnedBuilding = (Building)constructionContext.Affected.Clone();
            constructionContext.Affected = newSpawnedBuilding; // Refresh affected
            newSpawnedBuilding.Owner = player; // Building owner
            // Add to board and into coordinate
            LIVINGENTITY_InitializeEntity(newSpawnedBuilding);
            LIVINGENTITY_InsertInTile(newSpawnedBuilding, constructionContext.AbsoluteConstructionTile);
            // At this point, building has been constructed and placed, so now notify the "construction" interaction from both POVs
            constructionContext.ActivatedEntity = constructionContext.Actor;
            TRIGINTER_ProcessInteraction(InteractionType.UNIT_CONSTRUCTS_BUILDING, constructionContext);
            constructionContext.ActivatedEntity = constructionContext.Affected;
            TRIGINTER_ProcessInteraction(InteractionType.UNIT_CONSTRUCTS_BUILDING, constructionContext);
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            LIVINGENTITY_CheckIfUnitAlive(newSpawnedBuilding, true);
            // TODO: Construction events
            //ConstructionContext constructionContext = new ConstructionContext();
            //constructionContext.Affected = newSpawnedBuilding; // Construction context updated with actual building
            // Etc
            return newSpawnedBuilding;
        }
    }
}
