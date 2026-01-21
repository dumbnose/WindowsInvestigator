# Release Analysis Workflow

## Scope and Authority
**This workflow is designed for Cline AI agents to handle user requests for release and build analysis using the Release Analysis MCP Server.**

- **When to use:** When a user provides a release URL/ID or build URL/ID and requests analysis of deployment status, EV2 rollouts, and regional investigation
- **Prerequisites:** User must provide at minimum a release URL/ID or build URL/ID


## User Input Requirements

### Required Parameters
- **Release URL or Release ID**: Azure DevOps release identifier
- **OR Build URL or Build ID**: Azure DevOps build identifier (for YAML pipeline stages)

### Optional Parameters
- **Time Range**: Specific time window for analysis (e.g., "last 24 hours", "2024-07-01 to 2024-07-02")
- **Environment/Stage Status**: "Running", "Failed", or omit for "All" environments/stages
- **Environment/Stage Name**: Specific environment/region or stage to focus analysis on

### Example User Requests
```
"Analyze release https://msazure.visualstudio.com/One/_releaseProgress?releaseId=123456"
"Check failed environments for release 123456 in the last 6 hours"
"Investigate running environments for release 123456 in West US region"
"Analyze build https://msazure.visualstudio.com/One/_build/results?buildId=789012"
"Check failed stages for build 789012 in the last 2 hours"
"Investigate running stages for build 789012"
```

## Workflow Implementation Steps

### Step 1: Input Processing and Validation

```markdown
**Actions:**
1. **Extract Identifier** from URL or direct input:
   - **For Releases**: Parse Azure DevOps release URLs to extract releaseId
   - **For Builds**: Parse Azure DevOps build URLs to extract buildId
   - Validate that releaseId/buildId is numeric
   - Handle both URL formats and direct ID input

2. **Determine Analysis Type:**
   - **Release Analysis**: Use release-based tools for classic release pipelines
   - **Build Analysis**: Use build-based tools for YAML pipeline stages

3. **Parse Optional Parameters:**
   - **Time Range**: Convert user-friendly time expressions to startTime parameter
   - **Status Filter**: Normalize to "Running", "Failed", or default to "All"
   - **Environment/Stage Filter**: Store specific environment/stage name for later filtering

4. **Validation:**
   - Ensure releaseId or buildId is valid
   - Validate time range format if provided
   - Prepare parameters for MCP tool calls

**Error Handling:**
- Invalid release/build ID → Request clarification from user
- Invalid time range → Suggest correct format
- Missing required parameters → Prompt user for release ID or build ID
```

### Step 2: MCP Connection and Tool Selection

```markdown
**Connection Management:**
1. **Error-Based Connection Refresh:**
   - Only use `RefreshConnection` tool if MCP tool calls fail with authentication/connection errors
   - Wait for successful connection confirmation after refresh
   - Retry the failed request after connection refresh
   - Do NOT call RefreshConnection proactively

2. **Tool Selection Logic:**
   ```
   FOR RELEASES:
   IF user specified "Failed" status:
       → Use GetEv2RolloutInfoFromFailedReleaseEnvironmentsAsync
   ELSE IF user specified "Running" status:
       → Use GetEv2RolloutInfoFromRunningReleaseEnvironmentsAsync
   ELSE IF user specified specific environment name:
       → Use GetEv2RolloutInfoFromAllReleaseEnvironmentsAsync (will filter later)
   ELSE:
       → Use GetEv2RolloutInfoFromAllReleaseEnvironmentsAsync (default comprehensive analysis)

   FOR BUILDS:
   IF user specified "Failed" status:
       → Use GetEv2RolloutInfoFromFailedStages
   ELSE IF user specified "Running" status:
       → Use GetEv2RolloutInfoFromRunningStage
   ELSE:
       → Use GetEv2RolloutInfoFromAllStagesAsync (default comprehensive analysis)
   ```

3. **Parameter Preparation:**
   - releaseId: Extracted from input (for release analysis)
   - buildId: Extracted from input (for build analysis)
   - startTime: Converted from time range (optional)
```

### Step 3: Environment/Stage Analysis

```markdown
**Execute Primary Analysis:**
1. **Call Selected MCP Tool** with prepared parameters
2. **Process Response Data:**
   - **For Releases**: Extract environment/region information
   - **For Builds**: Extract stage information
   - Identify status of each environment/stage (Success/Failed/Running/Canceled)
   - Extract error messages and failure details
   - Look for EV2 rollout information in logs

3. **Environment/Stage Filtering** (if specific target requested):
   - **For Releases**: Find closest matching environment name using fuzzy matching
   - **For Builds**: Find closest matching stage name using fuzzy matching
   - Handle variations: partial names, case differences, region abbreviations
   - Provide feedback if exact match not found
   - **IMPORTANT**: If user specifies a specific region/environment or ring/stage, ONLY include that specific target in results

4. **Extract Key Information:**
   - **If specific target specified**: Only extract information for the matching environment/stage
   - **If comprehensive analysis**: Extract all environment/stage information
   - Environment/stage names and deployment status
   - Timeline information (start time, duration)
   - Error messages and failure reasons
   - EV2 rollout links and identifiers
```

### Step 4: EV2 Rollout Deep Analysis

```markdown
**EV2 Data Extraction and Analysis:**
1. **Extract EV2 Information** from Azure DevOps logs:
   - rolloutId: Unique identifier for EV2 rollout
   - serviceGroup: Service group name
   - rolloutinfra: Infrastructure specification

2. **Individual EV2 Investigation (Critical for Multiple Targets):**
   - **Call GetEv2RolloutLogs** for EACH environment with EV2 data separately
   - **Never assume**: Similar rollout IDs mean identical issues
   - **Individual analysis**: Each target gets its own EV2 deep dive
   - Use extracted rolloutId, serviceGroup, rolloutinfra for each target
   - Analyze EV2-specific deployment details per target
   - Extract rollout policies and health check status per target

3. **Generate Individual EV2 Links:**
   - EV2 portal links for real-time monitoring per target
   - Health dashboard URLs when available per target
   - Format links for easy access: `[Target EV2 Portal](ev2_link_here)`
   - **Separate links**: Each target gets its own set of monitoring links

4. **Target-Specific EV2 Analysis Focus:**
   - **Per Target**: Current rollout phase and progress
   - **Per Target**: Policy-level success/failure status
   - **Per Target**: Resource group deployment status
   - **Per Target**: Safety policy violations or blocks
   - **Per Target**: Dependencies causing delays
   - **Individual variations**: Document differences between targets even with similar symptoms
```

### Step 5: Targeted or Comprehensive Analysis and Insights

```markdown
**Data Processing and Analysis:**
1. **Analysis Scope Determination:**
   - **If specific target requested**: Focus analysis ONLY on the specified environment/stage
   - **If multiple targets requested**: Analyze each target individually with separate investigations
   - **If comprehensive analysis**: Analyze all environments/stages

2. **Individual Target Investigation (Critical Principle):**
   - **IMPORTANT**: Each environment/stage must be investigated individually
   - **Never assume**: If majority have failed with similar symptoms, ALL have the same issue
   - **Always verify**: Check each target's specific error messages, logs, and health status
   - **Individual EV2 analysis**: Call GetEv2RolloutLogs for each target separately
   - **Unique insights**: Extract target-specific findings, timelines, and remediation steps

3. **Regional/Stage Pattern Analysis:**
   - **For Single Target**: Deep dive into specific environment/stage behavior and health
   - **For Multiple Targets**: Individual analysis for each, then identify common patterns if they exist
   - **For Comprehensive Analysis**: Compare success/failure patterns across regions/stages
   - Identify systematic vs isolated issues
   - Analyze deployment/execution timing and sequence
   - Detect performance degradation trends
   - **Note variations**: Highlight differences between targets even if they appear similar

4. **Timeline Analysis:**
   - **For Single Target**: Focus on specific environment/stage timeline and dependencies
   - **For Multiple Targets**: Individual timelines for each target, highlighting unique progression
   - **For Comprehensive Analysis**: Chronological progression across all environments/stages
   - Duration comparison between environments/stages (if multiple)
   - Identify bottlenecks and stuck deployments/stages
   - Time-correlated failure analysis
   - **Individual patterns**: Track each target's specific timeline patterns

5. **Root Cause Identification:**
   - **For Single Target**: Environment/stage-specific error patterns and issues
   - **For Multiple Targets**: Individual root cause analysis for each target
   - **For Comprehensive Analysis**: Common error patterns across environments/stages
   - Infrastructure-specific issues (releases) or pipeline issues (builds)
   - Policy violations and safety blocks
   - Resource dependency failures
   - **Unique causes**: Identify target-specific root causes even when others show similar symptoms

6. **Risk Assessment:**
   - **For Single Target**: Impact assessment for the specific environment/stage
   - **For Multiple Targets**: Individual risk assessment for each target
   - **For Comprehensive Analysis**: Impact of failures on remaining deployments/stages
   - Success rate calculation (overall or specific)
   - Critical issues requiring immediate attention
   - Potential cascade failure risks
   - **Individual priorities**: Each target may have different risk levels and urgency
```

### Step 6: Chat-Based Report Generation and Formatting

```markdown
**Chat Output Generation:**
```
**Present the complete analysis directly in the Cline chat using the following structure:**

FOR RELEASES (Comprehensive):
## Release Analysis Summary
- **Release ID:** [extracted_release_id]
- **Analysis Time:** [current_timestamp]
- **Total Environments:** [environment_count]
- **Success Rate:** [success_percentage]%
- **Critical Issues:** [critical_issue_count]
- **Time Range:** [analyzed_time_range]

FOR RELEASES (Targeted - Specific Environment):
## Release Environment Analysis
- **Release ID:** [extracted_release_id]
- **Environment:** [specific_environment_name]
- **Analysis Time:** [current_timestamp]
- **Status:** [environment_status]
- **Critical Issues:** [environment_specific_issues]
- **Time Range:** [analyzed_time_range]

FOR BUILDS (Comprehensive):
## Build Analysis Summary
- **Build ID:** [extracted_build_id]
- **Analysis Time:** [current_timestamp]
- **Total Stages:** [stage_count]
- **Success Rate:** [success_percentage]%
- **Critical Issues:** [critical_issue_count]
- **Time Range:** [analyzed_time_range]

FOR BUILDS (Targeted - Specific Stage):
## Build Stage Analysis
- **Build ID:** [extracted_build_id]
- **Stage:** [specific_stage_name]
- **Analysis Time:** [current_timestamp]
- **Status:** [stage_status]
- **Critical Issues:** [stage_specific_issues]
- **Time Range:** [analyzed_time_range]

## Key Findings
- [Major finding 1 with specific details]
- [Major finding 2 with regional/stage impact]
- [Major finding 3 with timeline analysis]

## Immediate Actions Required
1. [Priority 1 action with specific steps]
2. [Priority 2 action with timeline]
3. [Priority 3 action with responsible team]
```

**Analysis Table (displayed in chat):**
```
FOR COMPREHENSIVE RELEASES:
| Region | Status | Error Details | EV2 Rollout | Health Dashboard | Timeline | Remediation |
|--------|--------|---------------|-------------|------------------|----------|-------------|
| [Region] | [Success/Failed/Running] | [Specific error messages] | [EV2 Portal Link] | [Health Dashboard Link] | [Start time - Duration] | [Specific action items] |

FOR TARGETED RELEASES (Single Environment):
| Environment | Status | Error Details | EV2 Rollout | Health Dashboard | Timeline | Remediation |
|-------------|--------|---------------|-------------|------------------|----------|-------------|
| [Target Environment] | [Success/Failed/Running] | [Specific error messages] | [EV2 Portal Link] | [Health Dashboard Link] | [Start time - Duration] | [Specific action items] |

FOR COMPREHENSIVE BUILDS:
| Stage | Status | Error Details | EV2 Rollout | Pipeline Details | Timeline | Remediation |
|-------|--------|---------------|-------------|------------------|----------|-------------|
| [Stage] | [Success/Failed/Running] | [Specific error messages] | [EV2 Portal Link] | [Stage Details Link] | [Start time - Duration] | [Specific action items] |

FOR TARGETED BUILDS (Single Stage):
| Stage | Status | Error Details | EV2 Rollout | Pipeline Details | Timeline | Remediation |
|-------|--------|---------------|-------------|------------------|----------|-------------|
| [Target Stage] | [Success/Failed/Running] | [Specific error messages] | [EV2 Portal Link] | [Stage Details Link] | [Start time - Duration] | [Specific action items] |
```

**Detailed EV2 Analysis Section (in chat):**
```
## EV2 Rollout Analysis

FOR MULTIPLE TARGETS (Each target gets individual analysis):
### [Target 1 Name]
- **Rollout ID:** [target1_rollout_id]
- **Current Phase:** [target1_rollout_phase]
- **Policy Status:** [target1_policy_success/failure_details]
- **Resource Status:** [target1_resource_deployment_status]
- **Safety Policies:** [target1_safety_policy_violations_or_success]
- **Portal Link:** [Target 1 EV2 Portal URL]
- **Recommended Actions:** [target1_specific_ev2_remediation_steps]

### [Target 2 Name]
- **Rollout ID:** [target2_rollout_id]
- **Current Phase:** [target2_rollout_phase]
- **Policy Status:** [target2_policy_success/failure_details]
- **Resource Status:** [target2_resource_deployment_status]
- **Safety Policies:** [target2_safety_policy_violations_or_success]
- **Portal Link:** [Target 2 EV2 Portal URL]
- **Recommended Actions:** [target2_specific_ev2_remediation_steps]

[Continue for each target individually...]

FOR SINGLE TARGET:
### [Target Name]
- **Rollout ID:** [rollout_id]
- **Current Phase:** [rollout_phase]
- **Policy Status:** [policy_success/failure_details]
- **Resource Status:** [resource_deployment_status]
- **Safety Policies:** [safety_policy_violations_or_success]
- **Portal Link:** [EV2 Portal URL]
- **Recommended Actions:** [specific_ev2_remediation_steps]
```

**Remediation Recommendations (in chat):**
- Environment-specific action items
- Timeline-sensitive remediation steps
- Contact information for responsible teams
- Links to runbooks and documentation

**OUTPUT REQUIREMENTS:**
- Display all analysis results directly in the Cline chat (no file creation)
- Use markdown formatting in chat responses for readability
- Include all sections in a single comprehensive chat message
- Make links clickable where possible
- Use tables for structured data presentation
```

## Chat Output Template Structure

### Complete Chat Response Format
```markdown
FOR RELEASES:
# Release [ReleaseID] Analysis Report

## Executive Summary
[Status overview with key metrics]

## Regional Deployment Status
[Comprehensive table with all environments]

## EV2 Rollout Analysis
[Detailed EV2 insights per region]

## Timeline Analysis
[Chronological progression and bottlenecks]

## Root Cause Analysis
[Common patterns and systematic issues]

## Immediate Actions Required
[Prioritized remediation steps]

## Additional Resources
- [EV2 Portal Links]
- [Health Dashboard URLs]
- [Runbook References]
- [Contact Information]

FOR BUILDS:
# Build [BuildID] Analysis Report

## Executive Summary
[Status overview with key metrics]

## Stage Execution Status
[Comprehensive table with all stages]

## EV2 Rollout Analysis
[Detailed EV2 insights per stage]

## Timeline Analysis
[Chronological stage progression and bottlenecks]

## Root Cause Analysis
[Common patterns and pipeline issues]

## Immediate Actions Required
[Prioritized remediation steps]

## Additional Resources
- [EV2 Portal Links]
- [Pipeline Details URLs]
- [Runbook References]
- [Contact Information]
```

**Chat Delivery Guidelines:**
- Present the entire analysis in a single, well-formatted chat response
- Use markdown formatting for headers, tables, and lists
- Include clickable links for EV2 portals and dashboards
- Ensure tables are properly formatted for chat display
- Break up long sections with appropriate headers and spacing
- Never suggest creating or saving files
- Always conclude with actionable next steps

## Error Handling and Edge Cases

### Authentication Issues
```markdown
1. **Connection Failures:**
   - Use RefreshConnection tool ONLY when MCP tools fail with authentication errors
   - Retry original request after successful connection refresh
   - Inform user of authentication problems if persistent failures occur

2. **Tool Timeout:**
   - Provide partial results if available
   - Suggest retry with shorter time range
   - Recommend alternative investigation approaches
```

### Data Quality Issues
```markdown
1. **Missing EV2 Information:**
   - Focus on Azure DevOps analysis
   - Note limitations in report
   - Suggest manual EV2 portal checking

2. **No Logs Found:**
   - Verify release ID accuracy
   - Check time range parameters
   - Suggest broader time window

3. **Partial Data:**
   - Present available information
   - Clearly mark incomplete sections
   - Provide guidance for obtaining missing data
```

### User Experience Enhancements
```markdown
1. **Environment Name Matching:**
   - Use fuzzy matching for partial names
   - Handle common abbreviations (WUS2, EUS, etc.)
   - Provide suggestions when exact match not found

2. **Time Range Processing:**
   - Accept natural language ("last 24 hours", "yesterday")
   - Convert to appropriate MCP tool parameters
   - Default to reasonable time windows if not specified

3. **Progressive Disclosure:**
   - Start with high-level summary
   - Provide detailed analysis sections
   - Include actionable recommendations
```

## Best Practices for Implementation

### Information Gathering
1. **Always start with comprehensive tool** unless user specifically requests filtered view:
   - **For Releases**: Use GetEv2RolloutInfoFromAllReleaseEnvironmentsAsync
   - **For Builds**: Use GetEv2RolloutInfoFromAllStagesAsync
2. **Extract EV2 information whenever available** for deep rollout analysis
3. **Look for patterns** to identify systematic vs isolated issues:
   - **For Releases**: Compare patterns across regions
   - **For Builds**: Compare patterns across stages
4. **Include timeline analysis** to understand progression:
   - **For Releases**: Deployment progression across environments
   - **For Builds**: Stage execution progression and dependencies

### Report Quality
1. **Provide actionable insights** - not just status, but specific next steps
2. **Include relevant links** for further investigation:
   - **For Releases**: EV2 portal links, health dashboards
   - **For Builds**: Pipeline details, stage logs, EV2 portal links
3. **Format results clearly** with tables and structured summaries
4. **Tailor recommendations** to specific issues found:
   - **For Releases**: Environment-specific remediation
   - **For Builds**: Stage-specific and pipeline-level fixes

### User Communication
1. **Clear status updates** during analysis process
2. **Explain tool selection** rationale when relevant:
   - Clarify whether analyzing releases or builds
   - Justify filtered vs comprehensive analysis approach
3. **Highlight critical issues** requiring immediate attention
4. **Provide context** for technical findings with proper terminology:
   - **For Releases**: Use environment, region, deployment terminology
   - **For Builds**: Use stage, pipeline, execution terminology

This workflow ensures comprehensive, actionable analysis for any release or build investigation scenario while maintaining consistency with established investigation processes.
