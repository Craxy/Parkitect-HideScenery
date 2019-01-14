using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Craxy.Parkitect.HideScenery.Utils
{
  [Flags]
  enum BlockSide
  {
    None = 0,
    North = 1 << 0,
    West = 1 << 1,
    South = 1 << 2,
    East = 1 << 3,
    All = ~0,
  }

  static class BlockSideHelper
  {
    private static readonly BlockSide[] values = new[] {
      BlockSide.North,
      BlockSide.West,
      BlockSide.South,
      BlockSide.East,
    };
    public static BlockSide[] GetValues()
    {
      return values;
    }

    public static BlockSide FromSide(int side)
    {
      switch (side)
      {
        case 0: return BlockSide.North;
        case 1: return BlockSide.West;
        case 2: return BlockSide.South;
        case 3: return BlockSide.East;
        default: return BlockSide.None;

      }
    }

    public const float FrontSidesAngleLimit = 15.0f;
    /// <summary>
    /// From GameController.Instance.cameraController.transform.eulerAngles.y == rotation
    ///          looking towards: 
    ///   180 -> North
    ///   0   -> South
    ///   90  -> West
    ///   270 -> East
    ///          (== back side)
    ///
    /// (eulerAngles.x is tilt: 90 top down (mapview); 45 normal tilt)
    /// </summary>
    public static BlockSide CalcFrontSides(float angle, float limit)
    {
      var sides = BlockSide.None;
      if (270 + limit < angle || angle < 90 - limit)
      {
        sides = sides.Add(BlockSide.North);
      }
      if (90 + limit < angle && angle < 270 - limit)
      {
        sides = sides.Add(BlockSide.South);
      }
      if (180 + limit < angle && angle < 360 - limit)
      {
        sides = sides.Add(BlockSide.West);
      }
      if (0 + limit < angle && angle < 180 - limit)
      {
        sides = sides.Add(BlockSide.East);
      }
      return sides;
    }
    public static BlockSide CalcFrontSidesFromCurrentView()
    {
      var ea = GameController.Instance.cameraController.transform.eulerAngles;
      // top down view? (map view)
      if (Mathf.Approximately(ea.x, 90.0f))
      {
        return BlockSide.None;
      }
      else
      {
        return CalcFrontSides(ea.y, FrontSidesAngleLimit);
      }
    }
  }

  static class BlockSideExtensions
  {
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public static bool HasSide(this BlockSide sides, int side)
      => sides.HasSide(BlockSideHelper.FromSide(side));
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public static bool HasSide(this BlockSide sides, BlockSide side)
      => side != BlockSide.None && ((sides & side) != BlockSide.None);
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public static BlockSide Add(this BlockSide sides, BlockSide side)
      => sides | side;
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public static BlockSide Remove(this BlockSide sides, BlockSide side)
      => sides & ~side;
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public static BlockSide Toggle(this BlockSide sides, BlockSide side)
      => sides ^ side;
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public static BlockSide Set(this BlockSide sides, BlockSide side, bool enabled)
      => enabled ? sides | side : sides & ~side;
  }
}
