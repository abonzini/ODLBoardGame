﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public partial class GameStateMachine // General handler of buildings
    {
        /// <summary>
        /// Given a building attempt, returns valid options plus construction info that may be useful for later effects
        /// </summary>
        /// <param name="player">Player who owns the building</param>
        /// <param name="building">Building to build</param>
        /// <param name="laneTarget">Which lane is attempted</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public ConstructionContext BUILDING_GetBuildingOptions(int player, Building building, TargetLocation laneTarget)
        {
            ConstructionContext res = new ConstructionContext();
            int[] bp = laneTarget switch // get the right BP
            {
                TargetLocation.PLAINS => building.PlainsBp,
                TargetLocation.FOREST => building.ForestBp,
                TargetLocation.MOUNTAIN => building.MountainBp,
                _ => throw new InvalidDataException("Invalid lane target fot building!"),
            };
            Lane lane = DetailedState.BoardState.GetLane(laneTarget);
            int i;
            for (i = 0; i < bp.Length; i++)
            {
                Tile tile = lane.GetTileRelative(bp[i], player); // Gets next tile candidate
                if (tile.GetPlacedEntities(EntityType.BUILDING).Count == 0 && tile.GetPlacedEntities(EntityType.UNIT, player).Count > 0) // Condition is, tile can't have building already, and there has to be atleast one unit to build it
                {
                    res.Affected = building;
                    res.FirstAvailableOption = i;
                    res.RelativeTile = bp[i];
                    res.AbsoluteTile = lane.GetAbsoluteTileCoord(bp[i], player);
                    res.Actor = (Unit)DetailedState.EntityData[tile.GetPlacedEntities(EntityType.UNIT, player).First()];
                    break;
                }
            }
            return res;
        }
        /// <summary>
        /// Spawns the building for a player
        /// </summary>
        /// <param name="player">Player</param>
        /// <param name="bldg">Building</param>
        /// <param name="chosenTarget">Target chosen for building</param>
        /// <returns>The created building</returns>
        Building BUILDING_PlayBuilding(int player, Building bldg, TargetLocation chosenTarget)
        {
            // To spawn a building, first you get the playable ID
            Building newSpawnedBuilding = (Building)bldg.Clone(); // Clone in order to not break the same species
            newSpawnedBuilding.Owner = player;
            // Create the context
            ConstructionContext constructionCtx = BUILDING_GetBuildingOptions(player, newSpawnedBuilding, chosenTarget);
            // Clone the building, add to board
            BOARDENTITY_InitializeEntity(newSpawnedBuilding);
            // Locates unit to right place. Get the lane where unit is played, and place it in first tile
            Lane bldgLane = DetailedState.BoardState.GetLane(chosenTarget);
            BOARDENTITY_InsertInLane(newSpawnedBuilding, bldgLane.Id);
            BOARDENTITY_InsertInTile(newSpawnedBuilding, constructionCtx.AbsoluteTile);
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            BOARDENTITY_CheckIfUnitAlive(newSpawnedBuilding);
            return newSpawnedBuilding;
        }
    }
}
