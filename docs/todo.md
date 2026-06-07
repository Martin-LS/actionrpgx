# To-Do

Flat list. No priority order within sections — reorder as needed.  
Update this as tasks are completed or new work is identified.

---

## Visuals / Art

- [x] Armour colours — set distinct colours per tier; all three tiers use the same base model (armour_heavy.blend) recoloured: Heavy=iron dark, Medium=moss green, Light=ice blue. Hats scaled 1.1x so they wrap around the head.
- [x] Idle animation — breathing cycle added to player.glb (Chest rises 0.06 units, shoulders lift 3°, 40-frame loop). PlayerController plays "idle" when standing still instead of stopping.
- [x] Attack animation arm swing — UpperArm_R X scaled 1.5×, Z sweep channel added (−20°→+40° horizontal arc), Chest X scaled 1.5×
- [ ] Armour models — attachment offsets will need tuning against the new box character proportions (KayKit warrior retired). Chest/Head/Hand_R bone names match; positions differ.
- [ ] Enemy variety — only Skeleton in v1. GDD lists runner and ranged types as TBD

---

## Animation / Equipment

- [ ] Weapon rotation fine-tuning — sword blade currently oriented along bone -Z; may need a Blender rotation tweak depending on how it looks in-game during attack
- [x] Review & approve all 3 animations in Blender before exporting GLB: run (frames 1–40), melee_atack (1–40), range_atack (1–40). All saved in player.blend. DO NOT export until approved.
- [x] Export player.glb from player.blend after animation approval (use Blender MCP, export_nla_strips=True)
- [x] idle animation — breathing cycle built in player.blend (5 bones, 40 frames: Chest X +3°, Spine X +1.5°, Head X +2°, UpperArm_L/R Z ±3°), exported to player.glb, looped via AnimationTree
- [ ] Implement partial body blending in Godot AnimationTree — attack animations drive upper body only, run legs play underneath during combat movement
- [x] Armour GLB split — armour_heavy/medium/light.blend each contain a combined hat+body mesh. Split each into two separate GLBs (hat_heavy.glb, hat_medium.glb, hat_light.glb, body_heavy.glb, body_medium.glb, body_light.glb), then update the hat/body path lookups in `PlayerController.cs`
- [x] Bow orientation fix — bow geometry rotated 90° around Z in Blender so limbs span X (left-right) instead of Y (forward); now clearly visible from top-down camera
- [ ] Wand model orientation — weapon_wand.glb needs the same horizontal-span fix as the bow
- [x] Rogue and Mage character models — all three archetypes now share `player_character.glb`, a custom box-geometry character with 17-bone rig and 4 animations (idle, run, attack_melee, attack_range). KayKit warrior model retired.

---

## Gameplay / Balance

- [ ] Stat balancing — archetype multipliers all marked TBD in GDD; needs a tuning pass
- [x] BalanceConfig.cs — all numeric balance values extracted to `src/balance/BalanceConfig.cs`; covers weapons, armour, skills, EoTs, enemies, drops, pickups, archetypes, level-up coefficients
- [x] Difficulty scaling — enemies currently kill a Level 10 character in ~20 seconds; too fast for testing. Tune spawn rate / HP / speed curves
- [ ] Coins — drop and accumulate but have no spend mechanic yet
- [x] Manual skill activation — keys 1/2/3 fire skill slots; each slot has `AutoActivate = true` by default so skills still fire automatically on cooldown
- [ ] Map level attribute — XP scaling by map level exists in GDD but no map selection screen or map level setting yet
- [x] Weapon-adaptive skill system — skills have no weapon gate; delivery (Melee/Ranged) is determined by skill delivery tag, falling back to weapon PreferredDelivery for untagged skills (e.g. Strike)
- [x] Strike skill — universal starter skill; weapon-adaptive delivery, Attack tag, 0.8s cooldown, 1× physical damage; all 3 slots pre-filled at character creation

---

## Systems / Features

- [ ] Pause screen — ESC is listed as pause in GDD controls but not implemented
- [ ] Boss mechanic — run win condition triggers when timer expires but boss is TBD
- [x] Map generation — single 24×24 KayKit dungeon arena; floor tiles, perimeter walls, corner pieces, scattered props (pillars, barrels, crates, torches); collision boundary; player spawns at centre; enemies spawn on floor tiles
- [ ] Map selection screen — only one arena map; no selection or variety yet
- [ ] Enemy pathfinding — enemies walk directly toward player; get stuck if spawned behind walls or in corridors leading away from player
- [ ] Archetype defense system — Rogue dodge and Mage focus shield are future design (GDD future notes section)
- [ ] Higher-tier crafting materials — drop system only has common tier; rarer tiers TBD

---

## UI / Character Screen

- [x] Character Screen redesign — GearCrafting and SkillCrafting tabs removed; Loadout tab is now two-column (character/gear left, inventory right); right-click equips/unequips; left-click shows context menu (Modify, Delete)
- [x] Modify panel popup — dark modal overlay; shows item name/tier, Upgrade button (costs 1 Common, disabled if max tier or insufficient materials), and augment socket rows (click empty slot → picker from inventory; click filled slot → removes augment)
- [x] Rarity border gap — two-panel technique: border ring (DrawCenter=false) + inset fill panel (8px offset); 3px dark gap visible between border and pale slate fill
- [x] PopupMenu theming — all popup menus use Iron & Slate colours, Exo 2 font, dark background via `NewStyledPopup()` factory
- [ ] Craft New from empty slot — left-click on an empty gear or skill slot should offer a Craft New option (opens recipe list for that slot type); currently only opens inventory picker

---

## Tech / Polish

- [ ] UID warnings in log — `xp_shard.tscn`, `coin_pickup.tscn`, `health_pickup.tscn` all log invalid UID warnings on every run. Cosmetic but noisy
- [ ] Dev overlay — only has a Speed slider; consider adding god mode / invincibility toggle for testing
- [x] Delete orphaned `src/skills/AugmentData.cs` and `src/skills/AugmentRegistry.cs` — dead files superseded by the Support/SkillAugment system; nothing references them
- [x] Rename `SupportData.cs` → `SkillAugmentData.cs`, `SupportItemInstance.cs` → `SkillAugmentInstance.cs`, `SupportRegistry.cs` → `SkillAugmentRegistry.cs` — classes were renamed but filenames were not (C# doesn't require a match, but it's confusing)
- [x] UI theme — custom Iron & Slate theme at `res://assets/ui/game_theme.tres`; covers panels, buttons, labels, tooltips, line edits, popups. Default font: Exo 2. Set via `gui/theme/custom`. Replaces Themey Spacey theme.
- [x] Fonts — 6 Google Fonts downloaded to `res://assets/fonts/` (Exo 2, Cinzel, Cinzel Decorative, EB Garamond, Almendra, Inter). Exo 2 active as UI default; others available for headings/lore text.
- [x] Panel borders — gold `#D4A017` 1px border on all PanelContainer, TabContainer, and Panel nodes via project theme.
- [x] Tooltip styling — `TooltipButton` (`src/ui/TooltipButton.cs`) renders a two-section custom tooltip: gold bold title on line 1, pale slate body on remaining lines. Applied to all gear/skill/augment buttons in CharacterScreen.
