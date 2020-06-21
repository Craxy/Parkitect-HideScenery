using System.Collections.Generic;
using System.Reflection;
using Craxy.Parkitect.HideScenery.Selection;
using Harmony;
using UnityEngine;

namespace Craxy.Parkitect.HideScenery
{
  sealed class Injector
  {
    private static Injector _injector = null;
    public static Injector Instance
    {
      get
      {
        if (_injector == null)
        {
          _injector = new Injector();
          _injector.InitializePatches();
        }
        return _injector;
      }
    }

    private Injector() { }

    private readonly List<MethodPatch> patches = new List<MethodPatch>();
    private HarmonyInstance harmony = null;
    private void InitializePatches()
    {
      if (harmony != null)
      {
        return;
      }

      var id = typeof(Mod).Namespace;
      harmony = HarmonyInstance.Create(id);

      // deco
      {
        var original = typeof(Deco).GetMethod(nameof(Deco.canBeSelected), BindingFlags.Instance | BindingFlags.Public);
        var prefix = typeof(PatchFunctions).GetMethod(nameof(PatchFunctions.Deco_CanBeSelected_Prefix), BindingFlags.Static | BindingFlags.Public);
        patches.Add(MethodPatch.Create(original, prefix));
      }
      // path
      {
        var original = typeof(Path).GetMethod(nameof(Path.canBeSelected), BindingFlags.Instance | BindingFlags.Public);
        var prefix = typeof(PatchFunctions).GetMethod(nameof(PatchFunctions.Path_CanBeSelected_Prefix), BindingFlags.Static | BindingFlags.Public);
        patches.Add(MethodPatch.Create(original, prefix));
      }
    }

    public void Apply(HitUtility.CalcVisibility calcVisibility)
    {
      Debug.Assert(harmony != null);

      PatchFunctions.CalcVisibility = calcVisibility;
      foreach (var patch in patches)
      {
        patch.Apply(harmony);
      }
    }
    public void Remove()
    {
      Debug.Assert(harmony != null);

      foreach (var patch in patches)
      {
        patch.Remove(harmony);
      }
      PatchFunctions.CalcVisibility = null;
    }

    private sealed class MethodPatch
    {
      public readonly MethodBase Original;
      public readonly HarmonyMethod Prefix;
      private MethodInfo Patch;
      public bool IsPatched => Patch != null;

      private MethodPatch(MethodBase original, HarmonyMethod prefix)
      {
        Original = original;
        Prefix = prefix;
      }
      public static MethodPatch Create(MethodInfo original, MethodInfo prefix)
        => new MethodPatch(original, new HarmonyMethod(prefix));

      public void Apply(HarmonyInstance harmony)
      {
        if (IsPatched)
        {
          return;
        }

        Patch = harmony.Patch(Original, Prefix);
      }
      public void Remove(HarmonyInstance harmony)
      {
        if (!IsPatched)
        {
          return;
        }

        harmony.Unpatch(Original, Patch);
        Patch = null;
      }
    }

    private static class PatchFunctions
    {
      public static HitUtility.CalcVisibility CalcVisibility = null;

      public static bool Deco_CanBeSelected_Prefix(Deco __instance, ref bool __result)
        => CanBeSelected_Prefix(__instance, ref __result);
      public static bool Path_CanBeSelected_Prefix(Path __instance, ref bool __result)
        => CanBeSelected_Prefix(__instance, ref __result);

      private static bool CanBeSelected_Prefix(BuildableObject __instance, ref bool __result)
      {
        // return: 
        //    true: call original function
        //    false: don't call and use __result

        if (__instance.isPreview)
        {
          return true;
        }

        var cv = CalcVisibility;
        if (cv == null)
        {
          return true;
        }

        var visibility = cv(__instance);
        switch (visibility)
        {
          case Visibility.HiddenByParkitect:
          case Visibility.Hidden:
            __result = false;
            break;
          case Visibility.Ignore:
          case Visibility.Visible:
          case Visibility.Block:
            __result = true;
            break;
          default:
            throw new System.InvalidOperationException($"Unhandled visibility {visibility}");
        }

        return false;
      }
    }
  }
}