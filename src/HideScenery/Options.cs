using System;
using System.Runtime.CompilerServices;
using Craxy.Parkitect.HideScenery.Selection;

namespace Craxy.Parkitect.HideScenery
{
  sealed class Options
  {
    public float Transparency
    {
      get => Settings.Instance.seeThroughObjectsAlpha;
      set
      {
        if(value != Transparency)
        {
          Settings.Instance.seeThroughObjectsAlpha = value;
          OnPropertyChanged();
        }
      }
    }

    private Mode mode = Mode.None;
    public Mode Mode
    {
      get => mode;
      set
      {
        if (value != mode)
        {
          mode = value;
          OnPropertyChanged();
        }
      }
    }

    public readonly BoxOptions BoxOptions = new BoxOptions();

    public readonly HideAboveHeightOptions HideAboveHeightOptions = new HideAboveHeightOptions();

    public delegate void PropertyChanged(Options options, string property);
    public event PropertyChanged Changed;
    private void OnPropertyChanged([CallerMemberName]string property = "") => Changed?.Invoke(this, property);
  }
  [Flags]
  enum SceneryType
  {
    Roof = 1 << 0,
    Wall = 1 << 1,
    Other = 1 << 2,
    All = ~0,
  }
  [Flags]
  enum HideType
  {
    Name = 1 << 0,
    Category = 1 << 1,
    Class = 1 << 2,
    All = ~0,
  }
  static class TypeExtensions
  {
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public static bool HasSet(this SceneryType value, SceneryType flag)
      => (value & flag) == flag;
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public static bool HasSet(this HideType value, HideType flag)
      => (value & flag) == flag;

    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public static SceneryType Add(this SceneryType value, SceneryType flag)
      => value | flag;
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public static HideType Add(this HideType value, HideType flag)
      => value | flag;
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)] 
    public static SceneryType Remove(this SceneryType value, SceneryType flag)
      => value & ~flag;
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)] 
    public static HideType Remove(this HideType value, HideType flag)
      => value & ~flag;
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)] 
    public static SceneryType Set(this SceneryType value, SceneryType flag, bool enabled)
      => enabled ? value | flag : value & ~flag;
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)] 
    public static HideType Set(this HideType value, HideType flag, bool enabled)
      => enabled ? value | flag : value & ~flag;
  }

  abstract class AdvancedOptions
  {
    public bool ApplyFiltersOnAddOnly = true;

    public bool HidePaths = true;
    public bool HideScenery = true;

    public SceneryType SceneryToHide = SceneryType.All;
    public RoofOptions RoofOptions = new RoofOptions {
      HideBy = HideType.All,
    };
    public WallOptions WallOptions = new WallOptions {
      HideBy = HideType.All,
      OnlyMatchExactlyInBounds = true,
      HideOnlyFacingCurrentView = false,
      UpdateNotFacingCurrentView = true,
    };
  }

  sealed class BoxOptions : AdvancedOptions {}

  sealed class RoofOptions
  {
    public HideType HideBy = HideType.All;  // cannot be class
  }
  sealed class WallOptions
  {
    public HideType HideBy = HideType.All;
    public bool OnlyMatchExactlyInBounds = false;
    public bool HideOnlyFacingCurrentView = false;
    public bool UpdateNotFacingCurrentView = true;
  }

  sealed class HideAboveHeightOptions : AdvancedOptions
  {
    public float Height = 4.1f;
  }
}
