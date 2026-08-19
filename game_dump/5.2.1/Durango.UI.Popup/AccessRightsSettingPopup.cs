using System;
using System.Text;
using Durango.Logic.Estate;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Estate;
using Shared.Player;
using UnityEngine;

namespace Durango.UI.Popup;

public class AccessRightsSettingPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private KScrollView _friendTypeList;

	[SerializeField]
	private UILabel _captionLabel;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private RectLayout _layout;

	private string _friendEntityId;

	private Shared.Player.FriendType _currentFriendType;

	public override bool DragLock => true;

	protected override void Start()
	{
		base.Start();
		_titleLabel.text = T._("친구 단계 설정");
		_captionLabel.text = T._("설정한 단계에 따라 친구가 사유지에서 할 수 있는 일이 달라집니다.\n어느 단계로 설정했는지 친구는 알 수 없습니다.");
		_confirmButton.Text = T._("확인");
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(ConfirmButton_Clicked));
		_friendTypeList.Nodes.Init(delegate(GameObject obj)
		{
			SelectableWidget component = obj.GetComponent<SelectableWidget>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickTab));
		});
		_layout.UpdateLayout();
	}

	private void OnClickTab()
	{
		GameObject obj = Selectable.Current.gameObject;
		int num = _friendTypeList.Nodes.IndexOf(obj);
		if (num != -1)
		{
			SelectFriendType(NodeIndexToFriendType(num));
		}
	}

	private void SelectFriendType(Shared.Player.FriendType friendType)
	{
		_currentFriendType = friendType;
		int num = FriendTypeToNodeIndex(friendType);
		for (int i = 0; i < _friendTypeList.Nodes.Count; i++)
		{
			_friendTypeList.Nodes[i].GetComponent<SelectableWidget>().Selected = i == num;
		}
	}

	public void Set(string friendEntityId)
	{
		_friendEntityId = friendEntityId;
	}

	private static int FriendTypeToNodeIndex(Shared.Player.FriendType friendType)
	{
		return (friendType != Shared.Player.FriendType.BestFriend) ? 1 : 0;
	}

	private static Shared.Player.FriendType NodeIndexToFriendType(int nodeIndex)
	{
		if (nodeIndex != 0)
		{
			return Shared.Player.FriendType.JustFriend;
		}
		return Shared.Player.FriendType.BestFriend;
	}

	protected override void FillData()
	{
		EstateSystem.GetEstateLicenses(delegate(EstateLicenses licenses)
		{
			string[] array = new string[2]
			{
				T._("친구"),
				T._("친한 친구")
			};
			_friendTypeList.Nodes.Set(2);
			for (int num = 1; num >= 0; num--)
			{
				Shared.Player.FriendType friendType = (Shared.Player.FriendType)num;
				Shared.Estate.AccessRights accessRights = Shared.Estate.AccessRights.None;
				if (licenses.PersonalEstate.HasValue && licenses.PersonalEstate.Value.AccessRights.HasValue)
				{
					accessRights = licenses.PersonalEstate.Value.AccessRights.Value.ForFriends.Get(friendType, Shared.Estate.AccessRights.None);
				}
				else if (licenses.UrbanEstate.HasValue && licenses.UrbanEstate.Value.AccessRights.HasValue)
				{
					accessRights = licenses.UrbanEstate.Value.AccessRights.Value.ForFriends.Get(friendType, Shared.Estate.AccessRights.None);
				}
				string title = array[num];
				string description = MakeDescription(accessRights);
				int index = FriendTypeToNodeIndex(friendType);
				_friendTypeList.Nodes[index].GetComponent<FriendTypeWidget>().Set(title, description, delegate
				{
					Hide();
					AccessRightsManageGroup accessRightsManageGroup = UIManager.FindScript<AccessRightsManageGroup>();
					string warningMessage = T._("사유지를 선언한 후에 설정할 수 있습니다.");
					accessRightsManageGroup.Open(friendType, delegate
					{
						UIManager.SystemMsg("NoPrivateEstate", warningMessage);
					});
				});
			}
			_friendTypeList.ResetPosition();
			SelectFriendType(GameSystem<SocialSystem>.Instance().GetFriendly(_friendEntityId));
		});
	}

	private string MakeDescription(Shared.Estate.AccessRights accessRights)
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		for (int num = 1; num <= 32; num *= 2)
		{
			Shared.Estate.AccessRights accessRights2 = (Shared.Estate.AccessRights)num;
			if ((accessRights & accessRights2) != 0)
			{
				if (value.Length != 0)
				{
					value.Append(", ");
				}
				value.Append(Util.GetName(OwnerType.Player, accessRights2));
			}
		}
		string text = value.ToString();
		if (string.IsNullOrEmpty(text))
		{
			text = T._("사용 권한 없음");
		}
		return text;
	}

	private void ConfirmButton_Clicked()
	{
		GameSystem<SocialSystem>.Instance().ChangeFriendType(_friendEntityId, _currentFriendType);
		Hide();
	}

	protected override void OnTryConfirmOnModal()
	{
		ConfirmButton_Clicked();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}
}
