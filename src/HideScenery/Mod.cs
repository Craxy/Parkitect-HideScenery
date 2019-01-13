using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Craxy.Parkitect.HideScenery
{
  public sealed class Mod : IMod
  {
    public string Name => name;
    public string Description => description;
    public string Identifier => identifier;

    internal static readonly string name, description, identifier;

    static Mod()
    {
      var assembly = Assembly.GetExecutingAssembly();

      var meta =
          assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
          .Cast<AssemblyMetadataAttribute>()
          .Single(a => a.Key == "Identifier");
      identifier = meta.Value;

      T GetAssemblyAttribute<T>() where T : Attribute => (T)assembly.GetCustomAttribute(typeof(T));

      name = GetAssemblyAttribute<AssemblyTitleAttribute>().Title
#if DEBUG
          + " (Debug)"
#endif
      ;
      description = GetAssemblyAttribute<AssemblyDescriptionAttribute>().Description;
    }

    public static void Log(string msg)
    {
      Debug.Log("[" + name + "] " + msg);
    }
    [System.Diagnostics.Conditional("DEBUG")]
    public static void DebugLog(string msg) => Log(msg);

    private GameObject go;
    public void onEnabled()
    {
      if(GameController.Instance == null)
      {
        Mod.Log("onEnable but no GameController -> ignore");
        var mod = ModManager.Instance.getModEntries().SingleOrDefault(me => me.mod.Identifier == this.Identifier);
        if(mod != null)
        {
          mod.disableMod();
        }
        return;
      }
      Log("enabled");
      
      go = new GameObject("HideScenery");
      go.AddComponent<HideSceneryHandler>();
      KeyHandler.RegisterKeys();
    }
    public void onDisabled()
    {
      if(GameController.Instance == null)
      {
        Mod.DebugLog("onDisable but no GameController -> ignore");
        return;
      }

      Log("disabled");
      KeyHandler.UnregisterKeys();
      GameObject.Destroy(go);
      go = null;
    }


  }
}
