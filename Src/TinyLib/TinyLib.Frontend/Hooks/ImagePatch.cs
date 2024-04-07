using HarmonyLib;

namespace TinyLib.Frontend.Hooks
{
    /// <summary>
    /// 替换不是动态加载的图片
    /// </summary>
    [HarmonyPatch(typeof(CImage))]
    internal static class CImagePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Awake))]
        private static void Awake(CImage __instance)
        {
            if (__instance.sprite == null)
            {
                return;
            }

            __instance.SetModSprite(__instance.sprite.name,sprite =>
            {
                if (sprite != __instance.sprite)
                {
                    __instance.sprite = sprite;
                }
            });
        }
    }

    [HarmonyPatch(typeof(CRawImage))]
    internal static class CRawImagePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Awake))]
        private static void Awake(CRawImage __instance)
        {
            if (__instance.texture == null)
            {
                return;
            }

            __instance.SetModSprite(__instance.texture.name, sprite =>
            {
                __instance.texture = sprite.texture;
            });
        }
    }
}
