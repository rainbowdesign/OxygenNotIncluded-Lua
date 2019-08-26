/*using LuaCore;
using Harmony;
using System.Reflection;
using System.IO;
using System;
using System.Runtime.CompilerServices;
[HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
public static class EntryPoint_inject
{
[MethodImpl(MethodImplOptions.NoInlining)]
    public static void Prefix()
    {
string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
string path2 = System.Reflection.Assembly.GetAssembly(typeof(EntryPoint)).Location.Replace(".dll", "");
        string path3 = AppDomain.CurrentDomain.BaseDirectory;
         string path4 =Directory.GetCurrentDirectory(); 
        ScriptingCore.InfoLog("path 1a " +EntryPoint.GetPath() + " path 2 " + path2+ " path 3 " + path3+ " path 4 " + path4);
        var sc = new LuaCore.ScriptingInstance(path);
        ScriptingCore.InfoLog("Started new Lua Scripting Instance " + sc.scriptpath);
    }
}
public static class EntryPoint
{
[MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetPath()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    }
}

/*
public static string AssemblyDirectory
{
    get
    {
        string codeBase = Assembly.GetExecutingAssembly().CodeBase;
        string name = Assembly.GetExecutingAssembly().GetName().Name;
        UriBuilder uri = new UriBuilder(codeBase+ Path.DirectorySeparatorChar+ name);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
    }
}
}



    */