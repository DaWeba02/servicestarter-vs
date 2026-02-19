# ADR 0002: JWT Authentication with Roles

- **Status:** Accepted
- **Date:** 2026-02-18

## Context
The service must secure admin-only endpoints and integrate easily with external clients. Tokens should be stateless and verifiable by the API without extra storage.

## Decision
Use JWT Bearer authentication with symmetric signing (HMAC-SHA256). Token claims include email and role. A simple rule assigns the Admin role when the email ends with `@admin.local`; otherwise role is User. The AdminOnly policy checks the `role` claim.

## Consequences
- Stateless tokens simplify scaling; no session store required.
- Token issuance depends on a shared signing key; rotate via configuration.
- Role logic is minimal and for starter use only—replace with a real user store before production.

## Alternatives Considered
- Cookie/session authentication: adds server-side state and stickiness; unnecessary for API-first template.
- External OAuth/OIDC provider: more realistic but heavier setup; deferred to adopters to plug in.
