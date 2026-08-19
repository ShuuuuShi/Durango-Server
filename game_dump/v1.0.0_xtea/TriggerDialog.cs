using System;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using UnityEngine;

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
			_bubbleTalkable = (Component)((bubbleTalker is Component) ? bubbleTalker : null);
			_motionPlayable = (Component)((motionPlayer is Component) ? motionPlayer : null);
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
				KSingleton<PrologueManager>.Instance().NPCFloatingGroup.Add(_talkers[i].BubbleTalkable, this, _isClampPos);
				KSingleton<PrologueManager>.Instance().NPCFloatingGroup.SetNametag(_talkers[i].BubbleTalkable, string.Empty);
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
		((MonoBehaviour)this).StopCoroutine(BeginDialog());
		((MonoBehaviour)this).StartCoroutine(BeginDialog());
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}

	private IEnumerator BeginDialog()
	{
		int count2 = _dialogs.Count;
		for (int j = 0; j < count2; j++)
		{
			DialogElem dialog = _dialogs[j];
			TalkerInfo talker = GetTalker(dialog.talkerIndex);
			if (dialog._faceToPlayer)
			{
				BoneLookAtTarget lookAt2 = talker.GetGameObject().GetComponent<BoneLookAtTarget>();
				if (Object.op_Implicit((Object)(object)lookAt2))
				{
					lookAt2.SetLookTarget(((Component)PlayerBehavior.LocalPlayer).gameObject, bFindHead: true);
				}
				float yaw = KMathUtil.CalcYawWithTarget(((Component)PlayerBehavior.LocalPlayer).transform.position, talker.GetGameObject().transform.position);
				TweenParms parms = new TweenParms();
				parms.Prop("localRotation", (object)Quaternion.Euler(0f, yaw, 0f));
				parms.Ease((EaseType)5);
				HOTween.To((object)talker.GetGameObject().transform, 0.5f, parms);
			}
			if (dialog._localPlayerFaceToTalker)
			{
				BoneLookAtTarget lookAt = ((Component)PlayerBehavior.LocalPlayer).gameObject.GetComponent<BoneLookAtTarget>();
				if (Object.op_Implicit((Object)(object)lookAt))
				{
					lookAt.SetLookTarget(talker.GetGameObject(), bFindHead: true);
				}
			}
			if (dialog.motion != string.Empty && dialog.motion != "_None_")
			{
				talker.MotionPlayable.CrossFade(dialog.motion, 0.5f, (talker.MotionPlayable.GetWrapMode(dialog.motion) & 2) > 0);
			}
			KSingleton<PrologueManager>.Instance().NPCFloatingGroup.ShowChatMsg(talker.BubbleTalkable, ConditionalText.Format(dialog.text), this);
			yield return (object)new WaitForSeconds(dialog.duration);
			KSingleton<PrologueManager>.Instance().NPCFloatingGroup.ShowChatMsg(talker.BubbleTalkable, null, this);
		}
		count2 = _talkers.Count;
		for (int i = 0; i < count2; i++)
		{
		}
		if (Object.op_Implicit((Object)(object)_onFinishListener))
		{
			if (!_onFinishListener.activeSelf)
			{
				_onFinishListener.SetActive(true);
			}
			if (_onFinishCmd != string.Empty)
			{
				_onFinishListener.SendMessage(_onFinishCmd);
			}
		}
	}
}
