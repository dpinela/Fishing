using IO = System.IO;
using IC = ItemChanger;
using NJ = Newtonsoft.Json;
using UE = UnityEngine;

namespace Fishing;

internal class PinSprite : IC.ISprite
{
    public static PinSprite Instance = new();

    [NJ.JsonIgnore] public UE.Sprite Value => LoadSprite(ref sprite, "FishPin.png");
    public IC.ISprite Clone() => (IC.ISprite)MemberwiseClone();

    private static UE.Sprite? sprite;

    private static UE.Sprite LoadSprite(ref UE.Sprite? store, string name)
    {
        if (store != null)
        {
            return store;
        }
        var loc = IO.Path.Combine(IO.Path.GetDirectoryName(typeof(PinSprite).Assembly.Location), name);
        var imageData = IO.File.ReadAllBytes(loc);
        var tex = new UE.Texture2D(1, 1, UE.TextureFormat.RGBA32, false);
        UE.ImageConversion.LoadImage(tex, imageData, true);
        tex.filterMode = UE.FilterMode.Bilinear;
        store = UE.Sprite.Create(tex, new UE.Rect(0, 0, tex.width, tex.height), new UE.Vector2(.5f, .5f));
        return store;
    }
}
