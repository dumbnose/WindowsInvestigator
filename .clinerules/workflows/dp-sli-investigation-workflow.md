# DP SLI Investigation Workflow

**When to use:** For investigating Service Level Indicator (SLI) drops, service degradation incidents, and proactive monitoring of service health.

This workflow provides a systematic approach for Log Search Alerts SLI investigation that integrates global monitoring with detailed root cause analysis.

## 🎯 Workflow Overview

This workflow provides a systematic approach to:

- 🔍 Run global SLI investigation to detect drops under 99.9%
- 📊 Stop if no drops are found globally
- 🌍 Identify relevant regions when drops are detected
- 👥 Calculate impact by subscription and rule ID
- 📋 Create investigation table for further analysis
- 🔧 Run investigation for each table row using the DRI-agent for root cause analysis with dependencies and exceptions tables

### Example User Requests
```
/dp-sli-investigation-workflow.md from 2025-08-31 to 2025-09-07
```

## 📋 Workflow Steps

### Step 1: Global SLI Investigation

**Process:** Run global SLI investigation using Log Search Alerts process

**🚨 CRITICAL REQUIREMENTS:**

- **📊 Data Binning**: Always bin the data by 1h maximum for temporal analysis
- **📈 Trend Analysis**: Look at the trend and check for drops under 99.9% per 1h
- **🛑 Stop Condition**: If no drops under 99.9% are found, stop investigation here

**Prerequisites:**

- Read and use `memory-bank/sli-investigation.md`
- Connect to cluster: `azalertsprodweu.westeurope.kusto.windows.net`
- Database: `LogAlertsScheduler`

**Step 1a: Execute Global SLI Health Check with 1h Binning**

```kql
LogAlertsSchedulerSliLogs 
| where MetricName == "RuleEvaluationSuccessRate" 
| where TIMESTAMP between(datetime(YYYY-MM-DD) .. datetime(YYYY-MM-DD)) 
| extend ExcludeFromSli = tostring(MetricInfo.Dimensions.ExcludeFromSli),
         MonitoringSystem = tostring(MetricInfo.Dimensions.MonitoringSystem),
         AlertCondition = tostring(MetricInfo.Dimensions.AlertCondition),
         Success = tostring(MetricInfo.Dimensions.Success) 
| where ExcludeFromSli == "False" 
| where MonitoringSystem == "Log Search Alerts" 
| where AlertCondition == "Fired" 
| summarize Expected = count(), Completed = countif(Success == "True") by bin(TIMESTAMP, 1h)  // 🚨 CRITICAL: 1h binning required
| project TIMESTAMP, Expected, Completed, SuccessRate = round((todouble(Completed)*100.0)/todouble(Expected), 3) 
| extend SliStatus = case(
    SuccessRate >= 99.9, "✅ HEALTHY",
    SuccessRate >= 99.5, "⚠️ DEGRADED", 
    SuccessRate >= 99.0, "🔴 CRITICAL",
    "🚨 EMERGENCY"
)
| order by TIMESTAMP asc
```

**Step 1b: Trend Analysis - Check for Drops Under 99.9%**

- **🎯 Primary Focus**: Look at trend and identify any periods where `SuccessRate < 99.9`
- **📊 Temporal Pattern**: Analyze hourly trend to identify degradation patterns
- **🛑 Decision Point**:
  - **✅ No drops under 99.9%**: Investigation complete - service is healthy, **STOP HERE**
  - **🔴 Drops detected**: Continue with regional and impact analysis

**Step 1c: SLI Drop Detection Query**

```kql
LogAlertsSchedulerSliLogs 
| where MetricName == "RuleEvaluationSuccessRate" 
| where TIMESTAMP between(datetime(YYYY-MM-DD) .. datetime(YYYY-MM-DD)) 
| extend ExcludeFromSli = tostring(MetricInfo.Dimensions.ExcludeFromSli),
         MonitoringSystem = tostring(MetricInfo.Dimensions.MonitoringSystem),
         AlertCondition = tostring(MetricInfo.Dimensions.AlertCondition),
         Success = tostring(MetricInfo.Dimensions.Success) 
| where ExcludeFromSli == "False" 
| where MonitoringSystem == "Log Search Alerts" 
| where AlertCondition == "Fired" 
| summarize Expected = count(), Completed = countif(Success == "True") by bin(TIMESTAMP, 1h)
| project TIMESTAMP, Expected, Completed, SuccessRate = round((todouble(Completed)*100.0)/todouble(Expected), 3) 
| extend SliStatus = case(
    SuccessRate >= 99.9, "✅ HEALTHY",
    SuccessRate >= 99.5, "⚠️ DEGRADED", 
    SuccessRate >= 99.0, "🔴 CRITICAL",
    "🚨 EMERGENCY"
)
| where SuccessRate < 99.9  // 🚨 Only show drops under 99.9%
| order by TIMESTAMP asc
```

**Decision Point:**

- **✅ No drops under 99.9%**: Investigation complete - service is healthy
- **🔴 Drops detected**: Continue with Step 2 for regional and impact analysis

### Step 2: Regional Impact and Customer Analysis (Only if drops detected)

**Process:** Identify relevant regions and calculate detailed impact metrics

**Step 2a: Regional Impact Analysis**

```kql
LogAlertsSchedulerSliLogs 
| where MetricName == "RuleEvaluationSuccessRate" 
| where TIMESTAMP between(datetime(YYYY-MM-DD) .. datetime(YYYY-MM-DD)) 
| extend ExcludeFromSli = tostring(MetricInfo.Dimensions.ExcludeFromSli),
         MonitoringSystem = tostring(MetricInfo.Dimensions.MonitoringSystem),
         AlertCondition = tostring(MetricInfo.Dimensions.AlertCondition),
         Success = tostring(MetricInfo.Dimensions.Success),
         Location = tostring(MetricInfo.Dimensions.MonitoringSystemLocation)
| where ExcludeFromSli == "False" 
| where MonitoringSystem == "Log Search Alerts" 
| where AlertCondition == "Fired" 
| summarize Expected = count(), Completed = countif(Success == "True") by Location, bin(TIMESTAMP, 1h) 
| project TIMESTAMP, Location, Expected, Completed, SuccessRate = round((todouble(Completed)*100.0)/todouble(Expected), 3)
| extend SliStatus = case(
    SuccessRate >= 99.9, "✅ HEALTHY",
    SuccessRate >= 99.5, "⚠️ DEGRADED", 
    SuccessRate >= 99.0, "🔴 CRITICAL",
    "🚨 EMERGENCY"
)
| where SuccessRate < 99.9
| order by TIMESTAMP asc, Location
```

**Step 2b: Subscription and Rule Impact Analysis**

```kql
LogAlertsSchedulerSliLogs 
| where MetricName == "RuleEvaluationSuccessRate" 
| where TIMESTAMP between(datetime(YYYY-MM-DD) .. datetime(YYYY-MM-DD)) 
| extend ExcludeFromSli = tostring(MetricInfo.Dimensions.ExcludeFromSli),
         MonitoringSystem = tostring(MetricInfo.Dimensions.MonitoringSystem),
         AlertCondition = tostring(MetricInfo.Dimensions.AlertCondition),
         Success = tostring(MetricInfo.Dimensions.Success),
         SubscriptionId = tostring(MetricInfo.Dimensions.SubscriptionId),
         RuleId = tostring(MetricInfo.Dimensions.RuleId),
         Location = tostring(MetricInfo.Dimensions.MonitoringSystemLocation)
| where ExcludeFromSli == "False" 
| where MonitoringSystem == "Log Search Alerts" 
| where AlertCondition == "Fired" 
| where Success == "False"
| where isnotempty(SubscriptionId) and isnotempty(RuleId)
| summarize 
    FailedEvaluations = count(),
    DistinctImpactedSubscriptions = dcount(SubscriptionId),
    DistinctImpactedRules = dcount(RuleId),
    SampleFailedRules = make_set(RuleId, 10),
    AffectedRegions = make_set(Location)
by bin(TIMESTAMP, 1h)
| project TIMESTAMP, FailedEvaluations, DistinctImpactedSubscriptions, DistinctImpactedRules, SampleFailedRules, AffectedRegions
| order by TIMESTAMP asc
```

### Step 3: Investigation Priority Table Creation

**Process:** Create prioritized table of issues requiring further investigation

**Step 3a: Generate Investigation Priority Matrix**

Create a table with the following structure for each detected drop:

| 🕐 Time Period | 🌍 Region | 📊 Success Rate | 🚨 Severity | 👥 Subscription Impact | 📋 Rule Impact | 🔍 Investigation Priority |
|----------------|-----------|-----------------|-------------|------------------------|----------------|---------------------------|
| YYYY-MM-DD HH:00 | Region Name | XX.X% | Critical/Emergency | X subscriptions | X rules | High/Medium/Low |

**Step 3b: Prioritization Criteria**

- **🔴 High Priority**: Success rate < 99.0%, >100 impacted subscriptions, >500 impacted rules
- **🟡 Medium Priority**: Success rate 99.0-99.4%, 50-100 impacted subscriptions, 100-500 impacted rules  
- **🟢 Low Priority**: Success rate 99.5-99.8%, <50 impacted subscriptions, <100 impacted rules

**Step 3c: Investigation Scope Definition**
For each priority item, define:

- Target time window for root cause analysis
- Specific regions to investigate
- Expected investigation depth (dependencies + exceptions vs exceptions only)

### Step 4: DRI-Agent Root Cause Investigation

**Process:** For each priority issue from Step 3, conduct detailed root cause analysis using DRI-agent methodology

**Prerequisites:**

1. **Read DRI-Agent Documentation**: Follow instructions in `.clinerules/04-livesite-investigation-process.md`
2. **Read Investigation Methodology**: Review `.clinerules/subagents/dri-agent/investigation-methodology.md` for systematic approach
3. **Connect to LogAlertsScheduler Database**: Use appropriate Kusto cluster for LSA telemetry

**Step 4a: Setup Investigation Documentation**
Create investigation file for each priority issue:

```
Naming convention: sli-investigation-{start-date}_{end-date}_{region}-root-cause.md
Example: sli-investigation-2025-06-11_2025-06-11_japaneast-root-cause.md
```

**Step 4b: Dependencies Analysis (lsa_dependencies table)**
**Process:** Investigate external service failures for each affected region and time period

**CRITICAL: Use the FULL incident timeframe identified in Steps 1-2**

```kql
// Dependencies analysis for specific region and timeframe
lsa_dependencies
| where timestamp between(datetime({incident_start}) .. datetime({incident_end}))
| extend Location = tostring(customDimensions.Location)
| where Location == "{target_region}"  // From Step 2 regional analysis
| where success == "False"
| summarize 
    FailureCount = count(),
    TotalRequests = count(),
    FailureRate = round(countif(success == "False") * 100.0 / count(), 2),
    SampleTargets = make_set(name, 5),
    SampleData = make_set(data, 3)
by bin(timestamp, 15m)
| where FailureCount > 0
| order by timestamp asc, FailureCount desc
```

**Common LSA Dependency Patterns to Check:**

- **Draft API Issues**: Look for "Draft" in name or data fields
- **Azure Storage Issues**: Table Storage, Queue Storage timeouts or throttling
- **Kusto/LogAnalytics**: Query execution failures or timeouts  
- **HTTP Services**: Any HTTP dependency failures
- **Authentication Issues**: AAD token acquisition failures

**Step 4c: Exceptions Analysis (lsa_exceptions table)**
**Process:** Analyze application exceptions that correlate with SLI drops

```kql
// Exceptions analysis for specific region and timeframe
lsa_exceptions
| where timestamp between(datetime({incident_start}) .. datetime({incident_end}))
| extend Location = tostring(customDimensions.Location)
| where Location == "{target_region}"  // From Step 2 regional analysis
| where outerType has_any("Draft", "Http", "Storage", "Timeout", "Internal", "Server") 
   or outerMessage has_any("Draft", "Http", "Storage", "Timeout", "Internal", "Server")
| extend ExceptionCategory = case(
    outerType contains "Draft", "Draft_API",
    outerType contains "HttpInternalServerError", "HTTP_500",
    outerType contains "TimeoutException", "Timeout",
    outerType contains "StorageException", "Storage",
    "Other"
)
| summarize 
    ExceptionCount = count(),
    UniqueTypes = dcount(outerType),
    SampleTypes = make_set(outerType, 5),
    SampleMessages = make_set(outerMessage, 3)
by ExceptionCategory, bin(timestamp, 15m)
| where ExceptionCount > 0
| order by timestamp asc, ExceptionCount desc
```

**Critical LSA Exception Patterns:**

- **DraftHttpInternalServerErrorException**: Draft service failures
- **HTTP 500 Errors**: Internal server errors
- **Timeout Exceptions**: Service timeout issues
- **Storage Exceptions**: Azure Storage connectivity issues

**Step 4d: Root Cause Hypothesis Formation**
**Process:** Apply critical thinking framework from investigation methodology

**Validation Requirements:**

- **Temporal Correlation**: Does the suspected cause align with SLI drop timeline?
- **Regional Distribution**: Is the pattern consistent across affected regions?
- **Evidence Strength**: Multiple independent sources support the hypothesis?
- **Alternative Explanations**: Have other possible causes been ruled out?

**Step 4e: Cross-Regional Pattern Analysis**
Compare dependency and exception patterns across all affected regions to identify:

- Global vs regional issues
- Failure propagation patterns
- Regional infrastructure correlations
- Recovery pattern differences

**Step 4f: Documentation Requirements**
Document in each regional investigation file:

- SLI drop timeline and severity
- Dependency failure patterns and affected services
- Exception analysis with categorized error types
- Root cause hypothesis with supporting evidence
- Recovery timeline and restoration patterns

### Step 5: Consolidated Investigation Report

**Process:** Generate comprehensive report consolidating all findings from Steps 1-4

**Step 5a: Report Structure**
Create final investigation report with:

- Executive summary of global SLI health
- Timeline of all detected drops under 99.9%
- Regional impact breakdown
- Subscription and rule impact analysis
- Root cause analysis for each priority issue
- Remediation recommendations

**Step 5b: Root Cause Integration**
For each investigated drop from Step 4:

- Integrate dependency failure findings
- Include exception analysis results
- Present consolidated root cause hypothesis
- Provide specific remediation steps

**Step 5c: Cross-Issue Pattern Analysis**
Identify patterns across multiple drops:

- Common dependency failures across regions
- Similar exception patterns
- Global vs regional root causes
- Service-wide vs localized issues

### Step 6: Investigation Report Generation and Chat Output

**Process:** Generate comprehensive SLI investigation report with detailed tables and customer impact analysis

**Step 6a: Chat Output Generation Structure**

**Present the complete SLI analysis directly in the Cline chat using the following structure:**

## 🔍 SLI Investigation Report

- **🕐 Investigation Time:** [current_timestamp]
- **📅 Time Range:** [analyzed_time_range]  
- **🔧 Cluster:** azalertsprodweu.westeurope.kusto.windows.net
- **🗄️ Database:** LogAlertsScheduler
- **🏥 Overall SLI Health Status:** [health_status_summary]
- **🚨 Critical Issues:** [critical_issue_count]

## 📋 Executive Summary

- **📊 Total Rule Evaluations:** [total_evaluation_count]
- **✅ Overall Success Rate:** [overall_success_rate]%
- **❌ Failed Evaluations:** [total_failed_evaluations]
- **⏰ Peak Failure Time:** [peak_failure_time]
- **🌍 Most Affected Region:** [worst_performing_region]
- **📈 Recovery Status:** [current_recovery_status]

## 🕐 SLI Drop Timeline Analysis (Hourly Granularity)

| 🕐 Time Period | 📊 Expected | ✅ Completed | 📈 Success Rate | 🚦 Status | ❌ Failed Evaluations | 🌍 Affected Regions |
|----------------|-------------|--------------|-----------------|-----------|---------------------|-------------------|
| [timestamp] | [expected] | [completed] | [rate]% | [status_emoji] | [failures] | [region_count] |

## 🌍 Regional Impact Analysis (Only Drops Under 99.9%)

| 🕐 Time Period | 🌍 Region | 📊 Expected | ✅ Completed | 📈 Success Rate | 🚦 SLI Status | ❌ Failures |
|----------------|-----------|-------------|--------------|-----------------|---------------|-------------|
| [timestamp] | [location] | [expected] | [completed] | [rate]% | [status_emoji] | [failures] |

## 👥 Customer Impact Analysis (Failed Evaluations Only)

| 🕐 Time Period | ❌ Failed Evaluations | 🏢 Impacted Subscriptions | 📋 Impacted Rules | 📊 Sample Failed Rules |
|----------------|----------------------|---------------------------|-------------------|------------------------|
| [timestamp] | [failures] | [subscription_count] | [rule_count] | [sample_rules] |

## 🏢 Overall Impact Summary by Region

| 🌍 Region | 📊 Total Evaluations | ❌ Failed Evaluations | 📈 Success Rate | 🏢 Total Subscriptions | 👥 Impacted Subscriptions | 📊 Subscription Impact % | 📋 Total Rules | ❌ Impacted Rules | 📊 Rule Impact % |
|-----------|---------------------|----------------------|-----------------|------------------------|---------------------------|-------------------------|-------------|-------------------|-------------------|
| [location] | [total] | [failed] | [rate]% | [total_subs] | [impacted_subs] | [sub_impact]% | [total_rules] | [impacted_rules] | [rule_impact]% |

## 🔝 Top Impacted Subscriptions Analysis

| 🏢 Subscription ID | 📊 Total Evaluations | ❌ Failed Evaluations | 📋 Rules in Subscription | 📊 Failure Rate % |
|-------------------|---------------------|----------------------|--------------------------|-------------------|
| [subscription_id] | [total] | [failed] | [rule_count] | [failure_rate]% |

## 🔄 Service Recovery Pattern Analysis

| 🕐 Time Period | 🌍 Region | 📊 Expected | ✅ Completed | 📈 Success Rate | 🔄 Recovery Status |
|----------------|-----------|-------------|--------------|-----------------|-------------------|
| [timestamp] | [location] | [expected] | [completed] | [rate]% | [recovery_emoji] |

**Recovery Status Legend:**

- ✅ **RECOVERED**: ≥99.9% success rate
- 🔄 **RECOVERING**: 99.5-99.8% success rate  
- 🔄 **SLOW_RECOVERY**: 99.0-99.4% success rate
- 🔴 **DEGRADED**: 95.0-98.9% success rate
- 🚨 **CRITICAL**: <95.0% success rate

## 🔧 Root Cause Analysis (Regional Dependency & Exception Investigation)

### 🔍 Primary Findings from DRI-Agent Investigation

| 🔧 Root Cause Category | 🌍 Affected Regions | 📊 Failure Count | 🕐 Timeline | 🔗 Evidence |
|------------------------|-------------------|------------------|-------------|-------------|
| [cause_category] | [regions] | [failure_count] | [timeframe] | [evidence_summary] |

### 🔗 Dependency Analysis Results

| 🛠️ Service Category | 🕐 Failure Timeline | ❌ Failure Count | 🔄 Failure Rate % | 📋 Sample Messages |
|---------------------|---------------------|------------------|-------------------|-------------------|
| [service_type] | [timestamp] | [count] | [rate]% | [messages] |

### ⚠️ Exception Analysis Results  

| 🔥 Exception Category | 🕐 Timeline | 📊 Count | 🔧 Unique Types | 📋 Sample Types | 💬 Sample Messages |
|----------------------|-------------|----------|------------------|------------------|-------------------|
| [exception_type] | [timestamp] | [count] | [unique_count] | [types] | [messages] |

### 🌍 Cross-Regional Comparison

| 🌍 Region | 🕐 Peak Failure Time | 🔗 Dependency Failures | ⚠️ Exception Count | 🔧 Top Failing Services |
|-----------|---------------------|------------------------|-------------------|------------------------|
| [location] | [timestamp] | [dep_failures] | [exception_count] | [services] |

## 👥 Customer Impact Assessment

- **📊 Peak Failure Rate:** [peak_failure_rate]% at [peak_time]
- **❌ Total Failed Evaluations:** [total_failures] evaluations
- **🌍 Regional Impact:** [affected_regions]
- **🏢 Subscription Impact:** [subscription_impact_summary]
- **📋 Alert Rule Impact:** [rule_impact_summary]
- **⏱️ Total Degradation Duration:** [degradation_duration]
- **🔄 Recovery Duration:** [recovery_duration]

## 🚨 Immediate Actions Required

1. **🔴 Priority 1**: [Critical action with timeline]
2. **🟡 Priority 2**: [Important action with responsible team]  
3. **🟢 Priority 3**: [Monitoring action with specific metrics]

## 💡 Recommendations

### 🔧 Short-term Actions

- **🛠️ Immediate**: [Immediate remediation steps]
- **📊 Monitoring**: [Monitoring improvements]
- **🚨 Alerting**: [Alert threshold adjustments]

### 📈 Long-term Improvements  

- **🔧 Reliability**: [Service reliability enhancements]
- **⚡ Performance**: [Performance optimization opportunities]
- **📊 Monitoring**: [Monitoring and alerting improvements]

## 📚 Additional Resources

- **🔧 Kusto Cluster**: [azalertsprodweu.westeurope.kusto.windows.net](https://azalertsprodweu.westeurope.kusto.windows.net)
- **📄 Investigation Documentation**: [investigation_file_path]
- **📊 Related Dashboards**: [monitoring_dashboard_links]
- **☎️ Escalation Contacts**: [team_contact_information]

**📋 Chat Delivery Guidelines:**

- Present the entire analysis in a single, well-formatted chat response
- Use markdown formatting for headers, tables, and lists with extensive emoji usage
- Include specific metrics and percentages in all summaries
- Ensure tables are properly formatted for chat display with emoji headers
- Break up long sections with appropriate emoji headers
- Always conclude with actionable next steps
- Include specific Kusto queries used if user requests them
- Provide clear status indicators (✅🟡🟠🔴💀🚨🔄) throughout all tables

### Step 5: Integration with Root Cause Analysis

**Process:** Consolidate regional findings into comprehensive analysis

**Integration Steps:**

- 🔄 Synthesize findings from all regional investigations
- 🎯 Identify primary root cause and contributing factors  
- 📅 Correlate with deployment timelines and infrastructure changes
- 📊 Document comprehensive timeline and customer impact assessment
- 💡 Provide recommendations for prevention and monitoring improvements

## 🔧 Technical Requirements

### Prerequisites

- Access to appropriate Kusto cluster for the service being investigated
- AAD authentication for service-specific databases
- Understanding of KQL query syntax
- Familiarity with time binning and sampling strategies

### Key Tools and Techniques

- **Primary Data Source**: Service-specific SLI data table (defined in memory-bank process)
- **Query Language**: KQL (Kusto Query Language)
- **Time Binning**: 1h for short periods, 1h for longer investigations
- **Sampling**: Use `sample N` for large datasets to prevent timeouts
- **Filtering**: Focus on service-specific data as defined in memory-bank process

## ⚠️ Important Considerations

### 🚨 Critical Investigation Mistakes to Avoid

**Based on lessons learned from failed investigations:**

| ❌ **Common Mistake** | ✅ **Correct Approach** | 🎯 **Impact** |
|----------------------|-------------------------|--------------|
| **⏰ Time Window Limitations** | Use FULL incident timeframe (08:00-13:00 UTC) | Avoids missing critical failure patterns |
| **🔍 Insufficient Search Patterns** | Use broad patterns with `has_any()` and `contains` | Captures all service failures ("Draft", "Http", "Storage") |  
| **🔚 Premature Conclusions** | Try multiple search approaches across different tables | Prevents "no failures found" false conclusions |
| **🌍 Single Region Focus** | Compare patterns across ALL affected regions | Identifies global vs regional issues |
| **🔧 Missing Service-Specific Issues** | Search for specific patterns like "DraftHttpInternalServerErrorException" | Captures service-specific failure modes |

### 📊 Investigation Quality Checklist

| ✅ **Validation Step** | 📋 **Requirement** | 🎯 **Purpose** |
|------------------------|-------------------|----------------|
| **⏰ Full Timeframe** | Used full incident timeframe, not just peak periods | Complete failure pattern analysis |
| **🔍 Service Patterns** | Searched for Draft, Http, Storage, etc. patterns | Service-specific failure detection |  
| **🌍 Regional Coverage** | Examined all affected regions, not just worst-affected | Global vs regional issue identification |
| **🔧 Search Breadth** | Used broad search patterns with `has_any()` and `contains` | Comprehensive failure discovery |
| **🔗 Cross-Reference** | Cross-referenced findings between dependencies and exceptions | Root cause validation |
| **📊 Multi-Source** | Validated findings against multiple telemetry sources | Evidence strength confirmation |

### Query Performance

- **Large datasets**: Use sampling and appropriate time binning
- **Timeout prevention**: Break large date ranges into smaller chunks
- **Progressive investigation**: Start broad, then drill down to specific periods

### Data Interpretation

- **Success rate threshold**: 99.9% is the standard SLI threshold
- **Service baseline**: Normal operation is typically 99.95%+ success rate
- **Processing volume**: 20-22 million evaluations per hour during peak times
- **Time zones**: All timestamps are in UTC

### Investigation Scope

- **Focus on degradation**: Only investigate periods below 99.9% success rate
- **Document everything**: Record all findings for future reference
- **Regional analysis**: Consider geographical distribution of issues
- **Recovery patterns**: Note how the service returned to normal operation

### 📋 Investigation Validation Checklist

| ✅ **Validation Check** | 📊 **Verification Method** | 🎯 **Success Criteria** |
|------------------------|---------------------------|------------------------|
| **⏰ Full Timeframe Usage** | Review query time ranges | All queries use complete incident window |
| **🔍 Service Pattern Search** | Check for Draft, Http, Storage patterns | Broad service-specific search executed |
| **🌍 Regional Coverage** | Verify all affected regions analyzed | Complete regional comparison performed |
| **🔧 Search Pattern Breadth** | Confirm `has_any()` and `contains` usage | Comprehensive failure pattern discovery |
| **🔗 Cross-Reference Analysis** | Dependencies vs exceptions correlation | Root cause hypothesis supported by multiple sources |
| **📊 Multi-Source Validation** | Multiple telemetry table analysis | Consistent findings across different data sources |

## 📈 Success Criteria

| 🎯 **Success Metric** | 📊 **Deliverable** | ✅ **Quality Standard** |
|----------------------|-------------------|------------------------|
| **🕐 Clear Timeline** | Degradation occurrence and resolution timeline | Hourly granularity with precise start/end times |
| **📊 Quantified Impact** | Failure counts, success rates, customer impact | Specific numbers with subscription and rule impact |
| **⏱️ Duration Analysis** | Issue persistence timeframe | Complete degradation and recovery duration |
| **🌍 Regional Scope** | Geographical impact understanding | All affected regions analyzed with comparison |
| **🔄 Recovery Assessment** | Service restoration pattern analysis | Recovery progression with status transitions |
| **🔧 Root Cause Evidence** | Dependency and exception analysis | Multi-source evidence supporting hypothesis |
| **👥 Customer Impact** | Subscription and alert rule impact quantification | Specific customer impact metrics and percentages |

## 🔗 Related Processes

This workflow integrates with:

- **Live Site Investigation** (`.clinerules/04-livesite-investigation-process.md`) for detailed root cause analysis
- **General Investigation Methodology** (`.clinerules/subagents/dri-agent/investigation-methodology.md`) for systematic troubleshooting
- **Telemetry Analysis** (`memory-bank/telemetry.md`) for detailed error investigation

---

*This workflow should be the first step in any service degradation investigation, providing the foundation for deeper analysis and root cause identification.*
