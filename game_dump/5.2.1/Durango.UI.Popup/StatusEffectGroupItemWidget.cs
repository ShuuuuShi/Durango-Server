using System;
using System.Text;
using Durango.Logic;
using Durango.Network;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class StatusEffectGroupItemWidget : UIWidget
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UISprite _progressSprite;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UILabel _durationLabel;

	[SerializeField]
	private RectLayout _layout;

	private StatusEffect _data;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying)
		{
			UpdateProgress();
		}
	}

	public void Set(StatusEffect se)
	{
		_data = se;
		_iconSprite.spriteName = se.Icon;
		_titleLabel.text = se.Name;
		if (se.Until > 0.0)
		{
			_durationLabel.gameObject.SetActive(value: true);
			_durationLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				float remainTime = se.GetRemainTime();
				text = T._("{0} 남음", TimedeltaFormatter.Format(remainTime));
				period = TimedeltaFormatter.NextPeriod(remainTime);
				if (se.DailyContents.HasValue && se.DailyContents.Value.ValidUntil > Connections.Frontend.GetPredictedServerTime())
				{
					float receiveAt = se.DailyContents.Value.ReceiveAt;
					DateTime utcNow = DateTime.UtcNow;
					DateTime dateTime = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, DateTimeKind.Utc).AddSeconds(receiveAt).ToLocalTime();
					text += T._(" <bar/> [icon=icon_mainhud_mail] 매일 {0}시 {1}분 보상 지급", dateTime.Hour, dateTime.Minute);
					double num = se.DailyContents.Value.ValidUntil - Connections.Frontend.GetPredictedServerTime();
					period = Mathf.Min((float)num, period);
				}
			}));
		}
		else
		{
			_durationLabel.gameObject.SetActive(value: false);
		}
		using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
		{
			StringBuilder value = reusable.Value;
			value.Append(se.Description);
			string value2 = ((se.EffectDetails != null) ? StatusEffect.EffectsText(se.EffectDetails.GetEnumerator()) : null);
			if (!string.IsNullOrEmpty(value2))
			{
				value.Append("\n<weak>");
				value.Append(value2);
				value.Append("</weak>");
			}
			_descriptionLabel.text = value.ToString().Trim();
		}
		UpdateProgress();
		_layout.UpdateLayout();
	}

	private void UpdateProgress()
	{
		if (_data != null)
		{
			double since = _data.Since;
			double until = _data.Until;
			if (until <= 0.0)
			{
				_progressSprite.fillAmount = 1f;
				return;
			}
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			float fillAmount = 1f - Mathf.Clamp01((float)((predictedServerTime - since) / (until - since)));
			_progressSprite.fillAmount = fillAmount;
		}
	}
}
