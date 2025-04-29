using PM = HutongGames.PlayMaker;
using static System.Linq.Enumerable;

namespace Fishing;

internal static class PlayMakerExtensions
{
    internal static PM.FsmFloat GetFsmFloat(this PlayMakerFSM fsm, string name)
    {
        return fsm.FsmVariables.FloatVariables.FirstOrDefault(v => v.Name == name);
    }

    internal static PM.FsmBool GetFsmBool(this PlayMakerFSM fsm, string name)
    {
        return fsm.FsmVariables.BoolVariables.FirstOrDefault(v => v.Name == name);
    }

    internal static PM.FsmGameObject GetFsmGameObject(this PlayMakerFSM fsm, string name)
    {
        return fsm.FsmVariables.GameObjectVariables.FirstOrDefault(v => v.Name == name);
    }

    internal static PM.FsmState GetState(this PlayMakerFSM fsm, string name)
    {
        return fsm.FsmStates.FirstOrDefault(s => s.Name == name);
    }

    internal static T FindAction<T>(this PM.FsmState s, System.Func<T, bool> pred)
    where T : PM.FsmStateAction
    {
        return s.Actions.OfType<T>().First(pred);
    }

    internal static void RemoveAction<T>(this PM.FsmState s)
    where T : PM.FsmStateAction
    {
        var i = System.Array.FindIndex(s.Actions, a => a is T);
        var actions = new PM.FsmStateAction[s.Actions.Length - 1];
        System.Array.Copy(s.Actions, actions, i);
        System.Array.Copy(s.Actions, i + 1, actions, i, s.Actions.Length - i - 1);
        s.Actions = actions;
    }

    internal static void AppendAction(this PM.FsmState s, System.Action a)
    {
        SpliceAction(s, s.Actions.Length, a);
    }

    internal static void SpliceAction(this PM.FsmState s, int pos, System.Action a)
    {
        var actions = new PM.FsmStateAction[s.Actions.Length + 1];
        System.Array.Copy(s.Actions, actions, pos);
        var fa = new FuncAction(a);
        fa.Init(s);
        actions[pos] = fa;
        System.Array.Copy(s.Actions, pos, actions, pos + 1, s.Actions.Length - pos);
        s.Actions = actions;
    }
}