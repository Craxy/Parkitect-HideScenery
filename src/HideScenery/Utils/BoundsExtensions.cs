using System.Runtime.CompilerServices;
using UnityEngine;

namespace Craxy.Parkitect.HideScenery.Utils
{
  public static class BoundsExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsCompletely(this Bounds bounds, Bounds testBound) 
      => bounds.Contains(testBound.min) && bounds.Contains(testBound.max);
  }
}