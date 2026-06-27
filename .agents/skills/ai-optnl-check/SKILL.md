# SKILL.md
---
name: ai-optnl-check
description: Compare the AI Operational Guidelines sections of .agents/AGENTS.md and CLAUDE.md and display a HUD warning if they differ.
---

## Instructions
1. Run the PowerShell consistency script:
   ```powershell
   pwsh -File C:/work/my/github/actionrpgx/.claude/check_consistency.ps1
   ```
2. Report the console output and diff details directly to the user.

The skill can be invoked via the command `/ai-optnl-check`.
