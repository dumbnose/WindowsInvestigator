<!-- filepath: .clinerules/04-livesite-investigation-process.md -->
# Investigation Process

## Overview
This file defines the **mandatory workflow and behavioral rules** for any AI Agent or user starting an investigation or live site incident process based on IcM (Incident Management) tickets. It is critical that these instructions are followed exactly to ensure consistency, completeness, and high-quality documentation.

**When to use this process:** For live site investigations that are NOT related to deployments or releases, primarily based on IcM ticket information. This covers general service issues, errors, performance problems, and operational incidents reported through the Incident Management system.

**IMPORTANT:** If the investigation involves Azure DevOps releases, deployments, build failures, or EV2 rollouts, refer to `.clinerules/workflows/release-analysis-worfklow.md`. Do NOT use this process for any deployment-related investigations.

---

## 1. Always Read the DRI-Agent Investigation Process
- **Before starting any investigation or live site incident, you MUST read the full contents of `.clinerules/subagents/dri-agent/investigation-process.md`.**
- This file is the single source of truth for investigation workflow, documentation structure, and execution guidelines.
- Do not begin any analysis, querying, or troubleshooting until you have read and understood the entire file.

## 2. Follow All Referenced Files
- If `.clinerules/subagents/dri-agent/investigation-process.md` references additional files (such as `troubleshooting.md`, `metrics.md`, `telemetry.md`, or others), you MUST read those files in full before proceeding.
- **You MUST also read `.clinerules/subagents/dri-agent/investigation-methodology.md`** - this file provides the systematic methodology and logical flow for investigating incidents using metrics and logs. It contains the step-by-step workflow approach with proven investigation phases and decision points that must be followed.
- If any of these files reference further documentation, continue reading until you have a complete understanding of all required context.
- **You are not allowed to skip any referenced file.**

## 3. Execution Discipline
- **Document before you act:** Write your planned step in the investigation file before executing it.
- **Record outcomes immediately:** After each step, update the file with results, evidence, and analysis.
- **Ask for help when blocked:** Use the `ask_followup_question` tool as described, and document both the question and the response.

## 4. Scope Discipline
- Focus only on the current ticket/incident.
- Do not investigate unrelated issues unless directly relevant.
- Reference previous investigations only if they are directly applicable.

## 5. Enforcement
- **Any deviation from this process is considered a critical error.**
- If you are unsure about any step, re-read the relevant documentation or ask for clarification before proceeding.

---

*This file is the authoritative guide for all investigation and live site processes. It must be kept up to date and referenced at the start of every investigation.*
