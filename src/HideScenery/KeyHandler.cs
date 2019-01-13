using UnityEngine;

namespace Craxy.Parkitect.HideScenery
{
  static class KeyHandler
  {
    //todo: description & names
    private static string GroupIdentifier => Mod.identifier;
    public static readonly KeyGroup Group = new KeyGroup(GroupIdentifier)
    {
      keyGroupName = Mod.name,
    };
    private static string Name(string name) => GroupIdentifier + "/" + name;
    public static readonly KeyMapping ToggleHideSceneryKey = new KeyMapping(
      Name("ToggleHideScenery"),
      KeyCode.Period,
      KeyCode.None
      )
    {
      keyName = "Toggle Hide Scenery",
      keyDescription = "",
    };

    public static readonly KeyMapping ToggleNoneSelectionKey = new KeyMapping(
      Name("ToggleNoneSelection"),
      KeyCode.Keypad7,
      KeyCode.None
      )
    {
      keyName = "Toggle None Selection",
      keyDescription = "",
    };
    public static readonly KeyMapping ToggleIndividualSelectionKey = new KeyMapping(
      Name("ToggleIndividualSelection"),
      KeyCode.Keypad8,
      KeyCode.None
      )
    {
      keyName = "Toggle Individual Selection",
      keyDescription = "",
    };
    public static readonly KeyMapping ToggleBoxSelectionKey = new KeyMapping(
      Name("ToggleBoxSelection"),
      KeyCode.Keypad9,
      KeyCode.None
      )
    {
      keyName = "Toggle Box Selection",
      keyDescription = "",
    };
    public static readonly KeyMapping ClearSelectionKey = new KeyMapping(
      Name("ClearSelection"),
      KeyCode.Keypad3,
      KeyCode.None
      )
    {
      keyName = "Clear all hidden objects",
      keyDescription = "",
    };

    public const string MoveHiddenCloserKeyIdentifier = "BuildingDecreaseObjectSize";
    public const string MoveHiddenAwayIdentifier = "BuildingIncreaseObjectSize";

    public static void RegisterKeys()
    {
      var im = InputManager.Instance;

      im.registerKeyGroup(Group);

      void registerKey(KeyMapping km)
      {
        km.keyGroupIdentifier = GroupIdentifier;
        im.registerKeyMapping(km);
      }

      registerKey(ToggleHideSceneryKey);
      registerKey(ToggleNoneSelectionKey);
      registerKey(ToggleIndividualSelectionKey);
      registerKey(ToggleBoxSelectionKey);
      registerKey(ClearSelectionKey);
    }
    public static void UnregisterKeys()
    {
      var im = InputManager.Instance;
      im.unregisterKeyMapping(ToggleHideSceneryKey);
      im.unregisterKeyMapping(ToggleNoneSelectionKey);
      im.unregisterKeyMapping(ToggleIndividualSelectionKey);
      im.unregisterKeyMapping(ToggleBoxSelectionKey);
      im.unregisterKeyMapping(ClearSelectionKey);

      im.unregisterKeyGroup(Group);
    }
  }
}
