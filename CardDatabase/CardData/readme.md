This is where card behaviour is modified easily.

To find something, the tree is the following:
**\[expansion\]** -> **\[class\].json**

For example, first expansion base class would be Vanilla/Base.json, if there's X class, look for Vanilla/X.json

The json file is somewhat complex, describes the elements of how a card is printed (i.e. what the user sees),
and the actual thing that is played. The structure is the following:

```
[
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
]
```
Where [] is the whole collection of all cards in this json file.
The first {} is each card.
**Every** card has ```CardData```, which describes both how the card looks to a player, and also when/how can it be played.
Then, depending on whether the card is Unit/Skill/Building (& etc if new kinds appear later!), just describe the corresponding json, the rest wouldn't be used.

# CardData
Basic data of how a card looks, imagine it as printing a card but also containing game info of how/when card can be played.

- ```Id:``` The unique card id, no other card can have this ID
- ```Name:``` Name string of the card
- ```Text:``` Text that describes the card effect loosely
- ```CardType:``` Card type, whether spell, unit, etc
    - ```UNKNOWN=0```
    - ```UNIT=1```
    - ```BUILDING=2```
    - ```SKILL=3```
- ```TargetMode:``` A number of where the card could potentially be played. 0 if not played in any lane (e.g. rush), 1, 2, 4 for respective lanes (plains, forest, mountains).
It has been made like this so you can also have combined binary lanes, e.g. lane 3 means only exclude mountains.  
- ```TargetConditions:``` Some cards are unable to be played without target, e.g. a skill that damages an enemy needs an enemy to be present (maybe in a specific lane), or similar. This is a collection (i.e. need to define with a []) and however many conditions we need.
Multiple conditions need to be put carefully to avoid weird states.
    - ```NONE=0``` No condition. Card would be played anytime unless other conditions also present. Technically this condition is unnecessary.
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
    - ```VANILLA=0```
- ```ClassType:``` If card is a class card...
    - ```BASE=0```
- ```StealthPlay:``` If card is a stealth card, it is represented differently by the engine.

# UnitData
When card is a unit, the unit needs to contain the following data. This will create a unit with the correct values and effects:
- ```Card:``` The card id number, this matches it with the card data from above in case of needing extra info.
- ```Name:``` Name of unit as it shows once in the field.
- ```Hp:``` Hp number of unit
- ```Attack:``` Attack value
- ```Movement:``` Movement value
- ```MovementDenominator:``` Denominator of movement stats, 1 by default if not defined here.

# SkillData
Will write as I implement

# BuildingData
Will write as I implement
