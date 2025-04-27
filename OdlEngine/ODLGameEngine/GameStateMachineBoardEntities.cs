using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
            // Finally, need to increment playable counter
            ENGINE_IncrementPlaceableCounter();
        }
        /// <summary>
        /// Inserts entity in new lane
        /// </summary>
        /// <param name="entity">Which entity</param>
        /// <param name="laneId">Which lane</param>
        public void BOARDENTITY_InsertInLane(PlacedEntity entity, LaneID laneId)
        {
            ENGINE_EntityLaneTransition(entity, laneId);
        }
        /// <summary>
        /// Inserts entity in tile RELATIVE TO LATE so Lane always needs to be correct when this is called
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="tileCoord">The tile coord</param>
        public void BOARDENTITY_InsertInTile(PlacedEntity entity, int tileCoord)
        {
            ENGINE_EntityTileTransition(entity, tileCoord);
        }
        /// <summary>
        /// Will check unit HP, and will kill unit if HP <= 0! This kills and deinits unit, very important!
        /// </summary>
        /// <param name="entity">Enity to verify</param>
        /// <returns>True if unit still alive</returns>
        public bool BOARDENTITY_CheckIfUnitAlive(BoardEntity entity)
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
        void BOARDENTITY_CleanUnit(BoardEntity entity)
        {
            ENGINE_AddMessageEvent($"{entity.Name} was destroyed");
            if(entity.EntityPlayInfo.EntityType == EntityType.UNIT || entity.EntityPlayInfo.EntityType == EntityType.BUILDING)
            {
                // Removes unit from its space, first from tile and then from lane!
                BOARDENTITY_InsertInTile((PlacedEntity)entity, -1);
                BOARDENTITY_InsertInLane((PlacedEntity)entity, LaneID.NO_LANE);
                // Moves unit from living space to dead space
                ENGINE_DeinitializeEntity((PlacedEntity)entity);
            }
            else if(entity.EntityPlayInfo.EntityType == EntityType.PLAYER) // Somethign more sinister, this is a game-ending situation, a player just died
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
        DamageContext BOARDENTITY_DamageStep(EntityBase attacker, BoardEntity defender, int damage)
        {
            DamageContext damageCtx = new DamageContext() // Create info of the result of this action
            {
                AttackingEntity = attacker,
                DefendingEntity = defender,
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
