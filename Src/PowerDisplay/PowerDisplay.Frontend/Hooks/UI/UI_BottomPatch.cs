using Assets.Scripts.Game.Model;
using Config;
using FrameWork;
using GameData.Domains.Building;
using GameData.Domains.Extra;
using GameData.Serializer;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerDisplay.Frontend.Hooks.UI
{
    [HarmonyPatch(typeof(UI_Bottom))]
    internal static class UI_BottomPatch
    {
        private static List<GameObject>? _cloneObjects;

        private static List<BuildingBlockData>? _blockList;

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
        [HarmonyPatch(nameof(OnEnable))]
        private static void OnEnable(UI_Bottom __instance)
        {
            _cloneObjects = new();

            ShowJiaoPoll(__instance);
            ShowTeaHorseCaravan(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(OnDisable))]
        private static void OnDisable()
        {
            if (_cloneObjects is not null)
            {
                for (int i = 0; i < _cloneObjects.Count; i++)
                {
                    GameObject.Destroy(_cloneObjects[i]);
                }
            }

            _cloneObjects = null;
        }

        /// <summary>
        /// 快速打开茶马帮
        /// </summary>
        private static void ShowTeaHorseCaravan(UI_Bottom __instance)
        {
            var location = _worldMapModel.GetTaiwuVillageBlock();
            BuildingDomainHelper.AsyncMethodCall.GetBuildingBlockList(null, location, delegate (int offset, RawDataPool dataPool)
            {
                Serializer.Deserialize(dataPool, offset, ref _blockList);

                if (_blockList == null)
                {
                    return;
                }

                BuildingBlockData? blockData = null;
                foreach (var item in _blockList)
                {
                    if (item.TemplateId == BuildingBlock.DefKey.TeaHorseCaravan)
                    {
                        blockData = item;
                        break;
                    }
                }
                if (blockData == null)
                {
                    return;
                }

                CreateBtnGo(__instance, new ButtonStyle(
                    "[Mmod]ChaMaBang",
                    ["bottom_icon_chamabang_0", "bottom_icon_chamabang_1"],
                    ["茶马帮", "快速打开茶马帮"],
                    () =>
                    {
                        UIElement.TeaHorseCaravan.SetOnInitArgs(EasyPool.Get<ArgumentBox>().SetObject("BuildingBlockData", blockData));
                        UIManager.Instance.ShowUI(UIElement.TeaHorseCaravan);
                }));
            });
        }

        /// <summary>
        /// 快速打开蛟池
        /// </summary>
        private static void ShowJiaoPoll(UI_Bottom __instance)
        {
            var location = _worldMapModel.GetTaiwuVillageBlock();
            BuildingDomainHelper.AsyncMethodCall.GetBuildingAreaData(__instance, location, delegate (int offset, RawDataPool dataPool)
            {
                BuildingAreaData areaData = new();
                Serializer.Deserialize(dataPool, offset, ref areaData);

                if (areaData == null)
                {
                    return;
                }

                ExtraDomainHelper.AsyncMethodCall.GetIsJiaoPoolOpen(null, delegate (int offset2, RawDataPool dataPool2)
                {
                    bool isOpen = false;
                    Serializer.Deserialize(dataPool2, offset2, ref isOpen);
                    if (isOpen)
                    {
                        CreateBtnGo(__instance, new ButtonStyle(
                            "[Mmod]JiaoChi",
                            ["bottom_icon_jiaopool_0", "bottom_icon_jiaopool_1"],
                            ["蛟池", "快速打开蛟池"],
                            () =>
                            {
                                ArgumentBox data = EasyPool.Get<ArgumentBox>();
                                data.Set("landFormType", areaData.LandFormType);
                                UIElement.BuildingJiaoPool.SetOnInitArgs(data);
                                UIManager.Instance.ShowUI(UIElement.BuildingJiaoPool);
                        }));
                    }
                });
            });
        }

        private static GameObject CreateBtnGo(UI_Bottom __instance, ButtonStyle style)
        {
            var go = __instance.CGet<GameObject>("WorldMap");
            var parent = __instance.CGet<GameObject>("BtnLayout");

            var ins = GameObject.Instantiate(go, parent.transform);
            _cloneObjects?.Add(ins);

            ins.name = style.Name;
            ins.GetComponent<CImage>().SetSprite(style.Icons[0]);
            ins.GetComponent<MouseTipDisplayer>().PresetParam = style.PresetParam;

            try
            {
                //如果直接调用方法会被内联异常捕获不到
                Action action = () =>
                {
                    var btn = ins.GetComponent<CButton>();
                    btn.SetModSprite(style.Icons[1], sprite =>
                    {
                        btn.spriteState = new SpriteState()
                        {
                            highlightedSprite = sprite
                        };
                    });
                };
                action();
            }
            catch (Exception e)
            {
                if (e is TypeLoadException)
                {
                    Debug.LogError("如果你看的这条说明依赖的模组没有订阅，重新订阅下【显示增强】模组");
                }
                else
                {
                    throw e;
                }
            }
            ins.GetComponent<CButton>().ClearAndAddListener(style.Action);

            return ins;
        }

        private class ButtonStyle
        {
            public string Name;

            public string[] Icons;

            public Action Action;

            public string[] PresetParam;

            public ButtonStyle(string name, string[] icons, string[] presetParam, Action action)
            {
                Name = name;
                Icons = icons;
                Action = action;
                PresetParam = presetParam;
            }
        }
    }
}
