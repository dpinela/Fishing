using IC = ItemChanger;
using UE = UnityEngine;
using PM = HutongGames.PlayMaker;
using SC = System.Collections;

namespace Fishing;

internal class FishingMinigame : UE.MonoBehaviour
{
    private System.Random rng = new();
    private InputHandler input = GameManager.instance.GetComponent<InputHandler>();
    private int firstUncaughtItem = 0;
    private float nextCatchOpportunity = -1f;
    private bool caught;
    private SC.IEnumerator? fishingCoroutine;

    private UE.GameObject dripPrefab = null!;
    private UE.GameObject splashPrefab = null!;
    private UE.AudioClip splashAudio = null!;

    public FishingLocation Location = null!;

    private const float minOpportunityInterval = 3;
    private const float maxOpportunityInterval = 7;
    private const float catchTolerance = 0.5f;

    public void Start()
    {
        var swr = UE.GameObject.Find(Location.WaterRegionName).LocateMyFSM("Surface Water Region");
        var eff = swr.GetState("Splash Out effects");
        // will be black instead for Abyss and DV
        var prefabStateName = Location.SplashColor == SplashColor.Black ? "Black" : "Blue";
        (dripPrefab, splashPrefab) = ExtractSplashPrefabs(swr.GetState(prefabStateName));
        splashAudio = (UE.AudioClip)((PM.Actions.AudioPlayerOneShotSingle)eff.Actions[1]).audioClip.Value;
    }

    private static (UE.GameObject dripper, UE.GameObject splash) ExtractSplashPrefabs(PM.FsmState state)
    {
        var splashAct = state.FindAction((PM.Actions.SetGameObject s) => s.variable.Name == "Splash Out Obj");
        var dripperAct = state.FindAction((PM.Actions.SetGameObject s) => s.variable.Name == "Dripper Obj");
        return (dripperAct.gameObject.Value, splashAct.gameObject.Value);
    }

    public void StartFishing()
    {
        if (fishingCoroutine != null)
        {
            StopCoroutine(fishingCoroutine);
        }

        if (!Location.Placement.AllObtained())
        {
            PlayMakerFSM.BroadcastEvent("REMINDER ATTACK");
        }
        
        fishingCoroutine = Fish();
        StartCoroutine(fishingCoroutine);
    }

    private SC.IEnumerator Fish()
    {
        for (; firstUncaughtItem < Location.Placement.Items.Count; firstUncaughtItem++)
        {
            if (Location.Placement.Items[firstUncaughtItem].IsObtained())
            {
                continue;
            }

            var splashPos = new UE.Vector3(Location.ShinySourceX, Location.ShinySourceY, 0);
            void Splash()
            {
                dripPrefab.Spawn(splashPos, UE.Quaternion.identity);
                splashPrefab.Spawn(splashPos, UE.Quaternion.identity);
                IC.Internal.SoundManager.PlayClipAtPoint(splashAudio, splashPos);
            }

            do
            {
                var dt = minOpportunityInterval + (float)rng.NextDouble() * (maxOpportunityInterval - minOpportunityInterval);
                nextCatchOpportunity = UE.Time.time + dt;
                yield return new UE.WaitForSeconds(dt);

                // Signal that a catch is available
                Splash();
                caught = false;
                // Wait for the player to maybe catch the item
                yield return new UE.WaitForSeconds(catchTolerance);
            } while (!caught);
            var flingDirection = Location.Direction == FacingDirection.Right ? IC.ShinyFling.Left : IC.ShinyFling.Right;
            var s = IC.Util.ShinyUtility.MakeNewShiny(Location.Placement, Location.Placement.Items[firstUncaughtItem], IC.FlingType.StraightUp);
            var shinyPos = new UE.Vector3(Location.ShinySourceX, Location.ShinySourceY, s.transform.position.z);
            s.transform.position = shinyPos;
            IC.Util.ShinyUtility.SetShinyFling(s.LocateMyFSM("Shiny Control"), flingDirection);
            s.SetActive(true);
            Splash();
        }
    }

    public void StopFishing()
    {
        if (fishingCoroutine != null)
        {
            StopCoroutine(fishingCoroutine);
            fishingCoroutine = null;
        }
    }

    public void FixedUpdate()
    {
        if (!GameManager.instance.isPaused
            && input.inputActions.attack.WasPressed)
        {
            caught = true;
        }
    }
}
