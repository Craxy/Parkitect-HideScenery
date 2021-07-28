using Craxy.Parkitect.HideScenery.Selection;
using UnityEngine;

namespace Craxy.Parkitect.HideScenery
{
  internal sealed class HideSceneryHandler : MonoBehaviour
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
    private void Awake()
    {
      selectionHandler = gameObject.AddComponent<HideScenerySelectionHandler>();
      selectionHandler.enabled = false;
    }
    private void OnDisable()
    {
      DisableSelectionHandler();
    }
    private void OnDestroy()
    {
      if (selectionHandler != null)
      {
        GameObject.Destroy(selectionHandler);
        selectionHandler = null;
      }
    }

    private void Update()
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

    private void EnableSelectionHandler()
    {
      if(!SelectionHandlerEnabled)
      {
        SelectionHandlerEnabled = true;
      }
    }
    private void DisableSelectionHandler()
    {
      if(SelectionHandlerEnabled)
      {
        SelectionHandlerEnabled = false;
      }
    }
    private void ClearSelection()
    {
      if(SelectionHandlerEnabled)
      {
        selectionHandler.DeselectAll();
      }
    }
  }
}
