# Game Design Document — UI Interaction Mechanics

> Part of the GDD. Screen layouts and HUD: `gdd-ui.md`. Item tiers, gear slot roles, stat tables: `gdd-progression.md`.
> Living document — details will evolve as the game is playtested.

## Character Screen — Interaction Model

**Reusable components — the core pattern.** Skills and Augments share one design pattern: a component that shows a Craft button and a list of owned items, reused in both the inventory tab and inside slot interactions. The component's event handlers differ by context (inventory vs. slot), but the visual is identical — styling changes propagate everywhere automatically.

* **Re-roll functionality:** The `[ Re-roll ]` buttons shown on skills, gear, and augments throughout the Modify panels are **disabled / deferred to v2** for the v1 milestone (as randomized stats and re-rolling costs are out of scope for v1).

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
[ Skill Name ]  [ Upgrade ]  [ Re-roll (v2) ]  [ Remove ]   ← Remove only when skill is slotted
─────────────────────────────────────────────────────
[ A1 ]   │  < context-sensitive right panel >
[ A2 ]   │
[ A3 ]   │
```

**Augment slot selected (left column):**
- **Empty slot** → right panel shows the Augments component (same as Augments inventory tab; different handlers):
  - Craft button → crafts augment + auto-slots it → right panel transitions to filled-slot view
  - Click an owned augment → slots it → right panel transitions to filled-slot view
- **Filled slot** → right panel shows: augment name, **Upgrade**, **Re-roll (v2)**, **Remove**
  - Remove → unslots augment (returns to augments inventory) → right panel returns to empty-slot view

---

### Skill Augments Inventory Tab

- Lists all unslotted Skill Augment items
- **Craft button** at top → switches view to craftable Skill Augment types (one button per type)
  - Click a type → crafts it (1 material) → adds to Skill Augments inventory → returns to owned list
- **Left-click an owned augment** → opens Skill Augment Modify Panel (no Remove — it's unslotted)

### Skill Augment Modify Panel (from inventory)

Augment name + **Upgrade** + **Re-roll (v2)** (no Remove). Same visual as the filled-slot view in the Skill Modify Panel.

---

### Equipment Augments Inventory Tab

- Lists all unslotted Equipment Augment items
- **Craft button** at top → switches view to craftable Equipment Augment types (one button per type)
  - Click a type → crafts it (1 material) → adds to Equipment Augments inventory → returns to owned list
- **Left-click an owned augment** → opens Equipment Augment Modify Panel (no Remove — it's unslotted)

### Equipment Augment Modify Panel (from inventory)

Augment name + **Upgrade** + **Re-roll (v2)** (no Remove). Same visual as the filled-slot view in the Equipment Modify Panel.

---

### Equipment Inventory Tab

Reusable component: a VBoxContainer whose content changes in-place through three states.

**Default state:**
- **Craft button** at top → switches component to type-pick state
- Separator + "Owned Gear" section label
- Lists all unslotted gear (font slightly smaller than Craft button to distinguish)
  - **Left-click** → opens Equipment Modify Panel
  - **Right-click** → equips to first empty valid slot for that type
- Empty: shows "None owned"

**Type-pick state:**
- ← Back button → returns to owned list
- Four buttons: Weapon / Hat / Body / Ring → switches component to subtype-pick state for that slot

**Subtype-pick state:**
- ← Back button → returns to type-pick state
- Material count / inventory-full status label
- One button per craftable subtype for the chosen slot:
  - Weapon → Sword / Bow / Wand
  - Hat → Heavy / Medium / Light
  - Body → Heavy / Medium / Light
  - Ring → Ring
  - Click a subtype → crafts it (1 material) → returns to owned list

### Equipment Slot (character loadout)

*(Separate spec — not yet implemented.)*

- **Empty equipment slot** → opens the Equipment component (same VBoxContainer, different context)
- **Filled equipment slot**:
  - **Left-click** → opens Equipment Modify Panel directly
  - **Right-click** → unequips; item returns to inventory

### Equipment Modify Panel

```
[ Item Name ]  [ Upgrade ]  [ Re-roll (v2) ]  [ Remove ]   ← Remove only when item is equipped
─────────────────────────────────────────────────────
[ A1 ]   │  < context-sensitive right panel >
[ A2 ]   │
[ A3 ]   │
```

**Augment slot selected (left column):**
- **Empty slot** → right panel shows the Equipment Augments component (same as Equipment Augments inventory tab; different handlers):
  - Craft button → crafts Equipment Augment + auto-slots it → right panel transitions to filled-slot view
  - Click an owned Equipment Augment → slots it → right panel transitions to filled-slot view
- **Filled slot** → right panel shows: augment name, **Upgrade**, **Re-roll (v2)**, **Remove**
  - Remove → unslots augment (returns to Equipment Augments inventory) → right panel returns to empty-slot view
