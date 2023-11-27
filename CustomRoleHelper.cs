using System.Collections.Generic;
using System.Linq;
using System;
using AmongUs.GameOptions;
using UnityEngine;

using ContinuetoEvolve.Patches;

namespace ContinuetoEvolve
{
    public static class RoleHelper
    {
        public static Dictionary<CustomRoles, string> RoleColors;
        public enum CustomRoles
        {
            //Vanilla
            Crewmate,
            Engineer,
            Scientist,
            GuardianAngel,
            Impostor,
            Shapeshifter,
            //MOD
            Sheriff,
        }
        public enum VanillaRoles
        {
            Crewmate,
            Engineer,
            Scientist,
            GuardianAngel,
            Impostor,
            Shapeshifter
        }
        public enum Team
        {
            Crewmate,
            Impostor,
            Neutral
        }
        public static RoleTypes RoleType(this CustomRoles cr)
            => cr switch
            {
                CustomRoles.Impostor => RoleTypes.Impostor,
                CustomRoles.Shapeshifter => RoleTypes.Shapeshifter,
                _ => RoleTypes.Crewmate
            };
        public static Team RoleTeam(this CustomRoles cr)
            => cr switch
            {
                CustomRoles.Impostor or
                CustomRoles.Shapeshifter => Team.Impostor,
                _ => Team.Crewmate
            };

        public static void RoleLoad()
        {
            RoleColors = new Dictionary<CustomRoles, string>()
            {
                //Vanilla
                {CustomRoles.Crewmate, "#00ffff"},
                {CustomRoles.Engineer, "#00ffff"},
                {CustomRoles.Scientist, "#00ffff"},
                {CustomRoles.GuardianAngel, "#00ffff"},
                {CustomRoles.Impostor, "#ff0000"},
                {CustomRoles.Shapeshifter, "#ff0000"},
                //Mod
                {CustomRoles.Sheriff,"#f4ff00"}
            };
        }
        public static string GetRoleColor(this CustomRoles role) => RoleColors.ContainsKey(role) ? RoleColors[role] : "#ffffff";
        public static bool IsVanillaRole(this CustomRoles role)
        {
            foreach (var r in Enum.GetValues(typeof(VanillaRoles)))
            {
                if (r.ToString() == $"{role}") return true;
            }
            return false;
        }

        public static List<CustomRoles> GetActiveRole()
        {
            List<CustomRoles> roles = new();
            foreach (var role in Enum.GetValues(typeof(CustomRoles)).Cast<CustomRoles>())
            {
                if (role.IsVanillaRole()) continue;
                for (var i = 0; i < CustomOption.options.Count; i++)
                {
                    CustomOption option = CustomOption.options[i];
                    Debug.Log($"{option.name == Translator.GetColorString(role)} {option.name}-{Translator.GetColorString(role)}");
                    if (option.name == Translator.GetColorString(role))
                    {
                        i++;
                        option = CustomOption.options[i];
                        Debug.Log($"{option.GetSelection()}");
                        for (var i2 = 0; i2 <= option.GetSelection(); i2++)
                            roles.Add(role);
                    }
                }
            }
            foreach (var r in roles) Debug.Log($"r: {r}");
            return roles;
        }
    }
}