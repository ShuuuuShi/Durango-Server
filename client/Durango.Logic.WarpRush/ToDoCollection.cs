using Durango.Logic.PlayGuide;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using Shared.Season2;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.WarpRush;

public class ToDoCollection : Durango.Logic.PlayGuide.ToDoCollection
{
	private class WarpRushToDo : ToDoBase
	{
		public Point2 Tile;

		public float RemainRatio;
	}

	private static NavigateGroup _navigate;

	private readonly ResourceType _resourceType;

	private static NavigateGroup Navigate
	{
		get
		{
			if (_navigate == null)
			{
				_navigate = UIManager.FindScript<NavigateGroup>();
			}
			return _navigate;
		}
	}

	public ToDoCollection(ResourceType resourceType)
	{
		_resourceType = resourceType;
		Title = WarpRushSystem.GetResourceName(_resourceType);
		base.Key = WarpRushSystem.GenerateTodoCollectionKey(resourceType);
		Icon = WarpRushSystem.GetResourceIcon(_resourceType);
		IconSize = 30;
		SetClicked(ToDoCollection_Clicked);
	}

	public override Detail? GetDetail()
	{
		return null;
	}

	public void UpdateOrAdd(Point2 tile, float remainRatio)
	{
		bool flag = IsVisibleTodo(remainRatio);
		for (int i = 0; i < ToDoList.Count; i++)
		{
			if (ToDoList[i] is WarpRushToDo warpRushToDo && !(warpRushToDo.Tile != tile))
			{
				if (flag)
				{
					Add(tile, remainRatio);
				}
				else
				{
					Remove(tile);
				}
				return;
			}
		}
		if (flag)
		{
			Add(tile, remainRatio);
		}
	}

	public static bool IsVisibleTodo(float remainRatio)
	{
		Season2.Quantity quantity = Yaml.Util.Singleton<Constants>.Instance.Season2.CalcQuantity(remainRatio);
		return quantity != Season2.Quantity.Few;
	}

	public void Add(Point2 tile, float remainRatio)
	{
		string key = GenerateKey(tile);
		AddToTodoList(key, tile, remainRatio);
		AddToNavigator(key, tile, remainRatio);
		AddToMapIndicator(key, tile);
		GameSystem<ToDoListSystem>.Instance().SetUpdated(this);
	}

	private void Remove(Point2 tile)
	{
		string key = GenerateKey(tile);
		RemoveFromTodoList(key);
		RemoveFromNavigator(key);
		RemoveFromMapIndicator(key);
		GameSystem<ToDoListSystem>.Instance().SetUpdated(this);
	}

	private void ToDoCollection_Clicked()
	{
		for (int i = 0; i < ToDoList.Count; i++)
		{
			if (ToDoList[i] is WarpRushToDo warpRushToDo)
			{
				AddToNavigator(warpRushToDo.Key, warpRushToDo.Tile, warpRushToDo.RemainRatio);
			}
		}
	}

	private void AddToTodoList(string key, Point2 tile, float remainRatio)
	{
		ToDoBase toDoBase = FindToDo(key);
		if (toDoBase != null)
		{
			if (toDoBase is WarpRushToDo warpRushToDo)
			{
				warpRushToDo.RemainRatio = remainRatio;
			}
		}
		else
		{
			ToDoList.Add(new WarpRushToDo
			{
				Key = key,
				Tile = tile,
				RemainRatio = remainRatio
			});
		}
	}

	private void RemoveFromTodoList(string key)
	{
		ToDoBase toDoBase = FindToDo(key);
		if (toDoBase != null)
		{
			ToDoList.Remove(toDoBase);
		}
	}

	private void AddToMapIndicator(string key, Point2 tile)
	{
		Durango.Utils.Singleton<MapIndicators>.Instance().AddAnnounceBalloon(AnnounceType.GeneralPoint, tile.ToVector2(), key, WarpRushSystem.GetResourceIcon(_resourceType), WarpRushSystem.GetResourceName(_resourceType), 28);
	}

	private static void RemoveFromMapIndicator(string key)
	{
		Durango.Utils.Singleton<MapIndicators>.Instance().RemoveAnnounceBalloon(AnnounceType.GeneralPoint, key);
	}

	private void AddToNavigator(string key, Point2 tile, float remainRatio)
	{
		Vector3 value = Durango.Terrain.Util.TilePositionToClientPosition(tile);
		Season2.Quantity quantity = Yaml.Util.Singleton<Constants>.Instance.Season2.CalcQuantity(remainRatio);
		Navigate.Point.SetTarget(key, new PointTargetController.Arguments
		{
			Position = value,
			Icon = Icon,
			IconSize = IconSize,
			BorderColor = PresetColor.UIWhite,
			Season = Season,
			ShowBg = true
		});
		Navigate.Point.UpdateGauge(key, remainRatio, quantity == Season2.Quantity.Several);
	}

	private static void RemoveFromNavigator(string key)
	{
		Navigate.Point.ClearTarget(key);
	}

	private static string GenerateKey(Point2 tile)
	{
		return $"WarpRush.{tile}";
	}

	public override string[] GetNavigationKey()
	{
		string[] array = new string[ToDoList.Count];
		for (int i = 0; i < ToDoList.Count; i++)
		{
			if (ToDoList[i] is WarpRushToDo warpRushToDo)
			{
				array[i] = warpRushToDo.Key;
			}
		}
		return array;
	}
}
