using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.Logic.Estate;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Estate;
using Shared.Player;
using UnityEngine;

namespace Durango.UI;

public class ArtifactInfoRights : UIWidget
{
	public Action ManageButtonClicked;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private GameObject _manageButton;

	[SerializeField]
	private UIWidget _rightsWidget;

	[SerializeField]
	private UILabel _rightsLabel;

	[SerializeField]
	private KeyValueLabel _inventoryAccessWidget;

	[SerializeField]
	private RectLayout _layout;

	private readonly List<string> _hasPermissionList = new List<string>();

	private ArtifactInfoGroup _parent;

	private int _settingFrame;

	private ArtifactAccess _access;

	private EstateInfo _estate;

	private bool _ownerClanLoaded;

	private Clan _ownerClan;

	private bool _friendTypeLoaded;

	private Shared.Player.FriendType _friendType;

	private bool _isClickableInventoryAccessTooltip;

	private bool _secured;

	private uint _sequence;

	private bool _isInit;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_titleLabel.text = T._("공개 대상");
		_parent = UIUtility.FindComponentInParent<ArtifactInfoGroup>(base.gameObject);
		UIEventListener uIEventListener = UIEventListener.Get(_inventoryAccessWidget.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnTooltipInventoryAccessHelp));
		UIEventListener uIEventListener2 = UIEventListener.Get(_manageButton);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (ManageButtonClicked != null)
			{
				ManageButtonClicked();
			}
		});
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			_sequence = 0u;
		}
	}

	public void Set(ArtifactAccess access, EstateInfo estate, bool secured = false)
	{
		Init();
		_access = access;
		_estate = estate;
		_secured = secured;
		_settingFrame = Time.frameCount;
		bool flag = EstateSystem.IsAdmin(estate);
		_manageButton.gameObject.SetActive(flag && !_secured);
		uint seq = ++_sequence;
		_friendTypeLoaded = true;
		_friendType = Shared.Player.FriendType.Invalid;
		_ownerClanLoaded = true;
		_ownerClan = null;
		switch (estate.License.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			if (flag || !GameSystem<SocialSystem>.Instance().IsFriend(estate.License.OwnerId))
			{
				break;
			}
			_friendTypeLoaded = false;
			SocialSystem.GetMyFriendType(estate.License.OwnerId, delegate(Messages.FriendType type)
			{
				if (seq == _sequence)
				{
					_friendTypeLoaded = true;
					_friendType = type._FriendType;
					RefreshHasPermissionList();
				}
			});
			break;
		case OwnerType.ClanEstate:
		case OwnerType.ClanWarphole:
			_ownerClanLoaded = false;
			ClanSystem.GetClanInfo(estate.License.OwnerId, delegate(Clan clan)
			{
				if (seq == _sequence)
				{
					_ownerClanLoaded = true;
					_ownerClan = clan;
					RefreshHasPermissionList();
				}
			});
			break;
		}
		RefreshHasPermissionList();
	}

	private void RefreshHasPermissionList()
	{
		_hasPermissionList.Clear();
		if (_secured)
		{
			_rightsLabel.text = T._("주인만 사용 가능");
		}
		else
		{
			if (_access.Friends != null)
			{
				if (_access.Friends.Get(Shared.Player.FriendType.BestFriend, defaultValue: false))
				{
					_hasPermissionList.Add(T._("친한 친구"));
				}
				if (_access.Friends.Get(Shared.Player.FriendType.JustFriend, defaultValue: false))
				{
					_hasPermissionList.Add(T._("친구"));
				}
			}
			if (_access.ClanMembers != null && _ownerClanLoaded && _ownerClan != null)
			{
				foreach (KeyValuePair<int, bool> clanMember in _access.ClanMembers)
				{
					if (clanMember.Value)
					{
						if (_ownerClan.TryGetRole(clanMember.Key, out var role))
						{
							_hasPermissionList.Add(role.GetName());
						}
						else
						{
							_hasPermissionList.Add(clanMember.Key.ToString());
						}
					}
				}
			}
			if (_access.Others)
			{
				_hasPermissionList.Add(T._("외부인"));
			}
			if (_hasPermissionList.Count > 0)
			{
				_rightsLabel.text = T._("<em>{0:l:{}|, }</em> 에게 공개", _hasPermissionList);
			}
			else
			{
				_rightsLabel.text = T._("사유지 권한에 따름");
			}
		}
		_rightsWidget.height = _rightsLabel.height + 20;
		int? num = null;
		if (_access.InventoryAccess.HasValue && !EstateSystem.IsAdmin(_estate))
		{
			switch (_estate.License.Type)
			{
			case OwnerType.Player:
			case OwnerType.PersonalPlayer:
				if (_friendTypeLoaded)
				{
					num = ((_friendType == Shared.Player.FriendType.Invalid) ? new int?(_access.InventoryAccess.Value.Others) : new int?((_access.InventoryAccess.Value.Friends != null) ? _access.InventoryAccess.Value.Friends.Get(_friendType, 0) : 0));
				}
				break;
			case OwnerType.ClanEstate:
			case OwnerType.ClanWarphole:
				num = ((!ClanSystem.IsMyClan(_estate.License.OwnerId)) ? new int?(_access.InventoryAccess.Value.Others) : new int?((_access.InventoryAccess.Value.ClanMembers != null) ? _access.InventoryAccess.Value.ClanMembers.Get(PlayerBehavior.LocalPlayer.Clan.RoleId, 0) : 0));
				break;
			}
		}
		_isClickableInventoryAccessTooltip = false;
		if (num.HasValue)
		{
			_inventoryAccessWidget.gameObject.SetActive(value: true);
			switch (num.Value)
			{
			case -1:
				_inventoryAccessWidget.Set(T._("꺼낼 수 있는 개수"), T._("<em>무제한</em>"));
				break;
			case 0:
				_inventoryAccessWidget.Set(T._("꺼낼 수 있는 개수"), T._("0"));
				break;
			default:
			{
				int takenCount = _access.InventoryAccess.Value.GetTakenCount(GameManager.PlayerId, Connections.Frontend.GetPredictedServerTime());
				int num2 = Mathf.Max(0, num.Value - takenCount);
				_inventoryAccessWidget.Set(string.Format("{0} [icon=img_loading_unknown_question2]", T._("꺼낼 수 있는 개수")), string.Format((num2 <= 0) ? "{0}/{1}" : "<em>{0}</em>/{1}", num2, num.Value));
				_isClickableInventoryAccessTooltip = true;
				break;
			}
			}
		}
		else
		{
			_inventoryAccessWidget.gameObject.SetActive(value: false);
		}
		_layout.UpdateLayout();
		if (Time.frameCount != _settingFrame)
		{
			_parent.RefreshLayout();
		}
	}

	private void OnTooltipInventoryAccessHelp(GameObject obj)
	{
		if (!_isClickableInventoryAccessTooltip)
		{
			return;
		}
		InventoryAccess? inventoryAccess = _access.InventoryAccess;
		if (!inventoryAccess.HasValue || !_access.InventoryAccess.Value.TakenCountsValidUntil.HasValue)
		{
			return;
		}
		double num = _access.InventoryAccess.Value.TakenCountsValidUntil.Value;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (num < predictedServerTime)
		{
			double num2 = OptionSystem.GetInventoryAccessRefreshPeriod() * 60 * 60;
			if (!(num2 > 0.0))
			{
				return;
			}
			num += Math.Ceiling((predictedServerTime - num) / num2) * num2;
		}
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, T._("{0} 후에 초기화됩니다.", TimedeltaFormatter.Format(num - predictedServerTime, 2, "min")));
		widgetTooltipControl.Show(10f);
	}
}
