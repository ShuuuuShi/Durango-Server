using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Accelerator;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class WarpAcceleratorInfoWidget : MonoBehaviour
{
	private struct Message
	{
		public float At;

		public string Text;

		public string Sound;
	}

	[Serializable]
	public class WarpAcceleratorWaveView
	{
		[SerializeField]
		private ListObjectPool _bg1;

		[SerializeField]
		private ListObjectPool _bg2;

		[SerializeField]
		private ListObjectPool _upper1;

		[SerializeField]
		private ListObjectPool _upper2;

		private int? _term;

		private int Term
		{
			get
			{
				int? term = _term;
				if (!term.HasValue)
				{
					_term = _bg2.BaseObject.GetComponent<UIWidget>().width;
				}
				return _term.Value;
			}
		}

		public void Clear()
		{
			_bg1.BaseObject.transform.parent.gameObject.SetActive(value: false);
		}

		public void Set(int current, int max)
		{
			if (max <= 0)
			{
				Clear();
				return;
			}
			_bg1.BaseObject.transform.parent.gameObject.SetActive(value: true);
			int term = Term;
			_bg1.BeginLoad();
			_bg2.BeginLoad();
			_upper1.BeginLoad();
			_upper2.BeginLoad();
			Vector3 vector = Vector3.left * (max - 1) * term * 0.5f;
			for (int i = 0; i < max; i++)
			{
				int num = i + 1;
				_bg1.GetNext().transform.localPosition = vector;
				if (i > 0)
				{
					_bg2.GetNext().transform.localPosition = vector + Vector3.left * term * 0.5f;
				}
				if (num <= current)
				{
					_upper1.GetNext().transform.localPosition = vector;
					if (i > 0)
					{
						_upper2.GetNext().transform.localPosition = vector + Vector3.left * term * 0.5f;
					}
				}
				vector.x += term;
			}
			_bg1.EndLoad();
			_bg2.EndLoad();
			_upper1.EndLoad();
			_upper2.EndLoad();
		}
	}

	private const string MsgKey = "WarpAccelerator";

	[SerializeField]
	private WarpAcceleratorOverWidget _overInfoWidget;

	[SerializeField]
	private GameObject _phaseWidget;

	[SerializeField]
	private TweenerPlayer _timeAlertObjects;

	[SerializeField]
	private UISprite _phaseStateSprite;

	[SerializeField]
	private ListObjectPool _phaseStateSeparators;

	[SerializeField]
	private UILabel _phaseTimerLabel;

	[SerializeField]
	private UILabel _phaseLabel;

	[SerializeField]
	private Transform _phaseTimerCursor;

	[SerializeField]
	private UILabel _remainAnimalLabel;

	[SerializeField]
	private WarpAcceleratorWaveView _waveView;

	[SerializeField]
	private UISprite _minimapBorder;

	[SerializeField]
	private GameObject _waitWidget;

	[SerializeField]
	private UILabel _waitTimerLabel;

	[SerializeField]
	private UISprite _waitTimerGauge;

	private readonly List<Message> _messages = new List<Message>();

	private Messages.WarpAccelerator? _prev;

	private Messages.WarpAccelerator? _accelerator;

	private bool? _isShowPhaseWidget;

	private bool? _isShowWaitWidget;

	private ListObjectPool<UISprite> _phaseStateSprites;

	private float[] _processingStateRatio;

	private ListObjectPool<WarpAcceleratorOverWidget> _overInfoWidgets;

	private void Awake()
	{
		Durango.Utils.Singleton<AnimalManager>.Instance().AnimalAppeared += OnAppearAnimal;
		InitializeStateSprite();
		_overInfoWidgets = new ListObjectPool<WarpAcceleratorOverWidget>();
		_overInfoWidgets.BaseObject = _overInfoWidget;
		_overInfoWidgets.Clear();
		GameSystem<WarpAcceleratorSystem>.Instance().WarpAcceleratorsUpdated += OnUpdateWarpAccelerators;
		OnUpdateWarpAccelerators();
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
				float phaseTime = Yaml.Util.Singleton<Constants>.Instance.WarpAccelerator.PhaseTime;
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

	private void LateUpdate()
	{
		for (int i = 0; i < _overInfoWidgets.Count; i++)
		{
			_overInfoWidgets[i].Tick();
		}
	}

	private void Update()
	{
		ProcessMessageQueue();
		Messages.WarpAccelerator? accelerator = _accelerator;
		if (!accelerator.HasValue)
		{
			return;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		Messages.WarpAccelerator value = _accelerator.Value;
		switch (value.Status)
		{
		case AcceleratorStatus.Waiting:
		case AcceleratorStatus.Intermission:
		{
			Yaml.WarpAccelerator warpAccelerator = Yaml.Util.Singleton<Constants>.Instance.WarpAccelerator;
			double num = value.StatusUntil.GetValueOrDefault() - predictedServerTime;
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
		Messages.WarpAccelerator? accelerator = _accelerator;
		if (!accelerator.HasValue)
		{
			return;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		Messages.WarpAccelerator value = _accelerator.Value;
		Yaml.WarpAccelerator warpAccelerator = Yaml.Util.Singleton<Constants>.Instance.WarpAccelerator;
		double num = value.StatusUntil.GetValueOrDefault() - predictedServerTime;
		float num2 = 0f;
		float num3 = 0f;
		if (warpAccelerator.Breaktime > 0f)
		{
			num2 = 1f - Mathf.Clamp01((float)num / warpAccelerator.PhaseTime);
			num3 = 1f - Mathf.Clamp01((float)(num + (double)Time.deltaTime) / warpAccelerator.PhaseTime);
		}
		_phaseTimerCursor.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(144f, -144f, num2));
		int num4 = 0;
		int num5 = 0;
		for (int num6 = _processingStateRatio.Length - 1; num6 >= 0; num6--)
		{
			if (num2 <= _processingStateRatio[num6])
			{
				num4 = num6;
			}
			if (num3 <= _processingStateRatio[num6])
			{
				num5 = num6;
			}
		}
		Color color = Color.white;
		Color color2 = Color.white;
		switch (num4)
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
			if (num4 < i)
			{
				uISprite.fillAmount = 0f;
				continue;
			}
			if (num4 > i)
			{
				uISprite.fillAmount = uISprite2.fillAmount;
				continue;
			}
			float num7 = ((i - 1 >= 0) ? _processingStateRatio[i - 1] : 0f);
			float num8 = _processingStateRatio[i];
			uISprite.fillAmount = uISprite2.fillAmount * ((num2 - num7) / (num8 - num7));
		}
		if (num5 != num4)
		{
			if (num4 < 2)
			{
				_timeAlertObjects.gameObject.SetActive(value: false);
				_timeAlertObjects.ResetToFirst();
				_phaseTimerLabel.color = Color.white;
			}
			else
			{
				_timeAlertObjects.gameObject.SetActive(value: true);
				_timeAlertObjects.Play();
				_phaseTimerLabel.color = PresetColor.UILightRed;
			}
		}
	}

	private void ProcessMessageQueue()
	{
		float time = Time.time;
		for (int num = _messages.Count - 1; num >= 0; num--)
		{
			Message message = _messages[num];
			if (!(time < message.At))
			{
				_messages.RemoveAt(num);
				if (!string.IsNullOrEmpty(message.Text))
				{
					UIManager.SystemMsg("WarpAccelerator", message.Text);
				}
				if (!string.IsNullOrEmpty(message.Sound))
				{
					SoundManager.PlayEvent(message.Sound);
				}
			}
		}
	}

	private void OnUpdateWarpAccelerators()
	{
		RefreshOverInfoWidgets();
		WarpAcceleratorInfo? myWarpAcceleratorInfo = GameSystem<WarpAcceleratorSystem>.Instance().GetMyWarpAcceleratorInfo();
		if (!myWarpAcceleratorInfo.HasValue)
		{
			Set(null);
		}
		else
		{
			Set(myWarpAcceleratorInfo.Value.Warpaccelerator);
		}
	}

	private void RefreshOverInfoWidgets()
	{
		_overInfoWidgets.BeginLoad();
		foreach (WarpAcceleratorInfo warpAccelerator in GameSystem<WarpAcceleratorSystem>.Instance().WarpAccelerators)
		{
			bool flag = false;
			AcceleratorStatus status = warpAccelerator.Warpaccelerator.Status;
			if (status == AcceleratorStatus.Waiting || status == AcceleratorStatus.Processing || status == AcceleratorStatus.Intermission)
			{
				flag = true;
			}
			if (flag)
			{
				Artifact artifact = Durango.Utils.Singleton<ArtifactManager>.Instance().Find(warpAccelerator.EntityId);
				if (!(artifact == null))
				{
					_overInfoWidgets.GetNext().Set(artifact, warpAccelerator.Warpaccelerator);
				}
			}
		}
		_overInfoWidgets.EndLoad();
	}

	private void Set(Messages.WarpAccelerator? accelerator)
	{
		_prev = _accelerator;
		_accelerator = accelerator;
		Refresh();
		Update();
	}

	private void Refresh()
	{
		_messages.Clear();
		Messages.WarpAccelerator? accelerator = _accelerator;
		if (!accelerator.HasValue)
		{
			_minimapBorder.gameObject.SetActive(value: false);
			Clear();
			return;
		}
		Messages.WarpAccelerator? prev = _prev;
		AcceleratorStatus acceleratorStatus = (prev.HasValue ? _prev.Value.Status : AcceleratorStatus.Invalid);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		Messages.WarpAccelerator value = _accelerator.Value;
		AcceleratorStatus status = value.Status;
		if ((uint)(status - 1) <= 2u)
		{
			_minimapBorder.gameObject.SetActive(value: true);
		}
		else
		{
			_minimapBorder.gameObject.SetActive(value: false);
		}
		switch (value.Status)
		{
		case AcceleratorStatus.Waiting:
			if (acceleratorStatus != AcceleratorStatus.Waiting)
			{
				_messages.Add(new Message
				{
					At = Time.time,
					Text = T._("몰려드는 동물들을 처치하고 워프 가속 활동을 진행하세요.")
				});
			}
			break;
		case AcceleratorStatus.Intermission:
		{
			if (acceleratorStatus == AcceleratorStatus.Processing)
			{
				UIManager.Alarm.WarpAcceleratorEffects.Play(WarpAcceleratorEffects.Type.End, value);
			}
			float num5 = (float)(value.StatusSince.GetValueOrDefault() + 5.0 - predictedServerTime);
			if (num5 > 0f)
			{
				_messages.Add(new Message
				{
					At = Time.time + num5,
					Text = T._("잠시 후 워프 가속기가 작동합니다.")
				});
			}
			num5 = (float)(value.StatusUntil.GetValueOrDefault() - 5.0 - predictedServerTime);
			if (num5 > 0f)
			{
				_messages.Add(new Message
				{
					At = Time.time + num5,
					Sound = "ui_warpaccelerator_countdown"
				});
			}
			ShowPhaseWidget(show: false);
			ShowWaitWidget(show: true);
			_waitTimerLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				SyncString.UpdateRemainTimeColonMsg(value.StatusUntil.GetValueOrDefault(), out text, out period, string.Empty);
			}));
			SetRemainAnimalCount(null);
			_waveView.Clear();
			return;
		}
		case AcceleratorStatus.Processing:
		{
			if (acceleratorStatus != AcceleratorStatus.Processing)
			{
				UIManager.Alarm.WarpAcceleratorEffects.Play(WarpAcceleratorEffects.Type.Start, value);
			}
			double valueOrDefault2 = value.StatusUntil.GetValueOrDefault();
			float phaseTime = Yaml.Util.Singleton<Constants>.Instance.WarpAccelerator.PhaseTime;
			float num3 = phaseTime * (1f - _processingStateRatio[0]);
			float num4 = (float)(valueOrDefault2 - (double)num3 - predictedServerTime);
			if (num3 > 0f && num4 > 0f)
			{
				_messages.Add(new Message
				{
					At = Time.time + num4,
					Text = T._("{0} 안에 동물들을 모두 처치하세요.", TimedeltaFormatter.Format(num3))
				});
			}
			num3 = phaseTime * (1f - _processingStateRatio[1]);
			num4 = (float)(valueOrDefault2 - (double)num3 - predictedServerTime);
			if (num3 > 0f && num4 > 0f)
			{
				_messages.Add(new Message
				{
					At = Time.time + num4,
					Text = T._("{0} 안에 동물들을 모두 처치하세요.", TimedeltaFormatter.Format(num3))
				});
			}
			ShowPhaseWidget(show: true);
			ShowWaitWidget(show: false);
			if (value.StatusUntil.HasValue)
			{
				num3 = (float)(value.StatusUntil.Value - predictedServerTime);
				if ((double)num3 > 10.0)
				{
					_timeAlertObjects.gameObject.SetActive(value: false);
					_timeAlertObjects.ResetToFirst();
					_phaseTimerLabel.color = Color.white;
				}
				else
				{
					_timeAlertObjects.gameObject.SetActive(value: true);
					_timeAlertObjects.Play();
					_phaseTimerLabel.color = PresetColor.UIRed;
				}
			}
			_phaseLabel.text = value.CurrentPhase.ToString();
			_phaseTimerLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				SyncString.UpdateRemainTimeColonMsg(value.StatusUntil.GetValueOrDefault(), out text, out period, string.Empty);
			}));
			SetRemainAnimalCount(value.RemainAnimals);
			_waveView.Set(value.CurrentWave.GetValueOrDefault(), value.CurrentMaxWave.GetValueOrDefault());
			return;
		}
		case AcceleratorStatus.End:
		{
			if (acceleratorStatus != AcceleratorStatus.End)
			{
				if (value.CurrentPhase > Yaml.Util.Singleton<Constants>.Instance.WarpAccelerator.MaxPhase)
				{
					UIManager.Alarm.WarpAcceleratorEffects.Play(WarpAcceleratorEffects.Type.FinishedWithShine, value);
				}
				else if (value.CurrentPhase > 1)
				{
					UIManager.Alarm.WarpAcceleratorEffects.Play(WarpAcceleratorEffects.Type.Finished, value);
				}
				else
				{
					UIManager.Alarm.WarpAcceleratorEffects.Play(WarpAcceleratorEffects.Type.Fail, value);
				}
			}
			double valueOrDefault = value.StatusUntil.GetValueOrDefault();
			if (!(valueOrDefault < predictedServerTime))
			{
				float num = 30f;
				float num2 = (float)(valueOrDefault - (double)num - predictedServerTime);
				if (num2 > 0f)
				{
					_messages.Add(new Message
					{
						At = Time.time + num2,
						Text = T._("{0} 뒤 워프 가속기가 사라집니다. 내용물을 확인하세요.", TimedeltaFormatter.Format(num))
					});
				}
				num = 10f;
				num2 = (float)(valueOrDefault - 10.0 - predictedServerTime);
				if (num2 > 0f)
				{
					_messages.Add(new Message
					{
						At = Time.time + num2,
						Text = T._("{0} 뒤 워프 가속기가 사라집니다. 내용물을 확인하세요.", TimedeltaFormatter.Format(num))
					});
				}
			}
			break;
		}
		}
		Clear();
	}

	private void OnAppearAnimal(AnimalBehavior animal)
	{
		Messages.WarpAccelerator? accelerator = _accelerator;
		if (accelerator.HasValue && !(animal.Role != "warp_guard"))
		{
			string portrait = AnimalYaml.GetPortrait(animal.EntityTypeId);
			UIManager.FindScript<NavigateGroup>().Point.SetTarget("WarpGuard_" + animal.EntityId, new PointTargetController.Arguments
			{
				Target = animal.transform,
				Icon = portrait,
				BorderColor = new Color32(171, 125, byte.MaxValue, byte.MaxValue),
				HideInScreen = true,
				ShowBg = true
			});
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

	private void SetRemainAnimalCount(int? count)
	{
		if (count.HasValue)
		{
			_remainAnimalLabel.text = count.Value.ToString();
			_remainAnimalLabel.transform.parent.gameObject.SetActive(value: true);
		}
		else
		{
			_remainAnimalLabel.transform.parent.gameObject.SetActive(value: false);
		}
	}

	private void Clear()
	{
		_isShowPhaseWidget = false;
		_isShowWaitWidget = false;
		_waveView.Clear();
		SetRemainAnimalCount(null);
		_phaseWidget.SetActive(value: false);
		_waitWidget.SetActive(value: false);
	}
}
