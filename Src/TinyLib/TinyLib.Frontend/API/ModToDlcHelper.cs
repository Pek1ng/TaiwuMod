using AudioKit;
using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TinyLib.Frontend.API
{
    internal static class ModToDlcHelper
    {
        private static uint _packageID = 1000;

        public static List<DlcPackageInfo> _dlcList = new Lazy<List<DlcPackageInfo>>(() =>
        {
            var manager = SingletonObject.getInstance<DlcManager>();
            return Traverse.Create(manager).Field<List<DlcPackageInfo>>("_dlcList").Value;
        }).Value;

        public static Dictionary<uint, DlcPackageInfo> _dlcDataMap = new Lazy<Dictionary<uint, DlcPackageInfo>>(() =>
        {
            var manager = SingletonObject.getInstance<DlcManager>();
            return Traverse.Create(manager).Field<Dictionary<uint, DlcPackageInfo>>("_dlcDataMap").Value;
        }).Value;

        public static DlcPackageInfo GetOrAddModPackage(string packageName)
        {
            if (_dlcList == null)
            {
                _dlcList = new();
                _dlcDataMap = new();
            }

            foreach (var package in _dlcList)
            {
                if (package != null && package.Name == packageName)
                {
                    return package;
                }
            }

            var dlcPackage = ScriptableObject.CreateInstance<DlcPackageInfo>();
            dlcPackage.Id = _packageID++;
            dlcPackage.Author = "Pek1ng";
            dlcPackage.Name = packageName;

            //创建图集包
            dlcPackage.AtlasInfo = ScriptableObject.CreateInstance<AtlasInfo>();
            dlcPackage.AtlasInfo.AllPackerInfos = new AtlasInfo.PackerDetail[1];
            dlcPackage.AtlasInfo.AllPackerInfos[0] = new AtlasInfo.PackerDetail
            {
                PackerName = "ModSprites",
                SpriteList = new(),
                SpriteNames = new(),
            };
            dlcPackage.AtlasInfo.InitSelf();

            dlcPackage.AudioInfo = ScriptableObject.CreateInstance<AudioInfos>();
            dlcPackage.AudioInfo.MusicInfos = new();
            dlcPackage.AudioInfo.SePackageInfos = new();
            dlcPackage.AtlasInfo.InitSelf();
            
            _dlcList.Add(dlcPackage);
            _dlcDataMap.Add(dlcPackage.Id, dlcPackage);

            return dlcPackage;
        }
    }
}
