namespace ODLGameEngine
{
    public partial class GameStateMachine // General handler of all placeable stuff, isntantiation, setting of coordinate, also check health (including player!)
    {
        /// <summary>
        /// Registers brand new entity in board
        /// </summary>
        /// <param name="entity">Entity to register</param>
        public void BOARDENTITY_InitializeEntity(PlacedEntity entity)
        {
            int unitId = DetailedState.NextUniqueIndex;
            entity.UniqueId = unitId;
            // Unit ready to be added
            ENGINE_InitializeEntity(entity); // Now entity is attached to game
            // Register the absolute triggers of entity
            TRIGINTER_RegisterEntityAbsoluteTriggers(entity);
            // Finally, need to increment playable counter
            ENGINE_IncrementPlaceableCounter();
        }
        /// <summary>
        /// Inserts entity in tile RELATIVE TO LANE so Lane always needs to be correct when this is called
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="tileCoord">The tile coord</param>
        public void BOARDENTITY_InsertInTile(PlacedEntity entity, int tileCoord)
        {
            ENGINE_EntityTileTransition(entity, tileCoord);
            if (entity.TileCoordinate > -1) // Checks if unit entered a nev (valid) tile
            {
                if (entity.EntityType == EntityType.UNIT) // In case of units, there may be building interactions
                {
                    Unit unit = (Unit)entity;
                    SortedSet<int> buildingsInUnitTile = DetailedState.BoardState.Tiles[unit.TileCoordinate].GetPlacedEntities(EntityType.BUILDING); // Look for building in my tile
                    if (buildingsInUnitTile.Count > 0) // Found a building, means unit has stepped on it
                    {
                        UNIT_EnterBuilding(unit, (Building)DetailedState.EntityData[buildingsInUnitTile.First()]);
                    }
                }
            }
        }
        /// <summary>
        /// Will check unit HP, and will kill unit if HP <= 0! This kills and deinits unit, very important!
        /// </summary>
        /// <param name="entity">Enity to verify</param>
        /// <returns>True if unit still alive</returns>
        public bool BOARDENTITY_CheckIfUnitAlive(LivingEntity entity)
        {
            if (entity.Hp.Total - entity.DamageTokens <= 0) // Entity is dead, will process death and return accordingly
            {
                BOARDENTITY_CleanUnit(entity);
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Unit needs to be killed for whatever reason, this process executes the action
        /// </summary>
        /// <param name="unitId">Which unit</param>
        void BOARDENTITY_CleanUnit(LivingEntity entity)
        {
            if (entity.EntityType == EntityType.UNIT || entity.EntityType == EntityType.BUILDING)
            {
                BOARDENTITY_InsertInTile((PlacedEntity)entity, -1); // Removes unit from its tile
                ENGINE_DeinitializeEntity((PlacedEntity)entity); // Deinits entity
            }
            else if (entity.EntityType == EntityType.PLAYER) // Somethign more sinister, this is a game-ending situation, a player just died
            {
                throw new EndOfGameException($"{entity.Name} dead by HP", 1 - entity.Owner); // Other player wins!
            }
            else
            {
                // Nothing to do
            }
        }
        /// <summary>
        /// An attacker damages a defender, damage is dealt a fixed amount.
        /// THIS FUNCTION SHOULD NEVER TRIGGER ANY INTERACTION OR TRIGGERS!!!!
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <param name="damage"></param>
        /// <returns>Description of damage & outcome for processing</returns>
        DamageContext BOARDENTITY_DamageStep(IngameEntity attacker, LivingEntity defender, int damage)
        {
            DamageContext damageCtx = new DamageContext() // Create info of the result of this action
            {
                Actor = attacker,
                Affected = defender,
                DamageAmount = damage
            };

            int remainingHp = defender.Hp.Total - defender.DamageTokens;
            if (damage > remainingHp)
            {
                damageCtx.OverflowDamage = damage - remainingHp;
                damageCtx.DamageAmount -= damageCtx.OverflowDamage;
                remainingHp = 0;
            }
            else
            {
                remainingHp -= damage;
            }

            ENGINE_ChangeEntityDamageTokens(defender, defender.Hp.Total - remainingHp);
            damageCtx.TargetDead = !BOARDENTITY_CheckIfUnitAlive(defender);
            return damageCtx;
        }
    }
}
