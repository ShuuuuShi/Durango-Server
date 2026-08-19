using System;
using Durango.Logic.Clan;
using Durango.Logic.Explore;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Terrain;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Estate;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class EstateInfoWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject _titleObject;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Transform _emblemSprite;

	[SerializeField]
	private GameObject _privateIsland;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _posLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _periodLabel;

	[SerializeField]
	private UILabel _helperLabel;

	[SerializeField]
	private ListObjectPool _bottomLabelPool;

	private EstateLicense _license;

	private long _extendCost;

	private long _costValue = -1L;

	private string _costValueText;

	private float _timerUpdateAt;

	private void Update()
	{
		float time = Time.time;
		if (_timerUpdateAt > 0f && _timerUpdateAt < time)
		{
			UpdateTimer();
		}
	}

	public void Set(EstateLicense info, int largestSize, EntityTile? warpholeTile)
	{
		_license = info;
		_extendCost = Singleton<CostsYaml>.Instance.Estate.GetExtendingCost(info.Type, info.Size) * info.Size;
		_titleObject.SetActive(info.Type != OwnerType.PersonalPlayer);
		switch (info.Type)
		{
		case OwnerType.Player:
			_titleLabel.text = T._("도시섬");
			break;
		case OwnerType.ClanEstate:
		{
			Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
			_titleLabel.text = T._("<em>{0} 부족</em> 영토", (playerClan == null) ? string.Empty : playerClan.Name);
			break;
		}
		}
		_helperLabel.text = T._("사유지 이사 <help>{0}</help>", T._("사유지 권한을 포기한 뒤, 다시 사유지를 선언할 때에는 <em>사유지 선언 비용만 들고, 한번 확장했던 땅 면적만큼은 확장 비용이 무료</em>입니다!\n<hr/>\n인구 밀도가 낮은 섬으로 이사가고 싶다면?<br>8</br><ref>ui://Estate/RecommendedRegion,추천 섬 보기</ref>"));
		Point2 point = new Point2(MapPositionParser.PositionToHumaneTile(Durango.Terrain.Util.TilePositionToWorldPosition(info.Tile)));
		_posLabel.text = $"{point.x}, {point.y}";
		UpdateBottomLabels(info.Size, largestSize, warpholeTile);
		UpdateTimer();
		_privateIsland.SetActive(info.Type == OwnerType.PersonalPlayer);
		_emblemSprite.gameObject.SetActive(value: false);
		_nameLabel.gameObject.SetActive(value: false);
		_levelLabel.gameObject.SetActive(value: false);
		GameSystem<MapSystem>.Instance().GetRegion(info.RegionId, OnRegion);
	}

	private void OnRegion(Messages.Region region)
	{
		_nameLabel.gameObject.SetActive(value: true);
		_levelLabel.gameObject.SetActive(value: true);
		Durango.Logic.Explore.Region region2 = new Durango.Logic.Explore.Region(region);
		if (_license.Type != OwnerType.PersonalPlayer)
		{
			_emblemSprite.gameObject.SetActive(value: true);
			Durango.Logic.Explore.Region.InstantiateIcon(_emblemSprite, region2.GetEmblem() + "_estate");
		}
		_nameLabel.text = region.Name;
		_levelLabel.text = T._("{0:lv:} {1}", region2.Level, region2.MajorBiome().GetName());
	}

	private void UpdateBottomLabels(int size, int largestSize, EntityTile? warpholeTile)
	{
		_bottomLabelPool.BeginLoad();
		if (_license.Type == OwnerType.PersonalPlayer)
		{
			KeyValueLabel component = _bottomLabelPool.GetNext().GetComponent<KeyValueLabel>();
			component.SetKey(T._("연결된 섬 <help>{0}</help>", T._("연결된 섬이란, 내 개인섬 워프홀이 연결된 도시섬을 말합니다.")));
			component.SetValue((!warpholeTile.HasValue) ? T._("없음") : warpholeTile.Value.GetText(17));
		}
		else
		{
			KeyValueLabel component2 = _bottomLabelPool.GetNext().GetComponent<KeyValueLabel>();
			component2.SetKey(T._("1일 유지비 <help>{0}</help>", (_license.Type != 0) ? T._("부족 영토의 면적에 비례하여 1일 유지비가 결정됩니다. 유지비는 지속적으로 예치금에서 차감됩니다.") : T._("사유지의 면적에 비례하여 1일 유지비가 결정됩니다. 유지비는 지속적으로 예치금에서 차감됩니다.")));
			component2.SetValue(Durango.Logic.Item.Inventory.CurrencyFormat(_extendCost, Currency.TStone));
			KeyValueLabel component3 = _bottomLabelPool.GetNext().GetComponent<KeyValueLabel>();
			component3.SetKey(T._("예치금 <help>{0}</help>", (_license.Type != 0) ? T._("부족 영토의 면적과 예치금 액수에 따라 부족 영토 기간이 결정됩니다. 예치금을 늘리려면 우측의 부족 영토 기간 연장 버튼을 누르세요.") : T._("사유지의 면적과 예치금 액수에 따라 사유지 기간이 결정됩니다. 예치금을 늘리려면 우측의 사유지 기간 연장 버튼을 누르세요.")));
			component3.SetValue(new SyncString(delegate(out string text, out float period)
			{
				period = 0.5f;
				double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
				Pair<int, double>? deposit = _license.Deposit;
				long val = (deposit.HasValue ? (_license.Deposit.Value.Item1 - (int)((predictedServerTime - _license.Deposit.Value.Item2) * (double)_extendCost / 86400.0)) : 0);
				val = Math.Max(0L, val);
				if (val != _costValue)
				{
					_costValue = val;
					_costValueText = Durango.Logic.Item.Inventory.CurrencyFormat(_costValue, Currency.TStone);
				}
				text = _costValueText;
			}));
		}
		KeyValueLabel component4 = _bottomLabelPool.GetNext().GetComponent<KeyValueLabel>();
		switch (_license.Type)
		{
		case OwnerType.PersonalPlayer:
			component4.SetKey(T._("개인섬 사유지 면적 현재 / 최대 <help>{0}</help>", T._("개인섬 사유지로 선언한 면적과 최대 확장 가능한 면적을 의미합니다.")));
			break;
		case OwnerType.Player:
			component4.SetKey(T._("사유지 면적 현재 / 최대 <help>{0}</help>", T._("사유지의 현재 면적이 최대 면적보다 작다면, 최대 면적까지 확장하는데는 추가로 티스톤이 들지 않습니다. 최대 면적을 확장할 때만 티스톤이 필요합니다.")));
			break;
		case OwnerType.ClanEstate:
			component4.SetKey(T._("부족 영토 면적 현재 / 최대 <help>{0}</help>", T._("부족 영토의 현재 면적이 최대 면적보다 작다면, 최대 면적까지 확장하는데는 추가로 티스톤이 들지 않습니다. 최대 면적을 확장할 때만 티스톤이 필요합니다.")));
			break;
		}
		component4.SetValue($"<em>{size}</em> / {largestSize}");
		_bottomLabelPool.EndLoad();
		UIUtility.WidgetsReposition(_bottomLabelPool, new Vector3(0f, 1f), new Vector3(0f, -105f));
	}

	private void UpdateTimer()
	{
		GetTimerText(_license, out var text, out var color, out var nextUpdateDelay);
		_periodLabel.text = text;
		color.a = _periodLabel.alpha;
		_periodLabel.color = color;
		if (nextUpdateDelay > 0f)
		{
			_timerUpdateAt = Time.time + nextUpdateDelay;
		}
		else
		{
			_timerUpdateAt = 0f;
		}
	}

	private static void GetTimerText(EstateLicense license, out string text, out Color color, out float nextUpdateDelay)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (!license.DepositRunsOutAt.HasValue || !license.ExpiresAt.HasValue || license.ExpiresAt.Value < predictedServerTime)
		{
			nextUpdateDelay = 0f;
			text = string.Empty;
			color = Color.white;
			return;
		}
		double num = license.DepositRunsOutAt.Value - predictedServerTime;
		int num2;
		if (num > 0.0)
		{
			if (num > 259200.0)
			{
				text = T._("{0} 남음", TimedeltaFormatter.Format(num, 2, "min"));
				num2 = TimedeltaFormatter.CurrentMinUnit();
				color = Color.white;
			}
			else
			{
				text = T._("[icon=icon_make_alert] {0} 남음", TimedeltaFormatter.Format(num, 2, "min"));
				num2 = TimedeltaFormatter.CurrentMinUnit();
				color = new Color32(245, 62, 62, byte.MaxValue);
			}
		}
		else
		{
			num = license.ExpiresAt.Value - predictedServerTime;
			text = T._("[icon=icon_make_alert] 만료됨 ({0} 후 완전히 사라짐)", TimedeltaFormatter.Format(num, 2, "min"));
			num2 = TimedeltaFormatter.CurrentMinUnit();
			color = new Color32(245, 62, 62, byte.MaxValue);
		}
		if (num2 > 0)
		{
			nextUpdateDelay = (float)(num % (double)num2);
		}
		else
		{
			nextUpdateDelay = 0f;
		}
	}
}
