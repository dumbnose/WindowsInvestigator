<!-- filepath: .clinerules/02-feature-development-workflow.md -->
# Feature Development Workflow

This document provides explicit instructions for developing new features using the Memory Bank system. Follow these steps for every new feature.

## 1. Documentation First
- **Never start coding before documentation is complete and approved.**
- Read all core context files: `projectbrief.md`, `productContext.md`, `systemPatterns.md`, `techContext.md`.

## 2. Sprint & Feature Folder Setup
- Identify the current sprint folder by date (YYYY-MM, e.g., `2025-06`).
- If the sprint folder does not exist, create it under `memory-bank/features/`.
- Create a new folder for the feature inside the sprint folder: `memory-bank/features/YYYY-MM/feature-name/`.
- In the feature folder, create:
  - `prd.md` (Product Requirements Document)
  - `activeContext.md` (Active Context)

## 3. PRD File Requirements
- **Must include:**
  - User stories and acceptance criteria
  - Deep design and implementation details
  - Technical constraints and dependencies
  - Edge cases and error handling
  - Test strategy and validation plan
  - Any additional details required by the team or user
- **Clarify requirements with the user until the PRD is complete and approved.**

## 4. Active Context File
- Tracks current work, recent changes, next steps, decisions, issues, learnings, and project insights.
- Update before and after any code or requirement change.

## 5. Plan Mode vs. Act Mode
- **Plan Mode:**
  - Read all relevant Memory Bank files
  - Ensure all documentation is complete
  - Develop and present a clear strategy
- **Act Mode:**
  - Check Memory Bank
  - Update documentation as needed
  - Execute code changes
  - Document all changes in `activeContext.md`

## 6. Existing Features
- Always use the feature's `prd.md` and `activeContext.md` as the main context.
- Any change must be reflected in these docs before code changes.

## 7. Future Additions
- This workflow will be updated with more detailed requirements for `prd.md` and `activeContext.md` as the project evolves.

## 📌 Summary
- Documentation is always the first step.
- PRD and active context files are required for every feature.
- No code work should begin until documentation is complete and approved.
- Keep documentation up to date throughout the feature lifecycle.
