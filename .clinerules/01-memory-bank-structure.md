<!-- filepath: .clinerules/01-memory-bank-structure.md -->
# Memory Bank Structure & Update Instructions

This document defines the structure, purpose, and update rules for the Memory Bank system used by Cline and all AI agents in this repository.

## 🧠 Memory Bank Overview

The Memory Bank is a structured documentation system that enables persistent, context-rich development. It is the single source of truth for project context, requirements, technical decisions, and feature progress.

### Core Files (Global Scope)

- `projectbrief.md`: Project foundation, core requirements, and goals. Defines project scope.
- `productContext.md`: Project purpose, user experience, and problem statements.
- `systemPatterns.md`: System architecture, design patterns, and key technical decisions.
- `techContext.md`: Technologies, development setup, constraints, and dependencies.

All core files are required and must be kept up to date if any project-wide change occurs.

### Feature Files (Sprint & Feature Scope)

Features are organized by sprint (YYYY-MM) under `memory-bank/features/`. Each feature has its own folder:

```
memory-bank/
  features/
    2025-06/           # Sprint folder (current: June 2025)
      my-feature/      # Feature folder
        prd.md         # Product requirements & design
        activeContext.md # Current state, next steps, learnings
```

#### `prd.md` (Product Requirements Document)
- User stories, acceptance criteria, design, and implementation details.
- Must be created and approved before any code work begins.

#### `activeContext.md`
- Tracks current work, recent changes, next steps, decisions, issues, and learnings.
- Must be updated before and after any code or requirement change.

### Per-Service Configuration Files

Configuration files that contain service-specific settings and defaults:

- `ado-configuration.md`: Azure DevOps integration settings, including project name, team name, area path, repository details, and query IDs for work item management.

These files must be created by each repository owner with values specific to their project and team. They provide the necessary configuration for external service integrations and should be maintained as part of the repository's documentation.

#### Creating `ado-configuration.md`

Each repository must create the file `memory-bank/ado-configuration.md` with the following required configuration values:

- **Project**: Your Azure DevOps project name
- **Team name**: Your team name within the project  
- **Area Path**: Your team's area path for work items
- **Repository Name**: Primary repository name for pull requests
- **Repository ID**: Repository ID for pull request operations
- **Target Branch**: Default target branch for pull requests
- **Query ID for work items**: Saved query ID for team work items (optional)

**Example structure:**
```markdown
# ADO Configuration

## Project Settings
- **Project**: YourProjectName
- **Team name**: YourTeamName
- **Area Path**: Your\\Project\\Area\\Path

## Repository Settings  
- **Repository Name**: YourRepositoryName
- **Repository ID**: your-repository-id-here
- **Target Branch**: master

## Query IDs
- **Query ID for work items**: query-id-here
- **Current sprint work items assigned to me**: query-id-here
```

Each repository owner must populate these values with their specific project and team configuration.

## 🔄 Memory Bank Update Rules

- **Feature-level updates:**
  - Update the relevant feature's `prd.md` and `activeContext.md` for any change to requirements, design, or implementation.
  - Always update `activeContext.md` before and after code changes.
- **Core/global updates:**
  - Only update core files (`projectbrief.md`, `productContext.md`, `systemPatterns.md`, `techContext.md`) if the change impacts the entire project or if explicitly requested.
- **Update triggers:**
  - New features, significant changes, new patterns, or user request ("update memory bank").

## 📌 Summary

- The Memory Bank is the foundation for all work. Its accuracy is critical.
- Feature docs (`prd.md`, `activeContext.md`) are updated most frequently.
- Core docs are only updated for project-wide changes.
- Always read the Memory Bank before starting any task.
