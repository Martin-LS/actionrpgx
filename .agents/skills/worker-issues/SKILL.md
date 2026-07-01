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
Format as a simple numbered list (issue number, title, URL) — or state plainly that no issues are ready if the list is empty.

**Stop there.** Do not run `gh issue view`, do not read any code, do not claim/branch/comment on an issue. Wait for the user to pick one (or say nothing further) before taking any other action.
