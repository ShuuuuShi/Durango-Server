using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Model;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class TriggerDialog : TriggerOnce
{
	[Serializable]
	public class TalkerInfo
	{
		[SerializeField]
		private Component _bubbleTalkable;

		[SerializeField]
		private Component _motionPlayable;

		public IBubbleTalkable BubbleTalkable => _bubbleTalkable as IBubbleTalkable;

		public IMotionPlayable MotionPlayable => _motionPlayable as IMotionPlayable;

		public string DisplayName => BubbleTalkable.GetDisplayName();

		public TalkerInfo(IBubbleTalkable bubbleTalker, IMotionPlayable motionPlayer)
		{
			_bubbleTalkable = bubbleTalker as Component;
			_motionPlayable = motionPlayer as Component;
		}

		public GameObject GetGameObject()
		{
			return BubbleTalkable.GetGameObject();
		}

		public string[] GetAnimPaths()
		{
			return BubbleTalkable.GetAnimPaths();
		}
	}

	[Serializable]
	public class DialogElem
	{
		public int talkerIndex;

		public string text;

		public float duration;

		public string motion;

		public bool _faceToPlayer;

		public bool _localPlayerFaceToTalker;

		public DialogElem()
		{
			duration = 1f;
		}
	}

	[CompilerGenerated]
	private sealed class _003CBeginDialog_003Ed__17 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TriggerDialog _003C_003E4__this;

		private int _003Ccount2_003E5__2;

		private int _003Ci_003E5__3;

		private TalkerInfo _003Ctalker_003E5__4;

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
		public _003CBeginDialog_003Ed__17(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Ctalker_003E5__4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TriggerDialog triggerDialog = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ccount2_003E5__2 = triggerDialog._dialogs.Count;
				_003Ci_003E5__3 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				Singleton<PrologueManager>.Instance().NPCFloatingGroup.ShowChatMsg(_003Ctalker_003E5__4.BubbleTalkable, null, triggerDialog);
				_003Ctalker_003E5__4 = null;
				_003Ci_003E5__3++;
				break;
			}
			if (_003Ci_003E5__3 < _003Ccount2_003E5__2)
			{
				DialogElem dialogElem = triggerDialog._dialogs[_003Ci_003E5__3];
				_003Ctalker_003E5__4 = triggerDialog.GetTalker(dialogElem.talkerIndex);
				if (dialogElem._faceToPlayer)
				{
					BoneLookAtTarget component = _003Ctalker_003E5__4.GetGameObject().GetComponent<BoneLookAtTarget>();
					if ((bool)component)
					{
						component.SetLookTarget(PlayerBehavior.LocalPlayer.gameObject, findHead: true);
					}
					float y = Maths.CalcYawWithTarget(PlayerBehavior.LocalPlayer.transform.position, _003Ctalker_003E5__4.GetGameObject().transform.position);
					TweenRotation tweenRotation = TweenRotation.Begin(_003Ctalker_003E5__4.GetGameObject(), 0.5f, Quaternion.Euler(0f, y, 0f));
					tweenRotation.method = UITweener.Method.EaseOut;
					tweenRotation.PlayForward();
				}
				if (dialogElem._localPlayerFaceToTalker)
				{
					BoneLookAtTarget component2 = PlayerBehavior.LocalPlayer.gameObject.GetComponent<BoneLookAtTarget>();
					if ((bool)component2)
					{
						component2.SetLookTarget(_003Ctalker_003E5__4.GetGameObject(), findHead: true);
					}
				}
				if (dialogElem.motion != string.Empty && dialogElem.motion != "_None_")
				{
					_003Ctalker_003E5__4.MotionPlayable.CrossFade(dialogElem.motion, 0.5f, (_003Ctalker_003E5__4.MotionPlayable.GetWrapMode(dialogElem.motion) & WrapMode.Loop) > WrapMode.Default);
				}
				Singleton<PrologueManager>.Instance().NPCFloatingGroup.ShowChatMsg(_003Ctalker_003E5__4.BubbleTalkable, ConditionalText.Format(dialogElem.text), triggerDialog);
				_003C_003E2__current = new WaitForSeconds(dialogElem.duration);
				_003C_003E1__state = 1;
				return true;
			}
			_003Ccount2_003E5__2 = triggerDialog._talkers.Count;
			for (int i = 0; i < _003Ccount2_003E5__2; i++)
			{
			}
			if ((bool)triggerDialog._onFinishListener)
			{
				if (!triggerDialog._onFinishListener.activeSelf)
				{
					triggerDialog._onFinishListener.SetActive(value: true);
				}
				if (triggerDialog._onFinishCmd != string.Empty)
				{
					triggerDialog._onFinishListener.SendMessage(triggerDialog._onFinishCmd);
				}
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

	public List<TalkerInfo> _talkers = new List<TalkerInfo>();

	public List<DialogElem> _dialogs = new List<DialogElem>();

	public GameObject _onFinishListener;

	public string _onFinishCmd;

	public bool _isClampPos;

	public bool _forceTriggerAtStart;

	private void Start()
	{
		if (Application.isPlaying)
		{
			int count = _talkers.Count;
			for (int i = 0; i < count; i++)
			{
				Singleton<PrologueManager>.Instance().NPCFloatingGroup.Add(_talkers[i].BubbleTalkable, this, _isClampPos);
				Singleton<PrologueManager>.Instance().NPCFloatingGroup.SetNametag(_talkers[i].BubbleTalkable, string.Empty);
			}
			if (_forceTriggerAtStart)
			{
				TriggerEntered(null);
			}
		}
	}

	public TalkerInfo GetTalker(int talkerIndex)
	{
		return _talkers[talkerIndex];
	}

	public void OnAdd()
	{
		_dialogs.Add(new DialogElem());
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

	protected override bool TriggerEntered(Collider other)
	{
		BeginEvent();
		return true;
	}

	public void BeginEvent()
	{
		StopCoroutine(BeginDialog());
		StartCoroutine(BeginDialog());
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}

	private IEnumerator BeginDialog()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CBeginDialog_003Ed__17(0)
		{
			_003C_003E4__this = this
		};
	}
}
