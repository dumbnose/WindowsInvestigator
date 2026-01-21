# Kusto Investigation Mandatory Process

## CRITICAL RULE: Context Reading Before Tool Use

When a user requests Kusto analysis, you MUST:

1. **ALWAYS read these files FIRST** (no exceptions):
   - `memory-bank/kusto-analysis-patterns.md` 
   - `memory-bank/telemetry.md`
   - `memory-bank/systemPatterns.md` (for architecture understanding)

2. **Follow the structured workflow** outlined in these files

3. **Never use kustoanalysis MCP tools** until you have read and understood the prerequisite documentation

4. **Document your investigation plan** based on the guidance before executing queries

## Enforcement
- Any deviation from this process is considered a critical error
- Tool usage without context reading violates investigation methodology