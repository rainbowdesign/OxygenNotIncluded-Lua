using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Harmony;
using Klei.CustomSettings;
using MoonSharp.Interpreter;


namespace LuaCore
{
    public class ScriptingInstance
    {

        static public Dictionary<ScriptingInstance, List<ScriptingCore>> instancedictionary = new Dictionary<ScriptingInstance, List<ScriptingCore>>();
        public Dictionary<string, Co.MapRegions> InstanceList = new Dictionary<string, Co.MapRegions>();
        public string scriptpath;
        public static int scripts = 0;
        int instance = 0;
        public static Dictionary<int, ScriptingCore> scriptdictionary = new Dictionary<int, ScriptingCore>();

        public static List<string> luafolder = new List<string>();
        public static List<string> modfolder = new List<string>();
        public static void LuaSearcher()
        {
            foreach (var mod in Global.Instance.modManager.mods)
            {
                if (mod.is_subscribed) { //ScriptingCore.InfoLog("modfolder contains " + mod.file_source.GetRoot()  ) ;   
                    modfolder.Add( mod.file_source.GetRoot().Replace('\\','/'));}
            }
            
            string basefolder = System.IO.Path.Combine(Util.RootFolder(), "mods/");
            foreach (string file in System.IO.Directory.GetFiles(
                basefolder, "LuaEntryPoint.dll", SearchOption.AllDirectories))
            {
                if(!luafolder.Contains(Path.GetDirectoryName(file))) luafolder.Add(Path.GetDirectoryName(file.Replace('\\','/')));
            }

            foreach (var file in luafolder) {
        ScriptingCore.InfoLog("tryload " + file);
                if (modfolder.Contains(file))
                {
                    new LuaCore.ScriptingInstance(file);
                    //ScriptingCore.InfoLog("Started new Lua Scripting Instance " + file);
                }
                }
        }

        
        static bool started = false;
        public ScriptingInstance(string instancepath)
        {   scriptpath = instancepath;
            //Debug.Log(" ---- Lua sc Path ---- " + scriptpath); 
            instancedictionary.Add( this, new List<ScriptingCore>()); 
            ScriptIniti();
        }
        public bool initphase=true;
        public void ScriptIniti()
        {
            Debug.Log(" ---- Start Lua   ---- ");
            Debug.Log(" ---- Lua Version ---- " + "Version: BETA 0.82 " + " Date: 25.08.2019 ");
            Debug.Log(" ---- Lua Path ---- " + scriptpath);
            if (!started)
            {
                RegisterTypes();
                RegisterFunctions();
                RegisterInfo();
                started = true;
            }
            new ScriptingCore(this,"registration");
            new ScriptingCore(this, "init");
            new ScriptingCore(this, "console", "onchange");
            new ScriptingCore(this, "gameconsole", "onchangegame");initphase=false;
        }
        public int NewScript(string scriptfilen, string scriptexecutiontimesi = "loaded", string scriptfolderi = "", string scriptnamei = "start", params object[] parameteri)
        {
            new ScriptingCore(this, scriptfilen, scriptexecutiontimesi, scriptfolderi, scriptnamei, "", parameteri); return 0;
        }
        public int NewScript(string scriptfilen, string scriptexecutiontimesi = "loaded", string scriptfolderi = "", string scriptnamei = "start")
        {
            new ScriptingCore(this, scriptfilen, scriptexecutiontimesi, scriptfolderi, scriptnamei); return 0;
        }

        public string RemoveScript(string scriptfilen, string scriptfolderi = "", string scriptnamei = "start")
        {
            foreach (var n2 in scriptdictionary)
            {
                var n = n2.Value;
                if (n.scriptfolder == scriptfolderi && n.scriptfileraw == scriptfilen && n.scriptname == scriptnamei)
                    scriptdictionary.Remove(n2.Key);
            }
            return scriptfolderi;
        }

        public bool HasScript(string scriptfilen, string scriptfolder = "", string scriptnamei = "start")
        {
            foreach (var n in scriptdictionary.Values)
            {
                if (n.scriptfolder == scriptfolder && n.scriptfileraw == scriptfilen && n.scriptname == scriptnamei)
                {
                    return true;
                }
            }
            return false;
        }
        public string EnableScript(string scriptfilen, string scriptfolderi = "", string scriptnamei = "start")
        {
            foreach (var n2 in scriptdictionary)
            {
                var n = n2.Value;
                if (n.scriptfolder == scriptfolderi && n.scriptfileraw == scriptfilen && n.scriptname == scriptnamei)
                    if (n.tags.ContainsKey("disabled"))
                        n.tags.Remove("disabled");
            }
            return scriptfolderi;
        }
        public string DisableScript(string scriptfilen, string scriptfolderi = "", string scriptnamei = "start")
        {
            foreach (var n2 in scriptdictionary)
            {
                var n = n2.Value;
                if (n.scriptfolder == scriptfolderi && n.scriptfileraw == scriptfilen && n.scriptname == scriptnamei)
                    if (n.tags.ContainsKey("disabled"))
                        n.tags.Add("disabled", "");
            }
            return scriptfolderi;
        }
        public void RegisterInfo()
        {
            LuaInterface.AddHelpInfo(" ---- Lua Help Info ----", " here you will find the essential Lua infos");
            LuaInterface.AddHelpInfo("Register Type", "Call LuaScriptingEngine.LuaInterface.RegisterType<T>(string typename, string Helptext) ");

            LuaInterface.AddHelpInfo(" ---- C# Integration ----", " here are the basic infos if you want to add new functions and types");
            LuaInterface.AddHelpInfo("Start", " Add LuaScriptingEngine to your refereces of your project.");
            LuaInterface.AddHelpInfo("Register Function", " Call LuaScriptingEngine.LuaInterface.AddFunction(string Functionnameinlua, Delegate your function, string Helptext)");
            LuaInterface.AddHelpInfo("Delegate syntax", "Example: Func<string, string, string, string, int>)NewScript ");
            LuaInterface.AddHelpInfo("Register Type", "Call LuaScriptingEngine.LuaInterface.RegisterType<T>(string typename, string Helptext) ");

        }
        public void RegisterFunctions()
        {
            LuaInterface.AddHelpInfo(" ---- Functions ----", " The functions available in lua");
            LuaInterface.AddHelpInfo(" --- Registration --- ", "");
            LuaInterface.AddFunction("NewScript", (Func<string, string, string, string, int>)NewScript, "Starts a new script, takes the arguments as strings parameters are: (filename without ending,execute at event of this string (default once on menu),subfoldername(default  for none), functionname in the file(default start)");
            LuaInterface.AddFunction("RemoveScript", (Func<string, string, string, string>)RemoveScript, "Removes a script from execution, takes the arguments as strings: (filename without ending,subfoldername(default  for none), functionname in the file(default start)");
            LuaInterface.AddFunction("HasScript", (Func<string, string, string, bool>)HasScript, "Returns true if the script is running, takes the arguments as strings: (filename without ending,subfoldername(default  for none), functionname in the file(default start)");
            LuaInterface.AddFunction("MakeEasyTypes", (Func<string, bool>)LuaSystemFunctions.MakeEasyTypes, "Makes types in the class useable only with the name instead the fully qualified accessor.  ");
            LuaInterface.AddFunction("RegisterType", (Func<string, Type>)LuaInterface.RegisterType, "Registers a type for use. Every type you want to access from Lua must be registered. You need to qualify it fully examples in ClassHelp()");
            LuaInterface.AddFunction("RegisterAllTypes", (Func<string, bool>)LuaSystemFunctions.RegisterAllTypes, "Registers all types from the given assembly Possible Dangerous and performancekiller use careful.");

            LuaInterface.AddHelpInfo("SafeMode", "Enables SafeMode to avoid crashes SafeMode(string true/false).");
            LuaInterface.AddFunction("DebugLog", (Func<string, bool>)ScriptingCore.InfoLog, "prints a line in the debug file.");

            LuaInterface.AddHelpInfo(" --- Help Info --- ", "");
            LuaInterface.AddFunction("AddHelpInfo", (Func<string, string, bool, bool>)LuaInterface.AddHelpInfo, "Adds to the helpfile AddHelpInfo(string info 1, info 2)");
            LuaInterface.AddFunction("Help", (Func<bool>)LuaSystemFunctions.Help, "Prints the Helpinfo for all functions.");
            LuaInterface.AddFunction("ListAllTypes", (Func<string, bool>)LuaSystemFunctions.ListAllTypes, "Lists all types from the given assembly.");
            LuaInterface.AddFunction("ClassHelp", (Func<bool>)LuaSystemFunctions.ClassHelp, "Lists all strings to use with ClassHelpString.");
            LuaInterface.AddFunction("ClassHelpType", (Func<Type, Type>)LuaSystemFunctions.ClassHelpType, "Lists all fields of a class takes a Type.");
            LuaInterface.AddFunction("ClassHelpString", (Func<string, Type>)LuaSystemFunctions.ClassHelpString, "Lists all fields of a class takes a string.");
            LuaInterface.AddFunction("LuaGetComponent", (Func<GameObject, string, Component>)LuaSystemFunctions.LuaGetComponent, "Returns the BuildingDef from a string to manipulate a building.");
            LuaInterface.AddFunction("AccessPrivateField", (Func<string, string, System.Reflection.FieldInfo>)LuaSystemFunctions.AccessPrivateField, "Returns the Fieldinfo from string type, string fieldname to manipulate a building.");
            LuaInterface.AddFunction("SetPrivateField", (Func<string, string, GameObject, object, bool>)LuaSystemFunctions.SetPrivateField, "Sets the value from the given field.");
            LuaInterface.AddFunction("GetPrivateField", (Func<string, string, GameObject, object>)LuaSystemFunctions.GetPrivateField, "Returns the value from the given field.");
            LuaInterface.AddFunction("convert", (Func<object, System.Reflection.FieldInfo, object>)LuaSystemFunctions.convert, "Returns float from double.");
            //LuaInterface.AddFunction("GetComponentStorage", (Func<GameObject, Storage>)LuaSystemFunctions.GetComponentStorage, "Returns the Storage from a building. Not Working Todo");
            LuaInterface.AddFunction("GetDatabase", (Func<Db>)LuaSystemFunctions.GetDatabase, "Returns the database of Oxygen not Included to the script.");

            LuaInterface.AddHelpInfo(" --- Timings --- ", "");
            LuaInterface.AddFunction("GetCycleNumber", (Func<int>)LuaFunctions.GetCycleNumber, "Returns the cycle number");
            LuaInterface.AddFunction("GetCycleTime", (Func<float>)LuaFunctions.GetCycleTime, "Returns the cycle time of day");
            LuaInterface.AddFunction("GetElement", (Func<string, Element>)LuaFunctions.GetElement, "Takes the Element ID and Returns the Element");

            LuaInterface.AddHelpInfo(" --- Settings --- Todo test & Finish section", "");
            /* LuaInterface.AddFunction("NewListSetting", (Func< ListSettingConfig>)LuaFunctions.NewListSetting, "returns you a list setting.");
             LuaInterface.AddFunction("NewToggleSetting", (Func<SettingLevel, SettingLevel, ToggleSettingConfig>)LuaFunctions.NewToggleSetting, "Returns you a toggle setting.");
             LuaInterface.AddFunction("NewSettingLevel", (Func<string, string, string, SettingLevel >)LuaFunctions.NewSettingLevel, "Returns you a new setting level takesstring id, string label, string tooltip.");
             LuaInterface.AddFunction("RegisterSetting", (Func<SettingConfig,bool>)LuaFunctions.RegisterSetting, "Registers a setting.");
             LuaInterface.AddFunction("HasSetting", (Func<string,bool>)LuaFunctions.HasSetting, "Returns true if the Setting was selected by the player, is deleted on save / load.");
*/
             LuaInterface.AddHelpInfo(" --- Buildings --- ", " This functions help you to manipulate buildings.  ");
           //  LuaInterface.AddFunction("MakeOrGetBuilding", (Func<String, LuaBuildings.newbuilding>)LuaBuildings.MakeOrGetBuilding, "Instantiate and returns a building. Todo Finish and make working.");
            LuaInterface.AddFunction("GetBuildingGameObject", (Func<string, GameObject>)LuaBuildings.GetBuildingGameObject, "Returns the Building GameObject from a string to manipulate a building.");
            LuaInterface.AddFunction("GetBuildingDef", (Func<string, BuildingDef>)LuaBuildings.GetBuildingDef, "Returns the BuildingDef from a string to manipulate a building.");
            LuaInterface.AddFunction("GetBuildingComplete", (Func<string, GameObject>)LuaBuildings.GetBuildingComplete, "Returns the GameObject from the function GetBuildingComplete of a building.");
            LuaInterface.AddFunction("GetBuildingUnderConstruction", (Func<string, GameObject>)LuaBuildings.GetBuildingUnderConstruction, "Returns the GameObject from the function GetBuildingUnderConstruction of a building.");
            LuaInterface.AddFunction("GetBuildingPreview", (Func<string, GameObject>)LuaBuildings.GetBuildingPreview, "Returns the GameObject from the function GetBuildingPreview of a building.");
            //LuaInterface.AddFunction("AddRecipe", (Func< string , List<string> , List<float> , List<string> , List<float> , float, bool>)LuaFunctions.AddRecipe, "TODO: Test. Helps you to add a recipe to a building. string Fabricatorid, List<string> ingredients, List<float> ingredientsamount, List<string> results, List<float> resultsamount, float recipetime = 40f");


            LuaInterface.AddHelpInfo(" --- Research --- ", " Here you can check if the player has a research and you can manipulate the costs of a research. ");
            LuaInterface.AddFunction("AddResearch", (Func<string, bool>)LuaResearch.AddResearch, "Researches a Research (marks it finished) The research is provided as ID look in the Tech.txt");//exposing a function so it can be called from lua syntax is: Func<parameters, returntype>
            LuaInterface.AddFunction("SetResearchCost", (Func<string, string, float, bool>)LuaResearch.SetResearchCost, "sets the research costs of a research ( researchname, costtype, costamount)");
            LuaInterface.AddFunction("SetResearchCostMultiplierAll", (Func<float, bool>)LuaResearch.SetResearchCostMultiplierAll, "Multiplys the research costs of all  researches SetResearchCostMultiplierAll(float Multiplier)");
            LuaInterface.AddFunction("SetResearchCostMultiplier", (Func<string, float, bool>)LuaResearch.SetResearchCostMultiplier, "Multiplys the research costs of the given type of all  researches SetResearchCostMultiplierAll(string costtype, float Multiplier)");
            LuaInterface.AddFunction("ListResearchsWithTypes", (Func<bool>)LuaResearch.ListResearchsWithTypes, "Prints string for use with SetResearchCostMultiplier");
            LuaInterface.AddFunction("GetRandomResearch", (Func<string>)LuaResearch.GetRandomResearch, "returns a random research as string.");
            LuaInterface.AddFunction("GetRandomAvailableResearch", (Func<string>)LuaResearch.GetRandomAvailableResearch, "returns a random researchable research as string.");



            LuaInterface.AddHelpInfo(" --- MapSettings --- ", " Here you can set the map size and manipulate map traits.");
            LuaInterface.AddFunction("SetMapSize", (Func<int, int, bool>)LuaWorldGeneration.SetMapSize, "Sets the Mapsize (int widht,int height).");
            LuaInterface.AddFunction("AddMapTraits", (Func<List<string>, bool>)LuaWorldGeneration.AddMapTraits, "Adds Traits to the Map.");
            LuaInterface.AddFunction("RemoveMapTraits", (Func<List<string>, bool>)LuaWorldGeneration.RemoveMapTraits, "Removes Traits from the Map.");
            LuaInterface.AddFunction("GetWorldName", (Func<string>)LuaWorldGeneration.GetWorldName, "Gets the World Name.");
            LuaInterface.AddFunction("GetWorldData", (Func<ProcGen.MutatedWorldData>)LuaWorldGeneration.GetWorldData, "Returns the World data: rather complicated to work with.");
            LuaInterface.AddFunction("SetWorldData", (Func<ProcGen.MutatedWorldData, bool>)LuaWorldGeneration.SetWorldData, "Use your new worlddata as parameter.");


            LuaInterface.AddHelpInfo(" --- Gridoperations --- ", " Here you can measure the map and modify sections.");
            LuaInterface.AddFunction("NewMapRegions", (Func<Co.MapRegions>)Co.MapRegions.NewMapRegions, "Returns a new mapregion fron a string.");
            LuaInterface.AddFunction("GetCellValue", (Func<int, string, float>)Co.MapManipulate.GetCellValue, "Returns the value of a cell.");
            LuaInterface.AddFunction("AddToCell", (Func<int, string, float, bool>)Co.MapManipulate.AddToCell, "Adds the amount to the cell value.");
            LuaInterface.AddFunction("ModifyCell", (Func<int, string, float, bool>)Co.MapManipulate.ModifyCell, "Sets the cell to the value.");
            LuaInterface.AddFunction("RevealCell", (Func<int, int, bool>)Co.MapManipulate.RevealCell, "Sets the cell to the value.");
            //LuaInterface.AddFunction("GetBaseLocation", (Func<int>)Co.MapRegions.GetBaseLocation, "Gets the location of the base.");

            // LuaInterface.AddFunction("GetBuildingPrimaryElement", (Func< GameObject, PrimaryElement>)LuaBuildings.GetBuildingPrimaryElement, "Returns the Element from a Building on the map.");
            // LuaInterface.AddFunction("NewBuildingOperation", (Func<Co.BuildingOperations>)Co.BuildingOperations.NewBuildingOperation, "Returns a new building operation.");

        }
        public void RegisterTypes()
        {
            LuaInterface.AddHelpInfo(" ---- Lua Engine Types ----", " Types declared by Lua Engine ");
            LuaInterface.RegisterType<LuaBuildings.newbuilding>("newbuilding", "Type used to create a new building.");
            LuaInterface.RegisterType<Co.ElementOperations>("GridOperations", "A way to manipulate stuff on the map.");
            LuaInterface.RegisterType<Co.MapRegions>("MapRegions", "A way to get locations on the map.");

            LuaInterface.AddHelpInfo(" ---- Types ----", " To lua exposed types");
            LuaInterface.RegisterType<Assets>("Assets", "The core asset class here are as example buildingdefs stored.");
            LuaInterface.RegisterType<BuildingDef>("BuildingDef", "The core building class here you can set building values.");
            LuaInterface.RegisterType<Component>("Component", "Base type for all Components.");
            LuaInterface.RegisterType<Element>("Element", "The core element class here you can as example set high and lowtemp.");
            LuaInterface.RegisterType<ElementLoader>("ElementLoader", "The Elementloader has lists of all elements and helps to find them.");
            LuaInterface.RegisterType<GameClock>("GameClock", "The time measurement core. Most things are in GameClock.instance");
            LuaInterface.RegisterType<GameObject>("GameObject", "Unitys generic class for objects in the game very generic");
            LuaInterface.RegisterType<Grid>("Grid", "The Grid on which elements and gameobjects are set.");
            LuaInterface.RegisterTypeAdv<ProcGen.MutatedWorldData>("MutatedWorldData", "The world Data of the game.");
            LuaInterface.RegisterTypeAdv<SettingLevel>("SettingLevel", "The SettingLevels to make mapsettings.");
            LuaInterface.RegisterTypeAdv<ListSettingConfig>("ListSettingConfig", "Listsetting takes: (id,label,tooltip, List<SettingLevel>, normaldefault level(id), nosweat default (id)) .");
            LuaInterface.RegisterTypeAdv<ToggleSettingConfig>("ToggleSettingConfig", "Listsetting takes: (id,label,tooltip, SettingLevel Off, SettingLevel On, normaldefault level(id), nosweat default (id)) .");
            LuaInterface.RegisterTypeAdv<Db>("Db", "The database.");
            LuaInterface.RegisterTypeAdv<Type>("Type", "Help to get c# types.");

            LuaInterface.AddHelpInfo(" ---- Building Types ----", " To lua exposed types to control buildings");
            LuaInterface.RegisterType<ManualDeliveryKG>("ManualDeliveryKG", "Building ManualDeliveryKG");
            LuaInterface.RegisterType<ElementConverter>("ElementConverter", "Building ElementConverter");
            LuaInterface.RegisterType<ElementConsumer>("ElementConsumer", "Building ElementConsumer");
            LuaInterface.RegisterType<PassiveElementConsumer>("PassiveElementConsumer", "Building PassiveElementConsumer");
            LuaInterface.RegisterType<EnergyGenerator>("EnergyGenerator", "Building EnergyGenerator");
            LuaInterface.RegisterType<ElementEmitter>("ElementEmitter", "Building ElementEmitter");
            LuaInterface.RegisterType<ElementDropper>("ElementDropper", "Building ElementDropper");
            LuaInterface.RegisterTypeAdv<Storage>("Storage", "Building Storage");
            LuaInterface.RegisterTypeAdv<Klei.AI.AttributeModifier>("KleiAIAttributeModifier", "The modifier for attributes of elements (overheat as example)");
            LuaInterface.RegisterTypeAdv<System.Reflection.FieldInfo>("FieldInfo", "The Fieldinfo class from system.reflection");
        }
    }
    public class ScriptingCore
    {
        // not instances are not possible because they might not fully supported by moonsharp.
        // We have to stick with lua files for each mod.
        #region Init
        static public Dictionary<string,Delegate> functiondict= new Dictionary<string, Delegate>();
        static public Dictionary<string, string> helpdict = new Dictionary<string, string>();
        static public Dictionary<string, string> classhelpdict = new Dictionary<string, string>();
        static public Dictionary<string, DynValue> typedict = new Dictionary<string, DynValue>();
        static public Dictionary<string, Type> typedirectdict = new Dictionary<string, Type>();
        ScriptingInstance parent;
        static public bool debuginfo =false;
        

        public static void OnEvent(string eventtype)
        {
            foreach (var i in ScriptingInstance.scriptdictionary) { 
                i.Value.OnEventi(eventtype);
                }
                }
        public  void OnEventi(string eventtype)
        {
            //if( !eventtype.Contains("update")) 
            //ModDebugLog("OnEvent " + eventtype);
            foreach (var ev in ScriptingInstance.scriptdictionary.Keys.ToList())
            {
                var evValue= ScriptingInstance.scriptdictionary[ev];
                if (evValue.scriptexecutiontimes == eventtype)
                {
                    if (evValue.scriptexecutiontimes == "onchange"||evValue.scriptexecutiontimes == "onchangegame")
                    {
                        if(!File.Exists(evValue.scriptfile)) continue;
                        if (evValue.scriptCode.Equals(File.ReadAllText(evValue.scriptfile))) LuaDebugLog("onchange failed scriptfile is the same: "+evValue.scriptfile);
                        if (!evValue.scriptCode.Equals(File.ReadAllText(evValue.scriptfile))){
                            evValue.scriptCode= File.ReadAllText(evValue.scriptfile);
                            evValue.tags.Remove("disabled");
                            LuaDebugLog("onchange successful");
                        }
                    }
                    if (evValue.tags.ContainsKey("disabled")) continue;
                    LuaDebugLog("OnEvent " + eventtype + " enable the script " + evValue.scriptname );
                    if (evValue.scriptexecutiontimes == "onchange" || evValue.scriptexecutiontimes == "onchangegame")
                    {
                        evValue.EvalText(evValue.scriptCode);
                        evValue.tags.Add("disabled", "");
                    }
                    else evValue.EvalText(evValue.scriptCode);
                }
            }
        }
        public Dictionary<string , string > tags = new Dictionary<string, string>();
        public string scriptfolder;
        public string scriptfile;
        public string scriptfileraw;
        public string scriptname;
        public string scriptexecutiontimes;
        public string scriptCode;
        public object[] parameters;
        public ScriptingCore(ScriptingInstance parenti, string scriptfilen, string scriptexecutiontimesi = "loaded", string scriptfolderi = "", string scriptnamei = "start", params object[] parameteri)
        {
            parent= parenti;
            ScriptingCorei(scriptfilen, scriptexecutiontimesi , scriptfolderi , scriptnamei ,  parameteri);
        }
        public void ScriptingCorei(string scriptfilen, string scriptexecutiontimesi = "loaded", string scriptfolderi = "", string scriptnamei = "start",  params object[] parameteri)
        {
            Script.GlobalOptions.RethrowExceptionNested = true;
            scriptexecutiontimes = scriptexecutiontimesi;
            scriptfolder = scriptfolderi;
            parameters = parameteri;
            if (scriptnamei != null) { scriptname = scriptnamei; } else { scriptname = "start"; }
            if (scriptfilen != null) { scriptfileraw = scriptfilen; } else { scriptfileraw = "start"; }
            if (string.IsNullOrEmpty(scriptfolder)) scriptfile = parent.scriptpath + Path.DirectorySeparatorChar + scriptfilen + ".lua";
            else scriptfile = parent.scriptpath + Path.DirectorySeparatorChar + scriptfolder + Path.DirectorySeparatorChar + scriptfilen + ".lua";
            if (!File.Exists(scriptfile)){ if( !parent.initphase ) InfoLog("Tried to load a File that does not exists: " + scriptfile); 
                return; }
             scriptCode = File.ReadAllText(scriptfile);
            if(scriptexecutiontimes=="loaded")EvalText(scriptCode);
            else ScriptingInstance.scriptdictionary.Add(ScriptingInstance.scripts++, this);
            LuaDebugLog("Load Script " + scriptfileraw + " Script Execution "  + scriptexecutiontimes + " Scriptfolder " + scriptfolder + " scriptname " + scriptname);
            
        }

        #endregion
        #region lua
        public bool safemode= true;
        public delegate R Func<P0, P1, P2, P3, P4, P5, R>( P0 p0, P1 p1, P2 p2, P3 p3, P4 p4,  P5 p5);
        public void EvalText(string scriptCode)
        {
            InfoLog("start script " + scriptfile + " scriptname " + scriptname + " ## " + scriptCode);
            if (safemode)
            {
                try
                {
                    DoScript(scriptCode);
                }
                catch (ScriptRuntimeException ex)
                {
                    DebugLogError(ex.DecoratedMessage);
                }
            }
            else DoScript(scriptCode);
        }

        public void DoScript(string scriptCode)
        {//this is the Core of the scripting engine it does the actual work.

            Script script = new Script(); //Generating the script instance
            
            UserData.RegisterAssembly(); //registering the asseblys you need to add [MoonSharpUserData] to the data you want to expose more under http://www.moonsharp.org/objects.html
            

            foreach (var i in typedict)
            {
                script.Globals.Set(i.Key,  i.Value);
            }
            foreach (var i in typedirectdict)
            {
                script.Globals[i.Key] = i.Value;
            }
            script.Globals["SafeMode"] = (Func<string, bool>)SafeMode;
            foreach (var i in functiondict)
            {
                script.Globals[i.Key] = i.Value;
            }

            if (safemode)
            {
                try
                {
                    script.DoString(scriptCode); //The command to load scriptCode as module more under http://www.moonsharp.org/scriptloaders.html
                }
                catch (SyntaxErrorException ex)
                {
                    DebugLogError(ex.DecoratedMessage);
                    return;
                }
            }
            else { script.DoString(scriptCode); }


            if (safemode)
            {
                try
                {
                    DynValue res = script.Call(script.Globals[scriptname], parameters);
                }
                catch (ScriptRuntimeException ex)
                {
                    DebugLogError(ex.DecoratedMessage);
                }
            }
            else { DynValue res = script.Call(script.Globals[scriptname],parameters); }//Calling the function inside the code you should define a default value here More about dynvalue http://www.moonsharp.org/dynvalue.html

        }
        #endregion
        #region Basic Functions
        public bool SafeMode(string enabled)
        {
            if(enabled=="true") safemode = true;
            if (enabled == "false") safemode = false;
            return safemode;
        }
        public static bool InfoLog(string logentry)
        {
            Debug.Log("[LuaLog] " + logentry);
            return true;
        }
        public static bool DebugLogError(string logentry)
        {
            Debug.Log("[LuaError] " + logentry);
            return true;
        }
        public static bool LuaDebugLog(string logentry)
        {
            if(debuginfo==false)return false;
            Debug.Log("[LuaDebugLog] " + logentry);
            return true;
        }

        #endregion

        public LuaBuildings.newbuilding MakeOrGetBuilding(string id)
        {
            if (LuaBuildings.buildingtable.ContainsKey(id)) return LuaBuildings.buildingtable[id];
            var nb = new LuaBuildings.newbuilding();
            nb.instance = this.parent;
            nb.ID = id;
            LuaBuildings.buildingtable.Add(id, nb);
            return nb;
        }
    }
}
