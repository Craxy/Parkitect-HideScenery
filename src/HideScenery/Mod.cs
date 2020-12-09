using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Craxy.Parkitect.HideScenery
{
  public sealed class Mod : AbstractMod
  {
    internal static readonly string name, description, identifier, version;

    static Mod()
    {
      var assembly = Assembly.GetExecutingAssembly();

      identifier = assembly.GetName().Name;

      T GetAssemblyAttribute<T>() where T : Attribute => (T)assembly.GetCustomAttribute(typeof(T));

      name = GetAssemblyAttribute<AssemblyTitleAttribute>().Title
#if DEBUG
          + " (Debug)"
#endif
      ;
      description = GetAssemblyAttribute<AssemblyDescriptionAttribute>().Description;

      version = assembly.GetName().Version.ToString();
    }

    public static void Log(string msg)
    {
      Debug.Log("[" + name + "] " + msg);
    }
    [System.Diagnostics.Conditional("DEBUG")]
    public static void DebugLog(string msg) => Log(msg);

    private GameObject go;
    public override void onEnabled()
    {
      if(GameController.Instance == null)
      {
        Mod.Log("onEnable but no GameController -> ignore");
        var mod = ModManager.Instance.getModEntries().SingleOrDefault(me => me.mod.getIdentifier() == this.getIdentifier());
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
    public override void onDisabled()
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

    public override string getName() => name;
    public override string getDescription() => description;
    public override string getVersionNumber() => version;
    public override string getIdentifier() => identifier;

    public override bool isMultiplayerModeCompatible() => true;
    public override bool isRequiredByAllPlayersInMultiplayerMode() => false;
  }
}
