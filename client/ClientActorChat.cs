using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Logic.Social;
using Durango.UI;
using Messages;
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

	[HideInInspector]
	[SerializeField]
	public List<DialogElem> _dialogs = new List<DialogElem>();

	[SerializeField]
	private float _beginDelay;

	[HideInInspector]
	[SerializeField]
	private bool _randomSequence;

	[HideInInspector]
	[SerializeField]
	private float _chatActivateDistance = 600f;

	[SerializeField]
	private int _portraitType;

	[SerializeField]
	private string _portraitName;

	[SerializeField]
	private string _groupTag = string.Empty;

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

	public bool IsPlayerInChatArea => (PlayerBehavior.LocalPlayer.CurrentPosition - base.transform.position).magnitude < ChatActivateDistance;

	private bool IsGroupChatter => !string.IsNullOrEmpty(GroupTag);

	public ClientActorChat GroupLeader { get; set; }

	public bool AmIGroupLeader => GroupLeader == this;

	public int PortraitType => _portraitType;

	public string PortraitName => _portraitName;

	public string GroupTag => _groupTag;

	private IEnumerator Start()
	{
		_owner = GetComponent<CharacterBehavior>();
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
			yield return new WaitForSeconds(1f);
		}
	}

	private void RecruitGroupChattersAndGroupLeader()
	{
		_groupMembers = new List<ClientActorChat>();
		ClientActorChat[] array = UnityEngine.Object.FindObjectsOfType<ClientActorChat>();
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
			if (!(clientActorChat == this) && clientActorChat.AmIGroupLeader)
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
			if (!(clientActorChat == null))
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
		_curDialogCoroutine = StartCoroutine(CoBeginDialog());
	}

	public void EndDialog()
	{
		if (_curDialogCoroutine != null)
		{
			GameSystem<SocialSystem>.Instance().HideChat(_ownerChatter);
			StopCoroutine(_curDialogCoroutine);
			_curDialogCoroutine = null;
		}
	}

	private IEnumerator CoBeginDialog()
	{
		yield return new WaitForSeconds(_beginDelay);
		while (_dialogs.Count != 0)
		{
			DialogElem dialog = _dialogs[_cursor];
			if (!string.IsNullOrEmpty(dialog.text))
			{
				ChatStruct chatStruct = new ChatStruct();
				chatStruct.EntityId = _owner.EntityId;
				chatStruct.Chatter = _ownerChatter;
				chatStruct.Body = new RadioNotice
				{
					Text = LocalizeSystem.Get(dialog.text)
				};
				chatStruct.Name = _ownerChatter.ChatterName;
				chatStruct.Emotion = dialog.emotion;
				chatStruct.Type = ChannelType.System;
				chatStruct.Duration = dialog.duration;
				chatStruct.IsVolatile = true;
				ChatStruct chat = chatStruct;
				GameSystem<SocialSystem>.Instance().AddChat(chat);
			}
			yield return new WaitForSeconds(dialog.duration);
			if (RandomSequence)
			{
				_cursor = (_cursor + UnityEngine.Random.Range(1, _dialogs.Count)) % _dialogs.Count;
			}
			else
			{
				_cursor = (_cursor + 1) % _dialogs.Count;
			}
		}
	}

	public bool IsTalkerVisible()
	{
		if (_mainRenderer == null)
		{
			_mainRenderer = base.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		}
		if ((bool)_mainRenderer)
		{
			return _mainRenderer.isVisible;
		}
		return true;
	}

	private static bool IsGuideCaptionShowed()
	{
		return UIManager.FindScript<DialogueGroupBase>().IsOpened;
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
