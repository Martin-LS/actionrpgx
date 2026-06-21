using System;
using System.Collections.Generic;

namespace ActionRpgX.Skills;

public static class AugmentResolver
{
    public static List<SkillAugmentInstance> Resolve(
        IList<string> socketedAugmentIds,
        Func<string, SkillAugmentInstance?> lookup)
    {
        var active        = new List<SkillAugmentInstance>();
        var claimedGroups = new HashSet<string>();

        for (int i = socketedAugmentIds.Count - 1; i >= 0; i--)
        {
            var id   = socketedAugmentIds[i];
            if (string.IsNullOrEmpty(id)) continue;
            var inst = lookup(id);
            if (inst == null) continue;

            var group = inst.Definition?.ConflictGroup;
            if (group != null)
            {
                if (claimedGroups.Contains(group)) continue;
                claimedGroups.Add(group);
            }

            active.Add(inst);
        }

        return active;
    }
}
