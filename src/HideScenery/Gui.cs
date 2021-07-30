using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Craxy.Parkitect.HideScenery.Selection;
using Craxy.Parkitect.HideScenery.Utils;
using Parkitect.UI;
using UnityEngine;
using static UnityEngine.GUILayout;
using Handler = Craxy.Parkitect.HideScenery.HideScenerySelectionHandler;

namespace Craxy.Parkitect.HideScenery
{
  internal sealed class Gui
  {
    private const float width = 270.0f;
    private const float contractedHeight = 175.0f;
    private const float expandedHeight = 450.0f;
    private const float right = 10.0f;
    private const float top = 75.0f;
    private const float padding = 5.0f;
    private const float hIndentation = 12.0f;
    private const float vIndentation = 12.0f;
    private static void hIndent() => Space(hIndentation);
    private static void vIndent() => Space(vIndentation);

    private readonly Texture2D backgroundTexture;
    public Gui()
    {
      backgroundTexture = new Texture2D(1, 1);
      backgroundTexture.SetPixel(0, 0, new Color(0.321f, 0.321f, 0.321f, 1.0f));
      backgroundTexture.Apply();
    }

    public bool Expanded = true;
    public bool TuckedAway = false;
    private Vector2 scrollPosition = Vector2.zero;

    internal void ShowIndicator()
    {
      var c = new GUIContent("<size=8><color=brown><b>HS</b></color></size>");
      var size = GUI.skin.label.CalcSize(c);
      var pos = new Rect(Screen.width - 1.0f - size.x, -8.0f, size.x, size.y);
      GUI.Label(pos, c);
    }
    private void ShowTuckedAway()
    {
      // about arrows: 
      // use ←↑→↓ instead of ⇐⇑⇒⇓ because double arrows are rendered inconsistently
      var c = new GUIContent("<b>←</b>");
      var size = GUI.skin.toggle.CalcSize(c);

      const float dp = padding * 2.0f;
      const float innerLeft = 5.0f, innerRight = 7.0f;
      const float innerTop = 7.0f, innerBottom = 5.0f;

      var innerWidth = size.x + innerLeft + innerRight;
      var innerHeight = size.y + innerTop + innerBottom;

      using(Layout.Area(new Rect(Screen.width - right - innerWidth - dp, top, innerWidth + dp, innerHeight + dp)))
      {
        GUI.Box(new Rect(0.0f, 0.0f, innerWidth + dp, innerHeight + dp), backgroundTexture);
        using(Layout.Area(new Rect(padding, padding, innerWidth, innerHeight)))
        {
          Space(innerTop);
          using(Layout.Horizontal())
          {
            Space(innerLeft);
            if(!Toggle(true, c)) {
              TuckedAway = false;
            }
            Space(innerRight);
          }
          Space(innerBottom);
        }
      }
    }
    private void ShowFull(Handler hs)
    {
      var expanded = Expanded;
      var height = expanded ? expandedHeight : contractedHeight;

      using (Layout.Area(new Rect(Screen.width - right - width, top, width, height)))
      {
        GUI.Box(new Rect(0.0f, 0.0f, width, height), backgroundTexture);
        using (Layout.Area(new Rect(padding, padding, width - (2 * padding), height - (2 * padding))))
        {
          using (Layout.Vertical())
          {
            // Always visible
            {
              Space(7.0f);
              ShowHeader(0.0f, hs);
              ShowTransparency(0.0f, hs);
              ShowToggleMode(0.0f, hs);
              ShowHideAboveHeight(0.0f, hs);
              ShowNumberOfHiddenObjects(0.0f, hs);
            }

            if (!Expanded)
            {
              return;
            }

            // only visible when expanded
            vIndent();
            scrollPosition = BeginScrollView(scrollPosition);
            if(showHideAboveHeightOptions)
            {
              ShowHideAboveHeightOptions(0.0f, hs);
            }
            {
              switch (hs.Options.Mode)
              {
                case Mode.Box:
                  ShowBoxOptions(0.0f, hs);
                  break;
              }

              ShowDebug(hIndentation, hs);
            }
            EndScrollView();
          }
        }
      }
    }
    public void Show(Handler hs)
    {
      if(TuckedAway)
      {
        ShowTuckedAway();
      }
      else
      {
        ShowFull(hs);
      }
    }

    private void ShowHeader(float indentation, Handler hs)
    {
      using (Layout.HorizontalIndentation(indentation))
      {
        Label("<b>Hide Scenery</b>");
        FlexibleSpace();
        Expanded = Toggle(Expanded, Expanded ? "<b>↑</b>" : "<b>↓</b>");
        Space(5.0f);
        TuckedAway = Toggle(false, "<b>→</b>");
        Space(3.0f);
      }
    }
    private static void ShowTransparency(float indentation, Handler hs)
    {
      using (Layout.Vertical())
      {
        // Transparency is Settings.seeThroughObjectsAlpha
        // between 0 and 1
        // but when applied as alpha in Park.updateSeeThroughObjectsMaterialAlpha()
        //  its limited: Mathf.Lerp(0.01960784f, 0.1764706f, t)
        var t = hs.Options.Transparency;
        using (Layout.HorizontalIndentation(indentation))
        {
          Label("Transparency: "); //Label(t.ToString()); FlexibleSpace();
        }
        using (Layout.HorizontalIndentation(indentation))
        {
          hs.Options.Transparency = HorizontalSlider(t, 0.0f, 1.0f);
        }
      }
    }
    private void ShowToggleMode(float indentation, Handler hs)
    {
      using (Layout.HorizontalIndentation(indentation))
      {
        Label("Selection tool: ");
      }

      using (Layout.HorizontalIndentation(indentation + hIndentation))
      {
        var options = hs.Options;
        var mode = options.Mode;
        void ShowTM(Mode m, string n)
        {
          var selected = mode == m;
          var t = Toggle(selected, n);
          if (t && !selected)
          {
            options.Mode = m;
          }
        }
        ShowTM(Mode.None, "none");
        FlexibleSpace();
        ShowTM(Mode.Individual, "individual");
        FlexibleSpace();
        ShowTM(Mode.Box, "box");
        FlexibleSpace();
      }
    }

    private readonly ValueParser<float> hideAboveHeightValueParser = new(Parser.Float, "0");
    private bool showHideAboveHeightOptions = false;
    private void ShowHideAboveHeight(float indentation, Handler hs)
    {
      var options = hs.Options.HideAboveHeightOptions;
      if(options.Height != hideAboveHeightValueParser.Value)
      {
        hideAboveHeightValueParser.Input = options.Height.ToString();
      }

      using(Layout.HorizontalIndentation(indentation + hIndentation))
      {
        Label("∀ height ≥");
        Space(1.0f);
        UIControl.ValidationTextField(hideAboveHeightValueParser, GUILayout.Width(65.0f));
        if(hideAboveHeightValueParser.IsValidInput && hideAboveHeightValueParser.Value != options.Height)
        {
          options.Height = hideAboveHeightValueParser.Value;
        }
        Space(1.0f);
        using(Layout.GuiEnabled(hideAboveHeightValueParser.IsValidInput))
        {
          if(Button("Hide"))
          {
            hs.HideSceneryAbove(options.Height);
          }
        }
        //show toggle for options
        if(Expanded)
        {
          Space(3.0f);
          showHideAboveHeightOptions = Toggle(showHideAboveHeightOptions, showHideAboveHeightOptions ? "↑" : "↓");
        }
        FlexibleSpace();
      }
    }

    private static void ShowNumberOfHiddenObjects(float indentation, Handler hs)
    {
      using (Layout.HorizontalIndentation(indentation))
      {
        Label("# hidden objects: ");
        Label(hs.NumberOfHiddenObjects.ToString());
        FlexibleSpace();
        if (Button("clear"))
        {
          hs.DeselectAll();
        }
        Space(10.0f);
      }
    }

    private static void ShowAdvancedOptions(float indentation, AdvancedOptions options)
    {
      using(Layout.HorizontalIndentation(indentation))
      {
        options.ApplyFiltersOnAddOnly = Toggle(options.ApplyFiltersOnAddOnly, "Apply filters only when hiding");
      }
      using(Layout.HorizontalIndentation(indentation))
      {
        options.OnlyMatchCompletelyInBounds = Toggle(options.OnlyMatchCompletelyInBounds, "Only match completely in bounds");
      }
      using(Layout.HorizontalIndentation(indentation))
      {
        options.HidePaths = Toggle(options.HidePaths, "Hide paths");
        FlexibleSpace();
        options.HideScenery = Toggle(options.HideScenery, "Hide scenery");
        FlexibleSpace();
      }
      if(options.HideScenery)
      {
        using(Layout.HorizontalIndentation(indentation))
        {
          Label("Scenery types:");
        }
        indentation += hIndentation;
        {
          var sceneryToHide = options.SceneryToHide;
          void ShowSceneryToHide(SceneryType st, string name)
          {
            using (Layout.HorizontalIndentation(indentation))
            {
              var isSet = sceneryToHide.HasSet(st);
              var newSet = Toggle(isSet, name);
              if (isSet != newSet)
              {
                sceneryToHide = sceneryToHide.Set(st, newSet);
              }
            }
          }

          ShowSceneryToHide(SceneryType.Wall, "walls");
          if(sceneryToHide.HasSet(SceneryType.Wall))
          {
            indentation += hIndentation;
            var wallOptions = options.WallOptions;

            using(Layout.HorizontalIndentation(indentation))
            {
              wallOptions.OnlyMatchCompletelyInBounds = ShowValueDropDown("Only completely in bounds", wallOptions.OnlyMatchCompletelyInBounds);
            }
            using(Layout.HorizontalIndentation(indentation))
            {
              wallOptions.HideOnlyFacingCurrentView = Toggle(wallOptions.HideOnlyFacingCurrentView, "Hide only walls facing view");
            }
            if(wallOptions.HideOnlyFacingCurrentView)
            {
              using (Layout.HorizontalIndentation(indentation))
              {
                options.WallOptions.UpdateNotFacingCurrentView = Toggle(options.WallOptions.UpdateNotFacingCurrentView, "Update walls not facing view");
              }
            }

            indentation -= hIndentation;
          }

          ShowSceneryToHide(SceneryType.Roof, "roofs");
          if(sceneryToHide.HasSet(SceneryType.Roof))
          {
            indentation += hIndentation;
            var roofOptions = options.RoofOptions;

            using(Layout.HorizontalIndentation(indentation))
            {
              roofOptions.OnlyMatchCompletelyInBounds = ShowValueDropDown("Only completely in bounds", roofOptions.OnlyMatchCompletelyInBounds);
            }

            indentation -= hIndentation;
          }
          ShowSceneryToHide(SceneryType.Other, "everything else");
          if(sceneryToHide.HasSet(SceneryType.Other))
          {
            indentation += hIndentation;
            var otherOptions = options.OtherSceneryOptions;

            using(Layout.HorizontalIndentation(indentation))
            {
              otherOptions.OnlyMatchCompletelyInBounds = ShowValueDropDown("Only completely in bounds", otherOptions.OnlyMatchCompletelyInBounds);
            }

            indentation -= hIndentation;
          }
          options.SceneryToHide = sceneryToHide;
        }
      }
    }

    private static void ShowBoxOptions(float indentation, Handler hs)
    {
      using (Layout.HorizontalIndentation(indentation))
      {
        Label("Box selection options:");
      }
      ShowAdvancedOptions(indentation + hIndentation, hs.Options.BoxOptions);
    }

    private void ShowHideAboveHeightOptions(float indentation, Handler hs)
    {
      using(Layout.HorizontalIndentation(indentation))
      {
        Label("Hide above Height options:");
      }
      ShowAdvancedOptions(indentation + hIndentation, hs.Options.HideAboveHeightOptions);
    }

    private static Value ShowValueDropDown(string label, Value value)
    {
      using(Layout.Horizontal())
      {
        Label(label);
        Space(2.0f);
        var name = RapidGUI.RGUI.SelectionPopup(value.Name(), ValueExtensions.Names);
        return name == value.Name() ? value : (Value)Array.IndexOf(ValueExtensions.Names, name);
      }
    }

    [Conditional("DEBUG")]
    private void ShowDebug(float indentation, Handler hs)
    {
#if DEBUG
      ShowDebugGui(indentation, hs);
#endif
    }
#if DEBUG
    private bool showTooltip = false;
    [Conditional("DEBUG")]
    private void ShowDebugGui(float indentation, Handler hs)
    {
      vIndent(); vIndent();
      using (Layout.HorizontalIndentation(indentation))
      {
        showTooltip = Toggle(showTooltip, "Show tooltip");
      }

      if (showTooltip)
      {
        ShowTooltip(hs);
      }

      using (Layout.HorizontalIndentation(indentation))
      {
        if (Button("Clear log"))
        {
          var t = typeof(DebugConsole);
          var dc = DebugConsole.Instance;
          var logQueue = (Queue<string>)t.GetField("logQueue", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dc);
          logQueue.Clear();
          t.GetField("logText", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dc, "");
        }
      }
    }

    private readonly Tooltip tooltip = new();
    private void ShowTooltip(Handler hs)
    {
      tooltip.Show(hs);
    }
    private class Tooltip
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

      private readonly StringBuilder sb = new();
      private string GetTooltip(Handler hs, BuildableObject o)
      {
        void indent(int indentation) => sb.Append(' ', indentation * 2);
        void kv<T>(string title, T value) => sb.Append(title).Append('=').Append(value);
        void n() => sb.AppendLine();
        void kvn<T>(string title, T value)
        {
          kv(title, value);
          n();
        }


        if (o != null)
        {
          kvn(o.GetType().Name, o.getUnlocalizedName());
          indent(2); kvn("Name", o.getUnlocalizedName());
          indent(2); kvn("ReferenceName", o.getReferenceName());
          indent(2); kvn("Category", o.getCategoryTag());
          indent(2); kvn("Group", o.groupTag);
          indent(2); kvn("Theme", o.themeTag);
          indent(2); kvn("canBeSelected", o.canBeSelected());
          indent(2); kvn("height", o.transform.localPosition.y);
          indent(2); kvn("Position", "");
          indent(4); kvn("local", o.transform.localPosition.ToString());
          indent(4); kvn("global", o.transform.position.ToString());
          indent(4); kvn("forward", o.transform.forward.ToString());
          var position = o.transform.position;
          (var x, var y, var z) = (Mathf.FloorToInt(position.x), position.y, Mathf.FloorToInt(position.z));
          indent(4); kvn("tile", $"{x},{y},{z}");

          // indent(2); kvn("CrossedTiles", "");
          // var cts = o.getCrossedTiles();
          // indent(4); kvn("offsetX", cts.offsetX);
          // indent(4); kvn("offsetZ", cts.offsetZ);
          // indent(4); kvn("offsetY", cts.offsetY);
          // indent(4); kvn("rotationY", cts.rotationY);
          // var ctis = cts.crossedTilesInfo;
          // indent(4); kvn("Infos", ctis.Count);
          // foreach (var cti in ctis)
          // {
          //   indent(6); kvn("Info", "");
          //   indent(8); kvn("LocalX", cti.getLocalX());
          //   indent(8); kvn("LocalY", cti.getLocalZ());
          //   indent(8); kvn("WorldX", cti.getWorldX());
          //   indent(8); kvn("WorldZ", cti.getWorldZ());
          //   indent(8); kvn("MinY", cti.getMinY());
          //   indent(8); kvn("MaxY", cti.getMaxY());
          // }

          switch (o)
          {
            case Path path:
              indent(2); kvn("Path", "");
              indent(4); kvn("decoScore", path.decoScore);
              break;
            case Wall wall:
              indent(2); kvn("Wall", "");
              indent(4); kvn("BlockedSides", wall.blockedSides.ToString());
              var bos = wall.getBuiltOnSide();
              indent(4); kvn("BuiltOnSide", bos.ToString());
              indent(4); kvn("BlockSide", BlockSideHelper.FromSide(bos).ToString());
              break;
          }

          // n();
          // kvn("Camera", "");
          // var gc = GameController.Instance.cameraController;
          // indent(2); kvn("Position", gc.transform.position);
          // indent(2); kvn("Forward", gc.transform.forward);
          // indent(2); kvn("Rotation", gc.transform.rotation);
          // indent(2); kvn("EulerAngle", gc.transform.eulerAngles);
          // indent(2); kvn("FrontSides", BlockSideHelper.CalcFrontSidesFromCurrentView().ToString());

          // n();
          // kvn("DebugData", "");
          // sb.Append(o.getDebugData());
          // n();
          // switch (o)
          // {
          //   case Block block:
          //     kvn("DebugInfo", "");
          //     sb.Append(block.getDebugInfo());
          //     n();
          //     break;
          // }

          n();
        }

        // {
        //   HitUtility.GetAllObjectsBelowMouse(hs.calc.BuildableObjectVisibility, hits);
        //   kvn("Objects under mouse", hits.Count);
        //   foreach (var hit in hits)
        //   {
        //     var bo = hit.HitObject;
        //     indent(2); kvn(bo.GetType().Name, bo.getReferenceName());
        //     indent(4); kvn("Distance", hit.HitDistance);
        //     indent(4); kvn("Visibility", hit.HitVisibility);
        //     indent(4); kvn("Layer", hit.HitLayer);
        //     indent(2); kvn("canBeSelected", hit.HitObject.canBeSelected());
        //   }
        //   hits.Clear();
        // }
        // {
        //   var ob = Utility.getObjectBelowMouse();
        //   kvn("Hit", ob.hitSomething);
        //   if (ob.hitSomething)
        //   {
        //     indent(2); kvn("Type", ob.hitObject.GetType().Name);
        //     indent(2); kvn("Distance", ob.hitDistance);
        //     indent(2); kvn("Layer", ob.hitLayerMask);
        //     indent(2); kvn("canBeSelected", ob.hitObject.canBeSelected());
        //   }
        // }

        var tooltip = sb.ToString();
        sb.Clear();
        return tooltip;
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
#endif
  }
}
