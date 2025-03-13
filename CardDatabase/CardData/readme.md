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
    "TargetOptions": "GLOBAL",
    "TargetConditions": [],
    "StealthPlay": false
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
    - ```CardType:``` Type of card, for now UNIT, SKILL, BUILDING
    - ```TargetOptions:``` Where the card can be targeted. Options:
        - ```GLOBAL```
        - ```PLAINS```
        - ```FOREST```
        - ```MOUNTAIN```
        - ```ALL_BUT_MOUNTAIN```
        - ```ALL_BUT_FOREST```
        - ```ALL_BUT_PLAINS```
        - ```ANY_LANE```
        - ```INVALID```
    - ```TargetConditions:``` Some cards are unable to be played without target, e.g. a skill that damages an enemy needs an enemy to be present (maybe in a specific lane), or similar. This is a collection (i.e. need to define with a []) and however many conditions we need. Multiple conditions need to be put carefully to avoid weird states
        - ```NONE```
    - ```StealthPlay:``` (true/false) if card is a stealth card

Then, depending on the type of cards, additional fields are needed/used. Every type of card also can have **Triggers** and **Interactions** which are additional fields explained below. 

# Units
When card is a unit, the unit needs to contain the following data. This will create a unit with the correct values and effects:
- ```Name:``` Name of unit as it shows once in the field, if left empty, will use the name of the printed card.
- ```Hp:``` Base Hp value of unit when summoned
- ```DamageTokens:``` How many damage tokens the unit has. Default value is naturally 0 unless unit needs to start damaged for some weird reason
- ```Attack:``` Attack value
- ```Movement:``` Movement value
- ```MovementDenominator:``` Denominator of movement stats, 1 by default if not defined here.

# SkillData
Will write as I implement

# BuildingData
Will write as I implement

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

In the example above, the ard would contain 2 interactions, and then each would perform a sequence of effects.
An effect is described as an effect type, and a series of parameters that may be needed for the effect to work.
This way, any card can be described in a dynamic, human readable way.

## Interaction/Trigger types

- ```WHEN_PLAYED:``` Will be executed when the card is played for the first time. Exmaples: Every single ```skill``` card

## Effect Types

- ```SUMMON_UNIT:``` Summons a unit in a desired lane or set of lanes. ```CardNumber``` is the card number of the unit summoned, and ```CardTargets``` is one or more lane targets where the card(s) will be summoned card targets are worded identically as the targets in **EntityPlayInfo**. Examples: ```RUSH```