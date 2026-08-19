using System;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class DomesticRatioWidget : UIWidget
{
	[SerializeField]
	private UISprite _staticStateIcon;

	[SerializeField]
	private UIWidget _gaugeHolderWidget;

	[SerializeField]
	private UISprite _ratioSprite;

	[SerializeField]
	private UIWidget _estimationRatio;

	[SerializeField]
	private UILabel _timeLabel;

	[SerializeField]
	private TweenerPlayer _inProgressAnimation;

	[SerializeField]
	private TweenerPlayer _yammyAnimation;

	private bool _skipAnimation;

	private DomesticationInfo? _domestication;

	private Reins? _rein;

	private double? _modifiedDomesticationTime;

	private float? _dirtyAt;

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			_skipAnimation = false;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Application.isPlaying)
		{
			return;
		}
		if (_dirtyAt.HasValue && _dirtyAt.Value < Time.time)
		{
			Refresh();
			return;
		}
		DomesticationInfo? domestication = _domestication;
		if (domestication.HasValue && _domestication.Value.DomesticationInProgress)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			if (!(_domestication.Value.DomesticateUntil < predictedServerTime))
			{
				SetDomesticateTimer(_domestication.Value, _modifiedDomesticationTime);
			}
		}
	}

	public void Set(DomesticationInfo rein, double? modifiedDomesticationTime = null)
	{
		_domestication = rein;
		_modifiedDomesticationTime = modifiedDomesticationTime;
		if (modifiedDomesticationTime.HasValue && Math.Abs(rein.DomesticateUntil - modifiedDomesticationTime.Value) < 1.0)
		{
			_modifiedDomesticationTime = null;
		}
		_rein = null;
		Refresh();
	}

	public void Set(Reins rein)
	{
		_domestication = null;
		_modifiedDomesticationTime = null;
		_rein = rein;
		Refresh();
	}

	public void SetBlank()
	{
		_domestication = null;
		_modifiedDomesticationTime = null;
		_rein = null;
		Refresh();
	}

	private void Refresh()
	{
		_dirtyAt = null;
		if (_domestication.HasValue)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			if (_domestication.Value.DomesticateUntil < predictedServerTime)
			{
				_dirtyAt = null;
			}
			else
			{
				_dirtyAt = Time.time + (float)(_domestication.Value.DomesticateUntil - predictedServerTime);
			}
			CageStatus cageStatus = PetUtil.ConverInfoToStatus(_domestication.Value);
			if (_skipAnimation)
			{
				if (cageStatus == CageStatus.InProgress || cageStatus == CageStatus.Wild)
				{
					_inProgressAnimation.ResetToLast();
				}
				else
				{
					_inProgressAnimation.ResetToFirst();
				}
			}
			else
			{
				_inProgressAnimation.Play(cageStatus == CageStatus.InProgress || cageStatus == CageStatus.Wild, null);
			}
			SetProgressState(_domestication.Value);
			_skipAnimation = true;
		}
		else if (_rein.HasValue)
		{
			float ratio = ((!_rein.Value.Domesticated) ? 0f : 1f);
			CageStatus status = ((!_rein.Value.Domesticated) ? CageStatus.Wild : CageStatus.Complete);
			SetProgressState(ratio, status, TimedeltaFormatter.Format(_rein.Value.DomesticateDuration));
			_skipAnimation = false;
		}
		else
		{
			_staticStateIcon.spriteName = PetUtil.ConverStatusToSrpite(CageStatus.Wild);
			_staticStateIcon.color = PetUtil.ConverStatusToColor(CageStatus.Wild);
			SetWidgetActivation(CageStatus.None);
			_skipAnimation = false;
		}
	}

	private void SetProgressState(DomesticationInfo rein)
	{
		SetProgressState(PetUtil.ConvertInfoToRatio(rein), PetUtil.ConverInfoToStatus(rein), PetUtil.ConvertInfoToRemainingTime(rein));
	}

	private void SetProgressState(float ratio, CageStatus status, string timeText)
	{
		switch (status)
		{
		case CageStatus.Complete:
		case CageStatus.Domesticated:
			_staticStateIcon.spriteName = PetUtil.ConverStatusToSrpite(status);
			_staticStateIcon.color = PetUtil.ConverStatusToColor(status);
			break;
		default:
			_ratioSprite.width = (int)Mathf.Clamp((float)_gaugeHolderWidget.width * (1f - ratio), 0f, base.width);
			break;
		case CageStatus.Wild:
			break;
		}
		_timeLabel.text = timeText;
		SetWidgetActivation(status);
	}

	private void SetWidgetActivation(CageStatus status)
	{
		bool flag = status == CageStatus.Complete || status == CageStatus.Domesticated;
		_ratioSprite.gameObject.SetActive(status == CageStatus.InProgress);
		_staticStateIcon.gameObject.SetActive(flag);
		_gaugeHolderWidget.gameObject.SetActive(!flag);
		_timeLabel.gameObject.SetActive(!flag && status != CageStatus.None);
		_estimationRatio.gameObject.SetActive(value: false);
	}

	public void PlayYammyAnimation()
	{
		_yammyAnimation.Play();
	}

	private void SetDomesticateTimer(DomesticationInfo domestication, double? modifiedEndTime = null)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double seconds = domestication.DomesticateUntil - predictedServerTime;
		if (!modifiedEndTime.HasValue)
		{
			_timeLabel.text = TimedeltaFormatter.Format(seconds);
			float num = PetUtil.ConvertInfoToRatio(domestication);
			_ratioSprite.width = (int)Mathf.Clamp((float)_gaugeHolderWidget.width * (1f - num), 0f, base.width);
			_estimationRatio.gameObject.SetActive(value: false);
			return;
		}
		double num2 = Maths.Clamp(modifiedEndTime.Value - predictedServerTime, 0.0, domestication.TotalTime);
		_timeLabel.text = T._("{0} [preset=animation_arrow] <em>{1}</em>", TimedeltaFormatter.Format(seconds), TimedeltaFormatter.Format(num2));
		float num3 = PetUtil.ConvertInfoToRatio(domestication);
		_ratioSprite.width = (int)Mathf.Clamp((float)_gaugeHolderWidget.width * (1f - num3), 0f, base.width);
		_ratioSprite.gameObject.SetActive(value: true);
		float num4 = (float)(num2 / domestication.TotalTime);
		_estimationRatio.width = (int)Mathf.Clamp((float)_gaugeHolderWidget.width * (1f - num4), 0f, base.width);
		_estimationRatio.gameObject.SetActive(value: true);
		UIUtility.UpdateAnchors(_estimationRatio.transform);
	}
}
