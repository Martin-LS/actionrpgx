# SKILL.md
---
name: worker-issues
description: List GitHub issues currently ready for pickup.
---

## Instructions
Run:
```
gh issue list --search "label:ready -label:blocked" --json number,title,url,labels
```
Format as a simple numbered list (issue number, title, URL) — or state plainly that no issues are ready if the list is empty. Also show the issue's `complexity-*` label (low/medium/high) next to each entry so the user can route it to an appropriately capable agent at a glance; if an issue has no `complexity-*` label (or more than one), display it as `complexity-high` — everything resolves upward, per AGENTS.md Rule 6.

**Stop there.** Do not run `gh issue view`, do not read any code, do not claim/branch/comment on an issue. Wait for the user to pick one (or say nothing further) before taking any other action.
