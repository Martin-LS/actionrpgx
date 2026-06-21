namespace ActionRpgX.Skills;

public enum SkillKind
{
    Normal,      // Real named skill (v2+)
    Prototype,   // Player-facing in v1; hidden in v2 when real named versions replace it
    EngineProof, // Never player-facing or craftable — exists solely to validate engine mechanics
}
