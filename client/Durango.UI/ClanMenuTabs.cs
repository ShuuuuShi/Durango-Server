using System;
using Durango.Logic.Clan;
using Durango.Logic.Notification;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Clan;
using UnityEngine;

namespace Durango.UI;

public class ClanMenuTabs : UIWidget
{
	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	private IconTabList _tabList;

	private ClanGroup.ClanMenus[] _menus;

	private bool _isInit;

	public event Action<ClanGroup.ClanMenus> Selected;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tabList = _tabLinker.Object.GetComponent<IconTabList>();
			_tabList.Clicked += OnTabSelected;
		}
	}

	private void OnTabSelected(int index)
	{
		if (index >= 0 && index < KUtility.GetSize(_menus) && this.Selected != null)
		{
			this.Selected(_menus[index]);
		}
	}

	public void Set(ClanGroup.ClanMenus[] menus)
	{
		Init();
		_menus = menus;
		_tabList.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(menus); i < size; i++)
		{
			_tabList.Add(IconMap.Get(menus[i]), menus[i].GetName());
		}
		_tabList.EndLoad();
		UpdateNotification();
	}

	public void SelectMenu(ClanGroup.ClanMenus menu)
	{
		Init();
		int index = ((_menus != null) ? _menus.IndexOf(menu) : (-1));
		_tabList.Select(index);
	}

	private void UpdateNotification()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_menus); i < size; i++)
		{
			if (_menus[i] == ClanGroup.ClanMenus.Members)
			{
				Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
				Messages.Member clan = PlayerBehavior.LocalPlayer.Clan;
				bool on = false;
				if (playerClan != null && !string.IsNullOrEmpty(clan.ClanId) && playerClan.TryGetRole(clan.RoleId, out var role) && (role.GetPermissions() & Permissions.ApproveMember) != 0)
				{
					on = KUtility.GetSize(playerClan.Appliers) > 0;
				}
				_tabList.SetNotification(i, on, Durango.Logic.Notification.Type.Normal);
			}
		}
	}
}
