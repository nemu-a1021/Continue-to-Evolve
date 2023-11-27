//using ContinuetoEvolve;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using static ContinuetoEvolve.Translator;

namespace ContinuetoEvolve.Patches
{
    //TheOtherRolesAUさん本当にありがとうございます！！！
    public class CustomOptionHolder
    {
        public static string[] rates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };
        public static string[] ratesModifier = new string[] { "1", "2", "3" };
        public static string[] presets = new string[] { "プリセット 1", "プリセット 2", "プリセット 3", "プリセット 4", "プリセット 5" };
        public static string[] offon = new string[] { "オフ", "オン" };

        public static CustomOption presetSelection;

        public static CustomOption NotVital;
        public static CustomOption NotAdmin;
        public static CustomOption NotReport;
        public static CustomOption NotButton;

        public static CustomOption crewmateRolesCountMax;
        public static CustomOption neutralRolesCountMax;
        public static CustomOption impostorRolesCountMax;


        internal static Dictionary<byte, byte[]> blockedRolePairings = new();

        public static string cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static void LoadOptions()
        {
            presetSelection = CustomOption.Create(0, CustomOption.CustomOptionType.General, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "Preset"), presets, null, true);
            int id = 0;
            foreach (var r in Enum.GetValues(typeof(RoleHelper.CustomRoles)).Cast<RoleHelper.CustomRoles>())
            {
                if (r.IsVanillaRole()) continue;
                var ro = GetOptionType(r.RoleTeam());
                id++;
                var co = CustomOption.Create(id, ro, GetColorString(r), offon, null, true);
                id++;
                CustomOption.Create(id, ro, "r", 1f, 1f, 15f, 1f, co);
            }
        }
        public static CustomOption.CustomOptionType GetOptionType(RoleHelper.Team team)
        {
            if (team == RoleHelper.Team.Crewmate) return CustomOption.CustomOptionType.Crewmate;
            if (team == RoleHelper.Team.Impostor) return CustomOption.CustomOptionType.Impostor;
            if (team == RoleHelper.Team.Neutral) return CustomOption.CustomOptionType.Neutral;
            return CustomOption.CustomOptionType.General;
        }
    }

}