using System.Collections.Generic;

using static ContinuetoEvolve.RoleHelper;

namespace ContinuetoEvolve
{
    class Translator
    {
        static Dictionary<CustomRoles, string> jp;

        public static void LoadTranslation()
        {
            jp = new Dictionary<CustomRoles, string>()
            {
                {CustomRoles.Crewmate,"クルーメイト"},
                {CustomRoles.Engineer,"エンジニア"},
                {CustomRoles.Scientist,"科学者"},
                {CustomRoles.GuardianAngel,"守護天使"},
                {CustomRoles.Impostor,"インポスター"},
                {CustomRoles.Shapeshifter,"シェイプシフター"},
                {CustomRoles.Sheriff,"シェリフ"}
            };
        }
        public static string GetString(CustomRoles cr)
        {
            if (jp == null) LoadTranslation();
            if (!jp.ContainsKey(cr)) return cr.ToString();
            return jp[cr];
        }
        public static string GetColorString(CustomRoles cr) => $"<color={cr.GetRoleColor()}>{GetString(cr)}</color>";
    }
}