using System;
using System.Collections.Generic;
using Durango.Logic.Social;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class ChatRoomOption : TooltipBase
{
	[SerializeField]
	private UIWidget _container;

	[SerializeField]
	private UILabel _textMemberCount;

	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private Selectable _renameButton;

	[SerializeField]
	private Selectable _exitButton;

	[SerializeField]
	private RectLayout _layout;

	private KInfiniteScrollView.View<string, ChattingMemberNode> _view;

	protected readonly List<string> EntityIds = new List<string>();

	private int _height;

	public event Action OnInvite;

	public event Action OnRename;

	public event Action OnExit;

	protected override void OnAwake()
	{
		SoundType = UISound.GroupType.NoSound;
		_view = _scrollView.Initialize(delegate(ChattingMemberNode node, string entityId)
		{
			node.Set(entityId);
		}, delegate(ChattingMemberNode node)
		{
			node.Clicked = (Action)Delegate.Combine(node.Clicked, new Action(OnClickMemberNode));
		});
		Selectable renameButton = _renameButton;
		renameButton.Clicked = (Action)Delegate.Combine(renameButton.Clicked, new Action(RenameButtonClicked));
		Selectable exitButton = _exitButton;
		exitButton.Clicked = (Action)Delegate.Combine(exitButton.Clicked, new Action(ExitButtonClicked));
	}

	public virtual void Set([NotNull] Conversation conversation, int height)
	{
		EntityIds.Clear();
		EntityIds.Add(null);
		EntityIds.AddRange(conversation.GetEntityIds());
		_height = height;
	}

	protected override void FillData()
	{
		if (_textMemberCount != null)
		{
			_textMemberCount.text = $"<em>{EntityIds.Count - 1}</em>";
		}
		_view.SetList(EntityIds);
	}

	protected override void UpdateLayout()
	{
		_container.height = _height;
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void RenameButtonClicked()
	{
		if (this.OnRename != null)
		{
			this.OnRename();
		}
		Hide();
	}

	private void ExitButtonClicked()
	{
		if (this.OnExit != null)
		{
			this.OnExit();
		}
		Hide();
	}

	protected void InviteButtonClicked()
	{
		if (this.OnInvite != null)
		{
			this.OnInvite();
		}
		Hide();
	}

	private void OnClickMemberNode()
	{
		ChattingMemberNode chattingMemberNode = Selectable.Current as ChattingMemberNode;
		if (!(chattingMemberNode != null))
		{
			return;
		}
		if (chattingMemberNode.EntityId != null)
		{
			PlayerInfoPopup.RequestShow(chattingMemberNode.EntityId, delegate(PlayerInfoPopup tooltip)
			{
				base.HideIgnoreParent = tooltip.transform;
				tooltip.AutoPosition = false;
				tooltip.Show();
				tooltip.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
			});
		}
		else
		{
			InviteButtonClicked();
		}
	}
}
