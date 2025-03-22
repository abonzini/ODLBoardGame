using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public partial class GameStateMachine // General handler of buildings
    {
        /// <summary>
        /// Returns the first possible tile where a buildign can be placed
        /// </summary>
        /// <param name="player">Which player woudl own the building</param>
        /// <param name="building">Building card, has the blueprints</param>
        /// <param name="laneTarget">Which lane I wanted to try</param>
        /// <returns>First possible tile, -1 if none</returns>
        /// <exception cref="InvalidDataException">If lane is incorrect</exception>
        public int BUILDING_GetFirstBuildableTile(int player, Building building, CardTargets laneTarget)
        {
            int[] bp = laneTarget switch // get the right BP
            {
                CardTargets.PLAINS => building.PlainsBp,
                CardTargets.FOREST => building.ForestBp,
                CardTargets.MOUNTAIN => building.MountainBp,
                _ => throw new InvalidDataException("Invalid lane target fot building!"),
            };
            Lane lane = _detailedState.BoardState.GetLane(laneTarget);
            int firstBuildableTile = -1;
            foreach (int tileCandidate in bp)
            {
                Tile tile = lane.GetTileRelative(tileCandidate, player); // Gets next tile candidate
                if(tile.BuildingInTile < 0 && tile.PlayerUnitCount[player] > 0) // Condition is, tile can't have building already, and there has to be atleast one unit to build it
                {
                    firstBuildableTile = tileCandidate;
                    break;
                }
            }
            return firstBuildableTile;
        }
        /// <summary>
        /// Spawns the building for a player
        /// </summary>
        /// <param name="player">Player</param>
        /// <param name="bldg">Building</param>
        /// <param name="chosenTarget">Target chosen for building</param>

        void BUILDING_PlayBuilding(int player, Building bldg, CardTargets chosenTarget)
        {
            // To spawn a building, first you get the playable ID
            // Clone the building, add to board
            // "Change" the coordinate of unit to right place
            Building newSpawnedBuilding = (Building)bldg.Clone(); // Clone in order to not break the same species
            if (newSpawnedBuilding.Name == "") { newSpawnedBuilding.Name = newSpawnedBuilding.EntityPrintInfo.Title; }
            newSpawnedBuilding.Owner = player;
            BOARDENTITY_InitializeEntity(newSpawnedBuilding);
            // Locates unit to right place. Get the lane where unit is played, and place it in first tile
            Lane bldgLane = _detailedState.BoardState.GetLane(chosenTarget);
            BOARDENTITY_InsertInLane(newSpawnedBuilding, bldgLane.Id);
            int tileCoord = BUILDING_GetFirstBuildableTile(player, bldg, chosenTarget);
            tileCoord = bldgLane.GetAbsoluteTileCoord(tileCoord, player); // Transform tile to absolute
            BOARDENTITY_InsertInTile(newSpawnedBuilding, tileCoord);
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            BOARDENTITY_CheckIfUnitAlive(newSpawnedBuilding);
        }
    }
}
