using System;
using System.Collections.Generic;
using ClanData;
using L10N;
using UnityEngine;

public class ClanListPage : MonoBehaviour
{
	[SerializeField]
	private UIInput _searchInput;

	[SerializeField]
	private DefaultSelectableButton _searchButton;

	[SerializeField]
	private KScrollView _clanList;

	[SerializeField]
	private GameObject _noData;

	[SerializeField]
	private UIWidget _bottomBar;

	[SerializeField]
	private DefaultSelectableButton _joinButton;

	[SerializeField]
	private DefaultSelectableButton _infoButton;

	private List<Clan> _list = new List<Clan>();

	private Clan _selected;

	private void Start()
	{
		EventDelegate.Set(_searchInput.onSubmit, OnSubmitSearch);
		DefaultSelectableButton searchButton = _searchButton;
		searchButton.Clicked = (Action)Delegate.Combine(searchButton.Clicked, new Action(OnSubmitSearch));
		DefaultSelectableButton joinButton = _joinButton;
		joinButton.Clicked = (Action)Delegate.Combine(joinButton.Clicked, new Action(OnClickJoinButton));
		DefaultSelectableButton infoButton = _infoButton;
		infoButton.Clicked = (Action)Delegate.Combine(infoButton.Clicked, new Action(OnClickInfoButton));
	}

	private void OnEnable()
	{
		((Component)_joinButton).gameObject.SetActive(GameSystem<ClanSystem>.Instance().PlayerClan == null);
		_searchInput.value = string.Empty;
		_clanList.Nodes.Init(OnInitClanListNode);
		_bottomBar.alpha = 0f;
		((Component)_bottomBar).gameObject.SetActive(false);
		SetClanList(null);
		OnSelectClanNode(null);
	}

	private void OnSubmitSearch()
	{
		string value = _searchInput.value;
		if (string.IsNullOrEmpty(value))
		{
			SetClanList(null);
		}
		else
		{
			ClanSystem.RequestClanInfo(value, SetClanList);
		}
	}

	private void OnClickJoinButton()
	{
		Clan clan = _selected;
		if (clan == null)
		{
			return;
		}
		UIManager.MessageBox.Show(T._("<{0}> 부족에 가입 신청 하시겠습니까?", clan.Name), delegate(bool ok)
		{
			if (ok)
			{
				ClanSystem.JoinClan(clan);
			}
		});
	}

	private void OnClickInfoButton()
	{
		if (_selected != null)
		{
			ClanTooltip clanTooltip = UIManager.Popup.Tooltip<ClanTooltip>();
			clanTooltip.Set(_selected);
			clanTooltip.Show();
		}
	}

	private void SetClanList(IList<Clan> list)
	{
		_list.Clear();
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				bool flag = false;
				ulong id = list[i].Id;
				for (int j = 0; j < _list.Count; j++)
				{
					if (_list[j].Id == id)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					_list.AddRange(list);
				}
			}
		}
		RefreshList();
	}

	private void RefreshList()
	{
		ListObjectPool nodes = _clanList.Nodes;
		nodes.Set(KUtility.GetSize(_list));
		for (int i = 0; i < nodes.Count; i++)
		{
			ClanListNode component = nodes[i].GetComponent<ClanListNode>();
			component.Set(_list[i]);
		}
		_clanList.ResetPosition();
		_noData.gameObject.SetActive(nodes.Count == 0);
		OnSelectClanNode(_selected);
	}

	private void OnInitClanListNode(GameObject obj)
	{
		ClanListNode component = obj.GetComponent<ClanListNode>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickClanNode));
	}

	private void OnClickClanNode()
	{
		int index = _clanList.Nodes.IndexOf(((Component)Selectable.Current).gameObject);
		OnSelectClanNode((_selected != _list[index]) ? _list[index] : null);
	}

	private void OnSelectClanNode(Clan clan)
	{
		ListObjectPool nodes = _clanList.Nodes;
		_selected = null;
		for (int i = 0; i < nodes.Count; i++)
		{
			ClanListNode component = nodes[i].GetComponent<ClanListNode>();
			if (_list[i] == clan)
			{
				component.Select = true;
				_selected = _list[i];
			}
			else
			{
				component.Select = false;
			}
		}
		bool enabled = ((Behaviour)_bottomBar).enabled;
		if (enabled != (_selected != null))
		{
			if (!enabled)
			{
				((Component)_bottomBar).gameObject.SetActive(true);
				((Behaviour)_bottomBar).enabled = true;
				AnimationWidget.Get(((Component)_bottomBar).gameObject, 0.2f, 0f, deactiveWhenFadeout: true).Alpha = 1f;
			}
			else
			{
				((Behaviour)_bottomBar).enabled = false;
				AnimationWidget.Get(((Component)_bottomBar).gameObject, 0.2f, 0f, deactiveWhenFadeout: true).Alpha = 0f;
			}
			WidgetLayoutController component2 = ((Component)this).GetComponent<WidgetLayoutController>();
			component2.UpdateLayout();
		}
	}
}
