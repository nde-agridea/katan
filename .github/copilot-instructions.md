# Katan – Copilot Instructions

## Project Overview

Katan is a turn-based board game implemented in C#. It has two main components:

- **Server** – game rules, state management, and engine logic (no UI dependencies)
- **Client** – user interface for playing the game (UI tech TBD)

The server and client communicate over REST/HTTP.

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

The server is a pure game engine exposed via REST API. It owns all game rules and enforces state transitions. Key responsibilities:

- Maintain authoritative game state
- Validate and apply player actions (moves, turns)
- Enforce turn order and win conditions
- Return updated game state to clients after each action

The server should have **no knowledge of the UI**. All game logic lives here.

### Client

The client is a stateless consumer of the server API. It is responsible for:

- Rendering the current game state received from the server
- Collecting player input and sending it as HTTP requests
- Displaying feedback from the server (validation errors, game events)

The client should contain **no game rules**. Any rule enforcement belongs in the server.

### Shared

DTOs and request/response models shared between server and client live in `Katan.Shared`. This project should contain no business logic — only data contracts.

## Key Conventions

- **Separation of concerns**: Game rules must not leak into the client. If you find yourself writing conditional game logic on the client, move it to the server.
- **State is server-side**: The server holds the single source of truth for game state. The client re-renders from whatever the server returns.
- **Action-based API**: Each player action (e.g., `POST /games/{id}/actions`) sends an action object and receives the new game state in return.
- **Immutable game state snapshots**: Prefer returning a full state snapshot per turn rather than incremental patches, to keep client logic simple.
- **xUnit** for tests; **FluentAssertions** for assertions (preferred but not required yet).
- **C# naming conventions**: PascalCase for types and members, camelCase for local variables, `_camelCase` for private fields.
