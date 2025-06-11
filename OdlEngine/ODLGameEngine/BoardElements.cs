using Newtonsoft.Json;

namespace ODLGameEngine
{
    public enum BoardElementListOperation
    {
        ADD,
        REMOVE
    }
    public enum BoardElementType
    {
        NONE,
        BOARD,
        LANE,
        TILE
    }
    class TriggerTupleComparer : IComparer<Tuple<int, EffectLocation>>
    {
        public int Compare(Tuple<int, EffectLocation> x, Tuple<int, EffectLocation> y)
        {
            int result = x.Item1.CompareTo(y.Item1);
            return result != 0 ? result : x.Item2.CompareTo(y.Item2);
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class BoardElement
    {
        [JsonProperty]
        public BoardElementType ElementType { get; set; } = BoardElementType.NONE;
        [JsonProperty]
        public Dictionary<(EntityType, int), SortedSet<int>> PlacedEntities { get; set; } = new Dictionary<(EntityType, int), SortedSet<int>>();
        [JsonProperty]
        public SortedDictionary<TriggerType, SortedSet<Tuple<int, EffectLocation>>> TriggerList = new SortedDictionary<TriggerType, SortedSet<Tuple<int, EffectLocation>>>();
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
        public void EntityListOperation(PlacedEntity entity, BoardElementListOperation op)
        {
            // Also define the flags to allow into generalised lists
            int allOwners = -1;
            List<EntityType> allowedEntities = new List<EntityType>([EntityType.UNIT, EntityType.BUILDING]);
            if (allowedEntities.Remove(entity.EntityType)) // This one is always 1 so I don't need to iterate on it
            {
                // Only continue if entity type was really present
                int index = entity.UniqueId;
                int owner = entity.Owner;

                int numberOfCombinations = 1 << allowedEntities.Count; // 2^count
                for (int i = 0; i < numberOfCombinations; i++)
                {
                    EntityType nextEntityCombination = entity.EntityType; // This flag is always 1
                    for (int bit = 1; bit <= allowedEntities.Count; bit++)
                    {
                        if ((i & bit) != 0) // Entity is present in this combination
                        {
                            nextEntityCombination |= allowedEntities[bit - 1]; // In this case, it's added to a combination
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
                        case BoardElementListOperation.ADD:
                            PlacedEntities[(nextEntityCombination, allOwners)].Add(index);
                            PlacedEntities[(nextEntityCombination, owner)].Add(index);
                            break;
                        case BoardElementListOperation.REMOVE:
                            PlacedEntities[(nextEntityCombination, allOwners)].Remove(index);
                            PlacedEntities[(nextEntityCombination, owner)].Remove(index);
                            break;
                        default:
                            throw new NotImplementedException("Invalid list operation");
                    }
                }
            }
        }
        /// <summary>
        /// Gets all the subscribed triggers for this board element and this trigger
        /// </summary>
        /// <param name="triggerType">Type of trigger</param>
        /// <returns>All of the subscribed triggers (if any)</returns>
        public SortedSet<Tuple<int, EffectLocation>> GetSubscribedTriggers(TriggerType triggerType)
        {
            TriggerList.TryGetValue(triggerType, out SortedSet<Tuple<int, EffectLocation>> result);
            return result;
        }
        /// <summary>
        /// Adds or removes a trigger from this element's trigger list
        /// </summary>
        /// <param name="trigger">Trigger type</param>
        /// <param name="id">Id of unit</param>
        /// <param name="relativeLocation">Relative location of unit trigger w.r.t. this board element</param>
        public void TriggerListOperation(TriggerType trigger, int id, EffectLocation relativeLocation, BoardElementListOperation op)
        {
            Tuple<int, EffectLocation> triggerDescriptor = new Tuple<int, EffectLocation>(id, relativeLocation);
            if (!TriggerList.TryGetValue(trigger, out SortedSet<Tuple<int, EffectLocation>> thisTriggerList)) // Create trigger handler and list if doesn't exist
            {
                thisTriggerList = new SortedSet<Tuple<int, EffectLocation>>(new TriggerTupleComparer());
                TriggerList.Add(trigger, thisTriggerList);
            }
            // Then I'll add or remove the elements
            switch (op)
            {
                case BoardElementListOperation.ADD:
                    thisTriggerList.Add(triggerDescriptor);
                    break;
                case BoardElementListOperation.REMOVE:
                    thisTriggerList.Remove(triggerDescriptor);
                    if (thisTriggerList.Count == 0) // Emptied the list
                    {
                        TriggerList.Remove(trigger); // Remove the trigger from this
                    }
                    break;
                default:
                    throw new NotImplementedException("Invalid list operation");
            }
        }
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            foreach (KeyValuePair<TriggerType, SortedSet<Tuple<int, EffectLocation>>> kvp in TriggerList)
            {
                hash.Add(kvp.Key);
                foreach (Tuple<int, EffectLocation> trigdata in kvp.Value)
                {
                    hash.Add(trigdata.Item1);
                    hash.Add(trigdata.Item2);
                }
            }
            return hash.ToHashCode();
        }
        public override string ToString()
        {
            return $"P1: {GetPlacedEntities(EntityType.UNIT, 0).Count} P2: {GetPlacedEntities(EntityType.UNIT, 1).Count} B: {GetPlacedEntities(EntityType.BUILDING).Count})";
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class Tile : BoardElement
    {
        [JsonProperty]
        public int Coord { get; set; } = -1;
        public Tile()
        {
            ElementType = BoardElementType.TILE;
        }
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode()); // Adds trigger data
            hash.Add(Coord);
            foreach (int entity in GetPlacedEntities(EntityType.UNIT | EntityType.BUILDING))
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
    public enum LaneRelativeIndexType
    {
        RELATIVE_TO_PLAYER,
        RELATIVE_TO_LANE,
        ABSOLUTE
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class Lane : BoardElement /// Player 0 goes from 0 -> N-1 and vice versa. Absolute truth is always w.r.t. player 0
    {
        [JsonProperty]
        public LaneID Id { get; set; } = LaneID.NO_LANE;
        [JsonProperty]
        public int Len { get; set; } = 0;
        [JsonProperty]
        public int FirstTileIndexOffset { get; set; } = 0;
        [JsonProperty]
        public List<Tile> Tiles { get; set; } = null;
        public Lane(LaneID id, int n, Tile[] tiles, int firstTileIndex)
        {
            ElementType = BoardElementType.LANE;
            Id = id;
            Len = n;
            FirstTileIndexOffset = firstTileIndex;
            Tiles = new List<Tile>(n);
            for (int i = 0; i < Len; i++)
            {
                Tiles.Add(tiles[firstTileIndex + i]);
            }
        }
        /// <summary>
        /// Complex lane operation that returns a tile coordinate in the output format required, given a reference input format and index
        /// </summary>
        /// <param name="outIndexType">Format of output index</param>
        /// <param name="inIndexType">Format of input index</param>
        /// <param name="inIndex">Index</param>
        /// <param name="referencePlayer">If input/output is relative to player, then i need the player reference</param>
        /// <returns></returns>
        public int GetTileCoordinateConversion(LaneRelativeIndexType outIndexType, LaneRelativeIndexType inIndexType, int inIndex, int referencePlayer = -1)
        {
            if (outIndexType == inIndexType)
            {
                return inIndex; // This means there was no conversion to do
            }
            int laneCoordinate; // Coordinate relative to this lane
            switch (inIndexType)
            {
                case LaneRelativeIndexType.RELATIVE_TO_PLAYER:
                    laneCoordinate = inIndex;
                    if (inIndex < 0) // Pyhton notation, need to add n, so that -1 -> n-1 and -n becomes 0
                    {
                        laneCoordinate = inIndex + Len;
                    }
                    if (referencePlayer == 1) // Next, for player, I need to flip, so that first is last and vice versa. This involves n-1 complement
                    {
                        laneCoordinate = Len - 1 - laneCoordinate;
                    }
                    else if (referencePlayer != 0)
                    {
                        throw new ArgumentException("Reference player is incorrect");
                    }
                    break;
                case LaneRelativeIndexType.RELATIVE_TO_LANE:
                    laneCoordinate = inIndex;
                    break;
                case LaneRelativeIndexType.ABSOLUTE:
                    laneCoordinate = inIndex - FirstTileIndexOffset;
                    break;
                default:
                    throw new ArgumentException("Invalid input coordinate type");
            }
            // Finally, convert to desired reference and return
            switch (outIndexType)
            {
                case LaneRelativeIndexType.RELATIVE_TO_PLAYER:
                    if (referencePlayer == 1) // Next, for player, I need to flip, so that first is last and vice versa. This involves n-1 complement
                    {
                        laneCoordinate = Len - 1 - laneCoordinate;
                    }
                    else if (referencePlayer != 0)
                    {
                        throw new ArgumentException("Reference player is incorrect");
                    }
                    break;
                case LaneRelativeIndexType.RELATIVE_TO_LANE: // Already got this...
                    break;
                case LaneRelativeIndexType.ABSOLUTE:
                    laneCoordinate += FirstTileIndexOffset;
                    break;
                default:
                    throw new ArgumentException("Invalid input coordinate type");
            }
            return laneCoordinate;
        }
        /// <summary>
        /// Operation that returns a Tile given a coordinate and input format
        /// Naturally, can't operate with most of the absolute tiles!
        /// </summary>
        /// <param name="inIndexType">Type of input index coord (relative?)</param>
        /// <param name="inIndex">Index of desired coord</param>
        /// <param name="referencePlayer">Player for reference in the relative to player tile mode</param>
        /// <returns>The tile</returns>
        public Tile GetTileFromCoordinate(LaneRelativeIndexType inIndexType, int inIndex, int referencePlayer = -1)
        {
            int laneCoordinate = GetTileCoordinateConversion(LaneRelativeIndexType.RELATIVE_TO_LANE, inIndexType, inIndex, referencePlayer);
            // At this point the coordinate is exactly relative to this current lane, assert to be within bounds
            if (laneCoordinate < 0 || laneCoordinate >= Len) throw new Exception("Invalid reference tile for this lane!");
            return Tiles[laneCoordinate];
        }
        /// <summary>
        /// Given a tile coord and a reference player, tells us whether this is the last tile of this lane
        /// </summary>
        /// <param name="inIndexType">Indicates the relative position of input index</param>
        /// <param name="inIndex">Coord of tile (relative or absolute)</param>
        /// <param name="referencePlayer">Reference player</param>
        /// <returns>Whether this tile coord is the end of a lane</returns>
        public bool IsRelativeEndOfLane(LaneRelativeIndexType inIndexType, int inIndex, int referencePlayer)
        {
            int laneCoordinate = GetTileCoordinateConversion(LaneRelativeIndexType.RELATIVE_TO_PLAYER, inIndexType, inIndex, referencePlayer);
            return laneCoordinate == (Len - 1);
        }
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode()); // Adds trigger data
            foreach (Tile tile in Tiles)
            {
                hash.Add(tile.GetHashCode());
            }
            return hash.ToHashCode();
        }
    }

    /// <summary>
    /// Contains everything board related, including all tiles and all lanes.
    /// Data is referenced in multiple insances to allow orderings such as lanes containing tiles and so on
    /// The newest scheme is that tiles are now global and have a single mono-coordinate to facilitate movement, therefore board also allows simpler ops
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Board : BoardElement
    {
        [JsonProperty]
        public Tile[] Tiles { get; set; } = null;
        [JsonProperty]
        public Lane[] Lanes { get; set; } = null; // These will be the lanes (when I fill them)
        [JsonProperty]
        public Lane PlainsLane { get; set; } = null;
        [JsonProperty]
        public Lane ForestLane { get; set; } = null;
        [JsonProperty]
        public Lane MountainLane { get; set; } = null;
        public Board()
        {
            ElementType = BoardElementType.BOARD;
            int numberOfTiles = GameConstants.PLAINS_NUMBER_OF_TILES + GameConstants.FOREST_NUMBER_OF_TILES + GameConstants.MOUNTAIN_NUMBER_OF_TILES;
            Tiles = new Tile[numberOfTiles]; // Inits all the tiles
            for (int i = 0; i < numberOfTiles; i++)
            {
                Tiles[i] = new Tile
                {
                    Coord = i
                };
            }
            PlainsLane = new Lane(LaneID.PLAINS, GameConstants.PLAINS_NUMBER_OF_TILES, Tiles, 0);
            ForestLane = new Lane(LaneID.FOREST, GameConstants.FOREST_NUMBER_OF_TILES, Tiles, GameConstants.PLAINS_NUMBER_OF_TILES);
            MountainLane = new Lane(LaneID.MOUNTAIN, GameConstants.MOUNTAIN_NUMBER_OF_TILES, Tiles, GameConstants.PLAINS_NUMBER_OF_TILES + GameConstants.FOREST_NUMBER_OF_TILES);
            Lanes = [PlainsLane, ForestLane, MountainLane]; // Reference lane properly
        }
        // Methods, obtain and do stuff
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
        public Lane GetLane(int lane)
        {
            return Lanes[lane];
        }
        public Lane GetLaneContainingTile(int tileCoord)
        {
            if (tileCoord < 0) { }
            else if (tileCoord < GameConstants.PLAINS_NUMBER_OF_TILES)
            {
                return PlainsLane;
            }
            else if (tileCoord < (GameConstants.PLAINS_NUMBER_OF_TILES + GameConstants.FOREST_NUMBER_OF_TILES))
            {
                return ForestLane;
            }
            else if (tileCoord < (GameConstants.PLAINS_NUMBER_OF_TILES + GameConstants.FOREST_NUMBER_OF_TILES + GameConstants.MOUNTAIN_NUMBER_OF_TILES))
            {
                return MountainLane;
            }
            return null;
        }
        public Lane GetLaneContainingTile(Tile tile)
        {
            return GetLaneContainingTile(tile.Coord);
        }
        //EntityListOperation(PlacedEntity entity, EntityListOperation op)
        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode()); // Adds trigger data
            hash.Add(PlainsLane.GetHashCode());
            hash.Add(ForestLane.GetHashCode());
            hash.Add(MountainLane.GetHashCode());
            return hash.ToHashCode();
        }
    }
}
