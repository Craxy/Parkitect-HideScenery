using System;
using System.Collections.Generic;
using System.Reflection;
using Craxy.Parkitect.HideScenery.Selection;
using Craxy.Parkitect.HideScenery.Utils;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace Craxy.Parkitect.HideScenery
{
  sealed class HideScenerySelectionHandler : MonoBehaviour
  {
    private readonly CustomSelectionTool tool = new CustomSelectionTool();
    internal Calc calc;
    public int NumberOfHiddenObjects => tool.NumberOfSelectedObjects;
    public readonly Options Options = new Options();
    private readonly Gui gui = new Gui();
    private Park park;


    void Awake()
    {
      park = GameController.Instance.park;

      Options.Changed += OnOptionsChanged;
      tool.OnAddedSelectedObject += OnAddedSelectedObject;
      tool.OnRemovedSelectedObject += OnRemovedSelectedObject;
      tool.OnRemoved += OnToolDisabled;
      tool.DeselectOnRemove = false;
      
      calc = new Calc(this);
    }
    void OnEnable()
    {
      foreach (var o in tool.GetSelectedObjects())
      {
        OnAddedSelectedObject(o);
      }
      Injector.Instance.Apply(calc.BuildableObjectVisibility);
      tool.CalcIndividualVisibility = calc.BuildableObjectVisibility;
      tool.CalcBoxAction = calc.BoxAction;
    }
    void OnGUI()
    {
      gui.Show(this);
    }
    void OnDisable()
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
    void OnDestroy()
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
    
    void ApplyMode()
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
    private readonly List<SerializedMonoBehaviour> isHiddenBuffer = new List<SerializedMonoBehaviour>();
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

    void UpdateTransparency()
    {
      // from Park.updateSeeThroughObjectsMaterialAlpha
      var color = HighlightMaterial.color;
      color.a = Mathf.Lerp(0.01960784f, 0.1764706f, Options.Transparency);
      HighlightMaterial.color = color;
    }

    private readonly List<SerializedMonoBehaviour> selectedObjectBuffer = new List<SerializedMonoBehaviour>();
    void OnAddedSelectedObject(BuildableObject o)
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
    void OnRemovedSelectedObject(BuildableObject o)
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
    
    void Update()
    {
      if(GameController.Instance.isActiveMouseTool(tool))
      {
        tool.tick();
      }
    }
    
    Visibility CalcVisibility(BuildableObject o)
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
  }
}
