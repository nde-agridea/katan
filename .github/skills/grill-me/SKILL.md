---
name: grill-me
description: Relentlessly interviews the user about a task through targeted questions until a shared, unambiguous understanding is reached. Use this skill when asked to "grill me", "interview me", or "clarify a task" before planning or implementation begins.
---

# Grill Me Skill

Your job is to interview the user about a task until you have a complete, unambiguous understanding of what needs to be done. Do not plan or implement anything yet. Ask questions only.

## Process

Work through the following dimensions one round at a time. Ask 2–4 focused questions per round. Wait for answers before continuing. Adapt follow-up questions based on what the user says.

### Round 1 – Intent and scope

Understand what the user wants and why.

- What is the task? What problem does it solve, or what value does it add?
- What is the expected outcome? What does "done" look like?
- What is explicitly out of scope?
- Is this a new feature, a change to existing behavior, or a bug fix?

### Round 2 – Domain and ubiquitous language

Ensure the task uses and respects the project's domain language. For the Katan project, this means the language of the Catan board game and the codebase conventions. Flag any term that is ambiguous, informal, or inconsistent with the existing codebase.

- Which domain concepts are involved (e.g. Player, Turn, Board, Settlement, Resource, Trade, Dice, Action)?
- Are any of the terms the user used informal or potentially ambiguous? If so, ask them to clarify using precise domain language.
- Are there existing types, services, or methods in the codebase that already represent these concepts? If you know of any, reference them by name and ask whether the task extends, replaces, or sits alongside them.

### Round 3 – Architecture and ownership

Ensure the task respects the architecture.

- Which layer(s) does this touch: `Katan.Server`, `Katan.Client`, `Katan.Shared`, or tests?
- Does any proposed behavior risk placing game logic in the client? If so, flag it.
- Are there gRPC message or service contract changes required?
- Does this affect game state? If so, is the server the authoritative owner of that state?

### Round 4 – Edge cases and constraints

Surface hidden complexity before it becomes a problem.

- What are the failure cases? What should happen when input is invalid or the action is not allowed?
- Are there race conditions, ordering constraints, or concurrency concerns?
- Are there known constraints (performance, backward compatibility, data migration)?
- Does this interact with any existing rules or win conditions?

### Round 5 – Validation

Check for internal consistency before closing.

- Summarize your understanding of the task back to the user in plain language.
- Explicitly call out any assumptions you are making.
- Ask: "Is there anything I've missed, misunderstood, or oversimplified?"

## Rules

- **Never skip a round** unless the user's previous answers have already fully resolved it.
- **Never assume** what the user means — always ask when ambiguous.
- **Challenge vague language.** If the user says "handle it", "support it", or "manage it", ask exactly what that means.
- **Cite the codebase.** If a term or concept conflicts with what already exists, name the conflict explicitly.
- **Do not suggest solutions** during the interview. Your only output is questions and summaries.
- **Do not stop early.** Keep asking until you can state the task precisely, with no open questions, and the user confirms your summary is correct.
