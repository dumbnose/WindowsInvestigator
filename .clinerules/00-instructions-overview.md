# Cline Instructions Overview

This directory contains custom instructions that guide Cline's behavior in this project. Each file provides specific guidance for different scenarios and workflows.

## Instruction Files

### [01-memory-bank-structure.md](./01-memory-bank-structure.md)
**When to use:** For understanding the Memory Bank system, its structure, the meaning of each file, and explicit rules for keeping the Memory Bank updated (feature-level and global updates).

### [02-feature-development-workflow.md](./02-feature-development-workflow.md)
**When to use:** For all new feature development work. Contains step-by-step instructions for creating and maintaining feature documentation (PRD and activeContext files), sprint/feature folder setup, and the Plan/Act workflow.

### [03-using-ado.md](./03-using-ado.md)
**When to use:** When planning work, starting new work items, or managing Azure DevOps tasks. Provides defaults and workflows for ADO integration.

### [04-livesite-investigation-process.md](./04-livesite-investigation-process.md)
**When to use:** For live site investigations that are NOT related to deployments or releases. This covers general service issues, errors, performance problems, and operational incidents.

## Workflow Files

### [workflows/release-analysis-worfklow.md](./workflows/release-analysis-worfklow.md)
**When to use:** When users provide release URLs/IDs and request analysis of deployment status, EV2 rollouts, and regional investigation. This workflow implements the patterns from the release investigation process in a user-friendly format.

---

## Usage Guidelines

- These instructions are automatically loaded when Cline starts working in this repository
- Each file contains specific workflows and defaults for different aspects of the project
- Follow the workflows in the order they appear when starting new work
- Refer back to these instructions when context or guidance is needed
- **Important**: When adding new instruction files to this folder, this overview file must be updated to reference them
