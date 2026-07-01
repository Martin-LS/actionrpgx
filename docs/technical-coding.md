# Technical: Coding Conventions

Reference for adding data-driven content — items, recipes, skills, augments — to the C# gameplay systems. Read this before touching a registry. See CLAUDE.md Rule 6 for the multi-agent issue workflow this doc supports.

## Registry pattern
Every content type lives in a `public static class` named `*Registry` holding a `Dictionary<string, T>` literal (`All` or `_all` + `Get`), plus LINQ filter helpers (`ForSlot`, `ForType`) over `.Values`. No shared base interface — each registry is hand-written but structurally identical. New entries are added as new dictionary keys, not new classes.

## ID naming
All ids are lowercase `snake_case`.
- Items: `{descriptor}_t{tier}` — e.g. `sword_t1`, `heavy_hat_t1`.
- Recipes: `recipe_` + the output item id — e.g. `recipe_sword_t1`.
- Skills/augments: plain descriptive `snake_case`, no prefix, no tier — e.g. `entity_burst`, `retaliation`.

## Balance values
Never inline numeric gameplay values in a registry. All stats come from `BalanceConfig.cs` — one nested `static class` per category (`Weapons`, `Armour`, `Accessories`, `Skills`, …) of `public const`. Add new constants there, then reference them (`BalanceConfig.Armour.HeavyBonusHp`) from the registry entry. Exception: crafting costs are currently inlined literals (not yet balance-tracked) — flag this if adding new recipe costs.

## C# style
File-scoped namespaces (`namespace ActionRpgX.Items;`), one type per file, filename matches type. Data types are positional `record`s with named/defaulted params (`init`-only for array properties). Static-lookup classes are `static class`. Explicit access modifiers everywhere.

## Adding a new gear item — required touch points
Adding to `ItemRegistry` alone is not enough. To be craftable and usable end-to-end, an item needs:
1. `src/items/ItemRegistry.cs` — the definition.
2. `src/crafting/RecipeRegistry.cs` — a matching recipe (`recipe_<id>`) or it can't be crafted.
3. `src/character/CharacterManager.cs` — only if it should be default starting equipment or needs an icon/model alias.
4. `src/player/PlayerController.cs` — only if it's a weapon needing a model-path mapping.

There is no auto-wiring; each touch point is a manual, separate edit. Issues should say explicitly which of 3–4 apply.

## Testing
**No automated test suite currently exists in this repo.** Do not assume GdUnit4 or any other test conventions exist. If an issue requires tests, treat it as new infrastructure and say so explicitly in the issue.

## PR checklist
Copy into the PR description:
- [ ] Registry entry added (state which registry/registries)
- [ ] Recipe added, if the item should be craftable
- [ ] Character/Player wiring added, if the issue called for default equipment or a model mapping
- [ ] No numeric values inlined outside `BalanceConfig` (except crafting costs, per current convention)
- [ ] Confirmed no other touch points were needed beyond what the issue listed
