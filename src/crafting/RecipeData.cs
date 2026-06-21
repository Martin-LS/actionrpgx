using System.Collections.Generic;

namespace Godot2.Crafting;

public record RecipeData(
    string                  Id,
    string                  OutputItemId,
    RecipeType              Type,
    Dictionary<string, int> MaterialCosts
);
