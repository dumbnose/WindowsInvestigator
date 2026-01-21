# AM Alerts Coding Agent Instructions - Shared

This repository contains a comprehensive set of shared AI agent rules and instructions designed to standardize development workflows across multiple repositories. It provides consistent guidance for feature development, Azure DevOps integration, investigation processes, and memory bank management across both Cline and GitHub Copilot (custom agents).

## What This Repository Contains

This repository supports two main objectives:

1. **Cline default instructions and workflows**
	- **Memory Bank System**: Documentation structure, update rules, and context management for persistent development
	- **Feature Development Workflow**: Step-by-step processes for creating features with proper documentation-first approach
	- **Azure DevOps Integration**: Defaults, workflows, and best practices for ADO work item management and pull requests
	- **Live Site Investigation Process**: Systematic methodology for incident investigation and troubleshooting
	- **Release Analysis Workflow**: Specialized processes for deployment and release-related investigations

2. **GitHub Copilot custom agents**
	- Custom agents that operationalize the same topics (Memory Bank, feature workflow, ADO integration, investigations, and release analysis) into repeatable, task-focused chat experiences in VS Code.

## Adding This Repository as a Git Submodule

To add these shared Cline rules to your repository, follow these steps:

### 1. Add the Submodule

From your repository root directory, run:

```bash
git submodule add https://msazure@dev.azure.com/msazure/One/_git/AM-Alerts-CodingAgentInstructions-Shared .clinerules
```

This will:
- Clone this repository into a `.clinerules` directory in your repo
- Create a `.gitmodules` file to track the submodule
- Add the submodule reference to your git index

### 2. Initialize and Update (for existing repos with submodules)

If you're cloning a repository that already has this submodule configured:

```bash
git submodule init
git submodule update
```

Or use the recursive clone option:

```bash
git clone --recursive <your-repo-url>
```

### 3. Update the Submodule

To pull the latest changes from the shared instructions:

```bash
cd .clinerules
git pull origin main
cd ..
git add .clinerules
git commit -m "Update shared Cline rules submodule"
```

### 4. Automatic Updates

You can also update all submodules at once:

```bash
git submodule update --remote
```

## Usage

Once the submodule is added:

### Usage in Cline

1. **Cline will automatically load the instructions** from the `.clinerules` directory when working in your repository
2. **All AI agents** will follow the standardized workflows defined in these files
3. **Documentation and development processes** will be consistent across all repositories using this submodule

#### File Structure

The `.clinerules` directory contains:

- `00-instructions-overview.md` - Overview of all instruction files and when to use them
- `01-memory-bank-structure.md` - Memory Bank system structure and update rules
- `02-feature-development-workflow.md` - Feature development processes and requirements
- `03-using-ado.md` - Azure DevOps integration guidelines and defaults
- `04-livesite-investigation-process.md` - Investigation and incident management workflows
- `workflows/release-analysis-worfklow.md` - Release and deployment analysis processes
- `workflows/cp-sli-investigation-workflow.md` - CP SLI investigation workflow for "CRUD" operations
- `workflows/dp-sli-investigation-workflow.md` - DP SLI investigation workflow for Log Search Alerts

#### Workflow Dependencies

### SLI Investigation Workflows

To use the SLI investigation workflows, you must have the following dependency file in your memory bank:

**Required Memory Bank File:**
- `memory-bank/sli-investigation.md` - Contains context and queries for SLI investigation processes

**Affected Workflows:**
- `workflows/cp-sli-investigation-workflow.md` - References `memory-bank/sli-investigation.md` for RP SLI memory bank context
- `workflows/dp-sli-investigation-workflow.md` - References `memory-bank/sli-investigation.md` for Log Search Alerts SLI investigation

These workflows cannot function properly without the memory bank dependency. Ensure the `memory-bank/sli-investigation.md` file exists in your repository before using these workflows.

### Usage in GitHub Copilot

This repository includes GitHub Copilot custom agents under `.clinerules/subagents`.

To make VS Code pick up the agents in this repo, update your VS Code **User** settings JSON and add:

```json
"chat.modeFilesLocations": {
				".github/agents": true,
				".clinerules/subagents": true
		},
```

After this, the agents from those folders will show up in the **Agents** section of Copilot Chat.

#### Example agents

This list will evolve over time as more agents are added.

- **Custom Agent Builder**: Use when you want to add a new custom agent (`.agent.md`) and keep it minimal, safe, and consistent with repo conventions.
	- **Dependencies (MCP servers):** None
- **dri-agent**: Use for structured live site investigations of IcM incidents (hypothesis-driven, evidence-first), including Kusto analysis.
	- **Dependencies (MCP servers):** Azure MCP (Kusto), IcM MCP
- **cri-agent**: Use for customer-reported incident (CRI) investigations and customer-facing remediation guidance, including Kusto analysis and IcM context.
	- **Dependencies (MCP servers):** Azure MCP (Kusto), IcM MCP
- **cp-sli-investigator**: Use for investigating Control Plane SLI drops and service degradation (workflow-driven, thorough root cause analysis).
	- **Dependencies (MCP servers):** Azure MCP (Kusto)
- **work-item-planner**: Use to start a documentation-first PRD plan from an Azure DevOps work item (creates PRD + `activeContext.md` in the correct Memory Bank location).
	- **Dependencies (MCP servers):** Azure DevOps MCP

## Benefits

- **Consistency**: Standardized development workflows across all repositories
- **Efficiency**: Reduced setup time for new projects and team members
- **Quality**: Proven processes and documentation standards
- **Maintainability**: Centralized updates that propagate to all repositories
- **Collaboration**: Shared understanding of processes and expectations

## Updating Shared Rules

Changes to the shared rules should be made in this repository and will automatically be available to all repositories using it as a submodule after they update their submodule reference.

# Contribute

TBD
