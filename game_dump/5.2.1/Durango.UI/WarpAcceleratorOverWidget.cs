using Durango.Network;
using Durango.Render.Camera;
using JetBrains.Annotations;
using Messages;
using Shared.Accelerator;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class WarpAcceleratorOverWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject _phaseWidget;

	[SerializeField]
	private UISprite _phaseStateSprite;

	[SerializeField]
	private ListObjectPool _phaseStateSeparators;

	[SerializeField]
	private UILabel _phaseLabel;

	[SerializeField]
	private Transform _phaseTimerCursor;

	[SerializeField]
	private WarpAcceleratorInfoWidget.WarpAcceleratorWaveView _waveView;

	[SerializeField]
	private GameObject _waitWidget;

	[SerializeField]
	private UISprite _waitTimerGauge;

	private Artifact _target;

	private Vector3 _targetOffset;

	private bool? _isShowPhaseWidget;

	private bool? _isShowWaitWidget;

	private ListObjectPool<UISprite> _phaseStateSprites;

	private float[] _processingStateRatio;

	private Messages.WarpAccelerator _accelerator;

	private void Awake()
	{
		InitializeStateSprite();
	}

	private void OnDisable()
	{
		Clear();
	}

	private void Clear()
	{
		_target = null;
		_isShowPhaseWidget = null;
		_isShowWaitWidget = null;
		_waveView.Clear();
		_phaseWidget.SetActive(value: false);
		_waitWidget.SetActive(value: false);
	}

	private void InitializeStateSprite()
	{
		_phaseStateSprites = new ListObjectPool<UISprite>();
		_phaseStateSprites.BaseObject = _phaseStateSprite;
		_phaseStateSprites.UseBase = true;
		_phaseStateSprites.BeginLoad();
		_phaseStateSeparators.BeginLoad();
		_processingStateRatio = new float[3];
		float num = 0f;
		for (int i = 0; i < 3; i++)
		{
			float num2 = 0f;
			switch (i)
			{
			case 0:
				num2 = 0.5f;
				break;
			case 1:
			{
				float phaseTime = Singleton<Constants>.Instance.WarpAccelerator.PhaseTime;
				num2 = ((!(phaseTime > 0f)) ? 1f : ((phaseTime - 10f) / phaseTime));
				break;
			}
			case 2:
				num2 = 1f;
				break;
			}
			_processingStateRatio[i] = num2;
			UISprite next = _phaseStateSprites.GetNext();
			UISprite component = _phaseStateSprites.GetNext().GetComponent<UISprite>();
			Transform obj = next.transform;
			Vector3 localEulerAngles = new Vector3(0f, 0f, 144f - 288f * num);
			component.transform.localEulerAngles = localEulerAngles;
			obj.localEulerAngles = localEulerAngles;
			float num4 = (component.fillAmount = (num2 - num) * 288f / 360f);
			float fillAmount = num4;
			next.fillAmount = fillAmount;
			next.depth = component.depth + 1;
			if (i > 0)
			{
				_phaseStateSeparators.GetNext().transform.localEulerAngles = new Vector3(0f, 0f, 144f - 288f * num);
			}
			num = num2;
		}
		_phaseStateSprites.EndLoad();
		_phaseStateSeparators.EndLoad();
	}

	public void Set([NotNull] Artifact target, Messages.WarpAccelerator info)
	{
		if (_target != target)
		{
			Clear();
		}
		_target = target;
		_accelerator = info;
		_targetOffset = target.InteractionPosition - target.transform.position;
		_targetOffset.y = 300f;
		switch (info.Status)
		{
		case AcceleratorStatus.Intermission:
			ShowPhaseWidget(show: false);
			ShowWaitWidget(show: true);
			_waveView.Clear();
			break;
		case AcceleratorStatus.Processing:
			ShowPhaseWidget(show: true);
			ShowWaitWidget(show: false);
			_phaseLabel.text = info.CurrentPhase.ToString();
			_waveView.Set(info.CurrentWave.GetValueOrDefault(), info.CurrentMaxWave.GetValueOrDefault());
			break;
		}
	}

	private void ShowPhaseWidget(bool show)
	{
		if (_isShowPhaseWidget == show)
		{
			return;
		}
		_isShowPhaseWidget = show;
		if (show)
		{
			_phaseWidget.SetActive(value: true);
			TweenRotation tweenRotation = UITweener.Begin<TweenRotation>(_phaseWidget, 0.3f);
			tweenRotation.from = new Vector3(0f, -90f, 0f);
			tweenRotation.to = Vector3.zero;
			tweenRotation.onFinished.Clear();
			tweenRotation.delay = 0.3f;
			tweenRotation.Sample(0f, isFinished: false);
		}
		else if (_phaseWidget.activeSelf)
		{
			TweenRotation tweenRotation2 = UITweener.Begin<TweenRotation>(_phaseWidget, 0.3f);
			tweenRotation2.from = _phaseWidget.transform.eulerAngles;
			tweenRotation2.to = new Vector3(0f, 90f, 0f);
			tweenRotation2.delay = 0f;
			EventDelegate.Add(tweenRotation2.onFinished, delegate
			{
				_phaseWidget.SetActive(value: false);
			}, oneShot: true);
		}
	}

	private void ShowWaitWidget(bool show)
	{
		if (_isShowWaitWidget == show)
		{
			return;
		}
		_isShowWaitWidget = show;
		if (show)
		{
			_waitWidget.SetActive(value: true);
			TweenRotation tweenRotation = UITweener.Begin<TweenRotation>(_waitWidget, 0.3f);
			tweenRotation.from = new Vector3(0f, -90f, 0f);
			tweenRotation.to = Vector3.zero;
			tweenRotation.onFinished.Clear();
			tweenRotation.delay = 0.3f;
			tweenRotation.Sample(0f, isFinished: false);
		}
		else if (_waitWidget.activeSelf)
		{
			TweenRotation tweenRotation2 = UITweener.Begin<TweenRotation>(_waitWidget, 0.3f);
			tweenRotation2.from = _waitWidget.transform.eulerAngles;
			tweenRotation2.to = new Vector3(0f, 90f, 0f);
			tweenRotation2.delay = 0f;
			EventDelegate.Add(tweenRotation2.onFinished, delegate
			{
				_waitWidget.SetActive(value: false);
			}, oneShot: true);
		}
	}

	public void Tick()
	{
		if (_target == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.transform.localPosition = MainCamera.WorldToNGUIPos(_target.transform.position + _targetOffset, base.transform.parent);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		switch (_accelerator.Status)
		{
		case AcceleratorStatus.Waiting:
		case AcceleratorStatus.Intermission:
		{
			Yaml.WarpAccelerator warpAccelerator = Singleton<Constants>.Instance.WarpAccelerator;
			double num = _accelerator.StatusUntil.GetValueOrDefault() - predictedServerTime;
			float fillAmount = 0f;
			if (warpAccelerator.Breaktime > 0f)
			{
				fillAmount = Mathf.Clamp01((float)num / warpAccelerator.Breaktime);
			}
			_waitTimerGauge.fillAmount = fillAmount;
			break;
		}
		case AcceleratorStatus.Processing:
			UpdateProcessingPhase();
			break;
		}
	}

	private void UpdateProcessingPhase()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		Yaml.WarpAccelerator warpAccelerator = Singleton<Constants>.Instance.WarpAccelerator;
		double num = _accelerator.StatusUntil.GetValueOrDefault() - predictedServerTime;
		float num2 = 0f;
		if (warpAccelerator.Breaktime > 0f)
		{
			num2 = 1f - Mathf.Clamp01((float)num / warpAccelerator.PhaseTime);
		}
		_phaseTimerCursor.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(144f, -144f, num2));
		int num3 = 0;
		for (int num4 = _processingStateRatio.Length - 1; num4 >= 0; num4--)
		{
			if (num2 <= _processingStateRatio[num4])
			{
				num3 = num4;
			}
		}
		Color color = Color.white;
		Color color2 = Color.white;
		switch (num3)
		{
		case 0:
			color = new Color32(51, 184, 99, byte.MaxValue);
			color2 = new Color32(25, 81, 46, byte.MaxValue);
			break;
		case 1:
			color = new Color32(byte.MaxValue, 161, 3, byte.MaxValue);
			color2 = new Color32(111, 71, 5, byte.MaxValue);
			break;
		case 2:
			color = new Color32(242, 53, 4, byte.MaxValue);
			color2 = new Color32(108, 22, 3, byte.MaxValue);
			break;
		}
		for (int i = 0; i < 3; i++)
		{
			UISprite uISprite = _phaseStateSprites[i * 2];
			UISprite uISprite2 = _phaseStateSprites[i * 2 + 1];
			uISprite.color = color;
			uISprite2.color = color2;
			if (num3 < i)
			{
				uISprite.fillAmount = 0f;
				continue;
			}
			if (num3 > i)
			{
				uISprite.fillAmount = uISprite2.fillAmount;
				continue;
			}
			float num5 = ((i - 1 >= 0) ? _processingStateRatio[i - 1] : 0f);
			float num6 = _processingStateRatio[i];
			uISprite.fillAmount = uISprite2.fillAmount * ((num2 - num5) / (num6 - num5));
		}
	}
}
