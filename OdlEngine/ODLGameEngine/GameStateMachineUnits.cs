using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public partial class GameStateMachine // Deals with unit and unit related stuff, maybe combat
    {
        void UNIT_PlayUnit(int player, Unit unit, CardTargets chosenTarget)
        {
            // To spawn a unit, first you get the playable ID
            // Clone the unit, get the id there
            // Then add to unit lists, now we have the unit defined
            // "Change" the coordinate of unit to right place
            // Add unit to board, this adds to tile, increases the counters in board and such
            // Finally, playable ID is incremented for next playable
            int unitId = _nextPlayableId;
            Unit newSpawnedUnit = (Unit)unit.Clone(); // Clone in order to not break the same species
            newSpawnedUnit.UniqueId = unitId;
            // Unit ready to be added
            ENGINE_InitializeUnit(player, newSpawnedUnit); // Now player has the unit
            // Locates unit to right place
            // TODO TODO
        }
    }
}
