using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

	[CompilerGenerated]
	private sealed class _003CCoBeginDialog_003Ed__46 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ClientActorChat _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoBeginDialog_003Ed__46(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			ClientActorChat clientActorChat = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(clientActorChat._beginDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				break;
			case 2:
				_003C_003E1__state = -1;
				if (clientActorChat.RandomSequence)
				{
					clientActorChat._cursor = (clientActorChat._cursor + UnityEngine.Random.Range(1, clientActorChat._dialogs.Count)) % clientActorChat._dialogs.Count;
				}
				else
				{
					clientActorChat._cursor = (clientActorChat._cursor + 1) % clientActorChat._dialogs.Count;
				}
				break;
			}
			if (clientActorChat._dialogs.Count != 0)
			{
				DialogElem dialogElem = clientActorChat._dialogs[clientActorChat._cursor];
				if (!string.IsNullOrEmpty(dialogElem.text))
				{
					ChatStruct chat = new ChatStruct
					{
						EntityId = clientActorChat._owner.EntityId,
						Chatter = clientActorChat._ownerChatter,
						Body = new RadioNotice
						{
							Text = LocalizeSystem.Get(dialogElem.text)
						},
						Name = clientActorChat._ownerChatter.ChatterName,
						Emotion = dialogElem.emotion,
						Type = ChannelType.System,
						Duration = dialogElem.duration,
						IsVolatile = true
					};
					GameSystem<SocialSystem>.Instance().AddChat(chat);
				}
				_003C_003E2__current = new WaitForSeconds(dialogElem.duration);
				_003C_003E1__state = 2;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CStart_003Ed__38 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ClientActorChat _003C_003E4__this;

		private bool _003CprevIsAvailable_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CStart_003Ed__38(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			ClientActorChat clientActorChat = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
			}
			else
			{
				_003C_003E1__state = -1;
				clientActorChat._owner = clientActorChat.GetComponent<CharacterBehavior>();
				clientActorChat._ownerChatter = clientActorChat._owner.ChatableBase;
				if (clientActorChat.IsGroupChatter)
				{
					clientActorChat.RecruitGroupChattersAndGroupLeader();
				}
				if (clientActorChat._dialogs.Count == 0 || (clientActorChat.IsGroupChatter && !clientActorChat.AmIGroupLeader))
				{
					return false;
				}
				_003CprevIsAvailable_003E5__2 = false;
			}
			bool flag = clientActorChat.IsTalkerVisible() && !IsGuideCaptionShowed() && clientActorChat.IsChatAvailable;
			if (_003CprevIsAvailable_003E5__2 != flag)
			{
				if (flag)
				{
					clientActorChat.BeginAllDialogs();
				}
				else
				{
					clientActorChat.EndAllDialogs();
				}
			}
			_003CprevIsAvailable_003E5__2 = flag;
			_003C_003E2__current = new WaitForSeconds(1f);
			_003C_003E1__state = 1;
			return true;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
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
					if (_groupMembers[i].IsPlayerInChatArea)
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CStart_003Ed__38(0)
		{
			_003C_003E4__this = this
		};
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoBeginDialog_003Ed__46(0)
		{
			_003C_003E4__this = this
		};
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
