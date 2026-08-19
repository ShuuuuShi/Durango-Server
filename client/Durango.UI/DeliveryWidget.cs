using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Faction;
using Durango.Logic.Item;
using Durango.UI.Control;
using L10N;
using Messages;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class DeliveryWidget : AnimationWidget
{
	[SerializeField]
	private NestedPrefabLinker _itemListLinker;

	[SerializeField]
	private UILabel _rewardLabel;

	[SerializeField]
	private ItemInfoContainer _itemInfo;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private SelectableButton _marketSearchButton;

	private DeliveryGroup _parent;

	private FactionDeliveryCondition _condition;

	private bool _isShow;

	private Mission? _mission;

	private Messages.MissionToDo? _todo;

	private ItemList _itemList;

	private bool _isInit;

	public SelectableButton ConfirmButton => _confirmButton;

	public event Action<List<ItemData>> DeliveryConfirmed;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_parent = GetComponentInParent<DeliveryGroup>();
		_itemList = _itemListLinker.Object.GetComponent<ItemList>();
		_itemList.MultiIconMode = ItemIconWidget.MultiIconMode.Index;
		ItemList itemList = _itemList;
		itemList.OnUpdateSelectItem = (Action)Delegate.Combine(itemList.OnUpdateSelectItem, new Action(OnUpdateItemSelected));
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, (Action)delegate
		{
			if (_itemList.SelectedList.Count == 0)
			{
				QuickFill();
			}
			else if (this.DeliveryConfirmed != null)
			{
				this.DeliveryConfirmed(_itemList.SelectedList);
			}
		});
		_marketSearchButton.gameObject.SetActive(GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Market));
	}

	private void OnEnable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += RefreshItemList;
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= RefreshItemList;
		_isShow = false;
	}

	private void OnUpdateItemSelected()
	{
		ItemData lastSelectedItem = _itemList.LastSelectedItem;
		_itemInfo.Show(lastSelectedItem);
		UpdateTitleLabel();
		UpdateButtonState();
	}

	public void Set(FactionDeliveryCondition condition)
	{
		Init();
		_isShow = true;
		_condition = condition;
		Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(_parent.Faction);
		if (faction == null || !faction.Mission.HasValue || !faction.Mission.Value.StartedAt.HasValue)
		{
			_mission = null;
		}
		else
		{
			_mission = faction.Mission;
		}
		_rewardLabel.text = ((!_mission.HasValue) ? string.Empty : FactionSystem.MissionRewardToString(_mission.Value.Reward));
		Mission? mission = _mission;
		if (mission.HasValue)
		{
			_todo = GetCurrentNotCompletedTodo(_mission.Value.Todos);
			Messages.MissionToDo? todo = _todo;
			if (todo.HasValue)
			{
				int selectableCount = Mathf.Abs(_todo.Value.GoalCount - _todo.Value.Progress);
				_itemList.SelectableCount = selectableCount;
			}
		}
		Messages.MissionToDo? todo2 = _todo;
		if (!todo2.HasValue)
		{
			_itemList.SelectableCount = condition.Count;
		}
		UpdateTitleLabel();
		RefreshItemList();
		UpdateButtonState();
	}

	public ItemIconWidget GetFirstSelectableEnabledItemOrNull()
	{
		return _itemList.GetFirstSelectableEnabledItemOrNull();
	}

	private void UpdateTitleLabel()
	{
		string prototypeId = _condition.Condition.PrototypeId;
		int prototypeLevel = _condition.Condition.ItemLevel;
		string tagId = _condition.Condition.TagId;
		if (!string.IsNullOrEmpty(prototypeId) || !string.IsNullOrEmpty(tagId))
		{
			_marketSearchButton.Widget.gameObject.SetActive(value: true);
			Durango.Logic.Faction.MissionToDo missionToDo = GameSystem<FactionSystem>.Instance().FindFactionToDoCollection(_parent.Faction)?.GetCurrentToDo();
			if (missionToDo != null)
			{
				_titleLabel.text = $"[icon=icon_map_poi_box:1.3] {missionToDo.DisplayText}";
			}
			_marketSearchButton.Clicked = delegate
			{
				MarketGroup marketGroup = UIManager.FindScript<MarketGroup>();
				marketGroup.OpenAndSearch(prototypeId, prototypeLevel, tagId);
			};
		}
		else
		{
			_marketSearchButton.Widget.gameObject.SetActive(value: false);
			if (_mission.HasValue)
			{
				_titleLabel.text = $"[icon=icon_map_poi_box:1.3] {_mission.Value.Subject}";
			}
		}
		Messages.MissionToDo? todo = _todo;
		if (todo.HasValue)
		{
			int num = _todo.Value.Progress + _itemList.SelectedList.Count;
			int goalCount = _todo.Value.GoalCount;
			_titleLabel.text += $" <em>{num}</em> / {goalCount}";
		}
		else
		{
			_titleLabel.text = string.Empty;
		}
	}

	private Messages.MissionToDo? GetCurrentNotCompletedTodo(Messages.MissionToDo[] todos)
	{
		for (int i = 0; i < todos.Length; i++)
		{
			Messages.MissionToDo value = todos[i];
			if (value.Progress < value.GoalCount)
			{
				return value;
			}
		}
		return null;
	}

	private void UpdateButtonState()
	{
		int count = _itemList.Count;
		_confirmButton.Disabled = count == 0;
		if (_itemList.SelectedList.Count == 0)
		{
			_confirmButton.Text = T._("자동 채우기");
			_confirmButton.SetClickSound(UISound.ClickType.AutoFill);
		}
		else
		{
			_confirmButton.Text = T._("넣기");
			_confirmButton.SetClickSound(UISound.ClickType.ButtonHighlight);
		}
	}

	private void RefreshItemList()
	{
		if (_isShow)
		{
			_itemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, ItemListFilter);
		}
	}

	private bool ItemListFilter(ItemData item)
	{
		ItemTodoCondition condition = _condition.Condition;
		return CheckTags(condition, item) && CheckPrototype(condition, item) && CheckCollectSource(condition, item);
	}

	private void QuickFill()
	{
		List<ItemData> list = new List<ItemData>(_itemList);
		list.Sort(QuickFillItemComparison);
		int num = Mathf.Min(list.Count, _itemList.SelectableCount);
		for (int i = 0; i < num; i++)
		{
			_itemList.SelectItem(list[i], sendEvent: true, scrollTo: false);
		}
	}

	private static int QuickFillItemComparison(ItemData i1, ItemData i2)
	{
		if (i1.Locked != i2.Locked)
		{
			return i1.Locked ? 1 : (-1);
		}
		if (i1.IsEquipments != i2.IsEquipments)
		{
			return i1.IsEquipments ? 1 : (-1);
		}
		return i1.OriginalLevel - i2.OriginalLevel;
	}

	private static bool CheckTags(ItemTodoCondition c, ItemData item)
	{
		if (string.IsNullOrEmpty(c.TagId))
		{
			return true;
		}
		TagData tagData = item.GetTagData(c.TagId);
		return tagData != null && tagData.Level > 0;
	}

	private static bool CheckPrototype(ItemTodoCondition c, ItemData item)
	{
		if (!string.IsNullOrEmpty(c.PrototypeId) && item.PrototypeId != c.PrototypeId)
		{
			return false;
		}
		return item.Level >= c.ItemLevel;
	}

	private static bool CheckCollectSource(ItemTodoCondition c, ItemData item)
	{
		if (!string.IsNullOrEmpty(c.CollectibleId) && c.CollectibleId != item.CollectibleId)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(c.GeneratorId) && c.GeneratorId != item.GeneratorId)
		{
			return false;
		}
		return true;
	}
}
