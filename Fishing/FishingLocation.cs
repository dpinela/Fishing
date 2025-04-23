using SC = System.Collections;
using IC = ItemChanger;
using UE = UnityEngine;
using PM = HutongGames.PlayMaker;
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
    public FacingDirection Direction;

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
        obj.transform.localScale = new UE.Vector3(Direction == FacingDirection.Right ? 1 : -1, 1, obj.transform.localScale.z);
        obj.SetActive(true);
        var fsm = obj.LocateMyFSM("Shop Region");
        fsm.GetFsmFloat("Move To X").Value = MarkerX;
        fsm.GetFsmBool("Face Hero Right").Value = Direction == FacingDirection.Right;
        var arrow = fsm.GetFsmGameObject("Prompt");
        fsm.GetState("Init").AppendAction(() =>
        {
            var prompts = FindImmediateChild(arrow.Value, "Labels");
            var sit = FindImmediateChild(prompts, "Sit");
            UE.Object.Destroy(sit.GetComponent<SetTextMeshProGameText>());
            var text = sit.GetComponent<TMP.TextMeshPro>();
            text.text = "FISH";
        });

        var swr = UE.GameObject.Find("Surface Water Region").LocateMyFSM("Surface Water Region");
        var eff = swr.GetState("Splash Out effects");
        var blue = swr.GetState("Blue");
        var dripPrefab = ((PM.Actions.SetGameObject)blue.Actions[3]).gameObject.Value;
        var splashPrefab = ((PM.Actions.SetGameObject)blue.Actions[1]).gameObject.Value;
        var splashAudio = (UE.AudioClip)((PM.Actions.AudioPlayerOneShotSingle)eff.Actions[1]).audioClip.Value;

        var firstUncaughtItem = 0;
        var flingDirection = Direction == FacingDirection.Right ? IC.ShinyFling.Left : IC.ShinyFling.Right;
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
                var shinyPos = new UE.Vector3(ShinySourceX, ShinySourceY, s.transform.position.z);
                s.transform.position = shinyPos;
                IC.Util.ShinyUtility.SetShinyFling(s.LocateMyFSM("Shiny Control"), flingDirection);
                s.SetActive(true);
                firstUncaughtItem = i + 1;

                dripPrefab.Spawn(shinyPos, UE.Quaternion.identity);
                splashPrefab.Spawn(shinyPos, UE.Quaternion.identity);
                IC.Internal.SoundManager.PlayClipAtPoint(splashAudio, shinyPos);
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

    // how high a fishing spot should be above the Knight's position on the ground
    // MarkerX should be about 1.5 units from the shore
    private const float SitRegionElevation = 0.7f;

    public static readonly FishingLocation LakeOfUnn = new()
    {
        name = "Fishing Spot-Lake of Unn",
        sceneName = IC.SceneNames.Fungus1_26,
        MarkerX = 58.9f,
        MarkerY = 16.1f,
        ShinySourceX = 55.5f,
        ShinySourceY = 11.28f,
        Direction = FacingDirection.Left,
    };
    // major pools:
    // - Lake of Unn
    // - Distant Village
    // - Blue Lake (east/west)
    // - Acid pool west of Waterways spike tunnel
    // - East side of both Waterways long pools
    // - Long acid pool above Isma's Grove
    // - Abyss east of the Lighthouse
    // - Godhome
    // minor pools:
    // - QGA (east/west)
    // - King's Station
    // - Mantis Village (east/west)
}

internal enum FacingDirection
{
    Left,
    Right
}