# ADR-0001: Layered Clean Architecture

## Status
Accepted

## Decision
Use a clean layered split:
- Domain for core entities
- Application for use-case logic and contracts
- Infrastructure for persistence/adapters
- API/UI as delivery mechanisms

## Rationale
Keeps simulation logic isolated from UI and EF concerns and easier to test deterministically.
