using System;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class MissionInfoPopup : TooltipBase
{
	public struct Data
	{
		public string Id;

		public string ClientName;

		public string Subject;

		public string Description;

		public double? StartedAt;

		public int? TimeLimit;

		public RewardInfo? Reward;

		public Data(Mission mission)
		{
			Id = mission.Id;
			Subject = mission.Subject;
			Description = mission.Description;
			StartedAt = mission.StartedAt;
			TimeLimit = mission.TimeLimit;
			Reward = mission.Reward;
			Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(mission.Faction);
			ClientName = ((faction != null) ? $"[icon={IconMap.Get(mission.Faction)}:1.3]  {faction.Name}" : string.Empty);
		}
	}

	private const string BlurKey = "MissionInfoPopup";

	private const int SeperatorWidth = 850;

	private const int Gap = 100;

	private const int DescriptionWidth = 750;

	[SerializeField]
	private Transform _container;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _missionLabel;

	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UILabel _rewardLabel;

	[SerializeField]
	private UISprite[] _separators;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private UIWidget _cancelButton;

	private Data _mission;

	private float _nextTimerUpdateAt;

	private bool _isInit;

	public override bool DragLock => true;

	public SelectableButton ConfirmButton => _confirmButton;

	public event Action Closed;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIEventListener uIEventListener = UIEventListener.Get(_cancelButton.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
			{
				Hide();
			});
		}
	}

	protected override void Update()
	{
		base.Update();
		if (_nextTimerUpdateAt < Time.time)
		{
			UpdateTimerLabel();
		}
	}

	private void OnConfirm()
	{
		MissionGroup missionGroup = UIManager.FindScript<MissionGroup>();
		if (missionGroup != null)
		{
			FactionSystem.AcceptMission(missionGroup.EntityId, missionGroup.Tile, _mission.Id);
		}
		Hide();
	}

	private void OnCancel()
	{
		UIManager.MessageBox.Show(T._("진행 중인 임무를 중단합니다."), delegate(bool ok)
		{
			if (!ok)
			{
				Show();
			}
			else
			{
				FactionSystem.CancelMission(_mission.Id);
			}
		});
		Hide();
	}

	protected override void UpdateLayout()
	{
		UpdateSize();
		UpdatePositions();
	}

	private void UpdatePositions()
	{
		Rect safeArea = UIManager.SafeArea;
		int num = (int)((1f - safeArea.xMax) * (float)UIManager.ScreenWidth);
		int num2 = (int)((1f - safeArea.yMax) * (float)UIManager.ScreenHeight);
		_cancelButton.leftAnchor.SetScreen(1f, -100f - (float)num);
		_cancelButton.rightAnchor.SetScreen(1f, -num);
		_cancelButton.topAnchor.SetScreen(1f, -num2);
		_cancelButton.bottomAnchor.SetScreen(1f, -100 - num2);
		_cancelButton.UpdateAnchors();
		Vector3 position = _titleLabel.GetPosition(0.5f, 0f);
		position.y -= 22f;
		_missionLabel.SetPosition(position, 0.5f, 1f);
		position.y -= _missionLabel.height;
		if (_timerLabel.gameObject.activeSelf)
		{
			position.y -= 22f;
			_timerLabel.SetPosition(position, 0.5f, 1f);
			position.y -= _titleLabel.height;
		}
		if (_descriptionLabel.gameObject.activeSelf)
		{
			position.y -= 24f;
			_separators[0].SetPosition(position, 0.5f, 1f);
			position.y -= (float)_separators[0].height + 40f;
			_descriptionLabel.SetPosition(position, 0.5f, 1f);
			position.y -= (float)_descriptionLabel.height + 40f;
			_separators[1].SetPosition(position, 0.5f, 1f);
			position.y -= _separators[1].height;
		}
		position.y -= 34f;
		_rewardLabel.SetPosition(position, 0.5f, 1f);
		position.y -= _rewardLabel.height;
		if (_confirmButton.gameObject.activeSelf)
		{
			position.y -= 50f;
			Vector3 localPosition = _confirmButton.transform.localPosition;
			localPosition.y = position.y;
			_confirmButton.transform.localPosition = localPosition;
		}
		else
		{
			position.y += 10f;
		}
		_container.transform.localPosition = Vector3.up * (0f - position.y) * 0.5f;
	}

	private void UpdateSize()
	{
		int num = Screen.width - 50;
		int width = Mathf.Min(num, 850);
		int width2 = Mathf.Min(num - 100, 750);
		UISprite[] separators = _separators;
		foreach (UISprite uISprite in separators)
		{
			uISprite.width = width;
		}
		_descriptionLabel.width = width2;
	}

	public void Show(Data mission, bool isAcceptable, bool isCancel = false)
	{
		Init();
		_mission = mission;
		_titleLabel.text = mission.ClientName;
		_missionLabel.text = mission.Subject;
		if (string.IsNullOrEmpty(mission.Description))
		{
			_descriptionLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_descriptionLabel.gameObject.SetActive(value: true);
			_descriptionLabel.text = mission.Description;
		}
		_rewardLabel.text = T._("임무 성공 보상 ") + FactionSystem.MissionRewardToString(mission.Reward);
		if (isAcceptable)
		{
			if (isCancel)
			{
				_confirmButton.SetStyle(PresetButton.Style.Border);
				_confirmButton.Text = T._("임무 중단");
				_confirmButton.Clicked = OnCancel;
			}
			else
			{
				_confirmButton.SetStyle(PresetButton.Style.Solid);
				_confirmButton.Text = T._("임무 시작");
				_confirmButton.Clicked = OnConfirm;
			}
		}
		_confirmButton.gameObject.SetActive(isAcceptable);
		UpdateTimerLabel();
		UpdateLayout();
		Show();
	}

	private void UpdateTimerLabel()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (_mission.TimeLimit.HasValue)
		{
			_timerLabel.gameObject.SetActive(value: true);
			if (_mission.StartedAt.HasValue)
			{
				double num = _mission.StartedAt.Value + (double)_mission.TimeLimit.Value;
				double seconds = num - predictedServerTime;
				_timerLabel.text = $"[icon=icon_timer2:1.1] {TimedeltaFormatter.Format(seconds)}";
				_nextTimerUpdateAt = Time.time + 1f;
			}
			else
			{
				double seconds2 = _mission.TimeLimit.Value;
				_timerLabel.text = $"[icon=icon_timer2:1.1] {TimedeltaFormatter.Format(seconds2)}";
				_nextTimerUpdateAt = float.MaxValue;
			}
		}
		else
		{
			_timerLabel.gameObject.SetActive(value: false);
			_nextTimerUpdateAt = float.MaxValue;
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		BlurController.BlurOn("MissionInfoPopup", BlurController.Mask.UI);
	}

	protected override void OnHide()
	{
		base.OnHide();
		BlurController.BlurOff("MissionInfoPopup");
		if (this.Closed != null)
		{
			this.Closed();
		}
	}

	protected override void OnTryConfirmOnModal()
	{
		if (_confirmButton.gameObject.activeInHierarchy && _confirmButton.Clicked != null)
		{
			_confirmButton.Clicked();
		}
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}
}
