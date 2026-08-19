using System;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class FriendBeRequestedList : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Selectable _requestedAlarmToggleButton;

	[SerializeField]
	private KInfiniteScrollView _scrollList;

	[SerializeField]
	private SelectableButton _acceptAllButton;

	[SerializeField]
	private SelectableButton _rejectAllButton;

	[SerializeField]
	private GameObject _waitAcceptListButton;

	[SerializeField]
	private UILabel _waitAcceptCountLabel;

	[SerializeField]
	private UIWidget _noData;

	[SerializeField]
	private RectLayout _layout;

	private KInfiniteScrollView.View<string, BeRequestedPlayerInfoWidget> _listView;

	private SocialGroup _parent;

	private bool _isInit;

	public event Action WaitAcceptListClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_parent = GetComponentInParent<SocialGroup>();
		_listView = _scrollList.Initialize(delegate(BeRequestedPlayerInfoWidget w, string entityId)
		{
			w.Set(entityId);
		}, delegate(BeRequestedPlayerInfoWidget comp)
		{
			comp.Accepted = _parent.AcceptFriend;
			comp.Rejected = _parent.RejectFriend;
		});
		SetIgnoreFriendRequestedAlarm(GameSystem<SocialSystem>.Instance().IgnoreFriendReqestedAlarm);
		Selectable requestedAlarmToggleButton = _requestedAlarmToggleButton;
		requestedAlarmToggleButton.Clicked = (Action)Delegate.Combine(requestedAlarmToggleButton.Clicked, (Action)delegate
		{
			bool flag = !GameSystem<SocialSystem>.Instance().IgnoreFriendReqestedAlarm;
			GameSystem<SocialSystem>.Instance().IgnoreFriendReqestedAlarm = flag;
			SetIgnoreFriendRequestedAlarm(flag);
		});
		SelectableButton acceptAllButton = _acceptAllButton;
		acceptAllButton.Clicked = (Action)Delegate.Combine(acceptAllButton.Clicked, new Action(_parent.AcceptAllFriend));
		SelectableButton rejectAllButton = _rejectAllButton;
		rejectAllButton.Clicked = (Action)Delegate.Combine(rejectAllButton.Clicked, new Action(_parent.RejectAllFriend));
		_acceptAllButton.Text = T._("모두 수락");
		_rejectAllButton.Text = T._("모두 거절");
		UIEventListener uIEventListener = UIEventListener.Get(_waitAcceptListButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (this.WaitAcceptListClicked != null)
			{
				this.WaitAcceptListClicked();
			}
		});
	}

	public void Set(Social social)
	{
		Init();
		string[] receivedFriendRequests = social.ReceivedFriendRequests;
		int size = KUtility.GetSize(receivedFriendRequests);
		if (size > 0)
		{
			_listView.SetList(receivedFriendRequests);
			_scrollList.gameObject.SetActive(value: true);
			_noData.gameObject.SetActive(value: false);
			_acceptAllButton.Disabled = false;
			_rejectAllButton.Disabled = false;
			_scrollList.Reposition();
		}
		else
		{
			_acceptAllButton.Disabled = true;
			_rejectAllButton.Disabled = true;
			_scrollList.gameObject.SetActive(value: false);
			_noData.gameObject.SetActive(value: true);
		}
		_waitAcceptCountLabel.text = string.Format("{0} >", T._("수락 대기 중 {0}", KUtility.GetSize(social.SentFriendRequests)));
		_layout.UpdateLayout();
		_titleLabel.text = T._("새로 들어온 친구 요청 {0}", size);
		UIUtility.UpdateAnchors(base.transform);
	}

	private void SetIgnoreFriendRequestedAlarm(bool ignore)
	{
		_requestedAlarmToggleButton.Selected = !ignore;
	}
}
