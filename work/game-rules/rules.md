# Katan – Game Rules

> First English draft, translated from the French source (`jeu.odt`).
> Ambiguities and open questions are marked with ⚠️.

---

## Objective

The first player to reach **10 victory points** wins the game.

---

## Victory Points

| Source | Points |
|---|---|
| Settlement | 1 |
| City | 2 |
| Victory Point development card | 1 |
| Longest Road bonus | 2 |
| Largest Army bonus | 2 |

---

## Components

| Item | Quantity |
|---|---|
| Map tiles | 19 (4 forest, 3 quarry, 3 field, 3 pasture, 3 mountain, 2 mine, 1 desert) |
| Number tokens | 19 (values 2–12) |
| Settlements | 25 (5 per player) |
| Cities | 20 (4 per player) |
| Roads | 75 (15 per player) |
| Armies | 50 (10 per player) |
| Robber | 1 |
| Dice | 2 |
| Resource cards | (deck) |
| Development cards | (deck) |

---

## Tiles and Resources

Each tile produces a specific resource when its number is rolled.

| Tile | Resource |
|---|---|
| Forest | Wood |
| Quarry | Brick |
| Field | Wheat |
| Pasture | Sheep |
| Mountain | Stone |
| Mine | Iron |
| Desert | None |

---

## Setup

The game starts with the **tallest player** going first. Play proceeds **clockwise**.

> ⚠️ The draft does not describe the initial placement phase (placing starting settlements and roads before the first turn). This needs to be defined.

---

## Turn Structure

Each turn consists of the following steps, in order:

### 1. Roll the dice

The active player rolls 2 dice and sums the result.

#### Normal roll (2–6, 8–12)

Every tile whose number token matches the roll produces resources. Each player with a settlement or city on that tile receives:

- **1 resource** per settlement
- **2 resources** per city

The tile where the **robber** is present produces **no resources**, regardless of the roll.

#### Rolling a 7

1. Any player holding **more than 7 resource cards** must discard half their hand, rounded down.
2. The active player moves the **robber** to any tile of their choice (see [Robber](#robber)).

### 2. Trade (optional)

The active player may trade resources in any combination of the following:

- **Player trade**: negotiate freely with other players.
- **Bank trade**: exchange 4 identical resources for 1 resource of any type (4:1).
- **Port trade**: use a port adjacent to one of your settlements or cities (see [Ports](#ports)).

### 3. Build and buy (optional)

The active player may spend resources to build structures or buy development cards (see [Building Costs](#building-costs)).

### 4. End turn

The turn passes to the next player clockwise.

---

## Ports

To use a port, a player must have a settlement or city on a tile adjacent to that port.

| Port type | Rate |
|---|---|
| Generic port (3:1) | Trade 3 identical resources for 1 resource of any type |
| Specialized port (2:1) | Trade 2 resources of a specific type for 1 resource of any type |

> ⚠️ Which specific resource each specialized port accepts needs to be defined in the map layout.

---

## Building Costs

| Structure | Cost |
|---|---|
| Road | 1 Wood + 1 Brick |
| Settlement | 2 Wood + 1 Brick + 1 Wheat + 1 Sheep |
| City | 3 Stone + 2 Wheat |
| Army | 2 Iron + 1 Wheat |
| Development card | (cost not specified in draft) ⚠️ |

### Placement rules

**Settlements**
- Must be placed on a tile.
- Except for the very first settlement placed at the start of the game, a settlement must be connected to another of the player's settlements via a road.

> ⚠️ The draft does not mention the standard Catan distance rule (no two settlements adjacent). Needs clarification.

**Cities**
- Built by upgrading an existing settlement. The settlement is replaced by the city.

**Roads**
- Connect settlements and cities.

**Armies**
- Must be placed on a tile where the player already has a settlement or city.

---

## Development Cards

Development cards may be purchased during a player's turn.

> ⚠️ The draft states cards "must be used immediately." This means a card cannot be saved for a future turn. Confirm this is intentional — it differs significantly from standard rules.

### Card types

#### Knight *(11 cards)*

The player immediately moves the robber to any tile of their choice. They may steal one random resource card from a player who has a settlement or city on that tile.

#### Victory Point *(5 cards)*

Grants 1 victory point.

#### Road Building *(count unspecified ⚠️)*

Grants 1 free road, placed immediately.

#### Monopoly *(count unspecified ⚠️)*

The player names a resource type. Every other player must give them one card of that resource. Players who have none give nothing.

#### Catastrophe *(count unspecified ⚠️)*

The player chooses any tile on the map.
- If a settlement or city is on that tile, it is destroyed.
- All armies on that tile are halved, rounded down.

---

## Robber

The robber is moved whenever a player rolls a **7**, or when a **Knight** card is played.

- The tile occupied by the robber **produces no resources**.
- **When first moved onto a tile**: if another player has a settlement or city there, the player who moved the robber may steal one random resource card from them. This steal happens only once per placement.
- **Each subsequent turn**: as long as the robber remains on a tile, any player with a settlement or city on that tile must give up one resource of their choice at the start of their turn.

> ⚠️ The ongoing resource loss per turn is unusual. Confirm this is intentional and define the exact timing (start of turn? after dice roll?).

---

## Armies

### Building

An army is built by spending **2 Iron + 1 Wheat**. It must be placed on a tile where the player has a settlement or city.

### Movement

A player may move any number of their armies during their turn, before or after attacking. An army may move from one tile to an adjacent tile, provided the destination:

- belongs to the player, **or**
- contains one of the player's settlements or cities.

> ⚠️ "Belongs to the player" needs a precise definition — does this mean a tile the player has conquered?

### Combat

A player may attack an adjacent tile that contains enemy armies.

**Attack roll:**
- The attacker rolls up to **3 dice** (1 die per 2 attacking armies, rounded down).
- The defender rolls up to **2 dice** (1 die per 2 defending armies, rounded down).

> ⚠️ The comparison rule (how dice results are compared to determine casualties) is not described. Needs definition.

**If the attacker wins** (all defending armies are eliminated):
- The tile is conquered.
- If the tile had a settlement or city belonging to the defender, it becomes the attacker's.
- If the tile had no settlement or city, the attacker may build one there.
- The attacker must move at least 1 army onto the conquered tile.

**If the attack fails:**
- The attacker loses half their attacking armies, rounded down.

---

## Special Bonuses

Both bonuses are awarded **at the end of the game only**.

> ⚠️ The draft says bonuses are attributed only at end of game. Clarify whether they are tracked and displayed mid-game (for information) or calculated only when a player reaches 10 VP.

### Longest Road

The player with the longest continuous road receives **+2 victory points**.

### Largest Army

The player with the largest army (most army units) receives **+2 victory points**.

---

## End of Game

The game ends immediately when a player reaches **10 victory points** at any point during their turn (including after dice resolution, building, or playing a development card).
