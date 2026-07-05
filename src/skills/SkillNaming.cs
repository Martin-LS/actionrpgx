namespace ActionRpgX.Skills;

using System.Linq;

public static class SkillNaming
{
    public static string GenerateName(string protoId, string formId, string identityId)
    {
        var matchingPreset = PresetRegistry.All.Values.FirstOrDefault(p =>
            p.PrototypeId == protoId &&
            p.FormId == formId &&
            p.IdentityId == identityId
        );

        if (matchingPreset != null)
        {
            return matchingPreset.Name;
        }

        return $"[{protoId}][{formId}][{identityId}]";
    }
}
