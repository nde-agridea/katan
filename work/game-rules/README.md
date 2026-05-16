# Game Rules

## Summary

Formalize the rules of Katan as a reference document that will drive server-side game logic implementation. The source material is a French first draft (`rules.md` in this directory). This work piece produces the canonical English rules document that the rest of the codebase must conform to.

## Goals

- Produce a complete, unambiguous English rules reference (`rules.md`)
- Establish the ubiquitous language for the domain (tile, settlement, city, road, army, robber, etc.)
- Cover all game phases: setup, turn structure, building, trading, combat, special bonuses, and end condition
- Identify any rules gaps or ambiguities that need resolution before implementation

## Out of scope

- Implementation of any game logic
- gRPC service or message definitions
- UI or client concerns

## Approach

The rules are translated and structured from the French draft. Any ambiguity in the source material is flagged with a `> ⚠️` callout in `rules.md` for later resolution.

## Tasks

1. Review and refine `rules.md` to resolve flagged ambiguities
2. Use `rules.md` as the authoritative reference when implementing server-side game rules
3. Keep `rules.md` updated if rules change during implementation

## Open questions

- Development cards: the draft lists them without clear separation — are Road Building, Monopoly, and Catastrophe distinct named cards, or house rules?
- Robber ongoing penalty: the draft says a player loses 1 resource per turn while the robber is on their tile — is this intentional and how does it interact with the Knight card?
- Longest Road and Largest Army: the draft says bonuses are attributed "only at the end of the game" — does this mean they are not tracked mid-game?
- Army placement: must an army be placed on a tile with a settlement or city at build time, or can it be placed on any owned tile?
