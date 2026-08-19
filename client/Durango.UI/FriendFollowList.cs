using Durango.UI.Control;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class FriendFollowList : MonoBehaviour
{
	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private GameObject _noData;

	private SocialGroup _parent;

	private KInfiniteScrollView.View<string, FollowPlayerInfoWidget> _view;

	private void Start()
	{
		_view = _scrollView.Initialize<string, FollowPlayerInfoWidget>(PlayerInfoSetter, delegate(FollowPlayerInfoWidget comp)
		{
			comp.RemoveClicked += _parent.CancelFollow;
		});
		_parent = GetComponentInParent<SocialGroup>();
		_parent.AddOnUpdated(Refresh);
	}

	private void OnEnable()
	{
		_scrollView.ResetPosition();
	}

	private void PlayerInfoSetter(PlayerInfoWidget comp, string entityId)
	{
		comp.Set(entityId);
	}

	private void Refresh(Social social)
	{
		int size = KUtility.GetSize(social.FollowingEntityIds);
		if (size > 0)
		{
			_scrollView.gameObject.SetActive(value: true);
			_noData.gameObject.SetActive(value: false);
			_view.SetList(social.FollowingEntityIds);
			_scrollView.Reposition();
		}
		else
		{
			_scrollView.gameObject.SetActive(value: false);
			_noData.gameObject.SetActive(value: true);
		}
	}
}
