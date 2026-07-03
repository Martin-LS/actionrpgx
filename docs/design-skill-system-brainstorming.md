# Skill System — Brainstorming Scratchpad

> **Status: OPEN brainstorm, not committed design.** This is a working doc spanning multiple sessions.
> Nothing here is final until it is resolved and moved into `design-skills.md`. Rejected ideas get deleted, not archived.
> For the committed skill design (the 12 v1 prototypes, AoE math, targeting rules), see [design-skills.md](design-skills.md).

---

## What this doc is for

Designing the **v2 named skills** — the actual flavoured skills (Strike, Cyclone, Blizzard, …) that get cloned from the 12 v1 prototypes. v1 is all prototypes (mechanic proofs with no identity); this doc works out how named skills acquire distinct identity and how many there are.

Hard constraints inherited from `design-skills.md` (do not relitigate here without flagging):
- **No per-skill damage multiplier.** Every hit deals `weapon base × stat block`. Tick and burst skills deal the same raw damage per hit.
- **Named skills are standalone clones of prototypes.** `BasedOn` is documentation only — no runtime template link. Editing a prototype never cascades to clones.
- **EoTs, mines, traps come from augments,** not baked into skills. A skill's base behaviour is its damage delivery.
- **Any augment sockets into any skill** — no tag gates (no-gate philosophy).

---

## Stat Ownership Matrix — MOVED

The Stat Ownership Matrix has been promoted to its own authoritative doc: **[design-stats.md](design-stats.md)** (Section 2). It governs every stat in the game and other docs defer to it. The guard note lives there. Two flags remain open there: `InherentEotIds` vs augment-only EoTs, and the missing `TickRate` field on `SkillData`.

---

## Open threads

- [x] **Naming & identity system** — RESOLVED 2026-07-04: Budget/Identity framework confirmed (see Thread 1).
- [x] **First-wave shape & count** — RESOLVED 2026-07-04: deep not broad; two sibling clones each on 4 prototypes = 8 named skills (see Thread 2).
- [ ] Design the 8 first-wave named skills themselves (names, elements, budget-spend shapes, tier tracks).
- [ ] Full roster beyond the first wave — gated on first-wave playtest verdict on the identity framework.
- [ ] Acquisition specifics — crafted via Craft New per the existing pillar; recipe costs deferred to the reward/economy surface (D4 in `design-stats-deferred.md`).

---

## Thread 1 — Naming & Identity System

### The core tension

Every skill deals the same raw number per hit (`weapon base × stat block`). So two named clones of the *same* prototype can only differ by: cooldown, tick rate, AoE radius, damage type, wind-up, duration/stack params, and VFX.

The trap: most of those are **power levers, not identity levers.** Lower cooldown = just more DPS. Bigger AoE = just more value. With no damage multiplier to buy a downside back, "identity" would collapse into "which clone is numerically best." The multiplier is what normally does that job in PoE/LE, and it was deliberately removed.

### Framework — Budget levers vs. Identity levers *(CONFIRMED 2026-07-04)*

Split the levers into two piles and treat them differently.

**Budget levers** — change total throughput, so across any two clones of the same prototype they must net to roughly the same power budget (Balancer owns the exchange rate):
- Cooldown ↔ AoE radius ↔ Focus cost ↔ wind-up.
- You may *trade* among these, but the sum stays on-budget. A bigger blast pays with a longer cooldown or a wind-up. Same DPS, different *shape*.

**Identity levers** — (nearly) free because they don't move raw throughput, they move *where the skill sits in the ecosystem*:
- **Damage type** — strongest identity lever. Couples the skill to enemy resistances and augment/EoT affinity (Burn wants Fire, etc.). A Fire self_burst and a Cold self_burst have identical throughput but feed different build stacks.
- **VFX / animation / sound** — pure felt identity, zero balance cost. Carries most of the "this is Blizzard, not Nova" weight.
- **Wind-up as telegraph** — even at equal budget, a telegraphed heavy hit *feels* different from an instant one.

**Net:** identity = **prototype (delivery fantasy) × damage type × VFX × budget-spend shape × tier track** — not raw power.

### Thread 1 resolutions (grill session 2026-07-04 — all confirmed)

1. **Budget/Identity split confirmed** as the governing rule, enforced as design discipline ("no free lunch on budget levers" — every budget-lever advantage visibly paid by a budget-lever cost), not exact math. Exchange rates are Balancer-owned.
2. **Budget parity is defined at equal tier.** Skill tier advances a fixed per-skill upgrade track over budget levers only (cooldown down, or radius up, etc.) and never touches hit size. Which lever the track improves is itself an identity axis (the Nova that grows vs. the Nova that quickens). Recorded in `design-skills.md` and `design-progression.md`.
3. **Damage-type mutability = option 2 (status quo, now a rule):** fixed at skill creation, not re-rollable by crafting; only a socketed augment may override it at fire time (the socket cost is what preserves default identity). Recorded in the `design-stats.md` matrix.
4. **No stronger mechanical identity lever for now.** Budget-spend *shape* is the mechanical identity (rhythm: heavy-slow vs. rapid-small at equal throughput). `InherentEotIds` resolves to **delete** — "EoTs are augment-only" stays pure. Escape hatch: if the first wave feels same-y in playtesting, revisit the signature-EoT carve-out consciously (matrix amendment first).

### Engine prerequisite for v2 named skills — per-skill VFX mapping

The framework leans on VFX as a primary identity carrier, but the engine has **no per-skill VFX/animation mapping** (`technical-systems.md` — animation/VFX is delivery-driven; all skills of a `SkillType` look identical, and the cyclone ring is hardcoded to `SkillType.Channeled`). Per-skill VFX mapping must be built before the first named-skill wave can carry visual identity. Write up as a GitHub issue when v2 work starts. Note: self_channeled_tick clones lean hardest on this.

---

## Thread 2 — First-wave roster shape *(RESOLVED 2026-07-04)*

**Deep, not broad: two sibling clones each on 4 prototypes = 8 named skills.** Rationale: the framework's entire risk is two clones of the *same* prototype collapsing into a numbers choice — a broad wave (one clone per prototype) never tests siblings side by side. Host prototypes (all implemented in `SkillRegistry.cs`):

| Prototype | Why chosen |
|---|---|
| entity_burst | Starter/most-played delivery; maximum playtest exposure for siblings |
| self_burst | Purest test of the cooldown↔AoE budget trade |
| fixed_zone_tick | Brings the zone lever set (radius, duration, tick rate) into the test |
| self_channeled_tick | The distinct rhythm (channel vs. pulses); stress-tests budget-spend-shape identity. Most dependent on per-skill VFX work |

Acquisition: crafted via Craft New (existing pillar). Recipe costs deferred to the reward/economy surface (D4).

---

## Session log

- **2026-07-03** — Doc created. Opened Thread 1 (identity system); proposed the Budget/Identity lever split. Awaiting confirmation of the model before expanding.
- **2026-07-04** — Traced where every stat lives in code (two layers: `BalanceConfig` constants → copied into `SkillData` by `SkillRegistry`; `TickRate` is the orphan, read straight from `BalanceConfig` by `WeaponController`). Formalised the Stat Ownership Matrix. Decided: damage type is skill-authoritative. Added guard note to push back on any design that violates the matrix. Left `InherentEotIds` and the missing `TickRate` field as open flags.
- **2026-07-04** — Promoted the matrix out of this brainstorm into its own authoritative GDD doc **`design-stats.md`** (Section 1 = stat catalogue, Section 2 = ownership matrix). Reconciled the move-speed row against the 4-source model in `design-mechanics.md`. Registered in `index.md`. This doc now just points to it.
- **2026-07-04 (grill session)** — Thread 1 fully resolved: framework confirmed, tier rule (budget levers only, parity at equal tier), damage-type mutability (option 2), no mechanical identity lever (`InherentEotIds` → delete). Thread 2 resolved: deep first wave, 2 siblings × 4 prototypes (entity_burst, self_burst, fixed_zone_tick, self_channeled_tick). Also settled outside this doc: no-multiplier rationale documented in `design-skills.md`; speed-family stance (D5) decided & promoted; Boots drift → docs follow code; `design-progression.md` multiplier drift fixed. Next: design the 8 first-wave named skills.
