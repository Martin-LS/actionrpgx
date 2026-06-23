# Game Design Document — UI Interaction Mechanics

> Part of the GDD. Screen layouts and HUD: `gdd-ui.md`. Item tiers, gear slot roles, stat tables: `gdd-progression.md`.
> Living document — details will evolve as the game is playtested.

## Character Screen — Interaction Model

**Reusable components — the core pattern.** Skills and Augments share one design pattern: a component that shows a Craft button and a list of owned items, reused in both the inventory tab and inside slot interactions. The component's event handlers differ by context (inventory vs. slot), but the visual is identical — styling changes propagate everywhere automatically.

---

### Skills Inventory Tab

- Lists all unslotted skill items
- **Craft button** at top → switches view to craftable skills list (one button per craftable skill type)
  - Click a skill type → crafts it (1 material) → adds to skills inventory → returns to owned list
- **Click an owned skill** → opens Skill Modify Panel for that skill (no Remove — it's unslotted)

### Skills Slot (character loadout)

- **Empty skill slot** → opens the Skills component (same visuals as the inventory tab; different handlers)
  - Craft button → crafts skill + auto-slots it → opens Skill Modify Panel
  - Click an owned skill → slots it → opens Skill Modify Panel
- **Filled skill slot** → opens Skill Modify Panel directly

### Skill Modify Panel

```
[ Skill Name ]  [ Upgrade ]  [ Re-roll ]  [ Remove ]   ← Remove only when skill is slotted
─────────────────────────────────────────────────────
[ A1 ]   │  < context-sensitive right panel >
[ A2 ]   │
[ A3 ]   │
```

**Augment slot selected (left column):**
- **Empty slot** → right panel shows the Augments component (same as Augments inventory tab; different handlers):
  - Craft button → crafts augment + auto-slots it → right panel transitions to filled-slot view
  - Click an owned augment → slots it → right panel transitions to filled-slot view
- **Filled slot** → right panel shows: augment name, **Upgrade**, **Re-roll**, **Remove**
  - Remove → unslots augment (returns to augments inventory) → right panel returns to empty-slot view

---

### Skill Augments Inventory Tab

- Lists all unslotted Skill Augment items
- **Craft button** at top → switches view to craftable Skill Augment types (one button per type)
  - Click a type → crafts it (1 material) → adds to Skill Augments inventory → returns to owned list
- **Left-click an owned augment** → opens Skill Augment Modify Panel (no Remove — it's unslotted)

### Skill Augment Modify Panel (from inventory)

Augment name + **Upgrade** + **Re-roll** (no Remove). Same visual as the filled-slot view in the Skill Modify Panel.

---

### Equipment Augments Inventory Tab

- Lists all unslotted Equipment Augment items
- **Craft button** at top → switches view to craftable Equipment Augment types (one button per type)
  - Click a type → crafts it (1 material) → adds to Equipment Augments inventory → returns to owned list
- **Left-click an owned augment** → opens Equipment Augment Modify Panel (no Remove — it's unslotted)

### Equipment Augment Modify Panel (from inventory)

Augment name + **Upgrade** + **Re-roll** (no Remove). Same visual as the filled-slot view in the Equipment Modify Panel.

---

### Equipment Inventory Tab

- **Slot type selector** — four buttons at top (Weapon / Hat / Body / Ring); no type selected by default
- Once a type is selected:
  - Lists all unslotted gear of that type
  - **Craft button** → switches view to craftable subtypes:
    - Weapon → Sword / Bow / Wand
    - Hat → Heavy / Medium / Light
    - Body → Heavy / Medium / Light
    - Ring → Ring
    - Click a subtype → crafts it (1 material) → adds to inventory → returns to owned list
  - **Left-click an owned item** → opens Equipment Modify Panel (no Remove — it's unequipped)
  - **Right-click an owned item** → equips to first empty valid slot for that type
- **Right-click a filled equipment slot** → unequips; item returns to inventory

### Equipment Slot (character loadout)

- **Empty equipment slot** → opens the Equipment component pre-filtered to that slot's type (slot type selector step skipped)
  - Craft button → crafts item + auto-equips it → opens Equipment Modify Panel
  - **Left-click an owned item** → equips it → opens Equipment Modify Panel
  - **Right-click an owned item** → equips it (quick path, panel does not open)
- **Filled equipment slot**:
  - **Left-click** → opens Equipment Modify Panel directly
  - **Right-click** → unequips; item returns to inventory

### Equipment Modify Panel

```
[ Item Name ]  [ Upgrade ]  [ Re-roll ]  [ Remove ]   ← Remove only when item is equipped
─────────────────────────────────────────────────────
[ A1 ]   │  < context-sensitive right panel >
[ A2 ]   │
[ A3 ]   │
```

**Augment slot selected (left column):**
- **Empty slot** → right panel shows the Equipment Augments component (same as Equipment Augments inventory tab; different handlers):
  - Craft button → crafts Equipment Augment + auto-slots it → right panel transitions to filled-slot view
  - Click an owned Equipment Augment → slots it → right panel transitions to filled-slot view
- **Filled slot** → right panel shows: augment name, **Upgrade**, **Re-roll**, **Remove**
  - Remove → unslots augment (returns to Equipment Augments inventory) → right panel returns to empty-slot view
