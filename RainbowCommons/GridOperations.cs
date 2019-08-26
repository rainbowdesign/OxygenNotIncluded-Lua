using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;
using Harmony;
using UnityEngine;
using Klei.CustomSettings;
using ProcGen;

namespace Co
{
    public static class MapManipulate
    {

        public static float GetCellValue(int cellNumber, string TargetStringID)
        {
            if (TargetStringID == "ElementIdx") return Grid.ElementIdx[cellNumber];
            if (TargetStringID == "Temperature") return Grid.Temperature[cellNumber];
            if (TargetStringID == "Mass") return Grid.Mass[cellNumber];
            if (TargetStringID == "DiseaseCount") return Grid.DiseaseCount[cellNumber];
            return 0;
        }
        public static bool RevealCell(int cellNumber, int newValue)
        {
            Grid.Reveal(cellNumber, (byte)newValue);
            return true;
        }
        public static bool AddToCell(int cellNumber, string TargetStringID, float newValue)
        {
            ModifyCell(cellNumber, TargetStringID, GetCellValue(cellNumber, TargetStringID) + newValue);
            return true;
        }
        public static bool ModifyCell(int cellNumber, string TargetStringID, float newValue)
        {
            if (TargetStringID == "ElementIdx") ModifyCell(cellNumber, (int)newValue, Grid.Temperature[cellNumber], Grid.Mass[cellNumber], Grid.DiseaseIdx[cellNumber], Grid.DiseaseCount[cellNumber]);
            if (TargetStringID == "Temperature") ModifyCell(cellNumber, Grid.ElementIdx[cellNumber], newValue, Grid.Mass[cellNumber], Grid.DiseaseIdx[cellNumber], Grid.DiseaseCount[cellNumber]);
            if (TargetStringID == "Mass") ModifyCell(cellNumber, Grid.ElementIdx[cellNumber], Grid.Temperature[cellNumber], newValue, Grid.DiseaseIdx[cellNumber], Grid.DiseaseCount[cellNumber]);
            if (TargetStringID == "DiseaseCount") ModifyCell(cellNumber, Grid.ElementIdx[cellNumber], Grid.Temperature[cellNumber], Grid.Mass[cellNumber], Grid.DiseaseIdx[cellNumber], (int)newValue);
            return true;
        }
        public static void ModifyCell(int gamecell, int elementidx, float temperature, float mass, byte diseaseid, int diseasecount)
        {
            if ((double)temperature < 0.0 || 10000.0 < (double)temperature)
            {
                Debug.Log("Modifycelltofailed: int" + gamecell + " int" + elementidx + "float" + temperature + "float" + mass + " byte" + diseaseid + " disease name " + Db.Get().Diseases.GetIndex(diseaseid) + " int" + diseasecount);
                return;
            }
            if ((double)temperature == 0.0 && (double)mass > 0.0 && elementidx >= 0)
            {
                Debug.Log("Modifycelltofailed: int" + gamecell + " int" + elementidx + "float" + temperature + "float" + mass + " byte" + diseaseid + " disease name " + Db.Get().Diseases.GetIndex(diseaseid) + " int" + diseasecount);
                return;
            }
            SimMessages.ModifyCell(gamecell, elementidx, temperature, mass, diseaseid, diseasecount, SimMessages.ReplaceType.Replace, false, -1);
        }

        public static Dictionary<string, WorldTrait> worldtraitsaddtogame = new Dictionary<string, WorldTrait>();
        public static List<WorldTrait> worldtraitsaddtomap = new List<WorldTrait>();
        public static List<WorldTrait> worldtraitsinmap = new List<WorldTrait>();
        [HarmonyPatch(typeof(SettingsCache), "LoadFiles", null)]
        public static class addtraitstogame
        {
            public static void Prefix(ProcGen.MutatedWorldData __instance)
            {
                FieldInfo info = typeof(SettingsCache).GetField("traits", BindingFlags.NonPublic | BindingFlags.Static);
                object value = info.GetValue(null);
                Dictionary<string, WorldTrait> traits = (Dictionary<string, WorldTrait>)value;
                traits.Merge(worldtraitsaddtogame);
                info.SetValue(null, traits);
            }
        }

        [HarmonyPatch(typeof(ProcGen.MutatedWorldData), "ApplyTrait", null)]
        public static class addtraits
        {
            public static void Prefix(ProcGen.MutatedWorldData __instance)
            {
                foreach (var trait in worldtraitsaddtomap)
                {
                    MethodInfo dynMethod = __instance.GetType().GetMethod("ApplyTrait", BindingFlags.NonPublic | BindingFlags.Instance);
                    dynMethod.Invoke(__instance, new object[] { trait });
                }
            }
        }
        [HarmonyPatch(typeof(ProcGen.MutatedWorldData), "ApplyTraits", null)]
        public static class gettraits
        {
            public static void Prefix(ProcGen.MutatedWorldData __instance)
            {
                worldtraitsinmap = __instance.traits.Cloneobj();
            }
        }
        public static WorldTrait newworldtrait()
        {
            return new WorldTrait();
        }
        public static List<int> randomtiles(List<int> baselist, int tileamount)
        {
            List<int> returnlist = new List<int>();
            while (returnlist.Count < tileamount)
            {
                int randomtile = baselist.RandomElement();
                if (!returnlist.Contains(randomtile)) returnlist.Add(randomtile);
            }
            return returnlist;
        }
        public static List<int> randomtilespercent(List<int> baselist, int tileamountpercent)
        {
            List<int> returnlist = new List<int>();
            int tileamount = (int)(returnlist.Count * (tileamountpercent / 0.01f));
            while (returnlist.Count < tileamount)
            {
                int randomtile = baselist.RandomElement();
                if (!returnlist.Contains(randomtile)) returnlist.Add(randomtile);
            }
            return returnlist;
        }
        public static bool CompareBiome(string biome, int Cell)
        {
            var zone = Game.Instance.world.zoneRenderData.GetSubWorldZoneType(Cell);

            string stringValue = Enum.GetName(typeof(ProcGen.SubWorld.ZoneType), zone);
            //Debug.Log(" CompareBiome " + stringValue + biome + biome.Equals(stringValue));
            if (biome.Equals(stringValue)) return true;
            else return false;
        }
        public static List<int> GetAllCells()
        {
            var l = new List<int>() { }; foreach (var i in Grid.CellCount.Enumerate()) l.Add(i);
            return l;
        }
        public static List<int> SubstractCells(List<int> BaseList, List<int> SubstractList)
        {
            foreach (var i in BaseList) if (SubstractList.Contains(i)) BaseList.Remove(i);
            return BaseList;
        }
        public static List<int> MergetCells(List<int> BaseList, List<int> MergeList)
        {
            foreach (var i in BaseList) if (!MergeList.Contains(i)) BaseList.Add(i);
            return BaseList;
        }
        public static bool HasObject(int Cell, int layer) { if (Grid.Objects[Cell, layer] == (UnityEngine.Object)null) return false; else return true; }
    }
    public class MapRegions
    {
        public static Dictionary<string, MapRegions> InstanceList = new Dictionary<string, MapRegions>();
        public Dictionary<string, List<int>> MapStringToRegionDict = new Dictionary<string, List<int>>();
        public List<int> MapBiomeList = new List<int>();
        public static MapRegions NewMapRegions() { return new MapRegions(); }//instancename must be completely unique in all scripts if you want to use it later
        public MapRegions Register(string instancename) { var m = this; if (!InstanceList.ContainsKey(instancename)) InstanceList.Add(instancename, m); return m; }
        private string[] lines;
        private Dictionary<int, string[]> positions = new Dictionary<int, string[]>();
        public Dictionary<int, string> mappedpositions = new Dictionary<int, string>();
        int startpos = 0;
        public string area;
        public Dictionary<string, List<int>> MapStringToRegion(string areai, int startposi)
        {
            area = areai;
            startpos = startposi;
            ParseString();
            ParseLine();
            MapRegion();
            return MapStringToRegionDict;
        }


        string biome = "";
        private List<int> biomecellssearch = new List<int>();
        private List<int> biomecellssearchcp = new List<int>();


        static public BaseLocation GetBaseLocationi()
        {
            if (worldi == null || worldi.defaultsOverrides == null || worldi.defaultsOverrides.baseData == null)
                return SettingsCache.defaults.baseData;
            DebugUtil.LogArgs((object)string.Format("World '{0}' is overriding baseData", (object)worldi.name));
            return worldi.defaultsOverrides.baseData;
        }
        public static ProcGen.World worldi;
        /*
        [HarmonyPatch(typeof(ProcGen.WorldGenSettings), "world", null)]
        public static class getworld
        {
            public static void Prefix(ProcGen.MutatedWorldData __instance)
            {
                worldi = __instance.world;
            }
        }*/
        static public int GetBaseLocation()
        {
            return GetBaseLocationi().bottom;
        }
        public Queue<int> q = new Queue<int>();
        public void CompareBiome(string biomename, List<int> tilelist)
        {
            foreach (var i in tilelist)
            {
                if (MapManipulate.CompareBiome(biome, i)) MapBiomeList.Add(i);
            }
        }
        public void MapBiome(int root)
        {
            biome = Enum.GetName(typeof(ProcGen.SubWorld.ZoneType), Game.Instance.world.zoneRenderData.GetSubWorldZoneType(root));
            q.Enqueue(root);
            while (q.Count > 0)
            {
                int Cell = q.Dequeue();
                BiomeSearchValidate(Grid.CellAbove(Cell));
                BiomeSearchValidate(Grid.CellBelow(Cell));
                BiomeSearchValidate(Grid.CellLeft(Cell));
                BiomeSearchValidate(Grid.CellRight(Cell));
                BiomeSearchValidate(Grid.CellDownLeft(Cell));
                BiomeSearchValidate(Grid.CellDownRight(Cell));
                BiomeSearchValidate(Grid.CellUpLeft(Cell));
                BiomeSearchValidate(Grid.CellUpRight(Cell));
            }
        }
        private void BiomeSearchValidate(int Cell)
        {
            if (!MapBiomeList.Contains(Cell))
            {
                if (MapManipulate.CompareBiome(biome, Cell))
                {
                    //Debug.Log(" CompareBiome success adding: " + Cell);
                    q.Enqueue(Cell);
                    MapBiomeList.Add(Cell);
                }
            }
        }

        private void ParseString()
        {
            lines = area.Split('#');
        }

        private void ParseLine()
        {
            int count = 0;
            foreach (var line in lines)
            {
                string[] pos = line.Split(',');
                positions[count] = pos;
                MapLine(count++);
            }

        }
        int linepos = 0;
        private void MapLine(int linenr)
        {
            if (linenr == 0) linepos = startpos;
            else linepos = Grid.CellBelow(linepos);
            int pos = linepos;
            foreach (var positionstring in positions[linenr])
            {
                if (mappedpositions.ContainsKey(pos)) { Debug.Log("mappedpos already contains line " + pos); continue; }
                mappedpositions.Add(pos, positionstring);
                pos = Grid.CellRight(pos);
            }

        }
        private void MapRegion()
        {
            foreach (var line in mappedpositions)
            {
                if (!MapStringToRegionDict.ContainsKey(line.Value)) MapStringToRegionDict.Add(line.Value, new List<int>());
                MapStringToRegionDict[line.Value].Add(line.Key);
            }

        }
    }

    public class ElementOperations
    {
        public Dictionary<int, int> foundpos = new Dictionary<int, int>();
        public Dictionary<int, int> foundpos2 = new Dictionary<int, int>();
        public static Dictionary<int, int> allpos = new Dictionary<int, int>();
        public bool ActiveOnly = false;
        System.Random randominst = new System.Random();
        public bool coroutineRunning = false;
        float oldtime = 0;

        public void framestart()
        {
            oldtime = Co.Helpers.stopwatch.ElapsedMilliseconds;
        }
        public float frameend()
        {
            return (-1 * (oldtime - Co.Helpers.stopwatch.ElapsedMilliseconds));
        }
        public bool framefinished(float frames = 1)
        {
            //ModHelper.ExternalFiles.Log("framefinished " + oldtime + " nt "+ ExternalFiles.stopwatch.ElapsedMilliseconds+ "sucess" + ((oldtime - ExternalFiles.stopwatch.ElapsedMilliseconds) < -50), "");

            if ((oldtime - Co.Helpers.stopwatch.ElapsedMilliseconds) < -50 * frames) return true;
            return false;
        }
        /*
		public System.Collections.IEnumerable Coroutine()
		{
			coroutineRunning = true;
			framestart();
			while (coroutineRunning)
			{
				//radiumpos.Clear();
				for (int i = 0; i < GridSize(); i++)
				{
					if (framefinished()) yield return null;
					if (randominst.NextDouble() > (chanceinpercent / 100)) continue;
					if ((ElementLoader.elements[(int)Grid.ElementIdx[i]].idx == (element) ))
					{
						if (ActiveOnly)
						{
							if (!ElementLoader.elements[(int)Grid.ElementIdx[i]].disabled)
							{
								foundpos[i] = 0;
							}
						}
						else { }
						foundpos[i] = 0;
					}
				}
				coroutineRunning = false;
			}
		}*/

        bool issolid; bool isliquid; bool isgas;
        void internalelementfinder()
        {
            //ExternalFiles.Log("Elementfinder " + element.id + " revealedtilesonly "+ revealedtilesonly+ " chanceinpercent "+ chanceinpercent+ " searchbiome "+ searchbiome+ " targetlayer " + targetlayer);
            //ExternalFiles.Log("tmpfinder ", "");
            foundpos.Clear();
            foundpos2.Clear();
            coroutineRunning = true;
            int gridsize = Grid.CellCount;
            bool foundelementestate = false;
            issolid = false; isliquid = false; isgas = false;
            if (elementi == "SOLID") { issolid = true; }
            if (elementi == "LIQUID") { isliquid = true; }
            if (elementi == "GAS") { isgas = true; }
            for (int i = 0; i < gridsize; i++)
            {
                //ExternalFiles.Log("targetlayer" + targetlayer);
                //ExternalFiles.Log(" "+(Grid.Objects[i, targetlayer].GetComponent<PrimaryElement>().Element != element));
                if (randominst.NextDouble() > (chanceinpercent / 100)) continue;
                //Debug.Log(element.name + i+ " compares " + Grid.Element[i].id + " " + (element.id) + "  "+(Grid.Element[i].id != (element.id))+ "targetlayer" + targetlayer);
                //if ((ElementLoader.elements[(int)Grid.ElementIdx[i]].idx == (ElementLoader.FindElementByName(element).idx) || (element == "ALL")))
                //if (revealedtilesonly) if (Grid.Revealed[i] == false) continue;
                if (searchbiome)
                {
                    if (biome == "exposedtospace") if (!(Game.Instance.world.zoneRenderData.GetSubWorldZoneType(i) == ProcGen.SubWorld.ZoneType.Space && (UnityEngine.Object)Grid.Objects[i, 2] == (UnityEngine.Object)null)) continue;
                        else if (Game.Instance.world.zoneRenderData.GetSubWorldZoneType(i) != Helpers.ToEnum<ProcGen.SubWorld.ZoneType>(biome)) continue;
                }
                //ExternalFiles.Log(i + " biome success" );
                // if(Game.Instance.world.zoneRenderData.GetSubWorldZoneType(i) == ProcGen.SubWorld.ZoneType.Space && (UnityEngine.Object)Grid.Objects[i, 2] == (UnityEngine.Object)null) is exposed to space
                if (targetlayer <= -1)
                {
                    if (Grid.Foundation[i] || Grid.HasLadder[i]) { continue; }
                    if (issolid) { if (Grid.IsSolidCell(i)) foundelementestate = true; }
                    if (isliquid) { if (Grid.IsLiquid(i)) foundelementestate = true; }
                    if (isgas) { if (Grid.IsGas(i)) foundelementestate = true; }
                    if (foundelementestate == false) if (Grid.Element[i].id != (element.id)) continue;
                }
                //ExternalFiles.Log("targetlayer2" + targetlayer + element.name + " " ) ;
                //ExternalFiles.Log(element.name + i + " compares success" + Grid.Element[i].id + " " + (element.id) + "  " + (Grid.Element[i].id == (element.id)));
                if (targetlayer > -1)
                {
                    if (Grid.Objects[i, targetlayer] != null) foundpos2[i] = targetlayer;
                }
                if (targetlayer <= -1)//if(!Grid.Solid[i])
                {
                    foundpos[i] = targetlayer;
                    //ExternalFiles.Log("Elementfinder success" + element + " Biome: " + Grid.CellToXY(i).x + " , " + Grid.CellToXY(i).Y);// + ProcGenGame.WorldGen.OverworldCells[i].node.type);// ProcGenGame.WorldGen.GetSubWorldType(Grid.CellToXY(i)));
                }                                                                                                   //Grid.Objects[i,(int)ObjectLayer.Building].GetComponent<Building>().Def(int)ObjectLayer.Building

            }
            try
            {
                finishedcycle = GameClock.Instance.GetCycle() + GameClock.Instance.GetCurrentCycleAsPercentage();
            }
            catch { }
            coroutineRunning = false;
            coroutinesuccess = true;
            Debug.Log(" end elementfinder " + element.id);
            return;
            //Thread.Sleep(0);
        }
        public bool revealedtilesonly = true;
        public int targetlayer = -1;
        public Element element;
        public string elementi;
        public string biome;
        public float getcycle = 0;
        public float finishedcycle = 0;
        public bool searchbiome = false;
        public float chanceinpercent;
        public Thread thread;
        public bool coroutinesuccess = false;


        public bool FindElement(string elementi, float chanceinpercenti = 100, int targetlayeri = -1, string biomei = "PASS")
        {
            if (coroutineRunning) return false;
            if (coroutinesuccess) return true;
            if (elementi == "PASS") return true;
            if (biomei != "PASS" || string.IsNullOrEmpty(biomei)) { searchbiome = true; biome = biomei; }
            this.elementi = elementi;
            targetlayer = targetlayeri;
            //ExternalFiles.DebugLog(" FindElement  " + elementi + " cr " + coroutineRunning + " ch " + chanceinpercenti + " ro " + revealedtilesonly + " tl " + targetlayeri + " bio " + biomei + searchbiome);
            if (elementi == "ALL")
            {
                if (allpos.Count < 1)
                {
                    for (int i = 0; i < Grid.CellCount; i++)
                    {
                        allpos[i] = -1;
                    }
                }
                foundpos = allpos;
                return true;
            }
            else { try { element = ElementLoader.FindElementByName(elementi); } catch { Debug.Log("failed to find the element " + elementi); return true; } }
            chanceinpercent = chanceinpercenti;
            //if (thread == null)
            //{
            //internalelementfinder();
            //return;
            thread = new System.Threading.Thread(new ThreadStart(this.internalelementfinder));
            thread.Priority = System.Threading.ThreadPriority.BelowNormal;
            thread.Start();
            return false;
            //}
            //else { thread.Resume(); }
            //ExternalFiles.Log("tried to start thread sucess: " + thread.ThreadState.ToString());
            //this.tmpfinder();
            //Global.Instantiate(Global.FindObjectOfType<GameObject>()).AddComponent<GridOperations>();
            //this.StartCoroutine ("Coroutine");
        }
        public Dictionary<int, int> FetchElement(bool reset = true)
        {
            try
            {
                getcycle = GameClock.Instance.GetCycle() + GameClock.Instance.GetCurrentCycleAsPercentage();
                // ExternalFiles.DebugLog(" getelement " + element.name + " Returncycle " + getcycle + " amount " + foundpos.Count + " amount2 " + foundpos2.Count + " coroutineRunning " + coroutineRunning, "");

            }
            catch { }

            if (reset) coroutinesuccess = false;
            if (coroutineRunning)
            {
                Debug.Log(" getelement coroutine still running ");
                return new Dictionary<int, int>();
            }
            else
            {
                if (targetlayer > -1)
                {
                    foreach (var i in foundpos2.Keys)
                    {
                        try { if (Grid.Objects[i, targetlayer].GetComponent<PrimaryElement>().Element.id == element.id) { foundpos[i] = targetlayer; } } catch { }
                    }
                }
            }
            var rfoundpos = new Dictionary<int, int>(foundpos);
            foundpos = new Dictionary<int, int>();
            Debug.Log(" getelement success count " + foundpos.Count);
            return rfoundpos;
        }

        /*public void GridReplace(string elementtoreplace, string elementistarget, float chanceinpercent=100)
		{
			foreach (var i in FindElement(elementtoreplace).Keys)
			{
				if (randominst.NextDouble() < (chanceinpercent / 100))
				{
					SimMessages.ReplaceElement(i, ElementLoader.FindElementByName(elementistarget).id, ((new CellElementEvent("Scenario", "Scenario", true, true)) as CellElementEvent), randominst.Next(500, 1000));
				}
			}
		}*/
    }
    public class BuildingOperations
    {
        Dictionary<int, int> foundpos = new Dictionary<int, int>();
        Dictionary<int, int> foundpos2 = new Dictionary<int, int>();
        public static Dictionary<int, int> allpos = new Dictionary<int, int>();
        public bool ActiveOnly = false;
        System.Random randominst = new System.Random();
        public bool coroutineRunning = false;
        float oldtime = 0;
        static public BuildingOperations NewBuildingOperation()
        {
            return new BuildingOperations();
        }

        public void framestart()
        {
            oldtime = Co.Helpers.stopwatch.ElapsedMilliseconds;
        }
        public float frameend()
        {
            return (-1 * (oldtime - Co.Helpers.stopwatch.ElapsedMilliseconds));
        }
        public bool framefinished(float frames = 1)
        {
            //ModHelper.ExternalFiles.Log("framefinished " + oldtime + " nt "+ ExternalFiles.stopwatch.ElapsedMilliseconds+ "sucess" + ((oldtime - ExternalFiles.stopwatch.ElapsedMilliseconds) < -50), "");

            if ((oldtime - Co.Helpers.stopwatch.ElapsedMilliseconds) < -50 * frames) return true;
            return false;
        }
        List<GameObject> deflist;
        void InternalFindBuildings()
        {
            deflist = new List<GameObject>();
            foreach (var i in Grid.CellCount.Enumerate())
            {
                if (randominst.NextDouble() > (chanceinpercent / 100)) continue;
                if (searchbiome)
                {
                    if (biome == "exposedtospace") if (!(Game.Instance.world.zoneRenderData.GetSubWorldZoneType(i) == ProcGen.SubWorld.ZoneType.Space && (UnityEngine.Object)Grid.Objects[i, 2] == (UnityEngine.Object)null)) continue;
                        else if (Game.Instance.world.zoneRenderData.GetSubWorldZoneType(i) != Helpers.ToEnum<ProcGen.SubWorld.ZoneType>(biome)) continue;
                }
                if (Grid.Objects[i, targetlayer] != null) if (name == Grid.Objects[i, targetlayer].GetComponent<BuildingComplete>().Def.PrefabID) deflist.Add(Grid.Objects[i, targetlayer]);
            }
            try
            {
                finishedcycle = GameClock.Instance.GetCycle() + GameClock.Instance.GetCurrentCycleAsPercentage();
            }
            catch { }
            coroutineRunning = false;
            coroutinesuccess = true;
            return;
        }
        public int targetlayer = -1;
        public string name;
        public string biome;
        public float getcycle = 0;
        public float finishedcycle = 0;
        public bool searchbiome = false;
        public float chanceinpercent;
        public Thread thread;
        public bool coroutinesuccess = false;


        public bool FindBuilding(string namei, int targetlayeri = -1, float chanceinpercenti = 100, string biomei = "PASS")
        {
            if (coroutineRunning) return false;
            if (coroutinesuccess) return true;
            if (biomei != "PASS") searchbiome = true;
            targetlayer = targetlayeri;
            name = namei;
            //ExternalFiles.DebugLog(" FindElement  " + elementi + " cr " + coroutineRunning + " ch " + chanceinpercenti + " ro " + revealedtilesonly + " tl " + targetlayeri + " bio " + biomei + searchbiome);
            chanceinpercent = chanceinpercenti;
            //if (thread == null)
            //{
            InternalFindBuildings();
            //return;
            thread = new System.Threading.Thread(new ThreadStart(this.InternalFindBuildings));
            thread.Priority = System.Threading.ThreadPriority.BelowNormal;
            thread.Start();
            return false;
        }
        public List<GameObject> FetchBuildings(bool reset = true)
        {
            try
            {
                getcycle = GameClock.Instance.GetCycle() + GameClock.Instance.GetCurrentCycleAsPercentage();
            }
            catch { }
            if (reset) coroutinesuccess = false;
            if (coroutineRunning) return new List<GameObject>();
            return deflist;
        }
    }
}