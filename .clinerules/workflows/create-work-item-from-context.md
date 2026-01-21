<!-- filepath: .clinerules/workflows/create-work-item-from-context.md -->
# Create Work Item from Discussion Context Workflow

## Overview

This workflow enables creating detailed Azure DevOps work items based on ongoing discussions about live site issues, customer problems, or technical investigations. It captures comprehensive context, code references, and discussion details to provide future AI agents with complete information for handling the work item.

## When to Use This Workflow

- During or after lengthy discussions about technical issues that require follow-up work
- When live site investigations reveal needed improvements or fixes
- When customer issues lead to actionable development work
- When technical discussions identify bugs, features, or improvements that need tracking

## Prerequisites

Before using this workflow, ensure:
- You have read `.clinerules/03-using-ado.md` for ADO integration context
- Repository configuration exists in `memory-bank/ado-configuration.md`
- Discussion contains sufficient technical detail for work item creation

## Workflow Parameters

### Required Parameters
None - all parameters have sensible defaults

### Optional Parameters
- **Parent Work Item ID**: ID of parent work item (default: no parent)
- **Work Item Type**: Type of work item to create (default: "Product Backlog Item")
  - Valid options: "Product Backlog Item", "Task", "Bug", "Feature", "Epic"

## Workflow Steps

### Step 1: Context Analysis and Preparation

1. **Analyze Current Discussion Context**
   - Review the entire conversation history
   - Identify key technical details, issues, and proposed solutions
   - Extract relevant code snippets, file paths, and repository references
   - Note any related work items, IcM tickets, or external references

2. **Determine Work Item Scope**
   - Assess whether this should be a Bug, Feature, PBI, or Task
   - Consider the complexity and effort required
   - Evaluate if this needs to be broken down into multiple work items

3. **Gather Repository Information**
   - Read `memory-bank/ado-configuration.md` for project configuration
   - Identify the main repository branch and URL
   - Collect relevant file paths and code references from discussion

### Step 2: Work Item Content Generation

1. **Create Comprehensive Title**
   - Generate a clear, descriptive title that summarizes the work
   - Include key technical terms and scope indicators
   - Keep title concise but informative (50-80 characters)

2. **Build Detailed Description**
   
   The description must include the following sections in HTML format:

   **Context and Origin Section** (if applicable):
   ```html
   <div><strong>ORIGIN CONTEXT</strong></div>
   <div>- IcM Ticket: <a href="[ticket_url]">[ticket_id]</a></div>
   <div>- Parent Work Item: <a href="[work_item_url]">[work_item_id]</a></div>
   <div>- Investigation: [brief_description]</div>
   <div><br></div>
   ```

   **Problem Statement**:
   ```html
   <div><strong>PROBLEM STATEMENT</strong></div>
   <div>[Detailed description of the issue or requirement]</div>
   <div><br></div>
   ```

   **Technical Details**:
   ```html
   <div><strong>TECHNICAL DETAILS</strong></div>
   <div>Repository: <a href="[repo_url]">[repo_name]</a></div>
   <div>Branch: [main_branch_name]</div>
   <div><br></div>
   <div><strong>Affected Components:</strong></div>
   <div>- [Component1]: [description]</div>
   <div>- [Component2]: [description]</div>
   <div><br></div>
   ```

   **Code References**:
   ```html
   <div><strong>RELEVANT CODE REFERENCES</strong></div>
   <div>- File: <a href="[file_url]">[file_path]</a></div>
   <div>  Method/Class: [specific_code_reference]</div>
   <div>  Description: [why_this_is_relevant]</div>
   <div><br></div>
   ```

   **Solution Approach** (if discussed):
   ```html
   <div><strong>PROPOSED SOLUTION</strong></div>
   <div>1. [Step_one_description]</div>
   <div>2. [Step_two_description]</div>
   <div>3. [Step_three_description]</div>
   <div><br></div>
   ```

   **Discussion Summary**:
   ```html
   <div><strong>DISCUSSION SUMMARY</strong></div>
   <div>Key findings from the discussion:</div>
   <div>- [Finding_1]</div>
   <div>- [Finding_2]</div>
   <div>- [Finding_3]</div>
   <div><br></div>
   <div>Decisions made:</div>
   <div>- [Decision_1]</div>
   <div>- [Decision_2]</div>
   <div><br></div>
   ```

   **AI Agent Context**:
   ```html
   <div><strong>AI AGENT INSTRUCTIONS</strong></div>
   <div>This work item was created from a detailed discussion. The AI agent assigned to this work should:</div>
   <div>1. Review all linked references and code files</div>
   <div>2. Understand the context from the discussion summary</div>
   <div>3. Follow the proposed solution approach if provided</div>
   <div>4. Consider the technical constraints and dependencies mentioned</div>
   <div><br></div>
   ```

### Step 3: Work Item Creation

1. **Prepare Work Item Data**
   - Use the generated title from Step 2
   - Use the comprehensive HTML description from Step 2
   - Apply workflow parameters (work item type, parent ID)
   - Get configuration values from `memory-bank/ado-configuration.md`

2. **Create the Work Item**
   - Follow the work item creation process detailed in `.clinerules/03-using-ado.md`
   - Use the `ado_create_work_item` tool with the prepared data
   - Include parent relationship if parent work item ID was provided as parameter
   - Capture the created work item ID for reference and reporting

### Step 4: Repository Linking and Final Setup

1. **Document Repository Links**
   - Ensure main repository branch is linked in description
   - Verify all file references include proper URLs
   - Add any additional repository references discovered in discussion

2. **Validate Work Item Creation**
   - Confirm work item was created successfully
   - Verify all sections are properly formatted
   - Check that links are accessible and correct

3. **Provide Summary**
   - Report work item ID and URL to user
   - Summarize what was captured in the work item
   - Mention any follow-up actions needed

## Best Practices

### Content Quality
- **Be Comprehensive**: Include all relevant technical details from discussion
- **Be Specific**: Reference exact file paths, method names, and code snippets
- **Be Contextual**: Explain why each reference is relevant to the work item
- **Be Actionable**: Provide clear next steps for the assigned AI agent

### HTML Formatting
- Use proper HTML structure with `<div>`, `<strong>`, and `<a>` tags
- Include `<div><br></div>` for spacing between sections
- Make all repository and file references clickable links
- Structure content in logical sections for easy navigation

### Repository References
- Always link to the main branch of the repository
- Include full file paths relative to repository root
- Add line numbers to code references when applicable
- Provide context for why each file/method is relevant

### AI Agent Handoff
- Include clear instructions for future AI agents
- Summarize key decisions and constraints from discussion
- Reference all external dependencies and related work items
- Provide sufficient context to continue work without additional explanation

## Error Handling

### Missing Configuration
If `memory-bank/ado-configuration.md` is missing:
1. Use ask_followup_question to request configuration details
2. Document the missing configuration requirement
3. Proceed with work item creation using manual input

### Insufficient Context
If discussion lacks technical details:
1. Use ask_followup_question to gather missing information
2. Focus on capturing what is available
3. Mark areas that need additional investigation in work item

### Repository Access Issues
If repository links cannot be constructed:
1. Include repository name and general references
2. Ask user for repository URL if needed
3. Ensure work item includes enough detail to locate relevant code

## Integration with Other Workflows

### Investigation Workflows
- Reference investigation files in work item description
- Link to related investigation tickets or IcM items
- Include findings and evidence from investigation process

### Feature Development Workflow
- Consider if work item should trigger feature development process
- Reference existing feature documentation if applicable
- Ensure work item aligns with memory bank structure

### Memory Bank Updates
- Update relevant feature activeContext.md if work item relates to existing feature
- Document work item creation in investigation files if applicable
- Consider if core memory bank files need updates based on discussion findings

## Example Usage

### Creating a Bug Work Item from Live Site Discussion

```javascript
// Parameters: parentWorkItemId=12345, workItemType="Bug"

{
  "project": "One",
  "workItemType": "Bug",
  "fields": {
    "System.Title": "Fix credential exposure in query logging during timeout scenarios",
    "System.Description": "<div><strong>ORIGIN CONTEXT</strong></div><div>- IcM Ticket: <a href=\"https://portal.microsofticm.com/imp/v3/incidents/details/123456789\">IcM-123456789</a></div><div>- Investigation: Live site credential exposure during Kusto query timeouts</div><div><br></div><div><strong>PROBLEM STATEMENT</strong></div><div>During Kusto query timeout scenarios, credential information is being logged in plain text, creating a security vulnerability...</div>...",
    "System.AreaPath": "One\\Azure Edge and Platform\\Health and Standards\\AIOps\\Azure Alerts\\Azure Alerts Data Plane",
    "System.State": "New"
  },
  "parentWorkItemId": 12345
}
```

This workflow ensures that all valuable discussion context is preserved and made actionable through properly structured ADO work items, enabling effective handoff to AI agents for implementation.
