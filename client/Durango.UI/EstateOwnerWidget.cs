using System;
using Durango.Logic.Clan;
using Durango.Logic.Estate;
using Durango.Network;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Estate;
using UnityEngine;

namespace Durango.UI;

public class EstateOwnerWidget : AnimationWidget
{
	[Serializable]
	private struct TypeOption
	{
		public Option My;

		public Option MyClan;

		public Option Other;

		public Option OtherClan;
	}

	[Serializable]
	private struct Option
	{
		public Color color;
	}

	[SerializeField]
	private UIWidget _emblemWidget;

	[SerializeField]
	private UITexture _emblemTexture;

	[SerializeField]
	private UITexture _noEmblem;

	[SerializeField]
	private UIWidget _portrantWidget;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMask;

	[SerializeField]
	private UIWidget _nameWidget;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private float _period;

	[SerializeField]
	private TypeOption _options;

	[SerializeField]
	private RectLayout _layout;

	private float _hideAt;

	private bool _isShow;

	private EstateInfo _estate;

	private float _timerUpdateAt;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			SetAlpha(0f, useTween: false);
			base.gameObject.SetActive(value: false);
		}
	}

	private void Start()
	{
		Init();
	}

	private void Update()
	{
		float time = Time.time;
		if (_hideAt < time)
		{
			Hide();
		}
		if (_timerUpdateAt > 0f && _timerUpdateAt < time)
		{
			UpdateTimer();
		}
	}

	public void Show(EstateInfo estate)
	{
		if (estate.License.Type != OwnerType.System)
		{
			Init();
			_isShow = true;
			_estate = estate;
			if (estate.License.Type == OwnerType.Player || estate.License.Type == OwnerType.PersonalPlayer)
			{
				Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(estate.License.OwnerId, OnPlayer);
			}
			else
			{
				_emblemTexture.alpha = 0f;
				_noEmblem.alpha = 1f;
				ClanSystem.GetClanInfo(estate.License.OwnerId, OnClan);
				ClanSystem.GetEmblem(estate.License.OwnerId, OnClanEmblem);
			}
			UpdateTimer();
			_hideAt = Time.time + _period;
			base.gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		if (_isShow)
		{
			_isShow = false;
			base.Alpha = 0f;
		}
	}

	private void OnClan(Clan clan)
	{
		if (_isShow && (_estate.License.Type == OwnerType.ClanEstate || _estate.License.Type == OwnerType.ClanWarphole) && clan != null && !(clan.Id != _estate.License.OwnerId))
		{
			_emblemWidget.gameObject.SetActive(value: true);
			_portrantWidget.gameObject.SetActive(value: false);
			Option option = GetOption(_estate);
			string text = string.Format("[{1}]{0}[-]", clan.Name, NGUIText.EncodeColor24(option.color));
			if (_estate.License.Type == OwnerType.ClanEstate)
			{
				_nameLabel.text = T._("{0} 부족의 영토", text);
			}
			else if (_estate.License.Type == OwnerType.ClanWarphole)
			{
				_nameLabel.text = T._("{0} 부족의 거점", text);
			}
			_nameWidget.width = _nameLabel.width + 30;
			_layout.UpdateLayout(0f, base.Widget.height);
			UIUtility.UpdateAnchors(base.transform);
			base.Alpha = 1f;
		}
	}

	private void OnClanEmblem(Point2 pos)
	{
		if (_isShow && pos.x >= 0 && pos.y >= 0)
		{
			_emblemTexture.alpha = 1f;
			_noEmblem.alpha = 0f;
			EmblemTexture.Set(_emblemTexture, pos);
		}
	}

	private void OnPlayer(Durango.Player.PlayerInfo player)
	{
		if (_isShow && (_estate.License.Type == OwnerType.Player || _estate.License.Type == OwnerType.PersonalPlayer) && !(_estate.License.OwnerId != player.EntityId) && player.Valid)
		{
			_emblemWidget.gameObject.SetActive(value: false);
			_portrantWidget.gameObject.SetActive(value: true);
			PortraitBuilder.Argument portraitArgument = player.GetPortraitArgument();
			portraitArgument.Mask = _portraitMask;
			PortraitBuilder.Set(portraitArgument, _portraitTexture);
			Option option = GetOption(_estate);
			_nameLabel.text = T._("{0} 님의 사유지", string.Format("[{1}]{0}[-]", player.Name, NGUIText.EncodeColor24(option.color)));
			_nameWidget.width = _nameLabel.width + 30;
			_layout.UpdateLayout(0f, base.Widget.height);
			UIUtility.UpdateAnchors(base.transform);
			base.Alpha = 1f;
		}
	}

	private Option GetOption(EstateInfo info)
	{
		switch (info.License.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			return (!(info.License.OwnerId == GameManager.PlayerId)) ? _options.Other : _options.My;
		case OwnerType.ClanEstate:
		case OwnerType.ClanWarphole:
			return (!ClanSystem.IsMyClan(info.License.OwnerId)) ? _options.OtherClan : _options.MyClan;
		default:
			return default(Option);
		}
	}

	private void UpdateTimer()
	{
		if (_estate == null)
		{
			_timerUpdateAt = 0f;
			_timerLabel.text = string.Empty;
			return;
		}
		GetTimerText(_estate.License, out var text, out var nextUpdateDelay);
		_timerLabel.text = text;
		if (nextUpdateDelay > 0f)
		{
			_timerUpdateAt = Time.time + nextUpdateDelay;
		}
		else
		{
			_timerUpdateAt = 0f;
		}
	}

	private static void GetTimerText(EstateLicense license, out string text, out float nextUpdateDelay)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		switch (license.Type)
		{
		case OwnerType.Player:
		case OwnerType.ClanEstate:
		{
			if (!license.DepositRunsOutAt.HasValue || !license.ExpiresAt.HasValue || license.ExpiresAt.Value < predictedServerTime)
			{
				nextUpdateDelay = 0f;
				text = string.Empty;
				break;
			}
			double num3 = license.DepositRunsOutAt.Value - predictedServerTime;
			int num4;
			if (num3 > 0.0)
			{
				text = T._("{0} 남음", TimedeltaFormatter.Format(num3, 2, "min"));
				num4 = TimedeltaFormatter.CurrentMinUnit();
			}
			else
			{
				num3 = license.ExpiresAt.Value - predictedServerTime;
				text = T._("만료됨 ({0} 후 완전히 사라짐)", TimedeltaFormatter.Format(num3, 2, "min"));
				num4 = TimedeltaFormatter.CurrentMinUnit();
			}
			if (num4 > 0)
			{
				nextUpdateDelay = (float)(num3 % (double)num4);
			}
			else
			{
				nextUpdateDelay = 0f;
			}
			break;
		}
		case OwnerType.ClanWarphole:
			if (!string.IsNullOrEmpty(license.OwnerId))
			{
				double? protectedUntil = license.ProtectedUntil;
				if (protectedUntil.HasValue)
				{
					double? cycleEndsAt = license.CycleEndsAt;
					if (cycleEndsAt.HasValue)
					{
						double num;
						int num2;
						if (predictedServerTime < license.ProtectedUntil.Value)
						{
							num = license.ProtectedUntil.Value - predictedServerTime;
							text = string.Format("[icon=estate_peace] {0}", T._("보호 중 {0} 남음", TimedeltaFormatter.Format(num, 2, "min")));
							num2 = TimedeltaFormatter.CurrentMinUnit();
						}
						else
						{
							if (!(predictedServerTime < license.CycleEndsAt.Value))
							{
								nextUpdateDelay = 0f;
								text = string.Empty;
								break;
							}
							num = license.CycleEndsAt.Value - predictedServerTime;
							text = string.Format("[{1}][icon=act_Attack] {0}[-]", T._("전쟁 중 {0} 남음", TimedeltaFormatter.Format(num, 2, "min")), NGUIText.EncodeColor(new Color32(186, 46, 46, byte.MaxValue)));
							num2 = TimedeltaFormatter.CurrentMinUnit();
						}
						if (num2 > 0)
						{
							nextUpdateDelay = (float)(num % (double)num2);
						}
						else
						{
							nextUpdateDelay = 0f;
						}
						break;
					}
				}
			}
			nextUpdateDelay = 0f;
			text = string.Empty;
			break;
		default:
			nextUpdateDelay = 0f;
			text = string.Empty;
			break;
		}
	}
}
