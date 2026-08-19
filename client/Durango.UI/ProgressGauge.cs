using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Logic.Timer;
using Durango.Render.Camera;
using UnityEngine;

namespace Durango.UI;

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
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
				if (_widget == null)
				{
					_widget = base.gameObject.AddComponent<UIWidget>();
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
		get
		{
			return _playCoroutine;
		}
		set
		{
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
		if (!IsPlayCoroutine)
		{
			if (Target == null)
			{
				Target = PlayerBehavior.LocalPlayer.MainTransform;
			}
			SetFadeInWidget(Widget);
			StartCoroutine(CoGaugeRoutine());
		}
	}

	protected virtual void Reposition()
	{
		if (Target == null)
		{
			if (_hasTarget)
			{
				IsPlayCoroutine = false;
			}
		}
		else
		{
			base.transform.localPosition = MainCamera.WorldToNGUIPos(Target.position + PositionOffset);
		}
	}

	public void SetOffset(Vector3 offset)
	{
		PositionOffset = offset;
		Reposition();
	}

	public void SetTarget(GameObject target)
	{
		if (target == null)
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
		SetTarget(target);
		SetOffset(offset);
	}

	protected void SetFadeInWidget(UIWidget widget)
	{
		_fadeInWidget.Clear();
		_fadeInWidget.Add(widget);
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
			while (Timer != null && Time.time < Timer.Since)
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
		if (IsTimerAlive())
		{
			while (Timer != null)
			{
				timer2 += Time.deltaTime;
				int j = 0;
				for (int count = _fadeInWidget.Count; j < count; j++)
				{
					_fadeInWidget[j].alpha = Mathf.Clamp01(timer2 / 0.5f);
				}
				float now = Time.time;
				float current = now - Timer.Since;
				float duration = Timer.Duration;
				float ratio = ((!(duration > 0f)) ? 0f : (current / duration));
				DrawGauge(ratio);
				Reposition();
				if (!IsTimerAlive())
				{
					break;
				}
				yield return null;
			}
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
