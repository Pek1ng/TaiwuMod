using Assets.Scripts.Game.Model;
using Config;
using FrameWork;
using GameData.Domains.Building;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.Display;
using GameData.Domains.Taiwu;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PowerDisplay.Frontend.Hooks.UI
{
    [HarmonyPatch(typeof(UI_SelectChar))]
    internal static class UI_SelectCharPatch
    {
        private static List<BuildingBlockData>? _blockList;

        private static BuildingBlockItem? _configData;

        private static List<int> _needUpdateCell = new();

        private static AccessTools.FieldRef<UI_SelectChar, InfinityScroll> _charScrollRef = AccessTools.FieldRefAccess<UI_SelectChar, InfinityScroll>("_charScroll");

        private static BuildingModel _buildingModel = new Lazy<BuildingModel>(() =>
        {
            return SingletonObject.getInstance<BuildingModel>();
        }).Value;

        private static BasicGameData _basicGameData = new Lazy<BasicGameData>(() =>
        {
            return SingletonObject.getInstance<BasicGameData>();
        }).Value;

        private static WorldMapModel _worldMapModel = new Lazy<WorldMapModel>(() =>
        {
            return SingletonObject.getInstance<WorldMapModel>();
        }).Value;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(OnInit))]
        private static void OnInit(UI_SelectChar __instance, ArgumentBox argsBox)
        {
            _needUpdateCell = new();

            if (argsBox is null)
            {
                return;
            }

            var location = _worldMapModel.GetTaiwuVillageBlock();
            BuildingDomainHelper.AsyncMethodCall.GetBuildingBlockList(__instance, location, delegate (int offset, RawDataPool dataPool)
            {
                Serializer.Deserialize(dataPool, offset, ref _blockList);

                foreach (int cell in _needUpdateCell) //在列表加载之前的元素刷新
                {
                    _charScrollRef(__instance).RefreshCell(cell);
                }

                _needUpdateCell.Clear();
            });

            _configData = null;
            UI_BuildingManage buildingManage = UIElement.BuildingManage.UiBaseAs<UI_BuildingManage>();
            if (buildingManage is not null)
            {
                _configData = AccessTools.FieldRefAccess<UI_BuildingManage, BuildingBlockItem>("_configData")(buildingManage);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(OnDisable))]
        private static void OnDisable()
        {
            _blockList?.Clear();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(OnRenderChar))]
        private static void OnRenderChar(
            UI_SelectChar __instance,

            bool[] ____iconShowFlags,
            List<int> ____canSelectCharIdList,

            byte ____charPrefabType,

            int index, Refers refers)
        {
            int charId = ____canSelectCharIdList[index];

            bool showWorking = ____iconShowFlags[0];
            if (showWorking)
            {
                ShowWorkPlace(index,charId, refers);
            }

            bool showSkillType = ____charPrefabType == 1;
            if (showSkillType)
            {
                ShowSpecialSkillType(__instance, charId, refers);
            }
        }

        /// <summary>
        /// 显示工作地点
        /// </summary>
        private static void ShowWorkPlace(int index,int charId, Refers refers)
        {
            if (!_buildingModel.VillagerWork.TryGetValue(charId, out var workData))
            {
                return;
            }

            string tips;
            string icon;

            sbyte workType = workData.WorkType;
            if (workType == VillagerWorkType.Build)
            {
                tips = "建设中";
                icon = "building_icon_kuojian_0";
            }
            else if (workType == VillagerWorkType.ShopManage)
            {
                tips = "经营中";
                icon = "building_icon_jingying_0";

                if (_blockList != null)
                {
                    if (workData.BuildingBlockIndex <= _blockList.Count)
                    {
                        var blockKey = new BuildingBlockKey(workData.AreaId, workData.BlockId, workData.BuildingBlockIndex);
                        BuildingBlockData blockData = _blockList[workData.BuildingBlockIndex];
                        var configData = BuildingBlock.Instance[blockData.TemplateId];
                        tips = configData.Name;

                        if (_buildingModel.CustomBuildingName.ContainsKey(blockKey))
                        {
                            var customTexts = _basicGameData.CustomTexts;
                            if (customTexts.ContainsKey(_buildingModel.CustomBuildingName[blockKey]))
                            {
                                tips = _basicGameData.CustomTexts[_buildingModel.CustomBuildingName[blockKey]];
                            }
                        }
                    }
                }
                else //可能加载在拿到协议之前
                {
                    _needUpdateCell.Add(index);
                }
            }
            else if (VillagerWorkType.IsWorkOnMap(workType))
            {
                tips = "派遣中";
                icon = "bottom_icon_zhipai_0";
            }
            else
            {
                tips = $"unknown work type : {workType}";
                icon = "sp_icon_renwuzhuangtai_0";
            }

            refers.CGet<MouseTipDisplayer>("WorkingIconTip").PresetParam[0] = tips;
            refers.CGet<GameObject>("WorkingIcon").GetComponent<CImage>().SetSprite(icon, false, null);
        }

       /// <summary>
       /// 根据特殊收益机制建筑显示对应属性
       /// </summary>
        private static void ShowSpecialSkillType(UI_SelectChar __instance, int charId, Refers refers)
        {
            //收益机制 https://taiwu.huijiwiki.com/wiki/%E5%BB%BA%E7%AD%91/%E6%94%B6%E7%9B%8A%E6%9C%BA%E5%88%B6#%E6%94%B6%E8%8E%B7%E8%B5%84%E6%BA%90%E7%9A%84%E5%BB%BA%E7%AD%91
            if (charId != -1 && _configData is not null)
            {
                short templateId = _configData.TemplateId;

                if (templateId == BuildingBlock.DefKey.GamblingHouse)
                {
                    CharacterDomainHelper.AsyncMethodCall.GetGroupCharDisplayDataList(__instance, new List<int> { charId }, delegate (int offset, RawDataPool dataPool)
                    {
                        List<GroupCharDisplayData>? dataList = null;
                        Serializer.Deserialize(dataPool, offset, ref dataList);
                        GroupCharDisplayData groupCharData = dataList[0];

                        int maxValue = 0;
                        for (int i = 0; i < 7; i++)
                        {
                            var personality = groupCharData.Personalities[i];
                            maxValue += personality;
                        }
                        var text = refers.CGet<TextMeshProUGUI>("SkillTypeNum").text;
                        refers.CGet<TextMeshProUGUI>("SkillTypeNum").SetText($"{text}\n七元:{maxValue}", true);
                    });
                }
                else if (templateId == BuildingBlock.DefKey.Brothel)
                {
                    CharacterDomainHelper.AsyncMethodCall.GetGroupCharDisplayDataList(__instance, new List<int> { charId }, delegate (int offset, RawDataPool dataPool)
                    {
                        List<GroupCharDisplayData>? dataList = null;
                        Serializer.Deserialize(dataPool, offset, ref dataList);
                        GroupCharDisplayData groupCharData = dataList[0];

                        CharacterItem charConfig = Character.Instance.GetItem(groupCharData.CharacterTemplateId);
                        bool isFixedPresetType = CreatingType.IsFixedPresetType(charConfig.CreatingType);
                        string charm = CommonUtils.GetCharmLevelText(groupCharData.Charm, groupCharData.Gender, groupCharData.CurrAge, groupCharData.ClothDisplayId, isFixedPresetType, groupCharData.FaceVisible);
                        string icon = CommonUtils.GetCharmLevelIcon(groupCharData.Charm, groupCharData.CurrAge, groupCharData.ClothDisplayId);
                        refers.CGet<CImage>("SkillTypeIcon").SetSprite(icon, false, null);
                    });
                }
            }
        }
    }
}
