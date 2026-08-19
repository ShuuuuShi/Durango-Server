using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Clan;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Estate;
using Shared.Player;
using UnityEngine;

namespace Durango.UI;

public class AccessRightsManageGroup : UIBase
{
	private enum FriendTypeOrder
	{
		BestFriend,
		JustFriend,
		Nobody,
		Max
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private KScrollView _tabScrollView;

	[SerializeField]
	private AccessRightsPage _accessRightsPage;

	private int _selectedTabIndex;

	private Clan _clan;

	private EstateLicense _license;

	private Messages.AccessRights _rights;

	private bool _isLoaded;

	private bool _isChanged;

	private Action<EstateLicense> _onChanged;

	private void Start()
	{
		_tabScrollView.Nodes.Init(delegate(GameObject obj)
		{
			AccessRightsTab component = obj.GetComponent<AccessRightsTab>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickTab));
		});
		_rights = default(Messages.AccessRights);
		_rights.ForClanMembers = new Dictionary<int, Shared.Estate.AccessRights>();
		_titleWidget.Object.SetTitle(T._("권한 관리"));
		base.TryClose();
	}

	private void OnClickTab()
	{
		GameObject obj = Selectable.Current.gameObject;
		int num = _tabScrollView.Nodes.IndexOf(obj);
		if (num != -1)
		{
			SelectTab(num);
		}
	}

	public void Open(Shared.Player.FriendType friendType, Action onFailed)
	{
		FriendTypeOrder index = FriendTypeOrder.Nobody;
		switch (friendType)
		{
		case Shared.Player.FriendType.Invalid:
			index = FriendTypeOrder.Nobody;
			break;
		case Shared.Player.FriendType.JustFriend:
			index = FriendTypeOrder.JustFriend;
			break;
		case Shared.Player.FriendType.BestFriend:
			index = FriendTypeOrder.BestFriend;
			break;
		}
		Open(OwnerType.Player, null, onFailed, (int)index);
	}

	public void Open(OwnerType type, Action<EstateLicense> onChanged, Action onFailed = null, int index = 0)
	{
		switch (type)
		{
		default:
			return;
		case OwnerType.ClanWarphole:
			type = OwnerType.ClanEstate;
			break;
		case OwnerType.System:
			return;
		case OwnerType.Player:
		case OwnerType.ClanEstate:
		case OwnerType.PersonalPlayer:
			break;
		}
		EstateSystem.GetEstateLicenses(delegate(EstateLicenses licenses)
		{
			bool flag = true;
			switch (type)
			{
			case OwnerType.PersonalPlayer:
			{
				EstateLicense? personalEstate = licenses.PersonalEstate;
				if (!personalEstate.HasValue)
				{
					flag = false;
				}
				else
				{
					_license = licenses.PersonalEstate.Value;
				}
				break;
			}
			case OwnerType.Player:
			{
				EstateLicense? urbanEstate = licenses.UrbanEstate;
				if (!urbanEstate.HasValue)
				{
					flag = false;
				}
				else
				{
					_license = licenses.UrbanEstate.Value;
				}
				break;
			}
			case OwnerType.ClanEstate:
			{
				EstateLicense? clanEstate = licenses.ClanEstate;
				if (!clanEstate.HasValue)
				{
					flag = false;
				}
				else
				{
					_license = licenses.ClanEstate.Value;
				}
				break;
			}
			default:
				flag = false;
				break;
			}
			if (!flag && onFailed != null)
			{
				onFailed();
			}
			else
			{
				_onChanged = onChanged;
				CopyLicenseRights();
				MakeTabList(index);
				base.Open();
			}
		});
	}

	public override bool Open()
	{
		throw new NotSupportedException();
	}

	protected override bool TryClose()
	{
		CheckCurrentRightChanged();
		if (_isChanged)
		{
			OnChagned();
		}
		return base.TryClose();
	}

	private void OnChagned()
	{
		EstateLicense license = _license;
		Messages.AccessRights rights = _rights;
		EstateSystem.SetEstateLicense(license.EstateId, rights, delegate
		{
			Messages.AccessRights value = ((!license.AccessRights.HasValue) ? default(Messages.AccessRights) : license.AccessRights.Value);
			value.ForFriends = rights.ForFriends;
			value.ForOthers = rights.ForOthers;
			if (value.ForClanMembers != null)
			{
				value.ForClanMembers.Clear();
			}
			if (rights.ForClanMembers != null)
			{
				if (value.ForClanMembers == null)
				{
					value.ForClanMembers = new Dictionary<int, Shared.Estate.AccessRights>();
				}
				foreach (KeyValuePair<int, Shared.Estate.AccessRights> forClanMember in rights.ForClanMembers)
				{
					value.ForClanMembers[forClanMember.Key] = forClanMember.Value;
				}
			}
			license.AccessRights = value;
			if (_onChanged != null)
			{
				_onChanged(license);
			}
		});
	}

	private void CopyLicenseRights()
	{
		_isChanged = false;
		if (_license.AccessRights.HasValue)
		{
			Messages.AccessRights value = _license.AccessRights.Value;
			_rights.ForFriends = value.ForFriends;
			_rights.ForOthers = value.ForOthers;
			_rights.ForClanMembers.Clear();
			if (value.ForClanMembers == null)
			{
				return;
			}
			{
				foreach (KeyValuePair<int, Shared.Estate.AccessRights> forClanMember in value.ForClanMembers)
				{
					_rights.ForClanMembers.Add(forClanMember.Key, forClanMember.Value);
				}
				return;
			}
		}
		_rights.ForFriends = new Dictionary<Shared.Player.FriendType, Shared.Estate.AccessRights>
		{
			{
				Shared.Player.FriendType.JustFriend,
				Shared.Estate.AccessRights.None
			},
			{
				Shared.Player.FriendType.BestFriend,
				Shared.Estate.AccessRights.None
			}
		};
		_rights.ForOthers = Shared.Estate.AccessRights.None;
		_rights.ForClanMembers.Clear();
	}

	private void MakeTabList(int index)
	{
		_selectedTabIndex = index;
		_isLoaded = false;
		switch (_license.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			_MakeTabList();
			break;
		case OwnerType.ClanEstate:
			ClanSystem.GetClanInfo(_license.OwnerId, OnEstateOwnerClan);
			break;
		}
		if (!_isLoaded)
		{
			_tabScrollView.Nodes.Clear();
		}
	}

	private void _MakeTabList()
	{
		_isLoaded = true;
		int i = 0;
		_tabScrollView.Nodes.BeginLoad();
		int[] array = null;
		switch (_license.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
		{
			array = new int[3];
			Dictionary<string, Shared.Player.FriendType> friendEntities = GameSystem<SocialSystem>.Instance().Social.FriendEntities;
			if (friendEntities != null)
			{
				array[0] = friendEntities.Count((KeyValuePair<string, Shared.Player.FriendType> x) => x.Value == Shared.Player.FriendType.BestFriend);
				array[1] = friendEntities.Count((KeyValuePair<string, Shared.Player.FriendType> x) => x.Value == Shared.Player.FriendType.JustFriend);
			}
			break;
		}
		case OwnerType.ClanEstate:
		{
			array = new int[_clan.RoleInfos.Count];
			int num = 0;
			foreach (KeyValuePair<int, MemberRole> roleInfo in _clan.RoleInfos)
			{
				int roleId = roleInfo.Key;
				array[num] = _clan.Members.Count((Durango.Logic.Clan.Member x) => x.RoleId == roleId);
				num++;
			}
			break;
		}
		}
		string nameText;
		Shared.Estate.AccessRights rights;
		bool writable;
		for (; GetAccessRights(_license.Type, i, out nameText, out rights, out writable); i++)
		{
			AccessRightsTab component = _tabScrollView.Nodes.GetNext().GetComponent<AccessRightsTab>();
			string subText = ((array != null && array.Length > i) ? array[i].ToString() : string.Empty);
			component.Set(nameText, subText);
		}
		_tabScrollView.Nodes.EndLoad();
		_tabScrollView.ResetPosition();
		SelectTab(_selectedTabIndex);
	}

	private void OnEstateOwnerClan(Clan clan)
	{
		_clan = clan;
		_MakeTabList();
	}

	private void SelectTab(int index)
	{
		if (_isLoaded)
		{
			if (_selectedTabIndex != index)
			{
				CheckCurrentRightChanged();
			}
			_selectedTabIndex = index;
			for (int i = 0; i < _tabScrollView.Nodes.Count; i++)
			{
				AccessRightsTab component = _tabScrollView.Nodes[i].GetComponent<AccessRightsTab>();
				component.Selected = i == index;
			}
			GetAccessRights(_license.Type, index, out var nameText, out var rights, out var writable);
			_accessRightsPage.Set(nameText, _license.Type, rights, writable);
		}
	}

	private void CheckCurrentRightChanged()
	{
		int selectedTabIndex = _selectedTabIndex;
		Shared.Estate.AccessRights rights = _accessRightsPage.Rights;
		SetAccessRights(_license.Type, selectedTabIndex, rights);
	}

	private bool GetAccessRights(OwnerType owner, int index, out string nameText, out Shared.Estate.AccessRights rights, out bool writable)
	{
		nameText = null;
		rights = Shared.Estate.AccessRights.None;
		writable = true;
		switch (owner)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			switch ((FriendTypeOrder)index)
			{
			case FriendTypeOrder.BestFriend:
				nameText = T._("친한 친구");
				rights = ((_rights.ForFriends != null) ? _rights.ForFriends.Get(Shared.Player.FriendType.BestFriend, Shared.Estate.AccessRights.None) : Shared.Estate.AccessRights.None);
				return true;
			case FriendTypeOrder.JustFriend:
				nameText = T._("친구");
				rights = ((_rights.ForFriends != null) ? _rights.ForFriends.Get(Shared.Player.FriendType.JustFriend, Shared.Estate.AccessRights.None) : Shared.Estate.AccessRights.None);
				return true;
			case FriendTypeOrder.Nobody:
				nameText = T._("외부인");
				rights = _rights.ForOthers;
				return true;
			}
			break;
		case OwnerType.ClanEstate:
			if (index < 0)
			{
				return false;
			}
			if (index < _clan.RoleInfos.Count)
			{
				int num = 0;
				MemberRole role = default(MemberRole);
				foreach (KeyValuePair<int, MemberRole> roleInfo in _clan.RoleInfos)
				{
					if (num == index)
					{
						role = roleInfo.Value;
						break;
					}
					num++;
				}
				nameText = role.GetName();
				if (role.IsSuperuser())
				{
					rights = (Shared.Estate.AccessRights)(-1);
					writable = false;
				}
				else
				{
					rights = _rights.ForClanMembers.Get(role.Id, Shared.Estate.AccessRights.None);
				}
				return true;
			}
			if (index == _clan.RoleInfos.Count)
			{
				nameText = T._("외부인");
				rights = _rights.ForOthers;
				return true;
			}
			return false;
		}
		return false;
	}

	private void SetAccessRights(OwnerType owner, int index, Shared.Estate.AccessRights rights)
	{
		GetAccessRights(_license.Type, index, out var _, out var rights2, out var writable);
		if (!writable || rights2 == rights)
		{
			return;
		}
		switch (owner)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			switch ((FriendTypeOrder)index)
			{
			case FriendTypeOrder.BestFriend:
				_rights.ForFriends[Shared.Player.FriendType.BestFriend] = rights;
				_isChanged = true;
				break;
			case FriendTypeOrder.JustFriend:
				_rights.ForFriends[Shared.Player.FriendType.JustFriend] = rights;
				_isChanged = true;
				break;
			case FriendTypeOrder.Nobody:
				_rights.ForOthers = rights;
				_isChanged = true;
				break;
			}
			break;
		case OwnerType.ClanEstate:
			if (index < 0)
			{
				break;
			}
			if (index < _clan.RoleInfos.Count)
			{
				int num = 0;
				MemberRole memberRole = default(MemberRole);
				foreach (KeyValuePair<int, MemberRole> roleInfo in _clan.RoleInfos)
				{
					if (num == index)
					{
						memberRole = roleInfo.Value;
						break;
					}
					num++;
				}
				_rights.ForClanMembers[memberRole.Id] = rights;
				_isChanged = true;
			}
			else if (index == _clan.RoleInfos.Count)
			{
				_rights.ForOthers = rights;
				_isChanged = true;
			}
			break;
		}
	}
}
