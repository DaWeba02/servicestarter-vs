# ADR 0004: Testing Strategy

- **Status:** Accepted
- **Date:** 2026-02-18

## Context
The starter must prove validators and critical flows work, and verify infrastructure wiring (auth, EF Core, health) against a real SQL Server.

## Decision
- Unit tests with xUnit + FluentAssertions cover validators and lightweight logic.
- Integration tests use Testcontainers to spin up SQL Server, `WebApplicationFactory` for the API, and apply migrations automatically. Scenarios: login returns token, admin enforcement on `/users`, successful creation, and duplicate handling (409).

## Consequences
- Fast unit tests guard validation rules.
- Integration tests exercise real middleware, auth, and database schema; provide confidence for CI.
- Docker is required in CI/local to run integration tests.

## Alternatives Considered
- InMemory providers for integration tests: faster but would miss SQL behaviors and migrations.
- Hitting shared dev DB: introduces flakiness, data coupling; avoided to keep tests hermetic.
