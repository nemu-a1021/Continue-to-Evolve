using BepInEx;
using BepInEx.Unity.IL2CPP;
using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hazel;
using System.Linq;

namespace ContinuetoEolve
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("Among Us.exe")]
    public class Main : BasePlugin
    {
        public const string PluginName = "Continue-to-Evolve";
        public const string PluginGuid = "com.nemua.cntinue-to-evolve";//なんか入れてもらて
        public const string PluginVersion = "0.0.1";//バージョン
        public static Sprite ModStamp;
        public Harmony Harmony { get; } = new Harmony(PluginGuid);
        public static bool HostMode = true;
        public static bool NoEndGame = true;
        public static Dictionary<string, bool> ModRoleActive = new();
        public static Dictionary<string, RoleTypes> ModRoleType = new();
        public static Dictionary<string, string> RoleColor = new();
        public static Dictionary<byte, string> PlayerRole = new();
        public override void Load()
        {
            Log.LogInfo($"{PluginName}Load!");
            Harmony.PatchAll();
            RoleLoad();
        }
        [HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
        class ModManagerLateUpdatePatch
        {
            public static void Prefix(ModManager __instance)
            {
                __instance.ShowModStamp();
            }
        }
        public static void RoleLoad()
        {
            ModRoleActive.Clear();
            ModRoleType.Clear();
            RoleColor.Clear();
            RoleColor.Add("Crewmate", "#00ffff");//バニラ役職
            RoleColor.Add("Engineer", "#00ffff");
            RoleColor.Add("Scientist", "#00ffff");
            RoleColor.Add("GuardianAngel", "#00ffff");
            RoleColor.Add("Impostor", "#ff0000");
            RoleColor.Add("Shapeshifter", "#ff0000");
            ModRoleActive.Add("Sheriff", true);
            ModRoleType.Add("Sheriff", RoleTypes.Impostor);
            RoleColor.Add("Sheriff", "#f4ff00");
            ModRoleActive.Add("Bait", true);
            ModRoleType.Add("Bait", RoleTypes.Crewmate);
            RoleColor.Add("Bait", "#00ffff");

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

    static class RPC
    {
        public static void RpcSetNamePrivate(this PlayerControl player, string name, bool DontShowOnModdedClient = false, PlayerControl seer = null, bool force = false)
        {//TOHによりお借りしました
            //player: 名前の変更対象
            //seer: 上の変更を確認することができるプレイヤー
            if (player == null || name == null || !AmongUsClient.Instance.AmHost) return;
            if (seer == null) seer = player;
            var clientId = GetClientId(seer);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, Hazel.SendOption.Reliable, clientId);
            writer.Write(name);
            writer.Write(DontShowOnModdedClient);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static int GetClientId(this PlayerControl player)
        {
            var client = GetClient(player);
            return client == null ? -1 : client.Id;
        }
        public static InnerNet.ClientData GetClient(this PlayerControl player)
        {//ここまでtoh
            var client = AmongUsClient.Instance.allClients.ToArray().Where(cd => cd.Character.PlayerId == player.PlayerId).FirstOrDefault();
            return client;
        }

    }


    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class SelectRolesPatch
    {
        public static bool Prefix(RoleManager __instance)
        {
            List<byte> AllPlayers = new();
            List<string> ActiveRoles = new();
            var rand = new System.Random();

            foreach (var pc in PlayerControl.AllPlayerControls)
                AllPlayers.Add(pc.PlayerId);
            //プレイヤーとロール
            foreach (var ra in Main.ModRoleActive)
                if (ra.Value)
                    ActiveRoles.Add(ra.Key);

            for (var i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
            {
                PlayerControl Player = null;
                string Role;
                var Playerid = AllPlayers[rand.Next(0, AllPlayers.Count)]; //この二つは一時的に使うだけ
                var Roleid = rand.Next(0, ActiveRoles.Count); //↑
                Role = ActiveRoles[Roleid];
                foreach (var pc in PlayerControl.AllPlayerControls)
                    if (pc.PlayerId == Playerid)
                    {
                        Player = pc;
                        break; //処理終了
                    }
                if (Player == null) continue;
                //pc.RpcSetRole(Role); ここは後でCustomSetRoleRPC作りますね
                Debug.Log($"Player:{Playerid},Role:{Role}");

                ActiveRoles.RemoveAt(Roleid);
                AllPlayers.Remove(Playerid);
            }
            return true;

            /*if (Main.HostMode)
            {
                if (!AmongUsClient.Instance.AmHost) return;
                Main.PlayerRole.Clear();
                foreach (var pc in PlayerControl.AllPlayerControls)
                    Main.PlayerRole.Add(pc.PlayerId, "none");
                foreach (var role in Main.ModRoleActive)
                {
                    if (role.Value)
                    {
                        var rand = new System.Random();
                        var id = rand.Next(0, PlayerControl.AllPlayerControls.Count);

                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.PlayerId == id)
                            {
                                if (Main.PlayerRole[pc.PlayerId] != "none")
                                    Console.print("key aru! error");
                                else
                                {
                                    Main.PlayerRole[pc.PlayerId] = role.Key;
                                    pc.RpcSetRole(Main.ModRoleType[role.Key]);
                                }
                            }
                        }
                    }
                }
                foreach (var pc in PlayerControl.AllPlayerControls)
                    if (Main.PlayerRole[pc.PlayerId] == "none")
                    {
                        Main.PlayerRole[pc.PlayerId] = $"{pc.Data.Role.Role}";
                    }
                foreach (var n in Main.PlayerRole)
                    Console.print(n.Key + " = " + n.Value);
                foreach (var pc in PlayerControl.AllPlayerControls)
                    RPC.RpcSetNamePrivate(pc, $"<color={Main.RoleColor[Main.PlayerRole[pc.PlayerId]]}>" + Main.PlayerRole[pc.PlayerId] + "\n" + pc.name, seer: pc);
            }*/

        }
    }
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
    class CheckTaskCompletionPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (Main.NoEndGame)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    class GameEndChecker
    {
        public static bool Prefix()
        {
            return !Main.NoEndGame;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    class CheckMurderPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return false;
            if (Main.PlayerRole[__instance.PlayerId] == "Sheriff")
                if (!target.Data.Role.IsImpostor)
                {
                    __instance.RpcMurderPlayer(__instance, true);
                    return false;
                }
            return true;

        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    class MurderPlayerPatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (Main.PlayerRole[target.PlayerId] == "Bait")
                __instance.ReportDeadBody(__instance.Data);
        }
    }

}

