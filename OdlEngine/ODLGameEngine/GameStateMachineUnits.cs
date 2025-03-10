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
            ENGINE_UnitLaneTransition(newSpawnedUnit, unitLane.Id);
            ENGINE_UnitTileTransition(newSpawnedUnit, tileCoord);
            // Finally, need to increment playable counter
            ENGINE_IncrementPlaceableCounter();
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            UNIT_VerifyUnitHpChange(newSpawnedUnit);
        }
        /// <summary>
        /// Verifies if unit HP has changed and unit is ready to die, or some other effect that triggers
        /// </summary>
        /// <param name="unitId">Which unit</param>
        void UNIT_VerifyUnitHpChange(Unit unit)
        {
            if(unit.Hp - unit.DamageTokens <= 0) // Unit is dead, move to graveyard
            {
                UNIT_KillUnit(unit);
            }
        }
        /// <summary>
        /// Unit needs to be killed for whatever reason, this process executes the action
        /// </summary>
        /// <param name="unitId">Which unit</param>
        void UNIT_KillUnit(Unit unit)
        {
            ENGINE_AddMessageEvent($"P{unit.Owner+1}'s {unit.Name} has been killed");
            // Removes unit from its space, first from tile and then from lane!
            ENGINE_UnitTileTransition(unit, -1);
            ENGINE_UnitLaneTransition(unit, LaneID.NO_LANE);
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
                unit.CurrentRemainingAdvance = unit.Movement; // How much to advance
                Lane lane = _detailedState.BoardState.GetLane(unit.LaneCoordinate); // Which lane
                while(unit.CurrentRemainingAdvance > 0) // Advancement loop, will advance until n is 0. This allow external modifiers to halt advance hopefully
                {
                    // Exiting current tile
                    if (lane.GetTileAbsolute(unit.TileCoordinate).PlayerUnitCount[opponentId] > 0) // If enemy unit in tile, will stop march here (and also attack)
                    {
                        unit.CurrentRemainingAdvance = 0;
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
                        if (enemyUnit == null) throw new Exception("There was no enemy unit in this tile! Something broke!");
                        UNIT_Combat(unit, enemyUnit); // Let them fight.
                    }
                    else if (lane.GetLastTileCoord(unitOwnerId) == unit.TileCoordinate) // Otherwise, if unit in last tile won't advance (and attack enemy player)
                    {
                        unit.CurrentRemainingAdvance = 0;
                        UNIT_DirectDamage(unit); // Deal direct damage!
                    }
                    else // Unit then can advance normally here, perform it
                    {
                        // Request unit advancement a tile
                        ENGINE_UnitTileTransition(unit, unit.TileCoordinate + Lane.GetAdvanceDirection(unitOwnerId));
                        // Entering new tile
                        // TODO: Building damage, building effects
                        unit.CurrentRemainingAdvance--;
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
        void UNIT_Combat(Unit attacker, Unit defender)
        {
            ENGINE_AddMessageEvent($"Combat between P{attacker.Owner + 1}'s {attacker.Name} and P{defender.Owner + 1}'s {defender.Name}");
            ENGINE_UnitDamageChange(defender, UNIT_CalculateDamageTokens(defender, attacker.Attack)); // First, the defender receives damage
            UNIT_VerifyUnitHpChange(defender);
            ENGINE_UnitDamageChange(attacker, UNIT_CalculateDamageTokens(attacker, defender.Attack)); // First, the defender receives damage
            UNIT_VerifyUnitHpChange(attacker);
        }
        /// <summary>
        /// Calculates remaining damage tokens from a damage/heal action. Has to clamp to 0 tokens if overhealed
        /// </summary>
        /// <param name="unit">Unit to calculate</param>
        /// <param name="tokens">Tokens to add/remove</param>
        /// <returns></returns>
        int UNIT_CalculateDamageTokens(Unit unit, int tokens)
        {
            int ret = unit.DamageTokens + tokens;
            return (ret >= 0) ? ret : 0;
        }
        /// <summary>
        /// Unit deals direct damage to opponent
        /// </summary>
        /// <param name="attacker">Who deals the damage</param>
        void UNIT_DirectDamage(Unit attacker)
        {
            ENGINE_AddMessageEvent($"P{attacker.Owner + 1}'s {attacker.Name} deals direct damage to opponent");
            // For now, assume unit always damages opponent player (?????)
            int opponent = 1- attacker.Owner; // Get opponent
            int opponentHp = _detailedState.PlayerStates[opponent].Hp;
            // For now, assume direct damage and no modifiers, in the future, there may be checks here to modify attack stats
            int newOpponentHp = opponentHp - attacker.Attack;
            if (newOpponentHp < 0) newOpponentHp = 0;
            if(opponentHp != newOpponentHp)
            {
                ENGINE_SetPlayerHp(opponent, newOpponentHp);
                STATE_VerifyPlayerHpChange(opponent);
            }
        }
    }
}
