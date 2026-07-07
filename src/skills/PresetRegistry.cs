using System;
using System.Collections.Generic;

namespace ActionRpgX.Skills;

public static class PresetRegistry
{
    public static readonly Dictionary<string, PresetData> All = new()
    {
        // entity_burst presets
        ["flurry"] = new PresetData(
            Id: "flurry",
            Name: "Flurry",
            PrototypeId: "entity_burst",
            FormId: "salvo",
            IdentityId: "physical",
            IconPath: "res://assets/icons/items/battle_axe.png"
        ),
        ["arcane_flurry"] = new PresetData(
            Id: "arcane_flurry",
            Name: "Arcane Flurry",
            PrototypeId: "entity_burst",
            FormId: "salvo",
            IdentityId: "fire",
            IconPath: "res://assets/icons/items/battle_axe.png"
        ),
        ["strike"] = new PresetData(
            Id: "strike",
            Name: "Strike",
            PrototypeId: "entity_burst",
            FormId: "swift",
            IdentityId: "physical",
            IconPath: "res://assets/icons/items/battle_axe.png"
        ),
        ["arcane_strike"] = new PresetData(
            Id: "arcane_strike",
            Name: "Arcane Strike",
            PrototypeId: "entity_burst",
            FormId: "swift",
            IdentityId: "fire",
            IconPath: "res://assets/icons/items/battle_axe.png"
        ),
        ["crushing_blow"] = new PresetData(
            Id: "crushing_blow",
            Name: "Crushing Blow",
            PrototypeId: "entity_burst",
            FormId: "heavy",
            IdentityId: "physical",
            IconPath: "res://assets/icons/items/battle_axe.png"
        ),
        ["smite"] = new PresetData(
            Id: "smite",
            Name: "Smite",
            PrototypeId: "entity_burst",
            FormId: "heavy",
            IdentityId: "fire",
            IconPath: "res://assets/icons/items/battle_axe.png"
        ),

        // self_burst presets
        ["shockwave"] = new PresetData(
            Id: "shockwave",
            Name: "Shockwave",
            PrototypeId: "self_burst",
            FormId: "nova",
            IdentityId: "physical"
        ),
        ["nova"] = new PresetData(
            Id: "nova",
            Name: "Nova",
            PrototypeId: "self_burst",
            FormId: "nova",
            IdentityId: "fire"
        ),
        ["quake"] = new PresetData(
            Id: "quake",
            Name: "Quake",
            PrototypeId: "self_burst",
            FormId: "quake",
            IdentityId: "physical"
        ),
        ["cataclysm"] = new PresetData(
            Id: "cataclysm",
            Name: "Cataclysm",
            PrototypeId: "self_burst",
            FormId: "quake",
            IdentityId: "fire"
        ),

        // stackable_zone presets
        ["spike_field"] = new PresetData(
            Id: "spike_field",
            Name: "Spike Field",
            PrototypeId: "stackable_zone",
            FormId: "swarm",
            IdentityId: "physical"
        ),
        ["arcane_saturation"] = new PresetData(
            Id: "arcane_saturation",
            Name: "Arcane Saturation",
            PrototypeId: "stackable_zone",
            FormId: "swarm",
            IdentityId: "fire"
        ),
        ["barbed_zone"] = new PresetData(
            Id: "barbed_zone",
            Name: "Barbed Zone",
            PrototypeId: "stackable_zone",
            FormId: "singular",
            IdentityId: "physical"
        ),
        ["arcane_nexus"] = new PresetData(
            Id: "arcane_nexus",
            Name: "Arcane Nexus",
            PrototypeId: "stackable_zone",
            FormId: "singular",
            IdentityId: "fire"
        ),

        // triggered_zone_burst presets
        ["spike_trap"] = new PresetData(
            Id: "spike_trap",
            Name: "Spike Trap",
            PrototypeId: "triggered_zone_burst",
            FormId: "trap_swarm",
            IdentityId: "physical"
        ),
        ["runic_mine"] = new PresetData(
            Id: "runic_mine",
            Name: "Runic Mine",
            PrototypeId: "triggered_zone_burst",
            FormId: "trap_swarm",
            IdentityId: "fire"
        ),
        ["claymore"] = new PresetData(
            Id: "claymore",
            Name: "Claymore",
            PrototypeId: "triggered_zone_burst",
            FormId: "trap_singular",
            IdentityId: "physical"
        ),
        ["blast_sigil"] = new PresetData(
            Id: "blast_sigil",
            Name: "Blast Sigil",
            PrototypeId: "triggered_zone_burst",
            FormId: "trap_singular",
            IdentityId: "fire"
        ),

        // fixed_zone_burst presets
        ["blast"] = new PresetData(
            Id: "blast",
            Name: "Blast",
            PrototypeId: "fixed_zone_burst",
            FormId: "blast",
            IdentityId: "physical"
        ),
        ["arcane_blast"] = new PresetData(
            Id: "arcane_blast",
            Name: "Arcane Blast",
            PrototypeId: "fixed_zone_burst",
            FormId: "blast",
            IdentityId: "fire"
        ),
        ["mortar"] = new PresetData(
            Id: "mortar",
            Name: "Mortar",
            PrototypeId: "fixed_zone_burst",
            FormId: "fuse",
            IdentityId: "physical"
        ),
        ["arcane_mortar"] = new PresetData(
            Id: "arcane_mortar",
            Name: "Arcane Mortar",
            PrototypeId: "fixed_zone_burst",
            FormId: "fuse",
            IdentityId: "fire"
        ),
        ["aftershock"] = new PresetData(
            Id: "aftershock",
            Name: "Aftershock",
            PrototypeId: "fixed_zone_burst",
            FormId: "echo",
            IdentityId: "physical"
        ),
        ["arcane_aftershock"] = new PresetData(
            Id: "arcane_aftershock",
            Name: "Arcane Aftershock",
            PrototypeId: "fixed_zone_burst",
            FormId: "echo",
            IdentityId: "fire"
        ),

        // fixed_zone_tick presets
        ["rockfall"] = new PresetData(
            Id: "rockfall",
            Name: "Rockfall",
            PrototypeId: "fixed_zone_tick",
            FormId: "storm",
            IdentityId: "physical"
        ),
        ["tempest"] = new PresetData(
            Id: "tempest",
            Name: "Tempest",
            PrototypeId: "fixed_zone_tick",
            FormId: "storm",
            IdentityId: "fire"
        ),
        ["caltrops"] = new PresetData(
            Id: "caltrops",
            Name: "Caltrops",
            PrototypeId: "fixed_zone_tick",
            FormId: "floor",
            IdentityId: "physical"
        ),
        ["glyph_of_agony"] = new PresetData(
            Id: "glyph_of_agony",
            Name: "Glyph of Agony",
            PrototypeId: "fixed_zone_tick",
            FormId: "floor",
            IdentityId: "fire"
        ),

        // self_channeled_tick presets
        ["surge"] = new PresetData(
            Id: "surge",
            Name: "Surge",
            PrototypeId: "self_channeled_tick",
            FormId: "ramp",
            IdentityId: "physical"
        ),
        ["kindle"] = new PresetData(
            Id: "kindle",
            Name: "Kindle",
            PrototypeId: "self_channeled_tick",
            FormId: "ramp",
            IdentityId: "fire"
        ),
        ["cyclone"] = new PresetData(
            Id: "cyclone",
            Name: "Cyclone",
            PrototypeId: "self_channeled_tick",
            FormId: "spin",
            IdentityId: "physical"
        ),
        ["arcane_cyclone"] = new PresetData(
            Id: "arcane_cyclone",
            Name: "Arcane Cyclone",
            PrototypeId: "self_channeled_tick",
            FormId: "spin",
            IdentityId: "fire"
        ),
        ["bladestorm"] = new PresetData(
            Id: "bladestorm",
            Name: "Bladestorm",
            PrototypeId: "self_channeled_tick",
            FormId: "vortex",
            IdentityId: "physical"
        ),
        ["maelstrom"] = new PresetData(
            Id: "maelstrom",
            Name: "Maelstrom",
            PrototypeId: "self_channeled_tick",
            FormId: "vortex",
            IdentityId: "fire"
        ),

        // self_aura presets
        ["steel_wall"] = new PresetData(
            Id: "steel_wall",
            Name: "Steel Wall",
            PrototypeId: "self_aura",
            FormId: "reserve_heavy",
            IdentityId: "physical"
        ),
        ["arcane_singularity"] = new PresetData(
            Id: "arcane_singularity",
            Name: "Arcane Singularity",
            PrototypeId: "self_aura",
            FormId: "reserve_heavy",
            IdentityId: "fire"
        ),
        ["razor_aura"] = new PresetData(
            Id: "razor_aura",
            Name: "Razor Aura",
            PrototypeId: "self_aura",
            FormId: "reserve_light",
            IdentityId: "physical"
        ),
        ["static_field"] = new PresetData(
            Id: "static_field",
            Name: "Static Field",
            PrototypeId: "self_aura",
            FormId: "reserve_light",
            IdentityId: "fire"
        ),
        // entity_debuff (fleet)
        ["fleet_frailty"] = new PresetData(
            Id: "fleet_frailty",
            Name: "Fleet Frailty",
            PrototypeId: "entity_debuff",
            FormId: "fleet",
            IdentityId: "physical"
        ),
        ["scorching_delay"] = new PresetData(
            Id: "scorching_delay",
            Name: "Scorching Delay",
            PrototypeId: "entity_debuff",
            FormId: "fleet",
            IdentityId: "fire"
        ),
        ["flash_frost"] = new PresetData(
            Id: "flash_frost",
            Name: "Flash Frost",
            PrototypeId: "entity_debuff",
            FormId: "fleet",
            IdentityId: "cold"
        ),
        ["static_stutter"] = new PresetData(
            Id: "static_stutter",
            Name: "Static Stutter",
            PrototypeId: "entity_debuff",
            FormId: "fleet",
            IdentityId: "lightning"
        ),
        // entity_debuff (enduring)
        ["enduring_curse"] = new PresetData(
            Id: "enduring_curse",
            Name: "Enduring Curse",
            PrototypeId: "entity_debuff",
            FormId: "enduring",
            IdentityId: "physical"
        ),
        ["lingering_ash"] = new PresetData(
            Id: "lingering_ash",
            Name: "Lingering Ash",
            PrototypeId: "entity_debuff",
            FormId: "enduring",
            IdentityId: "fire"
        ),
        ["permafrost"] = new PresetData(
            Id: "permafrost",
            Name: "Permafrost",
            PrototypeId: "entity_debuff",
            FormId: "enduring",
            IdentityId: "cold"
        ),
        ["staggering_shock"] = new PresetData(
            Id: "staggering_shock",
            Name: "Staggering Shock",
            PrototypeId: "entity_debuff",
            FormId: "enduring",
            IdentityId: "lightning"
        )
    };

    public static PresetData? Get(string id) => All.TryGetValue(id, out var p) ? p : null;

    public static IEnumerable<PresetData> GetAll() => All.Values;

    public static FormData? FormFor(string skillId)
    {
        if (All.TryGetValue(skillId, out var preset))
        {
            return FormRegistry.Get(preset.FormId);
        }
        return null;
    }
}
