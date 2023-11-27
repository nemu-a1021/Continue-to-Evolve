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
using System;

using ContinuetoEvolve.Patches;
using static ContinuetoEvolve.RoleHelper;

namespace ContinuetoEvolve
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("Among Us.exe")]
    public class Main : BasePlugin
    {
        public const string PluginName = "Continue-to-Evolve";
        public const string PluginGuid = "com.nemua.cntinue-to-evolve";//なんか入れてもらて
        public const string PluginVersion = "0.0.1";//バージョン
        public Harmony Harmony { get; } = new Harmony(PluginGuid);
        public static bool NoEndGame = true;
        public static Dictionary<byte, CustomRoles> PlayerRole = new();

        public static Main Instance;
        public override void Load()
        {
            Instance = this;
            Log.LogInfo($"{PluginName}Load!");
            Harmony.PatchAll();
            RoleLoad();
            CustomOptionHolder.LoadOptions();
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
            static void Postfix(MainMenuManager __instance)
            {
                var cte = new GameObject("Logo-CTE");
                cte.transform.localPosition = new(2f, 0.5f, 0f);
                cte.transform.localScale *= 1.2f;
                renderer = cte.AddComponent<SpriteRenderer>();
                renderer.sprite = LoadSprite("continue-to-evolve.Resources.Logo.png", 300f);

            }
            //TOH参考
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
        public static void RpcSetCustomRole(this PlayerControl player, CustomRoles role)
        {
            Main.PlayerRole[player.PlayerId] = role;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetCustomRole, SendOption.Reliable);
            writer.Write(player.PlayerId);
            writer.WritePacked((int)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public enum CustomRPC
        {
            VersionCheck,
            SetCustomRole,
        }

    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class HandleRpc
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            RPC.CustomRPC rpcType = (RPC.CustomRPC)callId;
            switch (rpcType)
            {
                case RPC.CustomRPC.SetCustomRole:
                    CustomRoles role = (CustomRoles)reader.ReadPackedInt32();
                    byte player = reader.ReadByte();
                    Main.PlayerRole[player] = role;
                    Debug.Log($"SetCustomRoleを受け取りました。 player:{player} role:{role}");
                    break;
            }
        }
    }


    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class SelectRolesPatch
    {
        public static bool Prefix(RoleManager __instance)
        {
            List<byte> AllPlayers = new();
            List<CustomRoles> ActiveRoles = new();
            List<byte> Impostor = new();
            var rand = new System.Random();
            //var numImpostors = Math.Min(PlayerControl.AllPlayerControls.Count, GameOptionsManager.Instance.CurrentGameOptions.NumImpostors);
            Main.PlayerRole.Clear();

            foreach (var pc in PlayerControl.AllPlayerControls)
                AllPlayers.Add(pc.PlayerId);
            //プレイヤーとロール
            ActiveRoles = GetActiveRole();
            /*for (var i = 0; i < numImpostors; i++)
            {
                Impostor.Add((byte)rand.Next(0, AllPlayers.Count));
            }*/
            if (ActiveRoles.Count == 0 || ActiveRoles == null) return true;
            for (var i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
            {
                Console.print("!!");
                PlayerControl Player = null;
                CustomRoles Role;
                var Playerid = AllPlayers[rand.Next(0, AllPlayers.Count)]; //この二つは一時的に使うだけ
                var Roleid = rand.Next(0, ActiveRoles.Count); //↑
                Debug.Log(Roleid + "," + ActiveRoles.Count);
                Role = ActiveRoles[Roleid];
                foreach (var pc in PlayerControl.AllPlayerControls)
                    if (pc.PlayerId == Playerid)
                    {
                        Player = pc;
                        break; //処理終了
                    }
                if (Player == null) continue;
                //pc.RpcSetRole(Role); ここは後でCustomSetRoleRPC作りますね
                Player.RpcSetCustomRole(Role);
                Debug.Log($"Player:{Playerid},Role:{Role}");

                //ActiveRoles.RemoveAt(Roleid);
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
            if (Main.PlayerRole[__instance.PlayerId] == CustomRoles.Sheriff)
                if (Main.PlayerRole[target.PlayerId].RoleTeam() != Team.Crewmate)
                {
                    __instance.RpcMurderPlayer(__instance, true);
                    return false;
                }
            return true;

        }
    }

    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    class BeginCrewmatePatch
    {
        public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            if (Main.PlayerRole[PlayerControl.LocalPlayer.PlayerId].RoleTeam() == Team.Neutral)
            {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                teamToDisplay = soloTeam;
            }
        }
        public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            CustomRoles role = Main.PlayerRole[PlayerControl.LocalPlayer.PlayerId];
            _ = ColorUtility.TryParseHtmlString(role.GetRoleColor(), out Color rc);
            __instance.TeamTitle.text = Translator.GetColorString(role);
            __instance.TeamTitle.color = rc;
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    class GameStartManagerUpdate
    {
        public static void Prefix(GameStartManager __instance)
        {
            __instance.MinPlayers = 1;
        }
    }
}

