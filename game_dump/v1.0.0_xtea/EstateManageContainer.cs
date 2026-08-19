using System;
using System.Collections.Generic;
using ClanData;
using Estate;
using Messages;
using Shared.Estate;
using UnityEngine;

public class EstateManageContainer : MonoBehaviour
{
	[SerializeField]
	private KScrollView _licenses;

	private Estate.EstateInfo _estate;

	private bool _isInit;

	public event Action Closed;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_licenses.Nodes.Init(delegate(GameObject o)
			{
				EstateLicenseNode component = o.GetComponent<EstateLicenseNode>();
				component.ExtendViewStateChanged += OnChangeExtendView;
			});
		}
	}

	private void OnDisable()
	{
		Submit();
		_estate = null;
	}

	private void Submit()
	{
		if (_estate == null)
		{
			return;
		}
		ListObjectPool nodes = _licenses.Nodes;
		bool flag = false;
		for (int i = 0; i < nodes.Count; i++)
		{
			EstateLicenseNode component = nodes[i].GetComponent<EstateLicenseNode>();
			if (component.IsChanged)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		switch (_estate.OwnerType)
		{
		case OwnerType.Player:
		{
			License license = default(License);
			license.ForOthers = nodes[0].GetComponent<EstateLicenseNode>().Right;
			license.ForFriends = nodes[1].GetComponent<EstateLicenseNode>().Right;
			GameSystem<EstateSystem>.Instance().SetEstateLicense(_estate.Id, license);
			break;
		}
		case OwnerType.ClanEstate:
		{
			License clanEstateLicense = default(License);
			clanEstateLicense.ForOthers = nodes[0].GetComponent<EstateLicenseNode>().Right;
			Dictionary<int, AccessRights> dictionary = new Dictionary<int, AccessRights>();
			for (int j = 1; j < nodes.Count; j++)
			{
				EstateLicenseNode component2 = nodes[j].GetComponent<EstateLicenseNode>();
				dictionary.Add(component2.ClanRole.Id, component2.Right);
			}
			clanEstateLicense.ForClanMembers = dictionary;
			GameSystem<EstateSystem>.Instance().SetClanEstateLicense(clanEstateLicense);
			break;
		}
		case OwnerType.ClanCapture:
		case OwnerType.System:
			break;
		}
	}

	private void OnChangeExtendView(EstateLicenseNode node)
	{
		_licenses.UpdateLayout(instant: false);
		int index = _licenses.Nodes.IndexOf(((Component)node).gameObject);
		_licenses.MoveToNode(index, instant: false);
	}

	public void Set(Estate.EstateInfo estate)
	{
		Init();
		_estate = estate;
		switch (estate.OwnerType)
		{
		case OwnerType.Player:
			SetPlayerEstate();
			break;
		case OwnerType.ClanEstate:
			SetClanEstate();
			break;
		default:
			Close();
			break;
		}
	}

	private void SetPlayerEstate()
	{
		ListObjectPool nodes = _licenses.Nodes;
		nodes.Clear();
		EstateLicenseNode estateLicenseNode = ((ListObjectPoolBase<GameObject>)nodes).Add<EstateLicenseNode>();
		estateLicenseNode.Set(Estate.LicenseCategory.Other, _estate.License.Others);
		EstateLicenseNode estateLicenseNode2 = ((ListObjectPoolBase<GameObject>)nodes).Add<EstateLicenseNode>();
		AccessRights? friends = _estate.License.Friends;
		estateLicenseNode2.Set(Estate.LicenseCategory.Friend, friends.HasValue ? _estate.License.Friends.Value : AccessRights.None);
		_licenses.ResetPosition();
	}

	private void SetClanEstate()
	{
		ListObjectPool nodes = _licenses.Nodes;
		nodes.Clear();
		EstateLicenseNode estateLicenseNode = ((ListObjectPoolBase<GameObject>)nodes).Add<EstateLicenseNode>();
		estateLicenseNode.Set(Estate.LicenseCategory.Other, _estate.License.Others);
		ClanSystem.GetClanInfo(_estate.Owner, OnEstateOwnerClan);
		_licenses.ResetPosition();
	}

	private void OnEstateOwnerClan(Clan owner)
	{
		if (_estate == null || owner.RoleInfos == null)
		{
			return;
		}
		ListObjectPool nodes = _licenses.Nodes;
		foreach (KeyValuePair<int, MemberRole> roleInfo in owner.RoleInfos)
		{
			MemberRole value = roleInfo.Value;
			EstateLicenseNode estateLicenseNode = ((ListObjectPoolBase<GameObject>)nodes).Add<EstateLicenseNode>();
			estateLicenseNode.Set(value, (_estate.License.ClanMembers != null) ? _estate.License.ClanMembers.Get(value.Id, AccessRights.None) : AccessRights.None);
		}
		_licenses.Reposition();
	}

	private void Close()
	{
		if (this.Closed != null)
		{
			this.Closed();
		}
	}
}
