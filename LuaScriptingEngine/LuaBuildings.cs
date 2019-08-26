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

namespace LuaCore
{

    public static class LuaBuildings
    {

        #region buildings
        public static GameObject GetBuildingGameObject(string prefab_id)
        {
            GameObject returndef = Assets.GetPrefab(prefab_id);
            if (returndef == null) ScriptingCore.DebugLogError("GameObject for " + prefab_id + " could not be found!");
            return returndef;
        }
        public static BuildingDef GetBuildingDef(GameObject prefab_id)
        {
            BuildingDef returndef = prefab_id.GetComponent<BuildingDef>();
            if (returndef == null) ScriptingCore.DebugLogError("GetBuildingDef for " + prefab_id + " could not be found!");
            return returndef;
        }
        public static PrimaryElement GetBuildingPrimaryElement(GameObject prefab_id)
        {
            PrimaryElement returndef = prefab_id.GetComponent<PrimaryElement>();
            if (returndef == null) ScriptingCore.DebugLogError("GetBuildingDef for " + prefab_id + " could not be found!");
            return returndef;
        }

        public static BuildingDef GetBuildingDef(string prefab_id)
        {
            BuildingDef returndef = Assets.GetBuildingDef(prefab_id);
            if (returndef == null) ScriptingCore.DebugLogError("GetBuildingDef for " + prefab_id + " could not be found!");
            return returndef;
        }
        public static void GetConfigureBuildingTemplate(string prefab_id)
        {
        }
        public static GameObject GetBuildingComplete(string prefab_id)
        {
            return GetBuildingDef(prefab_id).BuildingComplete;
        }
        public static GameObject GetBuildingPreview(string prefab_id)
        {
            return GetBuildingDef(prefab_id).BuildingPreview;
        }
        public static GameObject GetBuildingUnderConstruction(string prefab_id)
        {
            return GetBuildingDef(prefab_id).BuildingUnderConstruction;
        }

 
        static Dictionary<IBuildingConfig, BuildingDef> configTable = new Dictionary<IBuildingConfig, BuildingDef>();
        static public Dictionary<string, newbuilding> buildingtable = new Dictionary<string, newbuilding>();

       
        public class newbuilding : IBuildingConfig
        {
            BuildingDef clonebuildingdef;
            BuildingDef thisbuildingdef;
            public string ID = "NewID";
            public string NAME = "New Name";
            public string DESCRIPTION = "Description.";
            public string EFFECT = "Effect.";
            public string TECH = "SolidTransport";
            public string PLANCATEGORY = "Conveyance";
            public ScriptingInstance instance; //todo find a way to get the instance.
            public newbuilding()            {        }
            public void buildingbasics(string name, string description, string effect, string tech, string plancategory)
            {
                NAME = name; DESCRIPTION = description; EFFECT = effect; TECH = tech; PLANCATEGORY = plancategory;
            }

            public BuildingDef newbuildingdef(int sizex, int sizey, string anim, int hitpoints, float constructiontime, float[] constructionmass, string[] constructionmaterials, float meltingpoint)
            {
                thisbuildingdef = BuildingTemplates.CreateBuildingDef(ID, sizex, sizey, anim, hitpoints, constructiontime, constructionmass, constructionmaterials, meltingpoint, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.PENALTY.TIER2, TUNING.NOISE_POLLUTION.NOISY.TIER0, 0.2f);
                return thisbuildingdef;
            }
            public void finishbuilding()
            {
                BuildingConfigManager.Instance.RegisterBuilding(this);
                Co.Add.BuildingTech(ID, TECH);
                Co.Add.BuildingPlan(ID, NAME, DESCRIPTION, EFFECT, PLANCATEGORY);
            }
            
            public void clonebuildindef(string buildingname, string clonename)
            {
                if (TagManager.GetProperName(clonename) != null)
                {
                    ScriptingCore.DebugLogError(" The building cannot be cloned, the name " + clonename + " is already used.");
                    return;
                }
                foreach (var i in configTable)
                {
                    if (i.Value.PrefabID == buildingname)
                    {
                        clonebuildingdef = i.Value;
                    }
                }
                if (clonebuildingdef == null)
                {
                    ScriptingCore.DebugLogError(" The configTable did not contain, the name " + clonename + ".");
                    return;
                }
            }
            
            public override BuildingDef CreateBuildingDef()
            {
                if (thisbuildingdef==null) newbuildingdef(1,1, "heavywatttile_conductive_kanim", 10,10,new float[] { 1 }, new string[] {"steel" },10);
                if (instance == null) return thisbuildingdef; 
                instance.NewScript(ID, "loaded", "buildings", "CreateBuildingDef", thisbuildingdef);
                ScriptingCore.OnEvent(ID);
                ScriptingCore.LuaDebugLog( "CreateBuildingDef " +ID);
                return thisbuildingdef;
            }
            GameObject ConfigureBuildingTemplategameo;

            public GameObject ConfigureBuildingTemplatego()
            {  return ConfigureBuildingTemplategameo;
                }
            public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
            {
                if (instance == null) return;
                ConfigureBuildingTemplategameo =go;
                instance.NewScript(ID, "loaded", "buildings", "ConfigureBuildingTemplate",go,prefab_tag, thisbuildingdef);
                //GameObject.Instantiate(clonebuildingdef.BuildingPreview);
            }
            public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
            {
                if (instance == null) return;
                instance.NewScript(ID, "loaded", "buildings", "DoPostConfigurePreview", go, def);
            }
            public override void DoPostConfigureUnderConstruction(GameObject go)
            {
                if (instance == null) return;
                instance.NewScript(ID, "loaded", "buildings", "DoPostConfigureUnderConstruction", go,  thisbuildingdef);
            }
            public override void DoPostConfigureComplete(GameObject go)
            {
                if(instance==null) return;
                instance.NewScript(ID, "loaded", "buildings", "DoPostConfigureComplete", go,  thisbuildingdef);
            }
        }





        /* [HarmonyPatch(typeof(BuildingConfigManager), nameof(CustomGameSettings.AddSettingConfig))]
         static public class __BuildingConfigManager
         {
             public static void Postfix(CustomGameSettings __instance, SettingConfig config)
             {

             }
         }*/



        /*public void RegisterBuilding(IBuildingConfig config)
        {
            BuildingDef buildingDef = config.CreateBuildingDef();
            this.configTable[config] = buildingDef;
            GameObject go = UnityEngine.Object.Instantiate<GameObject>(this.baseTemplate);
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)go);
            go.GetComponent<KPrefabID>().PrefabTag = buildingDef.Tag;
            go.name = buildingDef.PrefabID + "Template";
            go.GetComponent<Building>().Def = buildingDef;
            go.GetComponent<OccupyArea>().OccupiedCellsOffsets = buildingDef.PlacementOffsets;
            if (buildingDef.Deprecated)
                go.GetComponent<KPrefabID>().AddTag(GameTags.DeprecatedContent, false);
            config.ConfigureBuildingTemplate(go, buildingDef.Tag);
            buildingDef.BuildingComplete = BuildingLoader.Instance.CreateBuildingComplete(go, buildingDef);
            bool flag = true;
            for (int index = 0; index < this.NonBuildableBuildings.Length; ++index)
            {
                if (buildingDef.PrefabID == this.NonBuildableBuildings[index])
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                buildingDef.BuildingUnderConstruction = BuildingLoader.Instance.CreateBuildingUnderConstruction(buildingDef);
                buildingDef.BuildingUnderConstruction.name = BuildingConfigManager.GetUnderConstructionName(buildingDef.BuildingUnderConstruction.name);
                buildingDef.BuildingPreview = BuildingLoader.Instance.CreateBuildingPreview(buildingDef);
                buildingDef.BuildingPreview.name += "Preview";
            }
            buildingDef.PostProcess();
            config.DoPostConfigureComplete(buildingDef.BuildingComplete);
            if (flag)
            {
                config.DoPostConfigurePreview(buildingDef, buildingDef.BuildingPreview);
                config.DoPostConfigureUnderConstruction(buildingDef.BuildingUnderConstruction);
            }
            Assets.AddBuildingDef(buildingDef);
        }
        */
        #endregion
    }
}
