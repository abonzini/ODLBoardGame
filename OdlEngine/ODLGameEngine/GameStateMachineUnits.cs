using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public partial class GameStateMachine // Deals with unit and unit related stuff, maybe advancing
    {
        /// <summary>
        /// Initializes the new unit for the specified player
        /// </summary>
        /// <param name="player">Player</param>
        /// <param name="unit">Unit</param>
        /// <param name="chosenTarget">Lane that was chosen</param>
        void UNIT_PlayUnit(int player, Unit unit, CardTargets chosenTarget)
        {
            // To spawn a unit, first you get the playable ID
            // Clone the unit, add to board
            // "Change" the coordinate of unit to right place
            Unit newSpawnedUnit = (Unit)unit.Clone(); // Clone in order to not break the same species
            if(newSpawnedUnit.Name == "") { newSpawnedUnit.Name = newSpawnedUnit.EntityPrintInfo.Title; }
            newSpawnedUnit.Owner = player;
            BOARDENTITY_InitializeEntity(newSpawnedUnit);
            // Locates unit to right place. Get the lane where unit is played, and place it in first tile
            Lane unitLane = _detailedState.BoardState.GetLane(chosenTarget);
            BOARDENTITY_InsertInLane(newSpawnedUnit, unitLane.Id);
            int tileCoord = unitLane.GetAbsoluteTileCoord(0,player); // Get tile coord
            BOARDENTITY_InsertInTile(newSpawnedUnit, tileCoord);
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            BOARDENTITY_CheckIfUnitAlive(newSpawnedUnit);
        }
        /// <summary>
        /// Found unit starts advance
        /// </summary>
        /// <param name="unit">Unit object that will advance</param>
        void UNIT_AdvanceUnit(Unit unit)
        {
            int unitOwnerId = unit.Owner;
            int opponentId = 1 - unitOwnerId;
            int cooldown = unit.MvtCooldownTimer;
            AdvancingContext advanceCtx = new AdvancingContext() // Set context for advance-related interactions/triggers
            {
                AdvancingUnit = unit,
            };
            if (cooldown == 0)
            {
                ENGINE_AddMessageEvent($"P{unitOwnerId + 1}'s {unit.Name} advances");
                advanceCtx.InitialMovement = advanceCtx.CurrentMovement = unit.Movement; // How much to advance
                Lane lane = _detailedState.BoardState.GetLane(unit.LaneCoordinate); // Which lane
                while(advanceCtx.CurrentMovement > 0) // Advancement loop, will advance until n is 0. This allow external modifiers to halt advance hopefully
                {
                    // Exiting current tile
                    if (lane.GetTileAbsolute(unit.TileCoordinate).PlayerUnitCount[opponentId] > 0) // If enemy unit in tile, will stop advance here (and also attack)
                    {
                        advanceCtx.CurrentMovement = 0;
                        Unit enemyUnit = null;
                        foreach(int enemyCandidateId in lane.GetTileAbsolute(unit.TileCoordinate).UnitsInTile) // Check all units in tile
                        {
                            Unit enemyUnitCandidate = _detailedState.BoardState.Units[enemyCandidateId]; // Check next unit
                            if(enemyUnitCandidate.Owner == opponentId) // Check if belongs to opponent
                            {
                                enemyUnit = enemyUnitCandidate; // Found the candidate! End search
                                break;
                            }
                        }
                        if (enemyUnit == null) throw new Exception("There was no enemy unit in this tile after all, discrepancy in internal data!");
                        UNIT_Combat(unit, enemyUnit); // Let them fight.
                    }
                    else if (lane.GetAbsoluteTileCoord(-1, unitOwnerId) == unit.TileCoordinate) // Otherwise, if unit in last tile won't advance (and attack enemy player)
                    {
                        advanceCtx.CurrentMovement = 0;
                        UNIT_Combat(unit, _detailedState.PlayerStates[opponentId]); // Deal direct damage to enemy!
                    }
                    else // Unit then can advance normally here, perform it
                    {
                        advanceCtx.CurrentMovement--;
                        // Request unit advancement a tile
                        BOARDENTITY_InsertInTile(unit, unit.TileCoordinate + Lane.GetAdvanceDirection(unitOwnerId));
                        // Entering new tile
                        Tile newTile = _detailedState.BoardState.GetLane(unit.LaneCoordinate).GetTileAbsolute(unit.TileCoordinate);
                        if (newTile.BuildingInTile >= 0 && newTile.BuildingInTileOwner == opponentId) // Found a building too, need to damage it
                        {
                            Building enemyBuildingCandidate = _detailedState.BoardState.Buildings[newTile.BuildingInTile]; //Get the building in the tile
                            UNIT_Combat(unit, enemyBuildingCandidate);
                        }
                    }
                }
            }
            cooldown++; // Cycle the timer so that next advance it's updated!
            cooldown %= unit.MovementDenominator;
            if(unit.MvtCooldownTimer != cooldown) // If unit has changed cooldown, need to activate this
            {
                ENGINE_UnitMovementCooldownChange(unit, cooldown);
            }
        }
        /// <summary>
        /// Performs combat of units
        /// </summary>
        /// <param name="attacker">Attacking unit</param>
        /// <param name="defender">Defending unit</param>
        void UNIT_Combat(Unit attacker, BoardEntity defender)
        {
            DamageContext attackerDmgCtx, defenderDmgCtx;
            ENGINE_AddMessageEvent($"Combat between P{attacker.Owner + 1}'s {attacker.Name} and P{defender.Owner + 1}'s {defender.Name}");

            // Surely, unit will apply damage to the victim
            attackerDmgCtx = BOARDENTITY_DamageStep(attacker, defender, attacker.Attack); // TODO: GetAttack fn to incorporate buffs and such
            if(defender is Unit defendingUnit)
            {
                // If defender was also a unit, then the attacker also receives damage
                defenderDmgCtx = BOARDENTITY_DamageStep(defender, attacker, defendingUnit.Attack);
            }

            // TODO: Contexts are checked here! even death and damage taking, to avoid Units doing stuff in this critical damage step
        }
    }
}
