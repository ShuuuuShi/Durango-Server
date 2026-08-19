using System;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Estate;
using UnityEngine;

namespace Durango.UI;

public class EstateMenuWidget : MonoBehaviour
{
	public enum Menu
	{
		Timeline,
		Expand,
		Licenses,
		Remove,
		Extend,
		ClanInfo,
		Bookmark,
		Admission
	}

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private EstateMenuItem _menuBase;

	[SerializeField]
	private UIWidget _disableTitle;

	[SerializeField]
	private UIWidget _estateWarningWidget;

	[SerializeField]
	private UILabel _estateWarningText;

	private ListObjectPool<EstateMenuItem> _menuList = new ListObjectPool<EstateMenuItem>();

	private OwnerType _type;

	private EstateLicense? _licenses;

	private bool _isInit;

	private static string ToString(OwnerType type, Menu menu)
	{
		switch (menu)
		{
		case Menu.Timeline:
			return T._("이력보기");
		case Menu.Expand:
			switch (type)
			{
			case OwnerType.Player:
				return T._("사유지 확장/축소");
			case OwnerType.ClanEstate:
				return T._("부족 영토 확장/축소");
			case OwnerType.PersonalPlayer:
				return T._("개인섬 확장/축소");
			}
			break;
		case Menu.Licenses:
			return T._("권한 조정");
		case Menu.Remove:
			if (type == OwnerType.ClanEstate)
			{
				return T._("부족 영토 권한 포기");
			}
			return T._("사유지 권한 포기");
		case Menu.Extend:
			if (type == OwnerType.ClanEstate)
			{
				return T._("부족 영토 기간 연장");
			}
			return T._("사유지 기간 연장");
		case Menu.ClanInfo:
			return T._("부족 정보");
		case Menu.Bookmark:
			return T._("섬 즐겨찾기");
		case Menu.Admission:
			return T._("섬 출입 권한");
		}
		return null;
	}

	private static string ToIcon(Menu menu)
	{
		return menu switch
		{
			Menu.Timeline => "act_history", 
			Menu.Expand => "action_extend_estate", 
			Menu.Licenses => "action_manage_estate", 
			Menu.Remove => "act_cancel", 
			Menu.Extend => "act_renewal_home", 
			Menu.ClanInfo => "act_send_clanestate", 
			Menu.Bookmark => "mark_island", 
			Menu.Admission => "icon_account_switch", 
			_ => null, 
		};
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_menuList.BaseObject = _menuBase;
			_menuList.UseBase = true;
		}
	}

	public void SetTitle(string title)
	{
		Init();
		_titleLabel.text = title;
	}

	public void SetButtonText(string text, Action onClick)
	{
		Init();
		_confirmButton.Text = text;
		_confirmButton.Clicked = onClick;
	}

	public void MenuLoadBegin(OwnerType type, EstateLicense? license)
	{
		_type = type;
		_licenses = license;
		_scrollView.Widgets.Clear();
		_menuList.BeginLoad();
	}

	public void AddMenu(Menu menu, Action onClick)
	{
		EstateMenuItem next = _menuList.GetNext();
		next.Set(ToIcon(menu), ToString(_type, menu));
		next.Menu = menu;
		next.Clicked = onClick;
		next.Disabled = onClick == null;
		_scrollView.Widgets.Add(next.Widget);
	}

	public void MenuLoadEnd()
	{
		_menuList.EndLoad();
		UIUtility.WidgetsReposition(_menuList, Vector3.down, Vector3.zero);
		EstateMenuItem estateMenuItem = null;
		bool flag = false;
		for (int i = 0; i < _menuList.Count; i++)
		{
			EstateMenuItem estateMenuItem2 = _menuList[i];
			if (estateMenuItem2.Disabled)
			{
				Vector3 position = estateMenuItem2.Widget.GetPosition(0.5f, 1f);
				if (!flag)
				{
					flag = true;
					_disableTitle.SetPosition(position, 0.5f, 1f);
				}
				position.y -= _disableTitle.height;
				estateMenuItem2.Widget.SetPosition(position, 0.5f, 1f);
			}
			else if (estateMenuItem2.Menu == Menu.Extend)
			{
				estateMenuItem = estateMenuItem2;
			}
		}
		if (flag)
		{
			_disableTitle.gameObject.SetActive(value: true);
			_scrollView.Widgets.Add(_disableTitle);
		}
		else
		{
			_disableTitle.gameObject.SetActive(value: false);
		}
		bool flag2 = false;
		if (estateMenuItem != null && _licenses.HasValue)
		{
			EstateLicense value = _licenses.Value;
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			if (value.DepositRunsOutAt.HasValue && predictedServerTime < value.DepositRunsOutAt.Value)
			{
				flag2 = value.DepositRunsOutAt.Value - predictedServerTime < 86400.0;
			}
		}
		ShowEstateWarningText(flag2);
		if (flag2)
		{
			_scrollView.UpdateLayout();
			_scrollView.MoveTo(_scrollView.ContentsLength, instant: false);
		}
		else
		{
			_scrollView.ResetPosition();
		}
	}

	private void ShowEstateWarningText(bool show)
	{
		if (show)
		{
			switch (_type)
			{
			case OwnerType.Player:
				_estateWarningText.text = T._("사유지가 곧 만료됩니다!\n[{0}]으로 보호하세요!", ToString(_type, Menu.Extend));
				break;
			case OwnerType.ClanEstate:
				_estateWarningText.text = T._("부족 영토가 곧 만료됩니다!\n[{0}]으로 보호하세요!", ToString(_type, Menu.Extend));
				break;
			default:
				ShowEstateWarningText(show: false);
				break;
			}
			_estateWarningWidget.height = (int)(0f - _estateWarningText.GetPosition(0f, 0f).y + 20f);
			_estateWarningWidget.gameObject.SetActive(value: true);
			UIUtility.UpdateAnchors(_estateWarningWidget.transform);
			_scrollView.Widgets.Add(_estateWarningWidget);
		}
		else
		{
			_estateWarningWidget.gameObject.SetActive(value: false);
		}
	}
}
