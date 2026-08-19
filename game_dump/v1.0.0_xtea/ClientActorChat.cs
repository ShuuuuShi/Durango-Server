using System;
using System.Collections;
using System.Collections.Generic;
using ChatData;
using Messages;
using Player;
using Shared.Chat;
using UnityEngine;

public class ClientActorChat : MonoBehaviour
{
	[Serializable]
	public class DialogElem
	{
		[LocalizableString]
		public string text;

		public float duration;

		public PortraitEmotion emotion;

		public DialogElem()
		{
			duration = 1f;
		}
	}

	[SerializeField]
	private float _beginDelay;

	[SerializeField]
	[HideInInspector]
	private bool _randomSequence;

	[HideInInspector]
	[SerializeField]
	private float _chatActivateDistance = 600f;

	[SerializeField]
	private int _portraitType;

	[SerializeField]
	[HideInInspector]
	public List<DialogElem> _dialogs = new List<DialogElem>();

	[SerializeField]
	private string _groupTag = string.Empty;

	[SerializeField]
	private float _activateGroupRange = 1000f;

	private int _cursor;

	private List<ClientActorChat> _groupMembers;

	private CharacterBehavior _owner;

	private ChatableBase _ownerChatter;

	private Coroutine _curDialogCoroutine;

	private Renderer _mainRenderer;

	public bool RandomSequence
	{
		get
		{
			return _randomSequence;
		}
		set
		{
			_randomSequence = value;
		}
	}

	public float ChatActivateDistance
	{
		get
		{
			return _chatActivateDistance;
		}
		set
		{
			_chatActivateDistance = value;
		}
	}

	public bool IsChatAvailable
	{
		get
		{
			if (IsGroupChatter)
			{
				int count = _groupMembers.Count;
				for (int i = 0; i < count; i++)
				{
					ClientActorChat clientActorChat = _groupMembers[i];
					if (clientActorChat.IsPlayerInChatArea)
					{
						return true;
					}
				}
				return false;
			}
			return IsPlayerInChatArea;
		}
	}

	public bool IsPlayerInChatArea
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = PlayerBehavior.LocalPlayer.CurrentPosition - ((Component)this).transform.position;
			return ((Vector3)(ref val)).magnitude < ChatActivateDistance;
		}
	}

	private bool IsGroupChatter => !string.IsNullOrEmpty(GroupTag);

	public ClientActorChat GroupLeader { get; set; }

	public bool AmIGroupLeader => (Object)(object)GroupLeader == (Object)(object)this;

	public virtual int PortraitType => _portraitType;

	public string GroupTag => _groupTag;

	private IEnumerator Start()
	{
		_owner = ((Component)this).GetComponent<CharacterBehavior>();
		_ownerChatter = _owner.ChatableBase;
		if (IsGroupChatter)
		{
			RecruitGroupChattersAndGroupLeader();
		}
		if (_dialogs.Count == 0 || (IsGroupChatter && !AmIGroupLeader))
		{
			yield break;
		}
		bool prevIsAvailable = false;
		while (true)
		{
			bool isAvailable = IsTalkerVisible() && !IsGuideCaptionShowed() && IsChatAvailable;
			if (prevIsAvailable != isAvailable)
			{
				if (isAvailable)
				{
					BeginAllDialogs();
				}
				else
				{
					EndAllDialogs();
				}
			}
			prevIsAvailable = isAvailable;
			yield return (object)new WaitForSeconds(1f);
		}
	}

	private void RecruitGroupChattersAndGroupLeader()
	{
		_groupMembers = new List<ClientActorChat>();
		ClientActorChat[] array = Object.FindObjectsOfType<ClientActorChat>();
		int num = array.Length;
		_groupMembers.Add(this);
		for (int i = 0; i < num; i++)
		{
			ClientActorChat clientActorChat = array[i];
			if (clientActorChat.IsGroupChatter && clientActorChat.GroupTag == GroupTag)
			{
				_groupMembers.Add(clientActorChat);
			}
		}
		if (!FindGroupLeader())
		{
			GroupLeader = this;
		}
	}

	private bool FindGroupLeader()
	{
		int count = _groupMembers.Count;
		for (int i = 0; i < count; i++)
		{
			ClientActorChat clientActorChat = _groupMembers[i];
			if (!((Object)(object)clientActorChat == (Object)(object)this) && clientActorChat.AmIGroupLeader)
			{
				GroupLeader = clientActorChat;
				return true;
			}
		}
		return false;
	}

	private void ProcessAllDialogs(Action<ClientActorChat> func)
	{
		if (func == null)
		{
			return;
		}
		int count = _groupMembers.Count;
		for (int i = 0; i < count; i++)
		{
			ClientActorChat clientActorChat = _groupMembers[i];
			if (!((Object)(object)clientActorChat == (Object)null))
			{
				func(clientActorChat);
			}
		}
	}

	private void BeginAllDialogs()
	{
		if (AmIGroupLeader)
		{
			ProcessAllDialogs(delegate(ClientActorChat chatter)
			{
				chatter.BeginDialog(bResetCursor: true);
			});
		}
		else
		{
			BeginDialog();
		}
	}

	private void EndAllDialogs()
	{
		if (AmIGroupLeader)
		{
			ProcessAllDialogs(delegate(ClientActorChat chatter)
			{
				chatter.EndDialog();
			});
		}
		else
		{
			EndDialog();
		}
	}

	public void BeginDialog(bool bResetCursor = false)
	{
		EndDialog();
		if (bResetCursor)
		{
			_cursor = 0;
		}
		_curDialogCoroutine = ((MonoBehaviour)this).StartCoroutine(CoBeginDialog());
	}

	public void EndDialog()
	{
		if (_curDialogCoroutine != null)
		{
			GameSystem<SocialSystem>.Instance().HideChat(_ownerChatter);
			((MonoBehaviour)this).StopCoroutine(_curDialogCoroutine);
			_curDialogCoroutine = null;
		}
	}

	private IEnumerator CoBeginDialog()
	{
		yield return (object)new WaitForSeconds(_beginDelay);
		while (_dialogs.Count != 0)
		{
			DialogElem dialog = _dialogs[_cursor];
			if (!string.IsNullOrEmpty(dialog.text))
			{
				ChatStruct chat = new ChatStruct
				{
					EntityId = _owner.EntityId,
					Chatter = _ownerChatter,
					Body = new RadioNotice
					{
						Text = LocalizeSystem.Get(dialog.text)
					},
					Name = _ownerChatter.ChatterName,
					Emotion = dialog.emotion,
					Type = ChannelType.System,
					Duration = dialog.duration,
					IsVolatile = true
				};
				GameSystem<SocialSystem>.Instance().AddChat(chat);
			}
			yield return (object)new WaitForSeconds(dialog.duration);
			if (RandomSequence)
			{
				_cursor = (_cursor + Random.Range(1, _dialogs.Count)) % _dialogs.Count;
			}
			else
			{
				_cursor = (_cursor + 1) % _dialogs.Count;
			}
		}
	}

	public bool IsTalkerVisible()
	{
		if ((Object)(object)_mainRenderer == (Object)null)
		{
			_mainRenderer = (Renderer)(object)((Component)this).gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		}
		if (Object.op_Implicit((Object)(object)_mainRenderer))
		{
			return _mainRenderer.isVisible;
		}
		return true;
	}

	private static bool IsGuideCaptionShowed()
	{
		return UIManager.FindScript<PlayGuideGroup>().IsOpen;
	}

	public void OnRemove(int index)
	{
		_dialogs.RemoveAt(index);
	}

	public void Upper(int index)
	{
		if (index > 0)
		{
			DialogElem item = _dialogs[index];
			_dialogs.RemoveAt(index);
			_dialogs.Insert(index - 1, item);
		}
	}

	public void Lower(int index)
	{
		if (index < _dialogs.Capacity - 1)
		{
			DialogElem item = _dialogs[index];
			_dialogs.RemoveAt(index);
			_dialogs.Insert(index + 1, item);
		}
	}

	public void OnAdd()
	{
		_dialogs.Add(new DialogElem());
	}
}
