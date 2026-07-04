using System;
using System.Collections.Generic;
using ActionRpgX.Items;

namespace ActionRpgX.Skills;

public static class IdentityRegistry
{
    private static readonly Dictionary<string, IdentityData> All = new()
    {
        ["physical"] = new IdentityData(
            Id: "physical",
            DamageType: DamageType.Physical,
            VfxKey: ""
        ),
        ["magic"] = new IdentityData(
            Id: "magic",
            DamageType: DamageType.Magic,
            VfxKey: ""
        )
    };

    public static IdentityData? Get(string id) => All.TryGetValue(id, out var i) ? i : null;

    public static IEnumerable<IdentityData> GetAll() => All.Values;
}
