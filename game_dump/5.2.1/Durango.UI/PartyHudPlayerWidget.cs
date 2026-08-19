using Durango.Logic.Party;
using Durango.Player;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PartyHudPlayerWidget : UIWidget
{
	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMaskTexture;

	[SerializeField]
	private UILabel _name;

	[SerializeField]
	private Color _nameColorLocalPlayer;

	[SerializeField]
	private Color _nameColorPartyPlayer;

	[SerializeField]
	private Color _nameColorOffline;

	[SerializeField]
	private UILabel _number;

	[SerializeField]
	private UIWidget _gauge;

	[SerializeField]
	private UISprite _life;

	[SerializeField]
	private UISprite _stamina;

	[SerializeField]
	private GameObject _crown;

	[SerializeField]
	private UILabel _statusLabel;

	[SerializeField]
	private float _offlineGaugeAlpha;

	[SerializeField]
	private float _offlinePortraitAlpha;

	[SerializeField]
	private TweenerPlayer _deathEffect;

	private bool? _prevAlive;

	private bool _isLocalPlayer;

	private Member _member;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying && _member != null)
		{
			UpdateMemberInfo();
		}
	}

	public void Set([NotNull] Member member, int index)
	{
		if (_member != null)
		{
			_member.PlayerInfoUpdated -= UpdatePlayerInfo;
		}
		_member = member;
		_member.PlayerInfoUpdated += UpdatePlayerInfo;
		_isLocalPlayer = PlayerBehavior.LocalPlayer.EntityId == _member.EntityId;
		_number.text = index.ToString();
		_crown.SetActive(_member.IsLeader);
		_gauge.gameObject.SetActive(_member.IsAccepted);
		UpdateMemberInfo();
		UpdatePlayerInfo(member.PlayerInfo);
	}

	private void UpdateMemberInfo()
	{
		bool isAlive = _member.IsAlive;
		bool? prevAlive = _prevAlive;
		if (!prevAlive.HasValue || isAlive != _prevAlive.Value)
		{
			_prevAlive = isAlive;
			PlayDeathEffect(!isAlive);
		}
		_life.fillAmount = _member.Life;
		_stamina.fillAmount = _member.Stamina;
		bool isOffline = _member.IsOffline;
		bool isAccepted = _member.IsAccepted;
		if (isOffline)
		{
			_number.color = new Color32(149, 149, 149, byte.MaxValue);
		}
		else if (isAlive)
		{
			_number.color = ((!_isLocalPlayer) ? ((Color)new Color32(228, 228, 228, byte.MaxValue)) : PresetColor.PlayerParty);
		}
		else
		{
			_number.color = PresetColor.UIPaleRed;
		}
		if (_isLocalPlayer)
		{
			_name.color = _nameColorLocalPlayer;
		}
		else
		{
			_name.color = ((!isOffline && isAccepted) ? _nameColorPartyPlayer : _nameColorOffline);
		}
		_gauge.alpha = ((!isOffline) ? 1f : _offlineGaugeAlpha);
		_portraitTexture.alpha = ((!isOffline && _member.IsAccepted) ? 1f : _offlinePortraitAlpha);
		if (!isAccepted)
		{
			_statusLabel.text = T._("수락 대기");
		}
		else if (isOffline)
		{
			_statusLabel.text = T._("오프라인");
		}
		_statusLabel.gameObject.SetActive(isOffline || !_member.IsAccepted);
	}

	private void UpdatePlayerInfo(PlayerInfo info)
	{
		if (info == null || !info.Valid)
		{
			_name.text = T._("불러오는 중...");
			_portraitTexture.gameObject.SetActive(value: false);
			return;
		}
		_name.text = info.Name;
		_portraitTexture.gameObject.SetActive(value: true);
		PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
		portraitArgument.Mask = _portraitMaskTexture;
		PortraitBuilder.Set(portraitArgument, _portraitTexture);
	}

	private void PlayDeathEffect(bool isPlay)
	{
		if (!(_deathEffect == null))
		{
			if (isPlay)
			{
				_deathEffect.Play();
			}
			else
			{
				_deathEffect.Stop();
			}
		}
	}
}
