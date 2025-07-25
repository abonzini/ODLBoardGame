using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GameInstance
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActionType
    {
        NOP,
        ACTIVE_POWER,
        GET_TARGETS,
        PLAY_CARD,
        END_TURN
    }
    /// <summary>
    /// Describes a game action, needs a class since a few actions have a few params
    /// </summary>
    public readonly struct GameAction
    {
        public ActionType Type { get; } = ActionType.NOP;
        public int Card { get; } = 0; // Card associated to this action
        public int Target { get; } = -1; // Target associated to this action
        public GameAction(ActionType type = ActionType.NOP, int card = 0, int target = -1)
        {
            Type = type;
            Card = card;
            Target = target;
        }
        public override string ToString()
        {
            return $"{Type}->{Card}->{Target}";
        }
    }

    /// <summary>
    /// It's the entity that controls the game, holds the StateMachine instance, receives commands and returns info.
    /// Notifies owner (if subscribed) when the next new state comes in, publishes hash and can give any data to whoever is asking for it
    /// </summary>
    public class GameManager
    {
    }
}
