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
    - ```TargetOptions:``` Where the card can be targeted when playing. Collection containing the valid targeting options for example ```[0, 1, 2]```. Units target by tiles (relative to the player), and Buildings target by units but use ```TargetOptions``` as the Blueprint locations (also relative).

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

## Skills
Skills can be targeted absolutely anywhere unlike the other cards. For this reason they have the following fields:
- ```TargetType``` determines what targeting this skill has. Works in tandem with ```TargetOptions``` list.
    - ```BOARD``` targets the board, and so only has value ```0``` as target
    - ```LANE``` can target one of the three lanes with options ```0,1,2``` respectively
    - ```TILE```/```TILE_RELATIVE``` targets tiles ```0-17```. Use the relative one if the spell has a complex or non-symmetrical blueprint, but the other one is slightly more efficient as it treates tiles as absolute.
    - ```UNIT```/```UNIT_RELATIVE```/```BUILDING``` targets units and buildings on the board but ```TargetOptions``` determines the valid possible tiles. Use relative to make the blueprint relative to a player, otherwise the absolutes are faster.
- ```TargetOwner```, for ```UNIT```/```UNIT_RELATIVE```/```BUILDING```, allows to use the ```OWNER```/```OPPONENT```/```BOTH``` flags to choose the owner(s) of entities targeted

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

- **Interactions:** Effects that are activated when something happens with a card. For example, a card that does something when taking damage, or when played.
Skills will have interactions only as they don't persist after it's effect resolves.
These are simple to describe in any card, as a Dictionary called ```Interactions``` where the Key is the **"Interaction Type"** (options described below) and then a sequence (list) of effects in order.
- **Triggers:** These are more complex as the entity needs to "subscribe" to a specific trigger in a specific location, because they trigger when something happens globally, outside of the card's control.
For example cards that are designed as "At the end of turn do X", or "When a player does X, this card does Y".
However triggers are also used when entities have location-based buffing effects or similar, as the buff has to be triggered every time a separate entity enters/exists the place.
These are described weirdly, as entities that can have Triggers (all but spells) describe it as a Dictionary called ```Triggers``` where the keys are the **"Effect Location"** where the trigger(s) will be attached (can be either absolute places or relative, position-based ones, options seen in ```EffectLocation``` enum), and the values are *another* dictionary this time with all the possible **Trigger Types** (optios below) and then the sequence (list) of effects. Basically they're similar to interactions but with an extra layer as they're subscribed to a specific location in the game.
Units do not trigger themselves if the current actor of a trigger, this is to avoid effect duplication with some interactions.

As you can see this is the more complex property of cards, so I really suggest taking a look at the ```.jsons``` and avoid doing these manually and just go through the Card Generator app if need to add or edit.

## Interaction Types

- ```WHEN_PLAYED``` Will be executed when the card is played (FROM HAND) for the first time. **Actor:** The card played
- ```UNIT_ENTERS_BUILDING``` Is executed when a unit enters a building (either when summoned on top or passing during march). This interaction happens twice from both POVs.
**Actor:** The unit entering, **Affected:** The building entered
- ```UNIT_CONSTRUCTS_BUILDING``` Is executed when a unit constructs a building. This interaction happens This interaction happens twice from both POVs.
**Actor:** The unit constructing, **Affected:** The building constructed
- ```PRE_DAMAGE``` Executed right before a damage step is applied.
**Actor:** The entity about to cause the damage, **Affected:** The entity about to receive the damage
- ```POST_DAMAGE``` Executed right after a damage step is applied.
**Actor:** The entity that caused the damage, **Affected:** The entity that received the damage

## Trigger Types

As the triggers happen when "another entity" is doing something, Actor/Affected is likely to be a different entity to the Activated Trigger entity, although it may also be the same one.

- ```ON_MARCH``` Triggers (currently only in **Tiles**) when a Unit marches in that tile. Specifically, it triggers when unit is about to move to escape the current tile.
**ACTOR:** The unit marching.
- ```ON_END_OF_TURN``` Triggers (currently only in **Board**) a player ends their turn.
**ACTOR:** The player ending their turn.

# Effect Mechanism

In order to be able to create complex card behaviour, an effect chain behaves similarly to a low-level CPU.
In this scheme, each ```EffectType``` is like an instruction, some of them can have parameters.
In order to implement complex effects that need to remember previous results (think effects such as *"Damage all enemy buildings, if any is killed, deal 2 damage to enemy player"* or *"Get 1 gold for each unit on the field"*), the effect chain contains a series of **Register Variables** that can be used as input and/or outputs:

- ```TEMP_VARIABLE```, is volatile and only remembered during the current effect of the chain. However it contains the "value" of many card effects (e.g. if an effect was *"Gain 2 gold"*, the temp variable would contain the value 2 in this case). This value can be used as a parameter in effects and in arithmetic operations
- ```ACC``` is the **"accumulator"** variable, and retains its value during the effect resolution chain, making it useful as the target of more complex math operations.
It is initialised with a value of 0 at the beginning of the effect chain
- ***Multiple other variables*** are also usable as both input and outputs, both to "check" ingame values (e.g. if you want to check if a target has X health or whatever), and some can also be the Output target of operations (e.g. if you want an effect that *sets* a target health)

For a list of posible ```Input```/```Output``` options, check below. 

Finally, the CPU context also has the following lists:
    - Reference entities, obtained by specific search/select operations
    - Reference locations obtained by assembling them during effect using specific operations
These references are important to track ownership, select targets, and other cool stuff.
By default the reference entity is the card with the ongoing effect, but some effects can change and override them.

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
    
    Parameters:
    - ```TargetPlayer``` serves as a filter where you only get the entities of the player owner in question (w.r.t. the card effect)
    - ```TargetType``` the type of entities that can be targeted 
    - ```SearchCriterion``` determines which target(s) can be found as valid targets. Some search criterions use a value $n$ as an input
    - The ```Input``` is used alongside some ```SearchCriterion``` cases as the value $n$. Negative values imply the search is done in reverse order

    Keep in mind that, no matter the ```EffectLocation```, the order of valid targets will be: ***[PLAYER]->[LOCATION]->[PLAYER]*** where which player is first is determined by the sign of $n$.
    In case of multiple entities in the same position, the unit that was played first is targeted first.

- ```ADD_LOCATION_REFERENCE``` Is a task that finds one or more target locations for future operations.
These can be used in subsequent effects like "search units" or "summon units".
    
    Parameters:
    - ```EffectLocation``` what location to add. Keep in mind some absolute locations (such as "Forest" or "Play Target") will add a single location, but locations relative to entities such as "Current Tile" will add one for each entity in the Reference Entities list

- ```SUMMON_UNIT``` Summons a unit in place(s) defined by the Location References. Multiple units may be summoned if there's multiple references.

    Parameters:
    - ```TargetPlayer``` is the player who will own the unit (relative to card effect)
    - ```Input``` will contain the card number of the unit summoned

- ```MODIFIER``` A mathematical operation, where an Output is changed, using the Input and an operation.
In some modifiers, the modification target is dependent on units found via a ```FIND_ENTITIES``` or ```SELECT_ENTITY``` operation.
For example, if stats need to be buffed/debuffed, or a specific player gets gold.

    Parameters:
    - ```ModifierOperation``` how the modifier's value is applied (i.e. whether it's a multiplaction, addition, etc)
    - ```Input``` contains the value $n$ of the modifier, needed for some (most?) operations
    - ```Output``` defines *what* is modified, if a stat, a damage value, etc
    - ```TargetPlayer``` in some cases where you want to modify a player's value (e.g. gold), this field is used to choose whether it's a card's owner or the opponents, whose value can be modified

- ```ASSERT``` is a mathematical operation that checks if the desired input value $\neq 0$. If the assert succeeds, nothing happens, but if it fails, the effect loop ends right there, and the following effects won't be executed.
Useful for effect with complex conditions where a part of the effect is conditional on something happening.

    Parameters:
    - ```Input``` contains the value $n$ to assert
    - ```ModifierOperation``` can be used but it only checks the ```NOT``` operation, in which case the asser asserts $\neq 0$. Other options just assert $=0$

- ```ASSERT_ROLE``` is like assert, ensures the entity currently activating it's effect it's a specific role. For example if in combat, ensure it's only activated when it is the unit dealing the damage. This is because many interactions can happen from both POVs, both for actor and affected.

    Parameters:
    - ```SearchCriterion``` determines which entity will be asserted
    - ```ModifierOperation``` can be used but it only checks the ```NOT``` operation, in which case the asser asserts $\neq$. Other options just assert $=$

- ```KILL_ENTITIES``` insta-kills (no damage step) each of the references on the current reference list

- ```EFFECT_DAMAGE``` deals effect damage to the reference targets. Similar to combat output but processed on a different place. There's not Defender attack, even if damage is dealt by unit.

    Parameters:
    - ```Input``` contains the value of the damage

- ```CARD_DRAW``` Makes players draw cards. Uses the Reference Entities to draw a number of cards for each one, works similarly to the "gold gaining" effect in ```MODIFIER```.

    Parameters:
    - ```Input``` how many cards will be drawn
    - ```TargetPlayer``` whether the owner of reference entity and/or its opponent will get the card

- ```MARCH_ENTITIES``` forces all Reference Entities to march.

## Possible Parameter Values

- ```EffectLocation```
    - ```BOARD``` will target the board
    - ```PLAINS```/```FOREST```/```MOUNTAIN``` wil target these lanes specifically
    - ```PLAY_TARGET``` the play target of the card just played is used, whatever that was. Only makes sense in cards with effects ```WHEN_PLAYED```. Searches in Board/Tile/Lane depending what the card has as "play target". Unit/Building targeting use the tile of the entity targeted.
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
    - ```PLAY_TARGET_ENTITY``` for cards that target a Unit/Building (e.g. all buildings and many spells) when played, get it here 

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
    - ```TARGET_COUNT``` the count of the number of targets found in a select/search operation. *Readonly value*
    - ```TARGET_HP```/```TAGET_ATTACK```/```TARGET_MOVEMENT```/```TARGET_MOVEMENT_DENOMINATOR``` once target(s) have been found with the ```SELECT_ENTITY``` or ```FIND_ENTITIES``` operations, the total value of the stat
    - ```PLAYERS_GOLD``` once target(s) have been found with the ```SELECT_ENTITY``` or ```FIND_ENTITIES``` operations, the owner's gold of those targets
    - ```MARCH_START_FLAG``` when on a marching context, this is a *readonly* flag that is $\neq 0$ if this is the first advancement of the current march, and $=0$ if not
    - ```MARCH_CURRENT_MOVEMENT``` when on a marching context, this is the number of remaining steps of the current march.
    - ```DAMAGE_AMOUNT``` the value of damage that will be caused or has been caused in a ```POST_DAMAGE``` or ```PRE_DAMAGE``` step.
- ```MultiInputProcessing``` for when an Input is an element present in a reference list with many items (e.g. after search)
    - ```FIRST``` the input of the first entity
    - ```SUM``` the total sum of all inputs
    - ```AVERAGE``` the average value of all inputs
    - ```MAX```/```MIN``` the maximum/minimum value of all inputs
    - ```EACH``` for some entity-related operations like buffing can use this to buff each entity individually calculating a separate input value each time