using System;
using Durango.Logic;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public abstract class PlayerHudGroupBase : UIBase
{
	[SerializeField]
	protected HyperGaugeViewer _lifeViewer;

	[SerializeField]
	protected HyperGaugeViewer _energyViewer;

	[SerializeField]
	protected HudFatigueGauge _fatigueGauge;

	[SerializeField]
	private AirballoonHudControl _airBalloonHudControl;

	[SerializeField]
	private StatusEffectsControl _statusEffects;

	[SerializeField]
	private StatusEffectsControl _eventBuffEffects;

	[SerializeField]
	private SpecialDealIconWidget _specialDealIconWidget;

	[SerializeField]
	private InteractiveMessageHud _interactiveMessageHud;

	[SerializeField]
	[CanBeNull]
	private TopCenterNoticeHud _topCenterNoticeHud;

	[SerializeField]
	private RectLayoutComponent _hudLayout;

	[SerializeField]
	private PartyHudControl _partyHudControl;

	private SpecialDealCommoditiesPopup _specialDealPopup;

	private bool _showSpecialDealPopup;

	private bool _pauseSpecialDealPopup;

	private bool _layoutChanged;

	protected virtual void Start()
	{
		if (_topCenterNoticeHud != null)
		{
			GameSystem<PvpIslandSystem>.Instance().GameStarted += delegate
			{
				_topCenterNoticeHud.Show();
			};
			GameSystem<PvpIslandSystem>.Instance().BattleStarted += delegate
			{
				_topCenterNoticeHud.Hide();
			};
			_topCenterNoticeHud.PositionChanged += delegate
			{
				_layoutChanged = true;
			};
		}
		GameSystem<PartySystem>.Instance().MembersUpdated += RefreshInteractiveMessageHud;
		GameSystem<ShopSystem>.Instance().SpecialDealsUpdated += delegate
		{
			if (_specialDealIconWidget.Refresh())
			{
				_layoutChanged = true;
			}
		};
		GameSystem<ShopSystem>.Instance().NewSpecialDealCommoditiesReleased += ShowSpecialDealCommodities;
		// [แก้เอง] เดิมสมัครรับ event อย่างเดียว ไม่ได้อ่านค่าที่มีอยู่แล้วตอนเริ่ม
		//
		// server ส่ง Survival มาตั้งแต่ตอน login ซึ่ง**มาถึงก่อน HUD จะ Start()**
		// ⇒ event ยิงตอนยังไม่มีใครฟัง ⇒ หลอดค้างอยู่ที่ค่า default ของ prefab (ขึ้น 999/999)
		//   จนกว่าจะมีอะไรมาเปลี่ยนค่าให้ server ส่ง SurvivalUpdated ตามมา
		// แก้โดยดึงค่าปัจจุบันมาวาดทันทีหลังสมัคร
		Action<CharacterBehavior> applySurvival = delegate(CharacterBehavior character)
		{
			SetLife(character.GetGauge("life"));
			Gauge gauge = character.GetGauge("stamina");
			SetEnergy((gauge == null) ? character.GetGauge("energy") : gauge);
		};
		PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated += applySurvival;
		applySurvival(PlayerBehavior.LocalPlayer);
		_statusEffects.ListUpdated += delegate
		{
			_layoutChanged = true;
		};
		_eventBuffEffects.ListUpdated += delegate
		{
			_layoutChanged = true;
		};
		_partyHudControl.HudVisibilityChanged += delegate
		{
			_layoutChanged = true;
		};
		_specialDealIconWidget.IconClicked += ShowSpecialDealCommodities;
		_layoutChanged = true;
		_hudLayout.UpdateOnSizeChange();
		RefreshInteractiveMessageHud();
	}

	private void Update()
	{
		if (_layoutChanged)
		{
			_hudLayout.UpdateLayout();
			_layoutChanged = false;
		}
		UpdateSpecialDealPopup();
	}

	protected void ShowGaugeTooltip(GameObject gauge, float duration)
	{
		if (!(gauge == null))
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Sign = 1;
			if (duration > 0f)
			{
				string format = "#survival_tooltip_title_{0}";
				string format2 = "#survival_tooltip_{0}";
				string arg = ((gauge == _lifeViewer.gameObject) ? "life_health" : ((!(gauge == _energyViewer.gameObject)) ? "fatigue" : "stamina_energy"));
				format = LocalizeSystem.Get(string.Format(format, arg));
				format2 = LocalizeSystem.Get(string.Format(format2, arg));
				widgetTooltipControl.Set(format, format2);
				widgetTooltipControl.Show(gauge.GetComponent<UIWidget>(), Vector2.right * 10f, duration);
			}
			else
			{
				widgetTooltipControl.Hide();
			}
		}
	}

	private void ShowSpecialDealCommodities()
	{
		if (GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Shop))
		{
			_showSpecialDealPopup = true;
		}
	}

	private void RefreshInteractiveMessageHud()
	{
		if (GameSystem<PartySystem>.Instance().IsInvited)
		{
			PartySystem partySystem = GameSystem<PartySystem>.Instance();
			string mainText = T._("<em>{0}</em> 님의 파티 초대", partySystem.LeaderName);
			_interactiveMessageHud.Show("icon_mainhud_party", mainText, null, T._("거절"), partySystem.RejectPartyInvitation, 20, T._("수락"), partySystem.JoinIntoParty);
		}
		else
		{
			_interactiveMessageHud.Hide();
		}
	}

	private void ShowSpecialDealPopup()
	{
		_specialDealPopup = UIManager.Popup.Tooltip<SpecialDealCommoditiesPopup>();
		if (_specialDealPopup.Set())
		{
			_specialDealPopup.Show();
			_specialDealPopup.AddOnFinished(delegate
			{
				_specialDealPopup = null;
			});
		}
		else
		{
			_specialDealPopup = null;
		}
	}

	private void PauseSpecialDealPopup()
	{
		if (_specialDealPopup != null && _specialDealPopup.IsVisible)
		{
			_specialDealPopup.Hide();
			_showSpecialDealPopup = true;
		}
	}

	private void UpdateSpecialDealPopup()
	{
		if (!_pauseSpecialDealPopup && _showSpecialDealPopup)
		{
			ShowSpecialDealPopup();
			_showSpecialDealPopup = false;
		}
	}

	public void SetAirballoon(bool show, VehicleAirBalloon target)
	{
		_airBalloonHudControl.Set(show, target);
	}

	private void SetLife(Gauge gauge)
	{
		_lifeViewer.Set(gauge);
		_lifeViewer.ToIntFunction = Mathf.FloorToInt;
	}

	private void SetEnergy(Gauge gauge)
	{
		_energyViewer.Set(gauge);
		_energyViewer.ToIntFunction = Mathf.FloorToInt;
	}

	public void VibrateStaminaGauge()
	{
		_energyViewer.SetEnable<UIVibrator>(enable: true);
	}

	public void PauseSpecialDealPopup(bool pause)
	{
		if (_pauseSpecialDealPopup != pause)
		{
			if (pause)
			{
				PauseSpecialDealPopup();
			}
			_pauseSpecialDealPopup = pause;
		}
	}
}
