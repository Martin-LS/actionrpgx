namespace Godot2.Skills;

public record SkillAugmentData(string Id, string Name, string[] RequiredTags, string? EotId = null, string? ConflictGroup = null);
