# Game Design Document — Meta-Progression, Gear & UI

> Part of the GDD. See also `gdd-mechanics.md` for combat, characters, and run structure. See `gdd-skills.md` for skill design and prototypes. See `gdd-augments.md` for all augment design, prototypes, and augment resolution. See `gdd-ui.md` for screen layouts and HUD. See `gdd-ui-mechanics.md` for interaction flows.
> Living document — details will evolve as the game is playtested.

## Meta-Progression (Between Runs)

### Level Bonuses (automatic)
Each level gained during a run permanently increases the character's HP and damage. Bonuses scale with both archetype and level — each archetype grows faster in the stats that define its playstyle. These stack across all runs and are applied automatically on level-up. Exact growth coefficients are owned by the Balancer.

### Item Tiers

All items — both equipment and skills — have a **tier** that represents quality and power level. Tier is shown as the **border colour** of the item slot everywhere it appears (inventory, slots, pickers). The slot background is always Pale Slate (`#8AA0AE`) — a neutral light gray that lets the icon art read clearly.

| Tier     | Border colour | Hex       | Notes                  |
|----------|---------------|-----------|------------------------|
| Common   | Dark Slate    | `#4A5560` | Starter / lowest power |
| Uncommon | Ash Grey      | `#6B8090` | Mid tier               |
| Rare     | Dark Gold     | `#A07810` | Highest tier (v1)      |

Border colours are taken from the Loot Rarity Border column in `color-scheme.md` for consistency across inventory, loot drops, and minimap dots.

Exact stat differences per tier are TBD. Higher tier also unlocks more augment slots on skill and equipment items (see Skill Augments, Equipment Augments).

---

### Gear Slots

Characters can equip up to 4 gear items (one per gear slot) and 5 skill items. All items persist between runs. Each slot has a distinct role:

| Slot      | Role                                                             | Progression axis                    |
|-----------|------------------------------------------------------------------|-------------------------------------|
| Weapon    | Root of base damage; sets Weapon Range; determines visual delivery of skill animations | Tier → higher base damage + higher Weapon Range |
| Hat       | Survival — HP, Speed, damage reduction (%) by category; Range Modifier by category | Tier → better stats within category |
| Body      | Survival — HP, Speed, damage reduction (%) by category; Range Modifier by category | Tier → better stats within category |
| Ring      | Mitigation — physical resistance (%)                             | Tier → higher resistance            |
| Skill ×5  | Active abilities used during a run. All 5 slots available from the start — no unlock progression | Tier → stronger effect + lower cooldown (cooldown is a skill attribute — no character-level attack speed stat exists) |

#### Skill Slots

5 skill slots shown on the HUD and used during a run. All 5 are available from the start — no unlock progression. Any archetype can equip any skill in any slot — fully freeform, no restrictions.

v1 starter skill: **Entity-Burst** in slot 1 (physical type, 1.0× multiplier), slots 2–5 empty. All archetypes start with plain Entity-Burst — no augments socketed. Weapon drives the delivery animation; skill defines the damage type.

Skill items are crafted (Craft New — accessible from an empty skill slot, left-click → Craft New; not yet implemented in v1) and equipped from the **Skills inventory tab**. Default keybindings: Q E R F + one mouse button for slots 1–5. Rebindable.

#### Skill Augments

> Skill Augment design, v1 augment list, trigger system, and augment resolution order are in `docs/gdd-augments.md`.

**Skill Augment slots per tier:**

| Skill tier | Skill Augment slots |
|------------|--------------|
| Common     | 1            |
| Uncommon   | 2            |
| Rare       | 3            |

#### Equipment Tags

Armour pieces (Hat and Body) carry a **category tag** (`Heavy`, `Medium`, `Light`) that drives their stat profile — see Hat & Body below. No augment gating — any augment can socket into any equipment item regardless of category. Category is fixed per item.

#### Equipment Augments

> Equipment Augment design, v1 augment list, and trigger system are in `docs/gdd-augments.md`.

**Equipment Augment slots per tier:**

| Equipment tier | Equipment Augment slots |
|----------------|------------------------|
| Common         | 1                      |
| Uncommon       | 2                      |
| Rare           | 3                      |

#### Weapon

Weapons do two things: provide the **base damage number** for all skill damage calculations, and set **Weapon Range**. No weapon gates any skill — every skill fires regardless of what is equipped.

**Weapon is the root of the damage number.** The skill defines the damage type and multiplier on top of the weapon's base. Upgrading weapon tier is the primary way to increase damage output. Each weapon type has a **passive identity bonus** that applies when the equipped skill's damage type matches the weapon's associated type — rewarding matched builds without restricting those who don't.

**Delivery is fixed per weapon type.** The weapon always drives the attack animation — a Sword always swings, a Bow always shoots, a Wand always fires a bolt. Skills do not override delivery.

**Weapon Range** is a flat number stat visible on the weapon item. Effective Range on the character sheet reflects this after armour modifiers are applied (see Hat & Body).

Range values are expressed in **tiles** (the canonical internal distance unit). One tile = 36 Godot world units — this conversion lives in `GameScale.TileSize`.

| Weapon type | Base Damage (tier 1) | Weapon Range | Delivery | Identity bonus (tier 1) |
|-------------|----------------------|--------------|----------|------------------------|
| Sword       | 15                   | 1.5 tiles    | `Melee`  | +10% physical damage (applies when skill is physical type) |
| Bow         | 12                   | 2.5 tiles    | `Ranged` | +8% crit chance (type-agnostic — applies always) |
| Wand        | 18                   | 2 tiles      | `Ranged` | +10% magic damage (applies when skill is magic type) + EoT affinity (higher base trigger chance for EoT augments) |

**Tier scaling:** tier 2 = ×1.5 base damage, tier 3 = ×2.0 base damage. Identity bonus % also scales with tier (TBD). All values are placeholder — owned by the Balancer.

Any character can equip any weapon.

**Visuals (in-run):** Weapon is rendered on the character model.

#### Hat & Body

Hat and Body are the two armour equipment slots. Each piece has a **category** that defines its identity and its equipment tag. Category is fixed per item — crafting a higher-tier Heavy hat makes it stronger within that category, not a different category.

Any character can equip any category in any slot. Slots are independent — a character can mix freely (e.g. Heavy hat, Light body).

| Category | Equipment tag | HP       | Speed   | Damage Reduction | Range Multiplier (per piece) |
|----------|---------------|----------|---------|------------------|------------------------------|
| Heavy    | `Heavy`       | High     | Penalty | Yes (%)          | ×0.85 (placeholder — Balancer v2+) |
| Medium   | `Medium`      | Moderate | Neutral | —                | ×1.00 (neutral)              |
| Light    | `Light`       | Low      | Bonus   | —                | ×1.15 (placeholder — Balancer v2+) |

Stats above apply per piece — each slot contributes its category's stats independently.

**Range Multiplier applies to all weapons.** Each armour piece multiplies Effective Range independently. Two pieces of the same category compound: e.g. two Heavy pieces at ×0.85 each → ×0.72 total. The multiplicative formula means higher base ranges are pulled down more by heavy armour than lower base ranges — a 2.5-tile bow takes a larger absolute hit from heavy armour than a 1.5-tile sword.

**Effective Range** (visible on the character sheet):
- `Weapon Range × hat Range Multiplier × body Range Multiplier` (in tiles), then × `GameScale.TileSize` → world units
- Range buff bonuses (v2+) are applied after the multiplier step as a flat tile addition.

Displayed as tiles in the UI.

**Range buffs (v2+):** Skills may temporarily or permanently modify Effective Range mid-run. Any such buff adds a flat tile bonus on top of the multiplied baseline. Effective Range is recalculated whenever a range buff is applied or expires.

**Visuals (in-run):** Hat, Body, and Weapon are rendered on the character model. Ring has no visual representation.

Heavy suits close-range builds taking hits; Light suits ranged builds that kite; Medium suits mixed or flexible builds. Mixing categories (e.g. Heavy hat, Light body) produces an intermediate multiplier.

#### Ring

Rings grant **physical resistance (%)**. No category, no equipment tags — any character can equip any ring, and any Equipment Augment can socket into a ring. Tier is the only progression axis: higher-tier rings give higher resistance.

#### Starter Gear

Each character starts with one item per slot, matched to their archetype:

| Archetype | Weapon         | Hat            | Body           | Ring          | Skill slot 1 (of 5) |
|-----------|----------------|----------------|----------------|---------------|---------------------|
| Warrior   | Sword (tier 1) | Heavy (tier 1) | Heavy (tier 1) | Ring (tier 1) | Entity-Burst (no augment) |
| Rogue     | Bow (tier 1)   | Light (tier 1) | Light (tier 1) | Ring (tier 1) | Entity-Burst (no augment) |
| Mage      | Wand (tier 1)  | Medium (tier 1)| Medium (tier 1)| Ring (tier 1) | Entity-Burst (no augment) |

All archetypes start with Entity-Burst — physical type, 1.0× multiplier, no augments. Weapon drives the delivery animation. Crit is a Bow identity bonus, not a Rogue identity; magic damage affinity is a Wand identity, not a Mage identity.

Specific item names and exact stat values are TBD.

**Acquisition:** Gear is not dropped by enemies — enemies drop crafting materials only. All items (gear, skills, augments) come from crafting.

**Item identity:** Each item is a unique instance with its own ID. Items **upgrade in-place** — tier increases on the existing item rather than producing a new one. The item's border colour updates to reflect its new tier (see Item Tiers).

**Inventory:** Crafted (unequipped) items go into the **account inventory** — a shared pool accessible by every character. The inventory has three tabs:

| Tab | Contents | Capacity |
|---|---|---|
| Equipment | Crafted gear (weapons, armour, accessories) | 50 items |
| Skills | Crafted skill items | 50 items |
| Skill Augments | Crafted Skill Augments | 50 items |
| Equipment Augments | Crafted Equipment Augments | 50 items |

Equipped items are held separately in the character's slots and do not count against inventory capacity. See *Character Screen — Interaction Model* for how each tab behaves.

---

## Currencies

### Coins
Earned during runs (25% enemy drop). **Account-shared** — earned by any character, spendable by any. Spend mechanic TBD — coins accumulate but have no current use.

### Crafting Materials
Crafting materials are the primary run reward — the only meaningful thing enemies drop. They are tiered — common through exotic. Each tier drops at a different rate during runs and enables crafting of items at the corresponding tier. **v1:** all items cost 1 crafting resource to craft. Future versions will use material combinations for higher-tier recipes.

| Tier    | Current name     | Drop rate | Enables                          |
|---------|------------------|-----------|----------------------------------|
| Common  | crafting resource | 20%       | Low-tier items                   |
| [TBD]   | —                | Rarer     | Mid-tier items                   |
| Exotic  | —                | Very rare | Exotic / high-tier items         |

- All materials are **account-shared** — earned by any character, spendable by any character
- The more exotic the craftable item, the rarer its required materials
- Specific tiers, drop rates, and material combinations will be designed when crafting is fleshed out

---

## UI & HUD

See `gdd-ui.md` for screen layouts and HUD, and `gdd-ui-mechanics.md` for interaction flows.
