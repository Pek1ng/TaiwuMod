using HarmonyLib;
using System;
using TaiwuModdingLib.Core.Plugin;
using TinyLib.Frontend.API;
using TinyLib.Frontend.API.Assets;
using TinyLib.Frontend.Lua;
using XLua;

namespace TinyLib.Frontend
{
    [PluginConfig("Pek1ng.TinyLib.Frontend", "Pek1ng", "0.0.0.1")]
    public class TinyLibFrontendPlugin : TaiwuRemakePlugin
    {
        private Harmony? _harmony;

        public override void Initialize()
        {
            _harmony = new Harmony(PluginName);
            _harmony.PatchAll();

            InitModAssets();
        }

        public override void Dispose()
        {
            _harmony?.UnpatchSelf();
        }

        private void InitModAssets()
        {
            SingletonObjectUtils.DoNotClear<ModAssetManager>();
            var modManager = SingletonObject.getInstance<ModAssetManager>();

            modManager.AddAssetImporter<SpriteImporter>(".png", ".jpg");
            modManager.AddAssetImporter<AudioImporter>(".ogg", ".wav");
            modManager.AddAssetImporter<LuaImporter>(".lua");
        }
    }
}
