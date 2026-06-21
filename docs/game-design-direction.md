# Game Design Direction

Items to discuss, in no particular order. Cross off when resolved.

---

## Open for Discussion

- [ ] **Skill prototype review.** All existing prototypes are back on the table with the action RPG pivot — including those previously flagged as engine-proof only (Fixed-Zone-Tick, Tracked-Tick, Triggered-Zone-Burst, etc.). Go through each prototype and redefine: is it player-facing, what is it good for, what builds does it suit, and what needs to change for a manual-cast 5-slot design. This is a separate design pass — do not redesign the skill system here, just re-evaluate each prototype against the new direction.

- [ ] **Skill system design.** High-level design of the full skill system for the action RPG pivot — skill identity and uniqueness. Augment model and passive/active rules resolved separately. To be fleshed out as design discussion progresses.

---

## Resolved

- [x] **Crafting resource cost — v1 everything costs 1 crafting resource.** There is one crafting resource in v1, called "crafting resource." All craftable items — skills, augments, gear — cost 1 crafting resource. Resource cost balancing and material tiers are out of scope for v1. The crafting resource name and tiered cost system will be designed post-v1.

- [x] **Passive skills — removed from skill slots, live on armour augments instead.**
  - The dividing line: **skill slot = things you actively trigger** (requires a button press — Active, Channeled, toggled auras, War Cries). **Armour augment = things that happen automatically** (no button press, gear-driven).
  - There is no Passive skill type on the skill bar. Everything in a skill slot requires player input to fire.
  - Persistent stat buffs and background effects belong on armour augments (Mending, Retaliation, etc.), not skill slots.
  - Skills like War Cry stay on the skill bar — they are intentional activations with cooldowns, not background passives.

- [x] **Skill augment model — separate socketable items, one type per skill at a time.**
  - Augments are separate craftable items that live in inventory and socket into skill slots.
  - Augments can be freely removed and re-socketed into a different skill at any time.
  - **One of each augment type per skill at a time** — you cannot have two Burn augments on the same skill. This is the uniqueness rule.
  - Two different skill instances can each have their own Burn socketed independently — valid, each skill has its own sockets.
  - **All augments use a single trigger type: `on_enemy_hit_%`.** No `always` trigger. Even mechanical augments (Splash, Pierce) have a % chance — higher base % than effect augments, but never guaranteed. Keeps the system uniform and balanced.
  - The trigger % is a property of the augment item itself, rolled at craft time and re-rollable via crafting — pushing the % higher is a meaningful crafting goal.
  - **Possibly bound-on-socket in future** as a crafting resource sink — not current design, noted as a direction to consider.
  - **Crit is not an augment.** CritChance comes entirely from Dex, CritDamage from Str. Crits roll automatically on every hit — no augment needed to enable or boost them. The Critical Strike augment is removed.

- [x] **Skill slots — 5 slots, freeform, PoE2 keybinding layout.**
  - 5 skill slots available from the start — no unlock progression.
  - Fully freeform (PoE-style) — any skill in any slot, no typed slot restrictions.
  - Auto-cast retained in the codebase for dev convenience only (see cast/targeting model decision above) — not a player-facing design concern for skill slots.
  - Default keybinding layout follows PoE2: `Q E R F` + one mouse button for the 5 slots. WASD for movement, `Space` for dodge.
  - Players can rebind `Q E R F` to `1 2 3 4` for D4-style preference — Godot's input action system handles this natively.
  - HUD skill bar and per-slot cooldown display are implementation details — TBD when HUD is redesigned for 5 slots.

- [x] **Base stats — Strength, Dexterity, Intelligence.** Three primary stats drive character power and archetype identity. Each stat is the primary driver for one archetype: Strength → Warrior, Dexterity → Rogue, Intelligence → Mage.

  | Stat | Governs |
  |---|---|
  | **Strength** | PhysicalDamage, PhysicalResistance, MaxHp, CritDamage |
  | **Dexterity** | CritChance |
  | **Intelligence** | MagicDamage, MagicResistance, MaxFocus, FocusRegen |

  - CritChance on Dex (precision — you land the crit), CritDamage on Str (power — the crit hurts more).
  - MaxHp is under Strength (Str/Con merged — physical body, damage output, resistance, and survivability together).
  - **Movement speed is not a base stat.** No archetype gets it for free. Speed comes from gear, augments, or skills only.
  - **CDR is not a base stat.** See below.

- [x] **Cooldown Reduction (CDR) — weapon property, not a base stat.** CDR is a property on the weapon item itself. It is global — applies to all skills regardless of type, with no weapon-skill gate. Different weapon types have different base CDR values and different roll ranges; the weapon type determines the floor and ceiling, crafting determines where in that range you land.

  | Weapon | CDR character |
  |---|---|
  | Bow | High base, high ceiling — naturally the fast weapon |
  | Sword | Low base, low ceiling — speed is not its identity |
  | Wand | TBD |

  - CDR is not an augment slot. It is a core weapon stat alongside base damage and weapon range.
  - **v1: all weapon CDR is a fixed value per weapon type.** No roll ranges, no crafting variance. Every Bow of the same tier has the same CDR. Simple and shippable.
  - **Post-v1:** roll ranges and crafting-driven CDR improvement are the design direction — noted here to inform future crafting design, not to be designed now.

- [x] **Cast and targeting model — fully manual.** The game is designed for manual casting and manual targeting. Auto-cast is not the intended experience — it is retained in the codebase for development convenience only (and may or may not ship). All skill and combat design should assume a player who is actively pressing keys and directing targets, not one who is watching skills fire automatically.

- [x] **Progression model — fully craft-driven.** Items, skills, and maps are all obtained through crafting. Enemies drop crafting materials only — no direct item drops. The crafting system is the sole progression engine between runs. Detailed crafting mechanics TBD as the system is built out.

---

## Parked (not open for discussion yet)

- [ ] How to handle summons / totems
- [ ] Boss mechanic design — what happens when the timer expires; boss type, behaviour, rewards
- [ ] Coins spend mechanic — coins accumulate but have no spend purpose yet
- [ ] Map selection / map level — only one hardcoded arena; how maps are unlocked, selected, and scaled
- [ ] Enemy variety — GDD lists runner and ranged types as TBD; design when combat feel is proven
- [ ] Archetype defense system — Rogue dodge and Mage focus shield are post-v1 (see gdd-mechanics.md Future Design Notes)
- [ ] Rare craftable properties on skills and augments — out of scope for now but forms a consistent pattern: higher tier items can roll rare properties that modify core behaviour. Examples: damage conversion (on skills), cooldown reduction (on skills — cut from Equipment Augments in v1 as it's offensive utility and redundant with tier-based CDR), EoT spread chance (on augments). These features would make crafting deeper and rare materials more meaningful to hunt for. Pulls in re-roll and affix systems — design when crafting is being expanded.
