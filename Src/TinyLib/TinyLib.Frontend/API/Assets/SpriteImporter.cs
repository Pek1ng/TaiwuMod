using System;
using System.Collections.Generic;
using System.IO;
using TinyLib.Frontend.API.Assets;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TinyLib.Frontend.API.Assets
{
    public class SpriteImporter : IAssetImporter
    {
        internal static DlcPackageInfo? DlcPackageInfo;

        public void OnImportStart()
        {
            DlcPackageInfo = ModToDlcHelper.GetOrAddModPackage("SpritePackge");
        }

        public void OnImportEnd()
        {
            DlcPackageInfo?.AtlasInfo.InitSelf();
        }


        public void OnImportAsset(IEnumerable<string> files)
        {
            AtlasInfo.PackerDetail detail = DlcPackageInfo!.AtlasInfo.AllPackerInfos[0];
            List<Sprite> sprites = new();
            List<string> spriteNames = new();
            foreach (var file in files)
            {
                try
                {
                    var bytes = File.ReadAllBytes(file);
                    Texture2D texture = new Texture2D(2, 2);
                    ImageConversion.LoadImage(texture, bytes);
                    Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    sprites.Add(sprite);
                    spriteNames.Add(Path.GetFileNameWithoutExtension(file));
                }
                catch
                {
                    Debug.LogError($"加载图片{file}失败！！！");
                }
            }

            detail.SpriteList = sprites;
            detail.SpriteNames= spriteNames;
        }
    }
}

public static class UIBehaviourExtensions
{
    /// <summary>
    /// 设置模组包的Sprite
    /// </summary>
    /// <param name="spriteName"></param>
    /// <param name="onGet"></param>
    public static void SetModSprite(this UIBehaviour image, string spriteName, Action<Sprite> onGet)
    {
        if (SpriteImporter.DlcPackageInfo == null)
        {
            return;
        }

        var modpack = SpriteImporter.DlcPackageInfo.AtlasInfo.AllPackerInfos[0];
        int index = modpack.SpriteNames.IndexOf(spriteName);

        if (index >= 0)
        {
            var sprite = modpack.SpriteList[index];
            onGet?.Invoke(sprite);
        }
    }
}
