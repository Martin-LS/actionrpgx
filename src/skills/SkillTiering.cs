using System;

namespace ActionRpgX.Skills;

public static class SkillTiering
{
    public static SkillData Apply(SkillData skill, TierTrack track, int tier)
    {
        if (tier <= 1 || track == TierTrack.None)
            return skill;

        float step = BalanceConfig.Tiers.TrackStepPerTier;
        float factor = (tier - 1) * step;

        switch (track)
        {
            case TierTrack.CooldownDown:
                float newCooldown = Math.Max(0f, skill.Cooldown * (1f - factor));
                return skill with { Cooldown = newCooldown };

            case TierTrack.FocusCostDown:
                float newFocusCost = Math.Max(0f, skill.FocusCost * (1f - factor));
                return skill with { FocusCost = newFocusCost };

            case TierTrack.RadiusUp:
                if (skill.TargetingShape == SkillTargetingShape.Self)
                {
                    float newRange = skill.Range * (1f + factor);
                    return skill with { Range = newRange };
                }
                else
                {
                    float newZoneRadius = skill.ZoneRadius * (1f + factor);
                    return skill with { ZoneRadius = newZoneRadius };
                }

            case TierTrack.TickRateUp:
                if (skill.TickRate > 0f)
                {
                    float newTickRate = skill.TickRate / (1f + factor);
                    return skill with { TickRate = newTickRate };
                }
                return skill;

            case TierTrack.RampSpeedUp:
                float newRampSpeed = skill.RampSpeed * (1f + factor);
                return skill with { RampSpeed = newRampSpeed };

            case TierTrack.StackLimitUp:
                if (skill.StackLimit > 0)
                {
                    int newStackLimit = skill.StackLimit + (tier - 1);
                    return skill with { StackLimit = newStackLimit };
                }
                return skill;

            default:
                return skill;
        }
    }
}
