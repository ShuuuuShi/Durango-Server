using Durango.UI.Control;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class FriendBlockList : MonoBehaviour
{
	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private GameObject _noData;

	private SocialGroup _parent;

	private KInfiniteScrollView.View<string, BlockPlayerInfoWidget> _view;

	private void Start()
	{
		_view = _scrollView.Initialize<string, BlockPlayerInfoWidget>(PlayerInfoSetter, delegate(BlockPlayerInfoWidget comp)
		{
			comp.RemoveClicked += _parent.CancelBlock;
		});
		_parent = GetComponentInParent<SocialGroup>();
		_parent.AddOnUpdated(Refresh);
	}

	private void OnEnable()
	{
		_scrollView.ResetPosition();
	}

	private void PlayerInfoSetter(BlockPlayerInfoWidget comp, string entityId)
	{
		comp.Set(entityId);
	}

	private void Refresh(Social social)
	{
		int size = KUtility.GetSize(social.BlockedEntityIds);
		if (size > 0)
		{
			_scrollView.gameObject.SetActive(value: true);
			_noData.gameObject.SetActive(value: false);
			_view.SetList(social.BlockedEntityIds);
			_scrollView.Reposition();
		}
		else
		{
			_scrollView.gameObject.SetActive(value: false);
			_noData.gameObject.SetActive(value: true);
		}
	}
}
