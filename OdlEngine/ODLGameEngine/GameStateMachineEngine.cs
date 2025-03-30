using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class StateMismatchException : Exception
    {
        public StateMismatchException(string msg) : base(msg) { }
    }
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
            BoardEntity auxBoardEntity;
            PlayerState auxPlayerState;
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
                case EventType.PLAYER_HP_TRANSITION:
                    auxInt1 = ((EntityTransitionEvent<int,int>)e).entity;
                    auxInt2 = ((EntityTransitionEvent<int, int>)e).newValue;
                    ((EntityTransitionEvent<int, int>)e).oldValue = DetailedState.PlayerStates[auxInt1].Hp;
                    DetailedState.PlayerStates[auxInt1].Hp = auxInt2;
                    break;
                case EventType.PLAYER_GOLD_TRANSITION:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    ((EntityTransitionEvent<int, int>)e).oldValue = DetailedState.PlayerStates[auxInt1].Gold;
                    DetailedState.PlayerStates[auxInt1].Gold = ((EntityTransitionEvent<int,int>)e).newValue;
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
                case EventType.PLAYER_GOLD_CHANGE:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    DetailedState.PlayerStates[auxInt1].Gold += ((EntityValueEvent<int, int>)e).value; // Add gold
                    break;
                case EventType.DISCARD_FROM_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value; // Card now popped from hand
                    DetailedState.PlayerStates[auxInt1].Hand.RemoveCard(auxInt2); // Remove from hand...
                    DetailedState.PlayerStates[auxInt1].DiscardPile.InsertCard(auxInt2); // And add to discard pile
                    break;
                case EventType.INIT_ENTITY:
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.InsertEntity(auxPlacedEntity);
                    DetailedState.BoardState.Entities.Add(auxPlacedEntity.UniqueId, auxPlacedEntity);
                    ENGINE_RegisterEntityTriggers(auxPlacedEntity);
                    break;
                case EventType.INCREMENT_PLACEABLE_COUNTER:
                    DetailedState.NextUniqueIndex++;
                    break;
                case EventType.ENTITY_LANE_TRANSITION:
                    auxPlacedEntity = ((EntityTransitionEvent<PlacedEntity, LaneID>)e).entity;
                    ((EntityTransitionEvent<PlacedEntity, LaneID>)e).oldValue = auxPlacedEntity.LaneCoordinate; // Store old value first
                    if(auxPlacedEntity.LaneCoordinate != LaneID.NO_LANE) // Remove count from old lane if applicable
                    {
                        DetailedState.BoardState.GetLane(auxPlacedEntity.LaneCoordinate).RemoveEntity(auxPlacedEntity);
                    }
                    auxPlacedEntity.LaneCoordinate = ((EntityTransitionEvent<PlacedEntity, LaneID>)e).newValue; // unit now has new value
                    // Finally, update count in lane(s)
                    if (auxPlacedEntity.LaneCoordinate != LaneID.NO_LANE) // Adds count to new lane if applicable
                    {
                        DetailedState.BoardState.GetLane(auxPlacedEntity.LaneCoordinate).InsertEntity(auxPlacedEntity);
                    }
                    break;
                case EventType.ENTITY_TILE_TRANSITION:
                    auxPlacedEntity = ((EntityTransitionEvent<PlacedEntity, int>)e).entity;
                    ((EntityTransitionEvent<PlacedEntity, int>)e).oldValue = auxPlacedEntity.TileCoordinate; // Store old value first
                    if (auxPlacedEntity.TileCoordinate >= 0) // Remove count from old tile if applicable
                    {
                        DetailedState.BoardState.GetLane(auxPlacedEntity.LaneCoordinate).GetTileAbsolute(auxPlacedEntity.TileCoordinate).RemoveEntity(auxPlacedEntity);
                    }
                    auxPlacedEntity.TileCoordinate = ((EntityTransitionEvent<PlacedEntity, int>)e).newValue; // unit now has new value
                    // Finally, update count in tile
                    if (auxPlacedEntity.TileCoordinate >= 0) // Adds count to new tile if applicable
                    {
                        DetailedState.BoardState.GetLane(auxPlacedEntity.LaneCoordinate).GetTileAbsolute(auxPlacedEntity.TileCoordinate).InsertEntity(auxPlacedEntity);
                    }
                    break;
                case EventType.DEINIT_ENTITY: // Unit simply leaves field and user loses the unit
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.RemoveEntity(auxPlacedEntity);
                    DetailedState.BoardState.Entities.Remove(auxPlacedEntity.UniqueId);
                    ENGINE_DeregisterEntityTriggers(auxPlacedEntity);
                    break;
                case EventType.UNIT_MOVEMENT_COOLDOWN_VALUE:
                    auxUnit = ((EntityTransitionEvent<Unit, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<Unit, int>)e).newValue;
                    ((EntityTransitionEvent<Unit, int>)e).oldValue = auxUnit.MvtCooldownTimer; // Store old value first
                    auxUnit.MvtCooldownTimer = auxInt1;
                    break;
                case EventType.ENTITY_DAMAGE_COUNTER_CHANGE:
                    auxBoardEntity = ((EntityTransitionEvent<BoardEntity, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<BoardEntity, int>)e).newValue;
                    ((EntityTransitionEvent<BoardEntity, int>)e).oldValue = auxBoardEntity.DamageTokens;
                    auxBoardEntity.DamageTokens = auxInt1;
                    break;
                case EventType.PLAYER_POWER_AVAILABILITY:
                    auxPlayerState = ((EntityTransitionEvent<PlayerState, bool>)e).entity;
                    ((EntityTransitionEvent<PlayerState, bool>)e).oldValue = auxPlayerState.PowerAvailable;
                    auxPlayerState.PowerAvailable = ((EntityTransitionEvent<PlayerState, bool>)e).newValue;
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
            BoardEntity auxBoardEntity;
            PlayerState auxPlayerState;
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
                case EventType.PLAYER_HP_TRANSITION:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    DetailedState.PlayerStates[auxInt1].Hp = ((EntityTransitionEvent<int, int>)e).oldValue;
                    break;
                case EventType.PLAYER_GOLD_TRANSITION:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    DetailedState.PlayerStates[auxInt1].Gold = ((EntityTransitionEvent<int, int>)e).oldValue;
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
                case EventType.PLAYER_GOLD_CHANGE:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    DetailedState.PlayerStates[auxInt1].Gold -= ((EntityValueEvent<int, int>)e).value; // Remove gold
                    break;
                case EventType.DISCARD_FROM_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value;
                    DetailedState.PlayerStates[auxInt1].DiscardPile.RemoveCard(auxInt2); // Pop from discard pile
                    DetailedState.PlayerStates[auxInt1].Hand.InsertCard(auxInt2); // Reinsert in hand
                    break;
                case EventType.INIT_ENTITY:
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.RemoveEntity(auxPlacedEntity);
                    DetailedState.BoardState.Entities.Remove(auxPlacedEntity.UniqueId);
                    ENGINE_DeregisterEntityTriggers(auxPlacedEntity);
                    break;
                case EventType.INCREMENT_PLACEABLE_COUNTER:
                    DetailedState.NextUniqueIndex--;
                    break;
                case EventType.ENTITY_LANE_TRANSITION:
                    auxPlacedEntity = ((EntityTransitionEvent<PlacedEntity, LaneID>)e).entity;
                    if (auxPlacedEntity.LaneCoordinate != LaneID.NO_LANE) // Remove count from old lane if applicable
                    {
                        DetailedState.BoardState.GetLane(auxPlacedEntity.LaneCoordinate).RemoveEntity(auxPlacedEntity);
                    }
                    auxPlacedEntity.LaneCoordinate = ((EntityTransitionEvent<PlacedEntity, LaneID>)e).oldValue; // unit now has prev value
                    // Finally, update count in lane(s)
                    if (auxPlacedEntity.LaneCoordinate != LaneID.NO_LANE) // Adds count to new lane if applicable
                    {
                        DetailedState.BoardState.GetLane(auxPlacedEntity.LaneCoordinate).InsertEntity(auxPlacedEntity);
                    }
                    break;
                case EventType.ENTITY_TILE_TRANSITION:
                    auxPlacedEntity = ((EntityTransitionEvent<PlacedEntity, int>)e).entity;
                    // Update count of tile
                    if (auxPlacedEntity.TileCoordinate >= 0) // Adds count to new tile if applicable
                    {
                        DetailedState.BoardState.GetLane(auxPlacedEntity.LaneCoordinate).GetTileAbsolute(auxPlacedEntity.TileCoordinate).RemoveEntity(auxPlacedEntity);
                    }
                    auxPlacedEntity.TileCoordinate = ((EntityTransitionEvent<PlacedEntity, int>)e).oldValue; // unit now has prev value
                    if (auxPlacedEntity.TileCoordinate >= 0) // Update its count if applicable
                    {
                        DetailedState.BoardState.GetLane(auxPlacedEntity.LaneCoordinate).GetTileAbsolute(auxPlacedEntity.TileCoordinate).InsertEntity(auxPlacedEntity);
                    }
                    break;
                case EventType.DEINIT_ENTITY:
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.InsertEntity(auxPlacedEntity);
                    DetailedState.BoardState.Entities.Add(auxPlacedEntity.UniqueId, auxPlacedEntity);
                    ENGINE_RegisterEntityTriggers(auxPlacedEntity);
                    break;
                case EventType.UNIT_MOVEMENT_COOLDOWN_VALUE:
                    auxUnit = ((EntityTransitionEvent<Unit, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<Unit, int>)e).oldValue;
                    auxUnit.MvtCooldownTimer = auxInt1;
                    break;
                case EventType.ENTITY_DAMAGE_COUNTER_CHANGE:
                    auxBoardEntity = ((EntityTransitionEvent<BoardEntity, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<BoardEntity, int>)e).oldValue;
                    auxBoardEntity.DamageTokens = auxInt1;
                    break;
                case EventType.PLAYER_POWER_AVAILABILITY:
                    auxPlayerState = ((EntityTransitionEvent<PlayerState, bool>)e).entity;
                    auxPlayerState.PowerAvailable = ((EntityTransitionEvent<PlayerState, bool>)e).oldValue;
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
                    SortedSet<int> subscribedEntities;
                    if (DetailedState.Triggers.TryGetValue(trigger, out subscribedEntities)) // If tigger already created, just add to list
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
        /// Sets a player HP to new value
        /// </summary>
        /// <param name="p">Which player</param>
        /// <param name="hp">Which value</param>
        void ENGINE_SetPlayerHp(int p, int hp)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<int, int>()
                {
                    eventType = EventType.PLAYER_HP_TRANSITION,
                    entity = p,
                    newValue = hp,
                    description = $"P{p + 1} now has {hp} HP"
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
        /// Change the gold of player (gain or loss)
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="goldDelta">How much gold to gain/lose</param>
        void ENGINE_PlayerGoldChange(int p, int goldDelta)
        {
            if (goldDelta == 0) return; // No need to do anything if there's no change...
            ENGINE_ExecuteEvent(
                new EntityValueEvent<int, int>()
                {
                    eventType = EventType.PLAYER_GOLD_CHANGE,
                    entity = p,
                    value = goldDelta,
                    description = $"P{p + 1} {((goldDelta > 0) ? "gains" : "loses")} {Math.Abs(goldDelta)} gold"
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
                    description = $"P{p + 1} played {CardDb.GetCard(cardPlayed).EntityPrintInfo.Title}"
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
        /// Moves unit to new lane
        /// </summary>
        /// <param name="unit">Which unit (needs to be initialized)</param>
        /// <param name="lane">Which lane</param>
        void ENGINE_EntityLaneTransition(PlacedEntity entity, LaneID laneId)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<PlacedEntity, LaneID>()
                {
                    eventType = EventType.ENTITY_LANE_TRANSITION,
                    entity = entity,
                    newValue = laneId,
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
                    eventType = EventType.ENTITY_TILE_TRANSITION,
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
        void ENGINE_ChangeEntityDamageTokens(BoardEntity entity, int newDamageCounters)
        {
            int delta = newDamageCounters - entity.DamageTokens;
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<BoardEntity, int>()
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
        void ENGINE_ChangePlayerPowerAvailability(PlayerState player, bool  powerAvailability)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<PlayerState, bool>()
                {
                    eventType = EventType.PLAYER_POWER_AVAILABILITY,
                    entity = player,
                    newValue = powerAvailability
                });
        }
        void ENGINE_AddDebugEvent()
        {
            ENGINE_ExecuteEvent(
                    new GameEngineEvent()
                    {
                        eventType = EventType.DEBUG_CHECK,
                    });
        }
    }
}
