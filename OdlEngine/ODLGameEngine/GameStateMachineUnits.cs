namespace ODLGameEngine
{
    public partial class GameStateMachine // Deals with unit and unit related stuff, maybe advancing
    {
        /// <summary>
        /// Obtains a Unit play context which describes where a unit can be placed
        /// </summary>
        /// <param name="player">Reference player owner</param>
        /// <param name="unit">Unit to summon</param>
        /// <param name="laneTarget">Lane to place it</param>
        /// <returns>Context with info of where it's allowed to play unit</returns>
        public UnitPlayContext UNIT_GetUnitPlayData(int player, Unit unit, PlayTargetLocation laneTarget)
        {
            // TODO: This can be extremely complex depending on future effects, for now, unit is literally placed on the first tile of lane
            UnitPlayContext res = new UnitPlayContext
            {
                Actor = unit,
                AbsoluteInitialTile = DetailedState.BoardState.GetLane(laneTarget).GetCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, 0, player)
            };
            return res;
        }
        /// <summary>
        /// Plays the unit as described by the context
        /// </summary>
        /// <param name="player">Player owner</param>
        /// <param name="playContext">Context of playability</param>
        /// <returns></returns>
        Unit UNIT_PlayUnit(int player, UnitPlayContext playContext)
        {
            // Clone unit, set player
            Unit newSpawnedUnit = (Unit)playContext.Actor.Clone();
            newSpawnedUnit.Owner = player;
            playContext.Actor = newSpawnedUnit;
            // Add to board
            BOARDENTITY_InitializeEntity(newSpawnedUnit);
            // Places unit in correct coord
            BOARDENTITY_InsertInTile(newSpawnedUnit, playContext.AbsoluteInitialTile);
            // In case unit has 0 hp or is hit by something, need to check by the end to make sure
            BOARDENTITY_CheckIfUnitAlive(newSpawnedUnit);
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
                    // About to start march in the current tile, begin march check
                    // TODO SEND TRIGGER HERE
                    if (DetailedState.BoardState.Tiles[unit.TileCoordinate].GetPlacedEntities(EntityType.UNIT, opponentId).Count > 0) // If enemy unit in tile, will stop march here (and also attack)
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
                        int nextTileCoord = lane.GetCoordinateConversion(LaneRelativeIndexType.RELATIVE_TO_PLAYER, LaneRelativeIndexType.ABSOLUTE, unit.TileCoordinate, unit.Owner);
                        nextTileCoord++; // Advancement, now its 1 more
                        // Finally get the following lane
                        nextTileCoord = lane.GetCoordinateConversion(LaneRelativeIndexType.ABSOLUTE, LaneRelativeIndexType.RELATIVE_TO_PLAYER, nextTileCoord, unit.Owner);
                        BOARDENTITY_InsertInTile(unit, nextTileCoord);
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
            DamageContext attackerDmgCtx, defenderDmgCtx;

            // Surely, unit will apply damage to the victim
            attackerDmgCtx = BOARDENTITY_DamageStep(attacker, defender, attacker.Attack.Total); // TODO: GetAttack fn to incorporate buffs and such
            if (defender is Unit defendingUnit)
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
