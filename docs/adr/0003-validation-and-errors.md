# ADR 0003: Validation and Error Handling

- **Status:** Accepted
- **Date:** 2026-02-18

## Context
Requests must be validated consistently and clients should receive RFC 7807 ProblemDetails responses. Minimal APIs do not provide automatic validation like MVC.

## Decision
- Use FluentValidation for request DTO validation.
- Apply a reusable endpoint filter (`ValidationFilter<T>`) to run validators and return `ValidationProblem` results.
- Add a global `ExceptionHandlingMiddleware` that translates exceptions to ProblemDetails payloads, logging via Serilog.

## Consequences
- Consistent 400 responses with per-field errors.
- Centralized error formatting; easier to extend for domain exceptions.
- Slight per-request overhead for validation filter; acceptable for starter template.

## Alternatives Considered
- Rely on Minimal API parameter binding only: insufficient for complex validation, inconsistent errors.
- MVC controllers with automatic validation: more boilerplate than desired for this template.
