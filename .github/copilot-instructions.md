# Katan – Copilot Instructions

## Project Overview

Katan is a turn-based board game implemented in C#. It has two main components:

- **Server** – game rules, state management, and engine logic (no UI dependencies)
- **Client** – user interface for playing the game (UI tech TBD)

The server and client communicate over gRPC.

## Repository Structure (planned)

```
katan/
├── doc/
├── src/
│   ├── Katan.Server/       # ASP.NET Core Web API — game engine & rules
│   ├── Katan.Client/       # Client application
│   └── Katan.Shared/       # Shared models/DTOs used by both server and client
├── tests/
│   ├── Katan.Server.Tests/
│   └── Katan.Client.Tests/
├── work/
│   ├── work-piece/
│   └── another-work-piece/
│   └── (...)/
└── Katan.sln
```

- `src/` contains the main application code for server, client, and shared models.
- `tests/` contains unit and integration tests for both server and client.
- `work/` is where pieces of work (features, bug fixes, etc.) are planned before being implemented. Each piece of work gets its own folder with a README describing the task and any relevant details. The terms `work` and `piece` are voluntary vague to allow for any kind of task, from a small bug fix to a large feature. Planning agents can organize and describe work here.
- `doc/` is for any documentation related to the project, such as ARDs, design docs, etc.

## Build, Test & Run

```bash
# Build the solution
dotnet build

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~ClassName"

# Run a single test method
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"

# Run the server
dotnet run --project src/Katan.Server

# Run the client
dotnet run --project src/Katan.Client
```

## Architecture

### Server

The server is a pure game engine exposed via gRPC. It owns all game rules and enforces state transitions. Key responsibilities:

- Maintain authoritative game state
- Validate and apply player actions (moves, turns)
- Enforce turn order and win conditions
- Return updated game state to clients after each action

The server should have **no knowledge of the UI**. All game logic lives here.

### Client

The client is a stateless consumer of the server API. It is responsible for:

- Rendering the current game state received from the server
- Collecting player input and sending it as gRPC requests
- Displaying feedback from the server (validation errors, game events)

The client should contain **no game rules**. Any rule enforcement belongs in the server.

### Shared

DTOs and request/response models shared between server and client live in `Katan.Shared`. This project should contain no business logic — only data contracts.

## Internationalization (i18n)

Katan is designed to be played in multiple languages. **French is the first supported language.**

### Core principles

- **The codebase is English-only.** All identifiers, types, method names, comments, log messages, and domain concepts are written in English. No French (or other language) text ever appears in source code.
- **All player-facing strings are translated.** Any text shown to a player — UI labels, game event descriptions, error messages, card names, resource names, tile names — must go through the translation layer. Hard-coded display strings in code are not acceptable.
- **Translation is a first-class concern.** When designing a feature that produces player-facing output, defining the translation keys and their English and French values is part of the work, not an afterthought.
- **Domain model terms are English.** The ubiquitous language of the codebase uses English terms (e.g. `Settlement`, `Road`, `ResourceType.Wood`, `ActionType.BuildCity`). Translations map these concepts to display strings in each supported language — they do not influence the domain model.

### Translation approach

- Translation resources live in `Katan.Client` (display strings) and, where applicable, in `Katan.Shared` (string keys / enums that drive translation lookups).
- New player-facing strings must be accompanied by at least an English and a French translation.
- Translation keys should be descriptive and namespaced (e.g. `resource.wood`, `action.build_settlement`, `error.not_your_turn`).

### Rules documents

The canonical rules document is `work/game-rules/rules.md` (English). Each supported language has a corresponding translation:

- `work/game-rules/rules-fr.md` — French

**Whenever `rules.md` is modified, all translation files must be updated in the same change.** No rule addition, removal, or clarification may be merged into `rules.md` without a matching update to every translation. The translations must reflect the same content, structure, and ⚠️ ambiguity callouts as the English source.

## Key Conventions

- **Separation of concerns**: Game rules must not leak into the client. If you find yourself writing conditional game logic on the client, move it to the server.
- **State is server-side**: The server holds the single source of truth for game state. The client re-renders from whatever the server returns.
- **Action-based API**: Each player action sends an action message via gRPC and receives the new game state in return.
- **Immutable game state snapshots**: Prefer returning a full state snapshot per turn rather than incremental patches, to keep client logic simple.
- **xUnit** for tests; **FluentAssertions** for assertions (preferred but not required yet).
- **C# naming conventions**: PascalCase for types and members, camelCase for local variables, `_camelCase` for private fields.
