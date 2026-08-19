using System;
using System.Collections.Generic;
using Durango.Logic.Explore;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Estate;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ClanBaseWidget : UIWidget
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _subtitleLabel;

	[SerializeField]
	private UILabel _cycleCountLabel;

	[SerializeField]
	private UISprite _stateSprite;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private UILabel _tunerHelpTooltip;

	[SerializeField]
	private UILabel _tunerStrengthHelpTooltip;

	[SerializeField]
	private UILabel _taxHelpTooltip;

	[SerializeField]
	private UILabel _taxRatioHelpTooltip;

	[SerializeField]
	private AdvancedResearchInfoWidget _advancedResearchInfoWidget;

	[SerializeField]
	private ClanBattleCycleWidget _battleCycleInfoWidget;

	[SerializeField]
	private UILabel _stateChangeAtLabel;

	[SerializeField]
	private UILabel _stateChangeAtTimeLabel;

	[SerializeField]
	private UISprite _tunerSpritebase;

	[SerializeField]
	private UILabel _tunerStrengthLabel;

	[SerializeField]
	private UILabel _taxLabel;

	[SerializeField]
	private UILabel _taxRateLabel;

	[SerializeField]
	private IntSelector _taxRateSelector;

	[SerializeField]
	private GameObject _warpCooltimeObject;

	[SerializeField]
	private UILabel _warpCooltimeLabel;

	[SerializeField]
	private SelectableButton _warpButton;

	private ListObjectPool<UISprite> _tuners = new ListObjectPool<UISprite>();

	private EstateLicenses _data;

	private float _nextRefreshTime;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tuners.BaseObject = _tunerSpritebase;
			_tuners.Init(delegate(UISprite sprite)
			{
				sprite.SetAnchor((Transform)null);
			});
			_warpButton.Clicked = OnClickWarp;
			_tunerHelpTooltip.text = string.Format("{0} [icon=icon_question_big]", T._("워프홀 튜너"));
			_tunerStrengthHelpTooltip.text = string.Format("{0} [icon=icon_question_big]", T._("워프홀 튜너의 강인도"));
			_taxHelpTooltip.text = string.Format("{0} [icon=icon_question_big]", T._("누적 이용료"));
			_taxRatioHelpTooltip.text = string.Format("{0} [icon=icon_question_big]", T._("이용 요금 조정"));
			UIEventListener.Get(_tunerHelpTooltip.gameObject).onClick = delegate
			{
				ClanBasePage.ShowHelpTitle(_tunerHelpTooltip.gameObject, T._("워프홀 튜너는 화물 워프홀이 전쟁 기간일 때 점령되는 것을 막습니다. 워프홀 튜너가 하나라도 존재하는 거점은 점령이 불가능합니다. 최대 {0}개까지만 건설할 수 있습니다.", Yaml.Util.Singleton<Constants>.Instance.War.MaxTunerCount));
			};
			UIEventListener.Get(_tunerStrengthHelpTooltip.gameObject).onClick = delegate
			{
				ClanBasePage.ShowHelpTitle(_tunerStrengthHelpTooltip.gameObject, T._("워프홀 튜너의 강인도가 높으면 공격으로부터 피해를 덜 입습니다. 강인도가 낮아질수록 더 많은 피해를 입게 됩니다."));
			};
			UIEventListener.Get(_taxHelpTooltip.gameObject).onClick = delegate
			{
				ClanBasePage.ShowHelpTitle(_taxHelpTooltip.gameObject, T._("화물 워프홀 이용자들로부터 징수한 이용요금입니다. 이 금액은 부족 자금으로 자동 이체됩니다."));
			};
			UIEventListener.Get(_taxRatioHelpTooltip.gameObject).onClick = delegate
			{
				ClanBasePage.ShowHelpTitle(_taxRatioHelpTooltip.gameObject, T._("화물 워프홀을 이용하는 이용자들에게 받을 이용요금 비율을 조정합니다."));
			};
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying && _isInit && _taxRateSelector.gameObject.activeSelf && _data.ClanCargoWarphole.HasValue)
		{
			int num = Mathf.RoundToInt(_data.ClanCargoWarphole.Value.RewardInfo.TaxRate * 100f);
			if (num != _taxRateSelector.Value)
			{
				EstateSystem.SetCargoWarpholeTaxRate((float)_taxRateSelector.Value / 100f);
			}
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying)
		{
			float time = Time.time;
			if (_nextRefreshTime > 0f && _nextRefreshTime < time)
			{
				_nextRefreshTime = 0f;
				Set(_data);
			}
		}
	}

	private void RefreshWarpButton(ClanCargoWarphole cargo)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (predictedServerTime < _data.ClanCargoWarpholeVisitAvailableAt)
		{
			ClanBattle? battleInfo = cargo.BattleInfo;
			if (battleInfo.HasValue && cargo.BattleInfo.Value.CycleProtectionUntil <= predictedServerTime)
			{
				_warpButton.SetStyle(PresetButton.Style.Border);
				_warpCooltimeObject.gameObject.SetActive(value: true);
				_warpCooltimeLabel.SetText(new SyncString(delegate(out string text, out float period)
				{
					double num = _data.ClanCargoWarpholeVisitAvailableAt - Connections.Frontend.GetPredictedServerTime();
					text = $"{TimedeltaFormatter.ColonFormat(num)} 후 무료";
					period = (float)num % 1f;
					Yaml.Cost clanWarpholeVisit = Yaml.Util.Singleton<CostsYaml>.Instance.ClanWarpholeVisit;
					clanWarpholeVisit.SetAmountParams(new KeyValuePair<string, object>("left_time", num));
					_warpButton.Text = T._("거점으로 워프 {0}", clanWarpholeVisit.CostToString(InventorySystem.Wallet));
				}));
				return;
			}
		}
		_warpButton.SetStyle(PresetButton.Style.Solid);
		_warpButton.Text = T._("거점으로 워프");
		_warpCooltimeObject.gameObject.SetActive(value: false);
	}

	private void RefreshTitle(ClanCargoWarphole cargo)
	{
		RegionTile? location = cargo.Location;
		if (location.HasValue)
		{
			_titleLabel.text = cargo.Location.Value.GetText();
			Durango.Logic.Explore.Region region = new Durango.Logic.Explore.Region(cargo.Location.Value.Region);
			_subtitleLabel.text = string.Format("{1} {0}", region.Role().GetName(), LocalizeUtil.FormatLevel(region.Level));
		}
	}

	private void RefreshBattleInfo(ClanCargoWarphole cargo)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		_battleCycleInfoWidget.Set(cargo);
		ClanBattle? battleInfo2 = cargo.BattleInfo;
		if (!battleInfo2.HasValue)
		{
			return;
		}
		ClanBattle battleInfo = cargo.BattleInfo.Value;
		_cycleCountLabel.text = T._("{0} 회차", $"[b][i]{battleInfo.Cycle}[/i][/b][size=20]");
		if (battleInfo.CycleProtectionUntil > predictedServerTime)
		{
			SetStateSprite("estate_peace", Color.white);
			_stateChangeAtLabel.text = T._("전쟁 시작까지 남은 시간");
			_stateChangeAtTimeLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				double num2 = battleInfo.CycleProtectionUntil - Connections.Frontend.GetPredictedServerTime();
				if (num2 > 0.0)
				{
					text = TimedeltaFormatter.Format(num2);
					period = (float)(num2 % (double)TimedeltaFormatter.CurrentMinUnit());
				}
				else
				{
					text = string.Empty;
					period = 0f;
				}
			}));
			return;
		}
		SetStateSprite("act_Attack", new Color32(186, 46, 46, byte.MaxValue));
		_stateChangeAtLabel.text = T._("전쟁 기간 종료까지 남은 시간");
		_stateChangeAtTimeLabel.SetText(new SyncString(delegate(out string text, out float period)
		{
			double num = battleInfo.CycleUntil - Connections.Frontend.GetPredictedServerTime();
			if (num > 0.0)
			{
				text = TimedeltaFormatter.Format(num);
				period = (float)(num % (double)TimedeltaFormatter.CurrentMinUnit());
			}
			else
			{
				text = string.Empty;
				period = 0f;
			}
		}));
	}

	private void RefreshTuner(ClanCargoWarphole cargo)
	{
		ClanBattle? battleInfo = cargo.BattleInfo;
		if (battleInfo.HasValue)
		{
			ClanBattle value = cargo.BattleInfo.Value;
			int count = Mathf.Max(Yaml.Util.Singleton<Constants>.Instance.War.MaxTunerCount, value.TunerCount);
			_tuners.Set(count);
			_tuners.Reposition(Vector3.left, 18);
			for (int i = 0; i < _tuners.Count; i++)
			{
				_tuners[i].color = ((i >= value.TunerCount) ? new Color(1f, 1f, 1f, 0.5f) : PresetColor.UIYellow);
			}
			_tunerStrengthLabel.text = $"{value.TunerStrength:P0}";
		}
	}

	private void RefreshTax(ClanCargoWarphole cargo)
	{
		_taxLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(cargo.RewardInfo.TotalTax, Currency.TStone);
		Member clan = PlayerBehavior.LocalPlayer.Clan;
		if (!string.IsNullOrEmpty(clan.ClanId) && clan.RoleId == 0)
		{
			_taxRateLabel.gameObject.SetActive(value: false);
			_taxRateSelector.gameObject.SetActive(value: true);
			_taxRateSelector.Set(Mathf.RoundToInt(cargo.RewardInfo.TaxRate * 100f), 0, 100);
		}
		else
		{
			_taxRateLabel.gameObject.SetActive(value: true);
			_taxRateSelector.gameObject.SetActive(value: false);
			_taxRateLabel.text = $"{cargo.RewardInfo.TaxRate:P0}";
		}
	}

	private void CalcNextUpdateAt(ClanCargoWarphole cargo, double visitAvailableAt)
	{
		double? num = null;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (visitAvailableAt > predictedServerTime)
		{
			num = visitAvailableAt;
		}
		ClanBattle? battleInfo = cargo.BattleInfo;
		if (battleInfo.HasValue)
		{
			ClanBattle value = cargo.BattleInfo.Value;
			if (value.CycleProtectionUntil > predictedServerTime)
			{
				num = ((!num.HasValue) ? value.CycleProtectionUntil : Math.Max(num.Value, value.CycleProtectionUntil));
			}
			else if (value.CycleUntil > predictedServerTime)
			{
				num = ((!num.HasValue) ? value.CycleUntil : Math.Max(num.Value, value.CycleUntil));
			}
		}
		_nextRefreshTime = ((!num.HasValue) ? 0f : Times.UnixTimeToUnityTime(num.Value));
	}

	public void Set(EstateLicenses data)
	{
		Init();
		_data = data;
		ClanCargoWarphole? clanCargoWarphole = _data.ClanCargoWarphole;
		if (clanCargoWarphole.HasValue && _data.ClanCargoWarphole.Value.Location.HasValue)
		{
			ClanCargoWarphole value = _data.ClanCargoWarphole.Value;
			RefreshTitle(value);
			RefreshWarpButton(value);
			_advancedResearchInfoWidget.Refresh();
			RefreshBattleInfo(value);
			RefreshTuner(value);
			RefreshTax(value);
			CalcNextUpdateAt(value, _data.ClanCargoWarpholeVisitAvailableAt);
			_scrollView.Reposition();
		}
	}

	private void SetStateSprite(string sprite, Color col)
	{
		_stateSprite.spriteName = sprite;
		_stateSprite.color = col;
	}

	private void OnClickWarp()
	{
		ClanCargoWarphole? clanCargoWarphole = _data.ClanCargoWarphole;
		if (!clanCargoWarphole.HasValue)
		{
			return;
		}
		ClanBattle? battleInfo = _data.ClanCargoWarphole.Value.BattleInfo;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (predictedServerTime < _data.ClanCargoWarpholeVisitAvailableAt && battleInfo.HasValue && battleInfo.Value.CycleProtectionUntil <= predictedServerTime)
		{
			double num = _data.ClanCargoWarpholeVisitAvailableAt - predictedServerTime;
			Yaml.Cost cost = Yaml.Util.Singleton<CostsYaml>.Instance.ClanWarpholeVisit;
			cost.SetAmountParams(new KeyValuePair<string, object>("left_time", num));
			UIManager.MessageBox.ShowCostConfirm(cost, T._("재사용 대기시간 중 거점으로 귀환하려면 워프젬이 소모됩니다.\n{0:을} 사용하여 거점으로 귀환하시겠습니까?", cost.CostToString(InventorySystem.Wallet)), null, delegate(bool ok)
			{
				if (ok)
				{
					double num2 = _data.ClanCargoWarpholeVisitAvailableAt - Connections.Frontend.GetPredictedServerTime();
					UIBase.CloseAllUI();
					cost.SetAmountParams(new KeyValuePair<string, object>("left_time", num2));
					EstateSystem.VisitEstate(OwnerType.ClanWarphole, PlayerBehavior.LocalPlayer.ClanId, new Money(cost.GetAmount(), cost.Currency));
				}
			});
		}
		else
		{
			UIBase.CloseAllUI();
			EstateSystem.VisitEstate(OwnerType.ClanWarphole, PlayerBehavior.LocalPlayer.ClanId);
		}
	}
}
