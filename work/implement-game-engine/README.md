# Implement Game Engine

Implement the core Katan game engine in `Katan.Server` using Domain-Driven Design. The engine enforces all game rules as described in `work/game-rules/rules.md`, exposes an action-based API over gRPC, and is developed strictly following a red-green-refactor TDD loop driven by user stories.

---

## Goals

- Model the game domain using DDD (aggregates, entities, value objects, domain events, bounded contexts) derived directly from the ubiquitous language of the rules.
- Cover every game mechanic with at least one user story and corresponding unit/integration test before writing production code.
- The server holds all game rules; no rule logic ever leaks into the client.
- Every player-visible outcome is expressed as a domain event so the client can react without knowing internal state.
- The engine passes all tests in CI before any gRPC wiring is added.

---

## Out of scope

- Client UI and rendering.
- gRPC transport layer (proto definitions and service wiring come after the domain is stable — see task 35+).
- Persistence / database (in-memory state only for now).
- Authentication and lobby management.
- AI / bot players.
- Internationalisation strings (translation keys are defined in `Katan.Shared`; actual string values are a separate work piece).

---

## Approach

### Projects affected

| Project | Role |
|---|---|
| `Katan.Server` | Domain model, game engine, application services |
| `Katan.Shared` | DTOs, enums, proto-generated types |
| `Katan.Server.Tests` | All unit and integration tests (xUnit + FluentAssertions) |

### DDD Bounded Contexts

The domain is split into the following bounded contexts, each owning its own folder under `Katan.Server/Domain/`:

| Context | Responsibility |
|---|---|
| **Board** | Map shape, tiles, intersections, edges, ports, robber, number tokens |
| **GameFlow** | Game lifecycle, turn state machine, phase transitions |
| **Resources** | Resource card hands, production, bank, trading |
| **Structures** | Settlement/city/road placement rules and piece limits |
| **Military** | Army placement, movement, tile ownership, combat |
| **Development** | Development card deck, draw, immediate play, effects |
| **Scoring** | Victory point tracking, Longest Road, Largest Army, win condition |

### Ubiquitous language (from rules)

**Value objects:** `ResourceType` (Wood, Brick, Wheat, Sheep, Stone, Iron), `TileType` (Forest, Quarry, Field, Pasture, Mountain, Mine, Desert), `TilePosition`, `Intersection`, `Edge`, `DiceResult`, `ResourceCost`.

**Entities:** `Tile`, `Port`, `Road`, `Settlement`, `City`, `Army`.

**Aggregates:** `Board` (map + robber), `Player` (hand + pieces + armies), `Game` (root aggregate — owns Board and Players, enforces turn order and win condition).

**Domain events:** `MapGenerated`, `MapVotecast`, `MapApproved`, `MapRejected`, `GameStarted`, `InitialSettlementPlaced`, `InitialRoadsPlaced`, `StartingResourcesGranted`, `TurnStarted`, `DiceRolled`, `ResourcesProduced`, `SevenRolled`, `ResourcesDiscarded`, `RobberMoved`, `RobberTributeCollected`, `TradeOffered`, `TradeAccepted`, `TradeDeclined`, `BankTradeExecuted`, `PortTradeExecuted`, `RoadBuilt`, `SettlementBuilt`, `CityUpgraded`, `ArmyBuilt`, `ArmyMoved`, `TileClaimed`, `CombatResolved`, `TileConquered`, `DevelopmentCardPurchased`, `DevelopmentCardPlayed`, `KnightPlayed`, `MonopolyPlayed`, `RoadBuildingPlayed`, `ExcessPlayed`, `DisasterPlayed`, `LongestRoadUpdated`, `LargestArmyUpdated`, `VictoryPointsUpdated`, `GameEnded`.

### TDD methodology

Every task below maps to one or more user stories. **Write the test first, see it fail, implement the minimum to pass, then refactor.**

Tests are organised by bounded context, mirroring the production folder structure:

```
tests/Katan.Server.Tests/
├── Board/
├── GameFlow/
├── Resources/
├── Structures/
├── Military/
├── Development/
└── Scoring/
```

---

## User stories

### Board / Map Generation

| # | User story |
|---|---|
| US-B1 | As the server, I generate a map of exactly 19 hex tiles forming one contiguous group with no holes and no thin peninsula longer than 2 tiles. |
| US-B2 | As the server, I place the desert tile on a non-edge tile (all 6 neighbours occupied). |
| US-B3 | As the server, I assign number tokens so the desert gets 7, and no two tiles from {5,6,8,9} are adjacent. |
| US-B4 | As the server, I distribute 18 resource tiles randomly with no three mutually adjacent tiles sharing the same resource type. |
| US-B5 | As the server, I place 8 ports (6 specialised + 2 generic) on coastal edges with no two adjacent ports. |
| US-B6 | As the server, after generating a map I present it to all players for a majority-vote approval. |
| US-B7 | As the server, I re-generate the map up to 3 times on majority rejection; the 4th map is always used. |

### Setup / Initial Placement

| # | User story |
|---|---|
| US-S1 | As the server, I let each player place 1 settlement (free, no distance rule) and 2 roads during the initial placement phase. |
| US-S2 | As the server, after each initial settlement placement I grant the player 1 resource per adjacent non-desert tile. |

### Turn Flow

| # | User story |
|---|---|
| US-T1 | As the active player, I roll 2 dice to start my turn; all subsequent actions are gated until the roll is done. |
| US-T2 | As the server, on a normal roll (2–6, 8–12) I distribute resources to every player with a settlement (×1) or city (×2) on a matching tile, skipping the robber tile. |
| US-T3 | As the server, on a roll of 7 I require every player holding more than 7 resource cards to discard half (rounded down) before the active player moves the robber. |
| US-T4 | As the active player, after rolling 7 I choose a tile to place the robber on. |
| US-T5 | As the server, I enforce that each turn phase (Roll → Trade → Build → End) occurs in order and that actions outside the current phase are rejected. |
| US-T6 | As the active player, I end my turn and the next player clockwise becomes active. |

### Robber

| # | User story |
|---|---|
| US-R1 | As the server, when the robber is first moved onto a tile I let the moving player steal 1 random resource from any opponent with a settlement or city there. |
| US-R2 | As the server, on subsequent turns while the robber is on a tile, any player with a settlement or city there must give up 1 resource of their choice after the dice roll. |

### Trading

| # | User story |
|---|---|
| US-TR1 | As the active player, I trade 4 identical resources for 1 resource of any type with the bank (4:1). |
| US-TR2 | As the active player, I use a generic port (3:1) to trade 3 identical resources for 1 of any type if I have a settlement/city on an adjacent tile. |
| US-TR3 | As the active player, I use a specialised port (2:1) to trade 2 resources of the matching type for 1 of any type. |
| US-TR4 | As the active player, I offer a trade to one or more other players; they may accept or decline. |

### Building

| # | User story |
|---|---|
| US-BU1 | As the active player, I spend 1 Wood + 1 Brick to build a road connected to my existing road/settlement network. |
| US-BU2 | As the active player, I spend 2 Wood + 1 Brick + 1 Wheat + 1 Sheep to build a settlement connected to my road network. |
| US-BU3 | As the active player, I spend 3 Stone + 2 Wheat to upgrade one of my settlements to a city. |
| US-BU4 | As the active player, I spend 2 Iron + 1 Wheat to build an army on a tile where I have a settlement or city. |
| US-BU5 | As the server, I reject any build action that exceeds the player's piece supply (5 settlements, 4 cities, 15 roads per player). |

### Military

| # | User story |
|---|---|
| US-M1 | As the active player, I move any of my armies to adjacent tiles I own, own a settlement/city on, or unclaimed tiles. |
| US-M2 | As the server, moving an army onto an unclaimed tile claims it for that player. |
| US-M3 | As the active player, I attack an adjacent tile containing enemy armies; both sides roll dice equal to their army count and sum the results; ties go to defender. |
| US-M4 | As the server, if the attacker wins I conquer the tile: ownership transfers, defender structures become attacker structures, attacker must move at least 1 army onto the tile. |
| US-M5 | As the server, if the attack fails the attacker loses half their attacking armies (rounded down). |

### Development Cards

| # | User story |
|---|---|
| US-D1 | As the active player, I spend 1 Stone + 1 Sheep + 1 Iron to draw and immediately play a development card. |
| US-D2 | As the active player, I play a Knight card: the robber moves and I may steal 1 resource from an opponent on that tile. |
| US-D3 | As the active player, I play a Victory Point card and immediately gain 1 VP. |
| US-D4 | As the active player, I play a Road Building card and immediately place 1 free road. |
| US-D5 | As the active player, I play a Monopoly card naming a resource type; all other players give me 1 card of that resource. |
| US-D6 | As the active player, I play an Excess card and take 2 resource cards of my choice from the bank. |
| US-D7 | As the active player, I play a Disaster card choosing a tile: a settlement or city on it is destroyed, and armies are halved. |

### Scoring & Win Condition

| # | User story |
|---|---|
| US-SC1 | As the server, I award 1 VP per settlement and 2 VP per city for each player. |
| US-SC2 | As the server, I track the player with the longest continuous road and update the Longest Road bonus live; +2 VP are awarded only at game end when a player hits 10 VP. |
| US-SC3 | As the server, I track the player with the most army units and update the Largest Army bonus live; +2 VP are awarded only at game end. |
| US-SC4 | As the server, the game ends immediately when a player reaches 10 VP at any point during their turn. |

---

## Tasks

Tasks are ordered by dependency. Each task is a red-green-refactor TDD cycle.

### Phase 1 – Domain model foundations

1. **[Board]** Define value objects: `ResourceType`, `TileType`, `TilePosition`, `Intersection`, `Edge`, `DiceResult`, `ResourceCost`.
2. **[Board]** Define `Tile` entity (type, number token, position, occupying armies, settlement/city slot).
3. **[Board]** Define `Port` entity (edge, port type, associated resource type).
4. **[Board]** Define `Board` aggregate (tile collection, robber position, port collection, intersection/edge graph).
5. **[Resources]** Define `ResourceHand` value object (counts per resource type, add/remove/validate operations).
6. **[Structures]** Define `Settlement`, `City`, `Road` entities.
7. **[Military]** Define `Army` entity (owner, tile).
8. **[Development]** Define `DevelopmentCard` types and `DevelopmentCardDeck` entity.
9. **[Scoring]** Define `VictoryPointLedger` value object.
10. **[GameFlow]** Define `Player` entity (hand, pieces, armies, VP ledger).
11. **[GameFlow]** Define `Game` aggregate root (players, board, turn order, active player, phase, domain event list).

### Phase 2 – Map generation (US-B1 – US-B5)

12. **[Board]** Implement hex grid adjacency model (6-neighbour topology for `TilePosition`).
13. **[Board]** Implement map shape generator: 19 connected tiles, no holes, no thin peninsula > 2 tiles.
14. **[Board]** Implement desert placement validator: desert must have 6 neighbours.
15. **[Board]** Implement number token assignment: desert → 7; no adjacent {5,6,8,9}; high-value adjacency rule enforced.
16. **[Board]** Implement resource tile distribution: no three mutually adjacent tiles with same resource (triangle constraint).
17. **[Board]** Implement port placement: 8 ports on coastal edges, no two adjacent.

### Phase 3 – Map voting (US-B6 – US-B7)

18. **[GameFlow]** Implement `MapVotingSession`: collect votes, evaluate majority, trigger re-generation or approval; hard-stop after 3 rejections.

### Phase 4 – Initial placement (US-S1 – US-S2)

19. **[GameFlow]** Implement initial placement phase: order, settlement placement (no distance rule), 2 roads, starting resource grant.

### Phase 5 – Turn state machine (US-T1 – US-T6)

20. **[GameFlow]** Implement `TurnStateMachine` with phases: `WaitingForRoll → Trade → Build → End`.
21. **[GameFlow]** Implement dice roll: generate `DiceResult`, emit `DiceRolled` event, gate all other actions until rolled.
22. **[Resources]** Implement resource production on normal roll: match tile tokens, skip robber tile, grant per settlement/city. (US-T2)
23. **[Resources]** Implement roll-7 discard: identify players with > 7 cards, require discard of ⌊n/2⌋. (US-T3)
24. **[Board]** Implement robber movement on roll-7: active player moves robber, emit `RobberMoved`. (US-T4)
25. **[GameFlow]** Implement turn end and clockwise player rotation. (US-T6)

### Phase 6 – Robber ongoing effects (US-R1 – US-R2)

26. **[Board]** Implement initial-placement steal: one random resource from any opponent on the robber's new tile.
27. **[Board]** Implement per-turn tribute: every player with structure on robber tile pays 1 resource after dice roll.

### Phase 7 – Trading (US-TR1 – US-TR4)

28. **[Resources]** Implement bank trade (4:1). (US-TR1)
29. **[Resources]** Implement generic port trade (3:1) and specialised port trade (2:1) with adjacency check. (US-TR2, US-TR3)
30. **[Resources]** Implement player-to-player trade offer/accept/decline flow. (US-TR4)

### Phase 8 – Building actions (US-BU1 – US-BU5)

31. **[Structures]** Implement road placement (cost, connectivity, piece-limit enforcement). (US-BU1, US-BU5)
32. **[Structures]** Implement settlement placement (cost, road-connectivity rule, piece limit; no cost/no distance rule for initial placement). (US-BU2, US-BU5)
33. **[Structures]** Implement city upgrade (cost, requires existing settlement, piece limit). (US-BU3, US-BU5)
34. **[Military]** Implement army building (cost, must be on player-owned tile or player structure tile). (US-BU4)

### Phase 9 – Military (US-M1 – US-M5)

35. **[Military]** Implement army movement rules (owned tile, settlement/city tile, or unclaimed tile; claims on entry). (US-M1, US-M2)
36. **[Military]** Implement combat resolution: attacker and defender each roll n dice (n = army count), sum, attacker wins on higher total, tie → defender wins. (US-M3)
37. **[Military]** Implement attacker-wins outcome: tile conquered, structures transferred, attacker moves at least 1 army. (US-M4)
38. **[Military]** Implement failed-attack outcome: attacker loses ⌊attacking_armies/2⌋. (US-M5)

### Phase 10 – Development cards (US-D1 – US-D7)

39. **[Development]** Implement development card purchase (cost, draw from shuffled deck, immediate play requirement). (US-D1)
40. **[Development]** Implement Knight card: move robber, optional steal. (US-D2)
41. **[Development]** Implement Victory Point card: +1 VP immediately. (US-D3)
42. **[Development]** Implement Road Building card: 1 free road placed immediately. (US-D4)
43. **[Development]** Implement Monopoly card: all opponents give 1 of named resource. (US-D5)
44. **[Development]** Implement Excess card: player takes 2 resources of choice from bank. (US-D6)
45. **[Development]** Implement Disaster card: destroy settlement/city, halve armies on chosen tile. (US-D7)

### Phase 11 – Scoring & win condition (US-SC1 – US-SC4)

46. **[Scoring]** Implement VP calculation: 1 per settlement, 2 per city. (US-SC1)
47. **[Scoring]** Implement Longest Road tracker: continuous road algorithm, live bonus leader tracking, +2 VP at game end. (US-SC2)
48. **[Scoring]** Implement Largest Army tracker: army-count comparison, live leader tracking, +2 VP at game end. (US-SC3)
49. **[Scoring]** Implement win condition check after every state-changing action; emit `GameEnded` when a player hits 10 VP. (US-SC4)

### Phase 12 – gRPC wiring

50. **[Katan.Shared]** Define `.proto` file: `GameService` with RPCs for every player action; `GameState` message as the canonical snapshot returned after each action.
51. **[Katan.Server]** Implement `GameService` gRPC handlers mapping gRPC requests to domain commands and returning `GameState` snapshots.
52. **[Katan.Server]** Implement `GameStateMapper`: translate `Game` aggregate state + domain events into the `GameState` proto message.

---

## Open questions

1. **Map shape algorithm** – Which algorithm to use for generating the 19-tile shape (random walk, flood-fill, constraint solver)? A constraint solver is most correct but may be slow; a retry-based random walk may be acceptable.
2. **Longest Road algorithm** – The continuous road calculation is non-trivial when roads branch. Clarify whether branching roads count only the longest single path, and confirm whether opponent roads break the chain.
3. **Tile claiming ambiguity** – The rules say a tile is claimed by the first player to place an army there. Does moving a settlement onto a tile via `City upgrade` constitute a claim? (Rules are silent.)
4. **Robber tribute timing** – The rules say tribute is paid "after the dice roll" on subsequent turns. Does this happen before or after normal resource production?
5. **Development card deck size** – The rules give counts (Knight ×11, VP ×5, Road Building ×3, Monopoly ×2, Excess ×3, Disaster ×1 = 25 total). Should the deck be refilled if exhausted, or is purchasing blocked?
6. **Player trade during non-active turns** – The rules allow trade only during the active player's turn. Can non-active players initiate counter-offers during the opponent's trade phase?
7. **Initial placement order** – The rules say players place in clockwise order. Do all players get one round, or does the placement alternate (e.g. ABCD then DCBA as in classic Catan)?
8. **Army piece limit** – The rules list 10 armies per player but don't state it as a hard limit. Confirm whether building is blocked at 10.
