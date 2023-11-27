/*using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ContinuetoEvolve
{
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    [HarmonyPriority(Priority.First)]
    public static class GameOptionsMenuPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (GameObject.Find("CECSettings") != null)
            {
                GameObject.Find("CECSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText("SettingSuperNewRoles");
                return;
            }
            StringOption template = GameObject.Find("Main Camera/PlayerOptionsMenu(Clone)/Game Settings/GameGroup/SliderInner/KillDistance").GetComponent<StringOption>();
            if (template == null) return;
            var gameSettings = GameObject.Find("Main Camera/PlayerOptionsMenu(Clone)/Game Settings/");
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();

            var cecSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var cecMenu = cecSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            cecSettings.name = "CECSettings";
            cecSettings.transform.FindChild("GameGroup").FindChild("SliderInner").name = "GenericSetting";
            gameSettings.name = "CECTab";
            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");

            var cecTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            var cecTabHighlight = cecTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            cecTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Main.LogoPatch.LoadSprite("continue-to-evolve.Resources.TabIcon.png", 100f);
            gameTab.transform.position += Vector3.left * 0.8f;
            roleTab.transform.position += Vector3.left * 0.8f;
            cecTab.transform.position += Vector3.right * 0.2f;

        }
    }
}*/