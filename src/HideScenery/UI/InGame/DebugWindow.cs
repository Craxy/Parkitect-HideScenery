using System.Diagnostics;
using Craxy.Parkitect.HideScenery.UI.Utils;
using RapidGUI;
using UnityEngine;
using Handler = Craxy.Parkitect.HideScenery.HideScenerySelectionHandler;

namespace Craxy.Parkitect.HideScenery.UI.InGame
{
  internal sealed class DebugWindow
  {
    private readonly CollapsibleWindow window;
    private readonly DebugContent content;
    public Handler Handler;

    public DebugWindow(Handler handler)
    {
      Handler = handler;

      content = new();

      window = new CollapsibleWindow("[DBG] Hide Scenery")
      {
        MinimizedName = "[D]HS",
        CustomWidth = 275.0f,
        Minimizable = true,
        Collapsible = false,
        IsOpen = true,
      };
      window.Rect.position = new Vector2(100.0f, 100.0f);
      window.Add(() => content.Show(Handler));
    }

    [Conditional("DEBUG")]
    public void Show()
    {
      if(GameController.Instance.isGameInputLocked())
      {
        return;
      }

      window.DoGUIWindow();
    }
  }
}
