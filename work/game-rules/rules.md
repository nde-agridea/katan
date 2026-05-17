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
| Number tokens | 19 (values 2–6 and 8–12 for resource tiles; value 7 for the desert tile) |
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

## Map

The game map consists of **19 hexagonal tiles** arranged in a random shape that is generated freshly each game. Unlike a fixed arrangement, the map shape varies and must satisfy the constraints below.

### Shape constraints

- All 19 tiles must form **one contiguous connected group** — no isolated clusters.
- The map must contain **no enclosed holes** (no empty space fully surrounded by tiles).
- **No thin peninsula may exceed 2 tiles in length.** A thin peninsula is a chain where each interior tile has exactly 2 neighbors.

### Desert placement

The desert tile is placed after the overall shape is determined.

- The desert **must not be an edge tile**. An edge tile is any tile that has at least one exposed side (fewer than 6 neighbors within the map).
- In other words, the desert must have all 6 of its neighbors occupied by other tiles.

### Number token placement

- Each of the 18 non-desert tiles receives one number token from the set {2, 3, 4, 5, 6, 8, 9, 10, 11, 12}.
- The desert tile receives the **7 token**. This token is visual only: it produces no resources and grants no additional effect. The robber mechanic is triggered by any roll of 7 regardless of this token.
- **High-value adjacency rule**: no two tiles from the set {5, 6, 8, 9} may be adjacent to each other.

### Resource tile distribution

The 18 non-desert resource tiles are distributed randomly, subject to one constraint:

- **No three mutually adjacent tiles may all share the same resource type** (triangle constraint).

### Ports

The map has **8 ports**: 6 specialized ports (2:1, one per resource type) and 2 generic ports (3:1).

- Ports are placed on exposed edges of coastal tiles (tiles that have at least one exposed side).
- **No two ports may be placed on adjacent coastal edges.**
- The assignment of which specialized port corresponds to which resource is random.

### Map approval

After the server generates a map, all players are shown the full map before the initial placement phase begins.

- Players vote to accept or reject the map. A **majority vote** rejects the map and triggers a re-generation.
- The map may be re-generated at most **3 times**. After the third re-generation, the resulting map is used regardless of the vote.

---

## Setup

The game starts with the **tallest player** going first. Play proceeds **clockwise**.

### Initial placement phase

Before the first turn, each player places their starting pieces in clockwise order. When it is a player's turn to place:

1. Place **1 settlement** on any tile intersection.
2. Place **2 roads**, each connected to the player's settlement or road network.
3. Receive **1 resource card for each tile adjacent to the placed settlement** (desert tiles yield nothing).

All placements are **free** (no resource cost). There is **no distance rule** — a settlement may be placed adjacent to any other settlement.

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

There is one specialized port per resource type (6 in total) and 2 generic ports. Port positions and specialized port assignments are determined randomly during map generation (see [Map](#map)).

---

## Building Costs

| Structure | Cost |
|---|---|
| Road | 1 Wood + 1 Brick |
| Settlement | 2 Wood + 1 Brick + 1 Wheat + 1 Sheep |
| City | 3 Stone + 2 Wheat |
| Army | 2 Iron + 1 Wheat |
| Development card | 1 Stone + 1 Sheep + 1 Iron |

### Placement rules

**Settlements**
- Must be placed on a tile intersection.
- Except for the very first settlement placed at the start of the game, a settlement must be connected to another of the player's settlements via a road.

**Cities**
- Built by upgrading an existing settlement. The settlement is replaced by the city.

**Roads**
- Connect settlements and cities.

**Armies**
- Must be placed on a tile where the player already has a settlement or city.

---

## Development Cards

Development cards may be purchased during a player's turn. A purchased card must be used immediately on that same turn — it cannot be saved for a future turn.

### Card types

#### Knight *(11 cards)*

The player immediately moves the robber to any tile of their choice. They may steal one random resource card from a player who has a settlement or city on that tile.

#### Victory Point *(5 cards)*

Grants 1 victory point.

#### Road Building *(3 cards)*

Grants 1 free road, placed immediately.

#### Monopoly *(2 cards)*

The player names a resource type. Every other player must give them one card of that resource. Players who have none give nothing.

#### Excess *(3 cards)*

The player receives 2 resource cards of their choice from the bank.

#### Disaster *(1 card)*

The player chooses any tile on the map.
- If a settlement or city is on that tile, it is destroyed.
- All armies on that tile are halved, rounded down.

---

## Robber

The robber is moved whenever a player rolls a **7**, or when a **Knight** card is played.

- The tile occupied by the robber **produces no resources**.
- **When first moved onto a tile**: if another player has a settlement or city there, the player who moved the robber may steal one random resource card from them. This steal happens only once per placement.
- **Each subsequent turn**: as long as the robber remains on a tile, any player with a settlement or city on that tile must give up one resource of their choice **after the dice roll** on their turn.

---

## Armies

### Building

An army is built by spending **2 Iron + 1 Wheat**. It must be placed on a tile where the player has a settlement or city.

### Tile Ownership

Tiles start the game as **unclaimed**. A tile is claimed when a player first places an army on it — either by building there or by moving an army onto it. Once claimed, a tile remains owned and can never return to unclaimed. Ownership transfers to the victorious attacker after a successful battle.

### Movement

A player may move any number of their armies during their turn, before or after attacking. An army may move from one tile to an adjacent tile, provided the destination:

- is owned by the player, **or**
- contains one of the player's settlements or cities, **or**
- is unclaimed (moving onto it claims it).

### Combat

A player may attack an adjacent tile that contains enemy armies.

**Attack roll:**
- Each attacking and defending army contributes **1 die**.
- Both sides roll all their dice simultaneously and sum the results.
- The side with the higher total wins the exchange. **Ties go to the defender.**
- Combat is resolved in a **single round**.

**If the attacker wins** (all defending armies are eliminated):
- The tile is conquered.
- If the tile had a settlement or city belonging to the defender, it becomes the attacker's.
- If the tile had no settlement or city, the attacker may build one there.
- The attacker must move at least 1 army onto the conquered tile.

**If the attack fails:**
- The attacker loses half their attacking armies, rounded down.

---

## Special Bonuses

The current leader for each bonus is tracked and **displayed live during the game** for all players to see. The **+2 victory points are awarded only at the end of the game**, when a player reaches 10 VP.

### Longest Road

The player with the longest continuous road receives **+2 victory points**.

### Largest Army

The player with the largest army (most army units) receives **+2 victory points**.

---

## End of Game

The game ends immediately when a player reaches **10 victory points** at any point during their turn (including after dice resolution, building, or playing a development card).
