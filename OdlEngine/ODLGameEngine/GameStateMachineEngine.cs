﻿using System;
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
                        ((TransitionEvent<States>)e).oldValue = _detailedState.CurrentState;
                        _stepHistory.Add(_currentStep);
                    }
                    _detailedState.CurrentState = ((TransitionEvent<States>)e).newValue;
                    _currentStep = new StepResult();
                    if (firstStep) _currentStep.tag = Tag.FIRST_STATE; // Tag it as first if needed (step can't be reverted)
                    // State transition complete!
                    break;
                case EventType.PLAYER_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    ((TransitionEvent<CurrentPlayer>)e).oldValue = _detailedState.CurrentPlayer;
                    _detailedState.CurrentPlayer = ((TransitionEvent<CurrentPlayer>)e).newValue; // Player transition complete!
                    break;
                case EventType.RNG_TRANSITION:
                    // Requested a transition to new rng seed
                    ((TransitionEvent<int>)e).oldValue = _detailedState.Seed;
                    _detailedState.Seed = ((TransitionEvent<int>)e).newValue; // Player transition complete!
                    _rng = new Random(_detailedState.Seed);
                    break;
                case EventType.PLAYER_HP_TRANSITION:
                    auxInt1 = ((EntityTransitionEvent<int,int>)e).entity;
                    auxInt2 = ((EntityTransitionEvent<int, int>)e).newValue;
                    ((EntityTransitionEvent<int, int>)e).oldValue = _detailedState.PlayerStates[auxInt1].Hp;
                    _detailedState.PlayerStates[auxInt1].Hp = auxInt2;
                    break;
                case EventType.PLAYER_GOLD_TRANSITION:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    ((EntityTransitionEvent<int, int>)e).oldValue = _detailedState.PlayerStates[auxInt1].Gold;
                    _detailedState.PlayerStates[auxInt1].Gold = ((EntityTransitionEvent<int,int>)e).newValue;
                    break;
                case EventType.MESSAGE:
                    break;
                case EventType.CARD_DECK_SWAP:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    _detailedState.PlayerStates[auxInt1].Deck.SwapCards(
                        ((EntityTransitionEvent<int, int>)e).newValue,
                        ((EntityTransitionEvent<int, int>)e).oldValue
                        );
                    break;
                case EventType.REMOVE_TOPDECK:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    ((EntityValueEvent<int, int>)e).value = _detailedState.PlayerStates[auxInt1].Deck.PopCard(); // Pop last card from deck
                    break;
                case EventType.ADD_CARD_TO_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value;
                    _detailedState.PlayerStates[auxInt1].Hand.InsertCard(auxInt2);
                    break;
                case EventType.PLAYER_GOLD_CHANGE:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    _detailedState.PlayerStates[auxInt1].Gold += ((EntityValueEvent<int, int>)e).value; // Add gold
                    break;
                case EventType.DISCARD_FROM_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value; // Card now popped from hand
                    _detailedState.PlayerStates[auxInt1].Hand.RemoveCard(auxInt2); // Remove from hand...
                    _detailedState.PlayerStates[auxInt1].DiscardPile.InsertCard(auxInt2); // And add to discard pile
                    break;
                case EventType.INIT_UNIT:
                    auxUnit = ((EntityEvent<Unit>)e).entity;
                    _detailedState.BoardState.Units.Add(auxUnit.UniqueId, auxUnit); // Adds unit
                    _detailedState.PlayerStates[auxUnit.Owner].NUnits++;
                    break;
                case EventType.INCREMENT_PLACEABLE_COUNTER:
                    _detailedState.NextUnitIndex++;
                    break;
                case EventType.UNIT_LANE_TRANSITION:
                    auxUnit = ((EntityTransitionEvent<Unit, LaneID>)e).entity;
                    ((EntityTransitionEvent<Unit, LaneID>)e).oldValue = auxUnit.LaneCoordinate; // Store old value first
                    if(auxUnit.LaneCoordinate != LaneID.NO_LANE) // Remove count from old lane if applicable
                    {
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).PlayerUnitCount[auxUnit.Owner]--;
                    }
                    auxUnit.LaneCoordinate = ((EntityTransitionEvent<Unit, LaneID>)e).newValue; // unit now has new value
                    // Finally, update count in lane(s)
                    if (auxUnit.LaneCoordinate != LaneID.NO_LANE) // Adds count to new lane if applicable
                    {
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).PlayerUnitCount[auxUnit.Owner]++;
                    }
                    break;
                case EventType.UNIT_TILE_TRANSITION:
                    auxUnit = ((EntityTransitionEvent<Unit, int>)e).entity;
                    ((EntityTransitionEvent<Unit, int>)e).oldValue = auxUnit.TileCoordinate; // Store old value first
                    if (auxUnit.TileCoordinate >= 0) // Remove count from old tile if applicable
                    {
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).GetTileAbsolute(auxUnit.TileCoordinate).PlayerUnitCount[auxUnit.Owner]--;
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).GetTileAbsolute(auxUnit.TileCoordinate).UnitsInTile.Remove(auxUnit.UniqueId);
                    }
                    auxUnit.TileCoordinate = ((EntityTransitionEvent<Unit, int>)e).newValue; // unit now has new value
                    // Finally, update count in tile
                    if (auxUnit.TileCoordinate >= 0) // Adds count to new tile if applicable
                    {
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).GetTileAbsolute(auxUnit.TileCoordinate).PlayerUnitCount[auxUnit.Owner]++;
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).GetTileAbsolute(auxUnit.TileCoordinate).UnitsInTile.Add(auxUnit.UniqueId);
                    }
                    break;
                case EventType.DEINIT_UNIT: // Unit simply leaves field and user loses the unit
                    auxUnit = ((EntityEvent<Unit>)e).entity;
                    _detailedState.BoardState.Units.Remove(auxUnit.UniqueId);
                    // Now, remove from player's
                    _detailedState.PlayerStates[auxUnit.Owner].NUnits--;
                    break;
                case EventType.UNIT_MOVEMENT_COOLDOWN_VALUE:
                    auxUnit = ((EntityTransitionEvent<Unit, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<Unit, int>)e).newValue;
                    ((EntityTransitionEvent<Unit, int>)e).oldValue = auxUnit.MvtCooldownTimer; // Store old value first
                    auxUnit.MvtCooldownTimer = auxInt1;
                    break;
                case EventType.UNIT_DAMAGE_COUNTER_CHANGE:
                    auxUnit = ((EntityTransitionEvent<Unit, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<Unit, int>)e).newValue;
                    ((EntityTransitionEvent<Unit, int>)e).oldValue = auxUnit.DamageTokens;
                    auxUnit.DamageTokens = auxInt1;
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
            switch (e.eventType)
            {
                case EventType.STATE_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    _detailedState.CurrentState = ((TransitionEvent<States>)e).oldValue; // Just retrieves the prev state
                    break;
                case EventType.PLAYER_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    _detailedState.CurrentPlayer = ((TransitionEvent<CurrentPlayer>)e).oldValue; // Player transition complete!
                    break;
                case EventType.RNG_TRANSITION:
                    // Requested a transition to new rng seed
                    _detailedState.Seed = ((TransitionEvent<int>)e).oldValue; // Player transition complete!
                    _rng = new Random(_detailedState.Seed);
                    break;
                case EventType.PLAYER_HP_TRANSITION:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    _detailedState.PlayerStates[auxInt1].Hp = ((EntityTransitionEvent<int, int>)e).oldValue;
                    break;
                case EventType.PLAYER_GOLD_TRANSITION:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    _detailedState.PlayerStates[auxInt1].Gold = ((EntityTransitionEvent<int, int>)e).oldValue;
                    break;
                case EventType.MESSAGE:
                    break;
                case EventType.CARD_DECK_SWAP:
                    auxInt1 = ((EntityTransitionEvent<int, int>)e).entity;
                    _detailedState.PlayerStates[auxInt1].Deck.SwapCards(
                        ((EntityTransitionEvent<int, int>)e).newValue,
                        ((EntityTransitionEvent<int, int>)e).oldValue
                        );
                    break;
                case EventType.REMOVE_TOPDECK:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value;
                    _detailedState.PlayerStates[auxInt1].Deck.InsertCard(auxInt2);
                    // Return to deck
                    break;
                case EventType.ADD_CARD_TO_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value;
                    _detailedState.PlayerStates[auxInt1].Hand.RemoveCard(auxInt2);
                    break;
                case EventType.PLAYER_GOLD_CHANGE:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    _detailedState.PlayerStates[auxInt1].Gold -= ((EntityValueEvent<int, int>)e).value; // Remove gold
                    break;
                case EventType.DISCARD_FROM_HAND:
                    auxInt1 = ((EntityValueEvent<int, int>)e).entity;
                    auxInt2 = ((EntityValueEvent<int, int>)e).value;
                    _detailedState.PlayerStates[auxInt1].DiscardPile.RemoveCard(auxInt2); // Pop from discard pile
                    _detailedState.PlayerStates[auxInt1].Hand.InsertCard(auxInt2); // Reinsert in hand
                    break;
                case EventType.INIT_UNIT:
                    auxUnit = ((EntityEvent<Unit>)e).entity;
                    _detailedState.BoardState.Units.Remove(auxUnit.UniqueId); // Just removes the unit
                    _detailedState.PlayerStates[auxUnit.Owner].NUnits--; 
                    break;
                case EventType.INCREMENT_PLACEABLE_COUNTER:
                    _detailedState.NextUnitIndex--;
                    break;
                case EventType.UNIT_LANE_TRANSITION:
                    auxUnit = ((EntityTransitionEvent<Unit, LaneID>)e).entity;
                    // Update count in lane(s)
                    if (auxUnit.LaneCoordinate != LaneID.NO_LANE) // Adds count to new lane if applicable
                    {
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).PlayerUnitCount[auxUnit.Owner]--;
                    }
                    auxUnit.LaneCoordinate = ((EntityTransitionEvent<Unit, LaneID>)e).oldValue; // Restore prev value
                    if (auxUnit.LaneCoordinate != LaneID.NO_LANE) // Restore the prev. lane
                    {
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).PlayerUnitCount[auxUnit.Owner]++;
                    }
                    break;
                case EventType.UNIT_TILE_TRANSITION:
                    auxUnit = ((EntityTransitionEvent<Unit, int>)e).entity;
                    // Update count of tile
                    if (auxUnit.TileCoordinate >= 0) // Adds count to new tile if applicable
                    {
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).GetTileAbsolute(auxUnit.TileCoordinate).PlayerUnitCount[auxUnit.Owner]--;
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).GetTileAbsolute(auxUnit.TileCoordinate).UnitsInTile.Remove(auxUnit.UniqueId);
                    }
                    auxUnit.TileCoordinate = ((EntityTransitionEvent<Unit, int>)e).oldValue; // unit now has prev value
                    if (auxUnit.TileCoordinate >= 0) // Update its count if applicable
                    {
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).GetTileAbsolute(auxUnit.TileCoordinate).PlayerUnitCount[auxUnit.Owner]++;
                        _detailedState.BoardState.GetLane(auxUnit.LaneCoordinate).GetTileAbsolute(auxUnit.TileCoordinate).UnitsInTile.Add(auxUnit.UniqueId);
                    }
                    break;
                case EventType.DEINIT_UNIT: // Unit is simply sent from GY to field and user regains the unit (no positioning handled here)
                    auxUnit = ((EntityEvent<Unit>)e).entity;
                    _detailedState.BoardState.Units.Add(auxUnit.UniqueId, auxUnit);
                    _detailedState.PlayerStates[auxUnit.Owner].NUnits++;
                    break;
                case EventType.UNIT_MOVEMENT_COOLDOWN_VALUE:
                    auxUnit = ((EntityTransitionEvent<Unit, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<Unit, int>)e).oldValue;
                    auxUnit.MvtCooldownTimer = auxInt1;
                    break;
                case EventType.UNIT_DAMAGE_COUNTER_CHANGE:
                    auxUnit = ((EntityTransitionEvent<Unit, int>)e).entity;
                    auxInt1 = ((EntityTransitionEvent<Unit, int>)e).oldValue;
                    auxUnit.DamageTokens = auxInt1;
                    break;
                default:
                    throw new NotImplementedException("Not a handled state rn");
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
                    description = $"P{p + 1} played {CardDb.GetCardData(cardPlayed).Name}"
                });
        }
        /// <summary>
        /// Creates unit for first time and applies ownership to player
        /// </summary>
        /// <param name="p">Player who owns the unit</param>
        /// <param name="unit">Unit</param>

        void ENGINE_InitializeUnit(Unit unit)
        {
            ENGINE_ExecuteEvent(
                new EntityEvent<Unit>()
                {
                    eventType = EventType.INIT_UNIT,
                    entity = unit,
                    description = $"P{unit.Owner + 1} now has a {unit.Name}"
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
        void ENGINE_UnitLaneTransition(Unit unit, LaneID lane)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<Unit, LaneID>()
                {
                    eventType = EventType.UNIT_LANE_TRANSITION,
                    entity = unit,
                    newValue = lane,
                });
        }
        /// <summary>
        /// Moves unit to new tile in lane
        /// </summary>
        /// <param name="unit">Which unit (needs to be initialized in a lane)</param>
        /// <param name="lane">Which tile</param>
        void ENGINE_UnitTileTransition(Unit unit, int tile)
        {
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<Unit, int>()
                {
                    eventType = EventType.UNIT_TILE_TRANSITION,
                    entity = unit,
                    newValue = tile,
                });
        }
        /// <summary>
        /// Moves a unit from the field (alive) to graveyard (dead)
        /// </summary>
        /// <param name="unit">Unit to be sent to graveyard</param>
        void ENGINE_DeinitializeUnit(Unit unit)
        {
            ENGINE_ExecuteEvent(
                new EntityEvent<Unit>()
                {
                    eventType = EventType.DEINIT_UNIT,
                    entity = unit
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
        /// Damage a unit, adds damage tokens
        /// </summary>
        /// <param name="unit">Unit to damage</param>
        /// <param name="newDamageCounters">How much damage</param>
        void ENGINE_UnitDamageChange(Unit unit, int newDamageCounters)
        {
            int delta = newDamageCounters - unit.DamageTokens;
            ENGINE_ExecuteEvent(
                new EntityTransitionEvent<Unit, int>()
                {
                    eventType = EventType.UNIT_DAMAGE_COUNTER_CHANGE,
                    entity = unit,
                    newValue = newDamageCounters,
                    description = $"P{unit.Owner + 1}'s {unit.Name} {((delta > 0) ? "received" : "healed")} {Math.Abs(delta)} damage"
                });
        }
    }
}
