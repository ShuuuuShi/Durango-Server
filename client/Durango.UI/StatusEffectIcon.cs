using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Network;
using Durango.Render.Camera;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class StatusEffectIcon : UIWidget
{
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
		Color color;
		string spriteName;
		switch (data.IconColor)
		{
		case "negative":
			color = PresetColor.UIDebuff;
			spriteName = "icon_se_decr";
			break;
		case "positive":
			color = PresetColor.UIBuff;
			spriteName = "icon_se_incr";
			break;
		default:
			color = Color.clear;
			spriteName = string.Empty;
			break;
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
				float num = remainTime / alertRemainTime;
				float num2 = Mathf.Cos(num * 12f * (float)Math.PI);
				SetAlpha(num2 * 0.25f + 0.5f);
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
		IsPlayingEffect = true;
		ChangeClippingChild(_noneClippingPanel);
		Vector3 defaultPos = base.transform.localPosition;
		float startTime = Time.time;
		float timer = 0f;
		while (timer < 3f)
		{
			timer = Time.time - startTime;
			float ratio = Mathf.Sqrt(timer / 3f);
			base.transform.localPosition = defaultPos + Vector3.down * 100f * ratio;
			SetAlpha(1f - ratio);
			yield return null;
		}
		IsPlayingEffect = false;
		OnFinishFadeEffect();
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
		IsPlayingEffect = true;
		ChangeClippingChild(_noneClippingPanel);
		Vector3 defaultPos = MainCamera.WorldToNGUIPos(PlayerBehavior.LocalPlayer.CurrentPosition + Vector3.up * 100f, base.transform.parent);
		int state = 0;
		float startTime = Time.time;
		float effectTime = FadeInStatesTime[state];
		float timer = 0f;
		Vector3 localPosition = default(Vector3);
		while (timer < effectTime)
		{
			timer = Time.time - startTime;
			float ratio = timer / effectTime;
			switch (state)
			{
			case 0:
				base.transform.localPosition = defaultPos + Vector3.up * 60f * Mathf.Pow(ratio, 2f);
				SetAlpha(Mathf.Clamp01(ratio * 2f));
				break;
			case 1:
				base.transform.localPosition = defaultPos + Vector3.up * 10f * ratio;
				break;
			case 2:
				localPosition.x = Mathf.Lerp(defaultPos.x, _fadeInTargetPos.x, Mathf.Pow(ratio, 2f));
				localPosition.y = Mathf.Lerp(defaultPos.y, _fadeInTargetPos.y, Mathf.Pow(ratio, 2f));
				localPosition.z = 0f;
				base.transform.localPosition = localPosition;
				SetAlpha(Mathf.Pow(Mathf.Abs(ratio * 2f - 1f), 3f));
				break;
			}
			if (timer >= effectTime && state < FadeInStatesTime.Length - 1)
			{
				state++;
				startTime = Time.time;
				effectTime = FadeInStatesTime[state];
				timer = 0f;
				defaultPos = base.transform.localPosition;
			}
			yield return null;
		}
		IsPlayingEffect = false;
		if (Index < 0)
		{
			PlayFadeOut();
		}
		else
		{
			OnFinishFadeEffect();
		}
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
			foreach (UIWidget uIWidget in clippingChildWidgets)
			{
				uIWidget.DrawPanel = p;
			}
		}
	}
}
