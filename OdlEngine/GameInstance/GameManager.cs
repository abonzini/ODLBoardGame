﻿using Newtonsoft.Json;
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
    public class GameAction
    {
        public ActionType Type { get; set; } = ActionType.NOP;
        public int Card { get; set; } = 0; // Card associated to this action
        public int Target { get; set; } = -1; // Target associated to this action
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
