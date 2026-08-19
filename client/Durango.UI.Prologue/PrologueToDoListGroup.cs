using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Durango.Prologue;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Prologue;

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

	[SerializeField]
	private SoundEventType _todoAddedAudio;

	[SerializeField]
	private SoundEventType _todoRemovedAudio;

	private readonly List<Item> _itemList = new List<Item>();

	private float _addedAudioPlayTime;

	private float _removedAudioPlayTime;

	private void Awake()
	{
		_checkBoxPrefab.gameObject.SetActive(value: false);
		GameSystem<PrologueToDoListSystem>.Instance().ListUpdated += ToDoSystem_ListUpdated;
		GameSystem<PrologueToDoListSystem>.Instance().ProgressUpdated += ToDoSystem_ProgressUpdated;
		GameSystem<PrologueToDoListSystem>.Instance().TextUpdated += ToDoSystem_TextUpdated;
		GameSystem<PrologueToDoListSystem>.Instance().CompletionUpdated += ToDoSystem_CompletionUpdated;
		SoundManager.PrepareEvent(_todoAddedAudio);
		SoundManager.PrepareEvent(_todoRemovedAudio);
	}

	private void ToDoSystem_ListUpdated(List<ToDoBase> sources, bool added)
	{
		int count = sources.Count;
		if (count > 0)
		{
			if (added)
			{
				PlayToDoSound(_todoAddedAudio, ref _addedAudioPlayTime);
			}
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
			Object.Destroy(item2.Control.gameObject);
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
			if (todo.IsCompleted)
			{
				PlayToDoSound(_todoRemovedAudio, ref _removedAudioPlayTime);
			}
			item.Control.Checked = todo.IsCompleted;
		}
	}

	private static void PlayToDoSound(SoundEventType audio, ref float playTime)
	{
		if (Time.time > playTime)
		{
			SoundManager.PlayEvent(audio);
			playTime = Time.time + 1f;
		}
	}

	[ExposedInEditor(null)]
	public void HideToDoList()
	{
		TweenAlpha.Begin(base.gameObject, 0.2f, 0f);
	}

	[ExposedInEditor(null)]
	public void RestoreToDoList()
	{
		TweenAlpha.Begin(base.gameObject, 0.2f, 1f);
	}

	private Item CreateItem(ToDoBase todo)
	{
		GameObject gameObject = _itemParent.gameObject.AddChild(_checkBoxPrefab);
		gameObject.transform.localPosition = _checkBoxPrefab.transform.localPosition;
		gameObject.SetActive(value: true);
		Item item = new Item();
		item.Key = todo.Key;
		PrologueToDoCheckBoxControl component = gameObject.GetComponent<PrologueToDoCheckBoxControl>();
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
			Vector3 localPosition = item.Control.transform.localPosition;
			localPosition.y = num2;
			item.Control.transform.localPosition = localPosition;
			num += item.Control.GetHeight();
		}
	}
}
