using System;
using System.Collections.Generic;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

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
			if (_widget == null)
			{
				_widget = GetComponent<AnimationWidget>();
			}
			return _widget;
		}
	}

	private void Awake()
	{
		UIEventListener.Get(_background.gameObject).onClick = OnClickDeathEffect;
		OnLoaclize();
	}

	public void Play()
	{
		if (_tweenersCount <= 0)
		{
			_playStartTime = Time.time;
			base.gameObject.SetActive(value: true);
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
			VisibleController.Hide(VisibleType.Base, hide: true, "Death");
		}
	}

	private void OnTweenerFinished()
	{
		_tweenersCount--;
		if (_tweenersCount == 0)
		{
			Invoke("Close", 5f);
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
			CancelInvoke("Close");
			if (onFinishedDeathEffect != null)
			{
				onFinishedDeathEffect();
			}
			VisibleController.Hide(VisibleType.Base, hide: false, "Death", 0.2f);
			AnimWidget.Alpha = 0f;
		}
	}

	private void OnLoaclize()
	{
		_deathLabel.text = T._("죽었습니다.");
		float x = _deathLabel.printedSize.x;
		float num = 20f;
		_deathLabelBackground.MakePixelPerfect();
		if ((float)_deathLabelBackground.width < x + num * 2f)
		{
			_deathLabelBackground.width = (int)(x + num * 2f);
		}
	}
}
