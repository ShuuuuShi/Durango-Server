using System.Text;
using ItemSystem;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Faction;
using TimerData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class FactionSummary : MonoBehaviour
{
	[SerializeField]
	private GameObject _knownContainer;

	[SerializeField]
	private GameObject _unknownContainer;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _textFactionName;

	[SerializeField]
	private UILabel _textTitle;

	[SerializeField]
	private UILabel _textGaugeName;

	[SerializeField]
	private UILabel _textGaugeValues;

	[SerializeField]
	private UILabel _textRemainTime;

	[SerializeField]
	private UISprite _iconRemainTime;

	[SerializeField]
	private UISpriteLabel _textRewards;

	[SerializeField]
	private UISprite _spriteGaugeBorder;

	[SerializeField]
	private UISprite _spriteGaugeBar;

	[SerializeField]
	private Color _colorNormal;

	[SerializeField]
	private Color _colorPressed;

	[SerializeField]
	private string _formatGaugeValues;

	[SerializeField]
	private float _tooltipDuration;

	public FactionType FactionType { get; private set; }

	public bool IsActivated => _knownContainer.activeSelf;

	public void Init()
	{
		UIEventListener.Get(((Component)_textRemainTime).gameObject).onClick = OnClickRemainTime;
		UIEventListener.Get(((Component)_iconRemainTime).gameObject).onClick = OnClickRemainTime;
	}

	public void SetSummary(Messages.Faction? msgFaction)
	{
		if (msgFaction.HasValue)
		{
			_knownContainer.SetActive(true);
			_unknownContainer.SetActive(false);
			SetFaction(msgFaction.Value);
		}
		else
		{
			_knownContainer.SetActive(false);
			_unknownContainer.SetActive(true);
		}
	}

	private void SetFaction(Messages.Faction msg)
	{
		FactionType = msg.Type;
		int num = msg.Level - 1;
		string id = $"#faction_{msg.Type.ToString()}";
		double remainTick = msg.AvailableAt - Connections.Frontend.GetPredictedServerTime();
		float gauge = 0f;
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		string text4 = string.Empty;
		string text5 = string.Empty;
		string remainTime = string.Empty;
		if (SingletonDict<FactionType, Yaml.Faction>.Instance.TryGetValue(msg.Type, out var value))
		{
			int num2 = value.level_thresholds.Length;
			int num3 = value.level_thresholds.Get(num - 1, 0);
			int num4 = value.level_thresholds.Get(num, 0);
			int num5 = num4 - num3;
			int num6 = msg.Point - num3;
			if (msg.Level == num2)
			{
				num6 = (num5 = num3);
			}
			gauge = ((num5 <= 0) ? 0f : ((float)num6 / (float)num5));
			text = value.name;
			text2 = value.titles.Get<Gettext>(num, string.Empty);
			text3 = value.friendship_label;
			text4 = string.Format(_formatGaugeValues, num6, num5);
			text5 = GetRewardsText(value.rewards.Get(num + 1));
			remainTime = GetRemainTime(value, remainTick);
		}
		UIUtility.SetSpriteName(_icon, IconMap.Get(id));
		UIUtility.SetLabelText(_textFactionName, text);
		UIUtility.SetLabelText(_textTitle, text2);
		UIUtility.SetLabelText(_textGaugeName, text3);
		UIUtility.SetLabelText(_textGaugeValues, text4);
		UIUtility.SetLabelText(_textRewards, text5);
		SetRemainTime(remainTime);
		SetGauge(gauge);
	}

	private void SetGauge(float ratio)
	{
		if (ratio > 0f)
		{
			((Component)_spriteGaugeBar).gameObject.SetActive(true);
			_spriteGaugeBar.width = Mathf.FloorToInt(ratio * (float)_spriteGaugeBorder.width);
		}
		else
		{
			((Component)_spriteGaugeBar).gameObject.SetActive(false);
		}
	}

	private void SetRemainTime(string remainTime)
	{
		if (remainTime != string.Empty)
		{
			((Component)_textRemainTime).gameObject.SetActive(true);
			((Component)_iconRemainTime).gameObject.SetActive(true);
			UIUtility.SetLabelText(_textRemainTime, remainTime);
		}
		else
		{
			((Component)_textRemainTime).gameObject.SetActive(false);
			((Component)_iconRemainTime).gameObject.SetActive(false);
		}
	}

	private void OnClickRemainTime(GameObject obj)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(string.Empty, T._("다음 연락 가능 시점까지 남은 시간"));
		widgetTooltipControl.Show((UIWidget)_textRemainTime, Vector2.zero, _tooltipDuration);
	}

	[UsedImplicitly]
	private void OnPress(bool press)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((!press || !IsActivated) ? _colorNormal : _colorPressed);
		_icon.color = color;
		_textFactionName.color = color;
		_textTitle.color = color;
		_textGaugeName.color = color;
		_textRemainTime.color = color;
		_iconRemainTime.color = color;
	}

	private static string GetTitleName(string titleId)
	{
		if (!string.IsNullOrEmpty(titleId) && GameSystem<StatisticsSystem>.Instance().TitlesDictionary.TryGetValue(titleId, out var value))
		{
			return value.name;
		}
		return string.Empty;
	}

	private static string GetCurrencyValue(FactionReward factionReward, Currency currency)
	{
		if (factionReward.money.TryGetValue(currency, out var value) && value > 0)
		{
			return ItemSystem.Inventory.CurrencyFormat(value, currency);
		}
		return string.Empty;
	}

	private static string GetRewardsText(FactionReward factionReward)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (factionReward != null)
		{
			string currencyValue = GetCurrencyValue(factionReward, Currency.TStone);
			string currencyValue2 = GetCurrencyValue(factionReward, Currency.Gem);
			if (currencyValue != string.Empty)
			{
				stringBuilder.Append(currencyValue);
				stringBuilder.Append("   ");
			}
			if (currencyValue2 != string.Empty)
			{
				stringBuilder.Append(currencyValue2);
			}
			string titleName = GetTitleName(factionReward.title_id);
			if (titleName != string.Empty)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append(T._("[FFD85B]{0}[-] 칭호 획득", titleName));
			}
		}
		return stringBuilder.ToString();
	}

	private static string GetRemainTime(Yaml.Faction faction, double remainTick)
	{
		if (faction.display_cooltime && remainTick > 0.0)
		{
			return TimerSystem.TimeToString(remainTick, TimePeriod.Min, 3);
		}
		return string.Empty;
	}
}
