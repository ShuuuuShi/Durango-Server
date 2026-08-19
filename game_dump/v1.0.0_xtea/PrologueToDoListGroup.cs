using System.Collections.Generic;
using JetBrains.Annotations;
using PlayGuide;
using UnityEngine;

public class PrologueToDoListGroup : UIBase
{
	private class Item
	{
		public string Key;

		public PrologueToDoCheckBoxControl Control;
	}

	[SerializeField]
	private Transform _itemParent;

	[SerializeField]
	private GameObject _checkBoxPrefab;

	private readonly List<Item> _itemList = new List<Item>();

	private void Awake()
	{
		_checkBoxPrefab.gameObject.SetActive(false);
		GameSystem<PrologueToDoListSystem>.Instance().ListUpdated += ToDoSystem_ListUpdated;
		GameSystem<PrologueToDoListSystem>.Instance().ProgressUpdated += ToDoSystem_ProgressUpdated;
		GameSystem<PrologueToDoListSystem>.Instance().TextUpdated += ToDoSystem_TextUpdated;
		GameSystem<PrologueToDoListSystem>.Instance().CompletionUpdated += ToDoSystem_CompletionUpdated;
	}

	private void ToDoSystem_ListUpdated(List<ToDoBase> sources)
	{
		int count = sources.Count;
		if (count > 0)
		{
			Open();
		}
		else
		{
			Close();
		}
		for (int i = 0; i < count; i++)
		{
			ToDoBase toDoBase = sources[i];
			Item item = FindItem(toDoBase.Key);
			if (item == null)
			{
				item = CreateItem(toDoBase);
				_itemList.Add(item);
			}
		}
		for (int j = 0; j < count; j++)
		{
			ToDoBase toDoBase2 = sources[j];
			int num = FindItemIndex(toDoBase2.Key);
			if (num != j)
			{
				Item value = _itemList[j];
				_itemList[j] = _itemList[num];
				_itemList[num] = value;
			}
		}
		for (int k = count; k < _itemList.Count; k++)
		{
			Item item2 = _itemList[k];
			Object.Destroy((Object)(object)((Component)item2.Control).gameObject);
		}
		_itemList.RemoveRange(count, _itemList.Count - count);
		RePosition();
	}

	private void ToDoSystem_ProgressUpdated([NotNull] ToDoBase todo)
	{
		FindItem(todo.Key)?.Control.SetProgress(todo.CurrentProgress, todo.TargetProgress);
	}

	private void ToDoSystem_TextUpdated([NotNull] ToDoBase todo)
	{
		FindItem(todo.Key)?.Control.SetText(todo.LocalText);
	}

	private void ToDoSystem_CompletionUpdated(ToDoBase todo)
	{
		Item item = FindItem(todo.Key);
		if (item != null)
		{
			item.Control.Checked = todo.IsCompleted;
		}
	}

	[ExposedInEditor(null)]
	public void HideToDoList()
	{
		TweenAlpha.Begin(((Component)this).gameObject, 0.2f, 0f);
	}

	[ExposedInEditor(null)]
	public void RestoreToDoList()
	{
		TweenAlpha.Begin(((Component)this).gameObject, 0.2f, 1f);
	}

	private Item CreateItem(ToDoBase todo)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = ((Component)_itemParent).gameObject.AddChild(_checkBoxPrefab);
		val.transform.localPosition = _checkBoxPrefab.transform.localPosition;
		val.SetActive(true);
		Item item = new Item();
		item.Key = todo.Key;
		PrologueToDoCheckBoxControl component = val.GetComponent<PrologueToDoCheckBoxControl>();
		SetControl(todo, component);
		item.Control = component;
		return item;
	}

	private static void SetControl(ToDoBase todo, PrologueToDoCheckBoxControl control)
	{
		control.SetText(todo.LocalText);
		control.SetProgress(todo.CurrentProgress, todo.TargetProgress);
		control.Checked = todo.IsCompleted;
	}

	private int FindItemIndex(string key)
	{
		for (int i = 0; i < _itemList.Count; i++)
		{
			if (_itemList[i].Key == key)
			{
				return i;
			}
		}
		return -1;
	}

	private Item FindItem(string key)
	{
		int num = FindItemIndex(key);
		return (num == -1) ? null : _itemList[num];
	}

	private void RePosition()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int count = _itemList.Count;
		for (int i = 0; i < count; i++)
		{
			Item item = _itemList[i];
			if (item.Control.TitleVisible && i != 0)
			{
				num += 30;
			}
			int num2 = -55 - num;
			Vector3 localPosition = ((Component)item.Control).transform.localPosition;
			localPosition.y = num2;
			((Component)item.Control).transform.localPosition = localPosition;
			num += item.Control.GetHeight();
		}
	}
}
