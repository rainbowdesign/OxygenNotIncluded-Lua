using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Harmony;
using MoonSharp.Interpreter;

namespace LuaScriptingEngine
{
    public class EntryPoints
    {
        public static bool loaded= false;

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class __pregeneratedbuildings
        {
            public static void Prefix()
            {
                ScriptingCore.ScriptInit();
                ScriptingCore.DebugLog("preloadbuildings");
                ScriptingCore.OnEvent("preloadbuildings");
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class __generatedbuildings
        {
            public static void Postfix()
            {
                ScriptingCore.DebugLog("loadbuildings");
                ScriptingCore.OnEvent("loadbuildings");
            }
        }


        [HarmonyPatch(typeof(MainMenu), "OnSpawn", null)]
        public static class Loadhook
        {
            public static void Postfix()
            {
                if (!loaded)
                {
                    ScriptingCore.OnEvent("loaded");
                    loaded=true;
                }
            }
        }
        [HarmonyPatch(typeof(WattsonMessage), "OnActivate", null)] //Suggestions on how to do this properly?
        public static class newgame
        {
            public static void Prefix()
            {
                ScriptingCore.OnEvent("newgame");
            }
        }
        [HarmonyPatch(typeof(WattsonMessage), "OnSpawn", null)] //Suggestions on how to do this properly?
        public static class maploaded
        {
            public static void Prefix()
            {
                ScriptingCore.OnEvent("maploaded");
            }
        }
        [HarmonyPatch(typeof(Game), "LateUpdate", null)]
        public static class Updatehook
        {
            public static void Prefix()
            {
                ScriptingCore.OnEvent("directupdate");
                delayedupdate();
                delayedupdategame();
            }
        }
        [HarmonyPatch(typeof(MainMenu), "Update", null)]
        public static class Updatehook2
        {
            public static void Prefix()
            {
                ScriptingCore.OnEvent("directupdate");
                delayedupdate();
            }
        }
        public static void onchangegame()
        {
            ScriptingCore.OnEvent("onchangegame");
        }
        static int delay=0;
        public static void delayedupdategame()
        {
            int delaydelta = delay++ % 100;
            if (delaydelta == 1)
            {
                ScriptingCore.OnEvent("updategame");
            }
            if (delaydelta == 20)
            {
                onchangegame();
            }
        }
        public static void delayedupdate()
        {
            int delaydelta = delay++ % 100;
            if (delaydelta == 1)
            {
                ScriptingCore.OnEvent("update");
            }
            if (delaydelta == 10)
            {
                onchange();
            }
            if (delaydelta == 20)
            {
                newday();
            }
            if (delaydelta == 30)
            {
                noon();
            }
        }
        static int lastcyclenumber=0;


        public static void onchange()
        {
            ScriptingCore.OnEvent("onchange");
        }
            public static void newday()
        {
            try
            {
                if (lastcyclenumber != LuaFunctions.GetCycleNumber())
            {
                ScriptingCore.OnEvent("newday");
                lastcyclenumber = LuaFunctions.GetCycleNumber();
                }
            }
            catch { }
        }
        static int lastcyclenumber2 = 0;
        public static void noon()
        {
            try
            {
                if (lastcyclenumber2 != LuaFunctions.GetCycleNumber())
                {
                    if (LuaFunctions.GetCycleTime() < 50)
                    {
                        ScriptingCore.OnEvent("noon");
                        lastcyclenumber2 = LuaFunctions.GetCycleNumber();
                    }
                }
            }
            catch { }
        }
    }
}