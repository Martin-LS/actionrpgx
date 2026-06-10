# Design Ideas — Parking Lot

> Uncommitted ideas worth remembering. Nothing here is scheduled or spec'd — this is a scratch space to capture concepts before they're lost. Pull from here when a topic becomes relevant.

---

## Weapon Design Approaches

*Context: exploring alternatives to hard weapon-skill requirements. Current design uses weapon affinity tags (Melee/Ranged/Magic) that grant flat damage bonuses to matching skills. These ideas push that further.*

---

### Idea: Weapons Grant Tags to Skills

Instead of skills requiring a weapon type, weapons *add* tags to all your skills. A sword adds `Slashing` and `Melee` tags; a bow adds `Projectile` and `Ranged`. Skills have no weapon requirement — but modifiers that require `Slashing` only activate when you're holding a sword.

The weapon shapes *how* skills behave rather than *whether* they're available. Same skill, different weapon → different modifier pool. Clean inversion of the PoE2 approach. Fits naturally into the existing tag system.

---

### Idea: Weapon as a Built-in Skill Modifier

The weapon itself is a modifier applied to compatible skills. Every weapon has a built-in mutation: a dagger adds "hits cause bleed," a staff adds "20% larger AoE," a bow adds "fires an additional projectile." The mutation only applies if the skill has a matching tag.

Any skill still fires with any weapon — the weapon just doesn't synergise unless tags match. Makes the "correct" weapon feel rewarding without hard-gating. Pairs naturally with the tag-grant idea above.

---

### Idea: Weapon Stance System

Decouple stance from equipment entirely. The player chooses a **stance** (Brawler, Marksman, Elementalist, etc.) independently of what weapon they're holding. The stance determines which skill tags are active; the weapon provides stats only.

Want Marksman stance with a sword? Fine — you're a precise melee fighter using the ranged modifier pool. Closer to Dragon's Dogma 2's Vocation system: identity comes from your chosen style, not your item slot.

---

### Idea: Dual-Nature Skills

Every skill has two modes — a melee version and a ranged version — and the equipped weapon determines which fires. "Blade Storm" swings in a close arc with a sword, fires energy projectiles with a staff. Same skill, same modifier slots, same cooldown; weapon transforms the delivery.

Modifiers work on both modes (shared tags), but some enhance only one ("increases range" does nothing in melee mode). Most interesting of the five but most work to implement.

---

### Idea: Weapon Affinity (Soft Lock)

Skills have an **affinity rating** per weapon type rather than a hard requirement. A bow skill might be 100% with bows, 60% with wands, 20% with swords. Off-affinity still works but at reduced effectiveness (damage and range scale down). Lets players make intentional tradeoffs — sometimes a slightly weaker off-affinity skill is worth it for what the weapon gives the rest of the build.

---

### Notes on Weapon Ideas

- Ideas 1 (tags) and 2 (mutation) are the most compatible with the current architecture and with each other — they could be layered on top of the existing affinity system without redesigning it.
- Ideas 3 (stance) and 4 (dual-nature) are more structural — they'd change how weapon identity works at the character level.
- All five avoid hard weapon-skill locks, which is the core design goal.

---

## Auto-Activate Cycling (Slot Round-Robin)

*Context: discussed after noticing that all auto-activate slots fire simultaneously when multiple skills become ready at the same instant. Triple Strike with all 3 slots auto-activate fires all 3 at the same frame — damage and VFX work fine (HitMelee runs per fire), but it feels like a burst rather than a rhythm.*

A single cursor cycles slot 1 → 2 → 3 → 1 continuously. At each step:
- If that slot has auto-activate ON and is not on cooldown → fire it → wait a minimum delay → advance to next slot
- If on cooldown or auto-activate is OFF → skip immediately (no delay)
- Aura and Channeled skills that are already active are skipped

The delay only fires **after a successful fire**, not on skips. This preserves full DPS when skills are on different cooldowns — the delay only affects simultaneous or near-simultaneous fires.

**Emergent design effect:** slot order becomes a soft priority system for auto-activate builds. Slot 1 fires before slot 2 when both are ready simultaneously. Meaningless for manual play — only relevant when auto-activate is on for multiple slots.

**Minimum delay:** TBD — somewhere between 50ms (barely noticeable, just prevents same-frame fires) and 200ms (creates a visible rhythmic stagger). Does not need to match animation duration — the animation issue (only 1 OneShot plays at a time) is acceptable as-is.

**Why this was set aside:** introducing any delay makes auto-activate strictly worse DPS than manual mashing. Auto-activate is designed to be equivalent to manual ("the honest version of holding all buttons") — a DPS penalty undermines that. The simultaneous-fire scenarios this was solving (triple Strike etc.) are already handled acceptably: OneShot guard protects slot 1's animation, HitMelee fires VFX for all hits, and in practice Focus costs / range / different cooldowns mean true simultaneous fires are rare. Revisit only if simultaneous firing genuinely feels bad in playtesting.

---

## Auto-Activate Stagger

*Context: discussed when all slotted skills fired simultaneously the moment any enemy entered range. Current behaviour (all slots fire at once) is intentional for v1 — simple and predictable.*

Each slot could track its own independent cooldown timer, with timers starting on first fire (not at run start). On first enemy contact, slots fire in sequence offset by `cooldown ÷ slots` (slot 1 at 0s, slot 2 at offset, slot 3 at 2× offset). After that first volley each slot runs independently for the rest of the run.

Benefit: eliminates the burst-all-at-once opening volley; makes duplicate skills (e.g. 3× Strike) feel like a rapid-fire build rather than a burst. Timers starting on first fire (not at run start) means no "burst debt" if enemies appear late.

Worth revisiting if simultaneous firing ever feels bad to play.

---

## Dedicated Dash + Movement Skills (v2)

*Context: v1 is WASD only. A dedicated non-slottable dash ships in v2 as a baseline for all characters.*

Every character gets a free dash — not in a skill slot, always available. This guarantees a movement floor without forcing players to sacrifice a damage slot. Also gives designers freedom to build encounters knowing every player can reposition.

Additional movement-type skills (slottable, with tradeoffs) are possible post-dash — e.g. a blink that deals damage on landing, or a leap that knocks enemies aside. These would occupy skill slots and cost Focus, unlike the free dash.

---

## Summon / Totem Skills (v2)

*Context: identified as a major skill category worth having, deferred to v2 due to system complexity.*

One activation places an autonomous entity that fights independently until it expires or dies. Flat Focus cost on placement, no ongoing drain.

Two variants to design when ready:
- **Totem** — timed duration, attacks nearest enemy, then expires. Max 1-2 active at once; placing a new one removes the oldest.
- **Minion** — persistent until killed, no timer. Necromancer/commander fantasy.

System requirement: autonomous entities need their own targeting, attack loop, and lifetime management — more complex than any v1 skill type. The design value is the player-as-commander paradigm, completely different from direct-attack skills.

---

## Weapon Bonus (Post-v1 Expansion)

*Context: discussed when designing the weapon-adaptive skill system. Cut from v1 to keep things simple.*

Each weapon has a single passive bonus that applies to compatible skills. Examples: a bow adds an extra projectile to all `Ranged` skills; a sword adds a bleed chance to all `Melee` skills. The bonus only applies if the skill's delivery tag matches the weapon type.

This extends weapon identity beyond range — two bows of the same tier could have different bonuses, making weapon choice within a type meaningful. Pairs naturally with the tag and range system already in place.

Worth revisiting when weapon depth is being expanded.

---
