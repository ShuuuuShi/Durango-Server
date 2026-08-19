using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.Logic.Estate;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Estate;
using Shared.Player;
using UnityEngine;

namespace Durango.UI;

public class ArtifactInfoManageRights : UIWidget
{
	public Action Closed;

	public Action<string, int, Action<int>> InventoryAccessEdited;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UIWidget _tooltipBox;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private SelectableButton _moveToManageUIButton;

	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private RectLayout _layout;

	private ArtifactAccess _access;

	private EstateInfo _estate;

	private Clan _ownerClan;

	private string _ownerClanId;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_titleLabel.text = string.Format("{0} [FFFFFF7F][icon=img_loading_unknown_question2][-]", T._("개별 공개 대상"));
			_moveToManageUIButton.Text = T._("사유지 권한 설정");
			UIEventListener uIEventListener = UIEventListener.Get(_titleWidget.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnBack));
			UIEventListener uIEventListener2 = UIEventListener.Get(_tooltipBox.gameObject);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate(GameObject go)
			{
				OnTooltip(go);
			});
			UIPanel componentInParent = GetComponentInParent<UIPanel>();
			_scrollView.Panel.depth = componentInParent.depth + 1;
			_scrollView.Nodes.Init(delegate(GameObject obj)
			{
				obj.GetComponent<ArtifactInfoManageRightsNode>().InventoryAccessEditClicked += OnClickInventoryAccessEdit;
			});
			SelectableButton moveToManageUIButton = _moveToManageUIButton;
			moveToManageUIButton.Clicked = (Action)Delegate.Combine(moveToManageUIButton.Clicked, new Action(OnClickMoveToManageUI));
			_layout.UpdateLayout();
		}
	}

	public bool TryGetChangedAccess(out ArtifactAccess access)
	{
		bool result = false;
		access = _access;
		for (int i = 0; i < _scrollView.Nodes.Count; i++)
		{
			ArtifactInfoManageRightsNode component = _scrollView.Nodes[i].GetComponent<ArtifactInfoManageRightsNode>();
			if (component.IsChanged)
			{
				SetArtifactAccess(ref access, i, component.Value, component.InventoryAccessCount);
				result = true;
			}
		}
		return result;
	}

	public void Set(ArtifactAccess access, [NotNull] EstateInfo estate)
	{
		Init();
		_access = access;
		_estate = estate;
		OwnerType type = estate.License.Type;
		if (type == OwnerType.ClanEstate || type == OwnerType.ClanWarphole)
		{
			_ownerClanId = estate.License.OwnerId;
		}
		else
		{
			_ownerClanId = null;
		}
		_scrollView.Panel.alpha = 0f;
		if (!string.IsNullOrEmpty(_ownerClanId))
		{
			ClanSystem.GetClanInfo(_ownerClanId, delegate(Clan clan)
			{
				if (!string.IsNullOrEmpty(_ownerClanId) && !(_ownerClanId != clan.Id))
				{
					UpdateRightsNodes(clan);
				}
			});
		}
		else
		{
			UpdateRightsNodes(null);
		}
	}

	private void UpdateRightsNodes(Clan ownerClan)
	{
		_ownerClan = ownerClan;
		_scrollView.Nodes.BeginLoad();
		switch (_estate.License.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			AddFriendNode(Shared.Player.FriendType.BestFriend);
			AddFriendNode(Shared.Player.FriendType.JustFriend);
			break;
		case OwnerType.ClanEstate:
		case OwnerType.ClanWarphole:
			if (ownerClan == null)
			{
				break;
			}
			foreach (KeyValuePair<int, MemberRole> roleInfo in ownerClan.RoleInfos)
			{
				MemberRole value = roleInfo.Value;
				if (!value.IsSuperuser())
				{
					AddClanNode(value);
				}
			}
			break;
		}
		AddOtherNode();
		_scrollView.Nodes.EndLoad();
		_scrollView.ResetPosition();
		_scrollView.Panel.alpha = 1f;
	}

	private void AddFriendNode(Shared.Player.FriendType type)
	{
		string text;
		switch (type)
		{
		default:
			return;
		case Shared.Player.FriendType.JustFriend:
			text = T._("친구");
			break;
		case Shared.Player.FriendType.BestFriend:
			text = T._("친한 친구");
			break;
		}
		bool access = _access.Friends != null && _access.Friends.Get(type, defaultValue: false);
		int? inventoryAccessCount = null;
		if (_access.InventoryAccess.HasValue)
		{
			inventoryAccessCount = ((_access.InventoryAccess.Value.Friends != null) ? _access.InventoryAccess.Value.Friends.Get(type, 0) : (-1));
		}
		AddNode(text, access, inventoryAccessCount);
	}

	private void AddClanNode(MemberRole role)
	{
		bool access = _access.CheckClanRole(role.Id);
		int? inventoryAccessCount = null;
		if (_access.InventoryAccess.HasValue)
		{
			inventoryAccessCount = ((_access.InventoryAccess.Value.ClanMembers != null) ? _access.InventoryAccess.Value.ClanMembers.Get(role.Id, 0) : (-1));
		}
		AddNode(role.Name, access, inventoryAccessCount);
	}

	private void AddOtherNode()
	{
		int? inventoryAccessCount = null;
		if (_access.InventoryAccess.HasValue)
		{
			inventoryAccessCount = _access.InventoryAccess.Value.Others;
		}
		AddNode(T._("외부인"), _access.Others, inventoryAccessCount);
	}

	private void AddNode(string text, bool access, int? inventoryAccessCount)
	{
		ArtifactInfoManageRightsNode component = _scrollView.Nodes.GetNext().GetComponent<ArtifactInfoManageRightsNode>();
		component.Set(text, access, inventoryAccessCount);
		component.height = (inventoryAccessCount.HasValue ? 100 : 70);
	}

	private void SetArtifactAccess(ref ArtifactAccess access, int index, bool rights, int? inventoryAccess)
	{
		switch (_estate.License.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			switch (index)
			{
			case 0:
				if (access.Friends == null)
				{
					access.Friends = new Dictionary<Shared.Player.FriendType, bool>(default(FriendTypeComparer));
				}
				access.Friends[Shared.Player.FriendType.BestFriend] = rights;
				if (access.InventoryAccess.HasValue && inventoryAccess.HasValue)
				{
					access.InventoryAccess = access.InventoryAccess.Value.SetFriendAccessCount(Shared.Player.FriendType.BestFriend, inventoryAccess.Value);
				}
				break;
			case 1:
				if (access.Friends == null)
				{
					access.Friends = new Dictionary<Shared.Player.FriendType, bool>(default(FriendTypeComparer));
				}
				access.Friends[Shared.Player.FriendType.JustFriend] = rights;
				if (access.InventoryAccess.HasValue && inventoryAccess.HasValue)
				{
					access.InventoryAccess = access.InventoryAccess.Value.SetFriendAccessCount(Shared.Player.FriendType.JustFriend, inventoryAccess.Value);
				}
				break;
			case 2:
				access.Others = rights;
				if (access.InventoryAccess.HasValue && inventoryAccess.HasValue)
				{
					access.InventoryAccess = access.InventoryAccess.Value.SetOtherAccessCount(inventoryAccess.Value);
				}
				break;
			}
			break;
		case OwnerType.ClanEstate:
		case OwnerType.ClanWarphole:
		{
			int num = 0;
			if (_ownerClan != null)
			{
				foreach (KeyValuePair<int, MemberRole> roleInfo in _ownerClan.RoleInfos)
				{
					if (roleInfo.Value.IsSuperuser())
					{
						continue;
					}
					if (num == index)
					{
						if (access.ClanMembers == null)
						{
							access.ClanMembers = new Dictionary<int, bool>();
						}
						access.ClanMembers[roleInfo.Key] = rights;
						if (access.InventoryAccess.HasValue && inventoryAccess.HasValue)
						{
							access.InventoryAccess = access.InventoryAccess.Value.SetClanRoleAccessCount(roleInfo.Key, inventoryAccess.Value);
						}
						return;
					}
					num++;
				}
			}
			if (num == index)
			{
				access.Others = rights;
				if (access.InventoryAccess.HasValue && inventoryAccess.HasValue)
				{
					access.InventoryAccess = access.InventoryAccess.Value.SetOtherAccessCount(inventoryAccess.Value);
				}
			}
			break;
		}
		case OwnerType.System:
			break;
		}
	}

	private void OnClickInventoryAccessEdit(ArtifactInfoManageRightsNode node)
	{
		InventoryAccess? inventoryAccess = _access.InventoryAccess;
		if (inventoryAccess.HasValue && node.InventoryAccessCount.HasValue && node.Value && InventoryAccessEdited != null)
		{
			InventoryAccessEdited(node.Text, node.InventoryAccessCount.Value, delegate(int value)
			{
				node.ChangeInventoryAccessCount(value);
			});
		}
	}

	private void OnBack(GameObject obj)
	{
		if (Closed != null)
		{
			Closed();
		}
	}

	private WidgetTooltipControl OnTooltip(GameObject obj)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.AutoPosition = false;
		widgetTooltipControl.Set(null, T._("공개 대상에 포함되지 않은 사람은 <em>사유지 권한 설정에서 지정한 권한</em>을 적용받게 됩니다."), 300);
		widgetTooltipControl.Show(20f);
		widgetTooltipControl.Widget.SetPosition(widgetTooltipControl.transform.parent.InverseTransformPoint(_tooltipBox.worldCorners[2]) + Vector3.up * 10f, 1f, 0f);
		widgetTooltipControl.HideArrow();
		widgetTooltipControl.IntoSafeArea();
		return widgetTooltipControl;
	}

	private void OnClickMoveToManageUI()
	{
		UIManager.FindScript<AccessRightsManageGroup>().Open(_estate.License.Type, null);
	}
}
