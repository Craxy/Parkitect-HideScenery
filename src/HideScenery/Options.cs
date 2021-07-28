using System;
using System.Runtime.CompilerServices;
using Craxy.Parkitect.HideScenery.Selection;

namespace Craxy.Parkitect.HideScenery
{
  internal sealed class Options
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

    public readonly BoxOptions BoxOptions = new();

    public readonly HideAboveHeightOptions HideAboveHeightOptions = new();

    public delegate void PropertyChanged(Options options, string property);
    public event PropertyChanged Changed;
    private void OnPropertyChanged([CallerMemberName]string property = "") => Changed?.Invoke(this, property);
  }
  [Flags]
  internal enum SceneryType
  {
    Roof = 1 << 0,
    Wall = 1 << 1,
    Other = 1 << 2,
    All = ~0,
  }
  [Flags]
  internal enum HideType
  {
    Name = 1 << 0,
    Category = 1 << 1,
    Class = 1 << 2,
    All = ~0,
  }
  internal static class TypeExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasSet(this SceneryType value, SceneryType flag)
      => (value & flag) == flag;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasSet(this HideType value, HideType flag)
      => (value & flag) == flag;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SceneryType Add(this SceneryType value, SceneryType flag)
      => value | flag;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HideType Add(this HideType value, HideType flag)
      => value | flag;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SceneryType Remove(this SceneryType value, SceneryType flag)
      => value & ~flag;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HideType Remove(this HideType value, HideType flag)
      => value & ~flag;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SceneryType Set(this SceneryType value, SceneryType flag, bool enabled)
      => enabled ? value | flag : value & ~flag;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HideType Set(this HideType value, HideType flag, bool enabled)
      => enabled ? value | flag : value & ~flag;
  }

  internal abstract class AdvancedOptions
  {
    public bool ApplyFiltersOnAddOnly = true;

    public bool HidePaths = true;
    public bool HideScenery = true;

    public SceneryType SceneryToHide = SceneryType.All;
    public RoofOptions RoofOptions = new()
    {
      HideBy = HideType.All,
    };
    public WallOptions WallOptions = new()
    {
      HideBy = HideType.All,
      OnlyMatchExactlyInBounds = true,
      HideOnlyFacingCurrentView = false,
      UpdateNotFacingCurrentView = true,
    };
  }

  internal sealed class BoxOptions : AdvancedOptions {}

  internal sealed class RoofOptions
  {
    public HideType HideBy = HideType.All;  // cannot be class
  }
  internal sealed class WallOptions
  {
    public HideType HideBy = HideType.All;
    public bool OnlyMatchExactlyInBounds = false;
    public bool HideOnlyFacingCurrentView = false;
    public bool UpdateNotFacingCurrentView = true;
  }

  internal sealed class HideAboveHeightOptions : AdvancedOptions
  {
    public float Height = 4.1f;
  }
}
