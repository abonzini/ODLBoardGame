using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class GameStateMachine
    {
        GameStateStruct _detailedState = null; // State info, will work over this to advance game
        public GameStateStruct getDetailedState() { return _detailedState; }
        CardFinder _cardDb = null;
        CardFinder cardDb
        {
            get
            {
                if (_cardDb == null)
                {
                    _cardDb = new CardFinder(".\\..\\..\\..\\..\\CardDatabase"); // Shouldn't happen unless testing!! Wonder if this path is always ok
                }
                return _cardDb;
            }
            set
            {
                _cardDb = value;
            }
        }
        Player[] players = [null, null]; // Both players, this should be never null for a new game
        List<StepResult> stepHistory = new List<StepResult>();
        StepResult currentStep = null;

        /// <summary>
        /// Initializes an empty game state, and will create a random seed unless overwritten later
        /// </summary>
        public GameStateMachine()
        {
            _detailedState = new GameStateStruct();
            int seed = (int)DateTime.Now.Ticks;
            Random seedGen = new Random(seed);
            _detailedState.seed = seed = seedGen.Next(int.MinValue, int.MaxValue);
        }

        // --------------------------------------------------------------------------------------
        // ------------------------  STATE, ACTIONS AND STEP OPERATORS --------------------------
        // --------------------------------------------------------------------------------------

        /// <summary>
        /// Performs a step of the state, moves the game state forward. Does nothing if machine is awaiting actions instead
        /// </summary>
        /// <returns>The new state action, null if nothing happened</returns>
        public StepResult step()
        {
            switch(_detailedState.currentState)
            {
                case States.START:
                case States.ACTION_PHASE:
                    return null;
                case States.P1_INIT:
                    InitializePlayer(PlayerId.PLAYER_1);
                    requestNewState(States.P2_INIT);
                    break;
                case States.P2_INIT:
                    InitializePlayer(PlayerId.PLAYER_2);
                    togglePlayer(); // Init finished, now begin game w P1 active
                    requestNewState(States.DRAW_PHASE);
                    break;
                default:
                    throw new NotImplementedException("State not yet implemented");
            }
            return stepHistory.Last();
        }
        /// <summary>
        /// Starts a game from loading a state. Only works in very beginning
        /// </summary>
        /// <param name="initialState">State to load</param>
        public void LoadGame(GameStateStruct initialState)
        {
            if (_detailedState.currentState != States.START) return; // Only works first thing
            _detailedState = initialState; // Overrides state to whatever I wanted
            requestNewState(_detailedState.currentState); // Asks to enter new state, will create next step too (new)

        }
        public void StartNewGame(Player p1, Player p2)
        {
            players[0] = p1;
            players[1] = p2;
            requestNewState(States.P1_INIT); // Switches to first actual state
        }

        public void InitializePlayer(PlayerId player) // This function randomizes! Needs to restore seed after!
        {
            int playerId;
            switch (player)
            {
                case PlayerId.PLAYER_1:
                    playerId = 0;
                    break;
                case PlayerId.PLAYER_2:
                    playerId = 1;
                    break;
                default:
                    throw new InvalidOperationException("Can only be used when intiializing player!");
            }
        }

        /// <summary>
        /// Goes back to beggining of previous step (i.e. undoes the last thing that happened)
        /// </summary>
        void undoPreviousStep()
        {
            if (currentStep == null || currentStep.tag == Tag.FIRST_STATE) { return; } // Nothing to do here
            if(currentStep.events.Count != 0) { throw new Exception("Standing in a non-empty current event!"); } // This should never happen

            currentStep = stepHistory.Last();
            stepHistory.RemoveAt(stepHistory.Count - 1); // Removes last step from history!
            for(int i = currentStep.events.Count - 1; i >= 0; i--) // Pops events in reverse order, one by one
            {
                revertEvent(currentStep.events[i]); // Revert the event
            }
            currentStep.events.Clear(); // Clear list as all events have been reverted
        }
        /// <summary>
        /// Executes an event to change game state, adds to current queue and moves state
        /// </summary>
        /// <param name="e">The event to add and excecute</param>
        void executeEvent(Event e)
        {
            if(currentStep != null) // Add event to history
            {
                currentStep.events.Add(e);
            }
            switch (e.eventType)
            {
                case EventType.STATE_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    TransitionEvent<States> ste = (TransitionEvent<States>)e; // Complete info and save "old" step
                    bool firstStep = false;
                    if(currentStep == null)
                    {
                        firstStep = true;
                    }
                    else
                    {
                        ste.oldValue = _detailedState.currentState;
                        stepHistory.Add(currentStep);
                    }
                    _detailedState.currentState = ste.newValue;
                    currentStep = new StepResult();
                    if(firstStep) currentStep.tag = Tag.FIRST_STATE; // Tag it as first if needed (step can't be reverted)
                    // State transition complete!
                    break;
                case EventType.PLAYER_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    TransitionEvent<PlayerId> pte = (TransitionEvent<PlayerId>)e; // Complete info and save "old" step
                    _detailedState.currentPlayer = pte.newValue; // Player transition complete!
                    break;
                default:
                    throw new NotImplementedException("Not a handled state rn");
            }
        }
        /// <summary>
        /// Performs the opposite action of an event. Doesn't remove from step! Just opposite
        /// </summary>
        /// <param name="e">Event to revert</param>
        void revertEvent(Event e)
        {
            switch (e.eventType)
            {
                case EventType.STATE_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    TransitionEvent<States> ste = (TransitionEvent<States>)e;
                    _detailedState.currentState = ste.oldValue; // Just retrieves the prev state
                    break;
                case EventType.PLAYER_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    TransitionEvent<PlayerId> pte = (TransitionEvent<PlayerId>)e; // Complete info and save "old" step
                    _detailedState.currentPlayer = pte.oldValue; // Player transition complete!
                    break;
                default:
                    throw new NotImplementedException("Not a handled state rn");
            }
        }

        // --------------------------------------------------------------------------------------
        // -------------------------------  GAME ENGINE REQUESTS --------------------------------
        // --------------------------------------------------------------------------------------
        void requestNewState(States state)
        {
            executeEvent(
                new TransitionEvent<States>()
                {
                    eventType = EventType.STATE_TRANSITION,
                    newValue = state,
                    description = $"Next state: {Enum.GetName(state)}"
                }); // Execute state transition event
        }
        void togglePlayer()
        {
            PlayerId nextPlayer;
            switch(_detailedState.currentPlayer) // Player is always 1 unless it goes from 1 -> 2
            {
                case PlayerId.PLAYER_1:
                    nextPlayer = PlayerId.PLAYER_2;
                    break;
                default:
                    nextPlayer = PlayerId.PLAYER_1;
                    break;
            }
            executeEvent(
                new TransitionEvent<PlayerId>()
                {
                    eventType = EventType.PLAYER_TRANSITION,
                    newValue = nextPlayer,
                    description = $"Switched to {Enum.GetName(nextPlayer)}"
                }); // Execute player transition event
        }
    }
}
