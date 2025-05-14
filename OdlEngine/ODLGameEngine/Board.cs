using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public enum EntityListOperation
    {
        ADD,
        REMOVE
    }
    public abstract class BoardElement
    {
        readonly Dictionary<(EntityType, int), SortedSet<int>> PlacedEntities = new Dictionary<(EntityType, int), SortedSet<int>>();
        public SortedSet<int> GetPlacedEntities(EntityType entityTypes, int owner = -1)
        {
            EntityType entityMask = EntityType.UNIT | EntityType.BUILDING; // Ignore noise as it can't be in board anyway
            entityTypes &= entityMask;
            if (!PlacedEntities.ContainsKey((entityTypes, owner)))
            {
                PlacedEntities[(entityTypes, owner)] = new SortedSet<int>(); // Returns an empty list if nothing there
            }
            return PlacedEntities[(entityTypes, owner)];
        }
        public void EntityListOperation(PlacedEntity entity, EntityListOperation op)
        {
            // Also define the flags to allow into generalised lists
            int allOwners = -1;
            List<EntityType> allowedEntities = new List<EntityType>([EntityType.UNIT, EntityType.BUILDING]);
            if(allowedEntities.Remove(entity.PrePlayInfo.EntityType)) // This one is always 1 so I don't need to iterate on it
            {
                // Only continue if entity type was really present
                int index = entity.UniqueId;
                int owner = entity.Owner;

                int numberOfCombinations = 1 << allowedEntities.Count; // 2^count
                for (int i = 0; i < numberOfCombinations; i++)
                {
                    EntityType nextEntityCombination = entity.PrePlayInfo.EntityType; // This flag is always 1
                    for (int bit = 1; bit <= allowedEntities.Count; bit++)
                    {
                        if ((i & bit) != 0) // Entity is present in this combination
                        {
                            nextEntityCombination |= allowedEntities[bit-1]; // In this case, it's added to a combination
                        }
                    }
                    // At this point a new combination has been created, check if any of them is new and needs to be init
                    if (!PlacedEntities.ContainsKey((nextEntityCombination, allOwners)))
                    {
                        PlacedEntities[(nextEntityCombination, allOwners)] = new SortedSet<int>();
                    }
                    if (!PlacedEntities.ContainsKey((nextEntityCombination, owner)))
                    {
                        PlacedEntities[(nextEntityCombination, owner)] = new SortedSet<int>();
                    }
                    // Then add the elements where they belong
                    switch (op)
                    {
                        case ODLGameEngine.EntityListOperation.ADD:
                            PlacedEntities[(nextEntityCombination, allOwners)].Add(index);
                            PlacedEntities[(nextEntityCombination, owner)].Add(index);
                            break;
                        case ODLGameEngine.EntityListOperation.REMOVE:
                            PlacedEntities[(nextEntityCombination, allOwners)].Remove(index);
                            PlacedEntities[(nextEntityCombination, owner)].Remove(index);
                            break;
                        default:
                            throw new NotImplementedException("Invalid list operation");
                    }
                }
            }
        }
        public override int GetHashCode()
        {
            return 0;
        }
        public override string ToString()
        {
            return $"P1: {GetPlacedEntities(EntityType.UNIT, 0).Count} P2: {GetPlacedEntities(EntityType.UNIT, 1).Count} B: {GetPlacedEntities(EntityType.BUILDING).Count})";
        }
    }
    public class Tile : BoardElement
    {
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            foreach(int entity in GetPlacedEntities(EntityType.UNIT|EntityType.BUILDING))
            {
                hash.Add(entity);
            }
            return hash.ToHashCode();
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
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            foreach (Tile tile in Tiles)
            {
                hash.Add(tile.GetHashCode());
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
        public Lane GetLane(TargetLocation laneTarget)
        {
            return laneTarget switch
            {
                TargetLocation.PLAINS => PlainsLane,
                TargetLocation.FOREST => ForestLane,
                TargetLocation.MOUNTAIN => MountainLane,
                _ => throw new Exception("Unrecognized lane requested"),
            };
        }
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(PlainsLane.GetHashCode());
            hash.Add(ForestLane.GetHashCode());
            hash.Add(MountainLane.GetHashCode());
            return hash.ToHashCode();
        }
    }
}
