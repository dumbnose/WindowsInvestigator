# PRD Template Guide

Use this template to draft Product Requirements Documents for Developer Canvas features. Each section should remain concise yet actionable, focusing on behavior, constraints, and validation strategy. Replace placeholder guidance with feature-specific content, but keep the structure so reviewers can scan quickly.

## Summary
- One paragraph describing the feature scope, target user, and why it matters now.

## Goals
- Bullet list of the measurable outcomes the feature must achieve.

## Non-Goals
- Bullet list of intentionally excluded items to prevent scope creep.

## Requirements

### High-Level Requirements (Spec)
- Describe the intended behavior at a product/spec level before listing low-level functional bullets.
- May be written as user stories ("As a ... I can ...") but does not have to be user-story phrased.
- Focus on outcomes and observable behavior (what must be true) rather than implementation.
- Include enumerated acceptance points if helpful.

### Functional Requirements
- Bullet list covering validation rules, dependency checks, configuration needs, and other must-have behaviors.

## Technical Design

### Technical Considerations
- Platform or integration constraints, performance limits, error-handling expectations, security considerations.

### Implementation Design
- Planned files/modules, their responsibilities, data/control flow overview, dependency management strategy. Reference diagrams (e.g., Mermaid) when useful. Add more sub-sections when needed.

## Testing and Validations

### Unit Tests
- Enumerate critical units/components and the behaviors their tests must cover.

### Validation Steps (E2E)
- List manual or automated end-to-end checks that confirm the main flows, error paths, and integration points.

## Implementation Plan
- Ordered list of low-level steps (e.g., scaffolding, service creation, wiring, tests, E2E). Each step should include explicit build/run/test checkpoints before moving to the next one.

## Open Questions (Optional)
- Track unanswered items blocking implementation. Remove the section once all questions are closed.

## Next Steps (Optional)
- Summarize immediate actions if the Implementation Plan is not yet ready or if approvals are pending.
