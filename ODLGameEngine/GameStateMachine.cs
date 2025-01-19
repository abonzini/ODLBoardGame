using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODLGameEngine
{
    public class GameStateMachine
    {
        States _currentState = States.START;
        public States getCurrentState() {  return _currentState; } // Used in testing and to determine what to wait for
        GameStateStruct detailedState = new GameStateStruct(); // State info, will work over this to advance game
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
        /// Performs a step of the state, moves the game state forward. Does nothing if machine is awaiting actions instead
        /// </summary>
        /// <returns>The new state action, null if nothing happened</returns>
        public StepResult step()
        {
            switch(_currentState)
            {
                case States.START:
                case States.ACTION_PHASE:
                    return null;
                default:
                    return null;
            }
        }
        /// <summary>
        /// Starts a game from loading a state. Only works in very beginning
        /// </summary>
        /// <param name="initialState">State to load</param>
        public void LoadGame(GameStateStruct initialState)
        {
            if (_currentState != States.START) return; // Only works first thing
            detailedState = initialState; // Overrides state to whatever I wanted
            requestNewState(detailedState.currentState); // Asks to enter new state, will create next step too (new)

        }
        public void StartNewGame(Player p1, Player p2)
        {
            players[0] = p1;
            players[1] = p2;
            requestNewState(States.P1_INIT); // Switches to first actual state
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
            switch (e.eventType)
            {
                case EventType.STATE_TRANSITION:
                    // Requested a transition to new state, which implies ending this step and creating a new one
                    StateTransitionEvent ste = (StateTransitionEvent)e; // Complete info and save "old" step
                    bool firstStep = false;
                    if(currentStep == null)
                    {
                        firstStep = true;
                    }
                    else
                    {
                        ste.oldState = _currentState;
                        currentStep.events.Add(e);
                        stepHistory.Add(currentStep);
                    }
                    _currentState = ste.newState;
                    currentStep = new StepResult();
                    if(firstStep) currentStep.tag = Tag.FIRST_STATE; // Tag it as first if needed (step can't be reverted)
                    // State transition complete!
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
                    StateTransitionEvent ste = (StateTransitionEvent)e;
                    _currentState = ste.oldState; // Just retrieves the prev state
                    break;
                default:
                    throw new NotImplementedException("Not a handled state rn");
            }
        }

        // REQUESTS (WHERE THE MAGIC HAPPENS, API TO BACKEND, GAME THINGS ARE DONE HERE)
        void requestNewState(States state)
        {
            executeEvent(new StateTransitionEvent() { newState = state }); // Execute state transition event
        }
    }
}
