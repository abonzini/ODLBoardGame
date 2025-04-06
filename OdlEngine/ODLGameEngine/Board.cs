using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ODLGameEngine
{
    public abstract class BoardElement : IHashable
    {
        public SortedSet<int>[] PlayerUnits { get; set; } = [new SortedSet<int>(), new SortedSet<int>()];
        public SortedSet<int> AllUnits { get; set; } = new SortedSet<int>();
        public SortedSet<int>[] PlayerBuildings { get; set; } = [new SortedSet<int>(), new SortedSet<int>()];
        public SortedSet<int> AllBuildings { get; set; } = new SortedSet<int>();
        public SortedSet<int>[] PlayerEntities { get; set; } = [new SortedSet<int>(), new SortedSet<int>()];
        public SortedSet<int> AllEntities { get; set; } = new SortedSet<int>();
        public void InsertEntity(PlacedEntity entity)
        {
            PlayerEntities[entity.Owner].Add(entity.UniqueId);
            AllEntities.Add(entity.UniqueId);
            switch (entity.EntityPlayInfo.EntityType)
            {
                case EntityType.UNIT:
                    AllUnits.Add(entity.UniqueId);
                    PlayerUnits[entity.Owner].Add(entity.UniqueId);
                    break;
                case EntityType.BUILDING:
                    AllBuildings.Add(entity.UniqueId);
                    PlayerBuildings[entity.Owner].Add(entity.UniqueId);
                    break;
                default:
                    throw new Exception("Board element can only contain placed entities!");
            }
        }
        public void RemoveEntity(PlacedEntity entity)
        {
            PlayerEntities[entity.Owner].Remove(entity.UniqueId);
            AllEntities.Remove(entity.UniqueId);
            switch (entity.EntityPlayInfo.EntityType)
            {
                case EntityType.UNIT:
                    AllUnits.Remove(entity.UniqueId);
                    PlayerUnits[entity.Owner].Remove(entity.UniqueId);
                    break;
                case EntityType.BUILDING:
                    AllBuildings.Remove(entity.UniqueId);
                    PlayerBuildings[entity.Owner].Remove(entity.UniqueId);
                    break;
                default:
                    throw new Exception("Board element can only contain placed entities!");
            }
        }
        public abstract int GetGameStateHash();
    }
    public class Tile : BoardElement
    {
        public override int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            foreach(int unit in AllUnits)
            {
                hash.Add(unit);
            }
            foreach(int building in AllBuildings)
            {
                hash.Add(building);
            }
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"P1: {PlayerUnits[0].Count} P2: {PlayerUnits[1].Count} B: {PlayerBuildings[0].Count+ PlayerBuildings[1].Count})";
        }
    }
    /// <summary>
    /// Lanes
    /// </summary>
    public enum LaneID
    {
        NO_LANE,
        PLAINS,
        FOREST,
        MOUNTAIN
    }
    public class Lane : BoardElement /// Player 0 goes from 0 -> N-1 and vice versa. Absolute truth is always w.r.t. player 0
    {
        public LaneID Id {get; set;} = LaneID.NO_LANE;
        public int Len { get; set; } = 0;
        public List<Tile> Tiles { get; set; }
        public Lane(LaneID id, int n)
        {
            Id = id;
            Len = n;
            Tiles = new List<Tile>(n);
            for(int i = 0; i < Len; i++)
            {
                Tiles.Add(new Tile());
            }
        }
        /// <summary>
        /// Gets the tile instance given the index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The actual instance of a tile to place units</returns>
        public Tile GetTileAbsolute(int index)
        {
            return Tiles[index];
        }
        /// <summary>
        /// Gets tile relative to player, so 1,2,3,4 etc from player's POV. Negative indices work like in python, starting from beginning
        /// </summary>
        /// <param name="index">Tile index</param>
        /// <param name="player">Relative to what?</param>
        /// <returns></returns>
        public Tile GetTileRelative(int index, int player)
        {
            index = GetAbsoluteTileCoord(index, player);
            // Now that I got the right index, return correct value...
            return GetTileAbsolute(index);
        }
        /// <summary>
        /// Returns the absolute tile coord given a tile # relative to a player
        /// </summary>
        /// <param name="relativeCoord">The coord in question</param>
        /// <param name="player">Player relative to</param>
        /// <returns></returns>
        public int GetAbsoluteTileCoord(int relativeCoord, int player)
        {
            if (relativeCoord < 0) // Pyhton notation, need to add n, so that -1 -> n-1 and -n becomes 0
            {
                relativeCoord += Len;
            }
            if (player == 1) // Next, for player, I need to flip, so that first is last and vice versa. This involves n-1 complement
            {
                relativeCoord = Len - 1 - relativeCoord;
            }
            return relativeCoord;
        }
        public static int GetAdvanceDirection(int player)
        {
            return (player == 0) ? 1 : -1;
        }
        public override string ToString()
        {
            return Id.ToString() + $", P1: {PlayerUnits[0].Count}u{PlayerBuildings[0].Count}b P2: {PlayerUnits[1].Count}u{PlayerBuildings[1].Count}b";
        }

        public override int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            foreach (Tile tile in Tiles)
            {
                hash.Add(tile.GetGameStateHash());
            }
            return hash.ToHashCode();
        }
    }

    /// <summary>
    /// Contains board content (state) as well as a method to serialize exactly whats going on.
    /// Lanes for now do not contain and shoudl not contain info that isn't found elsewhere.
    /// E.g. lanes/tiles know their units, but units also know their coords, so the hash can only be dependent on the unit to change the game state.
    /// Board however needs to be hashable as it's the root entity that stores and controls unit info.
    /// Lanes and tiles however need to be serialized as json to be quickly rendered into a game UI. They're the only ones that are serialized but not hashed!
    /// This may change in the future if there's extra tile modifiers that are stored in a tile but not on Board directly, in which change we may change the whole hashing scheme
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Board : BoardElement
    {
        [JsonProperty]
        public Lane PlainsLane { get; set; } = new Lane(LaneID.PLAINS, GameConstants.PLAINS_TILES_NUMBER);
        [JsonProperty]
        public Lane ForestLane { get; set; } = new Lane(LaneID.FOREST, GameConstants.FOREST_TILES_NUMBER);
        [JsonProperty]
        public Lane MountainLane { get; set; } = new Lane(LaneID.MOUNTAIN, GameConstants.MOUNTAIN_TILES_NUMBER);
        // Entities
        [JsonProperty]
        public readonly SortedList<int, PlacedEntity> EntityData = new SortedList<int, PlacedEntity>();
        // Methods
        public Lane GetLane(int i)
        {
            return i switch
            {
                0 => PlainsLane,
                1 => ForestLane,
                2 => MountainLane,
                _ => throw new IndexOutOfRangeException("Chosen lane higher than lane count"),
            };
        }
        public Lane GetLane(LaneID laneID)
        {
            return laneID switch
            {
                LaneID.NO_LANE => null,
                LaneID.PLAINS => PlainsLane,
                LaneID.FOREST => ForestLane,
                LaneID.MOUNTAIN => MountainLane,
                _ => throw new Exception("Unrecognized lane requested"),
            };
        }
        public Lane GetLane(CardTargets laneTarget)
        {
            return laneTarget switch
            {
                CardTargets.PLAINS => PlainsLane,
                CardTargets.FOREST => ForestLane,
                CardTargets.MOUNTAIN => MountainLane,
                _ => throw new Exception("Unrecognized lane requested"),
            };
        }
        public PlacedEntity GetEntity(int i)
        {
            if(EntityData.TryGetValue(i, out PlacedEntity entity))
            {
                return entity;
            }
            else
            {
                return null;
            }
        }
        public override int GetGameStateHash()
        {
            HashCode hash = new HashCode();
            foreach (KeyValuePair<int, PlacedEntity> kvp in EntityData)
            {
                hash.Add(kvp.Key);
                hash.Add(kvp.Value.GetGameStateHash());
            }
            hash.Add(PlainsLane.GetGameStateHash());
            hash.Add(ForestLane.GetGameStateHash());
            hash.Add(MountainLane.GetGameStateHash());
            return hash.ToHashCode();
        }
    }
}
