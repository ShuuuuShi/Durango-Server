using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class PlayerFloatingGroup : UIBase
{
	[SerializeField]
	private GameObject _floatingUIBase;

	[SerializeField]
	private Color _clanTextColor;

	private readonly List<PlayerFloatingControl> _controls = new List<PlayerFloatingControl>();

	private bool _hideLocalPlayer;

	private void Start()
	{
		Singleton<PlayerManager>.Instance().PlayerAppeared += OnAppearPlayer;
		Singleton<PlayerManager>.Instance().PlayerDisappeared += OnDisappearPlayer;
		Singleton<PlayerManager>.Instance().PlayerClanChanged += OnPlayerClanChange;
		Singleton<PlayerManager>.Instance().PlayerTitleChanged += OnPlayerTitleChange;
		GameSystem<ClanSystem>.Instance().ClanChanged += RefreshStates;
		GameSystem<ClanSystem>.Instance().AlliesUpdated += RefreshStates;
		GameSystem<PartySystem>.Instance().MembersUpdated += RefreshStates;
		GameSystem<StatusEffectSystem>.Instance().StatusEffectsUpdated += StatusEffectsUpdated;
		Refresh();
	}

	private void StatusEffectsUpdated(StatusEffects effects)
	{
		if (effects.EntityId != GameManager.PlayerId)
		{
			return;
		}
		string floatingStatusIcon = GetFloatingStatusIcon();
		foreach (PlayerFloatingControl control in _controls)
		{
			if (control.Target == PlayerBehavior.LocalPlayer)
			{
				control.SetFloatingIcon(floatingStatusIcon);
				break;
			}
		}
	}

	private static string GetFloatingStatusIcon()
	{
		StatusEffects statusEffects = GameSystem<StatusEffectSystem>.Instance().GetStatusEffects();
		StatusEffect statusEffect = statusEffects.List.FirstOrDefault((StatusEffect x) => !string.IsNullOrEmpty(x.FloatingIcon));
		return (statusEffect == null) ? string.Empty : statusEffect.FloatingIcon;
	}

	private void LateUpdate()
	{
		for (int num = _controls.Count - 1; num >= 0; num--)
		{
			PlayerFloatingControl playerFloatingControl = _controls[num];
			if (playerFloatingControl.Target != null)
			{
				playerFloatingControl.Process(_hideLocalPlayer);
			}
			else
			{
				Remove(playerFloatingControl);
			}
		}
	}

	private void OnAppearPlayer(PlayerBehavior player)
	{
		MakeControl(player);
	}

	private void OnDisappearPlayer(PlayerBehavior player)
	{
		Remove(GetControl(player));
	}

	private void OnPlayerClanChange(PlayerBehavior player)
	{
		SetClan(player);
	}

	private void OnPlayerTitleChange(PlayerBehavior player)
	{
		SetTitle(player, player.Title._Title);
	}

	private void Refresh()
	{
		MakeControl(PlayerBehavior.LocalPlayer);
		IEnumerable<PlayerBehavior> players = Singleton<PlayerManager>.Instance().GetPlayers();
		foreach (PlayerBehavior item in players)
		{
			MakeControl(item);
		}
	}

	private void RefreshStates()
	{
		int i = 0;
		for (int count = _controls.Count; i < count; i++)
		{
			RefreshLabelColor(_controls[i]);
		}
	}

	private PlayerFloatingControl GetControl(PlayerBehavior player, bool make = false)
	{
		PlayerFloatingControl playerFloatingControl = null;
		int count = _controls.Count;
		for (int i = 0; i < count; i++)
		{
			if (_controls[i].Target == player)
			{
				playerFloatingControl = _controls[i];
				break;
			}
		}
		if (make && playerFloatingControl == null)
		{
			playerFloatingControl = base.gameObject.AddChild(_floatingUIBase.gameObject).GetComponent<PlayerFloatingControl>();
			playerFloatingControl.Target = player;
			playerFloatingControl.gameObject.SetActive(value: false);
			_controls.Add(playerFloatingControl);
		}
		return playerFloatingControl;
	}

	public void HideLocalPlayer()
	{
		_hideLocalPlayer = true;
	}

	private void MakeControl(PlayerBehavior player)
	{
		if (!(player == null))
		{
			PlayerFloatingControl control = GetControl(player, make: true);
			control.SetName(player.PlayerName);
			control.SetTitle(player.Title._Title);
			control.SetClan(player);
			string floatingIcon = ((!player.IsLocalPlayer) ? string.Empty : GetFloatingStatusIcon());
			control.SetFloatingIcon(floatingIcon);
			RefreshLabelColor(control);
		}
	}

	private void SetTitle(PlayerBehavior player, string title)
	{
		PlayerFloatingControl control = GetControl(player);
		if (!(control == null))
		{
			control.SetTitle(title);
		}
	}

	private void SetClan(PlayerBehavior player)
	{
		PlayerFloatingControl control = GetControl(player);
		if (!(control == null))
		{
			control.SetClan(player);
			RefreshLabelColor(control);
		}
	}

	private void RefreshLabelColor(PlayerFloatingControl info)
	{
		if (!(info == null) && !(info.Target == null))
		{
			Color nameColor = ((!info.Target.IsLocalPlayer) ? GetPlayerColor(info.Target, Color.white) : Color.white);
			info.SetNameColor(nameColor);
			info.SetClanColor(_clanTextColor);
		}
	}

	public static Color GetPlayerColor([NotNull] PlayerBehavior player, Color defaultColor)
	{
		if (GameSystem<PartySystem>.Instance().IsInParty(player.EntityId))
		{
			return PresetColor.PlayerParty;
		}
		if (CombatSystem.IsHostilePlayer(player))
		{
			return PresetColor.PlayerHostile;
		}
		if (ClanSystem.IsMyClan(player))
		{
			return PresetColor.PlayerClan;
		}
		if (ClanSystem.IsMyClanOrAlliance(player))
		{
			return PresetColor.PlayerAlliance;
		}
		if (GameSystem<SocialSystem>.Instance().IsFriend(player.EntityId))
		{
			return PresetColor.UIFriendlyPink;
		}
		return defaultColor;
	}

	private void Remove(PlayerFloatingControl info)
	{
		if (!(info == null))
		{
			_controls.Remove(info);
			Object.Destroy(info.gameObject);
		}
	}
}
