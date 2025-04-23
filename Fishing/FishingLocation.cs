using SC = System.Collections;
using CG = System.Collections.Generic;
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
        // will be black instead for Abyss and DV
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
                yield return new UE.WaitForSeconds(3);
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
    private const float SitRegionElevation = 0.7f;

    public static readonly CG.List<FishingLocation> Locations = new()
    {
        new()
        {
            name = "Fishing_Spot-Lake_of_Unn",
            sceneName = IC.SceneNames.Fungus1_26,
            MarkerX = 58.9f,
            MarkerY = 15.4f + SitRegionElevation,
            ShinySourceX = 55.5f,
            ShinySourceY = 11.28f,
            Direction = FacingDirection.Left,
        },
        new()
        {
            name = "Fishing_Spot-Distant_Village",
            sceneName = IC.SceneNames.Deepnest_10,
            MarkerX = 58.8f,
            MarkerY = 10.4f + SitRegionElevation,
            ShinySourceX = 55.1f,
            ShinySourceY = 4.73f,
            Direction = FacingDirection.Left,
        },
        new()
        {
            name = "Fishing_Spot-West_Lake_Shore",
            sceneName = IC.SceneNames.Crossroads_50,
            MarkerX = 25.4f,
            MarkerY = 24.4f + SitRegionElevation,
            ShinySourceX = 28.1f,
            ShinySourceY = 23.09f,
            Direction = FacingDirection.Right,
        },
        new()
        {
            name = "Fishing_Spot-East_Lake_Shore",
            sceneName = IC.SceneNames.Crossroads_50,
            MarkerX = 225.9f,
            MarkerY = 25.4f + SitRegionElevation,
            ShinySourceX = 223.9f,
            ShinySourceY = 23.09f,
            Direction = FacingDirection.Left,
        },
        new()
        {
            name = "Fishing_Spot-Waterways_Acid_Sluice",
            sceneName = IC.SceneNames.Waterways_06,
            MarkerX = 97.0f,
            MarkerY = 12.2f + SitRegionElevation,
            ShinySourceX = 99.7f,
            ShinySourceY = 8.23f,
            Direction = FacingDirection.Right,
        },
        new()
        {
            name = "Fishing_Spot-Waterways_Central_Pool",
            sceneName = IC.SceneNames.Waterways_04,
            MarkerX = 112.9f,
            MarkerY = 8.4f + SitRegionElevation,
            ShinySourceX = 111.3f,
            ShinySourceY = 5.89f,
            Direction = FacingDirection.Left,
        },
        new()
        {
            name = "Fishing_Spot-Waterways_Mask_Shard_Pool",
            sceneName = IC.SceneNames.Waterways_04b,
            MarkerX = 123.7f,
            MarkerY = 8.4f + SitRegionElevation,
            ShinySourceX = 121.3f,
            ShinySourceY = 5.84f,
            Direction = FacingDirection.Left,
        },
        new()
        {
            name = "Fishing_Spot-Waterways_Long_Acid_Pool",
            sceneName = IC.SceneNames.Waterways_14,
            MarkerX = 73.1f,
            MarkerY = 20.4f + SitRegionElevation,
            ShinySourceX = 75.4f,
            ShinySourceY = 17.71f,
            Direction = FacingDirection.Right,
        },
        new()
        {
            name = "Fishing_Spot-Abyss_Lighthouse",
            sceneName = IC.SceneNames.Abyss_09,
            MarkerX = 117.6f,
            MarkerY = 22.4f + SitRegionElevation,
            ShinySourceX = 120.0f,
            ShinySourceY = 18.25f, // must be a little bit above water
            Direction = FacingDirection.Right,
        },
        new()
        {
            name = "Fishing_Spot-Godhome_Atrium",
            sceneName = IC.SceneNames.GG_Atrium,
            MarkerX = 142.4f,
            MarkerY = 14.4f + SitRegionElevation,
            ShinySourceX = 145.9f,
            ShinySourceY = 12.25f,
            Direction = FacingDirection.Right,
        }
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