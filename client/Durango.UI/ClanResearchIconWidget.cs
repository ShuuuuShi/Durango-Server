using Durango.Network;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ClanResearchIconWidget : UIWidget
{
	private enum State
	{
		None,
		Ready,
		Active,
		Cooltime
	}

	[SerializeField]
	private UISprite _borderSprite;

	[SerializeField]
	private UISprite _fillRatioSprite;

	[SerializeField]
	private UISprite _iconSprite;

	private Yaml.ClanResearch _researchYaml;

	private Messages.ClanResearch _research;

	private int _fillPercent;

	private State _state;

	public void Set(Messages.ClanResearch research)
	{
		_researchYaml = SingletonDict<string, Yaml.ClanResearch>.Get(research.ResearchId);
		if (_researchYaml != null)
		{
			_research = research;
			_iconSprite.spriteName = _researchYaml.Icon;
			_state = State.None;
			Refresh();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying)
		{
			Refresh();
		}
	}

	private void Refresh()
	{
		State state = GetState();
		if (state != _state)
		{
			_state = state;
			_fillPercent = -1;
			switch (state)
			{
			case State.Active:
				_borderSprite.gameObject.SetActive(value: true);
				_fillRatioSprite.gameObject.SetActive(value: true);
				_iconSprite.alpha = 1f;
				break;
			case State.Cooltime:
				_borderSprite.gameObject.SetActive(value: true);
				_fillRatioSprite.gameObject.SetActive(value: true);
				_iconSprite.alpha = 0.5f;
				break;
			default:
				_borderSprite.gameObject.SetActive(value: false);
				_fillRatioSprite.gameObject.SetActive(value: false);
				_iconSprite.alpha = 1f;
				break;
			}
		}
		double num;
		double num2;
		switch (state)
		{
		default:
			return;
		case State.Active:
			num = _research.Until - ((_researchYaml != null) ? _researchYaml.Duration : 0.0);
			num2 = _research.Until;
			break;
		case State.Cooltime:
			num = _research.Until;
			num2 = _research.CooltimeUntil;
			break;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		int num3 = (int)(100.0 * (predictedServerTime - num) / (num2 - num));
		if (_fillPercent != num3)
		{
			_fillPercent = num3;
			_fillRatioSprite.fillAmount = 1f - (float)num3 / 100f;
		}
	}

	private State GetState()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (predictedServerTime < _research.Until)
		{
			return State.Active;
		}
		if (predictedServerTime < _research.CooltimeUntil)
		{
			return State.Cooltime;
		}
		return State.Ready;
	}

	private void OnClick()
	{
		if (_researchYaml == null)
		{
			return;
		}
		State state = GetState();
		string text2 = $"<em>{_researchYaml.Name}</em>";
		WidgetTooltipControl widgetTooltipControl;
		string text3;
		SyncString subtitle;
		switch (state)
		{
		default:
			return;
		case State.Ready:
			widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			text3 = T._("고급 연구소에서 사용할 수 있습니다.");
			subtitle = string.Empty;
			break;
		case State.Active:
			widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			text3 = ClanResearchPopup.GetStatusEffectText(_researchYaml.Effect);
			subtitle = new SyncString(delegate(out string text, out float period)
			{
				double num = _research.Until - Connections.Frontend.GetPredictedServerTime();
				text = TimedeltaFormatter.Format(num);
				period = (float)(num % (double)TimedeltaFormatter.CurrentMinUnit());
			});
			break;
		case State.Cooltime:
			widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			text3 = T._("재사용 대기 중");
			subtitle = new SyncString(delegate(out string text, out float period)
			{
				double num2 = _research.CooltimeUntil - Connections.Frontend.GetPredictedServerTime();
				text = TimedeltaFormatter.Format(num2);
				period = (float)(num2 % (double)TimedeltaFormatter.CurrentMinUnit());
			});
			break;
		}
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Sign = -1;
		widgetTooltipControl.Set(text2, subtitle, text3, 500);
		widgetTooltipControl.Show(this, Vector2.up * 5f, 60f);
	}
}
