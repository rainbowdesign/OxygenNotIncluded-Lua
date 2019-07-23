using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using Harmony;
using MoonSharp.Interpreter;



namespace LuaScriptingEngine
{
    public static class LuaInterface
    {
        public static void AddFunction(string Functionnameinlua, Delegate Function, string Helptext = "")
        {
            ScriptingCore.DebugLog("RegisterFunction: " + Functionnameinlua);
            if (ScriptingCore.functiondict.ContainsKey(Functionnameinlua)) ScriptingCore.DebugLogError("The type " + Functionnameinlua + " is already registered skipping it! ");
            else
            {
                ScriptingCore.functiondict.Add(Functionnameinlua, Function);
            }
            AddHelpInfo(Functionnameinlua, Helptext);
        }
        public static bool AddHelpInfo(string Functionnameinlua, string Helptext = "", bool log = true)
        {
            if (log) ScriptingCore.DebugLog("AddHelpInfo: " + Functionnameinlua);
            if (ScriptingCore.helpdict.ContainsKey(Functionnameinlua)) ScriptingCore.DebugLogError("The info " + Functionnameinlua + " is already registered skipping it! ");
            else
            {
                ScriptingCore.helpdict.Add(Functionnameinlua, Helptext);
            }
            return true;
        }
        public static bool AddHelpInfo<T>(string Functionnameinlua, string Helptext = "", bool log = true)
        {
            if (log) ScriptingCore.DebugLog("AddHelpInfo: " + Functionnameinlua);
            if (ScriptingCore.helpdict.ContainsKey(Functionnameinlua)) ScriptingCore.DebugLogError("The info " + Functionnameinlua + " is already registered skipping it! ");
            else
            {
                ScriptingCore.helpdict.Add(Functionnameinlua, Helptext);
                ScriptingCore.classhelpdict.Add(Functionnameinlua, typeof(T).AssemblyQualifiedName);
            }
            return true;
        }
        public class luahelper {

            public void RegisterType<T>(string typename, string Helptext = "") where T : new()
            {
                LuaInterface.RegisterType<T>(typename, Helptext);
            }
            public FieldInfo[] GetFields<T>() {
                return typeof(T).GetFields();
            }
        }
        
        public static bool RegisterType(string typename)
        {
            ScriptingCore.DebugLog("RegisterType: " + typename);
            Type t = Type.GetType(typename);
            if (t == null) { ScriptingCore.DebugLogError(" ClassHelperString " + typename + " Was not found. "); return false; }
            
            ScriptingCore.typedirectdict.Add(typename, t);
            if (ScriptingCore.classhelpdict.ContainsKey(typename)) ScriptingCore.DebugLogError("The info " + typename + " is already registered skipping it! ");
            else
            {
                ScriptingCore.classhelpdict.Add(typename, t.AssemblyQualifiedName);
            }
           // foreach (var i in Db.Get().Urges) { Debug.Log( i.Name );}
            /*
            var d = new luahelper();
            try
            {
                var result = typeof(luahelper).GetMethod("RegisterType").MakeGenericMethod(t);
                RegisterTypeAdv<result>(typename);
            }
            catch(Exception e) { ScriptingCore.DebugLogError("RegisterType "+ typename+ " failed with error: " + e.Message ); }
            */
            return true;
            //somehow convert the typename to the type
        }
        public static void RegisterType<T>(string typename, string Helptext = "") where T : new()
        {
            ScriptingCore.DebugLog("RegisterType: " + typename);
            UserData.RegisterType<T>();
            DynValue dynv = UserData.Create(new T());
            if (ScriptingCore.typedict.ContainsKey(typename)) ScriptingCore.DebugLogError("The type " + typename + " is already registered skipping it! ");
            else
            {
                ScriptingCore.typedict.Add(typename, dynv);
            }
            AddHelpInfo<T>(typename, "Class: " +Helptext, false);
        }
        public static void RegisterTypeAdv<T>( string typename, string Helptext = "")
        {
            ScriptingCore.DebugLog("RegisterType: " + typename);
            UserData.RegisterType<T>();
            if (ScriptingCore.typedict.ContainsKey(typename)) ScriptingCore.DebugLogError("The type " + typename + " is already registered skipping it! ");
            else
            {
                ScriptingCore.typedirectdict.Add(typename, typeof(T));
            }
            AddHelpInfo<T>(typename, "Class: " + Helptext , false);
        }
        public static void RegisterTypeAdv<T>(System.Object dynv, string typename, string Helptext = "") 
        {
            ScriptingCore.DebugLog("RegisterType: "+ typename);
            if (dynv == null) { ScriptingCore.DebugLogError("The type " + typename + " is null! ");
                throw new ArgumentNullException("The type of " + typename + " is null! ");
            }
            DynValue dynvalue= UserData.Create(dynv);
            UserData.RegisterType<T>();
            if (ScriptingCore.typedict.ContainsKey(typename)) ScriptingCore.DebugLogError("The type " + typename + " is already registered skipping it! ");
            else
            {
                if (dynvalue == null)
                {
                    ScriptingCore.DebugLogError("The  Dynvalue of " + typename + " is null! ");
                    throw new ArgumentNullException("The  Dynvalue of " + typename + " is null! ");
                }
                else ScriptingCore.typedict.Add(typename, dynvalue);
            }
            AddHelpInfo<T>(typename, "Class: " + Helptext , false);
        }
    }
}