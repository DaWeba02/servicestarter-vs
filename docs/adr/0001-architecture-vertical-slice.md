# ADR 0001: Vertical Slice Minimal API

- **Status:** Accepted
- **Date:** 2026-02-18

## Context
We need a lightweight service starter on .NET 8 that stays maintainable as features grow. Controllers and layered architectures add boilerplate and scatter feature logic.

## Decision
Adopt a Vertical Slice architecture on top of Minimal APIs. Each feature owns its request/response contracts, validation, handler, and endpoint mapping inside a single folder (e.g., `Features/Auth/Login`). Cross-cutting concerns (logging, error handling, auth, persistence) stay in `Common` or `Infrastructure`.

## Consequences
- Easier feature-focused changes and testing.
- Clear boundaries keep dependencies tight.
- Minimal API keeps startup surface small; no controllers required.
- Requires discipline to avoid leaking shared state across slices.

## Alternatives Considered
- Classic layered architecture with controllers: more boilerplate, slower to evolve.
- Modular monolith modules: heavier upfront structure for a starter, deferred.
