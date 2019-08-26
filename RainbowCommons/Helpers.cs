using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;
using Klei.CustomSettings;
namespace Co
{
    public static class Helpers
    {
        public static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        [HarmonyPatch(typeof(GeneratorConfig), "CreateBuildingDef", null)]
        static class GeneratorConfigMOd
        {
            public static void Postfix(BuildingDef __result)
            {
                stopwatch.Start();
            }
        }
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
}