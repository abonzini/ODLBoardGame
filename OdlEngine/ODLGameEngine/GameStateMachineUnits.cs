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
        /// <returns>The generated unit</returns>
        Unit UNIT_PlayUnit(int player, Unit unit, TargetLocation chosenTarget)
        {
            // To spawn a unit, first you get the playable ID
            // Clone the unit, add to board
            // "Change" the coordinate of unit to right place
            Unit newSpawnedUnit = (Unit)unit.Clone(); // Clone in order to not break the same species
            if(newSpawnedUnit.Name == "") { newSpawnedUnit.Name = newSpawnedUnit.EntityPrintInfo.Title; }
            newSpawnedUnit.Owner = player;
            BOARDENTITY_InitializeEntity(newSpawnedUnit);
            // Locates unit to right place. Get the lane where unit is played, and place it in first tile
            Lane unitLane = DetailedState.BoardState.GetLane(chosenTarget);
            BOARDENTITY_InsertInLane(newSpawnedUnit, unitLane.Id);
            int tileCoord = unitLane.GetAbsoluteTileCoord(0,player); // Get tile coord
            BOARDENTITY_InsertInTile(newSpawnedUnit, tileCoord);
            // Now, verify if unit has just entered a tile where there's a building
            SortedSet<int> buildingsInUnitTile = unitLane.GetTileAbsolute(tileCoord).GetPlacedEntities(EntityType.BUILDING);
            if (buildingsInUnitTile.Count > 0) // Found a building, means unit has stepped on it
            {
                UNIT_EnterBuilding(newSpawnedUnit, (Building)DetailedState.EntityData[buildingsInUnitTile.First()]);
            }
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            BOARDENTITY_CheckIfUnitAlive(newSpawnedUnit);
            return newSpawnedUnit;
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
            if(cooldown > unit.MovementDenominator.Total) // if denominator was reduced, need to make sure unit can advance properly
            {
                cooldown = 0;
            }
            AdvancingContext advanceCtx = new AdvancingContext() // Set context for advance-related interactions/triggers
            {
                Actor = unit,
            };
            if (cooldown == 0)
            {
                ENGINE_AddMessageEvent($"P{unitOwnerId + 1}'s {unit.Name} advances");
                advanceCtx.InitialMovement = unit.Movement.Total; // How much to advance
                Lane lane = DetailedState.BoardState.GetLane(unit.LaneCoordinate); // Which lane
                // Ready to advance!
                advanceCtx.CurrentMovement = advanceCtx.InitialMovement;
                while (advanceCtx.CurrentMovement > 0) // Advancement loop, will advance until n is 0. This allow external modifiers to halt advance hopefully
                {
                    // Exiting current tile
                    if (lane.GetTileAbsolute(unit.TileCoordinate).GetPlacedEntities(EntityType.UNIT, opponentId).Count > 0) // If enemy unit in tile, will stop advance here (and also attack)
                    {
                        advanceCtx.CurrentMovement = 0;
                        Unit enemyUnit = (Unit)DetailedState.EntityData[lane.GetPlacedEntities(EntityType.UNIT,opponentId).First()] ?? throw new Exception("There was no enemy unit in this tile after all, discrepancy in internal data!"); // Get first enemy found in the tile
                        UNIT_Combat(unit, enemyUnit); // Let them fight.
                    }
                    else if (lane.GetAbsoluteTileCoord(-1, unitOwnerId) == unit.TileCoordinate) // Otherwise, if unit in last tile won't advance (and attack enemy player)
                    {
                        advanceCtx.CurrentMovement = 0;
                        UNIT_Combat(unit, DetailedState.PlayerStates[opponentId]); // Deal direct damage to enemy!
                    }
                    else // Unit then can advance normally here, perform it
                    {
                        advanceCtx.CurrentMovement--;
                        // Request unit advancement a tile
                        BOARDENTITY_InsertInTile(unit, unit.TileCoordinate + Lane.GetAdvanceDirection(unitOwnerId));
                        // Entering new tile
                        Tile newTile = DetailedState.BoardState.GetLane(unit.LaneCoordinate).GetTileAbsolute(unit.TileCoordinate);
                        if (newTile.GetPlacedEntities(EntityType.BUILDING).Count != 0) // If tile has a building, do potential building effects and/or combat
                        {
                            Building bldg = (Building)DetailedState.EntityData[newTile.GetPlacedEntities(EntityType.BUILDING).First()]; // Get building
                            UNIT_EnterBuilding(unit, bldg);
                        }
                    }
                }
            }
            cooldown++; // Cycle the timer so that next advance it's updated!
            cooldown %= unit.MovementDenominator.Total;
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
            attackerDmgCtx = BOARDENTITY_DamageStep(attacker, defender, attacker.Attack.Total); // TODO: GetAttack fn to incorporate buffs and such
            if(defender is Unit defendingUnit)
            {
                // If defender was also a unit, then the attacker also receives damage
                defenderDmgCtx = BOARDENTITY_DamageStep(defender, attacker, defendingUnit.Attack.Total);
            }

            // TODO: Contexts are checked here! even death and damage taking, to avoid Units doing stuff in this critical damage step
        }
        /// <summary>
        /// The event that occurs when a unit steps on a building
        /// </summary>
        /// <param name="unit">The unit</param>
        /// <param name="building">The building</param>
        void UNIT_EnterBuilding(Unit unit, Building building)
        {
            // First, unit enters building
            EntersBuildingContext enterCtx = new EntersBuildingContext()
            {
                ActivatedEntity = unit,
                Actor = unit,
                Affected = building
            };
            TRIGINTER_ProcessInteraction(InteractionType.UNIT_ENTERS_BUILDING, enterCtx);
            // Then, building is entered, context from POV of building
            enterCtx = new EntersBuildingContext()
            {
                ActivatedEntity = building,
                Actor = unit,
                Affected = building
            };
            TRIGINTER_ProcessInteraction(InteractionType.UNIT_ENTERS_BUILDING, enterCtx);
            // Also, when 
            if (building.Owner == 1 - unit.Owner) // If building and unit have different owners, then the unit attacks
            {
                UNIT_Combat(unit, building);
            }
        }
    }
}
