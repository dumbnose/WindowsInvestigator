---
title: Investigation Process Guide
skill_type: skill
generic: true
applies_to: common
version: 1.1
updated: 2025-05-06
tags:
  - process
  - workflow
  - documentation
dependencies:
  - troubleshooting.md
  - metrics.md
  - telemetry.md
---

# Purpose

Standardized workflow for documenting and managing investigations across all tickets. Defines where and how to store investigation notes, progress, and findings.

# Investigation File Workflow

## File Creation

1. On new ticket, check if `memory-bank/investigations/{ticketId}.md` or `memory-bank/investigations/{ticketId}-*.md` exists:
   - If exists: read for context/previous findings
   - If not: create new file at "`memory-bank/investigations/{ticketId}-{ticket short description}.md`" with initial investigation plan

## File Structure

```markdown
# Investigation: {Title} - Ticket {ID}

## Details
- Tenant: {tenant}
- Impact Date: {date}
- Issue: {brief description}

## Investigation Plan
Reference Documents: troubleshooting.md | metrics.md | telemetry.md
1. Step 1
2. Step 2
...

## Progress & Findings
{chronological updates as investigation proceeds}

## Final Conclusion
{summary of findings and recommendations}
Supporting Evidence Query: {Kusto link or saved query reference demonstrating conclusion}

## Investigation Timeline
{mandatory Gantt chart visualization of the entire incident timeline}
```

## Timeline Requirements

> ⚠️ **MANDATORY TIMELINE REQUIREMENT**  
> Every investigation MUST include a Gantt chart timeline visualization showing:
>
> **SCOPE: INCIDENT TIMELINE ONLY**
> The Gantt chart should document ONLY the production incident timeline - what happened to the systems and services during the actual incident. Do NOT include investigation activities, analysis steps, or when the investigation team discovered information.
>
> **1. Key Events (chronological order) - PRODUCTION EVENTS ONLY**
>
> - First system detection of issue (alerts, monitoring)
> - System state changes that occurred during incident
> - Configuration changes made during incident
> - Deployments or rollbacks performed during incident
> - Mitigation attempts made during incident
> - Incident resolution time (when systems returned to normal)
>
> **2. Impact Windows**
>
> - Clear start/end markers
> - Severity levels
> - Customer impact metrics
> - Error rates or degradation levels
>
> **3. Correlations**
>
> - Relationship between events and impact changes
> - System behavior changes
> - Performance metric variations
>
> **4. System State**
>
> - Before incident state (baseline)
> - During incident state changes
> - After resolution state
>
> **WHAT NOT TO INCLUDE:**
> - When investigation started
> - When root cause was identified by investigators  
> - When investigation completed
> - Analysis steps or query execution times
> - Investigation team activities
> - Discovery timestamps (when investigators found evidence)
>
> Format Requirements:
>
> - Use Mermaid Gantt syntax
> - Include timeline scale
> - Color-code different event types
> - Add clear annotations for each event
>
> ```mermaid
> gantt
>    title Investigation Timeline
>    dateFormat  YYYY-MM-DD HH:mm
     axisFormat  %H:%M
>    section Impact
>    High Severity     :crit, active, impact1, TIMESTAMP, TIMESTAMP
>    section Events
>    Issue Detected    :done, detect1, TIMESTAMP, 5m
>    Deploy Fix       :done, deploy1, after detect1, 15m
>    Verify Resolution :done, verify1, after deploy1, 10m
> ```
>
```

## Documentation Requirements

### Query Result Visualization

> ⚠️ **MANDATORY QUERY VISUALIZATION REQUIREMENT**  
> For EACH QUERY that produces time-series data:
>
> 1. Include the full query in a code block with comments
> 2. Add the query results or summary
> 3. Create a visualization using **ONLY xychart-beta**
> 4. Provide your analysis of the results
>
> Example:
> ```kql
> // Check partition lag during impact window
> evaluate geneva_metrics_request('{mdmAccount}', 'metricNamespace("{mdmNamespace}")...')
> ```
> 
> Result: Found 3 partitions with lag > 200 minutes
>
> ```mermaid
> xychart-beta
>     title "Partition Lag Trend"
>     x-axis ["T1", "T2", "T3", "T4"]
>     y-axis "Lag (minutes)" 0 --> 250
>     line [180, 200, 220, 205]
> ```
> 
> Analysis: Issue appears specific to partitions rather than system-wide
>
> **Important:** Always quote x-axis values in xychart-beta. For time-series data:
> ```mermaid
> xychart-beta
>     title "Error Rate Over Time"
>     x-axis ["23:30", "23:35", "23:40", "23:45"]
>     y-axis "Error Count" 0 --> 3000
>     line [2013, 618, 1013, 2740]
> ```

## Chart Type Enforcement

| Data Type | Required Chart | Location | Purpose |
|-----------|----------------|----------|---------|
| Query Results (time-series) | xychart-beta | Immediately after query results | Visualize metric trends from queries |
| Investigation Timeline | Gantt | End of file in "## Investigation Timeline" section | Show overall incident timeline with events |

## Progress Documentation

- Document each step attempted
- Update file in real-time as investigation progresses
- Include both successful and failed attempts

## Investigation Focus

- Focus only on the current ticket
- Ignore unrelated tickets/issues
- Reference previous investigations only if directly relevant

# System Connections

## ADX Connection Details

- Cluster: https://azalertsprodweu.westeurope.kusto.windows.net
- Database: LogSearchRule

# Execution Guidelines

## Step Execution

- Document each step before executing
- Record outcome immediately after execution
- If a step fails or requires clarification:
  1. Use the `ask_followup_question` tool to request guidance:

     ```
     <ask_followup_question>
     <question>Clear description of the problem or needed input</question>
     <options>["Option 1", "Option 2", "Other action"]</options>
     </ask_followup_question>
     ```

  2. Wait for human response
  3. Document both question and response in the investigation file
  4. Proceed based on the guidance received

Example usage:

```

<ask_followup_question>
<question>Query returned no results in 1h window. Should I expand the time range?</question>
<options>["Yes, try 3h", "No, check different metrics", "Skip this check"]</options>
</ask_followup_question>

```

Remember: Always provide clear context in the question and document the interaction.

## Progress Updates

- Keep findings chronological
- Include timestamps for significant events
- Link evidence (metrics, logs) to conclusions
- Note attempted mitigations and their results

# Progress Tracking

## Status Updates

- Current state of investigation
- Blockers encountered
- Next steps planned
- Open questions

## Resolution Documentation

- Root cause identified
- Steps taken to mitigate
- Prevention recommendations
- Lessons learned

# Related Documentation

- [Troubleshooting Guide](troubleshooting.md) - What to analyze
- [Logs Telemetry Guide](telemetry.md) - How to query logs
- [Metrics Guide](metrics.md) - How to query metrics
