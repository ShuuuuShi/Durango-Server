using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class FriendWaitAcceptList : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private KInfiniteScrollView _scrollList;

	[SerializeField]
	private UIWidget _noData;

	[SerializeField]
	private RectLayout _layout;

	private KInfiniteScrollView.View<string, WaitAcceptPlayerInfoWidget> _listView;

	private SocialGroup _parent;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_parent = GetComponentInParent<SocialGroup>();
			_listView = _scrollList.Initialize(delegate(WaitAcceptPlayerInfoWidget w, string entityId)
			{
				w.Set(entityId);
			}, delegate(WaitAcceptPlayerInfoWidget comp)
			{
				comp.Canceled = _parent.CancelRequest;
			});
		}
	}

	public void Set(Social social)
	{
		Init();
		string[] sentFriendRequests = social.SentFriendRequests;
		int size = KUtility.GetSize(sentFriendRequests);
		if (size > 0)
		{
			_listView.SetList(sentFriendRequests);
			_scrollList.gameObject.SetActive(value: true);
			_noData.gameObject.SetActive(value: false);
			_scrollList.Reposition();
		}
		else
		{
			_scrollList.gameObject.SetActive(value: false);
			_noData.gameObject.SetActive(value: true);
		}
		_layout.UpdateLayout();
		_titleLabel.text = T._("수락 대기중인 친구 {0}", size);
		UIUtility.UpdateAnchors(base.transform);
	}
}
