using  ContinuetoEolve;
using System.Collections.Generic;
using UnityEngine;
using static Rewired.Utils.Classes.Utility.ObjectInstanceTracker;
using UnityEngine.Networking.Types;
using static UnityEngine.ParticleSystem.PlaybackState;
using Types =  ContinuetoEolve.CustomOption.CustomOptionType;
using System.Diagnostics;

namespace  ContinuetoEolve
{
    //TheOtherRolesAUさん本当にありがとうございます！！！
    public class CustomOptionHolder
    {
        public static string[] rates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };
        public static string[] ratesModifier = new string[] { "1", "2", "3" };
        public static string[] presets = new string[] { "プリセット 1", "プリセット 2", "プリセット 3", "プリセット 4", "プリセット 5" };

        public static CustomOption presetSelection;

        public static CustomOption NotVital;
        public static CustomOption NotAdmin;
        public static CustomOption NotReport; 
        public static CustomOption NotButton;

        public static CustomOption crewmateRolesCountMax;
        public static CustomOption neutralRolesCountMax;
        public static CustomOption impostorRolesCountMax;


        internal static Dictionary<byte, byte[]> blockedRolePairings = new Dictionary<byte, byte[]>();

        public static string cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
     
        }
    }
