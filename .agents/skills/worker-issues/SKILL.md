# SKILL.md
---
name: worker-issues
description: List GitHub issues currently available for the coding-only implementer to pick up.
---

## Instructions
Run:
```
gh issue list --search "label:ready label:agent-external label:scope-logic -label:blocked" --json number,title,url,labels
```
Format as a simple numbered list (issue number, title, URL). Nothing else — no filtering options, no pagination.
