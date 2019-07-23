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
    class LuaSystemFunctions
    {
        #region System funtions
        public static bool Help()
        {
            string debuglogtext = " Luahelp: This lists Functions and classes, if you want to know more about a class do ClassHelp(classname) \n Functionname : Helptext\n";
            foreach (var i in ScriptingCore.helpdict)
            {
                debuglogtext += i.Key + " : " + i.Value + "\n";
            }
            ScriptingCore.DebugLog(debuglogtext);
            return true;
        }
        public static bool ClassHelp()
        {
            string fieldlist = " Use this strings with ClassHelpString " + "\n";
            foreach (var i in ScriptingCore.classhelpdict)
            {
                fieldlist += i.Key + " : " + i.Value + "\n";
            }
            ScriptingCore.DebugLog(fieldlist);
            return true;
        }
        public static bool ClassHelpString(string classname)
        {

            Type t = Type.GetType(classname);
            if (t == null)
            {
                try
                {
                    t = Type.GetType(ScriptingCore.classhelpdict[classname]);
                }
                catch { }
            }
            if (t == null) { ScriptingCore.DebugLogError(" ClassHelperString " + classname + " Was not found. "); return false; }
            var d = new LuaInterface.luahelper();
            //FieldInfo[] result = (FieldInfo[]) typeof(LuaInterface.luahelper).GetMethod("GetFields").MakeGenericMethod(t).Invoke(d, null);
            FieldInfo[] result = t.GetFields();
            // var fieldNames = result.Select(field => field.Name)                                        .ToList();

            string debuglogtext = " Luahelp: This lists Field names of the given class.\n";
            foreach (var i in result)
            {
                debuglogtext += i.Name + "\n";
            }
            ScriptingCore.DebugLog(debuglogtext);
            return true;
        }
        public static Component GetComponent(GameObject go, string componentname)
        {
            Type t = Type.GetType(componentname);
            if (t == null)
            {
                ScriptingCore.DebugLogError("The Component " + componentname + " could not be found!");
            }
            if (go == null)
            {
                ScriptingCore.DebugLogError("The GameObject is Null!");
            }
            return go.GetComponent(t);
        }
        public static Storage GetComponentStorage(GameObject go)
        {
            if (go == null)
            {
                ScriptingCore.DebugLogError("The GameObject is Null!");
            }
            return go.AddOrGet < Storage >();
        }

        public static Db GetDatabase() { return Db.Get(); }
        public static FieldInfo[] GetFields(string typename) { return Type.GetType(typename).GetFields(); }
        public static bool ListFields(Type typename)
        {
            string fieldlist = " List all fields of " + typename;
            foreach (var i in typename.GetFields())
            {
                fieldlist += i.Name + " : " + i.GetValue(i).ToString();
            }
            ScriptingCore.DebugLog(fieldlist);
            return true;
        }
        public static bool ListFieldsString(string typename)
        {
            string fieldlist = " List all fields of " + typename;
            foreach (var i in Type.GetType(typename).GetFields())
            {
                fieldlist += i.Name + " : " + i.GetValue(i).ToString();
            }
            ScriptingCore.DebugLog(fieldlist);
            return true;
        }
        static public bool ListAllTypes(string fileName)
        {
            if (fileName == null) { ScriptingCore.DebugLogError(" ClassHelperString " + fileName + " Filename is null. "); return false; }
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(fileName);
            }
            catch (Exception e) { ScriptingCore.DebugLogError(" RegisterAllTypes " + e.Message); return false; }

            foreach (Type type in assembly.GetTypes())
            {
                ScriptingCore.DebugLog(type.AssemblyQualifiedName);
            }
            return true;
        }
        static public bool RegisterAllTypes(string fileName)
        {
            if (fileName == null) { ScriptingCore.DebugLogError(" ClassHelperString " + fileName + " Filename is null. "); return false; }
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(fileName);
            }
            catch (Exception e) { ScriptingCore.DebugLogError(" RegisterAllTypes " + e.Message); return false; }

            foreach (Type type in assembly.GetTypes())
            {
                LuaInterface.RegisterType(type.AssemblyQualifiedName);
            }
            return true;
        }
        #endregion
    }
}
