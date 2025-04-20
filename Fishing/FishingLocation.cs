using SC = System.Collections;
using IC = ItemChanger;
using UE = UnityEngine;
using USM = UnityEngine.SceneManagement;
using TMP = TMPro;

namespace Fishing;

internal class FishingLocation : IC.Locations.AutoLocation
{
    public static UE.GameObject? FishingSpotPrefab;

    public float MarkerX;
    public float MarkerY;
    public float ShinySourceX;
    public float ShinySourceY;
    public IC.ShinyFling FlingDirection;

    protected override void OnLoad()
    {
        IC.Events.AddSceneChangeEdit(sceneName!, PlaceFishingSpot);
    }

    protected override void OnUnload()
    {
        IC.Events.RemoveSceneChangeEdit(sceneName!, PlaceFishingSpot);
    }

    private void PlaceFishingSpot(USM.Scene scene)
    {
        var obj = UE.Object.Instantiate(FishingSpotPrefab!);
        obj.transform.position = new UE.Vector3(MarkerX, MarkerY, obj.transform.position.z);
        obj.SetActive(true);
        var fsm = obj.LocateMyFSM("Shop Region");
        fsm.GetFsmFloat("Move To X").Value = MarkerX;
        var arrow = fsm.GetFsmGameObject("Prompt");
        fsm.GetState("Init").AppendAction(() =>
        {
            var prompts = FindImmediateChild(arrow.Value, "Labels");
            var sit = FindImmediateChild(prompts, "Sit");
            UE.Object.Destroy(sit.GetComponent<SetTextMeshProGameText>());
            var text = sit.GetComponent<TMP.TextMeshPro>();
            text.text = "FISH";
        });

        var firstUncaughtItem = 0;
        SC.IEnumerator Fish()
        {
            for (var i = firstUncaughtItem; i < Placement.Items.Count; i++)
            {
                if (Placement.Items[i].IsObtained())
                {
                    continue;
                }
                yield return new UE.WaitForSeconds(2);
                var s = IC.Util.ShinyUtility.MakeNewShiny(Placement, Placement.Items[i], IC.FlingType.StraightUp);
                s.transform.position = new UE.Vector3(ShinySourceX, ShinySourceY, s.transform.position.z);
                IC.Util.ShinyUtility.SetShinyFling(s.LocateMyFSM("Shiny Control"), FlingDirection);
                s.SetActive(true);
                firstUncaughtItem = i + 1;
            }
        };

        SC.IEnumerator? fishingCoroutine = null;

        fsm.GetState("Sit").AppendAction(() =>
        {
            if (fishingCoroutine != null)
            {
                fsm.StopCoroutine(fishingCoroutine);
            }
            fishingCoroutine = Fish();
            fsm.StartCoroutine(fishingCoroutine);
        });
        fsm.GetState("Rise").AppendAction(() =>
        {
            if (fishingCoroutine != null)
            {
                fsm.StopCoroutine(fishingCoroutine);
            }
        });
    }

    private static UE.GameObject FindImmediateChild(UE.GameObject parent, string childName)
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

    public override IC.AbstractPlacement Wrap() => new IC.Placements.AutoPlacement(name)
    {
        Location = this,
    };

    public static readonly FishingLocation LakeOfUnn = new()
    {
        name = "Fishing Spot-Lake of Unn",
        sceneName = IC.SceneNames.Fungus1_26,
        MarkerX = 58.1f,
        MarkerY = 15.6f,
        ShinySourceX = 56.1f,
        ShinySourceY = 11.6f,
        FlingDirection = IC.ShinyFling.Right,
    };
}