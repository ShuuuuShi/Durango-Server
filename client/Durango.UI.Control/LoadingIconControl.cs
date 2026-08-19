using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Control;

public class LoadingIconControl : MonoBehaviour
{
	private enum LoadingEnum
	{
		Looping,
		Ratio
	}

	[SerializeField]
	private UISprite _loadingUpperBase;

	[SerializeField]
	private Color _loadingUpperColor;

	[SerializeField]
	private int _loopingUpperCount = 3;

	[SerializeField]
	private bool _destoryUpperWhenDisable = true;

	[SerializeField]
	private LoadingEnum _loadingType;

	private readonly List<UISprite> _loadingUppers = new List<UISprite>();

	private bool _isLoading;

	private float _targetRatio;

	private void Awake()
	{
		_loadingUpperBase.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		switch (_loadingType)
		{
		case LoadingEnum.Looping:
			StartLoop();
			break;
		case LoadingEnum.Ratio:
			StartLoadingGauge();
			break;
		}
	}

	private void OnDisable()
	{
		StopLoading();
	}

	private void StartLoop(float loopSpeed = 0.1f)
	{
		if (!_isLoading)
		{
			StartCoroutine(CoLoadingLoop(loopSpeed));
		}
	}

	private void StopLoading()
	{
		_isLoading = false;
		if (!_destoryUpperWhenDisable)
		{
			return;
		}
		for (int num = _loadingUppers.Count - 1; num >= 0; num--)
		{
			UISprite uISprite = _loadingUppers[num];
			if (uISprite != null)
			{
				UnityEngine.Object.Destroy(uISprite.gameObject);
			}
		}
		_loadingUppers.Clear();
	}

	private IEnumerator CoLoadingLoop(float loopSpeed)
	{
		_isLoading = true;
		GetComponent<UIWidget>().alpha = 1f;
		float timer = 0f;
		int posIndex = 0;
		while (_isLoading)
		{
			for (int i = 0; i < 6; i++)
			{
				UISprite upper = GetUpper(i);
				upper.alpha = Mathf.Clamp01(upper.alpha - Time.deltaTime / (loopSpeed * (float)_loopingUpperCount));
			}
			if (timer == 0f)
			{
				posIndex++;
				UISprite upper2 = GetUpper(posIndex);
				upper2.alpha = 1f;
			}
			timer += Time.deltaTime;
			if (timer > loopSpeed)
			{
				timer = 0f;
			}
			yield return null;
		}
		_isLoading = false;
	}

	private void StartLoadingGauge()
	{
		if (!_isLoading)
		{
			_targetRatio = 0f;
			StartCoroutine(CoLoadingGauge());
		}
	}

	private IEnumerator CoLoadingGauge()
	{
		_isLoading = true;
		GetComponent<UIWidget>().alpha = 1f;
		float currentRatio = 0f;
		while (_isLoading)
		{
			if (_targetRatio != currentRatio)
			{
				float num = Mathf.Abs(_targetRatio - currentRatio);
				currentRatio = ((!(num < 0.01f)) ? (currentRatio + Time.deltaTime) : _targetRatio);
				SetRatio(currentRatio);
			}
			if (currentRatio == 1f)
			{
				TweenAlpha tweenAlpha = base.gameObject.GetComponent<TweenAlpha>();
				if (tweenAlpha == null)
				{
					tweenAlpha = base.gameObject.AddComponent<TweenAlpha>();
				}
				tweenAlpha.tweenFactor = 0f;
				tweenAlpha.from = 1f;
				tweenAlpha.to = 0f;
				tweenAlpha.delay = 0.3f;
				tweenAlpha.PlayForward();
				break;
			}
			yield return null;
		}
		_isLoading = false;
	}

	private void SetRatio(float r)
	{
		float num = Mathf.Clamp(r * 100f, 0f, 100f);
		float num2 = 16.666666f;
		int num3 = (int)(num / num2);
		float alpha = num % num2 / num2;
		for (int i = 0; i < num3; i++)
		{
			UISprite upper = GetUpper(i);
			upper.alpha = 1f;
		}
		GetUpper(num3).alpha = alpha;
	}

	private UISprite GetUpper(int index)
	{
		index %= 6;
		if (_loadingUppers.Count <= index)
		{
			for (int i = _loadingUppers.Count; i < index + 1; i++)
			{
				GameObject gameObject = _loadingUpperBase.transform.parent.gameObject.AddChild(_loadingUpperBase.gameObject);
				_loadingUppers.Add(gameObject.GetComponent<UISprite>());
				float num = (float)(i % 6) * (float)Math.PI / 3f;
				_loadingUppers[i].transform.localPosition = (Vector3.up * Mathf.Cos(num) + Vector3.right * Mathf.Sin(num)) * 29f;
				_loadingUppers[i].transform.localEulerAngles = Vector3.back * num * 57.29578f;
				_loadingUppers[i].color = _loadingUpperColor;
				_loadingUppers[i].gameObject.SetActive(value: true);
				_loadingUppers[i].alpha = 0f;
			}
		}
		return _loadingUppers[index];
	}
}
