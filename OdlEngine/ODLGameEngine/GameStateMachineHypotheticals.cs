namespace ODLGameEngine
{
    public partial class GameStateMachine // Deals with hypothetical playing and the hypothetical mode (for minmax checkings)
    {
        bool _hypotheticalMode = false; // Whether the state machine is currently in the hypohetical mode or not
        AssortedCardCollection _hypotheticalStoredOpponentHand = null;
        AssortedCardCollection _hypotheticalOpponentsDeck = null;
        int _hypotheticalPlayer;
        /// <summary>
        /// Begins hypothetical mode, by setting the opp hand to wildcards, will override draws, and some other stuff
        /// </summary>
        /// <param name="player">Hypothetical player (knows it's deck to make decisions and stuff)</param>
        /// <param name = "hypotheticalOpponentsDeck" > Hypothetical opponent's deck, which will be modified automatically as the model assumes stuff</param>
        public void StartHypotheticalMode(int player, AssortedCardCollection hypotheticalOpponentsDeck)
        {
            _hypotheticalPlayer = player;
            int opposingPlayer = 1 - player; // This is the opposing player
            _hypotheticalStoredOpponentHand = DetailedState.PlayerStates[opposingPlayer].Hand; // Need to save their hand because I'm about to do some wild shit
            AssortedCardCollection newOpponentHand = new AssortedCardCollection();
            newOpponentHand.AddToCollection(0, _hypotheticalStoredOpponentHand.CardCount); // Replaces opponen'ts whole hand with wildcards
            DetailedState.PlayerStates[opposingPlayer].Hand = newOpponentHand; // Changes opp hand
            _hypotheticalOpponentsDeck = hypotheticalOpponentsDeck;
            HYPOTHETICAL_InitializeOpponentsDeck();
            _hypotheticalMode = true;
            _currentStep.tag = Tag.HYPOTHETICAL; // Sets to hypothetical mode, this is a protection/stopgap for controlled reversion of steps
        }
        /// <summary>
        /// Does a pass to initialise opponent's deck in hypothetical mode
        /// </summary>
        private void HYPOTHETICAL_InitializeOpponentsDeck()
        {
            int opponent = 1 - _hypotheticalPlayer;
            // Information of what opponent doesn't have in deck is for now only on discard pile
            // (To-do, enhance with "known hand" and "known deck" when this is implemented
            foreach (KeyValuePair<int, int> kvp in DetailedState.PlayerStates[opponent].DiscardPile.GetCards())
            {
                _hypotheticalOpponentsDeck.RemoveFromCollection(kvp.Key, kvp.Value); // The assumed deck now doesn't have these cards as they've been played already
            }
        }
        /// <summary>
        /// Ends hypothetical mode, restores game functionality
        /// </summary>
        public void EndHypotheticalMode()
        {
            // First, revert all that happened here, clean event chain in hypothetical mode
            while (_currentStep.tag != Tag.HYPOTHETICAL)
            {
                UndoPreviousStep(); // Reverts until tag is found
            }
            _currentStep.tag = Tag.NO_TAG; // Removes hypothetical tag
            int opposingPlayer = 1 - _hypotheticalPlayer; // This is the opposing player
            DetailedState.PlayerStates[opposingPlayer].Hand = _hypotheticalStoredOpponentHand; // Returns opp hand
            _hypotheticalOpponentsDeck = null;
            _hypotheticalMode = false;
        }
        /// <summary>
        /// Forces a wildcard (card with ID=0) to become the desired one. Will use to guess plays
        /// </summary>
        /// <param name="player">Player who will get the wildcard replaced</param>
        /// <param name="card">Card to replace wildcard with</param>
        public StepResult DiscoverHypotheticalWildcard(int player, int card)
        {
            // This one requires going through game engine
            Player cardOwner = DetailedState.PlayerStates[player];
            ENGINE_HYPOTHETICAL_RevealWildcard(cardOwner, card);
            ENGINE_ChangeState(DetailedState.CurrentState); // Repeat current state to flush event queue
            return _stepHistory.Last(); // Returns everything that happened in this triggering, unneeded but needed to be able to reverse these stuffs
        }
    }
}
