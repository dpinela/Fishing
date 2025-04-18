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
        Log("Placing fishing spot");
        var obj = UE.Object.Instantiate(fishingSpotPrefab!);
        obj.transform.position = new UE.Vector3(60.9f, 15.6f, obj.transform.position.z);
        var fsm = obj.LocateMyFSM("Shop Region");
        fsm.GetFsmFloat("Move To X").Value = 60.9f;
        obj.SetActive(true);
        PrintObjHierarchy("", obj);
    }

    private void PrintObjHierarchy(string prefix, UE.GameObject obj)
    {
        Log(prefix + obj.name + ": " + obj.activeSelf);
        var extendedPrefix = "  " + prefix;
        foreach (var c in obj.GetComponents<UE.MonoBehaviour>()) {
            Log(extendedPrefix + "Component " + c.name + " " + c.GetType().Name);
        }
        foreach (UE.Transform t in obj.transform)
        {
            PrintObjHierarchy(extendedPrefix, t.gameObject);
        }
    }

    public override string GetVersion() => "1.0";
}

