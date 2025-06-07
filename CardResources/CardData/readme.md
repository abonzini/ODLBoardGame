This is where card behaviour is added and modified easily.
Cards are described via ```.json``` file.
The ```-illustration.json``` file contains extra illustration data for redrawing the card, which is not of use here.
Name of the file is the card ID.

The json file is somewhat complex, describes the card in its totality.
Thhis means, it contains all the data needed to describe how a card is played, visualized, and the effects it has once it's actually placed on the board.
Every card, no matter the type, has the following elements:

```
"Id": 0,
"EntityType": "NONE",
"Cost": 0,
"TargetOptions": "BOARD",
```

These elements define the basics of the basics for any type of card:
    - ```Id:``` Card ID number
    - ```Cost:``` How much it costs to play the card
    - ```EntityType:``` Type of card, such as ```UNIT```, ```SKILL```, ```BUILDING```, ```PLAYER```
    - ```TargetOptions:``` Where the card can be targeted when playing. Valid options:
        - ```BOARD```
        - ```PLAINS```
        - ```FOREST```
        - ```MOUNTAIN```
        - ```ALL_BUT_MOUNTAIN```
        - ```ALL_BUT_FOREST```
        - ```ALL_BUT_PLAINS```
        - ```ALL_LANES```

        These values are *Flags*, which means they can also be assembled with the ```|``` symbol.
        For example, ```PLAINS|FOREST``` would work exactly like ```ALL_BUT_MOUNTAIN```.

Depending on the type of cards, additional fields are needed/used. Every type of card also can have **Interactions**.
Living entities, e.g. entities that are permanent in the board (anything but skills), also have **Triggers**.
**Interactions** and **Triggers** are quite complex and so will be explained in more detail later in this file.

## Units
When card is a unit, the unit needs to contain the following data. This will create a unit with the correct values and effects:
- ```Name:``` Name of unit as it will show once in the field
- ```Hp:``` Base Hp value of unit when summoned
- ```DamageTokens:``` How many damage tokens the unit has. Default value is naturally 0 unless unit needs to start damaged for some weird reason
- ```Attack:``` Attack value
- ```Movement:``` Movement value
- ```MovementDenominator:``` Denominator of movement stats, 1 by default if not defined here.

## Buildings
Building cards are similar to units, they contain the following:
- ```Name:``` Name of building as it will show once in the field
- ```Hp:``` Base Hp value of building when constructed
- ```DamageTokens:``` How many damage tokens the building has. Default value is naturally 0 unless unit needs to start damaged for some weird reason
- ```PlainsBp```/```ForestBp```/```MountainBp:``` Blueprint of the building for each lane. If left empty, building can't be built in that lane. It is an ordered collection. E.g. ```"PlainsBp": [2,1,3]``` means that, in plains, the building will attempt to be constructed first in tile 2, then 1, and then 3.

## Skills
Skills do not contain any other info as they only have effects (I.e. "When played" interactions), and do not persist in the field.
This is why also they don't contain triggers.

## Player Class
Player classes have many similarities to some cards.
For example, players contain Hp, like Units and Buildings.
Moreover, player classes may also contain complex mechanics, including Triggers and Interactions, caused either by card effects but also as "Passives" of the class.
After much deliberation, it was decided to incorporate classes as a *type of card*.
Naturally, you won't be able to add them into the deck (unless its some sort of weird hearthstone tribe-change effect???) but it is loaded and interpreted as such.
Thay have the following properties:
- ```Name:``` Name of player (nickname), this doesn't need to be set in .json as it is loaded by the game engine
- ```Hp:``` Starting HP of player
- ```CurrentGold``` Amount of gold they hold
- ```ActivePowerId``` The card that takes part of the "active power". Has to be a skill with Board targeting

# Trigger and Interaction Effects

These define a card's "effects" and very complex behaviours.
The two types are:

- **Triggers:** These will trigger when something happens globally, outside of the card's control.
For example cards that are designed as "At the end of turn do X", or "When a player does X, this card does Y".
When this event happens, the corresponding entity will be triggered and do the effects.
- **Interactions:** Effects that are activated when something happens with a card. For example, a card that does something when taking damage, or when played.
Skills will have interactions only as they don't persist after it's effect resolves.

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

## Interaction Types

- ```WHEN_PLAYED``` Will be executed when the card is played (FROM HAND) for the first time.
- ```UNIT_ENTERS_BUILDING``` Is executed when a unit enters a building (either when summoned on top or passing during march). This interaction happens only once, and the Unit/Building will need to each process the effect from their own POV.

# Effect Mechanism

In order to be able to create complex card behaviour, an effect chain behaves similarly to a low-level CPU.
In this scheme, each ```EffectType``` is like an instruction, some of them can have parameters.
In order to implement complex effects that need to remember previous results (think effects such as *"Damage all enemy buildings, if any is killed, deal 2 damage to enemy player"* or *"Get 1 gold for each unit on the field"*), the effect chain contains a series of **Register Variables** that can be used as input and/or outputs:

- ```TEMP_VARIABLE```, is volatile and only remembered during the current effect of the chain. However it contains the "value" of many card effects (e.g. if an effect was *"Gain 2 gold"*, the temp variable would contain the value 2 in this case). This value can be used as a parameter in effects and in arithmetic operations
- ```ACC``` is the **"accumulator"** variable, and retains its value during the effect resolution chain, making it useful as the target of more complex math operations.
It is initialised with a value of 0 at the beginning of the effect chain
- ***Multiple other variables*** are also usable as both input and outputs, both to "check" ingame values (e.g. if you want to check if a target has X health or whatever), and some can also be the Output target of operations (e.g. if you want an effect that *sets* a target health)

For a list of posible ```Input```/```Output``` options, check below. 

Finally, the CPU context is also able to hold a list of reference entities, obtained by specific search/select operations.
This reference is important to track ownership, select targets, and other cool stuff.
By default the effect's reference is the card with the ongoing effect, but the references can be changed by using search/select operations (as described below).

When an operation's ```Input``` depends on values in the Reference's List (e.g. ```"Input": "TARGET_ATTACK"``` of a series of units found after a search), then there may be multiple inputs, and extra Input processing is needed to provide a single Input value.
For these options, check the possible values of ```MultiInputProcessing```.

## Effect List
These are the following supported operations (called effects).
All effects are relative to the list of references on the ongoing effect.
For example, the meaning of ```TargetPlayer``` as ```OWNER```/```OPPONENT``` is exclusively relative to the current reference(s).

- ```SELECT_ENTITY``` Selects the reference entity for the remainder of the effect (or until a new search is made).
It will select a single target out of known entities that participate in a trigger or interaction.
For example if a unit attacks another, ```SELECT_ENTITY``` can be used to target either the unit that attacked or the affected unit.
Owner and target type filters can be used for slightly more complex effects, such as "When a **friendly unit** does X".
    
    Parameters:
    - ```SearchCriterion``` determines which entity will be targeted by this
    - ```TargetPlayer``` serves as a filter where you only target entities of the player owner in question
    - ```TargetType``` the type of entities that can be targeted

- ```FIND_ENTITIES``` Is a task that finds all valid entities to be targeted for an effect.
All of these entities will become references for subsequent effects.
For example, skills that deal damage to an unit, or to the enemy hero, or destroy all buildings, etc.
These targets are found by using ```FIND_ENTITIES``` and setting a bunch  of search criteria.
If you want to change the reference of the search, you may need to do a ```SELECT_ENTITY``` call first, for example in an effect like *"When a card is played, all of that player's Units receive 1 damage"*, where the reference is not necesarily the same in every trigger.
    
    Parameters:
    - ```EffectLocation``` where to search for the entity in question
    - ```TargetPlayer``` serves as a filter where you only get the entities of the player owner in question
    - ```TargetType``` the type of entities that can be targeted 
    - ```SearchCriterion``` determines which target(s) can be found as valid targets. Some search criterions use a value $n$ as an input
    - The ```Input``` is used alongside some ```SearchCriterion``` cases as the value $n$. Negative values imply the search is done in reverse order

    This may seem convoluted but it's a robust way to target arbitrary combination of target conditions.
    Keep in mind that, no matter the ```EffectLocation```, the order of valid targets will be: ***[PLAYER]->[LOCATION]->[PLAYER]*** where which player is first is determined by the sign of $n$.
    When looking for entities on a lane, the system traverses the lane in order determined by $n$ sign.
    In case of multiple entities in the same position, the unit that was played first is targeted first.

- ```SUMMON_UNIT``` Summons a unit in a desired lane or set of lanes. Multiple units may be summoned if multiple lanes are defined, and/or if there's multiple references.
This allows crazy effects like *"When a unit dies, play a skeleton in the opponent's side"* or "Play a shadow demon for every unit the opponent has in the same lane".

    Parameters:
    - ```TargetPlayer``` is the player who will own the unit (relative to reference entity)
    - ```EffectLocation``` is one or more lane targets where the card(s) will be summoned
    - ```Input``` will contain the card number of the unit summoned

- ```MODIFIER``` A mathematical operation, where an Output is changed, using the Input and an operation.
In some modifiers, the modification target is dependent on units found via a ```FIND_ENTITIES``` or ```SELECT_ENTITY``` operation.
For example, if stats need to be buffed/debuffed, or a specific player gets gold.

    Parameters:
    - ```ModifierOperation``` how the modifier's value is applied (i.e. whether it's a multiplaction, addition, etc)
    - ```Input``` contains the value $n$ of the modifier, needed for some (most?) operations
    - ```Output``` defines *what* is modified, if a stat, a damage value, etc
    - ```TargetPlayer``` in some cases where you want to modify a player's value (e.g. gold), this field is used to choose whether it's a card's owner or the opponents, whose value can be modified. This allows effects such as *"Destroy a card and refund the owner"*.

- ```ASSERT``` is a mathematical operation that checks if the desired input value $\neq 0$. If the assert succeeds, nothing happens, but if it fails, the effect loop ends right there, and the following effects won't be executed.
Useful for effect with complex conditions where a part of the effect is conditional on something happening.

    Parameters:
    - ```Input``` contains the value $n$ to assert

## Possible Parameter Values

- ```EffectLocation```
    - ```BOARD``` will target the board
    - ```PLAINS```/```FOREST```/```MOUNTAIN``` wil target these lanes specifically
    - ```PLAY_TARGET``` the play target of the card just played is used, whatever that was. Only makes sense in cards with effects ```WHEN_PLAYED```.
- ```TargetPlayer```
    - ```OWNER```: Owner of the card receives the effect
    - ```OPPONENT```: Player opposing the card owner will receive the effect
    - ```BOTH```: Both players receive the effect

    These values are *Flags*, which means they can also be assembled with the ```|``` symbol.
    For example, ```OWNER|OPPONENT``` would work exactly like ```BOTH```.
- ```SearchCriterion```

    For board searching:
    - ```ORDINAL``` targets the $n^{th}$ element found
    - ```QUANTITY``` targets the first $n$ elements
    - ```ALL``` targets everything found

    When selecting a specific known entity:
    - ```EFFECT_OWNING_ENTITY``` will target the entity that owns the effect
    - ```ACTOR_ENTITY``` will target the entity that "does something"
    - ```AFFECTED_ENTITY``` will targeted the entity that was affected by an interaction

- ```TargetType```
    - ```NONE```
    - ```SKILL``` (this is a weird target type only makes sense when a skill targets itself during casting)
    - ```UNIT```
    - ```BUILDING```
    - ```PLAYER```

    These values are *Flags*, which means they can also be assembled with the ```|``` symbol.
    For example, ```BUILDING|UNIT``` would search for both buildings and units
- ```ModifierOperation```
    - ```SET``` will modify the target to have the value $n$
    - ```ADD``` adds $n$ to the target
    - ```MULTIPLY``` multiplies target by $n$
    - ```ABSOLUTE_SET``` very similar to ```SET``` but it is a harsher operation: it overwrites the base value to $n$ and this becomes the new value. Only makes sense in specific buffs and should really not be used except in some very specific situations
    - ```NOT``` negates the input. So that if $n \neq 0$ it becomes 0, and if $n = 0$, becomes 1
- ```Input```/```Output```
    - ```TEMP_VARIABLE``` the value of the temp variable of the current effect. *Read only*
    - ```ACC``` the **Accumulator**'s value in the effect processing CPU
    - ```TARGET_HP```/```TAGET_ATTACK```/```TARGET_MOVEMENT```/```TARGET_MOVEMENT_DENOMINATOR``` once target(s) have been found with the ```SELECT_ENTITY``` or ```FIND_ENTITIES``` operations, the total value of the stat
    - ```PLAYERS_GOLD``` once target(s) have been found with the ```SELECT_ENTITY``` or ```FIND_ENTITIES``` operations, the owner's gold of those targets
- ```MultiInputProcessing``` for when an Input is an element present in a reference list with many items (e.g. after search)
    - ```FIRST``` checks only the first element
    - ```COUNT``` doesn't check any element, just gives the number of elements on the list of references
    - ```SUM``` the total sum of all inputs
    - ```AVERAGE``` the average value of all inputs
    - ```MAX```/```MIN``` the maximum/minimum value of all inputs