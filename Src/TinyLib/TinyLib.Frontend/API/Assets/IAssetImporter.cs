using System.Collections.Generic;

namespace TinyLib.Frontend.API.Assets
{
    public interface IAssetImporter
    {
        void OnImportStart();

        void OnImportAsset(IEnumerable<string> files);

        void OnImportEnd();
    }
}
