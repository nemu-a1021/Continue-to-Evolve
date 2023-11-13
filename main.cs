using BepInEx;
using BepInEx.Unity.IL2CPP;
using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ContinuetoEolve
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("Among Us.exe")]
    public class Plugin : BasePlugin
    {
        public const string PluginName = "Continue-to-Evolve";
        public const string PluginGuid = "com.nemua.cntinue-to-evolve";//なんか入れてもらて
        public const string PluginVersion = "0.0.1";//バージョン
        public static Sprite ModStamp;
        public Harmony Harmony { get; } = new Harmony(PluginGuid);
        public override void Load()
        {
            Log.LogInfo($"{PluginName}Load!");
            Harmony.PatchAll();
        }
        [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
        class ModManagerLateUpdatePatch
        {
            public static void Prefix(ModManager __instance)
            {
                __instance.ShowModStamp();
            }
        }
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class LogoPatch
        {
            public static SpriteRenderer renderer;
            [HarmonyPriority(Priority.VeryHigh)]
            static void Postfix(MainMenuManager __instance)
            {
                var cte = new GameObject("Logo-CTE");
                cte.transform.localPosition = new(0f, 0f, 0f);
                cte.transform.localScale *= 1.2f;
                renderer = cte.AddComponent<SpriteRenderer>();
                renderer.sprite = LoadSprite("ContinuetoEolve.Resources.Logo.png", 300f);

            }
            //TOH参考 動かない☆
            public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
            {
                Sprite sprite = null;
                try
                {
                    var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
                    var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    using MemoryStream ms = new();
                    stream.CopyTo(ms);
                    ImageConversion.LoadImage(texture, ms.ToArray());
                    sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f), pixelsPerUnit);
                }
                catch
                {
                    Debug.LogError($"{path} ?なにそれ?");
                }
                return sprite;
            }
        }
    }
}