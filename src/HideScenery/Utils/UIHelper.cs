using System;
using UnityEngine;

namespace Craxy.Parkitect.HideScenery.Utils
{
  public sealed class Layout : IDisposable
  {
    //abusing IDisposable......

    private static readonly Layout _horizontal = new Layout(GUILayout.EndHorizontal);
    public static Layout Horizontal()
    {
      GUILayout.BeginHorizontal();
      return _horizontal;
    }

    private static readonly Layout _vertical = new Layout(GUILayout.EndVertical);
    public static Layout Vertical()
    {
      GUILayout.BeginVertical();
      return _vertical;
    }

    public static Layout HorizontalIndentation(float indentation)
    {
      var l = Horizontal();
      if (indentation > 0.0f)
      {
        GUILayout.Space(indentation);
      }
      return l;
    }

    private static readonly Layout _area = new Layout(GUILayout.EndArea);
    public static Layout Area(Rect rect)
    {
      GUILayout.BeginArea(rect);
      return _area;
    }

    private readonly Action _end;
    private Layout(Action end)
    {
      _end = end;
    }

    private Layout End()
    {
      _end();
      return this;
    }

    public void Dispose()
    {
      End();
    }
  }
}
