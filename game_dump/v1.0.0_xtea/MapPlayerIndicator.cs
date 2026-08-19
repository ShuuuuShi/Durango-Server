using UnityEngine;

public class MapPlayerIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _sprite;

	private PlayerBehavior _player;

	private void OnEnable()
	{
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated += UpdateSprite;
		GameSystem<ClanSystem>.Instance().EnemyClansDirtied += UpdateSprite;
		KSingleton<PlayerManager>.Instance().PlayerClanChanged += OnChangePlayerClan;
	}

	private void OnDisable()
	{
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated -= UpdateSprite;
		GameSystem<ClanSystem>.Instance().EnemyClansDirtied -= UpdateSprite;
		if (KSingleton<PlayerManager>.HasInstance())
		{
			KSingleton<PlayerManager>.Instance().PlayerClanChanged -= OnChangePlayerClan;
		}
	}

	public void SetPlayer(PlayerBehavior player)
	{
		_player = player;
		SetTarget(((Component)_player).gameObject);
		InitSprite();
		UpdateSprite();
	}

	private void InitSprite()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_player == (Object)null))
		{
			if (_player.IsLocalPlayer)
			{
				_sprite.spriteName = "icon_map_myplayer";
				_sprite.color = Color.white;
				UIUtility.ResizeToSquare(_sprite, 30);
				_sprite.depth = 100;
			}
			else
			{
				_sprite.spriteName = "icon_map_otherplayer";
				UIUtility.ResizeToSquare(_sprite, 20);
				_sprite.depth = 10;
			}
		}
	}

	private void UpdateSprite()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_player == (Object)null || _player.IsLocalPlayer)
		{
			return;
		}
		Color32 val = default(Color32);
		((Color32)(ref val))._002Ector((byte)188, (byte)185, (byte)183, byte.MaxValue);
		if (PlayerBehavior.LocalPlayer.ClanId != 0 && _player.ClanId != 0)
		{
			if (PlayerBehavior.LocalPlayer.ClanId == _player.ClanId)
			{
				((Color32)(ref val))._002Ector((byte)0, (byte)230, (byte)113, byte.MaxValue);
			}
			if (GameSystem<ClanSystem>.Instance().IsEnemyClan(_player.ClanId))
			{
				((Color32)(ref val))._002Ector(byte.MaxValue, (byte)61, (byte)0, byte.MaxValue);
			}
		}
		_sprite.color = Color32.op_Implicit(val);
	}

	private void OnChangePlayerClan(PlayerBehavior player)
	{
		if (!((Object)(object)player != (Object)(object)_player))
		{
			UpdateSprite();
		}
	}
}
