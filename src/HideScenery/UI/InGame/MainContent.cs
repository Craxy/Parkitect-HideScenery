using System;
using Craxy.Parkitect.HideScenery.Selection;
using Craxy.Parkitect.HideScenery.UI.Utils;
using RapidGUI;
using UnityEngine;
using static UnityEngine.GUILayout;
using Handler = Craxy.Parkitect.HideScenery.HideScenerySelectionHandler;

namespace Craxy.Parkitect.HideScenery.UI.InGame
{
  internal sealed class MainContent : IDoGUI
  {
    public MainContent(Handler handler)
    {
      Handler = handler;
    }

    public Handler Handler;
    private Options Options => Handler.Options;
    public void DoGUI()
    {
      using(Scope.Vertical())
      {
        ShowTransparency();
        ShowHideAboveHeight();
        ShowToggleMode();
        ShowNumberOfHiddenObjects();
      }
    }

    private void ShowTransparency()
    {
      using(Scope.Vertical())
      {
        Label("Transparency: ");
        Options.Transparency = HorizontalSlider(Options.Transparency, 0.0f, 1.0f);
      }
    }

    private bool hideAboveHeightOptionsExpanded = false;
    private void ShowHideAboveHeight()
    {
      var options = Options.HideInBoundsOptions;
      using(Scope.Vertical())
      {
        using(Scope.Horizontal())
        {
          Label("height:");
          var result = Controls.ValidationTextField(options.Height, float.TryParse, GUILayout.Width(55.0f));
          if(result.Valid && result.Changed)
          {
            options.Height = result.Value;
          }
          FlexibleSpace();
          using(Scope.GuiEnabled(result.Valid))
          {
            if(Button("Hide ≥"))
            {
              Handler.SceneryAbove(options.Height, SelectionOperation.Add);
            }
            Space(2.5f);
            using(Scope.GuiEnabled(Handler.NumberOfHiddenObjects > 0))
            {
              if(Button("Clear ≤"))
              {
                Handler.SceneryBelow(options.Height, SelectionOperation.Remove);
              }
            }
          }
          Space(5.0f);
          hideAboveHeightOptionsExpanded = Controls.ExpandCollapseButton(hideAboveHeightOptionsExpanded);
        }
        if(hideAboveHeightOptionsExpanded)
        {
          using(Scope.Indentation())
          {
            ShowHideAboveHeightOptions();
          }
        }
      }
    }
    private void ShowHideAboveHeightOptions()
    {
      ShowAdvancedOptions(Handler.Options.HideInBoundsOptions);
    }

    private bool boxOptionsExpanded = false;
    private void ShowToggleMode()
    {
      using(Scope.Vertical())
      {
        Label("Selection tool: ");
        using(Scope.Indentation())
        {
          using(Scope.Horizontal())
          {
            var mode = Options.Mode;
            void ShowTM(Mode m, string n)
            {
              var selected = mode == m;
              var t = Toggle(selected, n);
              if(t && !selected)
              {
                Options.Mode = m;
              }
            }
            ShowTM(Mode.None, "none");
            FlexibleSpace();
            ShowTM(Mode.Individual, "individual");
            FlexibleSpace();
            ShowTM(Mode.Box, "box");
            FlexibleSpace();
            using(Scope.GuiEnabled(Options.Mode == Mode.Box))
            {
              boxOptionsExpanded = Controls.ExpandCollapseButton(boxOptionsExpanded);
            }
          }

          if(Options.Mode == Mode.Box && boxOptionsExpanded)
          {
            ShowAdvancedOptions(Options.BoxOptions);
          }
        }
      }
    }

    private void ShowAdvancedOptions(AdvancedOptions options)
    {
      using(Scope.Vertical())
      {
        options.ApplyFiltersOnAddOnly = Toggle(options.ApplyFiltersOnAddOnly, "Apply filters only when hiding");
        ShowBoundsOptions(options.BoundsOptions);

        options.HidePaths = Toggle(options.HidePaths, "Hide paths");
        options.HideScenery = Toggle(options.HideScenery, "Hide scenery");
        if(options.HideScenery)
        {
          using(Scope.Indentation())
          {
            Label("Scenery types:");
            using(Scope.Indentation())
            {
              var sceneryToHide = options.SceneryToHide;
              void ShowSceneryToHide(SceneryType st, string name)
              {
                var isSet = sceneryToHide.HasSet(st);
                var newSet = Toggle(isSet, name);
                if (isSet != newSet)
                {
                  sceneryToHide = sceneryToHide.Set(st, newSet);
                }
              }

              ShowSceneryToHide(SceneryType.Wall, "walls");
              if(sceneryToHide.HasSet(SceneryType.Wall))
              {
                using(Scope.Indentation())
                {
                  var wallOptions = options.WallOptions;
                  wallOptions.HideOnlyFacingCurrentView = Toggle(wallOptions.HideOnlyFacingCurrentView, "Hide only walls facing view");
                  if(wallOptions.HideOnlyFacingCurrentView)
                  {
                    options.WallOptions.UpdateNotFacingCurrentView = Toggle(options.WallOptions.UpdateNotFacingCurrentView, "Update walls not facing view");
                  }
                }
              }

              ShowSceneryToHide(SceneryType.Roof, "roofs");
              ShowSceneryToHide(SceneryType.Other, "everything else");

              options.SceneryToHide = sceneryToHide;
            }
          }
        }
      }
    }
    private void ShowBoundsOptions(BoundsOptions options)
    {
      const float spacing = 5.0f;
      using(Scope.Vertical())
      {
        options.OnlyMatchCompletelyInBounds = Toggle(options.OnlyMatchCompletelyInBounds, "Only match completely in bounds");
        if(options.OnlyMatchCompletelyInBounds)
        {
          using(Scope.Horizontal())
          {
            Space(Scope.DefaultIndentation);
            Label("Precision: ");
            Space(spacing);
            if(Toggle(options.Precision == Precision.Exact, "Exact"))
            {
              options.Precision = Precision.Exact;
            }
            Space(spacing);
            if(Toggle(options.Precision == Precision.Approximately, "Approximately"))
            {
              options.Precision = Precision.Approximately;
            }
            FlexibleSpace();
          }

          if(options.Precision == Precision.Approximately)
          {
            using(Scope.Horizontal())
            {
              Space(Scope.DefaultIndentation * 2);
              Label("ε: ");
              Space(spacing);
              Label(options.Epsilon.ToString("0.000"), Width(40.0f));
              Space(spacing);
              options.Epsilon = EpsilonSlider(options.Epsilon);
              FlexibleSpace();
            }
          }
        }
      }
    }

    private void ShowMaybeInheritedBoundsOptions(MaybeInheritedBoundsOptions options, BoundsOptions root)
    {
      //todo: implement
    }

    private float EpsilonSlider(float current)
    {
      const float min = -0.20f;
      const float max = 0.20f;
      const float step = 0.05f;

      const float d = max - min;
      const int steps = (int)(d / step); // ~zero based (+1 for real step counts (including `min`))

      const int intMin = 0;
      const int intMax = steps;

      static int IntRangeFromFloat(float current)
      {
        return (int)((current - min) / step);
      }
      static float FloatFromIntRange(int current)
      {
        return (current * step) + min;
      }

      var intValue = IntRangeFromFloat(current);
      var intResult = (int)HorizontalSlider(intValue, intMin, intMax, GUILayout.MinWidth(100.0f));
      if(intValue == intResult)
      {
        return current;
      }
      else
      {
        return FloatFromIntRange(intResult);
      }
    }

    private void ShowNumberOfHiddenObjects()
    {
      using(Scope.Horizontal())
      {
        Label("# hidden objects: ");
        Label(Handler.NumberOfHiddenObjects.ToString());
        FlexibleSpace();
        if(Button("Clear"))
        {
          Handler.DeselectAll();
        }
        Space(10.0f);
      }
    }
  }
}