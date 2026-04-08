# ADR-0002: Deterministic DAG Simulation (Fail Fast on Cycles)

## Status
Accepted

## Decision
Use deterministic topological traversal for reachable graph and fail fast when cycles exist.

## Rationale
Predictable outputs improve architectural reasoning and testing stability.
