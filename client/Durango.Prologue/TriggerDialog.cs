using System;
using System.Collections;
using System.Collections.Generic;
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
		int count2 = _dialogs.Count;
		for (int i = 0; i < count2; i++)
		{
			DialogElem dialog = _dialogs[i];
			TalkerInfo talker = GetTalker(dialog.talkerIndex);
			if (dialog._faceToPlayer)
			{
				BoneLookAtTarget component = talker.GetGameObject().GetComponent<BoneLookAtTarget>();
				if ((bool)component)
				{
					component.SetLookTarget(PlayerBehavior.LocalPlayer.gameObject, findHead: true);
				}
				float y = Maths.CalcYawWithTarget(PlayerBehavior.LocalPlayer.transform.position, talker.GetGameObject().transform.position);
				TweenRotation tweenRotation = TweenRotation.Begin(talker.GetGameObject(), 0.5f, Quaternion.Euler(0f, y, 0f));
				tweenRotation.method = UITweener.Method.EaseOut;
				tweenRotation.PlayForward();
			}
			if (dialog._localPlayerFaceToTalker)
			{
				BoneLookAtTarget component2 = PlayerBehavior.LocalPlayer.gameObject.GetComponent<BoneLookAtTarget>();
				if ((bool)component2)
				{
					component2.SetLookTarget(talker.GetGameObject(), findHead: true);
				}
			}
			if (dialog.motion != string.Empty && dialog.motion != "_None_")
			{
				talker.MotionPlayable.CrossFade(dialog.motion, 0.5f, (talker.MotionPlayable.GetWrapMode(dialog.motion) & WrapMode.Loop) > WrapMode.Default);
			}
			Singleton<PrologueManager>.Instance().NPCFloatingGroup.ShowChatMsg(talker.BubbleTalkable, ConditionalText.Format(dialog.text), this);
			yield return new WaitForSeconds(dialog.duration);
			Singleton<PrologueManager>.Instance().NPCFloatingGroup.ShowChatMsg(talker.BubbleTalkable, null, this);
		}
		count2 = _talkers.Count;
		for (int j = 0; j < count2; j++)
		{
		}
		if ((bool)_onFinishListener)
		{
			if (!_onFinishListener.activeSelf)
			{
				_onFinishListener.SetActive(value: true);
			}
			if (_onFinishCmd != string.Empty)
			{
				_onFinishListener.SendMessage(_onFinishCmd);
			}
		}
	}
}
