# Tech To-Do

Code, systems, UI, and art tasks only. Design decisions live in the GDD files.

---


## Tech Spec Gaps — GDD designed, no technical spec written yet

Items where the GDD defines the design but no corresponding technical specification exists in the tech docs. Write the spec before implementing.

### UI



> **(v1 — postponed)**

- [ ] Spec Run Results Overlay content — what data is displayed (XP gained, level reached, materials earned, coins, win/lose state) and how it is populated from RunSession

### Systems

> **(v1 — postponed)**

- [ ] Spec runtime targeting resolver — which class owns it, how Entity/Position/Self targeting shapes are resolved at runtime, cursor-to-world projection, range clamping for Position skills, auto-pick logic for Entity skills
- [ ] Spec zone skill lifecycle — how Fixed-Zone, Stackable-Zone, and Triggered-Zone-Burst zones are spawned/tracked/evicted; proximity detection for Triggered-Zone-Burst; Tracked-Tick entity follow; stack cap eviction order
- [ ] Spec AoE radius modifier pipeline — `AoEModifier` field on `SkillData` or `StatBlock`, accumulation across augments, application of formula `effective_radius = base_radius × sqrt(1 + total_aoe_pct_increase)`
- [ ] Spec Proximity Cluster System — which class owns the computation, connected-component algorithm (flood fill vs. union-find), when recomputation fires, Dormant→Idle transition on `MapReady` for pre-placed enemies



---

## Spec vs Code Gaps — implemented differently or not yet done

These were found by cross-checking tech spec against live code. Each entry is a confirmed delta; clarify intent before closing.

### Combat / Stats

- [ ] **Evasion not applied in TakeDamage** *(v2+)* — `BuildStatBlock()` computes `StatId.Evasion` (Dex × DexToEvasion) but `PlayerController` never reads it. Deferred by design for v1. When implemented: probability roll (`if Random() < _evasion: return`), not guaranteed full miss; v2 will tune the balance. Field `_evasion` also needs adding to PlayerController.
- [ ] **Physical/Magic Resistance scaling unimplemented** *(v2+)* — Strength is supposed to scale Physical Resistance and Intelligence is supposed to scale Magic Resistance, but StrToPhysResistance and IntToMagResistance are set to 0f in PrimaryStatConversions.cs and CharacterData.cs doesn't generate resistance modifiers.
### Skill Augments

- [ ] **AugmentInstance.TriggerChance (Skill and Equipment) has no gameplay effect** *(v2+, not in scope)* — Value is stored and serialized. Critical Strike uses hardcoded `BalanceConfig.SkillAugments.CritChance`; Slow uses `EotData.ApplyChance`; Equipment Augments trigger at 100% or static rates. Per-instance proc % wiring deferred until v2 balance pass.

### Inventory Systems

### UI

- [x] **Skill slot picker — parity with Equipment slot overlay.** Three sub-tasks to bring `ShowSkillSlotPicker` / `ShowCraftSkillAndSlot` in line with the equivalent equipment flow implemented this session:
  1. [x] **Icon grid** — `ShowSkillSlotPicker` renders owned skills as plain `MakeModifyButton(skill.Name)` text buttons. Extract a `BuildSkillIconGrid(VBoxContainer container, IList<SkillItemInstance> items, int gridSlots, Action<SkillItemInstance> onLeftClick, Action<SkillItemInstance> onRightClick)` helper (same shape as `BuildEquipmentIconGrid`) and use it in both `RefreshSkillsInventory` and `ShowSkillSlotPicker`. Icon buttons should use `skill.IconPath`, `ApplyTierStyle`, and `BuildSkillTooltip` — identical to how the inventory tab already renders them.
  2. [x] **Craft flow** — `ShowSkillSlotPicker` currently closes its own overlay before opening a separate `ShowCraftSkillAndSlot` overlay, meaning if the user cancels the craft step they are left on the bare character screen with no picker open. Equipment's `ShowEquipmentSlotCraftSubtype` instead rebuilds the same vbox in-place (via `foreach child.QueueFree()` + rebuild) with a `← Back` button that returns to the default state. Change `ShowCraftSkillAndSlot` to follow this pattern: instead of a new overlay, pass the picker's vbox into a `ShowCraftSkillSubtype(VBoxContainer vbox, int slotIndex, Control overlay)` method that clears and rebuilds in-place, with Back → `ShowSkillSlotDefault(vbox, slotIndex, overlay)`.
  3. [x] **Label cleanup** — rename top-level `"Remove"` → `"Un-Socket"` in `ShowSkillModifyPanel` header (only shown when `charSlotIndex >= 0`, i.e. skill is actually slotted); rename augment right-panel `"Remove"` → `"Un-Socket"` in `BuildFilledAugSlotPanel`. Same applies to equipment augment modify panel. Conditional visibility is already correct — label-only changes.

- [ ] **Split Augments inventory tab.** As designed in `gdd-augments.md`, Skill Augments and Equipment Augments should live in separate inventory tabs rather than a single combined "Augments" tab:
  1. **UI Tabs** — In `character_screen.tscn`, split the single "Augments" tab under `InventoryTabs` into two separate tabs: "Skill Augs" and "Equip Augs" (each containing their own ScrollContainer and GridContainer).
  2. **Refactor C#** — Split `RefreshAugmentsInventory()` in `CharacterScreen.cs` into `RefreshSkillAugmentsInventory()` and `RefreshEquipmentAugmentsInventory()`, updating them to bind to their respective grids and populate only their respective items.

---

## Systems / Features

> **Not in scope.**

- [ ] Map selection screen — currently one hardcoded arena; no selection or variety
- [ ] Boss mechanic — run win condition triggers on timer expiry; boss is unimplemented

---

## Art / Assets

> **Not in scope.**

- [ ] Hollow Dark Forest assets — floor tile, tree trunk wall, wall corner (Blender); replace placeholder box geometry in DungeonGenerator
- [ ] **Roll animation** *(v2+)* — No roll clip exists in the player GLB; characters slide while dodging. Needs rig animation in Blender.
