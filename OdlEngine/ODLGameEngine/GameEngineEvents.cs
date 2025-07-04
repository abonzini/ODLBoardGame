namespace ODLGameEngine
{
    /// <summary>
    /// Available tags for step
    /// </summary>
    public enum Tag
    {
        NO_TAG,
        /// <summary>
        /// First state, can't rewind
        /// </summary>
        FIRST_STATE,
        /// <summary>
        /// Hypothetical state, also can't rewind, changes some behaviours
        /// </summary>
        HYPOTHETICAL
    }
    public class StepResult
    {
        public Tag tag = Tag.NO_TAG;
        public List<GameEngineEvent> events = new List<GameEngineEvent>(); // Contains list of events
        public override string ToString()
        {
            string ret = "";
            bool first = true;
            foreach (GameEngineEvent e in events)
            {
                ret += "\n";
                if (first)
                {
                    ret += ">";
                    first = false;
                }
                ret += "\t" + e.ToString();
            }
            return ret;
        }
    }
    /// <summary>
    /// The things the game "can do", will be large
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Transitions the state machine to next state
        /// </summary>
        STATE_TRANSITION,
        PLAYER_TRANSITION,
        RNG_TRANSITION,
        PLAYER_GOLD_TRANSITION,
        CARD_DECK_SWAP,
        REMOVE_TOPDECK,
        ADD_CARD_TO_HAND,
        DISCARD_FROM_HAND,
        INIT_ENTITY,
        INCREMENT_PLACEABLE_COUNTER,
        ENTITY_COORD_TRANSITION,
        DEINIT_ENTITY,
        UNIT_MOVEMENT_COOLDOWN_VALUE,
        ENTITY_DAMAGE_COUNTER_CHANGE,
        PLAYER_POWER_AVAILABILITY,
        DEBUG_EVENT,
        STAT_BASE_TRANSITION,
        STAT_MODIFIER_TRANSITION,
        TRIGGER_SUBSCRIBE,
        TRIGGER_UNSUBSCRIBE,
        HYPOTHETICAL_DECK_CHANGE_AMOUNT,
        HYPOTHETICAL_REVEAL_WILDCARD
    }

    public class GameEngineEvent
    {
        public EventType eventType;
        public override string ToString()
        {
            return eventType.ToString(); // Default is no info leaked
        }
    }
    public class EntityValueEvent<E, T> : GameEngineEvent
    {
        public E entity;
        public T value;
    }
    public class TransitionEvent<T> : GameEngineEvent
    {
        public T oldValue;
        public T newValue;
    }
    public class EntityTransitionEvent<E, T> : GameEngineEvent
    {
        public E entity;
        public T oldValue;
        public T newValue;
    }
    public class EntityEvent<E> : GameEngineEvent
    {
        public E entity;
    }
}
