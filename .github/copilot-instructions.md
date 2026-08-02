# Copilot Instructions

## AVOID MAGIC NUMBERS/TEST AT ALL TIMES
- Magic numbers/text should be defined in a static file.

## Core Principles
- Keep solutions simple.
- Prefer readable code over clever code.
- Follow existing project patterns.
- Avoid introducing new frameworks or libraries unless requested.
- Make the smallest change necessary.
- Explain why changes are needed.
- Preserve backward compatibility where possible.

## Output Style
- Show only the files that need to change.
- Include concise explanations.
- Highlight risks or assumptions.

## Testing Guidelines
- In tests, Assert calls should only be placed in the Assert section (not mixed into Arrange/Act sections).