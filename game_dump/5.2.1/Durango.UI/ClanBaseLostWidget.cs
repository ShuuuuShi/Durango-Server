using System;
using Durango.Logic.Clan;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ClanBaseLostWidget : UIWidget
{
	[SerializeField]
	private UILabel _locationLabel;

	[SerializeField]
	private UILabel _defenceCycleLabel;

	[SerializeField]
	private UILabel _defenceDurationLabel;

	[SerializeField]
	private UILabel _defenceDurationValueLabel;

	[SerializeField]
	private UILabel _unoccupiedAtLabel;

	[SerializeField]
	private UILabel _unoccupiedAtValueLabel;

	[SerializeField]
	private UILabel _unoccupiedByLabel;

	[SerializeField]
	private UILabel _unoccupiedByValueLabel;

	[SerializeField]
	private RectLayout _layout;

	private EstateLicenses _data;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_defenceDurationLabel.text = T._("거점 점유 기간");
			_unoccupiedAtLabel.text = T._("빼앗긴 시각");
			_unoccupiedByLabel.text = T._("빼앗아간 부족");
			_layout.UpdateOnSizeChange();
		}
	}

	public void Set(EstateLicenses data)
	{
		ClanCargoWarphole? clanCargoWarphole = data.ClanCargoWarphole;
		if (!clanCargoWarphole.HasValue)
		{
			return;
		}
		ClanCargoWarphole value = data.ClanCargoWarphole.Value;
		if (value.OccupiedAt.HasValue && value.OccupiedUntil.HasValue)
		{
			Init();
			_data = data;
			_locationLabel.text = ((!value.Location.HasValue) ? string.Empty : value.Location.Value.GetText());
			_defenceCycleLabel.text = $"[size=26][b][i]{value.MaxDefensedDays:0}[/i][b][/size]회차 방어 성공";
			double seconds = value.OccupiedUntil.Value - value.OccupiedAt.Value;
			_defenceDurationValueLabel.text = TimedeltaFormatter.Format(seconds, 2, "min");
			DateTime dateTime = Times.UnixTimeToDateTimeLocal(value.OccupiedUntil.Value);
			_unoccupiedAtValueLabel.text = dateTime.ToString("g", T.Culture);
			_unoccupiedByValueLabel.text = string.Empty;
			if (!string.IsNullOrEmpty(value.UnoccupiedBy))
			{
				ClanSystem.GetClanInfo(value.UnoccupiedBy, OnUnoccupiedBy, refresh: false, detail: false);
			}
		}
	}

	private void OnUnoccupiedBy(Clan clan)
	{
		if (clan != null)
		{
			ClanCargoWarphole? clanCargoWarphole = _data.ClanCargoWarphole;
			if (clanCargoWarphole.HasValue && !(_data.ClanCargoWarphole.Value.UnoccupiedBy != clan.Id))
			{
				_unoccupiedByValueLabel.text = clan.Name;
			}
		}
	}
}
