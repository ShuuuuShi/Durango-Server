using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Logic.Timer;
using Durango.Render.Camera;
using UnityEngine;

namespace Durango.UI;

public abstract class ProgressGauge : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoGaugeRoutine_003Ed__42 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ProgressGauge _003C_003E4__this;

		private float _003Ctimer2_003E5__2;

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
		public _003CCoGaugeRoutine_003Ed__42(int _003C_003E1__state)
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
			ProgressGauge progressGauge = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				progressGauge.IsPlayCoroutine = true;
				progressGauge.InitGauge();
				progressGauge.Reposition();
				for (int j = 0; j < progressGauge._fadeInWidget.Count; j++)
				{
					progressGauge._fadeInWidget[j].alpha = 0f;
				}
				if (progressGauge.IsTimerAlive())
				{
					goto IL_00a7;
				}
				goto IL_00c1;
			}
			case 1:
				_003C_003E1__state = -1;
				goto IL_00a7;
			case 2:
				_003C_003E1__state = -1;
				progressGauge.IsPlaying = true;
				_003Ctimer2_003E5__2 = 0f;
				if (progressGauge.IsTimerAlive())
				{
					goto IL_01ac;
				}
				goto IL_01b7;
			case 3:
				_003C_003E1__state = -1;
				goto IL_01ac;
			case 4:
				{
					_003C_003E1__state = -1;
					_003Ctimer2_003E5__2 += Time.deltaTime;
					break;
				}
				IL_01ac:
				if (progressGauge.Timer != null)
				{
					_003Ctimer2_003E5__2 += Time.deltaTime;
					int i = 0;
					for (int count = progressGauge._fadeInWidget.Count; i < count; i++)
					{
						progressGauge._fadeInWidget[i].alpha = Mathf.Clamp01(_003Ctimer2_003E5__2 / 0.5f);
					}
					float num2 = Time.time - progressGauge.Timer.Since;
					float duration = progressGauge.Timer.Duration;
					float ratio = ((!(duration > 0f)) ? 0f : (num2 / duration));
					progressGauge.DrawGauge(ratio);
					progressGauge.Reposition();
					if (progressGauge.IsTimerAlive())
					{
						_003C_003E2__current = null;
						_003C_003E1__state = 3;
						return true;
					}
				}
				goto IL_01b7;
				IL_00a7:
				if (progressGauge.Timer != null && Time.time < progressGauge.Timer.Since)
				{
					if (progressGauge.Timer.IsStop)
					{
						progressGauge.IsPlayCoroutine = false;
						return false;
					}
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_00c1;
				IL_01b7:
				progressGauge.IsPlaying = false;
				_003Ctimer2_003E5__2 = 0f;
				break;
				IL_00c1:
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
			bool num3 = progressGauge.EndedGauge(_003Ctimer2_003E5__2);
			progressGauge.Reposition();
			if (!num3)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 4;
				return true;
			}
			progressGauge.IsPlayCoroutine = false;
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoGaugeRoutine_003Ed__42(0)
		{
			_003C_003E4__this = this
		};
	}

	private bool IsTimerAlive()
	{
		if (Timer != null)
		{
			return !Timer.IsStop;
		}
		return false;
	}

	private void OnDisable()
	{
		IsPlaying = false;
		IsPlayCoroutine = false;
	}
}
