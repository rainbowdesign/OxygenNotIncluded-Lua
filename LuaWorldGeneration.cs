using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Harmony;
using MoonSharp.Interpreter;
using Klei.CustomSettings;
using ProcGenGame;
using ProcGen;


namespace LuaScriptingEngine
{
    //todo finish and test
    public static class LuaWorldGeneration
    {

        #region worldgeneration
        static int msizewidth = -1;
        static int msizeheight = -1;
        public static bool SetMapSize(int width, int height)
        {
            msizewidth = width;
            msizeheight = height;
            return true;
        }

        [HarmonyPatch(typeof(GridSettings), "Reset")]
        static class setmapsize
        {
            public static void Prefix(ref int width, ref int height)
            {
                if (msizewidth != -1) width = msizewidth;
                if (msizeheight != -1) width = msizeheight;
            }
        }
        static string worldnamen;
        static List<string> newtraits = new List<string>();
        static List<string> removetraits = new List<string>();

        public static bool AddMapTraits(List<string> traits)
        {
            newtraits = traits; return true;
        }
        public static bool RemoveMapTraits(List<string> traits)
        {
            removetraits = traits; return true;
        }
        public static string GetWorldName()
        {
            return worldnamen;
        }

        //[HarmonyPatch(typeof(WorldGenSettings), MethodType.Constructor, new Type[] { typeof(string), typeof(List<string>) })]
        static class overwriteworldtraits
        {
            public static void Prefix(ref string worldname, ref List<string> traits)
            {
                worldnamen = worldname;
                if (traits != null) foreach (var i in newtraits) { traits.Add(i); }
                if (traits != null) foreach (var i in removetraits) { traits.Remove(i); }
            }
        }
        static MutatedWorldData mworldData;
        static MutatedWorldData modifiedworldData;
        //todo[HarmonyPatch(typeof(SettingsCache), "CloneInToNewWorld", new Type[] { typeof(MutatedWorldData) })]
        static class overwriteworldgen
        {
            public static void Prefix(ref MutatedWorldData worldData)
            {
                mworldData = worldData;
                ScriptingCore.OnEvent("worlddata");
                if (modifiedworldData != null) worldData = modifiedworldData;
            }
        }
        public static MutatedWorldData GetWorldData()
        {
            return mworldData;
        }
        public static bool SetWorldData(MutatedWorldData worldData)
        {
            modifiedworldData = worldData; return true;
        }
        #endregion
    }
}
