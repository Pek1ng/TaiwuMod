using System.Collections.Generic;
using System.IO;

namespace TinyLib.Frontend.API.Assets
{
    public class AudioImporter : IAssetImporter
    {
        internal static DlcPackageInfo? DlcPackageInfo;

        public void OnImportStart()
        {
            DlcPackageInfo = ModToDlcHelper.GetOrAddModPackage("AudioPackge");
        }

        public void OnImportEnd()
        {
            DlcPackageInfo?.AudioInfo.InitSelf();
        }

        public void OnImportAsset(IEnumerable<string> files)
        {
            DlcPackageInfo!.AudioInfo.MusicInfos = new();
            foreach (var file in files)
            {
                DlcPackageInfo.AudioInfo.MusicInfos.Add(new AudioKit.MusicInfo
                {
                    MusicName = Path.GetFileNameWithoutExtension(file),
                    MusicPath = file,
                });
            }
        }
    }
}
