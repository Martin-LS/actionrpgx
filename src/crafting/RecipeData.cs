using System.Collections.Generic;

namespace ActionRpgX.Crafting;

public record RecipeData(
    string                  Id,
    string                  OutputItemId,
    RecipeType              Type,
    Dictionary<string, int> MaterialCosts
);
