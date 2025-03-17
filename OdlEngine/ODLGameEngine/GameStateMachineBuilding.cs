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
                }
            }
            return firstBuildableTile;
        }
    }
}
