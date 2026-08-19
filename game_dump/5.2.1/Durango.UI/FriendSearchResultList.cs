using System.Collections.Generic;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class FriendSearchResultList : MonoBehaviour
{
	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _noData;

	private KInfiniteScrollView.View<string, FriendSearchPlayerInfoWidget> _view;

	private SocialGroup _parent;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_parent = GetComponentInParent<SocialGroup>();
			_view = _scrollView.Initialize(delegate(FriendSearchPlayerInfoWidget widget, string entityId)
			{
				widget.Set(entityId);
			}, delegate(FriendSearchPlayerInfoWidget comp)
			{
				comp.Requested = _parent.RequestFriend;
			});
		}
	}

	public void Search(string key, string freq)
	{
		Init();
		_titleLabel.text = T._("검색 중");
		_scrollView.gameObject.SetActive(value: false);
		_noData.gameObject.SetActive(value: false);
		UIManager.Popup.LoadingRing.AttachToWidget(base.gameObject);
		Singleton<PlayerInfoManager>.Instance().SearchPlayerInfos(key, freq, OnSearchPlayerInfos);
	}

	private void OnSearchPlayerInfos(IList<FoundPlayerInfo> list)
	{
		UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
		List<string> list2 = FilterPlayerList(list);
		SetList(list2);
		int size = KUtility.GetSize(list2);
		_titleLabel.text = T._("검색 결과 {0}", size);
	}

	private void SetList(IList<string> list)
	{
		Init();
		if (KUtility.GetSize(list) > 0)
		{
			_view.SetList(list);
			_scrollView.ResetPosition();
			_scrollView.gameObject.SetActive(value: true);
			_noData.gameObject.SetActive(value: false);
		}
		else
		{
			_scrollView.gameObject.SetActive(value: false);
			_noData.gameObject.SetActive(value: true);
		}
	}

	private static List<string> FilterPlayerList(IList<FoundPlayerInfo> list)
	{
		List<string> list2 = new List<string>();
		SocialSystem socialSystem = GameSystem<SocialSystem>.Instance();
		int i = 0;
		for (int size = KUtility.GetSize(list); i < size; i++)
		{
			FoundPlayerInfo foundPlayerInfo = list[i];
			if (!socialSystem.IsFriend(foundPlayerInfo.EntityId) && !(foundPlayerInfo.EntityId == PlayerBehavior.LocalPlayer.EntityId))
			{
				list2.Add(foundPlayerInfo.EntityId);
			}
		}
		return list2;
	}
}
