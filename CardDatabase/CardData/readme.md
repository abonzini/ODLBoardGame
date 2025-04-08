This is where card behaviour is added and modified easily.

To find a specific card, the tree is the following:
**\[expansion\]** -> **\[class\]** -> **\[number\].json**

For example, first expansion base class would be Vanilla/Base, if there's X class, look for Vanilla/X.
Then, you'd look for the specific card.
The path is deduced by the index.txt previously mentioned.

The json file is somewhat complex, describes the card in its totality, as well as how the card is played and describes the board entity too.
Every card, no matter the type, has the following elements inside no matter what:

```
"EntityPrintInfo":
{
    "Id": 0,
    "Title": "",
    "Text": "",
    "Cost": "",
    "Hp": "",
    "Movement": "",
    "Attack": "",
    "Rarity": 0,
    "Expansion": "VANILLA",
    "ClassType": "BASE"
}
```

```
"EntityPlayInfo":
{
    "CardType": "UNKNOWN",
    "TargetOptions": "BOARD",
    "TargetConditions": []
}
```

Besides card-specific fields (explained below), these mandatory fields serve the following purpose:
- **EntityPrintInfo** contains all the visual information of how a card is "printed", includes data such as:
    - ```ID:``` Card ID number
    - ```Title:``` I.e. the card "name", or title of the card
    - ```Text:``` Card text/effect if any
    - ```Cost, Hp, Movement, Attack:``` The "stats" of a card if any, consider that these are strings as a placeholder for more complex effects
    - ```Rarity:``` The rarity of the card, ranging from 0 (generated) or 1-3
    - ```Expansion:``` Expansion name in card language
    - ```ClassType:``` Card class (or BASE)
- **EntityPlayInfo**
    - ```EntityType:``` Type of card, for now ```UNIT```, ```SKILL```, ```BUILDING```
    - ```TargetOptions:``` Where the card can be targeted. Options:
        - ```BOARD```
        - ```PLAINS```
        - ```FOREST```
        - ```MOUNTAIN```
        - ```ALL_BUT_MOUNTAIN```
        - ```ALL_BUT_FOREST```
        - ```ALL_BUT_PLAINS```
        - ```ALL_LANES```
        - ```INVALID```

        These values are *Flags*, which means they can also be assembled with the ```|``` symbol.
        For example, ```PLAINS|FOREST``` would work exactly like ```ALL_BUT_MOUNTAIN```.
    - ```TargetConditions:``` Some cards are unable to be played without target, e.g. a skill that damages an enemy needs an enemy to be present (maybe in a specific lane), or similar. Options:
        - ```NONE```, No condition, played always
        - ```BLUEPRINT```, Can be played only if blueprint condition is satisfied (buildings always have this condition)

Then, depending on the type of cards, additional fields are needed/used. Every type of card also can have **Triggers** and **Interactions** which are additional fields explained below. 

## Units
When card is a unit, the unit needs to contain the following data. This will create a unit with the correct values and effects:
- ```Name:``` Name of unit as it shows once in the field, if left empty, will use the name of the printed card.
- ```Hp:``` Base Hp value of unit when summoned
- ```DamageTokens:``` How many damage tokens the unit has. Default value is naturally 0 unless unit needs to start damaged for some weird reason
- ```Attack:``` Attack value
- ```Movement:``` Movement value
- ```MovementDenominator:``` Denominator of movement stats, 1 by default if not defined here.

## Buildings
Building cards are similar to units, they contain the following:
- ```Name:``` Name of building as it shows once in the field, if left empty, will use the name of the printed card.
- ```Hp:``` Base Hp value of building when constructed
- ```DamageTokens:``` How many damage tokens the building has. Default value is naturally 0 unless unit needs to start damaged for some weird reason
- ```PlainsBp```/```ForestBp```/```MountainBp:``` Blueprint of the building for each lane. If left empty, building can't be built in that lane. It is an ordered collection. E.g. ```"PlainsBp": [2,1,3]``` means that, in plains, the building will attempt to be constructed first in tile 2, then 1, and then 3.

## Skills
Skills do not contain any other info as they only have effects (I.e. "When played" interactions), and do not persist in the field.

# Trigger and Interaction Effects

These define a card's "effects".
The two types are:

- **Triggers:** These will trigger when something happens globally, outside of the card's control. For example cards that are designed as "At the end of turn do X", or "When a player does X, this card does Y".
- **Interactions:** Effects that are activated when somethign happens with a card. For example, a card that does something when taking damage, or when played. Most skills will have interactions as they will have effects when played.

The way to define them on a card is with the same syntax, adding ```Interactions``` or ```Triggers``` effect.
They are defined in json as a dictionary.
The key is the type of trigger/interaction, and the value is a list of the effects that will be performed in sequence:

```
"Interactions":
    {
        "INTERACTION_TYPE":
            [
                {
                    "EffectType": <...>
                    <Param 1>: <...>
                    <Param 2>: <...>
                    <...>
                },
                {
                    "EffectType": <...>
                    <Param 1>: <...>
                    <Param 2>: <...>
                    <...>
                },
                <etc>
            ],
        "INTERACTION_TYPE":
            [
                {
                    "EffectType": <...>
                    <Param 1>: <...>
                    <Param 2>: <...>
                    <...>
                },
                {
                    "EffectType": <...>
                    <Param 1>: <...>
                    <Param 2>: <...>
                    <...>
                },
                <etc>
            ],
    }
```

In the example above, the card would contain 2 interactions, and then each would perform a sequence of effects.
An effect is described as an effect type, and a series of parameters that may be needed for the effect to work.
This way, any card can be described in a dynamic, human readable way.
When an effect is ongoing (either because of a Trigger or an Interaction), the game remembers the card running the effects, this means the owner, the card's location, etc.

## Interaction/Trigger types

- ```WHEN_PLAYED:``` Will be executed when the card is played for the first time. Examples: Every single **skill** card

## Effect Types
Search in the next section for what the fields mean and the possible values they may take

- ```FIND_ENTITIES:``` Task that finds all valid entities to be targetd for an effect.
For example, skills that deal damage to an unit, or to the enemy hero, or destroy all buildings, etc.
These targets are found by using ```FIND_ENTITIES``` and setting a bunch  of search criteria.
Parameters:
    - ```TargetLocation``` where to search for the entity in question
    - ```TargetPlayer``` serves as a filter where you only get the entities of the player owner in question
    - ```TargetType``` the type of entities that can be targeted 
    - ```SearchCriterion``` determines which target(s) can be found as valid targets
    - ```Value``` is used alongside ```SearchCriterion``` as the value $n$. Negative values imply the search is done in reverse order.

    This may seem convoluted but it's a robust way to target arbitrary combination of target conditions.
    Keep in mind that, no matter the ```TargetLocation```, the order of valid targets will be: ***[PLAYER]->[LOCATION]->[PLAYER]*** where which player is first is determined by the sign of ```Value```.
    When looking for entities on a lane, the system traverses the lane in order determined by ```Value``` sign.
    In case of multiple entities in the same position, the unit that was played first is targeted first.

- ```SUMMON_UNIT:``` Summons a unit in a desired lane or set of lanes.
Parameters:
    - ```CardNumber``` is the card number of the unit summoned
    - ```TargetPlayer``` is the player who will own the unit
    - ```TargetLocation``` is one or more lane targets where the card(s) will be summoned
    
    Examples: **RUSH**

## Enum Values

- ```TargetLocation```
    - ```BOARD```. For cards where the target is the "whole board" 
    - ```PLAINS```
    - ```FOREST```
    - ```MOUNTAIN```
    - ```ALL_BUT_MOUNTAIN```
    - ```ALL_BUT_FOREST```
    - ```ALL_BUT_PLAINS```
    - ```ALL_LANES```

    These values are *Flags*, which means they can also be assembled with the ```|``` symbol.
    For example, ```PLAINS|FOREST``` would work exactly like ```ALL_BUT_MOUNTAIN```.
- ```TargetPlayer```
    - ```OWNER```: Owner of the card receives the effect
    - ```OPPONENT```: Player opposing the card owner will receive the effect
    - ```BOTH```: Both players receive the effect

    These values are *Flags*, which means they can also be assembled with the ```|``` symbol.
    For example, ```OWNER|OPPONENT``` would work exactly like ```BOTH```.
- ```SearchCriterion```
    - ```ORDINAL``` targets the $n^{th}$ element found
    - ```QUANTITY``` targets the first $n$ elements
    - ```ALL``` targets everything found
- ```EntityType```
    - ```NONE```
    - ```UNIT```
    - ```BUILDING```
    - ```PLAYER```

    These values are *Flags*, which means they can also be assembled with the ```|``` symbol.
    For example, ```UNIT|BUILDING``` means both units and buildings will be accepted.