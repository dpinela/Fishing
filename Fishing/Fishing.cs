using IO = System.IO;
using MAPI = Modding;
using UE = UnityEngine;
using CG = System.Collections.Generic;
using IC = ItemChanger;
using MC = MenuChanger;
using RC = RandomizerCore;
using Rando = RandomizerMod;
using RandoData = RandomizerMod.RandomizerData;

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

        foreach (var loc in FishingLocation.Locations)
        {
            var tag = loc.AddTag<IC.Tags.InteropTag>();
            tag.Message = "RandoSupplementalMetadata";
            tag.Properties["ModSource"] = nameof(Fishing);
            tag.Properties["MapLocations"] = new (string scene, float x, float y)[]
            {
                (loc.sceneName!, loc.MarkerX, loc.MarkerY)
            };
            IC.Finder.DefineCustomLocation(loc);
        }

        Rando.RC.RequestBuilder.OnUpdate.Subscribe(30, ApplyLocationSettings);
        // must happen before ApplyMultiLocationRebalancing in RandomizerMod
        Rando.RC.RequestBuilder.OnUpdate.Subscribe(49, AddLocationsToPool);
        // run just after Transcendence so its logic patches also apply to our locations
        Rando.RC.RCData.RuntimeLogicOverride.Subscribe(50.1f, HookLogic);
        Rando.Menu.RandomizerMenuAPI.AddMenuPage(_ => {}, BuildConnectionMenuButton);
        Rando.Logging.SettingsLog.AfterLogSettings += LogRandoSettings;
    }

    private ModSettings settings = new();

    public void OnLoadGlobal(ModSettings s)
    {
        settings = s;
    }

    public ModSettings OnSaveGlobal() => settings;

    private void ApplyLocationSettings(Rando.RC.RequestBuilder rb)
    {
        if (!settings.Enabled)
        {
            return;
        }
        foreach (var loc in FishingLocation.Locations)
        {
            rb.EditLocationRequest(loc.name, info =>
            {
                info.getLocationDef = () => new()
                {
                    Name = loc.name,
                    SceneName = loc.sceneName!,
                    FlexibleCount = true,
                    AdditionalProgressionPenalty = true,
                };
            });
        }
    }

    private void AddLocationsToPool(Rando.RC.RequestBuilder rb)
    {
        if (!settings.Enabled)
        {
            return;
        }
        foreach (var loc in FishingLocation.Locations)
        {
            rb.AddLocationByName(loc.name);
        }
    }

    private void HookLogic(Rando.Settings.GenerationSettings gs, RC.Logic.LogicManagerBuilder lmb)
    {
        var modDir = IO.Path.GetDirectoryName(typeof(Fishing).Assembly.Location);
        var jsonFmt = new RC.Json.JsonLogicFormat();
        using (var locations = IO.File.OpenRead(IO.Path.Combine(modDir, "locations.json")))
        {
            lmb.DeserializeFile(RC.Logic.LogicFileType.Locations, jsonFmt, locations);
        }
    }

    private bool BuildConnectionMenuButton(MC.MenuPage landingPage, out MC.MenuElements.SmallButton settingsButton)
    {
        var button = new MC.MenuElements.SmallButton(landingPage, "Fishing");

        void UpdateButtonColor()
        {
            button.Text.color = settings.Enabled ? MC.Colors.TRUE_COLOR : MC.Colors.DEFAULT_COLOR;
        }

        UpdateButtonColor();
        button.OnClick += () =>
        {
            settings.Enabled = !settings.Enabled;
            UpdateButtonColor();
        };
        settingsButton = button;
        return true;
    }

    private void LogRandoSettings(Rando.Logging.LogArguments args, IO.TextWriter w)
    {
        w.WriteLine("Logging Fishing settings:");
        w.WriteLine(RandoData.JsonUtil.Serialize(settings));
    }

    public override string GetVersion() => "1.0";
}

