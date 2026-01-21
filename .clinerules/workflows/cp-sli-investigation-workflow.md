# CP SLI Investigation Workflow

> Reference: memory-bank/sli-investigation.md
> You MUST follow these steps, do not skip any step unless it is written to skip.
> You MUST document all steps results in the "Step 2: Reporting".
> Do no Sample any data, I want you to investigate all data you have, and put all in the report.

## Prerequisites: Target Resource Type Parameter

**MANDATORY**: Before starting this workflow, you MUST receive the targetResourceType parameter from the user.

**Valid targetResourceType options:**
- `SCHEDULEDQUERYRULES`
- `METRICALERTS` 
- `ACTIVITYLOGALERTS`
- `PROMETHEUSRULEGROUPS`

**Important**: All investigation steps will be filtered to analyze ONLY the specified targetResourceType. The entire workflow focuses exclusively on this single resource type.

### Example User Requests
```
/cp-sli-investigation-workflow.md from 2025-08-31 to 2025-09-07 for "PROMETHEUSRULEGROUPS"
```

### Kusto Query Execution Method

**MANDATORY**: For all Kusto queries in this workflow:
1. **First, attempt to use** the `github.com/Azure/azure-mcp` kusto tool if available
2. **If not available**, search for and use another available kusto MCP server

## Step 1: Identify SLI Drops for SLI - "CRUD" Operations (Filtered by targetResourceType)

### Inner Step 1.0: Read RP SLI Memory Bank

- Read and understand all context and queries in `\memory-bank\sli-investigation.md`.
- You MUST do this step

### Inner Step 1.1: Calculate SLI Trends for SLI - "CRUD" Operations (Filtered by targetResourceType)

- Focus on hourly intervals to identify precise drop times.
- Look for any success rate drops below the 99.9% threshold.
- **CRITICAL**: Filter all queries by the specified targetResourceType parameter. Only analyze data for the selected resource type.
- Document timestamp of each drop in the "Step 2: Reporting" report results.
- This is critical step. You must document it, and you must find all drops we have and the drop period. For each drop, you will need to do further investigation in the next steps.
- **Filter Requirement**: All SLI trend calculations must include `where targetResourceType == "{targetResourceType}"` in the query.

### Inner Step 1.2: Drill Down Analysis by region for SLI - "CRUD" Operations - Regional Breakdown (Filtered by targetResourceType)

- Do this step on every drop from the Inner Step 1.1 above. If there are no drops, stop here.
- **CRITICAL**: Filter all regional analysis queries by the specified targetResourceType parameter.
- I want you to add this step results as a new column in the Inner Step 1.1 results above, with the most impacted regions.
- Document all in the "Step 2: Reporting" report results.
- **Filter Requirement**: All regional breakdown queries must include `where targetResourceType == "{targetResourceType}"` in the query.

### Inner Step 1.3: Drill Down Analysis by impacted subscriptions for SLI - "CRUD" Operations - Impacted Subscriptions Breakdown (Filtered by targetResourceType)

- Do this step on every drop from the Inner Step 1.1 above. If there are no drops, stop here.
- **CRITICAL**: Filter all subscription impact analysis queries by the specified targetResourceType parameter.
- I want you to add this step results as a new column in the Inner Step 1.1 results above, with the number of impacted subscriptions.
- Document all in the "Step 2: Reporting" report results.
- **Filter Requirement**: All subscription impact queries must include `where targetResourceType == "{targetResourceType}"` in the query.

### Inner Step 1.4: Calculate Root Cause Analysis - get all ActivityId list - SLI - "CRUD" Operations - ActivityId Extraction (Filtered by targetResourceType)

- Do this step on every drop from the Inner Step 1.1 above. If there are no drops, stop here.
- **CRITICAL**: Filter all ActivityId extraction queries by the specified targetResourceType parameter.
- Get all ActivityId list, you need to use it in the next step.
- Fetch all ActivityIds for the specified targetResourceType only.
- Document them all, do not Sample them, I want all.
- Document it is the "Step 2: Reporting" report results.
- The results of this step is a table with the drop timestamps from the Inner Step 1.1 above and it's ActivityId list.
- **Filter Requirement**: All ActivityId extraction queries must include `where targetResourceType == "{targetResourceType}"` in the query.

### Inner Step 1.5: Calculate Root Cause Analysis - use the ActivityId list and use this step Resource Provider Traces data Analysis by ActivityId to calculate the root cause analysis (Filtered by targetResourceType)

- Use the ActivityId list from Step Inner Step 1.4 above (filtered by targetResourceType) and run this query: "RP Traces Analysis by ActivityId".
- **CRITICAL**: Ensure the ActivityId list used is only for the specified targetResourceType.
- Use the ActivityId list from Step Inner Step 1.4 above and run the dri-agent to calculate the root cause analysis.
- Analyse the results focusing on the specific targetResourceType operations and patterns.
- Use the Operation, ErrorType and Error columns to understand the error root cause for the targetResourceType.
- **CRITICAL - Error Details Collection**: For each error found, capture:
  - **Exact error messages** from the Error column (full text, no truncation)
  - **HTTP status codes** and response codes
  - **Dependency responses** and failure details (if available in traces)
  - **Stack traces** or detailed error information
  - **Operation context** (which specific operation failed and why)
- Document ALL error details - do not summarize or truncate error messages.
- Document it is the "Step 2: Reporting" report results.
- The results of this step is a table with the drop timestamps from the Inner Step 1.1 above and it's root cause analysis results for the specified targetResourceType only, including complete error messages and dependency responses.

## Step 2: Reporting (Single targetResourceType Focus)

- You MUST create a report file and write all findings in that file.
- **File naming convention**: File format name should be indicative and include the targetResourceType: `cp-sli-investigation-{targetResourceType}-{YYYY-MM-DD}.md`
- **Single File Output**: Save ONE file in this folder: `memory-bank\investigations\sli-investigations` specifically for the targetResourceType parameter provided.
- **Report Focus**: The entire report should focus exclusively on the specified targetResourceType. Do not include analysis for other resource types.

### Report Format Requirements

#### Main Summary Table

Create a comprehensive table with the following columns for each SLI drop identified for the **{targetResourceType}**:

| Drop Timestamp | Success Rate | Impacted Regions (Failure Count) | Number of Impacted Subscriptions |
|---|---|---|---|
| YYYY-MM-DD HH:MM:SS UTC | 📉 XX.XXX% | 🌍 region1 (count), region2 (count), region3 (count) | 📊 Count

**Report Header**: All reports must start with a clear header indicating the target resource type:
```markdown
# CP SLI Investigation Report - {targetResourceType}
**Investigation Date**: {Current Date}  
**Target Resource Type**: {targetResourceType}  
**Analysis Scope**: CRUD Operations SLI Analysis  
```

#### Detailed Root Cause Analysis Section

For each SLI drop, create a dedicated detailed root cause analysis section with the following structure:

```markdown
## 🔍 Root Cause Analysis for Drop: [TIMESTAMP] - {targetResourceType}

### 📊 Impact Summary
- **Target Resource Type**: {targetResourceType}
- **Duration**: [Start Time] to [End Time] UTC
- **Success Rate Drop**: From X% to Y%
- **Total Failed Operations**: [Count] (for {targetResourceType} only)
- **Most Impacted Regions**: [Region list with failure counts for {targetResourceType}]

### 🎯 Primary Root Cause
- **Resource Type**: {targetResourceType}
- **Operation Type**: [e.g., PUT, GET, DELETE, POST for {targetResourceType}]
- **Error Category**: [Specific category like "Query Validation Timeout", "Invalid Schema", "Dependency Failure" - NOT generic terms]
- **Primary Error Code**: [HTTP status code and specific exception type, e.g., "HTTP 500 - DraftTimeoutException"]
- **Error Message Pattern**: [Specific error from RP traces, e.g., "Draft request timed out: The request was canceled due to the configured HttpClient.Timeout of 70 seconds elapsing"]

### 🚨 Error Details & Dependency Responses
- **Primary Error Message**: 
  ```
  [Complete error message from RP traces - must include full exception type and message]
  Example: AlertRP.Models.Common.Exceptions.Draft.DraftTimeoutException: Draft request timed out...
  ```
- **HTTP Response Code**: [Full HTTP status code and description]
- **Dependency Failure Details**:
  - **Dependency Name**: [Specific service/component that failed, e.g., "Draft Service (Query Validation Service)"]
  - **Dependency Response**: 
    ```
    [Complete dependency response/error message from RP traces]
    Example: System.Net.Sockets.SocketException (995): The I/O operation has been aborted...
    ```
  - **Timeout/Connection Details**: [Specific timeout values, connection error details]
- **Operation Context**: [Describe what operation was being performed when it failed]

### 🔧 Technical Details
- **Failed Component**: [Specific service/component that failed for {targetResourceType}, e.g., "Draft Client (Query Validation Service) - LSA Regional Endpoint"]
- **Resource Type Specific Patterns**: [Patterns specific to {targetResourceType}, e.g., "All failures were PUT operations on SCHEDULEDQUERYRULES"]
- **Error Type Breakdown** (for {targetResourceType} only):
  - [Specific Error Type]: [Count] occurrences - [Brief description of error and its cause]
  - Example: "Draft Timeout Exception: 136 occurrences - Query validation exceeding 70-second HTTP client timeout"
- **Root Cause Code Path** (if available from RP traces):
  ```
  [Code path showing where the error occurred]
  Example: QueryValidator.ValidateQueryInternalAsync() → DraftClient.ExecuteQueryAsync() → Timeout
  ```

### 📋 Activity ID Analysis
- **Total Activity IDs**: [Count] (for {targetResourceType} only)
- **Detailed Activity ID Breakdown** (for {targetResourceType}):
  - `[ActivityId1]`:
    - **Operation**: [Specific operation that failed]
    - **Error Message**: `[Complete error message - no truncation]`
    - **HTTP Status**: [Status code and description]
    - **Dependency Response**: `[Complete dependency response if available]`
  - `[ActivityId2]`:
    - **Operation**: [Specific operation that failed]
    - **Error Message**: `[Complete error message - no truncation]`
    - **HTTP Status**: [Status code and description]
    - **Dependency Response**: `[Complete dependency response if available]`
  - `[ActivityId3]`:
    - **Operation**: [Specific operation that failed]
    - **Error Message**: `[Complete error message - no truncation]`
    - **HTTP Status**: [Status code and description]
    - **Dependency Response**: `[Complete dependency response if available]`

### 🌐 Regional Impact Details
| Region | Failed Operations ({targetResourceType}) | Success Rate | Primary Error |
|---|---|---|---|
| Region1 | [Count] | [%] | [Error Type] |
| Region2 | [Count] | [%] | [Error Type] |

---
```

### Reporting Guidelines (targetResourceType Specific)

- **Drop Timestamp**: Use precise UTC timestamp from Inner Step 1.1 findings (for {targetResourceType} only)
- **Impacted Regions**: List all regions from Inner Step 1.2 analysis WITH failure counts in parentheses, e.g., "westeurope (132), eastus2 (24)" (for {targetResourceType} only)
- **Number of Impacted Subscriptions**: Exact count from Inner Step 1.3 analysis (for {targetResourceType} only)
- **Resource Type Focus**: All metrics and analysis must be specific to the {targetResourceType} parameter provided
- **Severity**: Determine based on {targetResourceType} specific metrics:
  - 🔴 **High**: >1000 impacted subscriptions OR >5 regions OR success rate <95% (for {targetResourceType})
  - 🟡 **Medium**: 100-1000 impacted subscriptions OR 2-5 regions OR success rate 95-99% (for {targetResourceType})
  - 🟢 **Low**: <100 impacted subscriptions OR 1 region OR success rate >99% (for {targetResourceType})

### Root Cause Analysis Requirements (targetResourceType Specific)

**CRITICAL: You MUST investigate EVERY SINGLE SLI drop identified in Inner Step 1.1 for the specified {targetResourceType}. Do NOT skip any drop, regardless of how many drops are found.**

For each and every drop for the {targetResourceType}, you MUST provide:

1. **Specific Error Details from RP Traces**: Use actual error messages, exception types, and stack traces from RP traces - NOT generic "HTTP 500 Internal Server Error"
2. **Clear Error Classification**: Categorize errors by SPECIFIC type (e.g., "Query Validation Timeout", "Invalid Schema Error", "Dependency Connection Failure") - NOT generic categories
3. **Operation Breakdown**: Specify which CRUD operations were affected for {targetResourceType}
4. **Component Identification**: Identify SPECIFIC service/component and code path that caused the failure (from RP traces)
5. **Error Pattern Analysis**: Identify the most common error messages WITH actual error text from logs
6. **Regional Impact**: Show how different regions were affected WITH failure counts for each region

### Mandatory Investigation Process (targetResourceType Specific)

**ENFORCE: Complete root cause analysis for ALL drops for the specified {targetResourceType}**

1. **Count Verification**: Before proceeding to reporting, verify that the number of detailed root cause analysis sections matches exactly the number of SLI drops identified in Inner Step 1.1 for {targetResourceType}
2. **Drop-by-Drop Investigation**: For each drop timestamp found for {targetResourceType}:
   - Execute Inner Step 1.2 (Regional Breakdown for {targetResourceType})
   - Execute Inner Step 1.3 (Impacted Subscriptions for {targetResourceType})
   - Execute Inner Step 1.4 (ActivityId Extraction for {targetResourceType})
   - Execute Inner Step 1.5 (Root Cause Analysis via RP Traces for {targetResourceType})
   - Create detailed root cause analysis section in report for {targetResourceType}
3. **No Sampling Rule**: Investigate ALL data for each drop for {targetResourceType} - do not sample or summarize
4. **Completeness Check**: Each drop must have its own complete "🔍 Root Cause Analysis for Drop: [TIMESTAMP] - {targetResourceType}" section

### Investigation Discipline (targetResourceType Specific)

- **One-to-One Mapping**: Each SLI drop for {targetResourceType} = One complete root cause analysis section
- **No Skipping**: Even minor drops or drops with similar patterns must be individually investigated for {targetResourceType}
- **Full Data Analysis**: Use complete ActivityId lists and full trace data for each drop for {targetResourceType} only
- **Individual Documentation**: Each drop gets its own detailed analysis for {targetResourceType}, not grouped summaries
- **Resource Type Focus**: All analysis must be filtered and focused exclusively on the specified {targetResourceType}

### Additional Requirements (targetResourceType Specific)

- Use emojis and visual indicators for clarity
- Include quantitative data (counts, percentages, timelines) specific to {targetResourceType}
- Provide sample Activity IDs with their specific error details for {targetResourceType} operations
- Include summary statistics at the top of each report for {targetResourceType} only
- **Resource Type Validation**: Ensure all data points, metrics, and analysis relate exclusively to the specified {targetResourceType}
- **VERIFICATION STEP**: Before completing the report, count that you have the same number of detailed root cause analysis sections as SLI drops identified for {targetResourceType}
- You MUST do this step.

## Summary of Changes

This workflow has been updated to:

1. **Require targetResourceType Parameter**: The workflow now mandates receiving one of the four valid targetResourceType values before starting
2. **Filter All Queries**: Every query and analysis step now includes filtering by the specified targetResourceType
3. **Single Report Focus**: Instead of generating 3 separate files, the workflow now generates one focused report for the specified targetResourceType
4. **Targeted Analysis**: All investigation steps, root cause analysis, and reporting focus exclusively on the provided targetResourceType
5. **Clear Resource Type Context**: All report sections, tables, and analysis explicitly reference the targetResourceType being investigated
6. **Enhanced Error Details Collection**: Added mandatory capture of complete error messages, HTTP status codes, dependency responses, and stack traces with no truncation or summarization
7. **Comprehensive Root Cause Analysis**: Enhanced reporting template with dedicated sections for error details, dependency responses, and detailed Activity ID breakdown including complete error context

**Usage**: When invoking this workflow, provide one of these targetResourceType values:
- `SCHEDULEDQUERYRULES`
- `METRICALERTS`
- `ACTIVITYLOGALERTS`
- `PROMETHEUSRULEGROUPS`

The investigation will then focus exclusively on that resource type's SLI performance and issues.
