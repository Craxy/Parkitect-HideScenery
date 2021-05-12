using Craxy.Parkitect.HideScenery.Selection;
using UnityEngine;

namespace Craxy.Parkitect.HideScenery
{
  sealed class HideSceneryHandler : MonoBehaviour
  {
    private HideScenerySelectionHandler selectionHandler;
    private bool SelectionHandlerEnabled
    {
      get => selectionHandler.enabled;
      set => selectionHandler.enabled = value;
    }
    private bool GuiEnabled
    {
      get => selectionHandler.ShowGui;
      set => selectionHandler.ShowGui = value;
    }
    void Awake()
    {
      selectionHandler = gameObject.AddComponent<HideScenerySelectionHandler>();
      selectionHandler.enabled = false;
    }
    void OnDisable()
    {
      DisableSelectionHandler();
    }
    void OnDestroy()
    {
      if (selectionHandler != null)
      {
        GameObject.Destroy(selectionHandler);
        selectionHandler = null;
      }
    }

    void Update()
    {
      if (UIUtility.isInputFieldFocused() || GameController.Instance.isGameInputLocked())
      {
        return;
      }
      
      void ToggleMode(Mode mode)
      {
        if(!SelectionHandlerEnabled)
        {
          EnableSelectionHandler();
        }
        var options = selectionHandler.Options;
        if(options.Mode == mode)
        {
          options.Mode = Mode.None;
        }
        else
        {
          options.Mode = mode;
        }
      }
      void ToggleEnabled(bool withGui)
      {
        // switch gui mode, if already same gui: toggle enabled
        if(SelectionHandlerEnabled)
        {
          if(GuiEnabled == withGui)
          {
            DisableSelectionHandler();
          }
          else
          {
            GuiEnabled = withGui;
          }
        }
        else
        {
          GuiEnabled = withGui;
          EnableSelectionHandler();
        }
      }

      if (InputManager.getKeyDown(KeyHandler.ToggleHideSceneryKey.keyIdentifier))
      {
        ToggleEnabled(withGui: true);
      }
      else if(InputManager.getKeyDown(KeyHandler.ToggleHideSceneryNoGuiKey.keyIdentifier)) 
      {
        ToggleEnabled(withGui: false);
      }
      else if(InputManager.getKeyDown(KeyHandler.ToggleNoneSelectionKey.keyIdentifier))
      {
        ToggleMode(Mode.None);
      }
      else if(InputManager.getKeyDown(KeyHandler.ToggleIndividualSelectionKey.keyIdentifier))
      {
        ToggleMode(Mode.Individual);
      }
      else if(InputManager.getKeyDown(KeyHandler.ToggleBoxSelectionKey.keyIdentifier))
      {
        ToggleMode(Mode.Box);
      }
      else if(InputManager.getKeyDown(KeyHandler.ClearSelectionKey.keyIdentifier))
      {
        ClearSelection();
      }
    }
    
    void EnableSelectionHandler()
    {
      if(!SelectionHandlerEnabled)
      {
        SelectionHandlerEnabled = true;
      }
    }
    void DisableSelectionHandler()
    {
      if(SelectionHandlerEnabled)
      {
        SelectionHandlerEnabled = false;
      }
    }
    void ClearSelection()
    {
      if(SelectionHandlerEnabled)
      {
        selectionHandler.DeselectAll();
      }
    }
  }
}
