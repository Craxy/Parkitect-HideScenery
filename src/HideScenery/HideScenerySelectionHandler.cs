using System;
using System.Collections.Generic;
using System.Reflection;
using Craxy.Parkitect.HideScenery.Selection;
using UnityEngine;

namespace Craxy.Parkitect.HideScenery
{
  internal sealed class HideScenerySelectionHandler : MonoBehaviour
  {
    private readonly CustomSelectionTool tool = new();
    internal Calc calc;
    public int NumberOfHiddenObjects => tool.NumberOfSelectedObjects;
    public readonly Options Options = new();
    private readonly Gui gui = new();
    private Park park;
    public bool ShowGui = true;

    private void Awake()
    {
      park = GameController.Instance.park;

      Options.Changed += OnOptionsChanged;
      tool.OnAddedSelectedObject += OnAddedSelectedObject;
      tool.OnRemovedSelectedObject += OnRemovedSelectedObject;
      tool.OnRemoved += OnToolDisabled;
      tool.DeselectOnRemove = false;

      calc = new Calc(this);
    }
    private void OnEnable()
    {
      foreach (var o in tool.GetSelectedObjects())
      {
        OnAddedSelectedObject(o);
      }
      Injector.Instance.Apply(calc.BuildableObjectVisibility);
      tool.CalcIndividualVisibility = calc.BuildableObjectVisibility;
      tool.CalcBoxAction = calc.BoxAction;
    }
    private void OnGUI()
    {
      if(ShowGui)
      {
        gui.Show(this);
      }
      else
      {
        gui.ShowIndicator();
      }
    }
    private void OnDisable()
    {
      GameController.Instance.removeMouseTool(tool);

      tool.CalcIndividualVisibility = IndividualSelectionTool.DefaultVisibility;
      tool.CalcBoxAction = BoxSelectionTool.DefaultAction;
      Injector.Instance.Remove();
      foreach (var o in tool.GetSelectedObjects())
      {
        OnRemovedSelectedObject(o);
      }
    }
    private void OnDestroy()
    {
      Options.Changed -= OnOptionsChanged;
      tool.OnAddedSelectedObject -= OnAddedSelectedObject;
      tool.OnRemovedSelectedObject -= OnRemovedSelectedObject;
      tool.OnRemoved -= OnToolDisabled;

      park = null;
      calc = null;
    }

    private void OnToolDisabled()
    {
      Options.Mode = Mode.None;
    }
    private void OnOptionsChanged(Options options, string property)
    {
      switch (property)
      {
        case nameof(Options.Transparency):
          UpdateTransparency();
          break;
        case nameof(Options.Mode):
          ApplyMode();
          break;
      }
    }

    private void ApplyMode()
    {
      Mod.Log($"Apply mode {Options.Mode}");
      switch(Options.Mode)
      {
        case Mode.Individual:
          tool.Mode = Mode.Individual;
          GameController.Instance.enableMouseTool(tool);
          break;
        case Mode.Box:
          tool.Mode = Mode.Box;
          GameController.Instance.enableMouseTool(tool);
          break;
        case Mode.None:
          tool.Mode = Mode.None;
          GameController.Instance.removeMouseTool(tool);
          break;
      }
    }

    private Material _highlightMaterial = null;
    private Material HighlightMaterial
    {
      get
      {
        if (_highlightMaterial == null)
        {
          _highlightMaterial = (Material)
            typeof(Park)
              .GetField("seeThroughMaterialInstance", BindingFlags.NonPublic | BindingFlags.Instance)
              .GetValue(GameController.Instance.park);
        }
        return _highlightMaterial;
      }
    }
    public const string HideSceneryTag = "HideSceneryObject";
    private readonly List<SerializedMonoBehaviour> isHiddenBuffer = new();
    public bool IsHidden(BuildableObject o)
    {
      o.retrieveObjectsBelongingToThis(isHiddenBuffer);
      foreach (var c in isHiddenBuffer)
      {
        if(!Utility.isMaterialManagerApplied(o.transform, HideSceneryTag))
        {
          return false;
        }
      }
      isHiddenBuffer.Clear();
      return true;
    }

    private void UpdateTransparency()
    {
      // from Park.updateSeeThroughObjectsMaterialAlpha
      var color = HighlightMaterial.color;
      color.a = Mathf.Lerp(0.01960784f, 0.1764706f, Options.Transparency);
      HighlightMaterial.color = color;
    }

    private readonly List<SerializedMonoBehaviour> selectedObjectBuffer = new();
    private void OnAddedSelectedObject(BuildableObject o)
    {
      if(o == null)
      {
        return;
      }

      Mod.DebugLog($"OnAdd: {o.GetType().Name} -- {o.getName()}");

      o.retrieveObjectsBelongingToThis(selectedObjectBuffer);
      foreach (var c in selectedObjectBuffer)
      {
        Utility.attachMaterialManagerByObject(c, HideSceneryTag, HighlightMaterial, null, true);
      }
      selectedObjectBuffer.Clear();
    }
    private void OnRemovedSelectedObject(BuildableObject o)
    {
      if(o == null)
      {
        return;
      }

      Mod.DebugLog($"OnRemove: {o.GetType().Name} -- {o.getName()}");
      o.retrieveObjectsBelongingToThis(selectedObjectBuffer);
      foreach (var c in selectedObjectBuffer)
      {
        Utility.destroyMaterialManagerByObject(c, HideSceneryTag);
      }
      selectedObjectBuffer.Clear();
    }
    public void DeselectAll() => tool.DeselectAll();

    private void Update()
    {
      if(GameController.Instance.isActiveMouseTool(tool))
      {
        tool.tick();
      }
    }

    private Visibility CalcVisibility(BuildableObject o)
    {
      if(o.isPreview)
      {
        return Visibility.Ignore;
      }

      var isDeco = o is Deco;
      var isPath = o is Path;
      if(!(isDeco || isPath))
      {
        return Visibility.Ignore;
      }

      if(isDeco && park.hideSceneryEnabled)
      {
        return Visibility.Ignore;
      }
      else if(isPath && park.hidePathsEnabled)
      {
        return Visibility.Ignore;
      }

      if(IsHidden(o))
      {
        return Visibility.Hidden;
      }
      else
      {
        return Visibility.Visible;
      }
    }

    public void HideSceneryAbove(float height)
    {
      var mc = MouseCollisions.Instance;

      var bounds = new Bounds();
      bounds.SetMinMax(new Vector3(0.0f, height, 0.0f), new Vector3(Park.MAX_SIZE, 10_000.0f, Park.MAX_SIZE));
      Mod.DebugLog($"Bounds: {bounds}");

      //todo: cache?
      var results = new List<MouseCollider>();
      mc.octree.getColliding(bounds, results);

      foreach (var coll in results)
      {
        var o = coll.buildableObject;
        switch (calc.HideAboveHeightAction(SelectionOperation.Add, bounds, o))
        {
          case SelectionAction.Add:
            tool.Add(o);
            break;
          case SelectionAction.Remove:
          case SelectionAction.DoNothing:
          default:
            break;
        }
      }
    }
  }
}
