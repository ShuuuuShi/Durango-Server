using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		((Component)_loadingUpperBase).gameObject.SetActive(false);
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
			((MonoBehaviour)this).StartCoroutine(CoLoadingLoop(loopSpeed));
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
			if ((Object)(object)uISprite != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)uISprite).gameObject);
			}
		}
		_loadingUppers.Clear();
	}

	private IEnumerator CoLoadingLoop(float loopSpeed)
	{
		_isLoading = true;
		((Component)this).GetComponent<UIWidget>().alpha = 1f;
		float timer = 0f;
		int posIndex = 0;
		while (_isLoading)
		{
			for (int i = 0; i < 6; i++)
			{
				UISprite sprite2 = GetUpper(i);
				sprite2.alpha = Mathf.Clamp01(sprite2.alpha - Time.deltaTime / (loopSpeed * (float)_loopingUpperCount));
			}
			if (timer == 0f)
			{
				posIndex++;
				UISprite sprite = GetUpper(posIndex);
				sprite.alpha = 1f;
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
			((MonoBehaviour)this).StartCoroutine(CoLoadingGauge());
		}
	}

	private IEnumerator CoLoadingGauge()
	{
		_isLoading = true;
		((Component)this).GetComponent<UIWidget>().alpha = 1f;
		float currentRatio = 0f;
		while (_isLoading)
		{
			if (_targetRatio != currentRatio)
			{
				float d = Mathf.Abs(_targetRatio - currentRatio);
				currentRatio = ((!(d < 0.01f)) ? (currentRatio + Time.deltaTime) : _targetRatio);
				SetRatio(currentRatio);
			}
			if (currentRatio == 1f)
			{
				TweenAlpha tweener = ((Component)this).gameObject.GetComponent<TweenAlpha>();
				if ((Object)(object)tweener == (Object)null)
				{
					tweener = ((Component)this).gameObject.AddComponent<TweenAlpha>();
				}
				tweener.tweenFactor = 0f;
				tweener.from = 1f;
				tweener.to = 0f;
				tweener.delay = 0.3f;
				tweener.PlayForward();
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
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		index %= 6;
		if (_loadingUppers.Count <= index)
		{
			for (int i = _loadingUppers.Count; i < index + 1; i++)
			{
				GameObject val = ((Component)((Component)_loadingUpperBase).transform.parent).gameObject.AddChild(((Component)_loadingUpperBase).gameObject);
				_loadingUppers.Add(val.GetComponent<UISprite>());
				float num = (float)(i % 6) * (float)Math.PI / 3f;
				((Component)_loadingUppers[i]).transform.localPosition = (Vector3.up * Mathf.Cos(num) + Vector3.right * Mathf.Sin(num)) * 29f;
				((Component)_loadingUppers[i]).transform.localEulerAngles = Vector3.back * num * 57.29578f;
				_loadingUppers[i].color = _loadingUpperColor;
				((Component)_loadingUppers[i]).gameObject.SetActive(true);
				_loadingUppers[i].alpha = 0f;
			}
		}
		return _loadingUppers[index];
	}
}
