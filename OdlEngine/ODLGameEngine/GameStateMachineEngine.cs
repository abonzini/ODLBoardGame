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
                    DetailedState.PlayerStates[auxInt1].CurrentGold = ((EntityTransitionEvent<int, int>)e).newValue;
                    break;
                case EventType.DEBUG_EVENT:
                    break;
                case EventType.CARD_DECK_SWAP:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    DetailedState.PlayerStates[auxInt1].Deck.SwapCards(
                        ((EntityTransitionEvent<int, int>)e).newValue,
                        ((EntityTransitionEvent<int, int>)e).oldValue
                        );
                    break;
                case EventType.REMOVE_TOPDECK:
                    auxPlayerState = ((EntityValueEvent<Player, int>)e).entity;
                    ((EntityValueEvent<Player, int>)e).value = auxPlayerState.Deck.PopCard(); // Pop last card from deck
                    break;
                case EventType.ADD_CARD_TO_HAND:
                    auxPlayerState = ((EntityValueEvent<Player, int>)e).entity;
                    auxInt1 = ((EntityValueEvent<Player, int>)e).value;
                    auxPlayerState.Hand.AddToCollection(auxInt1);
                    break;
                case EventType.DISCARD_FROM_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value; // Card now popped from hand
                    DetailedState.PlayerStates[auxInt1].Hand.RemoveFromCollection(auxInt2); // Remove from hand...
                    DetailedState.PlayerStates[auxInt1].DiscardPile.AddToCollection(auxInt2); // And add to discard pile
                    break;
                case EventType.INIT_ENTITY:
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.EntityListOperation(auxPlacedEntity, BoardElementListOperation.ADD);
                    DetailedState.EntityData.Add(auxPlacedEntity.UniqueId, auxPlacedEntity);
                    break;
                case EventType.INCREMENT_PLACEABLE_COUNTER:
                    DetailedState.NextUniqueIndex++;
                    break;
                case EventType.ENTITY_COORD_TRANSITION:
                    auxPlacedEntity = ((EntityTransitionEvent<PlacedEntity, int>)e).entity;
                    ((EntityTransitionEvent<PlacedEntity, int>)e).oldValue = auxPlacedEntity.TileCoordinate; // Store old value first
                    auxInt1 = ((EntityTransitionEvent<PlacedEntity, int>)e).newValue; // The new coord!
                    ChangeEntityCoord(auxPlacedEntity, auxInt1);
                    break;
                case EventType.DEINIT_ENTITY: // Unit simply leaves field and user loses the unit
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.EntityListOperation(auxPlacedEntity, BoardElementListOperation.REMOVE);
                    DetailedState.EntityData.Remove(auxPlacedEntity.UniqueId);
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
                case EventType.TRIGGER_SUBSCRIBE:
                    {
                        EntityValueEvent<BoardElement, TrigInfoHelper> auxTriggerEvent = (EntityValueEvent<BoardElement, TrigInfoHelper>)e;
                        SubscribeTrigger(auxTriggerEvent.entity, auxTriggerEvent.value);
                    }
                    break;
                case EventType.TRIGGER_UNSUBSCRIBE:
                    {
                        EntityValueEvent<BoardElement, TrigInfoHelper> auxTriggerEvent = (EntityValueEvent<BoardElement, TrigInfoHelper>)e;
                        UnsubscribeTrigger(auxTriggerEvent.entity, auxTriggerEvent.value);
                    }
                    break;
                case EventType.HYPOTHETICAL_DECK_CHANGE_AMOUNT:
                    auxPlayerState = ((EntityValueEvent<Player, int>)e).entity;
                    auxInt1 = ((EntityValueEvent<Player, int>)e).value;
                    auxPlayerState.Deck.HYPOTHETICAL_ChangeCount(auxInt1);
                    break;
                case EventType.HYPOTHETICAL_REVEAL_WILDCARD:
                    auxPlayerState = ((EntityValueEvent<Player, int>)e).entity;
                    auxInt1 = ((EntityValueEvent<Player, int>)e).value;
                    auxPlayerState.Hand.RemoveFromCollection(0);
                    auxPlayerState.Hand.AddToCollection(auxInt1);
                    _hypotheticalOpponentsDeck.RemoveFromCollection(auxInt1); // One less card in hypothetical deck too
                    break;
                case EventType.HYPOTHETICAL_SET_WILDCARD_RELEVANCE:
                    auxInt1 = ((EntityTransitionEvent<int, bool>)e).entity;
                    ((EntityTransitionEvent<int, bool>)e).oldValue = _hasRelevantWildcards[auxInt1];
                    _hasRelevantWildcards[auxInt1] = ((EntityTransitionEvent<int, bool>)e).newValue;
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
                case EventType.DEBUG_EVENT:
                    break;
                case EventType.CARD_DECK_SWAP:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    DetailedState.PlayerStates[auxInt1].Deck.SwapCards(
                        ((EntityTransitionEvent<int, int>)e).newValue,
                        ((EntityTransitionEvent<int, int>)e).oldValue
                        );
                    break;
                case EventType.REMOVE_TOPDECK:
                    auxPlayerState = ((EntityValueEvent<Player, int>)e).entity;
                    auxInt1 = ((EntityValueEvent<Player, int>)e).value;
                    auxPlayerState.Deck.InsertCard(auxInt1);
                    // Return to deck
                    break;
                case EventType.ADD_CARD_TO_HAND:
                    auxPlayerState = ((EntityValueEvent<Player, int>)e).entity;
                    auxInt1 = ((EntityValueEvent<Player, int>)e).value;
                    auxPlayerState.Hand.RemoveFromCollection(auxInt1);
                    break;
                case EventType.DISCARD_FROM_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value;
                    DetailedState.PlayerStates[auxInt1].DiscardPile.RemoveFromCollection(auxInt2); // Pop from discard pile
                    DetailedState.PlayerStates[auxInt1].Hand.AddToCollection(auxInt2); // Reinsert in hand
                    break;
                case EventType.INIT_ENTITY:
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.EntityListOperation(auxPlacedEntity, BoardElementListOperation.REMOVE);
                    DetailedState.EntityData.Remove(auxPlacedEntity.UniqueId);
                    break;
                case EventType.INCREMENT_PLACEABLE_COUNTER:
                    DetailedState.NextUniqueIndex--;
                    break;
                case EventType.ENTITY_COORD_TRANSITION:
                    auxPlacedEntity = ((EntityTransitionEvent<PlacedEntity, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<PlacedEntity, int>)e).oldValue; // The old coord!
                    ChangeEntityCoord(auxPlacedEntity, auxInt1);
                    break;
                case EventType.DEINIT_ENTITY:
                    auxPlacedEntity = ((EntityEvent<PlacedEntity>)e).entity;
                    DetailedState.BoardState.EntityListOperation(auxPlacedEntity, BoardElementListOperation.ADD);
                    DetailedState.EntityData.Add(auxPlacedEntity.UniqueId, auxPlacedEntity);
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
                case EventType.TRIGGER_SUBSCRIBE:
                    {
                        EntityValueEvent<BoardElement, TrigInfoHelper> auxTriggerEvent = (EntityValueEvent<BoardElement, TrigInfoHelper>)e;
                        UnsubscribeTrigger(auxTriggerEvent.entity, auxTriggerEvent.value);
                    }
                    break;
                case EventType.TRIGGER_UNSUBSCRIBE:
                    {
                        EntityValueEvent<BoardElement, TrigInfoHelper> auxTriggerEvent = (EntityValueEvent<BoardElement, TrigInfoHelper>)e;
                        SubscribeTrigger(auxTriggerEvent.entity, auxTriggerEvent.value);
                    }
                    break;
                case EventType.HYPOTHETICAL_DECK_CHANGE_AMOUNT:
                    auxPlayerState = ((EntityValueEvent<Player, int>)e).entity;
                    auxInt1 = ((EntityValueEvent<Player, int>)e).value;
                    auxPlayerState.Deck.HYPOTHETICAL_ChangeCount(-auxInt1);
                    break;
                case EventType.HYPOTHETICAL_REVEAL_WILDCARD:
                    auxPlayerState = ((EntityValueEvent<Player, int>)e).entity;
                    auxInt1 = ((EntityValueEvent<Player, int>)e).value;
                    auxPlayerState.Hand.RemoveFromCollection(auxInt1);
                    auxPlayerState.Hand.AddToCollection(0);
                    _hypotheticalOpponentsDeck.AddToCollection(auxInt1); // One less card in hypothetical deck too
                    break;
                case EventType.HYPOTHETICAL_SET_WILDCARD_RELEVANCE:
                    auxInt1 = ((EntityTransitionEvent<int, bool>)e).entity;
                    _hasRelevantWildcards[auxInt1] = ((EntityTransitionEvent<int, bool>)e).oldValue;
                    break;
                default:
                    throw new NotImplementedException("Not a handled state rn");
            }
        }
        /// <summary>
        /// Deals with anything regarding a unit moving to a new coord. Deals with registration too execpt to board ofc. PRIVATE
        /// </summary>
        /// <param name="entity">Entity to change coord</param>
        /// <param name="tileCoord">Which coord to put into</param>
        public void ChangeEntityCoord(PlacedEntity entity, int tileCoord)
        {
            if (entity.TileCoordinate != tileCoord) // Otherwise nothing changes
            {
                // Get the corresponding lanes to see if there was a change
                Lane oldLane = DetailedState.BoardState.GetLaneContainingTile(entity.TileCoordinate);
                Lane newLane = DetailedState.BoardState.GetLaneContainingTile(tileCoord);
                // Change tiles
                if (entity.TileCoordinate >= 0) // If the old tile coordinate was valid
                {
                    DetailedState.BoardState.Tiles[entity.TileCoordinate].EntityListOperation(entity, BoardElementListOperation.REMOVE); // Remove entity from this tile
                    // Check if entity had relative triggers for the current tile
                    if (entity.Triggers != null && entity.Triggers.TryGetValue(EffectLocation.CURRENT_TILE, out Dictionary<TriggerType, List<Effect>> relativeTriggers))
                    {
                        foreach (TriggerType triggerType in relativeTriggers.Keys) // Remove all triggers
                        {
                            DetailedState.BoardState.Tiles[entity.TileCoordinate].TriggerListOperation(triggerType, entity.UniqueId, EffectLocation.CURRENT_TILE, BoardElementListOperation.REMOVE);
                        }
                    }
                }
                entity.TileCoordinate = tileCoord;
                if (entity.TileCoordinate >= 0) // If the new tile coordinate is valid
                {
                    DetailedState.BoardState.Tiles[entity.TileCoordinate].EntityListOperation(entity, BoardElementListOperation.ADD); // Add entity to tile
                    // Check if entity has relative triggers for the current tile
                    if (entity.Triggers != null && entity.Triggers.TryGetValue(EffectLocation.CURRENT_TILE, out Dictionary<TriggerType, List<Effect>> relativeTriggers))
                    {
                        foreach (TriggerType triggerType in relativeTriggers.Keys) // Remove all triggers
                        {
                            DetailedState.BoardState.Tiles[entity.TileCoordinate].TriggerListOperation(triggerType, entity.UniqueId, EffectLocation.CURRENT_TILE, BoardElementListOperation.ADD);
                        }
                    }
                }
                // Change lane (if applies)
                if (oldLane != newLane) // There's a change in lane so i need to deal with it
                {
                    oldLane?.EntityListOperation(entity, BoardElementListOperation.REMOVE); // Remove unit from old lane
                    newLane?.EntityListOperation(entity, BoardElementListOperation.ADD); // Add entity to the new lane
                }
            }
        }
        /// <summary>
        /// Subscribes a trigger to a place
        /// </summary>
        /// <param name="place">Place</param>
        /// <param name="trigInfo">TrigInfo</param>
        static public void SubscribeTrigger(BoardElement place, TrigInfoHelper trigInfo)
        {
            place.TriggerListOperation(trigInfo.Trigger, trigInfo.EntityId, trigInfo.RelativeLocation, BoardElementListOperation.ADD);
        }
        /// <summary>
        /// Unsubscribes a trigger from a place
        /// </summary>
        /// <param name="place">Place</param>
        /// <param name="trigInfo">TrigInfo</param>
        static public void UnsubscribeTrigger(BoardElement place, TrigInfoHelper trigInfo)
        {
            place.TriggerListOperation(trigInfo.Trigger, trigInfo.EntityId, trigInfo.RelativeLocation, BoardElementListOperation.REMOVE);
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
                    newValue = state
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
                    newValue = nextPlayer
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
                    newValue = gold
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
        void ENGINE_DeckDrawSingle(Player player)
        {
            ENGINE_ExecuteEvent(
                new EntityValueEvent<Player, int>() // Will store also the card if need to reverse
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
        void ENGINE_AddCardToHand(Player player, int card)
        {
            ENGINE_ExecuteEvent(
                new EntityValueEvent<Player, int>() // Will store also the card if need to reverse
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
                    value = cardPlayed
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
                new EntityTransitionEvent<Unit, int>()
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
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<LivingEntity, int>()
                {
                    eventType = EventType.ENTITY_DAMAGE_COUNTER_CHANGE,
                    entity = entity,
                    newValue = newDamageCounters,
                });
        }
        /// <summary>
        /// Sets the state of player's active power
        /// </summary>
        /// <param name="player">Which player</param>
        /// <param name="powerAvailability">New state</param>
        void ENGINE_ChangePlayerPowerAvailability(Player player, bool powerAvailability)
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
                    eventType = EventType.DEBUG_EVENT,
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
        public class TrigInfoHelper
        {
            public TriggerType Trigger;
            public int EntityId;
            public EffectLocation RelativeLocation;
        }
        /// <summary>
        /// Subscribes a trigger to the corresponding board element
        /// </summary>
        /// <param name="boardElement">BoardElement to subscribe</param>
        /// <param name="trigger">Trigger</param>
        /// <param name="entityId">Entity id</param>
        /// <param name="relativeLocation">Rel location of trigger</param>
        void ENGINE_SubscribeTrigger(BoardElement boardElement, TriggerType trigger, int entityId, EffectLocation relativeLocation)
        {
            TrigInfoHelper trigInfoHelper = new TrigInfoHelper()
            {
                Trigger = trigger,
                EntityId = entityId,
                RelativeLocation = relativeLocation
            };
            ENGINE_ExecuteEvent(
                new EntityValueEvent<BoardElement, TrigInfoHelper>()
                {
                    eventType = EventType.TRIGGER_SUBSCRIBE,
                    entity = boardElement,
                    value = trigInfoHelper
                });
        }
        /// <summary>
        /// Unsubscribes a trigger to the corresponding board element
        /// </summary>
        /// <param name="boardElement">BoardElement to subscribe</param>
        /// <param name="trigger">Trigger</param>
        /// <param name="entityId">Entity id</param>
        /// <param name="relativeLocation">Rel location of trigger</param>
        void ENGINE_UnsubscribeTrigger(BoardElement boardElement, TriggerType trigger, int entityId, EffectLocation relativeLocation)
        {
            TrigInfoHelper trigInfoHelper = new TrigInfoHelper()
            {
                Trigger = trigger,
                EntityId = entityId,
                RelativeLocation = relativeLocation
            };
            ENGINE_ExecuteEvent(
                new EntityValueEvent<BoardElement, TrigInfoHelper>()
                {
                    eventType = EventType.TRIGGER_UNSUBSCRIBE,
                    entity = boardElement,
                    value = trigInfoHelper
                });
        }
        /// <summary>
        /// Alters the player's deck count by a specific amount
        /// </summary>
        /// <param name="player">Deck owner</param>
        /// <param name="delta">How much to change deck by</param>
        void ENGINE_HYPOTHETICAL_AlterDeckAmount(Player player, int delta)
        {
            ENGINE_ExecuteEvent(
                new EntityValueEvent<Player, int>()
                {
                    eventType = EventType.HYPOTHETICAL_DECK_CHANGE_AMOUNT,
                    entity = player,
                    value = delta
                });
        }
        /// <summary>
        /// A player reveals a wildcard
        /// </summary>
        /// <param name="player">Player to reveal wildcard</param>
        /// <param name="card">Card to reveal</param>
        void ENGINE_HYPOTHETICAL_RevealWildcard(Player player, int card)
        {
            ENGINE_ExecuteEvent(
                new EntityValueEvent<Player, int>()
                {
                    eventType = EventType.HYPOTHETICAL_REVEAL_WILDCARD,
                    entity = player,
                    value = card
                });
        }
        /// <summary>
        /// Sets whether a player's wildcards are relevant
        /// </summary>
        /// <param name="player">Which player</param>
        /// <param name="relevance">Whether relevant or not</param>
        void ENGINE_HYPOTHETICAL_SetWildcardRelevance(int player, bool relevance)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<int, bool>()
                {
                    eventType = EventType.HYPOTHETICAL_SET_WILDCARD_RELEVANCE,
                    entity = player,
                    newValue = relevance
                });
        }
    }
}
