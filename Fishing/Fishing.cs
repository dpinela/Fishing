using MAPI = Modding;
using static Modding.Utils.UnityExtensions;
using UE = UnityEngine;
using CG = System.Collections.Generic;
using IC = ItemChanger;

namespace Fishing;

public class Fishing : MAPI.Mod
{
    private static Fishing? Instance;

    internal static void LogInfo(string msg)
    {
        Instance!.Log(msg);
    }

    private const string SitTemplateName = "Ghost NPC/Dreamnail Hit/Sit Region";

    public override CG.List<(string, string)> GetPreloadNames() => new()
    {
        (IC.SceneNames.Ruins_Bathhouse, SitTemplateName)
    };

    public override void Initialize(CG.Dictionary<string, CG.Dictionary<string, UE.GameObject>> preloads)
    {
        Instance = this;

        FishingLocation.FishingSpotPrefab = preloads[IC.SceneNames.Ruins_Bathhouse][SitTemplateName];

        IC.Finder.DefineCustomLocation(FishingLocation.LakeOfUnn);

        On.UIManager.StartNewGame += PlaceFishingSpots;
    }

    private void PlaceFishingSpots(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        try
        {
            IC.ItemChangerMod.CreateSettingsProfile(overwrite: false, createDefaultModules: false);
            var p = IC.Finder.GetLocation("Fishing Spot-Lake of Unn")!
                .Wrap()
                .Add(IC.Finder.GetItem("Rancid_Egg")!)
                .Add(IC.Finder.GetItem("Grub")!)
                .Add(IC.Finder.GetItem("Mantis_Claw")!)
                .Add(IC.Finder.GetItem("Grub")!)
                .Add(IC.Finder.GetItem("Lumafly_Escape")!)
                .Add(IC.Finder.GetItem("Mimic_Grub")!);
            IC.ItemChangerMod.AddPlacements(new CG.List<IC.AbstractPlacement> {p});
        }
        catch (System.Exception err)
        {
            LogError($"error initializing fishing spots: {err}");
        }
        orig(self, permaDeath, bossRush);
    }

    public override string GetVersion() => "1.0";
}

