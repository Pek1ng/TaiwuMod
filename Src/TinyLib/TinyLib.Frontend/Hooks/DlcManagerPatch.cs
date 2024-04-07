using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TinyLib.Frontend.API.Assets;
using UnityEngine;
using UnityEngine.Networking;

namespace TinyLib.Frontend.Hooks
{
    [HarmonyPatch(typeof(DlcManager))]
    internal static class DlcManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GetAudioClip))]
        private static bool GetAudioClip(ref bool __result,string clipName, bool isMusic, Action<AudioClip> onGetClip)
        {
            if (clipName.IsNullOrEmpty() || onGetClip == null)
            {
                return true;
            }
            else
            {
                var hasClip = GetModAudioClip(clipName, onGetClip);
                if (hasClip)
                {
                    __result = true;
                    return false; //阻断后续执行
                }
            }

            return true;
        }

        public static bool GetModAudioClip(string clipName, Action<AudioClip> onGetClip)
        {
            if (AudioImporter.DlcPackageInfo == null)
            {
                return false;
            }

            if (AudioImporter.DlcPackageInfo.AudioInfo.HasClip(clipName, true))
            {
                foreach (var info in AudioImporter.DlcPackageInfo.AudioInfo.MusicInfos)
                {
                    if (clipName == info.MusicName)
                    {
                        SingletonObject.getInstance<YieldHelper>().StartCoroutine(LoadExternalAudio(info.MusicPath, onGetClip));
                        return true;
                    }
                }
            }

            return false;
        }

        private static IEnumerator LoadExternalAudio(string path, Action<AudioClip> onGetClip)
        {
            if (!File.Exists(path))
            {
                yield break;
            }

            AudioType type;
            if (_extensionToAudioType.TryGetValue(Path.GetExtension(path), out type))
            {
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, type))
                {
                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogError(www.error);
                    }
                    else
                    {
                        AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                        onGetClip.Invoke(myClip);
                    }
                }
            }
        }

        private static Dictionary<string, AudioType> _extensionToAudioType = new()
        {
            [".ogg"] = AudioType.OGGVORBIS,
            [".wav"] = AudioType.WAV,
        };
    }
}
