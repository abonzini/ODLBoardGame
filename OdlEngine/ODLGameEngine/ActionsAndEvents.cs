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
        DECK_DRAW
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
    public class TransitionEvent<T> : Event
    {
        public T oldValue;
        public T newValue;
    }
    public class PlayerValueEvent<T> : Event
    {
        public PlayerId playerId;
        public T oldValue;
        public T newValue;
    }
    public class PlayerEvent : Event
    {
        public PlayerId playerId;
    }
}
