using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Logic;
using Durango.Network;
using Durango.Render.Camera;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class StatusEffectIcon : UIWidget
{
	[CompilerGenerated]
	private sealed class _003CCoFadeIn_003Ed__50 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public StatusEffectIcon _003C_003E4__this;

		private Vector3 _003CdefaultPos_003E5__2;

		private int _003Cstate_003E5__3;

		private float _003CstartTime_003E5__4;

		private float _003CeffectTime_003E5__5;

		private float _003Ctimer_003E5__6;

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
		public _003CCoFadeIn_003Ed__50(int _003C_003E1__state)
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
			StatusEffectIcon statusEffectIcon = _003C_003E4__this;
			Vector3 localPosition = default(Vector3);
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				statusEffectIcon.IsPlayingEffect = true;
				statusEffectIcon.ChangeClippingChild(statusEffectIcon._noneClippingPanel);
				_003CdefaultPos_003E5__2 = MainCamera.WorldToNGUIPos(PlayerBehavior.LocalPlayer.CurrentPosition + Vector3.up * 100f, statusEffectIcon.transform.parent);
				_003Cstate_003E5__3 = 0;
				_003CstartTime_003E5__4 = Time.time;
				_003CeffectTime_003E5__5 = FadeInStatesTime[_003Cstate_003E5__3];
				_003Ctimer_003E5__6 = 0f;
				localPosition = default(Vector3);
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (_003Ctimer_003E5__6 < _003CeffectTime_003E5__5)
			{
				_003Ctimer_003E5__6 = Time.time - _003CstartTime_003E5__4;
				float num2 = _003Ctimer_003E5__6 / _003CeffectTime_003E5__5;
				switch (_003Cstate_003E5__3)
				{
				case 0:
					statusEffectIcon.transform.localPosition = _003CdefaultPos_003E5__2 + Vector3.up * 60f * Mathf.Pow(num2, 2f);
					statusEffectIcon.SetAlpha(Mathf.Clamp01(num2 * 2f));
					break;
				case 1:
					statusEffectIcon.transform.localPosition = _003CdefaultPos_003E5__2 + Vector3.up * 10f * num2;
					break;
				case 2:
					localPosition.x = Mathf.Lerp(_003CdefaultPos_003E5__2.x, statusEffectIcon._fadeInTargetPos.x, Mathf.Pow(num2, 2f));
					localPosition.y = Mathf.Lerp(_003CdefaultPos_003E5__2.y, statusEffectIcon._fadeInTargetPos.y, Mathf.Pow(num2, 2f));
					localPosition.z = 0f;
					statusEffectIcon.transform.localPosition = localPosition;
					statusEffectIcon.SetAlpha(Mathf.Pow(Mathf.Abs(num2 * 2f - 1f), 3f));
					break;
				}
				if (_003Ctimer_003E5__6 >= _003CeffectTime_003E5__5 && _003Cstate_003E5__3 < FadeInStatesTime.Length - 1)
				{
					_003Cstate_003E5__3++;
					_003CstartTime_003E5__4 = Time.time;
					_003CeffectTime_003E5__5 = FadeInStatesTime[_003Cstate_003E5__3];
					_003Ctimer_003E5__6 = 0f;
					_003CdefaultPos_003E5__2 = statusEffectIcon.transform.localPosition;
				}
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			statusEffectIcon.IsPlayingEffect = false;
			if (statusEffectIcon.Index < 0)
			{
				statusEffectIcon.PlayFadeOut();
			}
			else
			{
				statusEffectIcon.OnFinishFadeEffect();
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
	private sealed class _003CCoFadeOut_003Ed__48 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public StatusEffectIcon _003C_003E4__this;

		private Vector3 _003CdefaultPos_003E5__2;

		private float _003CstartTime_003E5__3;

		private float _003Ctimer_003E5__4;

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
		public _003CCoFadeOut_003Ed__48(int _003C_003E1__state)
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
			StatusEffectIcon statusEffectIcon = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				statusEffectIcon.IsPlayingEffect = true;
				statusEffectIcon.ChangeClippingChild(statusEffectIcon._noneClippingPanel);
				_003CdefaultPos_003E5__2 = statusEffectIcon.transform.localPosition;
				_003CstartTime_003E5__3 = Time.time;
				_003Ctimer_003E5__4 = 0f;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (_003Ctimer_003E5__4 < 3f)
			{
				_003Ctimer_003E5__4 = Time.time - _003CstartTime_003E5__3;
				float num2 = Mathf.Sqrt(_003Ctimer_003E5__4 / 3f);
				statusEffectIcon.transform.localPosition = _003CdefaultPos_003E5__2 + Vector3.down * 100f * num2;
				statusEffectIcon.SetAlpha(1f - num2);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			statusEffectIcon.IsPlayingEffect = false;
			statusEffectIcon.OnFinishFadeEffect();
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

	private static readonly float[] FadeInStatesTime = new float[3] { 0.5f, 0.4f, 1f };

	[SerializeField]
	private UISprite _icon;

	[CanBeNull]
	[SerializeField]
	private UISprite _arrow;

	[CanBeNull]
	[SerializeField]
	private UILabel _stackCountLabel;

	[CanBeNull]
	[SerializeField]
	private UISprite _progressSprite;

	[CanBeNull]
	[SerializeField]
	private UISprite _frameSprite;

	private int _index;

	private Vector3 _fadeInTargetPos;

	private float _alphaRatio = 1f;

	private float _alpha;

	private readonly List<StatusEffect> _groups = new List<StatusEffect>();

	private UIPanel _noneClippingPanel;

	private UIWidget[] _clippingChildWidgets;

	public StatusEffect Data { get; private set; }

	public List<StatusEffect> Groups => _groups;

	public int Index
	{
		get
		{
			return _index;
		}
		set
		{
			if (_index != value)
			{
				_index = value;
				IsRepositionRequired = _index >= 0;
			}
		}
	}

	public bool IsPlayingEffect { get; protected set; }

	public bool IsRepositionRequired { get; private set; }

	public virtual Vector3 Position
	{
		get
		{
			return base.transform.localPosition;
		}
		set
		{
			if (IsPlayingEffect)
			{
				_fadeInTargetPos = value;
			}
			else
			{
				base.transform.localPosition = value;
			}
		}
	}

	public event Action<StatusEffectIcon> FadeEffectFinished;

	private void UpdateProgress()
	{
		if (Groups.Count <= 0 && !(_progressSprite == null))
		{
			if (Data == null || Data.Until <= 0.0)
			{
				_progressSprite.fillAmount = 0f;
				return;
			}
			double since = Data.Since;
			double until = Data.Until;
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			float fillAmount = 1f - Mathf.Clamp01((float)((predictedServerTime - since) / (until - since)));
			_progressSprite.fillAmount = fillAmount;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (!Application.isPlaying)
		{
			return;
		}
		UIPanel uIPanel = panel;
		if (uIPanel != null)
		{
			UIDrawCall.Clipping clipping = uIPanel.clipping;
			if (clipping == UIDrawCall.Clipping.TextureMask || clipping == UIDrawCall.Clipping.SoftClip)
			{
				_clippingChildWidgets = GetComponentsInChildren<UIWidget>();
				_noneClippingPanel = UIUtility.FindComponentInParent<UIPanel>(uIPanel.gameObject);
			}
			if (IsPlayingEffect)
			{
				ChangeClippingChild(_noneClippingPanel);
			}
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying)
		{
			UpdateProgress();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			Index = -1;
			Position = Vector3.zero;
			if (IsPlayingEffect)
			{
				IsPlayingEffect = false;
				OnFinishFadeEffect();
			}
		}
	}

	public void SetAlphaRatio(float ratio)
	{
		_alphaRatio = ratio;
		alpha = _alpha * _alphaRatio;
	}

	public void SetAlpha(float a)
	{
		_alpha = a;
		alpha = _alpha * _alphaRatio;
	}

	public void SetGroup(StatusEffect data, Color col)
	{
		if (Groups.Count == 0)
		{
			Data = data;
			SetIcon(data.Template.UIGroup, col);
		}
		Groups.Add(data);
		if (_arrow != null)
		{
			_arrow.alpha = 0f;
		}
		if (_progressSprite != null)
		{
			_progressSprite.gameObject.SetActive(value: false);
		}
		if (_frameSprite != null)
		{
			_frameSprite.gameObject.SetActive(value: false);
		}
	}

	public void Set(StatusEffect data, Color col)
	{
		Data = data;
		Groups.Clear();
		SetIcon(data.Icon, col);
		string iconColor = data.IconColor;
		Color color;
		string spriteName;
		if (!(iconColor == "negative"))
		{
			if (iconColor == "positive")
			{
				color = PresetColor.UIBuff;
				spriteName = "icon_se_incr";
			}
			else
			{
				color = Color.clear;
				spriteName = string.Empty;
			}
		}
		else
		{
			color = PresetColor.UIDebuff;
			spriteName = "icon_se_decr";
		}
		if (_arrow != null)
		{
			if (color.a > 0f)
			{
				_arrow.color = color;
				_arrow.spriteName = spriteName;
			}
			else
			{
				_arrow.alpha = 0f;
			}
		}
		if (_progressSprite != null)
		{
			_progressSprite.gameObject.SetActive(value: true);
		}
		if (_frameSprite != null)
		{
			_frameSprite.gameObject.SetActive(value: true);
		}
	}

	public void SetStackCount(int count)
	{
		if (_stackCountLabel != null)
		{
			if (count > 1)
			{
				_stackCountLabel.gameObject.SetActive(value: true);
				_stackCountLabel.text = count.ToString();
			}
			else
			{
				_stackCountLabel.gameObject.SetActive(value: false);
			}
		}
	}

	private void SetIcon(string icon, Color col)
	{
		if (!(_icon == null))
		{
			_icon.spriteName = ((!string.IsNullOrEmpty(icon)) ? icon : "icon_question");
			_icon.color = col;
		}
	}

	public void UpdateEffect(float alertRemainTime)
	{
		if (Data != null && !IsPlayingEffect)
		{
			float remainTime = Data.GetRemainTime();
			if (Data.Until > 0.0 && remainTime < alertRemainTime)
			{
				float num = Mathf.Cos(remainTime / alertRemainTime * 12f * (float)Math.PI);
				SetAlpha(num * 0.25f + 0.5f);
			}
			else
			{
				SetAlpha(1f);
			}
		}
	}

	public void PlayFadeOut()
	{
		Index = -1;
		if (!IsPlayingEffect)
		{
			StartCoroutine(CoFadeOut());
		}
	}

	private IEnumerator CoFadeOut()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoFadeOut_003Ed__48(0)
		{
			_003C_003E4__this = this
		};
	}

	public virtual void PlayFadeIn(Vector3 targetPos)
	{
		if (!IsPlayingEffect)
		{
			_fadeInTargetPos = targetPos;
			StartCoroutine(CoFadeIn());
		}
	}

	private IEnumerator CoFadeIn()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoFadeIn_003Ed__50(0)
		{
			_003C_003E4__this = this
		};
	}

	protected void OnFinishFadeEffect()
	{
		ChangeClippingChild(null);
		if (this.FadeEffectFinished != null)
		{
			this.FadeEffectFinished(this);
		}
	}

	private void ChangeClippingChild(UIPanel p)
	{
		if (_clippingChildWidgets != null)
		{
			UIWidget[] clippingChildWidgets = _clippingChildWidgets;
			for (int i = 0; i < clippingChildWidgets.Length; i++)
			{
				clippingChildWidgets[i].DrawPanel = p;
			}
		}
	}
}
