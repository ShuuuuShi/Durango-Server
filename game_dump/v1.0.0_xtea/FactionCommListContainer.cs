using System;
using System.Collections.Generic;
using Messages;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class FactionCommListContainer : MonoBehaviour
{
	[SerializeField]
	private UILabel _textFactionName;

	[SerializeField]
	private GameObject _NoComm;

	[SerializeField]
	private KScrollView _kScrollView;

	public FactionType CurrentFaction { get; private set; }

	public event Action<FactionType, int> ListItemClicked;

	public void Init()
	{
		_kScrollView.Nodes.Init(delegate(GameObject gameObject)
		{
			UIEventListener.Get(gameObject).onClick = OnClickCommListItem;
		});
		_NoComm.SetActive(false);
		CurrentFaction = FactionType.Invalid;
	}

	public void Refresh(FactionType type)
	{
		FactionSystem factionSystem = GameSystem<FactionSystem>.Instance();
		CurrentFaction = type;
		Yaml.Faction value = null;
		SingletonDict<FactionType, Yaml.Faction>.Instance.TryGetValue(type, out value);
		_textFactionName.text = ((value == null) ? string.Empty : value.name.ToString());
		IList<FactionRadioRecord> factionRecords = GameSystem<FactionSystem>.Instance().GetFactionRecords(type);
		if (factionRecords != null)
		{
			_kScrollView.Nodes.Set(factionRecords.Count);
			for (int i = 0; i < factionRecords.Count; i++)
			{
				int index = factionRecords.Count - (i + 1);
				FactionCommListItem component = _kScrollView.Nodes[index].GetComponent<FactionCommListItem>();
				component.SetRecord(i, factionRecords[i]);
			}
		}
		else
		{
			_kScrollView.Nodes.Clear();
		}
		_kScrollView.Reposition();
		_NoComm.SetActive(factionRecords == null || factionRecords.Count == 0);
	}

	private void OnClickCommListItem(GameObject obj)
	{
		FactionCommListItem component = obj.GetComponent<FactionCommListItem>();
		if ((Object)(object)component != (Object)null && this.ListItemClicked != null)
		{
			this.ListItemClicked(CurrentFaction, component.Index);
		}
	}
}
