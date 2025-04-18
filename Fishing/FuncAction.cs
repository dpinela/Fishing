using PM = HutongGames.PlayMaker;

namespace Fishing;

internal class FuncAction : PM.FsmStateAction
{
    private readonly System.Action _func;

    public FuncAction(System.Action func)
    {
        _func = func;
    }

    public override void OnEnter()
    {
        _func();
        Finish();
    }
}