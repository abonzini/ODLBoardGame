namespace ODLGameEngine
{
    public partial class GameStateMachine // General handler of all placeable stuff, isntantiation, setting of coordinate, also check health (including player!)
    {
        /// <summary>
        /// Registers brand new entity in board
        /// </summary>
        /// <param name="entity">Entity to register</param>
        public void LIVINGENTITY_InitializeEntity(PlacedEntity entity)
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
        /// Inserts entity in tile
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="tileCoord">The tile coord</param>
        public void LIVINGENTITY_InsertInTile(PlacedEntity entity, int tileCoord)
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
        /// <param name="cleanUpDeadEntity">Entity is cleaned if hp=0 (true unless more stuff has to happen beforehand, like death effects or more damage.</param>
        /// <returns>True if unit still alive</returns>
        public bool LIVINGENTITY_CheckIfUnitAlive(LivingEntity entity, bool cleanUpDeadEntity = true)
        {
            if (entity.Hp.Total - entity.DamageTokens <= 0) // Entity is dead, will process death and return accordingly
            {
                if (cleanUpDeadEntity)
                {
                    LIVINGENTITY_Kill(entity);
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// New wrapper to kill an entity. Allows to call this for instakill effects and some extra triggers
        /// </summary>
        /// <param name="entity"></param>
        void LIVINGENTITY_Kill(LivingEntity entity)
        {
            LIVINGENTITY_CleanUnit(entity);
        }
        /// <summary>
        /// Unit needs to be killed for whatever reason, this process executes the action
        /// </summary>
        /// <param name="unitId">Which unit</param>
        void LIVINGENTITY_CleanUnit(LivingEntity entity)
        {
            if (entity.EntityType == EntityType.UNIT || entity.EntityType == EntityType.BUILDING)
            {
                LIVINGENTITY_InsertInTile((PlacedEntity)entity, -1); // Removes unit from its tile
                ENGINE_DeinitializeEntity((PlacedEntity)entity); // Deinits entity
            }
            else if (entity.EntityType == EntityType.PLAYER) // Somethign more sinister, this is a game-ending situation, a player just died
            {
                throw new EndOfGameException($"{entity.Name} dead", 1 - entity.Owner); // Other player wins!
            }
            else
            {
                // Nothing to do
            }
        }
        /// <summary>
        /// An attacker damages a defender, damage is dealt a fixed amount.
        /// THIS ONLY RESOLVES DAMAGE, INTERACTION WRAPPING SHOULD BE OUTSIDE OF THIS
        /// </summary>
        /// <param name="damageContext">Context that contains all info about current combat</param>
        /// <param name="victimCleanup">Cleans up victim if dead</param>
        /// <returns>The modified damage context (not a new instance!)</returns>
        DamageContext LIVINGENTITY_DamageStep(DamageContext damageContext, bool victimCleanup = true)
        {
            // Pre damage
            damageContext.ActivatedEntity = damageContext.Actor;
            TRIGINTER_ProcessInteraction(InteractionType.PRE_DAMAGE, damageContext); // Pre-damage call
            if (damageContext.Actor.UniqueId != damageContext.Affected.UniqueId) // If the victim is a different entity
            {
                damageContext.ActivatedEntity = damageContext.Affected;
                TRIGINTER_ProcessInteraction(InteractionType.PRE_DAMAGE, damageContext);
            }
            // Now, the damage is processed and done
            LivingEntity victim = damageContext.Affected;
            int damage = damageContext.DamageAmount;
            int remainingHp = victim.Hp.Total - victim.DamageTokens;
            if (damage > remainingHp)
            {
                damageContext.OverflowDamage = damage - remainingHp;
                damageContext.DamageAmount -= damageContext.OverflowDamage;
                remainingHp = 0;
            }
            else
            {
                remainingHp -= damage;
            }
            ENGINE_ChangeEntityDamageTokens(victim, victim.Hp.Total - remainingHp);
            damageContext.TargetDead = !LIVINGENTITY_CheckIfUnitAlive(victim, victimCleanup);
            // Post damage
            damageContext.ActivatedEntity = damageContext.Actor;
            TRIGINTER_ProcessInteraction(InteractionType.POST_DAMAGE, damageContext); // Pre-damage call
            if (damageContext.Actor.UniqueId != damageContext.Affected.UniqueId) // If the victim is a different entity
            {
                damageContext.ActivatedEntity = damageContext.Affected;
                TRIGINTER_ProcessInteraction(InteractionType.POST_DAMAGE, damageContext);
            }
            return damageContext;
        }
    }
}
