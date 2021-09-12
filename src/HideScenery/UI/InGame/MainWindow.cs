using System.Diagnostics;
using Craxy.Parkitect.HideScenery.UI.Utils;
using RapidGUI;
using UnityEngine;
using Handler = Craxy.Parkitect.HideScenery.HideScenerySelectionHandler;

namespace Craxy.Parkitect.HideScenery.UI.InGame
{
  internal sealed class MainWindow
  {
    private readonly MainContent content;
    private readonly CollapsibleWindow window;

    public MainWindow(Handler handler)
    {
      content = new MainContent(handler);

      const float width = 275.0f;
      window = new CollapsibleWindow("Hide Scenery")
      {
        CustomWidth = width,
        MinimizedName = "HS",
        Minimizable = true,
        Collapsible = false,
        Pinnable = true,
        IsOpen = true,
      };
      window.Rect.position = new Vector2(Screen.width - 10.0f - width, 75.0f);
      window.Add(content.DoGUI);
      AddDebug();
    }

    public void Show()
    {
      if(GameController.Instance.isGameInputLocked())
      {
        return;
      }

      window.DoGUIWindow();
      ShowDebug();
    }

#if DEBUG
    private bool showDebug = false;
    private DebugWindow debugWindow = null;
#endif

    [Conditional("DEBUG")]
    private void AddDebug()
    {
#if DEBUG
      window.Add(() =>
      {
        using (Scope.Vertical())
        {
          GUILayout.Space(15.0f);
          showDebug = GUILayout.Toggle(showDebug, "show debug");
        }
      });
#endif
    }

    [Conditional("DEBUG")]
    private void ShowDebug()
    {
#if DEBUG
      if (showDebug)
      {
        (debugWindow ??= new DebugWindow(content.Handler)).Show();
      }
#endif
    }
  }
}