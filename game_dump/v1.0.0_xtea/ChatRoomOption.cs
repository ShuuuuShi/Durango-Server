using System;
using ChatData;
using UnityEngine;

public class ChatRoomOption : TooltipBase
{
	[SerializeField]
	private Selectable _inviteButton;

	[SerializeField]
	private Selectable _renameButton;

	[SerializeField]
	private Selectable _pushToggleButton;

	[SerializeField]
	private Selectable _exitButton;

	private Conversation _conversation;

	public event Action OnInvite;

	public event Action OnRename;

	public event Action OnPushToggle;

	public event Action OnExit;

	protected override void OnAwake()
	{
		Selectable inviteButton = _inviteButton;
		inviteButton.Clicked = (Action)Delegate.Combine(inviteButton.Clicked, new Action(OnClickButton));
		Selectable renameButton = _renameButton;
		renameButton.Clicked = (Action)Delegate.Combine(renameButton.Clicked, new Action(OnClickButton));
		Selectable pushToggleButton = _pushToggleButton;
		pushToggleButton.Clicked = (Action)Delegate.Combine(pushToggleButton.Clicked, new Action(OnClickButton));
		Selectable exitButton = _exitButton;
		exitButton.Clicked = (Action)Delegate.Combine(exitButton.Clicked, new Action(OnClickButton));
	}

	private void OnClickButton()
	{
		Selectable current = Selectable.Current;
		if ((Object)(object)current == (Object)(object)_inviteButton)
		{
			if (this.OnInvite != null)
			{
				this.OnInvite();
			}
		}
		else if ((Object)(object)current == (Object)(object)_renameButton)
		{
			if (this.OnRename != null)
			{
				this.OnRename();
			}
		}
		else if ((Object)(object)current == (Object)(object)_pushToggleButton)
		{
			_pushToggleButton.Select = !_pushToggleButton.Select;
			if (this.OnPushToggle != null)
			{
				this.OnPushToggle();
			}
		}
		else if ((Object)(object)current == (Object)(object)_exitButton && this.OnExit != null)
		{
			this.OnExit();
		}
		Hide();
	}

	public void Set(Conversation conversation)
	{
		_conversation = conversation;
	}

	protected override void FillData()
	{
		_pushToggleButton.Select = _conversation != null && _conversation.PushEnabled;
	}

	protected override void UpdateLayout()
	{
	}
}
