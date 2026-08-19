using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

[Serializable]
public class CheatCommandPanelContainer : MonoBehaviour
{
	[SerializeField]
	private CheatCommandPanel _baseObject;

	[SerializeField]
	private GameObject _background;

	private readonly Dictionary<string, CheatCommandPanel> _panels = new Dictionary<string, CheatCommandPanel>();

	public int Index { get; private set; }

	public CheatCommandPanel CurrentPanel { get; private set; }

	public void Init(int index)
	{
		Index = index;
		_baseObject.gameObject.SetActive(value: false);
		_background.SetActive(value: false);
	}

	public CheatCommandPanel CreatePanel(string name, string command)
	{
		if (!_panels.TryGetValue(name, out var value))
		{
			value = _baseObject.gameObject.transform.parent.gameObject.AddChild(_baseObject.gameObject).GetComponent<CheatCommandPanel>();
			value.Init(Index, name, command);
			_panels.Add(name, value);
		}
		return value;
	}

	public CheatCommandPanel GetPanel(string name)
	{
		if (_panels.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public CheatCommandPanel ShowPanel(string name)
	{
		CurrentPanel = GetPanel(name);
		foreach (CheatCommandPanel value in _panels.Values)
		{
			value.Show(value == CurrentPanel);
		}
		_background.SetActive(CurrentPanel != null);
		return CurrentPanel;
	}

	public void RefreshToggleButtonSelectStates(Dictionary<string, bool> toggleDictionary)
	{
		foreach (CheatCommandPanel value in _panels.Values)
		{
			value.RefreshToggleButtonSelectStates(toggleDictionary);
		}
	}
}
