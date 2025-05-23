# Concept

This is a battle between two armies. Each player has **20 health points (HP)**.
The player who reaches 0HP loses the game.

Players take turn, playing **Units**, **Skills** and **Buildings** to try and defeat their opponent.

There are 3 **lanes** a player can take to their enemies territory:
The **plains**, the **forest** and the **mountains** (each one is longer, respectively).

Players have **gold** as a resource, which they use to play stuff. Players start the game with 5 gold each.

![](./../Pictures/Board/BoardElements.png)
![](./../Pictures/Board/BoardElementsExplained.png)

In their turn, players can choose to:
- Play a card, which will summon a **Unit**, **Building** or **Skill**. The card is discarded and goes to the discard pile after playing
- Use the **RUSH** ability (explained below)
- Pass their turn

# Units

![](./../Pictures/Icons/BaseCard.png)

Units are played by paying their gold cost (top left of the card). The units stats are:
- ![](./../Pictures/Icons/HP.png) **Health:** Damage that the unit can take. When it reaches 0, unit dies.
- ![](./../Pictures/Icons/Movement.png) **Movement:** How many **tiles** the unit advances when advancing.
- ![](./../Pictures/Icons/Attack.png) **Attack:** Damage that the unit does to enemies when **advancing**.

When a unit is played, the player chooses in which **lane** to place it (plains, forest or mountains).
Unit is then placed on the first square of that **lane**.

# Advance

Units **advance** as the result of a **advance** effect (caused by the effects of some cards, etc).
When advancing, the unit will move to the next tile by their respective **movement** stats.

![](./../Pictures/Icons/March1.png)

If unit shares the **tile** with an enemy unit, the advancing unit instead will attack it.
Both units will receive HP damage "simultaneously" depending on the enemies attack.

![](./../Pictures/Icons/March2.png)

![](./../Pictures/Icons/March3.png)

![](./../Pictures/Icons/March4.png)

- If a combat between units occur, the unit finishes their advance, regardless of outcome of battle and the movement stat of the unit.
- If unit is in the last **tile** (in front of the opponent) it will instead damage the opposing player when advancing, but doesn't advance further, this can only happen once per advance step regardless of the movement stat.
- If multiple defending units are in a **tile**, the first unit spawned is the one targeted for the attack.
- For most purposes, units are damaged simultaneously, and may even die at the same time. However in the fine grain, the attacker (marching unit) will attack and deal damage (and possibly kill) first, and the defender will deal damage (and possibly kill) after. For example, cards that activate when damage is dealt may trigger their effects mid combat.

# Skills

A type of card that casts a spell or skill that affects the board in some way.
Similar to units, it has a gold cost.
For example, the **RUSH** skill summons **peasants** in every lane and is available every turn for every player (only once per turn).
It is the most basic skill of the game.
Other skill cards can be added to a player's deck and used.

![](./../Pictures/BaseSet/1.png)
![](./../Pictures/BaseSet/2.png)

# Buildings

Buildings are a mix between skills and units.
They need to be **constructed** by a unit. For this purpose they contain a blueprint, which specifies in which **tiles** it can be built.
Once built, the building remains in that **tile** until destroyed.

![](./../Pictures/BaseSet/3.png)

- The player chooses in which **lane** the building will be constructed
- For a building to be built, an unit needs to be present on at least one of the **tiles** indicated in the blueprint
- If there are units in multiple of those possible slots, the building will be placed in the first available number
- When an enemy enters a **tile** containing a building, the building receives damage equal to the unit's attack. (The **tile** will still be occupied by the enemy)
- Only one building per **tile** allowed

# Classes

Each player can choose a class (e.g. warrior, mage, astronauts, dinosaurs, whatever).
Each class has different cards with different game styles, but can also use the **base set** "medieval" cards (e.g workshops) allowed to all classes.
- When building a deck, a player has to choose a class, and a deck with a total of 30 cards, using both cards from the class chosen and the **base** cards.
- Card with rarity 1*, 2* and 3* can only have 3, 2, 1 copies in a deck respectively
- Cards with rarity 0* can't be put in a deck. They consist of cards that are auto-generated by other cards effects
- Each class also has a passive ability that give them advantages on certain situations or change the style of the class

# Game Structure

When the game begins:
- Each player starts with **4 cards and 5 gold**

Each turn there's the following phases:
- Beggining of turn:
    - First, all of the units from that player **advance**, in order they were played
    - Player then **draws a card**
    - Also, player **gets 2 gold**
- Play phase: Player can use their gold to play units, skills, buildings or the **RUSH** skill (**RUSH** only once per turn)
- End of turn: Player ends their turn
- Beginnint of turn for the next player

Game continues until one player loses

- If a player runs out of cards in their deck, the player loses 5HP at the beginning of each turn.

# Complex Rules
## Fractional Movement

A unit can have a **movement** expressed in a fraction (e.g. X/Y).
This means that the unit will advance X spaces every Y turns (i.e. waits a number of turns until can move again).
This creates slow units or units with an effectively non-integer movement stat.

![](./../Pictures/BaseSet/13.png)