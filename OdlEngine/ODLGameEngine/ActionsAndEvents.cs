using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        FIRST_STATE
    }
    public class StepResult
    {
        public Tag tag = Tag.NO_TAG;
        public List<Event> events = new List<Event>(); // Contains list of events
        public override string ToString()
        {
            string ret = "";
            bool first = true;
            foreach (Event e in events)
            {
                if (e.description == "") continue; // Not print if empty
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
        PLAYER_HP_TRANSITION,
        PLAYER_GOLD_TRANSITION,
        MESSAGE,
        CARD_DECK_SWAP,
        REMOVE_TOPDECK,
        ADD_CARD_TO_HAND,
        PLAYER_GOLD_CHANGE,
        DISCARD_FROM_HAND,
        INIT_UNIT,
        INCREMENT_PLACEABLE_COUNTER,
        UNIT_LANE_TRANSITION,
        UNIT_TILE_TRANSITION,
        UNIT_FIELD_TO_GRAVEYARD,
        UNIT_MOVEMENT_COOLDOWN_VALUE
    }

    public class Event
    {
        public EventType eventType;
        public string description = "";
        public override string ToString()
        {
            return description; // Default is no info leaked
        }
    }
    public class EntityValueEvent<E,T> : Event
    {
        public E entity;
        public T value;
    }
    public class TransitionEvent<T> : Event
    {
        public T oldValue;
        public T newValue;
    }
    public class EntityTransitionEvent<E,T> : Event
    {
        public E entity;
        public T oldValue;
        public T newValue;
    }
    public class EntityEvent<E> : Event
    {
        public E entity;
    }
}
