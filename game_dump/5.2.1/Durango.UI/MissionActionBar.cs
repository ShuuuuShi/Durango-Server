using System;
using Durango.Logic.Faction;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Faction;
using UnityEngine;

namespace Durango.UI;

public class MissionActionBar : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private SelectableButton _baseButton;

	[SerializeField]
	private UIWidget _timerWidget;

	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private UILabel _infoLabel;

	private readonly ListObjectPool<SelectableButton> _buttons = new ListObjectPool<SelectableButton>();

	private Durango.Logic.Faction.Faction _faction;

	[CanBeNull]
	private SelectableButton _refreshMissionButton;

	private ShuffleCondition _shuffleCondition;

	public event Action<Mission> MissionStartClicked;

	public event Action<Mission> MissionRefreshClicked;

	public event Action<Mission> MissionCancelClicked;

	public event Action<Mission> MissionDetailClicked;

	public event Action<FactionType> MissionResetCooltimeClicked;

	void IUIInitializable.Init()
	{
		_buttons.BaseObject = _baseButton;
		_buttons.UseBase = true;
		_buttons.Clear();
		UIEventListener.Get(_infoLabel.gameObject).onClick = delegate
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, T._("매일 처음 완료하는 임무는 평소보다 더 많은 보상을 받습니다!"), 360);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show(_infoLabel, Vector2.zero, 60f);
		};
	}

	public void SetFaction(Durango.Logic.Faction.Faction faction)
	{
		_faction = faction;
		_refreshMissionButton = null;
		if (_faction == null || !_faction.Mission.HasValue)
		{
			if (_faction != null && _faction.MissionAvailableAt > 0.0)
			{
				_buttons.Set(1);
				SelectableButton selectableButton = _buttons[0];
				selectableButton.SetStyle(PresetButton.Style.Border);
				selectableButton.Text = T._("임무 바로 받기");
				selectableButton.Clicked = OnResetMissionCooltime;
			}
			else
			{
				_buttons.Clear();
			}
		}
		else if (_faction.Mission.Value.StartedAt.HasValue)
		{
			_buttons.Set(2);
			SelectableButton selectableButton2 = _buttons[0];
			selectableButton2.SetStyle(PresetButton.Style.Border);
			selectableButton2.Text = T._("자세히 보기");
			selectableButton2.Clicked = OnMissionDetail;
			SelectableButton selectableButton3 = _buttons[1];
			selectableButton3.SetStyle(PresetButton.Style.Border);
			selectableButton3.Text = T._("임무 중단");
			selectableButton3.Clicked = OnCancelMission;
		}
		else
		{
			_buttons.Set(2);
			SelectableButton selectableButton4 = _buttons[0];
			selectableButton4.SetStyle(PresetButton.Style.Solid);
			selectableButton4.Text = T._("임무 시작");
			selectableButton4.Clicked = OnMissionStart;
			SelectableButton selectableButton5 = _buttons[1];
			selectableButton5.SetStyle(PresetButton.Style.Border);
			selectableButton5.Clicked = OnRefreshMission;
			_refreshMissionButton = selectableButton5;
		}
		_buttons.Reposition(Vector3.left, 10);
		UpdateRefreshMissionButton();
	}

	public SelectableButton GetStartButton()
	{
		if (_buttons.Count == 0)
		{
			return null;
		}
		if (_faction == null || !_faction.Mission.HasValue)
		{
			return null;
		}
		if (_faction.Mission.Value.StartedAt.HasValue)
		{
			return null;
		}
		return _buttons[0];
	}

	public void SetShuffleCondition(ShuffleCondition condition)
	{
		_shuffleCondition = condition;
		UpdateRefreshMissionButton();
	}

	private void UpdateRefreshMissionButton()
	{
		if (_refreshMissionButton != null && _shuffleCondition != null)
		{
			_refreshMissionButton.Text = T._("다른 임무 받기({0}/{1})", _shuffleCondition.RemainCount, _shuffleCondition.MaxCount);
			if (_shuffleCondition.RechargeEndsAt > 0.0)
			{
				_timerWidget.gameObject.SetActive(value: true);
				Vector3 pos = _refreshMissionButton.Widget.GetPosition(0.75f, 1f) + Vector3.down * 15f;
				_timerWidget.SetPosition(pos, 0.5f, 0f);
				_timerLabel.SetText(new SyncString(delegate(out string text, out float period)
				{
					SyncString.UpdateRemainTimeColonMsg(_shuffleCondition.RechargeEndsAt, out text, out period, string.Empty);
					_timerWidget.gameObject.SetActive(_refreshMissionButton != null && period > 0f);
				}));
				return;
			}
		}
		_timerWidget.gameObject.SetActive(value: false);
	}

	public void SetDailyMissionAvailableAt(double availableAt)
	{
		_infoLabel.SetText(new SyncString(delegate(out string text, out float period)
		{
			SyncString.UpdateRemainTimeMsg(availableAt, T._("<link>오늘의 첫 임무</link> 보상까지 <em>{0}</em> 남음"), out text, out period, string.Format("[preset=particle_warpgem:1.5]  {0}", T._("<link>오늘의 첫 임무</link> 보상을 받을 수 있습니다.")));
		}));
	}

	private void OnMissionStart()
	{
		if (_faction != null && _faction.Mission.HasValue && this.MissionStartClicked != null)
		{
			this.MissionStartClicked(_faction.Mission.Value);
		}
	}

	private void OnRefreshMission()
	{
		if (_faction != null && _faction.Mission.HasValue && this.MissionRefreshClicked != null)
		{
			this.MissionRefreshClicked(_faction.Mission.Value);
		}
	}

	private void OnCancelMission()
	{
		if (_faction != null && _faction.Mission.HasValue && this.MissionCancelClicked != null)
		{
			this.MissionCancelClicked(_faction.Mission.Value);
		}
	}

	private void OnResetMissionCooltime()
	{
		if (_faction != null && this.MissionResetCooltimeClicked != null)
		{
			this.MissionResetCooltimeClicked(_faction.Type);
		}
	}

	private void OnMissionDetail()
	{
		if (_faction != null && _faction.Mission.HasValue && this.MissionDetailClicked != null)
		{
			this.MissionDetailClicked(_faction.Mission.Value);
		}
	}
}
