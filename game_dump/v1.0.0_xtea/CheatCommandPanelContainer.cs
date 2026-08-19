using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CheatCommandPanelContainer : MonoBehaviour
{
	[SerializeField]
	private CheatCommandPanel _baseObject;

	[SerializeField]
	private GameObject _background;

	private readonly Dictionary<string, CheatCommandPanel> panels = new Dictionary<string, CheatCommandPanel>();

	public int Index { get; private set; }

	public CheatCommandPanel CurrentPanel { get; private set; }

	public void Init(int index)
	{
		Index = index;
		((Component)_baseObject).gameObject.SetActive(false);
		_background.SetActive(false);
	}

	public CheatCommandPanel CreatePanel(string name, string command)
	{
		if (!panels.TryGetValue(name, out var value))
		{
			GameObject val = ((Component)((Component)_baseObject).gameObject.transform.parent).gameObject.AddChild(((Component)_baseObject).gameObject);
			value = val.GetComponent<CheatCommandPanel>();
			value.Init(Index, name, command);
			panels.Add(name, value);
		}
		return value;
	}

	public CheatCommandPanel GetPanel(string name)
	{
		CheatCommandPanel value;
		return (!panels.TryGetValue(name, out value)) ? null : value;
	}

	public CheatCommandPanel ShowPanel(string name)
	{
		CurrentPanel = GetPanel(name);
		foreach (CheatCommandPanel value in panels.Values)
		{
			value.Show((Object)(object)value == (Object)(object)CurrentPanel);
		}
		_background.SetActive((Object)(object)CurrentPanel != (Object)null);
		return CurrentPanel;
	}

	public void RefreshToggleButtonSelectStates(Dictionary<string, bool> toggleDictionary)
	{
		foreach (CheatCommandPanel value in panels.Values)
		{
			value.RefreshToggleButtonSelectStates(toggleDictionary);
		}
	}
}
