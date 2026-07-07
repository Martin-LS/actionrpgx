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
            VfxKey: "kinetic",       // physical = steel-grey particle skins (kinetic)
            SignatureEotId: "bleed"
        ),
        ["fire"] = new IdentityData(
            Id: "fire",
            DamageType: DamageType.Fire,
            VfxKey: "fire",          // fire = ember/flame particle skins
            SignatureEotId: "burn"
        ),
        ["cold"] = new IdentityData(
            Id: "cold",
            DamageType: DamageType.Cold,
            VfxKey: "cold",          // cold = frost/ice particle skins
            SignatureEotId: "chill"
        ),
        ["lightning"] = new IdentityData(
            Id: "lightning",
            DamageType: DamageType.Lightning,
            VfxKey: "lightning",     // lightning = arc/spark particle skins
            SignatureEotId: "shock"
        ),
        ["poison"] = new IdentityData(
            Id: "poison",
            DamageType: DamageType.Poison,
            VfxKey: "poison",        // poison = toxic/venom particle skins
            SignatureEotId: "poison"
        ),
        ["void"] = new IdentityData(
            Id: "void",
            DamageType: DamageType.Void,
            VfxKey: "void",          // void = shadow/decay particle skins
            SignatureEotId: "decay"
        )
    };

    public static IdentityData? Get(string id) => All.TryGetValue(id, out var i) ? i : null;

    public static IEnumerable<IdentityData> GetAll() => All.Values;
}
