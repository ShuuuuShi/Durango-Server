using System;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class WarpAcceleratorEffects : MonoBehaviour, AlarmRewardQueue.IMessageGroup, IUIInitializable
{
	[Serializable]
	private struct Effect
	{
		public TweenerPlayer Tween;

		public SoundEventType Sound;
	}

	public enum Type
	{
		Start,
		End,
		Fail,
		Finished,
		FinishedWithShine
	}

	private struct Item
	{
		public Type Type;

		public Messages.WarpAccelerator Info;
	}

	[SerializeField]
	[EnumList(typeof(Type), false, 0, -1)]
	private Effect[] _effects;

	private bool _isPause;

	private bool _isPlaying;

	private Item? _next;

	void IUIInitializable.Init()
	{
		Effect[] effects = _effects;
		for (int i = 0; i < effects.Length; i++)
		{
			Effect effect = effects[i];
			effect.Tween.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (!_isPause)
		{
			Item? next = _next;
			if (next.HasValue && !_isPlaying)
			{
				Item value = _next.Value;
				_next = null;
				Play(value);
			}
		}
	}

	public void Play(Type type, Messages.WarpAccelerator info)
	{
		Item item = default(Item);
		item.Type = type;
		item.Info = info;
		Item item2 = item;
		if (_isPlaying)
		{
			_next = item2;
		}
		else
		{
			Play(item2);
		}
	}

	private void OnFinish()
	{
		_isPlaying = false;
	}

	private void Play(Item item)
	{
		Effect effect = _effects[(int)item.Type];
		GameObject gameObject = effect.Tween.gameObject;
		gameObject.SetActive(value: true);
		int? num = null;
		switch (item.Type)
		{
		case Type.Start:
			num = item.Info.CurrentPhase;
			break;
		case Type.End:
			num = item.Info.CurrentPhase - 1;
			break;
		}
		if (num.HasValue)
		{
			Transform transform = KUtility.FindTransformByName(gameObject, "_Phase");
			if (transform != null)
			{
				UILabel component = transform.GetComponent<UILabel>();
				if (component != null)
				{
					component.text = T._("{0} 단계", num.Value);
				}
			}
		}
		effect.Tween.Play(OnFinish);
		SoundManager.PlayEvent(effect.Sound);
		_isPlaying = true;
	}

	bool AlarmRewardQueue.IMessageGroup.IsPlaying()
	{
		return _isPlaying;
	}

	void AlarmRewardQueue.IMessageGroup.PauseToNext()
	{
		_isPause = true;
	}

	void AlarmRewardQueue.IMessageGroup.Resume()
	{
		_isPause = false;
	}
}
