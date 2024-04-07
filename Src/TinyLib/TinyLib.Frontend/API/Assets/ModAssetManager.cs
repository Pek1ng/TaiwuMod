using GameData.Domains.Mod;
using System.Collections.Generic;
using System.IO;

namespace TinyLib.Frontend.API.Assets
{
    public class ModAssetManager : ISingletonInit
    {
        /// <summary>
        /// 加载的资源根目录
        /// </summary>
        public const string AssetsDirectoryName = "Assets";

        private Dictionary<string, List<string>> _fileDic = new();

        public void Init()
        {
            foreach (ModId id in ModManager.EnabledMods)
            {
                ModInfo modInfo = ModManager.GetModInfo(id);
                if (!Directory.Exists(modInfo.DirectoryName))
                {
                    continue;
                }

                string assetsDir = Path.Combine(modInfo.DirectoryName, AssetsDirectoryName);
                if (!Directory.Exists(assetsDir))
                {
                    continue;
                }

                foreach (string file in Directory.GetFiles(assetsDir,"*.*", SearchOption.AllDirectories))
                {
                    string extension = Path.GetExtension(file);
                    if (_fileDic.TryGetValue(extension, out var list))
                    {
                        list.Add(file);
                    }
                    else
                    {
                        _fileDic.Add(extension, new List<string> { file });
                    }
                }
            }
        }

        public void Dispose()
        {
            _fileDic.Clear();
        }

        /// <summary>
        /// 添加一个资源导入器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extensions">文件拓展名</param>
        public void AddAssetImporter<T>(params string[] extensions) where T : IAssetImporter, new()
        {
            IAssetImporter importer = new T();
            importer.OnImportStart();

            foreach (string extension in extensions)
            {
                if (_fileDic.TryGetValue(extension, out var list))
                {
                    importer.OnImportAsset(list);
                }
            }

            importer.OnImportEnd();
        }
    }
}
