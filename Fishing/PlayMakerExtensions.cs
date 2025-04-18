using PM = HutongGames.PlayMaker;
using static System.Linq.Enumerable;

namespace Fishing;

internal static class PlayMakerExtensions
{
    internal static PM.FsmFloat GetFsmFloat(this PlayMakerFSM fsm, string name)
    {
        return fsm.FsmVariables.FloatVariables.FirstOrDefault(v => v.Name == name);
    }
}