using System.Collections.Generic;
using System.Linq;

namespace ActionRpgX.Stats;

public class StatBlock
{
    private readonly Dictionary<StatId, float> _base      = new();
    private readonly List<StatModifier>         _modifiers = new();

    public void SetBase(StatId stat, float value) => _base[stat] = value;

    public void AddModifier(StatModifier mod) => _modifiers.Add(mod);

    public void RemoveModifiersFromSource(ModifierSource source, string? sourceId = null) =>
        _modifiers.RemoveAll(m => m.Source == source && (sourceId == null || m.SourceId == sourceId));

    public float Get(StatId stat)
    {
        float baseVal    = _base.GetValueOrDefault(stat, 0f);
        var   relevant   = _modifiers.Where(m => m.Stat == stat).ToList();
        float flatSum    = relevant.Where(m => m.Type == ModifierType.FlatAdd)   .Sum(m => m.Value);
        float percentSum = relevant.Where(m => m.Type == ModifierType.PercentAdd).Sum(m => m.Value);
        float multiProd  = relevant.Where(m => m.Type == ModifierType.Multiply)  .Aggregate(1f, (acc, m) => acc * m.Value);
        return (baseVal + flatSum) * (1f + percentSum) * multiProd;
    }
}
