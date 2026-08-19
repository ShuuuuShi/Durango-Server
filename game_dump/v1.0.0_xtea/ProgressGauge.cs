using System;
using System.Collections;
using System.Collections.Generic;
using TimerData;
using UnityEngine;

public abstract class ProgressGauge : MonoBehaviour
{
	public Action<ProgressGauge> Ended;

	protected Vector3 PositionOffset = Vector3.up * 220f;

	private UIWidget _widget;

	private readonly List<UIWidget> _fadeInWidget = new List<UIWidget>();

	private bool _playCoroutine;

	private bool _hasTarget;

	private bool _isPlaying;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
				if ((Object)(object)_widget == (Object)null)
				{
					_widget = ((Component)this).gameObject.AddComponent<UIWidget>();
				}
			}
			return _widget;
		}
	}

	public Timer Timer { get; set; }

	public bool IsPooledGauge { get; set; }

	public Transform Target { get; private set; }

	public bool IsPlaying
	{
		get
		{
			return _isPlaying;
		}
		private set
		{
			if (_isPlaying != value)
			{
				_isPlaying = value;
				if (value)
				{
					OnPlay();
				}
				else
				{
					OnStop();
				}
			}
		}
	}

	private bool IsPlayCoroutine
	{
		set
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			if (_playCoroutine == value)
			{
				return;
			}
			_playCoroutine = value;
			if (_playCoroutine)
			{
				OnStart();
				return;
			}
			_hasTarget = false;
			Target = null;
			PositionOffset = Vector3.up * 220f;
			OnEnd();
			if (Ended != null)
			{
				Ended(this);
			}
		}
	}

	protected abstract void InitGauge();

	protected abstract void DrawGauge(float ratio);

	protected abstract bool EndedGauge(float timer);

	protected virtual void OnStart()
	{
	}

	protected virtual void OnEnd()
	{
	}

	protected virtual void OnPlay()
	{
	}

	protected virtual void OnStop()
	{
	}

	protected virtual void OnChangeTarget(GameObject target)
	{
	}

	public void Play(Timer timer)
	{
		Timer = timer;
		if (!_playCoroutine)
		{
			if ((Object)(object)Target == (Object)null)
			{
				Target = PlayerBehavior.LocalPlayer.MainTransform;
			}
			SetFadeInWidget(Widget);
			((MonoBehaviour)this).StartCoroutine(CoGaugeRoutine());
		}
	}

	protected virtual void Reposition()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Target == (Object)null)
		{
			if (_hasTarget)
			{
				IsPlayCoroutine = false;
			}
		}
		else
		{
			((Component)this).transform.localPosition = MainCamera.WorldToNGUIPos(Target.position + PositionOffset);
		}
	}

	public void SetOffset(Vector3 offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		PositionOffset = offset;
		Reposition();
	}

	public void SetTarget(GameObject target)
	{
		if ((Object)(object)target == (Object)null)
		{
			_hasTarget = false;
			Target = null;
		}
		else
		{
			_hasTarget = true;
			Target = target.transform;
		}
		Reposition();
		OnChangeTarget(target);
	}

	public void SetTarget(GameObject target, Vector3 offset)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		SetTarget(target);
		SetOffset(offset);
	}

	protected void SetFadeInWidget(params UIWidget[] widgets)
	{
		_fadeInWidget.Clear();
		_fadeInWidget.AddRange(widgets);
	}

	protected void ClearFadeInWidget()
	{
		SetFadeInWidget();
	}

	public float RemainTime()
	{
		return Timer.Duration - (Time.time - Timer.Since);
	}

	private IEnumerator CoGaugeRoutine()
	{
		IsPlayCoroutine = true;
		InitGauge();
		Reposition();
		for (int i = 0; i < _fadeInWidget.Count; i++)
		{
			_fadeInWidget[i].alpha = 0f;
		}
		if (IsTimerAlive())
		{
			while (Time.time < Timer.Since)
			{
				if (Timer.IsStop)
				{
					IsPlayCoroutine = false;
					yield break;
				}
				yield return null;
			}
		}
		yield return null;
		IsPlaying = true;
		float timer2 = 0f;
		while (IsTimerAlive())
		{
			timer2 += Time.deltaTime;
			int j = 0;
			for (int iMax = _fadeInWidget.Count; j < iMax; j++)
			{
				_fadeInWidget[j].alpha = Mathf.Clamp01(timer2 / 0.5f);
			}
			float now = Time.time;
			float current = now - Timer.Since;
			float duration = Timer.Duration;
			float ratio = ((!(duration > 0f)) ? 0f : (current / duration));
			DrawGauge(ratio);
			Reposition();
			yield return null;
		}
		IsPlaying = false;
		timer2 = 0f;
		while (true)
		{
			bool isEnded = EndedGauge(timer2);
			Reposition();
			if (isEnded)
			{
				break;
			}
			yield return null;
			timer2 += Time.deltaTime;
		}
		IsPlayCoroutine = false;
	}

	private bool IsTimerAlive()
	{
		return Timer != null && !Timer.IsStop;
	}

	private void OnDisable()
	{
		IsPlaying = false;
		IsPlayCoroutine = false;
	}
}
