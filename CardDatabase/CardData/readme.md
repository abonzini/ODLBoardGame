This is where card behaviour is added and modified easily.

To find a specific card, the tree is the following:
**\[expansion\]** -> **\[class\]** -> **\[number\].json**

For example, first expansion base class would be Vanilla/Base, if there's X class, look for Vanilla/X.
Then, you'd look for the specific card.
The path is deduced by the index.txt previously mentioned.

The json file is somewhat complex, describes the elements of how a card is printed (i.e. what the user sees),
and the actual thing that is played. The structure is the following:

```
{
    "CardData":
    {
    },
    "UnitData":
    {
    },
    "SkillData":
    {
    },
    "BuildingData":
    {
    }
}
```

The first {} is the card.
**Every** card has ```CardData```, which describes both how the card looks to a player, and also when/how can it be played.
Then, depending on whether the card is Unit/Skill/Building (& etc if new kinds appear later!), just describe the corresponding json, the rest wouldn't be used.

# CardData
Basic data of how a card looks, imagine it as printing a card but also containing game info of how/when card can be played.

- ```Id:``` The unique card id, no other card can have this ID
- ```Name:``` Name string of the card
- ```Text:``` Text that describes the card effect loosely
- ```CardType:``` Card type
    - ```UNKNOWN```
    - ```UNIT```
    - ```BUILDING```
    - ```SKILL```
- ```TargetMode:``` Where the card can be targeted. Options:
    - ```GLOBAL```
    - ```PLAINS```
    - ```FOREST```
    - ```MOUNTAIN```
    - ```ALL_BUT_MOUNTAIN```
    - ```ALL_BUT_FOREST```
    - ```ALL_BUT_PLAINS```
    - ```ANY_LANE```
    - ```INVALID```
- ```TargetConditions:``` Some cards are unable to be played without target, e.g. a skill that damages an enemy needs an enemy to be present (maybe in a specific lane), or similar. This is a collection (i.e. need to define with a []) and however many conditions we need.
Multiple conditions need to be put carefully to avoid weird states.
    - ```NONE```
- ```Bp conditions:``` only relevant for buildings, it's 3 separate integers that describe if the tiles (each bit = each tile) are available to be built on, depending on each building.
    - ```PlainsBpCondition```
    - ```ForestBpCondition```
    - ```MountainsBpCondition```
- ```Cost:``` Cost of card. A string because you could write cool stuff like ```X``` or something w variable costs (although this would neccessitate working more on the back end and adding extra elements to this flag). Currently this will be always interpreted as a number.
- ```Hp:``` String for HP of card
- ```Movement:``` String for movement
- ```Attack:``` String for attack
- ```Rarity:``` Rarity between 1-3 stars (0 if no rarity string)
- ```Expansion:``` Expansion number
    - ```VANILLA```
- ```ClassType:``` If card is a class card...
    - ```BASE```
- ```StealthPlay:``` (true/false) if card is a stealth card.

# UnitData
When card is a unit, the unit needs to contain the following data. This will create a unit with the correct values and effects:
- ```Card:``` The card id number, this matches it with the card data from above in case of needing extra info.
- ```Name:``` Name of unit as it shows once in the field.
- ```Hp:``` Hp number of unit
- ```DamageTokens:``` How many damage tokens the unit has. Default value is naturally 0 unless unit needs to start damaged for some weird reason.
- ```Attack:``` Attack value
- ```Movement:``` Movement value
- ```MovementDenominator:``` Denominator of movement stats, 1 by default if not defined here.

# SkillData
Will write as I implement

# BuildingData
Will write as I implement
