namespace ODLGameEngine
{
    public partial class GameStateMachine // Deals with unit and unit related stuff, maybe advancing
    {
        /// <summary>
        /// Plays the unit as described by the context
        /// </summary>
        /// <param name="player">Player owner</param>
        /// <param name="playContext">Context of playability</param>
        /// <returns></returns>
        public Unit UNIT_PlayUnit(int player, PlayContext playContext)
        {
            // Clone unit, set player
            Unit newSpawnedUnit = (Unit)playContext.Actor.Clone();
            newSpawnedUnit.Owner = player;
            playContext.Actor = newSpawnedUnit;
            // Add to board
            LIVINGENTITY_InitializeEntity(newSpawnedUnit);
            // Places unit in correct coord
            LIVINGENTITY_InsertInTile(newSpawnedUnit, playContext.PlayedTarget);
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            LIVINGENTITY_CheckIfUnitAlive(newSpawnedUnit);
            return newSpawnedUnit;
        }
        /// <summary>
        /// Found unit starts march
        /// </summary>
        /// <param name="unit">Unit object that will march</param>
        void UNIT_UnitMarch(Unit unit)
        {
            int unitOwnerId = unit.Owner;
            int opponentId = 1 - unitOwnerId;
            int cooldown = unit.MvtCooldownTimer;
            if (cooldown > unit.MovementDenominator.Total) // if denominator was reduced, need to make sure unit can advance properly
            {
                cooldown = 0;
            }
            MarchingContext marchCtx = new MarchingContext() // Set context for march-related interactions/triggers
            {
                Actor = unit,
            };
            if (cooldown == 0)
            {
                marchCtx.CurrentMovement = unit.Movement.Total; // How much to advance
                Lane lane = DetailedState.BoardState.GetLaneContainingTile(unit.TileCoordinate); // Lane of march
                // Ready to march
                marchCtx.FirstTileMarch = true;
                while (marchCtx.CurrentMovement > 0) // Advancement loop, will advance until n is 0. This allow external modifiers to halt advance hopefully
                {
                    // About to start march in the current tile
                    TRIGINTER_ProcessTrigger(TriggerType.ON_MARCH, DetailedState.BoardState.Tiles[unit.TileCoordinate], marchCtx); // Trigger
                    if (marchCtx.CurrentMovement <= 0) // Re-check because it may have been altered by a card effect
                    { }
                    else if (DetailedState.BoardState.Tiles[unit.TileCoordinate].GetPlacedEntities(EntityType.UNIT, opponentId).Count > 0) // If enemy unit in tile, will stop march here (and also attack)
                    {
                        marchCtx.CurrentMovement = 0;
                        Unit enemyUnit = (Unit)DetailedState.EntityData[lane.GetPlacedEntities(EntityType.UNIT, opponentId).First()] ?? throw new Exception("There was no enemy unit in this tile after all, discrepancy in internal data!"); // Get first enemy found in the tile
                        UNIT_Combat(unit, enemyUnit); // Let them fight.
                    }
                    else if (lane.IsRelativeEndOfLane(LaneRelativeIndexType.ABSOLUTE, unit.TileCoordinate, unit.Owner)) // Otherwise, if unit in last tile won't advance (and attack enemy player)
                    {
                        marchCtx.CurrentMovement = 0;
                        UNIT_Combat(unit, DetailedState.PlayerStates[opponentId]); // Deal direct damage to enemy!
                    }
                    else // Unit then can advance normally here, perform it
                    {
                        marchCtx.CurrentMovement--;
                        // Current tile
                        int nextTileCoord = lane.GetTileCoordinateConversion(LaneRelativeIndexType.RELATIVE_TO_PLAYER, LaneRelativeIndexType.ABSOLUTE, unit.TileCoordinate, unit.Owner);
                        nextTileCoord++; // Advancement, now its 1 more
                        // Finally get the following lane
                        nextTileCoord = lane.GetTileCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, nextTileCoord, unit.Owner);
                        LIVINGENTITY_InsertInTile(unit, nextTileCoord);
                    }
                    marchCtx.FirstTileMarch = false; // From now on the unit is in the middle of the march
                }
            }
            cooldown++; // Cycle the timer so that next advance it's updated!
            cooldown %= unit.MovementDenominator.Total;
            if (unit.MvtCooldownTimer != cooldown) // If unit has changed cooldown, need to activate this
            {
                ENGINE_UnitMovementCooldownChange(unit, cooldown);
            }
        }
        /// <summary>
        /// Performs combat of units
        /// </summary>
        /// <param name="attacker">Attacking unit</param>
        /// <param name="defender">Defending unit</param>
        void UNIT_Combat(Unit attacker, LivingEntity defender)
        {
            // Create damage contexts
            DamageContext attackerDmgCtx = new DamageContext()
            {
                Actor = attacker,
                Affected = defender,
                DamageAmount = attacker.Attack.Total
            };
            bool defenseDamage = (defender.EntityType == EntityType.UNIT); // Whether there'll be a returned damage
            DamageContext defenderDmgCtx = defenseDamage ? new DamageContext()
            {
                Actor = defender,
                Affected = attacker,
                DamageAmount = ((Unit)defender).Attack.Total
            } : null; // Defender does damage or not, depending
            // Pre damage step
            attackerDmgCtx.ActivatedEntity = attacker;
            TRIGINTER_ProcessInteraction(InteractionType.PRE_DAMAGE, attackerDmgCtx);
            if (defenseDamage)
            {
                defenderDmgCtx.ActivatedEntity = defender;
                TRIGINTER_ProcessInteraction(InteractionType.PRE_DAMAGE, defenderDmgCtx);
            }
            // Actual damage step
            attackerDmgCtx = LIVINGENTITY_DamageStep(attackerDmgCtx);
            if (defenseDamage)
            {
                // If defender was also a unit, then the attacker also receives damage
                defenderDmgCtx = LIVINGENTITY_DamageStep(defenderDmgCtx);
            }
            // Post damage interactions for both entities
            TRIGINTER_ProcessInteraction(InteractionType.POST_DAMAGE, attackerDmgCtx);
            if (defenseDamage)
            {
                TRIGINTER_ProcessInteraction(InteractionType.POST_DAMAGE, defenderDmgCtx);
            }
            // TODO: Trigger death interactions
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
            // Also, when building and unit have different owners, then the unit attacks the building
            if (building.Owner == 1 - unit.Owner)
            {
                // Create damage context
                DamageContext damageCtx = new DamageContext
                {
                    Actor = unit,
                    Affected = building,
                    DamageAmount = 1,
                    ActivatedEntity = unit
                };
                TRIGINTER_ProcessInteraction(InteractionType.PRE_DAMAGE, damageCtx);
                // Actual damage step
                damageCtx = LIVINGENTITY_DamageStep(damageCtx);
                // Post damage interactions for both entities
                TRIGINTER_ProcessInteraction(InteractionType.POST_DAMAGE, damageCtx);
            }
        }
    }
}
