using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.UI.Control;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class GatheringCheatWidget : MonoBehaviour
{
	[SerializeField]
	private UIInput _searchInput;

	[SerializeField]
	private GameObject _clearButton;

	[SerializeField]
	private KGridScrollView _collectibleScrollView;

	[SerializeField]
	private KScrollView _itemScrollView;

	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private IntSelector _levelSelector;

	[SerializeField]
	private SelectableButton _gatheringButton;

	[SerializeField]
	private RectLayout _layout;

	private Dictionary<string, string> _collectibles = new Dictionary<string, string>();

	private Generator[] _currentCollectibleInfo;

	private string _currentCollectibleId;

	private void Awake()
	{
		Connections.Frontend.On<Tool_Collectibles>(OnTool_CollectiblesMsg);
	}

	private void Start()
	{
		_levelSelector.Set(60, 1, 60);
		SelectableButton gatheringButton = _gatheringButton;
		gatheringButton.Clicked = (Action)Delegate.Combine(gatheringButton.Clicked, new Action(GatherItem));
		_gatheringButton.Text = "채집";
		_layout.UpdateOnSizeChange();
		Connections.Frontend.Send(new Tool_Collectibles
		{
			Collectibles = new Pair<string, string>[0]
		});
		EventDelegate.Set(_searchInput.onSubmit, delegate
		{
			UpdateCollectibles(_searchInput.value);
		});
		UIEventListener uIEventListener = UIEventListener.Get(_clearButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			UpdateCollectibles(null);
		});
	}

	private void OnTool_CollectiblesMsg(Tool_Collectibles msg, PacketHeader header)
	{
		if (_collectibles.Count != 0)
		{
			return;
		}
		List<Pair<string, string>> list = new List<Pair<string, string>>(msg.Collectibles);
		list.Sort(new CheatItemCategoryNameComparer());
		foreach (Pair<string, string> item in list)
		{
			_collectibles.Add(item.Item1, item.Item2);
		}
		UpdateCollectibles(string.Empty);
	}

	private void UpdateCollectibles(string keyword)
	{
		_collectibleScrollView.gameObject.SetActive(value: true);
		_collectibleScrollView.Nodes.Clear();
		_itemScrollView.Nodes.Clear();
		foreach (KeyValuePair<string, string> collectible in _collectibles)
		{
			if (string.IsNullOrEmpty(keyword) || collectible.Value.Contains(keyword))
			{
				SelectableWidget selectableWidget = _collectibleScrollView.Nodes.Add<SelectableWidget>();
				selectableWidget.Clicked = OnSelectCollectible;
				selectableWidget.transform.Find("Id").GetComponent<UILabel>().text = collectible.Key;
				selectableWidget.transform.Find("Name").GetComponent<UILabel>().text = collectible.Value;
			}
		}
		_collectibleScrollView.ResetPosition();
	}

	private void OnSelectCollectible()
	{
		int num = _collectibleScrollView.Nodes.IndexOf(Selectable.Current.gameObject);
		for (int i = 0; i < _collectibleScrollView.Nodes.Count; i++)
		{
			bool selected = i == num;
			_collectibleScrollView.Nodes[i].GetComponent<Selectable>().Selected = selected;
		}
		_currentCollectibleId = Selectable.Current.transform.Find("Id").GetComponent<UILabel>().text;
		Connections.Frontend.Send(new Cheat
		{
			_Cheat = "collectible " + _currentCollectibleId
		}).On<Collectible>(OnCollectible);
	}

	private void OnCollectible(Collectible msg, PacketHeader header)
	{
		_currentCollectibleInfo = msg.Generators;
		_title.text = _collectibles.Get(msg.CollectibleId, "???");
		_itemScrollView.Nodes.BeginLoad();
		Generator[] currentCollectibleInfo = _currentCollectibleInfo;
		for (int i = 0; i < currentCollectibleInfo.Length; i++)
		{
			Generator generator = currentCollectibleInfo[i];
			GameObject next = _itemScrollView.Nodes.GetNext();
			next.GetComponent<KeyValueLabel>().SetKey(generator.Name);
			next.GetComponent<ToggleWidget>().SetOptions(new string[6] { "0", "1", "2", "3", "4", "5" });
		}
		_itemScrollView.Nodes.EndLoad();
		_itemScrollView.ResetPosition();
	}

	private void GatherItem()
	{
		for (int i = 0; i < _itemScrollView.Nodes.Count; i++)
		{
			int index = _itemScrollView.Nodes[i].GetComponent<ToggleWidget>().Index;
			if (index > 0)
			{
				Tool_Collect tool_Collect = default(Tool_Collect);
				tool_Collect.CollectibleId = $"{_currentCollectibleId}:{_levelSelector.Value}";
				tool_Collect.GeneratorId = _currentCollectibleInfo[i].Id;
				Tool_Collect msg = tool_Collect;
				for (int j = 0; j < index; j++)
				{
					Connections.Frontend.Send(msg);
				}
			}
		}
	}
}
