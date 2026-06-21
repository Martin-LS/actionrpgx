# Game Design Direction

Items to discuss, in no particular order. Cross off when resolved.

---

## Open for Discussion

- [ ] **Skill slots — count and cast model.** Moving from 1 auto-cast skill slot to up to 5 skill slots, all manual cast. Questions to resolve:
  - Exact slot count (up to 5, but do all 5 unlock at once or progressively?
  - Keybindings — what keys map to slots 1–5?
  - Auto-cast removal: is the toggle gone entirely, or retained as an option per slot?
  - How does the HUD skill bar change to show 5 slots?
  - Does each slot have its own cooldown displayed independently?
  - Any restrictions on what can go in each slot (e.g. one movement skill, one primary, etc.) or fully freeform like PoE?

---

## Resolved

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
