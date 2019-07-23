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
    public static class LuaFunctions
    {
        #region functions
        public static Element GetElement(string elementname)
        {
            return ElementLoader.FindElementByName(elementname);
        }
        public static bool AddRecipe(string Fabricatorid, List<string> ingredients, List<float> ingredientsamount, List<string> results, List<float> resultsamount, float recipetime = 40f)
        {
            int ingreds = ingredients.Count;
            if (ingredientsamount.Count < ingredients.Count) ingreds = ingredientsamount.Count;
            int resus = results.Count;
            if (resultsamount.Count < results.Count) resus = resultsamount.Count;
            List<ComplexRecipe.RecipeElement> ingredientslist = new List<ComplexRecipe.RecipeElement>();
            List<ComplexRecipe.RecipeElement> resultslist = new List<ComplexRecipe.RecipeElement>();
            foreach (var i in ingreds.Enumerate()) {
                ingredientslist.Add(new ComplexRecipe.RecipeElement(ElementLoader.FindElementByName(ingredients.ElementAt(i)).tag, ingredientsamount.ElementAt(i)));
            }
            foreach (var i in resus.Enumerate())
            {
                resultslist.Add(new ComplexRecipe.RecipeElement(ElementLoader.FindElementByName(ingredients.ElementAt(i)).tag, ingredientsamount.ElementAt(i)));
            }
            Co.Add.Recipe(Fabricatorid, ingredientslist.ToArray(), resultslist.ToArray(), recipetime);
            return true;
        }
        #endregion
        #region cycles


        public static int GetCycleNumber()
        {
            return GameClock.Instance.GetCycle()+1;
        }
        public static float GetCycleTime()
        {
            return GameClock.Instance.GetCurrentCycleAsPercentage();
        }

        #endregion
        #region settings //Todo finish region
        static public bool HasSetting(string setting)
        {
            return CustomGameSettings.Instance.QualitySettings.ContainsKey(setting);
        }

        static public bool RegisterSetting(SettingConfig setting)
        {
            if (!tsettings.Contains(setting))
            {
                tsettings.Add(setting);
                return true;
            }
            return false;
        }
        static List<SettingConfig> tsettings = new List<SettingConfig>();
        [HarmonyPatch(typeof(CustomGameSettings), nameof(CustomGameSettings.AddSettingConfig))]
        static public class custominfosettings
        {
            public static void Postfix(CustomGameSettings __instance, SettingConfig config)
            {

                foreach (var tsetting in tsettings)
                {
                    __instance.AddSettingConfig(tsetting);
                }
            }
        }
        static public ListSettingConfig NewListSetting()        {  return new ListSettingConfig("", "", "", new List<SettingLevel>(), "", "");        }
        static public ToggleSettingConfig NewToggleSetting(SettingLevel offlevel ,  SettingLevel onlevel) { 
            return new ToggleSettingConfig("", "", "", offlevel, onlevel, "", "");
        }
        static public List<SettingLevel> NewSettingList()        {            return new  List<SettingLevel>();        }
        static public SettingLevel NewSettingLevel(string id, string label, string tooltip)
        {
            return new SettingLevel(id, label, tooltip);
        }

        #endregion
    }
}