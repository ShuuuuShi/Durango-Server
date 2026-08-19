using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.Logic.Estate;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Estate;
using UnityEngine;

namespace Durango.UI;

public class ClanListPage : ClanMenuPage, IUIInitializable
{
	[SerializeField]
	private UIInput _searchInput;

	[SerializeField]
	private GameObject _labelSearchNotice;

	[SerializeField]
	private SelectableButton _searchButton;

	[SerializeField]
	private RectLayoutComponent _headerLayout;

	[SerializeField]
	private GameObject _buttonColumn;

	[SerializeField]
	private KInfiniteScrollView _clanList;

	[SerializeField]
	private UILabel _noData;

	[SerializeField]
	private GameObject[] _onlyLandcapeColumn;

	[SerializeField]
	private UIWidget _waitingClanWidget;

	[SerializeField]
	private ClanListNode _waitingClanNode;

	private ClanGroup _parent;

	private KInfiniteScrollView.View<string, ClanListNode> _scrollList;

	private string _buttonText;

	private bool _isSearching;

	private Clan _waitingClan;

	private readonly HashSet<string> _recommendClans = new HashSet<string>();

	private readonly List<string> _clanIdList = new List<string>();

	void IUIInitializable.Init()
	{
		_parent = GetComponentInParent<ClanGroup>();
		_scrollList = _clanList.Initialize<string, ClanListNode>(SetClanNode, OnInitClanListNode);
		EventDelegate.Set(_searchInput.onSubmit, OnSubmitSearch);
		EventDelegate.Set(_searchInput.onChange, OnChangeSearch);
		SelectableButton searchButton = _searchButton;
		searchButton.Clicked = (Action)Delegate.Combine(searchButton.Clicked, new Action(OnSubmitSearch));
		ClanListNode waitingClanNode = _waitingClanNode;
		waitingClanNode.ButtonClicked = (Action<Clan>)Delegate.Combine(waitingClanNode.ButtonClicked, new Action<Clan>(OnCancelJoin));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		bool flag = false;
		_buttonText = null;
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			flag = true;
			_buttonText = T._("가입하기");
		}
		else
		{
			Messages.Member clan = PlayerBehavior.LocalPlayer.Clan;
			if (clan.ClanId == playerClan.Id && clan.RoleId == 0)
			{
				flag = true;
				_buttonText = T._("동맹 맺기");
			}
		}
		if (_buttonColumn.gameObject.activeSelf != flag)
		{
			_buttonColumn.gameObject.SetActive(flag);
			_headerLayout.UpdateLayout();
		}
		_searchInput.value = string.Empty;
		_searchInput.isSelected = _parent.RedirectedFromAllyPage;
		ShowRecommendClanList();
		RefeshNoDataText(_parent.RedirectedFromAllyPage);
		ShowWaitingClan(_waitingClan != null);
	}

	public void UpdateWaitingClan(Clan waitingClan)
	{
		_waitingClan = waitingClan;
		if (waitingClan == null)
		{
			ShowWaitingClan(isShow: false);
		}
		else
		{
			ShowWaitingClan(isShow: true);
		}
	}

	protected override void UpdateLayout()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_onlyLandcapeColumn); i < size; i++)
		{
			_onlyLandcapeColumn[i].SetActive(!_parent.IsPortrait);
		}
		base.UpdateLayout();
	}

	private void SetClanNode(ClanListNode node, string clanId)
	{
		node.Set(clanId, _buttonText);
	}

	private void OnSubmitSearch()
	{
		if (!_isSearching)
		{
			string value = _searchInput.value;
			if (string.IsNullOrEmpty(value))
			{
				ShowRecommendClanList();
				return;
			}
			_isSearching = true;
			UIManager.Popup.LoadingRing.AttachToWidget(_noData.gameObject, base.gameObject);
			_clanList.Panel.alpha = 0f;
			_noData.gameObject.SetActive(value: false);
			GameSystem<ClanSystem>.Instance().RequestClanInfo(value, SetClanList);
		}
	}

	private void OnChangeSearch()
	{
		_labelSearchNotice.SetActive(string.IsNullOrEmpty(_searchInput.value));
	}

	private void OnClickClanButton(Clan clan)
	{
		if (clan != null)
		{
			if (GameSystem<ClanSystem>.Instance().PlayerClan == null)
			{
				UIManager.FindScript<ClanGroup>().JoinClan(clan);
			}
			else
			{
				_parent.SuggestAlly(clan.Id);
			}
		}
	}

	private void OnCancelJoin(Clan clan)
	{
		ClanSystem.CancelWaitingClan(clan.Id);
	}

	private void ShowRecommendClanList()
	{
		_recommendClans.Clear();
		string text = ((!(PlayerBehavior.LocalPlayer == null)) ? PlayerBehavior.LocalPlayer.ClanId : null);
		foreach (EstateInfo estate in GameSystem<EstateSystem>.Instance().GetEstates())
		{
			if (_recommendClans.Count >= 10)
			{
				break;
			}
			if (estate.License.Type == OwnerType.ClanEstate && !(estate.Id == text))
			{
				_recommendClans.Add(estate.Id);
			}
		}
		IEnumerable<PlayerBehavior> players = Singleton<PlayerManager>.Instance().GetPlayers();
		foreach (PlayerBehavior item in players)
		{
			if (_recommendClans.Count >= 10)
			{
				break;
			}
			if (item.HasClan && !(item.ClanId == text))
			{
				_recommendClans.Add(item.ClanId);
			}
		}
		_clanIdList.Clear();
		_clanIdList.AddRange(_recommendClans);
		SetList(_clanIdList);
	}

	private void SetClanList(IList<Clan> list)
	{
		_isSearching = false;
		UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
		_clanIdList.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(list); i < size; i++)
		{
			if (list[i] != null && !string.IsNullOrEmpty(list[i].Id))
			{
				_clanIdList.Add(list[i].Id);
			}
		}
		SetList(_clanIdList);
	}

	private void SetList(List<string> list)
	{
		_scrollList.SetList(list);
		_clanList.ResetPosition();
		if (KUtility.GetSize(list) == 0)
		{
			_noData.gameObject.SetActive(value: true);
			RefeshNoDataText(searchingAlly: false);
			_clanList.Panel.alpha = 0f;
		}
		else
		{
			_noData.gameObject.SetActive(value: false);
			_clanList.Panel.alpha = 1f;
		}
	}

	private void RefeshNoDataText(bool searchingAlly)
	{
		_noData.text = ((!searchingAlly) ? T._("검색된 부족이 없습니다.") : T._("검색을 통해 다른 부족과 동맹을 맺을 수 있습니다."));
	}

	private void OnInitClanListNode(ClanListNode node)
	{
		node.Clicked = (Action)Delegate.Combine(node.Clicked, new Action(OnClickClanNode));
		node.ButtonClicked = OnClickClanButton;
	}

	private void OnClickClanNode()
	{
		ClanListNode clanListNode = (ClanListNode)Selectable.Current;
		if (clanListNode.Clan != null)
		{
			ClanInfoPopup clanInfoPopup = UIManager.Popup.Tooltip<ClanInfoPopup>();
			clanInfoPopup.Set(clanListNode.Clan, hideJoin: true);
			clanInfoPopup.Show();
		}
	}

	private void ShowWaitingClan(bool isShow)
	{
		_waitingClanWidget.gameObject.SetActive(isShow);
		if (isShow)
		{
			_waitingClanNode.Set(_waitingClan.Id, T._("가입 취소"));
		}
		UpdateLayout();
		_scrollList.Reset();
		_clanList.UpdateLayout();
		_clanList.Reposition();
	}
}
