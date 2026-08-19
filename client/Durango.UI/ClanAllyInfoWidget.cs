using System;
using Durango.Logic.Clan;
using Durango.Network;
using UnityEngine;

namespace Durango.UI;

public class ClanAllyInfoWidget : UIWidget
{
	[SerializeField]
	private UISprite _levelPrefix;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISprite _memberIcon;

	[SerializeField]
	private UILabel _memberLabel;

	[SerializeField]
	private UISprite _timerIcon;

	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private UISprite _separator;

	[SerializeField]
	private UIWidget _commentWidget;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private RectLayout _layout;

	public void Set(string clanId, double? time, bool isDelta, string comment)
	{
		alpha = 0f;
		if (string.IsNullOrEmpty(comment))
		{
			_commentWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_commentWidget.gameObject.SetActive(value: true);
			_commentLabel.text = comment;
		}
		_layout.UpdateLayout(Mathf.Min(UIManager.SafeWidth - 40, 800), 0f);
		if (time.HasValue)
		{
			_timerLabel.gameObject.SetActive(value: true);
			if (isDelta)
			{
				_timerLabel.text = TimedeltaFormatter.Format(time.Value);
			}
			else
			{
				double t = time.Value;
				_timerLabel.SetText(new SyncString(delegate(out string text, out float period)
				{
					double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
					double num = t - predictedServerTime;
					text = TimedeltaFormatter.Format(Math.Max(1.0, num));
					period = (float)(num % 1.0);
				}));
			}
		}
		else
		{
			_timerLabel.gameObject.SetActive(value: false);
		}
		ClanSystem.GetClanInfo(clanId, OnClan);
	}

	private void OnClan(Clan clan)
	{
		alpha = 1f;
		_levelLabel.text = clan.Level.ToString();
		_nameLabel.text = clan.Name;
		_memberLabel.text = $"{clan.MemberCount} [FFFFFF7F]/[-] {clan.Capacity}";
		int num = _levelPrefix.width + 10 + _levelLabel.width + 16 + _nameLabel.width;
		Vector3 pos = Vector3.left * num * 0.5f;
		_levelPrefix.SetPosition(pos, 0f, 0.5f);
		pos.x += _levelPrefix.width + 10;
		_levelLabel.SetPosition(pos, 0f, 0.5f);
		pos.x += _levelLabel.width + 16;
		_nameLabel.SetPosition(pos, 0f, 0.5f);
		if (_timerLabel.gameObject.activeSelf)
		{
			_timerIcon.gameObject.SetActive(value: true);
			_separator.gameObject.SetActive(value: true);
			Vector3 zero = Vector3.zero;
			zero.x -= 30f;
			_memberLabel.SetPosition(zero, 1f, 0.5f);
			zero.x -= _memberLabel.width + 5;
			_memberIcon.SetPosition(zero, 1f, 0.5f);
			zero = Vector3.zero;
			zero.x += 30f;
			_timerIcon.SetPosition(zero, 0f, 0.5f);
			zero.x += _timerIcon.width + 5;
			_timerLabel.SetPosition(zero, 0f, 0.5f);
			_separator.SetPosition(Vector3.zero, 0.5f, 0.5f);
		}
		else
		{
			_timerIcon.gameObject.SetActive(value: false);
			_separator.gameObject.SetActive(value: false);
			int num2 = _memberLabel.width + _memberIcon.width + 5;
			Vector3 pos2 = Vector3.left * num2 * 0.5f;
			_memberIcon.SetPosition(pos2, 0f, 0.5f);
			pos2.x += _memberIcon.width + 5;
			_memberLabel.SetPosition(pos2, 0f, 0.5f);
		}
		UIUtility.UpdateAnchors(base.transform);
	}
}
