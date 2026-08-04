---
name: Test Coverage Expansion
description: Find, prioritize, and write high-value unit tests for MoopelBackend.
---

# Test Coverage Expansion

Use this skill when the goal is to increase confidence with more unit tests, better edge-case coverage, or a broader testing strategy.

## Priorities

1. Test pure helpers first.
2. Test business rules in services before controller paths.
3. Cover authentication, authorization, validation, and not-found flows.
4. Add integration tests only where the behavior crosses HTTP, database, or framework boundaries.

## Repo rules

- Follow the existing Arrange / Act / Assert style.
- Keep assertions in the Assert section.
- Use `Routes.*.Generate` helpers and test data classes instead of hardcoded values.
- Avoid magic numbers and text in tests; centralize them in `ApiTests/TestData`.
- Prefer small, deterministic tests over broad end-to-end coverage.

## What to look for

- Public methods with no tests.
- Utility methods with branching logic.
- Mapping code between requests, DTOs, and database models.
- Error handling paths and null/empty input handling.
- Authorization checks and ownership checks.

## Output expectation

When asked to improve coverage, identify the highest-value gaps first, then add tests that are:

- fast
- isolated
- readable
- stable

