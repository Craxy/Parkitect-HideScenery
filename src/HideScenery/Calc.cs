using System;
using Craxy.Parkitect.HideScenery.Selection;
using Craxy.Parkitect.HideScenery.Utils;
using UnityEngine;
using static Craxy.Parkitect.HideScenery.Selection.Visibility;
using static Craxy.Parkitect.HideScenery.Selection.SelectionAction;
using Op = Craxy.Parkitect.HideScenery.Selection.SelectionOperation;
using Handler = Craxy.Parkitect.HideScenery.HideScenerySelectionHandler;

namespace Craxy.Parkitect.HideScenery
{
  sealed class Calc
  {
    private readonly Park park;
    private readonly Handler handler;
    private Options Options => handler.Options;

    public Calc(Handler handler)
    {
      this.handler = handler;
      Debug.Assert(GameController.Instance != null);
      park = GameController.Instance.park;
      Debug.Assert(park != null);
    }

    private bool IsHidden(BuildableObject o) => handler.IsHidden(o);
    private static bool IsPath(BuildableObject o) => o is Path;
    private static bool IsDeco(BuildableObject o) => o is Deco;
    private static bool IsAttraction(BuildableObject o) => o is Attraction;
    private Visibility CalcVisibilityWhenNeitherPathNorDeco(BuildableObject o)
    {
      Debug.Assert(!(o is Path));
      Debug.Assert(!(o is Deco));
      if (park.hideAttractionsEnabled && IsAttraction(o))
      {
        return HiddenByParkitect;
      }
      else
      {
        return Visibility.Block;
      }
    }
    public Visibility BuildableObjectVisibility(BuildableObject o)
    {
      if (o.isPreview)
      {
        return Ignore;
      }

      var isPath = IsPath(o);
      var isDeco = IsDeco(o);

      if (!(isPath || isDeco))
      {
        return CalcVisibilityWhenNeitherPathNorDeco(o);
      }

      if (isPath && park.hidePathsEnabled)
      {
        Mod.Log("HidePathEnabled on Paths");
        return HiddenByParkitect;
      }
      if (isDeco && park.hideSceneryEnabled)
      {
        Mod.Log("HideSceneryEnabled on Deco");
        return HiddenByParkitect;
      }

      return IsHidden(o) ? Hidden : Visible;
    }

    public SelectionAction BoxAction(SelectionOperation op, Bounds bounds, BuildableObject o)
    {
      if (o.isPreview)
      {
        return DoNothing;
      }

      var options = handler.Options.BoxOptions;

      if (IsPath(o))
      {
        return PathBoxAction(op, bounds, o);
      }
      else if (IsDeco(o))
      {
        return DecoBoxAction(op, bounds, o);
      }
      else
      {
        return DoNothing;
      }
    }

    private SelectionAction PathBoxAction(SelectionOperation op, in Bounds bounds, BuildableObject o)
    {
      Debug.Assert(IsPath(o));

      if (park.hidePathsEnabled)
      {
        return DoNothing;
      }

      var options = Options.BoxOptions;

      switch (op)
      {
        case Op.Remove when options.ApplyFiltersOnAddOnly:
          return IsHidden(o) ? Remove : DoNothing;
        case var _ when !options.HidePaths:
          return DoNothing;
        default:
          return CalcAddRemove(op, o);
      }
    }
    private SelectionAction DecoBoxAction(SelectionOperation op, in Bounds bounds, BuildableObject o)
    {
      Debug.Assert(IsDeco(o));

      if (park.hideSceneryEnabled)
      {
        return DoNothing;
      }

      var options = Options.BoxOptions;

      switch (op)
      {
        case Op.Remove when options.ApplyFiltersOnAddOnly:
          return IsHidden(o) ? Remove : DoNothing;
        case var _ when !options.HideScenery:
          return DoNothing;
      }

      var sceneryType = CalcSceneryType(o);
      if (options.SceneryToHide.HasSet(sceneryType))
      {
        switch (sceneryType)
        {
          case SceneryType.Wall:
            return WallBoxAction(op, bounds, o);
          default:
            return CalcAddRemove(op, o);
        }
      }
      else
      {
        return DoNothing;
      }
    }
    private SelectionAction WallBoxAction(SelectionOperation op, in Bounds bounds, BuildableObject o)
    {
      Debug.Assert(CalcSceneryType(o) == SceneryType.Wall);

      var options = Options.BoxOptions.WallOptions;
      if (options.OnlyMatchExactlyInBounds)
      {
        switch (o)
        {
          case Wall wall:
            bool IsInBounds(in Bounds b)
            {
              // bounds.Contains(o.transform.position);
              // ^^^
              // this only checks one point (bottom center)

              var wallHeightBounds = new Bounds();
              var pos = wall.transform.position;
              wallHeightBounds.SetMinMax(pos, pos + new Vector3(0.0f, wall.height, 0.0f));

              return b.Intersects(wallHeightBounds);
            }
            if(!IsInBounds(bounds))
            {
              return DoNothing;
            }
            break;
        }
      }

      if (options.HideOnlyFacingCurrentView)
      {
        switch (o)
        {
          case Wall wall:
            {
              var sidesToHide = BlockSideHelper.CalcFrontSidesFromCurrentView();
              var side = wall.getBuiltOnSide();

              if (sidesToHide.HasSide(side))
              {
                return CalcAddRemove(op, o);
              }
              else
              {
                if (options.UpdateNotFacingCurrentView)
                {
                  return CalcRemove(o);
                }
                else
                {
                  return DoNothing;
                }
              }
            }
          default:
            return CalcAddRemove(op, o);
        }
      }
      else
      {
        return CalcAddRemove(op, o);
      }
    }

    private SelectionAction CalcAdd(BuildableObject o) => IsHidden(o) ? DoNothing : Add;
    private SelectionAction CalcRemove(BuildableObject o) => IsHidden(o) ? Remove : DoNothing;
    private SelectionAction CalcAddRemove(SelectionOperation op, BuildableObject o)
    {
      switch (op)
      {
        case Op.Add:
          return CalcAdd(o);
        case Op.Remove:
          return CalcRemove(o);
        default:
          throw new InvalidOperationException("Unknown Operation " + op);
      }
    }
    private bool IsCategory(BuildableObject o, string category)
      => o.getCategoryTag() == category;
    private bool NameContains(BuildableObject o, string str)
      => o.getUnlocalizedName().Contains(str, StringComparison.InvariantCultureIgnoreCase);
    private bool IsWall(BuildableObject o)
    {
      var hideWallsBy = handler.Options.BoxOptions.WallOptions.HideBy;
      return
          (hideWallsBy.HasSet(HideType.Class) && o is Wall && !(o is Fence))
        ||
          (hideWallsBy.HasSet(HideType.Category) && IsCategory(o, "Structures/Walls"))
        ||
          (hideWallsBy.HasSet(HideType.Name) && NameContains(o, "wall"))
        ;
    }
    private bool IsRoof(BuildableObject o)
    {
      var hideRoofsBy = handler.Options.BoxOptions.RoofOptions.HideBy;
      return
          (hideRoofsBy.HasSet(HideType.Category) && IsCategory(o, "Structures/Roofs"))
        ||
          (hideRoofsBy.HasSet(HideType.Name) && NameContains(o, "roof"))
        ;
    }
    private SceneryType CalcSceneryType(BuildableObject o)
    {
      if (IsWall(o))
      {
        return SceneryType.Wall;
      }
      if (IsRoof(o))
      {
        return SceneryType.Roof;
      }
      return SceneryType.Other;
    }
  }
}
