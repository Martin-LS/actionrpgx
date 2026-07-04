namespace ActionRpgX.Skills;

// PresetData.cs — one entry per recipe-book skill
public record PresetData(
    string Id,          // the composed skill id: "strike", "smite", "rockfall", ...
    string Name,        // curated display name: "Strike", "Glyph of Agony", ...
    string PrototypeId,
    string FormId,
    string IdentityId,
    string IconPath = ""
);
