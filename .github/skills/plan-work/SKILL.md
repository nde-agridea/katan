---
name: plan-work
description: Plan and organize work pieces in the `work/` directory. Use this skill when asked to plan a feature, bug fix, task, or any piece of work for the Katan project.
---

# Plan Work Skill

When asked to plan a piece of work, create a structured entry under the `work/` directory at the root of the repository.

## Directory structure

Each piece of work gets its own subdirectory under `work/`. The name should be a short, descriptive, lowercase kebab-case identifier (e.g., `work/add-dice-roll`, `work/fix-resource-trading`).

```
work/
└── <work-piece-name>/
    └── README.md
```

## README.md contents

The `README.md` for each work piece must include the following sections:

### Title and summary

A short, human-readable title followed by a one-paragraph summary of what the work is and why it is needed.

### Goals

A bullet list of clear, specific goals the work piece must achieve. Each goal should be independently verifiable.

### Out of scope

A bullet list of things that are explicitly NOT part of this work piece. This prevents scope creep.

### Approach

A description of the proposed implementation approach. Include:
- Which projects are affected (`Katan.Server`, `Katan.Client`, `Katan.Shared`, tests)
- Key design decisions and rationale
- Any gRPC service or message changes required
- New or modified domain concepts

### Tasks

A numbered list of concrete implementation tasks, ordered by dependency. Each task should be small enough to implement and test independently.

### Open questions

Any unresolved decisions or unknowns that need clarification before or during implementation.

## Guidelines

- Keep work pieces focused. If a request is large, break it into multiple work pieces.
- Work pieces should be written from the perspective of a developer who will implement them — be specific and actionable.
- Do not include time estimates.
- After creating the README, summarize the work piece to the user and ask if they want to adjust anything.
