using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace TinyLib.Frontend
{
    [PluginConfig("Pek1ng.TinyLib.Backend", "Pek1ng", "0.0.0.1")]
    public class TinyLibBackendPlugin : TaiwuRemakePlugin
    {
        private Harmony? _harmony;

        public override void Initialize()
        {
            _harmony = new Harmony(PluginName);
            _harmony.PatchAll();
        }

        public override void Dispose()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
