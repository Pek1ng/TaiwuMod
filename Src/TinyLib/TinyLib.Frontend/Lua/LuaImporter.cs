using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinyLib.Frontend.API.Assets;
using XLua;

namespace TinyLib.Frontend.Lua
{
    public class LuaImporter : IAssetImporter
    {
        private static List<string> _luaFiles = new();

        public void OnImportStart()
        {
            LuaEnv env = LuaGame.Instance.GetMainEnv();
            env.AddLoader(CustomLoader);
        }

        public void OnImportEnd()
        {

        }

        public void OnImportAsset(IEnumerable<string> files)
        {
            _luaFiles.AddRange(files);
        }

        private byte[]? CustomLoader(ref string path)
        {
            path = path.Replace(".", "/") + ".lua";
            string key = path;

            if (path.StartsWith(ModAssetManager.AssetsDirectoryName))
            {
                string? filePath = _luaFiles.FirstOrDefault(fullPath => fullPath.Contains(key));
                if (!string.IsNullOrEmpty(filePath))
                {
                    return File.ReadAllBytes(filePath);
                }
            }

            return null;
        }
    }
}
