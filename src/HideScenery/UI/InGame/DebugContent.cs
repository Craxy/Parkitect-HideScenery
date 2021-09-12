using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Craxy.Parkitect.HideScenery.Selection;
using Craxy.Parkitect.HideScenery.UI.Utils;
using Craxy.Parkitect.HideScenery.Utils;
using Parkitect.UI;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;
using Handler = Craxy.Parkitect.HideScenery.HideScenerySelectionHandler;

namespace Craxy.Parkitect.HideScenery.UI.InGame
{
  internal sealed class DebugContent
  {
    [Conditional("DEBUG")]
    public void Show(Handler handler)
    {
      ShowDebugUI(handler);

      if (showTooltip)
      {
        ShowTooltip(handler);
      }
    }

    private bool showTooltip = false;
    private void ShowDebugUI(Handler hs)
    {
      using (Scope.Vertical())
      {
        showTooltip = Toggle(showTooltip, "Show tooltip");
        using(Scope.Indentation())
        {
          tooltip.Default = Toggle(tooltip.Default, "default");
          tooltip.DebugData = Toggle(tooltip.DebugData, "debug data");
          tooltip.ObjectBelowMouse = Toggle(tooltip.ObjectBelowMouse, "object below mouse");
          tooltip.ObjectsBelowMouse = Toggle(tooltip.ObjectsBelowMouse, "objects below mouse");
          tooltip.Camera = Toggle(tooltip.Camera, "camera");
        }
        Space(10.0f);
        if (Button("Clear log"))
        {
          ClearLog();
        }
      }
    }
    private static void ClearLog()
    {
      var t = typeof(DebugConsole);
      var dc = DebugConsole.Instance;
      var logQueue = (Queue<string>)t.GetField("logQueue", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dc);
      logQueue.Clear();
      t.GetField("logText", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dc, "");
    }

    private readonly Tooltip tooltip = new();
    private void ShowTooltip(Handler hs)
    {
      tooltip.Show(hs);
    }
    private sealed class Tooltip
    {
      private float updateTooltipTimeout = 0.0f;
      private const float updateTooltipTimeoutStep = 0.2f;

      public void Show(Handler hs)
      {
        if (UIUtility.isMouseOverUIElement())
        {
          return;
        }

        if (updateTooltipTimeout <= 0.0f)
        {
          updateTooltipTimeout = updateTooltipTimeoutStep;
          if (TryGetBuildableObjectBelowMouse(out var o))
          {
            var text = GetTooltip(hs, o);
            UITooltipController.Instance.showTooltip(text, true, updateTooltipTimeoutStep * 1.1f);
          }
          else
          {
            var text = GetTooltip(hs, null);
            UITooltipController.Instance.showTooltip(text, true, updateTooltipTimeoutStep * 1.1f);
            // UITooltipController.Instance.hideTooltip();
          }
        }

        updateTooltipTimeout -= Time.unscaledDeltaTime;
      }

      private sealed class Builder : IDisposable
      {
        private const int IndentationLength = 2;
        private int currentIndentation = 0;

        public Builder Indent()
        {
          currentIndentation += IndentationLength;
          return this;
        }
        // abusing `Dispose` for indentation
        void IDisposable.Dispose()
        {
          UnityEngine.Debug.Assert(currentIndentation >= 0);
          currentIndentation -= IndentationLength;
        }
        public void AssertNoIndentation()
        {
          UnityEngine.Debug.Assert(currentIndentation == 0);
        }

        private readonly StringBuilder sb = new();
        public string GetCurrent() => sb.ToString();
        public string GetAndClear()
        {
          var text = sb.ToString();
          sb.Clear();
          return text;
        }
        private StringBuilder AppendIndent() => sb.Append(' ', currentIndentation * IndentationLength);
        public Builder KeyValue<T>(string title, T value)
        {
          AppendIndent().Append(title).Append(" = ").Append(value).AppendLine();
          return this;
        }

        public Builder Lines(string lines)
        {
          foreach (var line in lines.Split('\n'))
          {
            AppendIndent().AppendLine(line);
          }
          return this;
        }
      }
      private readonly Builder t = new();

      public bool Default = true;
      public bool DebugData = false;
      public bool ObjectBelowMouse = false;
      public bool ObjectsBelowMouse = false;
      public bool Camera = false;

      private readonly List<BuildableObjectBelowMouseInfo> hits = new(0);

      private string GetTooltip(Handler hs, BuildableObject o)
      {
        if (o is not null)
        {
          t.KeyValue(o.GetType().Name, o.getUnlocalizedName());
          if (Default)
          {
            using (t.Indent())
            {
              t.KeyValue("Name", o.getUnlocalizedName());
              t.KeyValue("ReferenceName", o.getReferenceName());
              t.KeyValue("Category", o.getCategoryTag());
              t.KeyValue("Group", o.groupTag);
              t.KeyValue("Theme", o.themeTag);
              t.KeyValue("canBeSelected", o.canBeSelected());
              t.KeyValue("height", o.transform.localPosition.y);
              t.KeyValue("Position", "");
              using (t.Indent())
              {
                t.KeyValue("local", o.transform.localPosition.ToString());
                t.KeyValue("global", o.transform.position.ToString());
                t.KeyValue("forward", o.transform.forward.ToString());
                var pos = o.transform.position;
                (var x, var y, var z) = (Mathf.FloorToInt(pos.x), pos.y, Mathf.FloorToInt(pos.z));
                t.KeyValue("tile", $"{x},{y},{z}");
              }

              switch (o)
              {
                case Path path:
                  {
                    t.KeyValue("Path", "");
                    using (t.Indent())
                    {
                      t.KeyValue("decoScore", path.decoScore);
                    }
                  }
                  break;
                case Wall wall:
                  {
                    t.KeyValue("Wall", "");
                    using (t.Indent())
                    {
                      t.KeyValue("BlockedSides", wall.blockedSides.ToString());
                      var bos = wall.getBuiltOnSide();
                      t.KeyValue("BuiltOnSide", bos.ToString());
                      t.KeyValue("BlockSide", BlockSideHelper.FromSide(bos).ToString());
                    }
                  }
                  break;
              }
            }
          }

          if(DebugData)
          {
            t.KeyValue("DebugData", "");
            using(t.Indent())
            {
              t.Lines(o.getDebugData());
              switch(o)
              {
                case Block block:
                {
                  t.KeyValue("DebugInfo", "");
                  using(t.Indent())
                  {
                    t.Lines(block.getDebugInfo());
                  }
                }
                break;
              }
            }
          }

          if(ObjectBelowMouse)
          {
            var ob = Utility.getObjectBelowMouse();
            t.KeyValue("Hit", ob.hitSomething);
            if(ob.hitSomething)
            {
              using(t.Indent())
              {
                t.KeyValue("Type", ob.hitObject.GetType().Name);
                t.KeyValue("Distance", ob.hitDistance);
                t.KeyValue("Layer", ob.hitLayerMask);
                t.KeyValue("canBeSelected", ob.hitObject.canBeSelected());
              }
            }
          }

          if(ObjectsBelowMouse)
          {
            HitUtility.GetAllObjectsBelowMouse(hs.calc.BuildableObjectVisibility, hits);
            t.KeyValue("Objects under mouse", hits.Count);
            using(t.Indent())
            {
              foreach (var hit in hits)
              {
                var bo = hit.HitObject;
                t.KeyValue(bo.GetType().Name, bo.getReferenceName());
                using(t.Indent())
                {
                  t.KeyValue("Distance", hit.HitDistance);
                  t.KeyValue("Visibility", hit.HitVisibility);
                  t.KeyValue("Layer", hit.HitLayer);
                  t.KeyValue("canBeSelected", bo.canBeSelected());
                }
              }
            }
            hits.Clear();
          }

          if(Camera)
          {
            t.KeyValue("Camera", "");
            using(t.Indent())
            {
              var gc = GameController.Instance.cameraController;
              t.KeyValue("position", gc.transform.position);
              t.KeyValue("forward", gc.transform.forward);
              t.KeyValue("rotation", gc.transform.rotation);
              t.KeyValue("euler angle", gc.transform.eulerAngles);
              t.KeyValue("FrontSides", BlockSideHelper.CalcFrontSidesFromCurrentView().ToString());
            }
          }
        }

        t.AssertNoIndentation();
        return t.GetAndClear();
      }

      private static bool TryGetBuildableObjectBelowMouse(out BuildableObject o)
      {
        var hit = Utility.getObjectBelowMouse<BuildableObject>(0);
        static bool TryGetMatchingObject(IMouseSelectable ms, out BuildableObject bo)
        {
          if (ms is Deco || ms is Path)
          {
            bo = (BuildableObject)ms;
            return true;
          }
          else
          {
            bo = default;
            return false;
          }
        }
        if (hit.hitSomething && TryGetMatchingObject(hit.hitObject, out o))
        {
          return true;
        }
        else
        {
          o = default;
          return false;
        }
      }
    }
  }
}
