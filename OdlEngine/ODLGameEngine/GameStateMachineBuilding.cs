namespace ODLGameEngine
{
    public partial class GameStateMachine // General handler of buildings
    {
        /// <summary>
        /// Given a building attempt, returns valid options plus construction info that may be useful for later effects
        /// </summary>
        /// <param name="player">Player who would own the building</param>
        /// <param name="building">Building to built</param>
        /// <param name="laneTarget">Which lane is attempted</param>
        /// <returns>A construction context that contains, among other things, where the building can be built and stuff</returns>
        public ConstructionContext BUILDING_GetBuildingOptions(int player, Building building, PlayTargetLocation laneTarget)
        {
            ConstructionContext res = new ConstructionContext();
            int[] bp = laneTarget switch // get the right BP
            {
                PlayTargetLocation.PLAINS => building.PlainsBp,
                PlayTargetLocation.FOREST => building.ForestBp,
                PlayTargetLocation.MOUNTAIN => building.MountainBp,
                _ => throw new InvalidDataException("Invalid lane target fot building!"),
            };
            Lane lane = DetailedState.BoardState.GetLane(laneTarget);
            int i;
            for (i = 0; i < bp.Length; i++)
            {
                // Get the next tile referenced in the Bp
                Tile tile = lane.GetTileFromCoordinate(LaneRelativeIndexType.RELATIVE_TO_PLAYER, bp[i], player);
                if (tile.GetPlacedEntities(EntityType.BUILDING).Count == 0 && tile.GetPlacedEntities(EntityType.UNIT, player).Count > 0) // Condition is, tile can't have building already, and there has to be atleast one unit to build it
                {
                    res.Actor = (Unit)DetailedState.EntityData[tile.GetPlacedEntities(EntityType.UNIT, player).First()];
                    res.Affected = building;
                    res.AbsoluteConstructionTile = tile.Coord; // This is the tile!
                    break;
                }
            }
            return res;
        }
        /// <summary>
        /// Constructs a building, given a player owner and the constructionContext that has all the data necessary for building to appear
        /// </summary>
        /// <param name="player">Player who'll own the building</param>
        /// <param name="constructionContext">Building construction context</param>
        /// <returns>The initialised building</returns>
        public Building BUILDING_ConstructBuilding(int player, ConstructionContext constructionContext)
        {
            // Clone building
            Building newSpawnedBuilding = (Building)constructionContext.Affected.Clone();
            constructionContext.Affected = newSpawnedBuilding; // Construction context updated with actual building
            newSpawnedBuilding.Owner = player; // Building owner
            // Add to board
            LIVINGENTITY_InitializeEntity(newSpawnedBuilding);
            // Places building in new coordinate
            LIVINGENTITY_InsertInTile(newSpawnedBuilding, constructionContext.AbsoluteConstructionTile);
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            LIVINGENTITY_CheckIfUnitAlive(newSpawnedBuilding);
            return newSpawnedBuilding;
        }
    }
}
