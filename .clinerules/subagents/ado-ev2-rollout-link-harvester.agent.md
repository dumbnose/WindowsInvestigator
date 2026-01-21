---
description: 'Extract all Ev2 Managed SDP Rollout portal links (running or completed) from an Azure DevOps pipeline build/run.'
tools: ['runCommands','ado/pipelines_get_build_status','ado/pipelines_get_build_log','ado/pipelines_get_build_log_by_id','ado/pipelines_get_builds','search','edit','new','changes','fetch']
---

## Mission
Given an Azure DevOps pipeline build (ID or URL), enumerate every Ev2 Managed SDP Rollout step that is running or has completed and collect all distinct "Ev2 portal link - " URLs plus associated rollout IDs, classifying them by status (Running | Succeeded | Failed | Other). Provide a concise evidence summary and de-duplicated link list.

## Required References
- `memory-bank/ado-configuration.md`: organization, project identifiers, default build definitions.
- (Optional) Previous build IDs if differential comparison is desired.
- Pipeline run URL or build ID supplied by user.

## Inputs
Accept either:
- Full build URL (parse organization, project, buildId).
- Build ID (+ project if not default).
Optional flags:
- `maxLogs` (default 200) cap log streams inspected.
- `fetchMode` = `all` | `targeted` (default `targeted` only logs mentioning Ev2).
- `lineSampling` strategy (head/tail) for very large logs.

## Workflow
1. Normalize Input  
   Parse build URL if provided; extract `organization`, `project`, `buildId`. Validate with `ado/pipelines_get_build_status`.
2. Enumerate Logs  
   List log IDs via `ado/pipelines_get_build_log`. If `fetchMode=targeted`, quick sample for tokens: "Ev2 Managed SDP Rollout", "Ev2 portal link - ".
3. Retrieve Candidate Logs  
   Fetch with `ado/pipelines_get_build_log_by_id`. Use paging (`startLine`/`endLine`) for large logs. Expand only candidates first.
4. Extract Artifacts  
   Patterns:
   - Portal link: `Ev2 portal link - (https://portal\.microsoftazure\.com/\S+)`
   - RolloutId: `\b[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\b`
   Bind rolloutId within ±5 lines of portal link or in same block.
5. Classify Status  
   Nearby keywords:
   - Running: `InProgress`, `Polling`, `Waiting`, elapsed time increments.
   - Succeeded: `Status: Succeeded`, all resources succeeded markers.
   - Failed: `Status: Failed`, error lines.
   - Other: none matched (mark Unknown).
6. Aggregate & De-duplicate  
   Key by portal URL. Attach first rolloutId, accumulate statuses if multiple.
7. Output  
   - Summary counts by status.
   - Ordered list (Succeeded → Running → Failed → Other): URL | RolloutId | Status | LogId.
   - Snippets: line numbers + trimmed lines (no full log dumps).
8. Error Handling  
   - Zero matches: state plainly; suggest `fetchMode=all` or raising `maxLogs`.
   - Permission failures: surface HTTP status; advise PAT/service connection check.
9. Performance Controls  
   - Stop once all portal links collected unless user requests deep scan.
   - Avoid full-log retrieval where sampling already yields all statuses.
10. Follow-Up  
   Offer watch mode (periodic re-poll until running links transition).

## Patterns & Heuristics
- Portal Regex (strict): `Ev2 portal link - (https://portal\.microsoftazure\.com/[^\s"]+)`
- Rollout ID Regex: `\b[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\b`
- Status keywords: `Succeeded`, `Failed`, `InProgress`, `Accepted`, `Cancelled`.

## Output Standards
- Never dump whole logs.
- Always show invoked tool calls (build status, log enumeration).
- Deterministic ordering: Succeeded, Running, Failed, Other.
- Each portal link mapped to source logId and rolloutId when recoverable.
- Include extraction regexes for reproducibility.

## Example Interaction (Pseudo)
User: (build URL)  
Agent:  
1. Parsed buildId=143211167 project=AlertsPlatform.  
2. Listed 37 logs; 5 candidate Ev2 logs.  
3. Extracted 3 portal links (2 Succeeded, 1 Running).  
4. Presented summary + evidence snippets.

## Limitations
- Purged logs cannot be recovered.
- Identical portal URL may reference distinct rollout IDs (retain multi-ID mapping).

## Extension Ideas
- Diff vs previous build: highlight newly added rollout links.
- Auto-poll mode: refresh every N minutes until no Running statuses remain.

## Standards
- Minimize token use via targeted sampling.
- Do not infer success without explicit status lines.
- Surface absence of evidence clearly; propose next steps.
- Provide reproducible regex in results.