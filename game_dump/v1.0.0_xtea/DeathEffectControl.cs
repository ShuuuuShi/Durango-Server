using System;
using System.Collections.Generic;
using L10N;
using UnityEngine;

public class DeathEffectControl : MonoBehaviour
{
	public Action onFinishedDeathEffect;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _sideBackground;

	[SerializeField]
	private UILabel _deathLabel;

	[SerializeField]
	private UIWidget _deathLabelBackground;

	[SerializeField]
	private UILabel _deathDescriptionLabel;

	[SerializeField]
	private UISprite _deathIcon;

	[SerializeField]
	private UISprite _deathIconBackground;

	[SerializeField]
	private GameObject[] _tweenerObjects;

	private float _playStartTime;

	private AnimationWidget _widget;

	private int _tweenersCount;

	public AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _widget;
		}
	}

	private void Awake()
	{
		UIEventListener.Get(((Component)_background).gameObject).onClick = OnClickDeathEffect;
		OnLoaclize();
	}

	public void Play()
	{
		if (_tweenersCount <= 0)
		{
			_playStartTime = Time.time;
			((Component)this).gameObject.SetActive(true);
			AnimWidget.SetAlpha(1f, useTween: false);
			List<UITweener> list = new List<UITweener>();
			int i = 0;
			for (int num = _tweenerObjects.Length; i < num; i++)
			{
				list.AddRange(_tweenerObjects[i].GetComponents<UITweener>());
			}
			_tweenersCount = list.Count;
			for (int j = 0; j < _tweenersCount; j++)
			{
				UITweener uITweener = list[j];
				EventDelegate.Add(uITweener.onFinished, OnTweenerFinished, oneShot: true);
				uITweener.ResetToBeginning();
				uITweener.PlayForward();
			}
			UIBase.HideUI(UIBase.UIFlag.Base, hide: true, "Death");
		}
	}

	public void SetDescription(string str)
	{
		_deathDescriptionLabel.text = str;
	}

	private void OnTweenerFinished()
	{
		_tweenersCount--;
		if (_tweenersCount == 0)
		{
			((MonoBehaviour)this).Invoke("Close", 5f);
		}
	}

	private void OnClickDeathEffect(GameObject go)
	{
		if (Time.time - _playStartTime > 5f)
		{
			_tweenersCount = 0;
		}
		Close();
	}

	private void Close()
	{
		if (_tweenersCount <= 0)
		{
			((MonoBehaviour)this).CancelInvoke("Close");
			if (onFinishedDeathEffect != null)
			{
				onFinishedDeathEffect();
			}
			UIBase.HideUI(UIBase.UIFlag.Base, hide: false, "Death");
			AnimWidget.Alpha = 0f;
		}
	}

	private void OnLoaclize()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		_deathLabel.text = T._("죽음");
		float x = _deathLabel.printedSize.x;
		float num = 20f;
		_deathLabelBackground.MakePixelPerfect();
		if ((float)_deathLabelBackground.width < x + num * 2f)
		{
			_deathLabelBackground.width = (int)(x + num * 2f);
		}
	}
}
