# Game Design Document — UI & HUD

> Part of the GDD. Interaction flows and component patterns: `gdd-ui-mechanics.md`. Item tiers, gear slot roles, stat tables: `gdd-progression.md`. Combat and run structure: `gdd-mechanics.md`.
> Living document — details will evolve as the game is playtested.

## HUD (In-Run)

- Health bar (HUD, bottom-left)
- XP bar + current level
- Coin counter (this run)
- Elapsed time / countdown
- **Skill bar** — bottom-center of the HUD. 5 slots (Q E R F + mouse). Each slot shows its equipped skill with independent cooldown state:
  - Active skill on cooldown: slot is greyed out, fills from bottom as cooldown recovers
  - Active skill ready: slot fully lit
  - Empty slot: visually empty (no icon)
- **Floating HP bar** — above both player and enemies (see Hit Feedback in `gdd-mechanics.md` for colours and visibility rules)
- **Damage numbers** — float upward from the hit point on every hit, colour-coded by damage type and crit (see Hit Feedback in `gdd-mechanics.md`)
- [TBD] Minimap

---

## Screens

### Main Menu

Title screen with Play button.

### Account Screen

The account-level hub. Always the first screen after Main Menu. Contains the character roster (list characters, create new, delete). Designed to grow — future account-level info (account stats, global progress, etc.) will live alongside the roster. Selecting a character navigates to their Character Screen.

**Character creation — name rules:** Required (non-empty). Alphanumeric only — no spaces or special characters. Must be unique across all characters on the account. Confirm button is disabled until all rules pass; inline error message explains which rule is violated.

### Character Screen

Full management hub for the selected character. Two tabs: **Loadout** (default) and **Sigils**. Start Run button at the bottom. Back button returns to Account Screen.

**Loadout tab** — two-column layout:
- *Left/centre* — character name, archetype, stats, and equipped slots: Weapon / Hat / Body / Ring / Skills 1–5
- *Right column* — account inventory, always visible within this tab. Scrollable 5-column icon grid with four sub-tabs:
  - *Equipment* — crafted gear, 50-item cap
  - *Skills* — crafted skill items, 50-item cap
  - *Skill Augments* — crafted Skill Augments, 50-item cap
  - *Equipment Augments* — crafted Equipment Augments, 50-item cap
- Equipped items are not shown in the inventory — they live in the character slots.

**Sigils tab** — visible, empty (reserved for future sigil system)

Both tabs are always visible; empty tabs are not locked or greyed out.

See `gdd-ui-mechanics.md` for all click/interaction flows within the Character Screen.

### Run Results Overlay

Shown at run end. Return button goes back to Character Screen.

### Pause Menu

Triggered by ESC during a run. Second ESC or Resume button closes it. Run is paused while open.

- **Resume** — closes menu, run continues
- **End Run** — exits immediately to character screen; all progress from this run is discarded (level, XP, coins, crafting materials). Warning text alongside: *"All progress from this run will be lost."*
- No confirmation step — warning text is the friction
