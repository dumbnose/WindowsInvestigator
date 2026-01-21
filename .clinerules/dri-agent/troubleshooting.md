---
title: Global Troubleshooting Guidelines
skill_type: reference
generic: true
applies_to: common
version: 1.0
updated: 2025-04-27
tags:
  - troubleshooting
  - best-practices
  - investigation
dependencies: []
---

# Purpose

Standardized investigation patterns and best practices for troubleshooting across all services. This guide focuses on the technical analysis methodology; for documentation workflow see [Investigation Process Guide](investigation-process.md).

# Time Binning Rules

When querying metrics, use appropriate bin sizes based on time range:

| Time Range | Bin Size | Use Case |
|------------|----------|----------|
| < 1 hour   | 1m      | Recent, detailed analysis |
| 1-3 hours  | 5m      | Short-term trends |
| 3-12 hours | 15m     | Medium-term patterns |
| > 12 hours | 1h      | Long-term analysis |

# Investigation Framework

## 1. Dependency Health Check

First step in any investigation is to verify health of service dependencies:

- Check each dependency listed in service documentation
- Look for correlation between dependency issues and service impact
- Cross-reference dependency issues with service logs

If dependency status is unclear or metrics are inconclusive, use the `ask_followup_question` tool to request guidance:

```
<ask_followup_question>
<question>Specific question about unclear dependency state</question>
<options>["Suggested action 1", "Suggested action 2", ...]</options>
</ask_followup_question>
```

## 2. Initial Assessment

- Confirm current status
- Define impact scope
- Identify time of onset
- List affected components

## 3. Data Collection

- Gather relevant metrics
- Pull error logs
- Check recent changes
- Review similar incidents

## 4. Impact Analysis

- Number of affected users/requests
- Geographic spread
- Performance degradation level
- Business impact

## 5. Root Cause Analysis

1. Review timeline:
   - When did symptoms start?
   - Any correlated events?
   - Recent deployments?

2. System boundaries:
   - Internal components
   - External dependencies
   - Network paths
   - Resource limits

3. Pattern recognition:
   - Is this a known issue?
   - Similar past incidents?
   - Common failure modes?

## 6. Mitigation Steps

1. Immediate actions:
   - Stop customer impact
   - Apply temporary fixes
   - Scale resources if needed

2. Validation:
   - Verify fix effectiveness
   - Monitor recovery
   - Check for side effects

3. Communication:
   - Update stakeholders
   - Document actions taken
   - Share findings

# Best Practices

## Log Analysis

1. Start broad, then narrow:
   - Filter by time range
   - Add component filters
   - Refine error types

2. Group effectively:
   - By error type
   - By component
   - By severity

3. Look for patterns:
   - Error frequency
   - Error distribution
   - Impact correlation

## Resource Analysis

1. Check utilization:
   - CPU/Memory trends
   - Network patterns
   - Storage metrics

2. Review limits:
   - Quotas
   - Throttling
   - Capacity

3. Examine scaling:
   - Instance counts
   - Load distribution
   - Auto-scale triggers

# Common Investigation Mistakes

## Time-Related

1. Wrong time zone assumptions
2. Missing daylight savings changes
3. Insufficient historical context
4. Too small/large time ranges

## Scope-Related

1. Too narrow initial focus
2. Missing dependency impacts
3. Overlooking regional differences
4. Incomplete component coverage

## Analysis-Related

1. Premature conclusions
2. Correlation ≠ causation
3. Ignoring baseline metrics
4. Missing periodic patterns

# Documentation Requirements

## During Investigation

1. Timeline of findings
2. Queries used
3. Metrics observed
4. Actions taken

## Post-Resolution

1. Root cause summary
2. Mitigation steps
3. Prevention measures
4. Lessons learned

# Quick Reference

## Time Zone Handling

- Always use UTC in queries
- Convert timestamps explicitly
- Document local time with offset

## Metric Baseline

- Daily patterns
- Weekly cycles
- Monthly trends
- Known peaks

## Scale Understanding

- Normal ranges
- Warning thresholds
- Critical limits
- Recovery times
