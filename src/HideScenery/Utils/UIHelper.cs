using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace Craxy.Parkitect.HideScenery.Utils
{
  public sealed class Layout : IDisposable
  {
    //abusing IDisposable......

    private static readonly Layout _horizontal = new(GUILayout.EndHorizontal);
    public static Layout Horizontal()
    {
      GUILayout.BeginHorizontal();
      return _horizontal;
    }

    private static readonly Layout _vertical = new(GUILayout.EndVertical);
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

    private static readonly Layout _area = new(GUILayout.EndArea);
    public static Layout Area(Rect rect)
    {
      GUILayout.BeginArea(rect);
      return _area;
    }

    private static readonly Layout _doNothing = new(() => {});
    private static readonly Layout _disableGuiEnd = new(() => GUI.enabled = false);
    private static readonly Layout _enableGuiEnd = new(() => GUI.enabled = true);
    public static Layout GuiEnabled(bool value)
    {
      if(GUI.enabled == value)
      {
        return _doNothing;
      }
      else
      {
        GUI.enabled = value;
        return value ? _disableGuiEnd : _enableGuiEnd;
      }
    }

    private readonly Action _end;
    private Layout(Action end)
    {
      _end = end;
    }

    private void End()
    {
      _end();
    }

    public void Dispose()
    {
      End();
    }
  }

  public static class UIControl
  {
    private static readonly Dictionary<int, GUIContent> cachedContents = new();
    public static GUIContent CachedContent(string text, string tooltip)
    {
      var hash = (tooltip.GetHashCode() * 17) + text.GetHashCode();
      if(cachedContents.TryGetValue(hash, out var c))
      {
        return c;
      }
      else
      {
        var cc = new GUIContent(text, tooltip);
        cachedContents.Add(hash, cc);
        return cc;
      }
    }

    public static void ValidationTextField<T>(ValueParser<T> valueParser, params GUILayoutOption[] options)
      where T : struct
    {
      Color? original = null;
      if(!valueParser.IsValidInput)
      {
        original = GUI.backgroundColor;
        GUI.backgroundColor = Color.red;
      }
      valueParser.Input = TextField(valueParser.Input, options);

      if(original.HasValue)
      {
        GUI.backgroundColor = original.Value;
      }
    }

    private static GUIStyle _checkBoxStyle;
    private static GUIStyle GetCheckBoxStyle()
    {
      if(_checkBoxStyle == null)
      {
        _checkBoxStyle = new GUIStyle(GUI.skin.toggle);
        _checkBoxStyle.normal.background = _checkBoxStyle.active.background = null;
        _checkBoxStyle.onNormal.background = _checkBoxStyle.onActive.background = null;
        // _checkBoxStyle.hover.background = _checkBoxStyle.onHover.background = GUI.skin.toggle.hover.background;
        _checkBoxStyle.hover.background = _checkBoxStyle.onHover.background = null;
        _checkBoxStyle.padding = GUI.skin.button.padding;
        _checkBoxStyle.overflow = new RectOffset(0,0,0,0);
        _checkBoxStyle.border = new RectOffset(2,2,2,2);
      }
      return _checkBoxStyle;
    }
    private static readonly Dictionary<int, (string, string)> checkBoxCache = new();
    public static bool CachedCheckBox(bool value, string text, string checkMark, string uncheckedMark, int id, params GUILayoutOption[] options)
    {
      string CreateText(bool value) => value ? $"{checkMark} {text}" : $"{uncheckedMark} {text}";
      static string Get(bool value, ref (string,string) t) => value ? t.Item1 : t.Item2;
      string GetText()
      {
        if(checkBoxCache.TryGetValue(id, out var c))
        {
          return Get(value, ref c);
        }
        else
        {
          var t = (CreateText(true), CreateText(false));
          checkBoxCache.Add(id, t);
          return Get(value, ref t);
        }
      }

      return Toggle(value, GetText(), GetCheckBoxStyle(), options);
    }
  }
}
