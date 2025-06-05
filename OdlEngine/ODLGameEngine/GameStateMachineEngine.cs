using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public partial class GameStateMachine
    {
        /// <summary>
        /// Executes an event to change game state, adds to current queue and moves state
        /// </summary>
        /// <param name="e">The event to add and excecute</param>
        void ENGINE_ExecuteEvent(GameEngineEvent e)
        {
            int auxInt1, auxInt2;
            Unit auxUnit;
            PlacedEntity auxPlacedEntity;
            LivingEntity auxBoardEntity;
            Player auxPlayerState;
            Stat auxStat;

            _currentStep?.events.Add(e);
            switch (e.eventType)
            {
                case EventType.STATE_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    bool firstStep = false;
                    if (_currentStep == null)
                    {
                        firstStep = true;
                    }
                    else
                    {
                        ((TransitionEvent<States>)e).oldValue = DetailedState.CurrentState;
                        _stepHistory.Add(_currentStep);
                    }
                    DetailedState.CurrentState = ((TransitionEvent<States>)e).newValue;
                    _currentStep = new StepResult();
                    if (firstStep) _currentStep.tag = Tag.FIRST_STATE; // Tag it as first if needed (step can't be reverted)
                    // State transition complete!
                    break;
                case EventType.PLAYER_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    ((TransitionEvent<CurrentPlayer>)e).oldValue = DetailedState.CurrentPlayer;
                    DetailedState.CurrentPlayer = ((TransitionEvent<CurrentPlayer>)e).newValue; // Player transition complete!
                    break;
                case EventType.RNG_TRANSITION:
                    // Requested a transition to new rng seed
                    ((TransitionEvent<int>)e).oldValue = DetailedState.Seed;
                    DetailedState.Seed = ((TransitionEvent<int>)e).newValue; // Player transition complete!
                    _rng = new Random(DetailedState.Seed);
                    break;
                case EventType.PLAYER_GOLD_TRANSITION:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    ((EntityTransitionEvent<int, int>)e).oldValue = DetailedState.PlayerStates[auxInt1].CurrentGold;
                    DetailedState.PlayerStates[auxInt1].CurrentGold = ((EntityTransitionEvent<int,int>)e).newValue;
                    break;
                case EventType.MESSAGE:
                case EventType.DEBUG_CHECK:
                    break;
                case EventType.CARD_DECK_SWAP:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    DetailedState.PlayerStates[auxInt1].Deck.SwapCards(
                        ((EntityTransitionEvent<int, int>)e).newValue,
                        ((EntityTransitionEvent<int, int>)e).oldValue
                        );
                    break;
                case EventType.REMOVE_TOPDECK:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    ((EntityValueEvent<int, int>)e).value = DetailedState.PlayerStates[auxInt1].Deck.PopCard(); // Pop last card from deck
                    break;
                case EventType.ADD_CARD_TO_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value;
                    DetailedState.PlayerStates[auxInt1].Hand.InsertCard(auxInt2);
                    break;
                case EventType.DISCARD_FROM_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value; // Card now popped from hand
                    DetailedState.PlayerStates[auxInt1].Hand.RemoveCard(auxInt2); // Remove from hand...
                    DetailedState.PlayerStates[auxInt1].DiscardPile.InsertCard(auxInt2); // And add to discard pile
                    break;
                case EventType.INIT_ENTITY:
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.EntityListOperation(auxPlacedEntity, EntityListOperation.ADD);
                    DetailedState.EntityData.Add(auxPlacedEntity.UniqueId, auxPlacedEntity);
                    ENGINE_RegisterEntityTriggers(auxPlacedEntity);
                    break;
                case EventType.INCREMENT_PLACEABLE_COUNTER:
                    DetailedState.NextUniqueIndex++;
                    break;
                case EventType.ENTITY_COORD_TRANSITION:
                    auxPlacedEntity = ((EntityTransitionEvent<PlacedEntity, int>)e).entity;
                    ((EntityTransitionEvent<PlacedEntity, int>)e).oldValue = auxPlacedEntity.TileCoordinate; // Store old value first
                    auxInt1 = ((EntityTransitionEvent<PlacedEntity, int>)e).newValue; // The new coord!
                    ENGINE_ChangeEntityCoord(auxPlacedEntity, auxInt1);
                    break;
                case EventType.DEINIT_ENTITY: // Unit simply leaves field and user loses the unit
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.EntityListOperation(auxPlacedEntity, EntityListOperation.REMOVE);
                    DetailedState.EntityData.Remove(auxPlacedEntity.UniqueId);
                    ENGINE_DeregisterEntityTriggers(auxPlacedEntity);
                    break;
                case EventType.UNIT_MOVEMENT_COOLDOWN_VALUE:
                    auxUnit = ((EntityTransitionEvent<Unit, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<Unit, int>)e).newValue;
                    ((EntityTransitionEvent<Unit, int>)e).oldValue = auxUnit.MvtCooldownTimer; // Store old value first
                    auxUnit.MvtCooldownTimer = auxInt1;
                    break;
                case EventType.ENTITY_DAMAGE_COUNTER_CHANGE:
                    auxBoardEntity = ((EntityTransitionEvent<LivingEntity, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<LivingEntity, int>)e).newValue;
                    ((EntityTransitionEvent<LivingEntity, int>)e).oldValue = auxBoardEntity.DamageTokens;
                    auxBoardEntity.DamageTokens = auxInt1;
                    break;
                case EventType.PLAYER_POWER_AVAILABILITY:
                    auxPlayerState = ((EntityTransitionEvent<Player, bool>)e).entity;
                    ((EntityTransitionEvent<Player, bool>)e).oldValue = auxPlayerState.PowerAvailable;
                    auxPlayerState.PowerAvailable = ((EntityTransitionEvent<Player, bool>)e).newValue;
                    break;
                case EventType.STAT_BASE_TRANSITION:
                    auxStat = ((EntityTransitionEvent<Stat, int>)e).entity;
                    ((EntityTransitionEvent<Stat, int>)e).oldValue = auxStat.BaseValue;
                    auxStat.BaseValue = ((EntityTransitionEvent<Stat, int>)e).newValue;
                    break;
                case EventType.STAT_MODIFIER_TRANSITION:
                    auxStat = ((EntityTransitionEvent<Stat, int>)e).entity;
                    ((EntityTransitionEvent<Stat, int>)e).oldValue = auxStat.Modifier;
                    auxStat.Modifier = ((EntityTransitionEvent<Stat, int>)e).newValue;
                    break;
                default:
                    throw new NotImplementedException("Not a handled state rn");
            }
        }
        /// <summary>
        /// Performs the opposite action of an event. Doesn't remove from step! Just opposite
        /// </summary>
        /// <param name="e">Event to revert</param>
        void ENGINE_RevertEvent(GameEngineEvent e)
        {
            int auxInt1, auxInt2;
            Unit auxUnit;
            PlacedEntity auxPlacedEntity;
            LivingEntity auxBoardEntity;
            Player auxPlayerState;
            Stat auxStat;

            switch (e.eventType)
            {
                case EventType.STATE_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    DetailedState.CurrentState = ((TransitionEvent<States>)e).oldValue; // Just retrieves the prev state
                    break;
                case EventType.PLAYER_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    DetailedState.CurrentPlayer = ((TransitionEvent<CurrentPlayer>)e).oldValue; // Player transition complete!
                    break;
                case EventType.RNG_TRANSITION:
                    // Requested a transition to new rng seed
                    DetailedState.Seed = ((TransitionEvent<int>)e).oldValue; // Player transition complete!
                    _rng = new Random(DetailedState.Seed);
                    break;
                case EventType.PLAYER_GOLD_TRANSITION:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    DetailedState.PlayerStates[auxInt1].CurrentGold = ((EntityTransitionEvent<int, int>)e).oldValue;
                    break;
                case EventType.MESSAGE:
                case EventType.DEBUG_CHECK:
                    break;
                case EventType.CARD_DECK_SWAP:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    DetailedState.PlayerStates[auxInt1].Deck.SwapCards(
                        ((EntityTransitionEvent<int, int>)e).newValue,
                        ((EntityTransitionEvent<int, int>)e).oldValue
                        );
                    break;
                case EventType.REMOVE_TOPDECK:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value;
                    DetailedState.PlayerStates[auxInt1].Deck.InsertCard(auxInt2);
                    // Return to deck
                    break;
                case EventType.ADD_CARD_TO_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value;
                    DetailedState.PlayerStates[auxInt1].Hand.RemoveCard(auxInt2);
                    break;
                case EventType.DISCARD_FROM_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value;
                    DetailedState.PlayerStates[auxInt1].DiscardPile.RemoveCard(auxInt2); // Pop from discard pile
                    DetailedState.PlayerStates[auxInt1].Hand.InsertCard(auxInt2); // Reinsert in hand
                    break;
                case EventType.INIT_ENTITY:
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.EntityListOperation(auxPlacedEntity, EntityListOperation.REMOVE);
                    DetailedState.EntityData.Remove(auxPlacedEntity.UniqueId);
                    ENGINE_DeregisterEntityTriggers(auxPlacedEntity);
                    break;
                case EventType.INCREMENT_PLACEABLE_COUNTER:
                    DetailedState.NextUniqueIndex--;
                    break;
                case EventType.ENTITY_COORD_TRANSITION:
                    auxPlacedEntity = ((EntityTransitionEvent<PlacedEntity, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<PlacedEntity, int>)e).oldValue; // The old coord!
                    ENGINE_ChangeEntityCoord(auxPlacedEntity, auxInt1);
                    break;
                case EventType.DEINIT_ENTITY:
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.EntityListOperation(auxPlacedEntity, EntityListOperation.ADD);
                    DetailedState.EntityData.Add(auxPlacedEntity.UniqueId, auxPlacedEntity);
                    ENGINE_RegisterEntityTriggers(auxPlacedEntity);
                    break;
                case EventType.UNIT_MOVEMENT_COOLDOWN_VALUE:
                    auxUnit = ((EntityTransitionEvent<Unit, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<Unit, int>)e).oldValue;
                    auxUnit.MvtCooldownTimer = auxInt1;
                    break;
                case EventType.ENTITY_DAMAGE_COUNTER_CHANGE:
                    auxBoardEntity = ((EntityTransitionEvent<LivingEntity, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<LivingEntity, int>)e).oldValue;
                    auxBoardEntity.DamageTokens = auxInt1;
                    break;
                case EventType.PLAYER_POWER_AVAILABILITY:
                    auxPlayerState = ((EntityTransitionEvent<Player, bool>)e).entity;
                    auxPlayerState.PowerAvailable = ((EntityTransitionEvent<Player, bool>)e).oldValue;
                    break;
                case EventType.STAT_BASE_TRANSITION:
                    auxStat = ((EntityTransitionEvent<Stat, int>)e).entity;
                    auxStat.BaseValue = ((EntityTransitionEvent<Stat, int>)e).oldValue;
                    break;
                case EventType.STAT_MODIFIER_TRANSITION:
                    auxStat = ((EntityTransitionEvent<Stat, int>)e).entity;
                    auxStat.Modifier = ((EntityTransitionEvent<Stat, int>)e).oldValue;
                    break;
                default:
                    throw new NotImplementedException("Not a handled state rn");
            }
        }
        /// <summary>
        /// Registers an entity's trigger
        /// </summary>
        /// <param name="entity">Entity</param>
        void ENGINE_RegisterEntityTriggers(PlacedEntity entity)
        {
            if (entity.Triggers != null) // Check if unit has triggers to process
            {
                foreach (TriggerType trigger in entity.Triggers.Keys) // Register this unit's trigger
                {
                    if (DetailedState.Triggers.TryGetValue(trigger, out SortedSet<int> subscribedEntities)) // If tigger already created, just add to list
                    {
                        subscribedEntities.Add(entity.UniqueId);
                    }
                    else // Otherwise create new list with this unit as trigger
                    {
                        subscribedEntities = new SortedSet<int>();
                        DetailedState.Triggers.Add(trigger, subscribedEntities);
                        subscribedEntities.Add(entity.UniqueId);
                    }
                }
            }
        }
        /// <summary>
        /// Deregisters an entity's trigger (ENGINE HELPER)
        /// </summary>
        /// <param name="entity">Entity</param>
        void ENGINE_DeregisterEntityTriggers(PlacedEntity entity)
        {
            if (entity.Triggers != null) // Check if unit has triggers to process
            {
                foreach (TriggerType trigger in entity.Triggers.Keys) // Deregisters all the triggers present in the unit
                {
                    DetailedState.Triggers[trigger].Remove(entity.UniqueId);
                    if (DetailedState.Triggers[trigger].Count == 0) // Removes trigger so that disctionary can be reverted identically if needed
                    {
                        DetailedState.Triggers.Remove(trigger);
                    }
                }
            }
        }
        /// <summary>
        /// Deals with anything regarding a unit moving to a new coord. Deals with registration too execpt to board ofc
        /// </summary>
        /// <param name="entity">Entity to change coord</param>
        /// <param name="tileCoord">Which coord to put into</param>
        public void ENGINE_ChangeEntityCoord(PlacedEntity entity, int tileCoord)
        {
            if (entity.TileCoordinate != tileCoord) // Otherwise nothing changes
            {
                // Get the corresponding lanes to see if there was a change
                Lane oldLane = DetailedState.BoardState.GetLaneContainingTile(entity.TileCoordinate);
                Lane newLane = DetailedState.BoardState.GetLaneContainingTile(tileCoord);
                // Change tiles
                if (entity.TileCoordinate >= 0) // If the old tile coordinate was valid
                {
                    DetailedState.BoardState.Tiles[entity.TileCoordinate].EntityListOperation(entity, EntityListOperation.REMOVE); // Remove entity from this tile
                }
                entity.TileCoordinate = tileCoord;
                if (entity.TileCoordinate >= 0) // If the new tile coordinate is valid
                {
                    DetailedState.BoardState.Tiles[entity.TileCoordinate].EntityListOperation(entity, EntityListOperation.ADD); // Add entity to tile
                }
                // Change lane (if applies)
                if(oldLane != newLane) // There's a change in lane so i need to deal with it
                {
                    if(oldLane != null)
                    {
                        oldLane.EntityListOperation(entity, EntityListOperation.REMOVE); // Remove unit from old lane
                    }
                    if(newLane != null)
                    {
                        newLane.EntityListOperation(entity, EntityListOperation.ADD); // Add entity to the new lane
                    }
                }
            }
        }
        // --------------------------------------------------------------------------------------
        // -------------------------------  GAME ENGINE REQUESTS --------------------------------
        // --------------------------------------------------------------------------------------
        // Use ENGINE_ in fn names, the more convoluted game mechanics (e.g. when player builds X, or when player draws card) need to be dealt here
        /// <summary>
        /// Advances state machine
        /// </summary>
        /// <param name="state">State to go to</param>
        void ENGINE_ChangeState(States state)
        {
            ENGINE_ExecuteEvent(
                new TransitionEvent<States>()
                {
                    eventType = EventType.STATE_TRANSITION,
                    newValue = state,
                    description = $"Next state: {Enum.GetName(state)}"
                });
        }
        /// <summary>
        /// Toggles active player
        /// </summary>
        void ENGINE_SetNextPlayer(CurrentPlayer nextPlayer)
        {
            ENGINE_ExecuteEvent(
                new TransitionEvent<CurrentPlayer>()
                {
                    eventType = EventType.PLAYER_TRANSITION,
                    newValue = nextPlayer,
                    description = $"Switched to {Enum.GetName(nextPlayer)}"
                });
        }
        /// <summary>
        /// Next step will have a new rng seed, important to mantain determinism
        /// </summary>
        /// <param name="seed">Seed to adopt</param>
        void ENGINE_NewRngSeed(int seed)
        {
            ENGINE_ExecuteEvent(
                new TransitionEvent<int>()
                {
                    eventType = EventType.RNG_TRANSITION,
                    newValue = seed
                });
        }
        /// <summary>
        /// Sets a player gold to new value
        /// </summary>
        /// <param name="p">Which player</param>
        /// <param name="gold">Which value</param>
        void ENGINE_SetPlayerGold(int p, int gold)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<int, int>()
                {
                    eventType = EventType.PLAYER_GOLD_TRANSITION,
                    entity = p,
                    newValue = gold,
                    description = $"P{p + 1} now has {gold} gold"
                });
        }
        /// <summary>
        /// Adds message that can be seen by player
        /// </summary>
        /// <param name="msg">Message</param>
        void ENGINE_AddMessageEvent(string msg)
        {
            ENGINE_ExecuteEvent(
                new GameEngineEvent()
                {
                    eventType = EventType.MESSAGE,
                    description = msg
                });
        }
        /// <summary>
        /// Swaps 2 cards in a player's deck, for shuffling
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="card1">Card 1</param>
        /// <param name="card2">Card 2</param>
        void ENGINE_SwapCardsInDeck(int p, int card1, int card2)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<int, int>()
                {
                    eventType = EventType.CARD_DECK_SWAP,
                    entity = p,
                    newValue = card1,
                    oldValue = card2
                });
        }
        /// <summary>
        /// Draws a single card for a player
        /// </summary>
        /// <param name="player">Player</param>
        void ENGINE_DeckDrawSingle(int player)
        {
            ENGINE_ExecuteEvent(
                new EntityValueEvent<int, int>() // Will store also the card if need to reverse
                {
                    eventType = EventType.REMOVE_TOPDECK,
                    entity = player
                });
        }
        /// <summary>
        /// Adds a (new) card to a player hand in right pile location
        /// </summary>
        /// <param name="player">Which player</param>
        /// <param name="card">Which card</param>
        void ENGINE_AddCardToHand(int player, int card)
        {
            ENGINE_ExecuteEvent(
                new EntityValueEvent<int, int>() // Will store also the card if need to reverse
                {
                    eventType = EventType.ADD_CARD_TO_HAND,
                    value = card,
                    entity = player
                });
        }
        /// <summary>
        /// Player has played a card from hand, so that card is not in hand anymore then
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="cardPlayed">Card</param>
        void ENGINE_DiscardCardFromHand(int p, int cardPlayed)
        {
            ENGINE_ExecuteEvent(
                new EntityValueEvent<int, int>()
                {
                    eventType = EventType.DISCARD_FROM_HAND,
                    entity = p,
                    value = cardPlayed,
                    description = $"P{p + 1} played #{cardPlayed}"
                });
        }
        /// <summary>
        /// Creates unit for first time and applies ownership to player
        /// </summary>
        /// <param name="p">Player who owns the unit</param>
        /// <param name="unit">Unit</param>

        void ENGINE_InitializeEntity(PlacedEntity entity)
        {
            ENGINE_ExecuteEvent(
                new EntityEvent<PlacedEntity>()
                {
                    eventType = EventType.INIT_ENTITY,
                    entity = entity,
                    description = $"P{entity.Owner + 1} now has a {entity.Name}"
                });
        }
        /// <summary>
        /// System increments placeable counter to keep track of units
        /// </summary>
        void ENGINE_IncrementPlaceableCounter()
        {
            ENGINE_ExecuteEvent(
                new GameEngineEvent()
                {
                    eventType = EventType.INCREMENT_PLACEABLE_COUNTER
                });
        }
        /// <summary>
        /// Moves unit to new tile in lane
        /// </summary>
        /// <param name="unit">Which unit (needs to be initialized in a lane)</param>
        /// <param name="lane">Which tile</param>
        void ENGINE_EntityTileTransition(PlacedEntity entity, int tile)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<PlacedEntity, int>()
                {
                    eventType = EventType.ENTITY_COORD_TRANSITION,
                    entity = entity,
                    newValue = tile,
                });
        }
        /// <summary>
        /// Moves a unit from the field (alive) to graveyard (dead)
        /// </summary>
        /// <param name="unit">Unit to be sent to graveyard</param>
        void ENGINE_DeinitializeEntity(PlacedEntity entity)
        {
            ENGINE_ExecuteEvent(
                new EntityEvent<PlacedEntity>()
                {
                    eventType = EventType.DEINIT_ENTITY,
                    entity = entity
                });
        }
        /// <summary>
        /// When a unit changes the movement cooldown, incorporate it here
        /// </summary>
        /// <param name="unit">Unit ID</param>
        /// <param name="cooldown">New cooldown</param>
        void ENGINE_UnitMovementCooldownChange(Unit unit, int cooldown)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<Unit,int>()
                {
                    eventType = EventType.UNIT_MOVEMENT_COOLDOWN_VALUE,
                    entity = unit,
                    newValue = cooldown
                });
        }
        /// <summary>
        /// Change damage tokens of an entity
        /// </summary>
        /// <param name="unit">Unit to damage</param>
        /// <param name="newDamageCounters">How much damage</param>
        void ENGINE_ChangeEntityDamageTokens(LivingEntity entity, int newDamageCounters)
        {
            int delta = newDamageCounters - entity.DamageTokens;
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<LivingEntity, int>()
                {
                    eventType = EventType.ENTITY_DAMAGE_COUNTER_CHANGE,
                    entity = entity,
                    newValue = newDamageCounters,
                    description = $"P{entity.Owner + 1}'s {entity.Name} {((delta > 0) ? "received" : "healed")} {Math.Abs(delta)} damage"
                });
        }
        /// <summary>
        /// Sets the state of player's active power
        /// </summary>
        /// <param name="player">Which player</param>
        /// <param name="powerAvailability">New state</param>
        void ENGINE_ChangePlayerPowerAvailability(Player player, bool  powerAvailability)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<Player, bool>()
                {
                    eventType = EventType.PLAYER_POWER_AVAILABILITY,
                    entity = player,
                    newValue = powerAvailability
                });
        }
        /// <summary>
        /// Adds a debug event to the event pile for testing
        /// </summary>
        /// <param name="ctx">Contains an effect context, useful for extra debug</param>
        void ENGINE_AddDebugEvent(CpuState ctx)
        {
            ENGINE_ExecuteEvent(
                new EntityEvent<CpuState>()
                {
                    eventType = EventType.DEBUG_CHECK,
                    entity = ctx
                });
        }
        /// <summary>
        /// Sets a stat's BaseValue to new value
        /// </summary>
        /// <param name="stat">The stat</param>
        /// <param name="value">The value</param>
        void ENGINE_SetStatBaseValue(Stat stat, int value)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<Stat, int>()
                {
                    eventType = EventType.STAT_BASE_TRANSITION,
                    entity = stat,
                    newValue = value
                });
        }
        /// <summary>
        /// Sets a stat's Modifier to new value
        /// </summary>
        /// <param name="stat">The stat</param>
        /// <param name="value">The value</param>
        void ENGINE_SetStatModifierValue(Stat stat, int value)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<Stat, int>()
                {
                    eventType = EventType.STAT_MODIFIER_TRANSITION,
                    entity = stat,
                    newValue = value
                });
        }
    }
}
