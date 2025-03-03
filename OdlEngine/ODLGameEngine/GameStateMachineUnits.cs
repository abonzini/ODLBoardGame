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
            int unitId = _detailedState.NextUnitIndex;
            Unit newSpawnedUnit = (Unit)unit.Clone(); // Clone in order to not break the same species
            newSpawnedUnit.UniqueId = unitId;
            newSpawnedUnit.Owner = player;
            // Unit ready to be added
            ENGINE_InitializeUnit(newSpawnedUnit); // Now player has the unit
            // Locates unit to right place. Get the lane where unit is played, and place it in first tile
            Lane unitLane = _detailedState.BoardState.GetLane(chosenTarget);
            int tileCoord = unitLane.GetFirstTileCoord(player); // Get tile coord
            ENGINE_UnitLaneTransition(unitId, unitLane.Id);
            ENGINE_UnitTileTransition(unitId, tileCoord);
            // Finally, need to increment playable counter
            ENGINE_IncrementPlaceableCounter();
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            UNIT_VerifyUnitHpChange(unitId);
        }
        /// <summary>
        /// Verifies if unit HP has changed and unit is ready to die, or some other effect that triggers
        /// </summary>
        /// <param name="unitId">Which unit</param>
        void UNIT_VerifyUnitHpChange(int unitId)
        {
            // Get the unit, if still existing and alive. This action just checks and doesn't modiy game step
            if (_detailedState.BoardState.Units.TryGetValue(unitId, out Unit unit))
            {
                if(unit.Hp <= 0) // Unit is dead, move to graveyard
                {
                    UNIT_KillUnit(unitId);
                }
            }
        }
        /// <summary>
        /// Unit needs to be killed for whatever reason, this process executes the action
        /// </summary>
        /// <param name="unitId">Which unit</param>
        void UNIT_KillUnit(int unitId)
        {
            Unit unit = _detailedState.BoardState.Units[unitId];
            ENGINE_AddMessageEvent($"P{unit.Owner+1}'s {unit.Name} has been killed");
            // Removes unit from its space, first from tile and then from lane!
            ENGINE_UnitTileTransition(unitId, -1);
            ENGINE_UnitLaneTransition(unitId, LaneID.NO_LANE);
            // Moves unit from living space to dead space
            ENGINE_DeinitializeUnit(unit);            
        }
        /// <summary>
        /// Found unit starts a march (initial march checks and march modifiers already applied)
        /// </summary>
        /// <param name="unit">Unit object that will advance</param>
        void UNIT_AdvanceUnit(Unit unit)
        {
            int unitOwnerId = unit.Owner;
            int opponentId = 1 - unitOwnerId;
            int cooldown = unit.MvtCooldownTimer;
            // TODO LATER: Good place for the stone road buff token, and remove after ending function
            if (cooldown == 0)
            {
                ENGINE_AddMessageEvent($"P{unitOwnerId + 1}'s {unit.Name} advances");
                int n = unit.Movement; // How much to advance
                Lane lane = _detailedState.BoardState.GetLane(unit.LaneCoordinate); // Which lane
                while(n > 0) // Advancement loop, will advance until n is 0. This allow external modifiers to halt advance hopefully
                {
                    // Exiting current tile
                    if (lane.GetTile(unit.TileCoordinate).PlayerUnitCount[opponentId] > 0) // If enemy unit in tile, will stop march here (and also attack)
                    {
                        n = 0;
                        // TODO BATTLE!
                    }
                    else if (lane.GetLastTileCoord(unitOwnerId) == unit.TileCoordinate) // Otherwise, if unit in last tile won't advance (and attack enemy player)
                    {
                        n = 0;
                        // TODO Player damage!
                    }
                    else // Unit then can advance normally here, perform it
                    {
                        // Request unit advancement a tile
                        ENGINE_UnitTileTransition(unit.UniqueId, unit.TileCoordinate + Lane.GetAdvanceDirection(unitOwnerId));
                        // Entering new tile
                        // TODO: Building damage, building effects
                        n--;
                    }
                }
            }
            cooldown++; // Cycle the timer so that next advance it's updated!
            cooldown %= unit.MovementDenominator;
            if(unit.MvtCooldownTimer != cooldown) // If unit has changed cooldown, need to activate this
            {
                ENGINE_UnitMovementCooldownChange(unit.UniqueId, cooldown);
            }
        }
    }
}
