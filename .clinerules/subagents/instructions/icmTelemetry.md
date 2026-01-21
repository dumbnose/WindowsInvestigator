# IcM Telemetry Reference

## Cluster & Database
- **Cluster URI**: `https://icmcluster.kusto.windows.net`
- **Database**: `IcMDataWarehouse`
- **Connection string example**: `Data Source=https://icmcluster.kusto.windows.net;Initial Catalog=IcMDataWarehouse;AAD Federated Security=True`
- Authenticate with Microsoft Entra ID (AAD). Service principals or managed identities must be granted at least viewer permissions on the database.

## Dataset Characteristics
- IcM telemetry is ingested on a frequent schedule, but expect up to a few minutes of lag behind the IcM portal.
- Schemas evolve; always run `.show tables` or `.show table <name> schema` after connecting to confirm column names before automating queries.
- Key entities:
  - **Incident**-level facts: incident metadata (severity, state, owning team, timestamps).
  - **Timeline updates**: investigation notes, routing actions, mitigations.
  - **Classification data**: tags, custom fields, customer vs. platform ownership.
  - **Relationships**: links to support requests, similar incident identifiers, and work item references.

## Getting Started
1. Connect with preferred tooling (Kusto Explorer, Kusto Web UI, or `az data explorer query`).
2. Verify schema:
   ```kusto
   .show tables | where TableName startswith "Incident"
   ```
3. Inspect specific table columns when needed:
   ```kusto
   .show table Incident schema
   ```

## Core Query Patterns
### Single Incident Snapshot
```kusto
let targetIncidentId = 51000000775185;
Incident
| where IncidentId == targetIncidentId
| project IncidentId, Title, Severity, State, CreatedDate, MitigateTime, OwningTeamId, SubscriptionId
```

### Timeline & Updates
```kusto
let targetIncidentId = 51000000775185;
IncidentUpdate
| where IncidentId == targetIncidentId
| sort by UpdateTime asc
| project UpdateTime, UpdateType, UpdatedBy, Summary
```

### Custom Fields or Tags
If the `IncidentCustomField` or `IncidentTag` tables are present, correlate them for richer context:
```kusto
let targetIncidentId = 51000000775185;
IncidentCustomField
| where IncidentId == targetIncidentId
| project IncidentId, CustomFieldName, CustomFieldValue
```

## Finding Similar Incidents by Owning Team
Once the CRI investigation identifies the failure signature, reuse that knowledge under the same `OwningTeamId` to surface other affected incidents.

### Step 1: Capture Anchor Context
```kusto
let targetIncidentId = 51000000775185;
let anchor = Incident
    | where IncidentId == targetIncidentId
    | project AnchorIncidentId = IncidentId,
              AnchorOwningTeamId = OwningTeamId,
              AnchorCreatedDate = CreatedDate;
```

### Step 2: Distill the Catch Phrase
During the investigation note the exact failure wording (for example, `"SyntaxError (SYN0002 at token 'Connection')"`). Store it in a variable so the search remains auditable:
```kusto
let catchPhrase = "SyntaxError (SYN0002 at token 'Connection')";
```

### Step 3: Search Within the Same Owning Team
Use the Kusto `search` operator to scan timeline text, mitigation summaries, and titles for the same catch phrase restricted to the team:
```kusto
Incident
| where OwningTeamId == toscalar(anchor.AnchorOwningTeamId)
| where IncidentId != targetIncidentId
| project IncidentId, Title, Severity, State, CreatedDate, OwningTeamId
| search catchPhrase
| top 100 by CreatedDate desc
```

### Step 4: Inspect Matches
Pivot into updates or custom fields for the returned incidents to confirm the failure aligns with the original catch phrase:
```kusto
let relatedIncidentIds =
    Incident
    | where OwningTeamId == toscalar(anchor.AnchorOwningTeamId)
    | where IncidentId != targetIncidentId
    | search catchPhrase
    | project IncidentId;

IncidentUpdate
| where IncidentId in (relatedIncidentIds)
| project IncidentId, UpdateTime, UpdateType, Summary
| top 50 by UpdateTime desc
```

Record any additional incidents you confirm in the investigation log so downstream stakeholders see the blast radius analysis.

## Operational Tips
- Always confirm access complies with incident data handling policies—do not export customer data outside approved channels.
- Use parameterized Kusto functions for reusable filters (e.g., `get_incident_history(incidentId)`), stored under the same database.
- Combine IcM data with internal telemetry (e.g., `lsa_requests`) via time correlation, but keep cross-environment data joins in downstream tooling because cross-cluster joins are not supported directly.
- Document every query used during investigations in the corresponding `memory-bank/investigations/<icm-id>.md` file for auditability.

## Troubleshooting Connection Issues
- If authentication fails, verify the account or service principal has viewer access to `IcMDataWarehouse` and that conditional access policies allow Kusto queries.
- For schema or permission errors, run `.show queries` to confirm the database is reachable and contact the IcM data engineering team if tables are unavailable.

This reference should be stored alongside other telemetry guides (`memory-bank/telemetry.md`) and updated whenever the IcM Data Warehouse schema changes or new similarity heuristics are adopted.
