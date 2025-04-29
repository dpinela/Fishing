using RSMVersioning = RandoSettingsManager.SettingsManagement.Versioning;
using CG = System.Collections.Generic;
using static System.Linq.Enumerable;

namespace Fishing;

internal class StructuralVersioningPolicy : RSMVersioning.VersioningPolicy<Signature>
{
    internal required System.Func<ModSettings> settingsGetter;

    public override Signature Version => new() { FeatureSet = FeatureSetForSettings(settingsGetter()) };

    private static CG.List<string> FeatureSetForSettings(ModSettings rs) =>
        SupportedFeatures.Where(f => f.feature(rs)).Select(f => f.name).ToList();

    public override bool Allow(Signature s) => s.FeatureSet.All(name => SupportedFeatures.Any(sf => sf.name == name));

    private static CG.List<(System.Predicate<ModSettings> feature, string name)> SupportedFeatures = new()
    {
        (rs => rs.Enabled, "MajorFishingSpots")
    };
}

internal struct Signature
{
    public CG.List<string> FeatureSet;
}
