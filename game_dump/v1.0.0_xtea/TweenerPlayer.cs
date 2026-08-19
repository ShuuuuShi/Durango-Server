using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class TweenerPlayer : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _tweeners;

	[SerializeField]
	private bool _playWhenEnable;

	[SerializeField]
	private bool _playWhenPress;

	[SerializeField]
	private bool _playWhenClick;

	[SerializeField]
	private bool _deactiveWhenFinish;

	[SerializeField]
	private bool _loop;

	private int _tweenersCount;

	private readonly List<EventDelegate> _onAllTweenerFinished = new List<EventDelegate>();

	public List<EventDelegate> OnAllTweenerFinished => _onAllTweenerFinished;

	private void OnEnable()
	{
		if (_playWhenEnable)
		{
			Play();
		}
	}

	[UsedImplicitly]
	private void OnPress(bool press)
	{
		if (press && _playWhenPress)
		{
			Play();
		}
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (_playWhenClick)
		{
			Play();
		}
	}

	public void Play(EventDelegate.Callback finishCallback)
	{
		if (finishCallback != null)
		{
			OnAllTweenerFinished.Clear();
			EventDelegate.Add(OnAllTweenerFinished, finishCallback, oneShot: true);
		}
		Play();
	}

	public void ResetToBeginning()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_tweeners); i < size; i++)
		{
			if (!((Object)(object)_tweeners[i] == (Object)null))
			{
				UITweener[] components = _tweeners[i].GetComponents<UITweener>();
				for (int j = 0; j < components.Length; j++)
				{
					components[j].ResetToBeginning();
					((Behaviour)components[j]).enabled = false;
				}
			}
		}
	}

	[ExposedInEditor(null)]
	public void Play()
	{
		_tweenersCount = 0;
		int i = 0;
		for (int size = KUtility.GetSize(_tweeners); i < size; i++)
		{
			if ((Object)(object)_tweeners[i] == (Object)null)
			{
				continue;
			}
			UITweener[] components = _tweeners[i].GetComponents<UITweener>();
			for (int j = 0; j < components.Length; j++)
			{
				components[j].ResetToBeginning();
				if (Application.isPlaying)
				{
					components[j].PlayForward();
					EventDelegate.Remove(components[j].onFinished, OnTweenerFinished);
					EventDelegate.Add(components[j].onFinished, OnTweenerFinished, oneShot: true);
				}
				else
				{
					((Behaviour)components[j]).enabled = true;
					EditorUpdateLoop.Play((MonoBehaviour)(object)components[j], OnTweenerFinished);
				}
				_tweenersCount++;
			}
		}
	}

	[ExposedInEditor(null)]
	public void Stop()
	{
		if (_tweenersCount != 0)
		{
			_tweenersCount = 0;
			ResetToBeginning();
			EventDelegate.Execute(OnAllTweenerFinished);
		}
	}

	private void OnTweenerFinished()
	{
		_tweenersCount--;
		if (_tweenersCount != 0)
		{
			return;
		}
		if (_loop)
		{
			if (NGUITools.GetActive((Behaviour)(object)this))
			{
				Play();
			}
			return;
		}
		EventDelegate.Execute(OnAllTweenerFinished);
		if (Application.isPlaying)
		{
			if (_deactiveWhenFinish)
			{
				((Component)this).gameObject.SetActive(false);
			}
		}
		else
		{
			Stop();
		}
	}
}
