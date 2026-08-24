# Project Working Guidelines

## Code design

- Keep fixes small, simple, and easy to understand.
- Prefer reusable modules when behavior is shared by existing features or confirmed future features.
- Generalize only when it serves a real use case; avoid speculative abstractions.
- Keep feature-specific rules in their feature module and shared mechanics in focused utilities.
- Reuse an existing utility before adding duplicate logic.
