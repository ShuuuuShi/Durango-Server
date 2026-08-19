using Durango.Logic.Clusters;
using Durango.Logic.Party;
using Durango.Logic.WarpRush;
using Durango.Player;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class MapPlayerIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _sprite;

	private bool _isDirectional;

	[SerializeField]
	private UISprite _number;

	[SerializeField]
	private GameObject _border;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMaskTexture;

	[CanBeNull]
	private PlayerBehavior _player;

	[CanBeNull]
	private Durango.Logic.Party.Member _partyMember;

	[CanBeNull]
	private Durango.Logic.WarpRush.Member _warpRushMember;

	private bool _isWorldmapMode;

	public bool IsDirectional
	{
		get
		{
			return _isDirectional;
		}
		set
		{
			_isDirectional = value;
		}
	}

	public override void OnInitialized()
	{
		base.OnInitialized();
		_isWorldmapMode = Singleton<MapContext>.Instance().IsWorldMapMode;
		SetPlayer(null);
		SetPartyMember(null, -1);
		SetWarpRushMember(null);
	}

	public override void OnRefresh(Refresh type)
	{
		if (type == Refresh.ClanInfoUpdated || type == Refresh.PlayerClanChanged)
		{
			UpdateSpriteColor();
		}
		if (type == Refresh.MapModeChanged)
		{
			_isWorldmapMode = Singleton<MapContext>.Instance().IsWorldMapMode;
			UpdatePortrait();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (IsDirectional && _player != null)
		{
			_sprite.transform.localRotation = Quaternion.AngleAxis(0f - (_player.CurrentYaw - 45f), new Vector3(0f, 0f, 1f));
		}
		if (_partyMember != null)
		{
			bool isAlive = _partyMember.IsAlive;
			_number.color = ((!isAlive) ? PresetColor.UIPaleRed : PresetColor.PlayerParty);
			Tile = _partyMember.Tile;
			bool hide = _partyMember.RegionId != GameManager.Region.Id || _partyMember.IsOffline;
			ToggleHideFlag(HideFlag.Member, hide);
		}
		else if (_warpRushMember != null)
		{
			Tile = _warpRushMember.Tile;
			ToggleHideFlag(HideFlag.Member, _warpRushMember.IsOffline);
		}
		else
		{
			Tile = -Point2.one;
		}
	}

	public void SetPlayer([CanBeNull] PlayerBehavior player)
	{
		if (_player != null)
		{
			_player.VisibleChanged -= Player_VisibleChanged;
		}
		_player = player;
		SetTarget((!(_player != null)) ? null : _player.gameObject);
		UpdateReveal();
		UpdateSprite();
		if (_player != null)
		{
			_player.VisibleChanged += Player_VisibleChanged;
			Player_VisibleChanged(_player.GetVisible());
		}
	}

	public void SetPartyMember(Durango.Logic.Party.Member member, int index)
	{
		if (_partyMember != null)
		{
			_partyMember.PlayerInfoUpdated -= PartyMember_PlayerInfoUpdated;
		}
		_partyMember = member;
		base.StickToBoundary = _partyMember != null;
		_number.gameObject.SetActive(_partyMember != null);
		_number.spriteName = "num_" + index;
		UpdateReveal();
		UpdateSprite();
		UpdatePortrait();
		if (_partyMember != null)
		{
			_partyMember.PlayerInfoUpdated += PartyMember_PlayerInfoUpdated;
		}
	}

	public void SetWarpRushMember(Durango.Logic.WarpRush.Member member)
	{
		_warpRushMember = member;
		UpdateReveal();
	}

	private void UpdateReveal()
	{
		bool flag = _player != null && (_player.IsLocalPlayer || ClanSystem.IsMyClan(_player));
		if (GameManager.ClusterMode != Mode.Online || flag || _partyMember != null || _warpRushMember != null)
		{
			base.CheckReveal = false;
			ToggleHideFlag(HideFlag.Reveal, hide: false);
		}
		else
		{
			base.CheckReveal = true;
		}
	}

	private void UpdateSprite()
	{
		if (_player != null && _player.IsLocalPlayer)
		{
			if (IsDirectional)
			{
				_sprite.spriteName = "icon_map_myplayer_dir";
				_sprite.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			}
			else
			{
				_sprite.spriteName = "icon_map_myplayer";
				_sprite.color = PresetColor.UIPaleRed;
			}
			UIUtility.ResizeToSquare(_sprite, 30);
			_sprite.depth = 90;
		}
		else if (_partyMember != null)
		{
			_sprite.spriteName = "circle_32";
			UIUtility.ResizeToSquare(_sprite, 20);
			_sprite.depth = 20;
		}
		else
		{
			_sprite.spriteName = "icon_map_otherplayer";
			UIUtility.ResizeToSquare(_sprite, 20);
			_sprite.depth = 20;
		}
		UpdateSpriteColor();
	}

	private void UpdateSpriteColor()
	{
		if (!(_player != null) || !_player.IsLocalPlayer)
		{
			if (_partyMember != null)
			{
				_sprite.color = PresetColor.UIBlack;
			}
			else if (_player != null && ClanSystem.IsMyClan(_player))
			{
				_sprite.color = new Color32(0, 230, 113, byte.MaxValue);
			}
			else
			{
				_sprite.color = new Color32(188, 185, 183, byte.MaxValue);
			}
		}
	}

	private void UpdatePortrait()
	{
		_border.SetActive(_partyMember != null && !_isWorldmapMode);
		bool flag = _partyMember != null && _partyMember.PlayerInfo != null && _partyMember.PlayerInfo.Valid;
		_portraitTexture.gameObject.SetActive(_isWorldmapMode && flag);
		if (flag)
		{
			PortraitBuilder.Argument portraitArgument = _partyMember.PlayerInfo.GetPortraitArgument();
			portraitArgument.Mask = _portraitMaskTexture;
			PortraitBuilder.Set(portraitArgument, _portraitTexture);
		}
	}

	private void Player_VisibleChanged(bool visible)
	{
		ToggleHideFlag(HideFlag.EntityVisible, !visible);
	}

	private void PartyMember_PlayerInfoUpdated(Durango.Player.PlayerInfo info)
	{
		UpdatePortrait();
	}
}
