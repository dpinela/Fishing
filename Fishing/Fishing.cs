using MAPI = Modding;
using static Modding.Utils.UnityExtensions;
using CG = System.Collections.Generic;
using UE = UnityEngine;
using USM = UnityEngine.SceneManagement;
using IC = ItemChanger;
using TMP = TMPro;

namespace Fishing;

public class Fishing : MAPI.Mod
{
    private const string SitTemplateName = "Ghost NPC/Dreamnail Hit/Sit Region";

    public override CG.List<(string, string)> GetPreloadNames() => new()
    {
        (IC.SceneNames.Ruins_Bathhouse, SitTemplateName)
    };

    private UE.GameObject? fishingSpotPrefab = null;

    public override void Initialize(CG.Dictionary<string, CG.Dictionary<string, UE.GameObject>> preloads)
    {
        fishingSpotPrefab = preloads[IC.SceneNames.Ruins_Bathhouse][SitTemplateName];

        IC.Events.OnSceneChange += PlaceFishingSpots;
    }

    private void PlaceFishingSpots(USM.Scene scene)
    {
        if (scene.name != IC.SceneNames.Fungus1_26)
        {
            return;
        }
        var obj = UE.Object.Instantiate(fishingSpotPrefab!);
        obj.transform.position = new UE.Vector3(58.1f, 15.6f, obj.transform.position.z);
        var fsm = obj.LocateMyFSM("Shop Region");
        fsm.GetFsmFloat("Move To X").Value = 58.1f;
        var arrow = fsm.GetFsmGameObject("Prompt");
        fsm.GetState("Init").AppendAction(() =>
        {
            var prompts = FindImmediateChild(arrow.Value, "Labels");
            var sit = FindImmediateChild(prompts, "Sit");
            UE.Object.Destroy(sit.GetComponent<SetTextMeshProGameText>());
            var text = sit.GetComponent<TMP.TextMeshPro>();
            text.text = "FISH";
        });
        fsm.GetState("Sit").AppendAction(() =>
        {
            var loc = new IC.Locations.CoordinateLocation()
            {
                name = "Fishing Spot #1",
                sceneName = IC.SceneNames.Fungus1_26,
                flingType = IC.FlingType.Everywhere,
                forceShiny = true,
                x = 0,
                y = 0,
                elevation = 0,
            };
            loc.AddTag<IC.Tags.ShinyFlingTag>().fling = IC.ShinyFling.Right;
            var p = new IC.Placements.MutablePlacement("Fishing Spot #1")
            {
                Location = loc,
                containerType = IC.Container.Shiny,
            };
            p.Items.Add(IC.Finder.GetItem("Rancid_Egg")!);
            var ci = new IC.ContainerInfo(IC.Container.Shiny, p, IC.FlingType.Everywhere);
            var s = IC.Util.ShinyUtility.MakeNewMultiItemShiny(ci);
            s.transform.position = new UE.Vector3(56.1f, 12.6f, s.transform.position.z);
            s.SetActive(true);
        });
        obj.SetActive(true);
    }

    private UE.GameObject FindImmediateChild(UE.GameObject parent, string childName)
    {
        var transform = parent.transform;
        for (var i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i).gameObject;
            if (c.name == childName)
            {
                return c;
            }
        }
        throw new System.InvalidOperationException($"GO {parent.name} has no child named {childName}");
    }

    public override string GetVersion() => "1.0";
}

