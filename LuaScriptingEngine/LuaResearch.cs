using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaCore
{
    class LuaResearch
    {
        #region research
        public static bool AddResearch(string research)
        {
            foreach (Tech techtoadd in Db.Get().Techs.resources)
            {
                if (techtoadd.Id == research)
                {
                    var res = Research.Instance.Get(techtoadd);
                    var req = Research.Instance.Get(techtoadd).tech.requiredTech;
                    foreach (var i in req) AddResearch(i.Id);
                    if (res == null)
                    {
                        ScriptingCore.DebugLogError("AddResearch did not find: " + research);
                        return false;
                    }
                    res.Purchased();
                    return true;
                }
            }
            ScriptingCore.DebugLogError("AddResearch did not find: " + research);
            return false;
        }
        public static bool SetResearchCost(string research, string costtype, float costamount)
        {
            if (Db.Get().Techs.resources == null) return false;
            foreach (Tech techtoadd in Db.Get().Techs.resources)
            {
                if (techtoadd.Id == research)
                {
                    var res = Research.Instance.Get(techtoadd);

                    if (res.tech.costsByResearchTypeID.ContainsKey(costtype))
                        res.tech.costsByResearchTypeID[costtype] = costamount;
                    return true;
                }
            }
            ScriptingCore.DebugLogError("SetResearchCost did not find: " + research);
            return false;
        }
        public static bool SetResearchCostMultiplierAll(float costmultiply)
        {
            if (Db.Get().Techs.resources == null) return false;
            foreach (Tech techtoadd in Db.Get().Techs.resources)
            {
                var res = Research.Instance.Get(techtoadd);
                foreach (var i in res.tech.costsByResearchTypeID.Keys.ToList()) res.tech.costsByResearchTypeID[i] *= costmultiply;
            }
            return true;
        }
        public static bool SetResearchCostMultiplier(string costtype, float costmultiply)
        {
            if (Db.Get().Techs.resources == null) return false;
            foreach (Tech techtoadd in Db.Get().Techs.resources)
            {
                var res = Research.Instance.Get(techtoadd);
                if (res.tech.costsByResearchTypeID.ContainsKey(costtype))
                    res.tech.costsByResearchTypeID[costtype] *= costmultiply;
            }
            return true;
        }
        public static bool ListResearchsWithTypes()
        {
            string debuglogstring = " ListResearchsWithTypes ";
            foreach (Tech techtoadd in Db.Get().Techs.resources)
            {
                debuglogstring += techtoadd.Id + "\n";
                var res = Research.Instance.Get(techtoadd);
                foreach (var i in res.tech.costsByResearchTypeID.Keys.ToList())
                    debuglogstring += i + res.tech.costsByResearchTypeID[i] + "\n";
            }
            ScriptingCore.InfoLog(debuglogstring);
            return true;
        }
        public static string GetRandomResearch()
        {
            return Db.Get().Techs.resources.RandomElement().Id;
        }
        public static string GetRandomAvailableResearch()
        {
            int counter = 0;
            while (counter++ < 100)
            {
                var tech = Db.Get().Techs.resources.RandomElement();
                if (tech.ArePrerequisitesComplete()&& !tech.IsComplete()) return tech.Id;
            }
            ScriptingCore.DebugLogError("GetRandomAvailableResearch failed returning InteriorDecor ");
            return "InteriorDecor";
        }
        #endregion
    }
}
