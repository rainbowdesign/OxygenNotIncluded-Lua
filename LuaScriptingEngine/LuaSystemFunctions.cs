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
            ScriptingCore.InfoLog(debuglogtext);
            return true;
        }
        public static bool ClassHelp()
        {
            string fieldlist = " Use this strings with ClassHelpString " + "\n";
            foreach (var i in ScriptingCore.classhelpdict)
            {
                fieldlist += i.Key + " : " + i.Value + "\n";
            }
            ScriptingCore.InfoLog(fieldlist);
            return true;
        }
        public static Type ClassHelpString(string classname)
        {

            Type t = LuaInterface.GetType(classname);
            if (t == null)
            {
                try
                {
                    t = LuaInterface.GetType(ScriptingCore.classhelpdict[classname]);
                }
                catch { }
            }if (t == null) { ScriptingCore.InfoLog(" ClassHelperString " + classname + " Was not found. "); return null; }
            ClassHelpType(t); return t;
        }
        public static Type ClassHelpType(Type t)
        {

            var d = new LuaInterface.luahelper();
            //FieldInfo[] result = (FieldInfo[]) typeof(LuaInterface.luahelper).GetMethod("GetFields").MakeGenericMethod(t).Invoke(d, null);
            FieldInfo[] result = t.GetFields();
            // var fieldNames = result.Select(field => field.Name)                                        .ToList();

            string debuglogtext = " Luahelp: This lists Field names of the given class.\n";
            foreach (var i in result)
            {
                debuglogtext += i.Name + "\n";
            }
            ScriptingCore.InfoLog(debuglogtext);
            return t;
        }

        public static object convert(object i,FieldInfo f)
        {
            return Convert.ChangeType(i, f.FieldType);
        }

        public static FieldInfo AccessPrivateField( string type, string fieldname)
        {
            return AccessTools.Field(LuaInterface.GetType( type), fieldname);
        }
        public static bool SetPrivateField(string typei, string fieldname,GameObject go, object value)
        {
            var type = LuaInterface.GetType(typei);
            var field = AccessTools.Field(type, fieldname);
            field.SetValue(go.GetComponent(type),convert(value,field ));
            return true;
        }
        public static object GetPrivateField(string typei, string fieldname, GameObject go)
        {
            var type = LuaInterface.GetType(typei);
            var field = AccessTools.Field(type, fieldname);
            return field.GetValue(go.GetComponent(type));
        }
        public static Component LuaGetComponent(GameObject go, string componentname)
        {
            Type t = LuaInterface.GetType(componentname);
            if (t == null)
            {
                LuaInterface.RegisterType(componentname);
                t = LuaInterface.GetType(componentname);
            }
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
        public static FieldInfo[] GetFields(string typename) { return LuaInterface.GetType(typename).GetFields(); }
        public static bool ListFields(Type typename)
        {
            string fieldlist = " List all fields of " + typename;
            foreach (var i in typename.GetFields())
            {
                fieldlist += i.Name + " : " + i.GetValue(i).ToString();
            }
            ScriptingCore.InfoLog(fieldlist);
            return true;
        }
        public static bool ListFieldsString(string typename)
        {
            string fieldlist = " List all fields of " + typename;
            foreach (var i in LuaInterface.GetType(typename).GetFields())
            {
                fieldlist += i.Name + " : " + i.GetValue(i).ToString();
            }
            ScriptingCore.InfoLog(fieldlist);
            return true;
        }

        static public Dictionary<string, string > typelist = new Dictionary<string, string>();
        static public bool MakeEasyTypes(string fileName)
        {
            if (fileName == null) { ScriptingCore.InfoLog(" ClassHelperString " + fileName + " Filename is null. "); return false; }
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(fileName);
            }
            catch (Exception e) { ScriptingCore.InfoLog(" RegisterAllTypes " + e.Message); return false; }

            foreach (Type type in assembly.GetTypes())
            {
                typelist[type.Name]=type.AssemblyQualifiedName;
            }
            return true;
        }
        static public bool ListAllTypes(string fileName)
        {
            if (fileName == null) { ScriptingCore.InfoLog(" ClassHelperString " + fileName + " Filename is null. "); return false; }
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(fileName);
            }
            catch (Exception e) { ScriptingCore.InfoLog(" RegisterAllTypes " + e.Message); return false; }

            foreach (Type type in assembly.GetTypes())
            {
                ScriptingCore.InfoLog(type.AssemblyQualifiedName);
            }
            return true;
        }
        static public bool RegisterAllTypes(string fileName)
        {
            if (fileName == null) { ScriptingCore.InfoLog(" ClassHelperString " + fileName + " Filename is null. "); return false; }
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(fileName);
            }
            catch (Exception e) { ScriptingCore.InfoLog(" RegisterAllTypes " + e.Message); return false; }

            foreach (Type type in assembly.GetTypes())
            {
                LuaInterface.RegisterType(type.AssemblyQualifiedName);
            }
            return true;
        }
        #endregion
    }
}
