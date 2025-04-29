using RSM = RandoSettingsManager.SettingsManagement;
using RSMVersioning = RandoSettingsManager.SettingsManagement.Versioning;

namespace Fishing;

internal class RandoSettingsManagerProxy : RSM.RandoSettingsProxy<ModSettings, Signature>
{
    internal required System.Func<ModSettings> getter;
    internal required System.Action<ModSettings> setter;

    public override string ModKey => nameof(Fishing);

    public override RSMVersioning.VersioningPolicy<Signature> VersioningPolicy => new StructuralVersioningPolicy() { settingsGetter = this.getter };

    public override bool TryProvideSettings(out ModSettings? sent)
    {
        sent = getter();
        return sent.Enabled;
    }

    public override void ReceiveSettings(ModSettings? received)
    {
        setter(received ?? new());
    }
}
