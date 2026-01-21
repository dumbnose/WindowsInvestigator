<!-- filepath: .clinerules/03-using-ado.md -->
# Azure DevOps (ADO) Integration Instructions

This file provides guidance on when and how to use the ADO MCP server for project management and work item tracking.

## When to Use ADO MCP Server

### Planning Phase
- When starting a new sprint or planning period
- When needing to understand current work items and priorities
- When investigating work item dependencies
- When reviewing team capacity and assignments

### Work Item Management
- When starting work on a new work item
- When updating work item status or progress
- When linking work items to pull requests
- When creating child work items or breaking down tasks
- When documenting work completion

### Status Reporting
- When providing updates on current work
- When checking team progress
- When identifying blockers or dependencies

## Default Configuration

ADO configuration values are project-specific and are read from `memory-bank/ado-configuration.md` in your repository. Refer to the Memory Bank structure documentation for configuration details and setup instructions.

## Querying Work Items from Query ID

To get detailed work item information from a saved query:

1. **Get Query Results**: Use `ado_get_query_results_by_id` with the query ID to get work item IDs

2. **Batch Retrieve Details**: Use `ado_get_work_items_batch_by_ids` to get full work item details

   - Process in batches of up to 20 items at a time
   - Repeat as needed until all items are retrieved

Example workflow:

```javascript
# Step 1: Get work item IDs from query
ado_get_query_results_by_id(query_id="6ca20715-c4cb-43f5-8387-b44f45de248e")

# Step 2: Batch retrieve details (20 items max per batch)
ado_get_work_items_batch_by_ids(ids=[30987884, 27415546, ...])
# Repeat for additional batches if needed
```

## Creating Work Items

To create new work items use `ado_create_work_item` with the following structure:

### Basic Work Item Creation Structure

```javascript
{
  "project": "One",
  "workItemType": "Product Backlog Item",  // or "Task", "Bug", "Feature", etc.
  "fields": {
    "System.Title": "Work item title",
    "System.Description": "HTML formatted description",
    "System.AreaPath": "One\\Azure Edge and Platform\\Health and Standards\\AIOps\\Azure Alerts\\Azure Alerts Data Plane",
    "System.State": "New"
  },
  "parentWorkItemId": 12345  // Optional: for creating child work items
}
```

### Required vs Optional Parameters

**Required Parameters:**
- `project`: Always use "One"
- `workItemType`: Must match exact ADO work item type names
- `fields`: Object containing work item field values

**Optional Parameters:**
- `parentWorkItemId`: Use when creating child work items (Tasks under PBIs, PBIs under Features, etc.)

### Work Item Type Names

Use these exact strings for `workItemType`:
- `"Product Backlog Item"` - For PBIs
- `"Task"` - For tasks under PBIs
- `"Bug"` - For bug reports
- `"Feature"` - For feature work items
- `"Epic"` - For epic-level items

### Common Field Names for Work Items

**Required Fields:**
- `"System.Title"`: Work item title
- `"System.AreaPath"`: Use default area path
- `"System.State"`: Usually "New" for new items

**Optional Fields:**
- `"System.Description"`: HTML formatted description (see formatting rules below)
- `"System.AssignedTo"`: Email address for assignment
- `"System.IterationPath"`: Sprint/iteration assignment

### Example: Creating a Product Backlog Item

```javascript
{
  "project": "One",
  "workItemType": "Product Backlog Item",
  "fields": {
    "System.Title": "Implement new validation logic",
    "System.Description": "<div><strong>Problem Statement</strong></div><div>Current validation is insufficient...</div>",
    "System.AreaPath": "One\\Azure Edge and Platform\\Health and Standards\\AIOps\\Azure Alerts\\Azure Alerts Data Plane",
    "System.State": "New"
  },
  "parentWorkItemId": 12879699
}
```

### Example: Creating a Task under a PBI

```javascript
{
  "project": "One",
  "workItemType": "Task",
  "fields": {
    "System.Title": "Add unit tests for validation logic",
    "System.AreaPath": "One\\Azure Edge and Platform\\Health and Standards\\AIOps\\Azure Alerts\\Azure Alerts Data Plane",
    "System.State": "New"
  },
  "parentWorkItemId": 33667200  // Parent PBI ID
}
```

### Adding Repository Links to Work Items

When creating work items that relate to specific code repositories or branches, you can add repository links as an optional parameter. Repository links appear as "Branch" links in the work item's Development section.

**Optional Parameter for Repository Links:**
- `relations`: Array of relation objects to link repositories or branches

**Repository Link Structure:**
```javascript
"relations": [
  {
    "rel": "ArtifactLink",
    "url": "vstfs:///Git/Ref/<project-id>%2F<repository-id>%2FGB<branch-name>",
    "attributes": {
      "name": "Branch"
    }
  }
]
```

**Example: Creating a Work Item with Repository Link**
```javascript
{
  "project": "One",
  "workItemType": "Product Backlog Item",
  "fields": {
    "System.Title": "Fix validation logic bug",
    "System.Description": "<div><strong>Bug Fix</strong></div><div>Description here...</div>",
    "System.AreaPath": "One\\Azure Edge and Platform\\Health and Standards\\AIOps\\Azure Alerts\\Azure Alerts Control Plane",
    "System.State": "New"
  },
  "relations": [
    {
      "rel": "ArtifactLink",
      "url": "vstfs:///Git/Ref/b32aa71e-8ed2-41b2-9d77-5bc261222004%2Fad30dc2f-fca1-438c-a1ea-ca6b845a9dec%2FGBmaster",
      "attributes": {
        "name": "Branch"
      }
    }
  ]
}
```

**URL Format Breakdown:**
- `vstfs:///Git/Ref/` - Fixed prefix for Git references
- `<project-id>` - Azure DevOps project GUID (from work item context)
- `%2F` - URL-encoded forward slash separator
- `<repository-id>` - Repository GUID (from ADO configuration)
- `%2F` - URL-encoded forward slash separator  
- `GB<branch-name>` - Git branch reference (GB prefix + branch name)

**Common Repository Links:**
- Master branch: `GBmaster`
- Feature branch: `GBfeature/my-feature-name`
- Development branch: `GBdevelop`

### Common Mistakes to Avoid

❌ **Don't use**: Incorrect parameter names like `work_item_type` instead of `workItemType`
❌ **Don't use**: Individual parameters like `title`, `description` - use `fields` object instead
❌ **Don't use**: Plain text descriptions - always use HTML formatting

✅ **Do use**: Exact parameter names: `workItemType`, `fields`
✅ **Do use**: Proper field names with `System.` prefix
✅ **Do use**: HTML formatted descriptions following the formatting rules
✅ **Do use**: Repository links when work items relate to specific code changes

## Updating Work Item Fields

To update work item fields like state or iteration path:

### Basic Update Structure

Use `ado_update_work_item` with the following format:

```javascript
{
  "project": "One",
  "id": <work_item_id>,
  "updates": [
    {
      "path": "/fields/<FieldName>",
      "value": "<new_value>"
    }
  ]
}
```

### Common Field Updates

**Update State:**

```javascript
{
  "path": "/fields/System.State",
  "value": "Active"  // or "New", "Done", etc.
}
```

**Update Iteration Path:**

```javascript
{
  "path": "/fields/System.IterationPath", 
  "value": "One\\Bromine\\CY25Q2\\Monthly\\06 Jun (Jun 01 - Jun 28)"
}
```

**Update Assignment:**

```javascript
{
  "path": "/fields/System.AssignedTo",
  "value": "asafst@microsoft.com"
}
```

### Multiple Field Updates

You can update multiple fields in a single request by adding multiple objects to the `updates` array:

```javascript
"updates": [
  {
    "path": "/fields/System.State",
    "value": "Active"
  },
  {
    "path": "/fields/System.IterationPath",
    "value": "One\\Bromine\\CY25Q2\\Monthly\\06 Jun (Jun 01 - Jun 28)"
  }
]
```


## Work Item Description Formatting

Azure DevOps work item descriptions expect **HTML format**, not plain text. Using `\n` for line breaks will not work properly.

### HTML Formatting Rules

**CRITICAL**: When updating work item descriptions (System.Description field), use proper HTML formatting:

- **Section headers**: `<div><strong>SECTION NAME</strong></div>`
- **Empty lines/spacing**: `<div><br></div>`
- **Regular paragraphs**: `<div>Content text here</div>`
- **Clickable links**: `<div>- Description: <a href="URL">Link Text</a></div>`
- **Lists**: Each list item in separate `<div>` elements

### Example Proper Formatting

```html
<div><strong>Enhanced Credential Redaction for Query Strings</strong></div>
<div><br></div>
<div><strong>HIGH RISK SECURITY ISSUE</strong></div>
<div>This work item implements enhanced credential redaction functionality...</div>
<div><br></div>
<div><strong>SOLUTION APPROACH</strong></div>
<div>1. Create new extension method RedactPotentialCredsFromQuery()</div>
<div>2. Maintain existing functionality by calling current method</div>
<div><br></div>
<div><strong>REFERENCES</strong></div>
<div>- Repository: <a href="https://msazure.visualstudio.com/One/_git/Repo">Link Text</a></div>
```

### Common Formatting Mistakes to Avoid

❌ **Don't use**: Plain text with `\n` characters ❌ **Don't use**: Markdown formatting (`## Headers`, `**bold**`) ❌ **Don't use**: Single long strings without proper HTML structure

✅ **Do use**: Proper HTML with `<div>`, `<strong>`, `<br>`, and `<a>` tags ✅ **Do use**: Separate `<div>` elements for each line/paragraph ✅ **Do use**: `<div><br></div>` for empty lines and spacing



## Work Item Structure

The project follows a hierarchical work item structure:

### **Epics** (Top Level)
- Long-term strategic initiatives updated every few months
- No need to update them during regular development work
- Provide high-level direction and context

### **Features**
- Multiple features can exist under an Epic
- Each feature is scoped to specific work that needs to be done
- May span longer than a single sprint
- Represent substantial functionality or capabilities

### **Product Backlog Items (PBIs)**
- Multiple PBIs can exist under a Feature
- Each PBI is scoped to specific work within a sprint
- Used to break down Features into smaller, manageable items
- Should not take more than one sprint to complete
- Primary work units for sprint planning

### **Tasks** (Optional)
- Can be created under PBIs for further breakdown
- Not mandatory but useful for detailed tracking
- Represent specific implementation steps


## Creating Pull Requests

### When to Create Pull Requests
- When completing work on a feature or bug fix
- When code changes are ready for review
- When work item tasks are completed and need to be merged

### Repository Configuration

Repository configuration values are read from `memory-bank/ado-configuration.md` in your repository. This includes project name, repository details, and target branch settings for pull request creation.

### Pull Request Creation Workflow

1. **Prepare Branch**: 
   - Check for uncommitted changes using `git status`
   - If there are uncommitted changes, ask the user if they need to be committed before proceeding
   - Ensure the branch is fully synchronized with remote by suggesting to commit and push all changes
   - Verify the feature branch is up to date and ready for merge

2. **Preview Pull Request Details**: Before creating the pull request:
   - Generate and present the proposed title and description to the user
   - Ask for user confirmation and feedback on the content
   - Allow for multiple iterations of comments and revisions
   - **Do not create the pull request until the title and description are fully approved by the user**

3. **Create Pull Request**: Use `ado_create_pull_request` with the following structure:

```javascript
{
  "repositoryId": "a22b2a2e-4ca0-4b2c-9cd7-fc784ee549de",
  "sourceRefName": "refs/heads/your-feature-branch",
  "targetRefName": "refs/heads/master", 
  "title": "Brief description of changes",
  "description": "[PR details]",
  "isDraft": false
}
```

### Example Pull Request Creation

```javascript
{
  "repositoryId": "a22b2a2e-4ca0-4b2c-9cd7-fc784ee549de",
  "sourceRefName": "refs/heads/feature/add-new-validation",
  "targetRefName": "refs/heads/master",
  "title": "Add input validation for alert rule parameters",
  "description": "### Pull Request Checklist\n\n**Please fill out the following information before submitting the PR:**\n\n1. **What flow does this change affect?**\n   Alert rule creation and validation flow\n\n2. **What is the risk assessment of the change?**\n   - [ ] Low\n   - [x] Medium\n   - [ ] High\n\n3. **Risk assessment justification:**\n   Adding new validation logic to existing flow with backward compatibility maintained\n\n4. **Is the change covered by Unit Tests?**\n   - [x] Yes\n   - [ ] No\n\n5. **How was the change validated?**\n   Unit tests added and manual testing performed on dev environment\n\n6. **Is ARM resources deployment required for this change?**\n   - [ ] Yes\n   - [x] No\n\n---\n\n### Description of Changes\n\nAdded comprehensive input validation for alert rule parameters to prevent invalid configurations and improve error messaging.",
  "isDraft": false
}
```

### Risk Assessment Guidelines

When asked to create a pull request, **always ask for the risk level** if not provided:

- **Ask**: "What is the risk level for this change? (Low/Medium/High)"
- **Clarify**: If unsure about any template details, ask for clarification
- **Validate**: Ensure all template sections are properly filled before creating the PR

### Linking Work Items to Pull Requests

After creating a pull request, link it to relevant work items **only if this PR is part of a feature that is connected to a work item**. Use the `ado_link_work_item_to_pull_request` tool:

```javascript
{
  "project": "One",
  "repositoryId": "a22b2a2e-4ca0-4b2c-9cd7-fc784ee549de", 
  "pullRequestId": <pr_id>,
  "workItemId": <work_item_id>
}
```

### Best Practices for Pull Requests

- Use descriptive titles that summarize the change
- Fill out all template sections completely and accurately
- Always assess risk level carefully
- Link to related work items for traceability
- Create draft PRs for work-in-progress that needs early feedback
- Ensure branch naming follows conventions (e.g., `feature/`, `bugfix/`, `hotfix/`)


## Best Practices

- Use descriptive comments when updating work items
- Link related work items and pull requests for traceability
- Break down large work items into smaller, manageable tasks
- **Before updating iteration paths**: Always use the ADO MCP server tools to find the correct iteration path format and validate available iterations for the project before attempting updates
- **Always use proper HTML formatting** when updating work item descriptions


## Integration with Memory Bank

When working with ADO:
- Document work item context in feature `activeContext.md`
- Reference ADO work item IDs in documentation
- Use ADO queries to inform sprint planning and feature prioritization
- Document pull request links and outcomes in feature documentation
