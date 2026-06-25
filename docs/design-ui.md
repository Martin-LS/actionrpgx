# Game Design Document — UI & HUD

> Part of the design docs. Interaction flows and component patterns: `design-ui-mechanics.md`. Item tiers, gear slot roles, stat tables: `design-progression.md`. Combat and run structure: `design-mechanics.md`.
> Living document — details will evolve as the game is playtested.

## HUD (In-Run)

- Health bar (HUD, bottom-left)
- XP bar + current level
- Coin counter (this run)
- Elapsed time / countdown
- **Skill bar** — bottom-center of the HUD. Contains 5 skill slots (Q E R F + mouse) and a dedicated Dodge cooldown indicator on the far right:
  - Skill slots: Show equipped skills with independent cooldown states (greyed out and fills from bottom during cooldown). Empty slots are visually empty.
  - Dodge indicator: Positioned to the right of the skill slots, separated by a visual gap. Re-uses the skill slot design (filling from bottom as the 1-second cooldown recovers) and is labeled "Space" to denote its activation keybind.
- **Floating HP bar** — above both player and enemies (see Hit Feedback in `design-mechanics.md` for colours and visibility rules)
- **Damage numbers** — float upward from the hit point on every hit, colour-coded by damage type and crit (see Hit Feedback in `design-mechanics.md`)
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

See `design-ui-mechanics.md` for all click/interaction flows within the Character Screen.

### Run Results Overlay

Shown at run end. Return button goes back to Character Screen.

## Global Options Overlay

The Global Options Overlay is a central menu accessible during any part of the game session except the initial Welcome screen (`main_menu.tscn`).

### Access & Modality
* **Trigger:** Pressing the `Esc` key or clicking a global `"Options"` button (visible in the top-right corner of all screens except the Welcome screen) opens the overlay.
* **Exclusion:** The overlay cannot be opened on the Welcome screen.
* **Modality:** Opening the overlay instantly pauses the entire game state (`GetTree().Paused = true`) and blocks all inputs to underlying screens.
* **Dismissal:** Pressing `Esc` again, or clicking the `"Close"` button, unpauses the game state and closes the overlay.

### Visual Layout
* A centered modal container using the standard **Iron & Slate** UI theme (slate background with gold borders).
* Title at the top: `"Options"`.
* A vertical list of buttons, separated by category (player-facing buttons at the top, developer/debug options at the bottom):
  * **Context: In-Run (`main.tscn`):**
    1. `"Resume"` — Unpauses and closes the overlay.
    2. `"End Run"` — Exits back to the Character Screen; all progress from this run is discarded (with alongside warning text: *"All progress from this run will be lost."*).
    3. `"Debug Options"` (Debug Category, positioned at the bottom) — Opens the debug options panel (handled by a separate task).
  * **Context: Out-of-Run (Roster, Inventory, Loadout, etc.):**
    1. `"Close"` — Closes the overlay.
    2. `"Debug Options"` (Debug Category, positioned at the bottom) — Opens the debug options panel.

### Debug Options Panel

Debug-build only (`OS.IsDebugBuild()`). Accessible from the options overlay on any screen. Replaces the overlay's button list in-place (same modal, "← Back" returns to the main options list).

**In-run controls** (hidden when not in a run — require player and weapon nodes):
- **Range Indicator** checkbox — toggles the attack range torus visible at the player's feet.
- **God Mode** checkbox — player takes no damage while active.
- **Skill Auto-cast** — header label above a horizontal row of 5 checkboxes labelled `1`–`5`. Each checkbox enables or disables auto-firing for that skill slot. All default to on.

**Always available:**
- **Add Materials** button — adds 500 `crafting_common` to the current character's inventory. Useful for rapid crafting iteration during development.
