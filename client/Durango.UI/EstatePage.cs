using System;
using Durango.Logic.Clan;
using Durango.Logic.Estate;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Estate;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class EstatePage : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private EmptyPersonalEstatePage _emptyPersonalEstatePage;

	[SerializeField]
	private EmptyUrbanEstatePage _emptyUrbanEstatePage;

	[SerializeField]
	private EmptyClanEstatePage _emptyClanEstatePage;

	[SerializeField]
	private EmptyBaseEstatePage _emptyBaseEstatePage;

	[SerializeField]
	private UIWidget _estatePage;

	[SerializeField]
	private NestedPrefabLinker _pioneerInfoLinker;

	[SerializeField]
	private EstateInfoWidget _infoWidget;

	[SerializeField]
	private EstateMenuWidget _menuWidget;

	[SerializeField]
	private RectLayoutComponent _layout;

	private PioneerInfoWidget _pioneerInfoWidget;

	private OwnerType _type;

	private EstateLicense? _license;

	private int _largestSize;

	private EstateGroup _parent;

	public Transform DelcareEstateButton => _type switch
	{
		OwnerType.Player => _emptyUrbanEstatePage.DeclareButton, 
		OwnerType.ClanEstate => _emptyClanEstatePage.DeclareButton, 
		OwnerType.PersonalPlayer => _emptyPersonalEstatePage.DeclareButton, 
		_ => null, 
	};

	void IUIInitializable.Init()
	{
		_parent = GetComponentInParent<EstateGroup>();
		_emptyPersonalEstatePage.DeclareButtonClicked += delegate
		{
			EstateGridGroup estateGridGroup3 = UIManager.FindScript<EstateGridGroup>();
			estateGridGroup3.Open(OwnerType.PersonalPlayer, _largestSize);
		};
		_emptyUrbanEstatePage.DeclareButtonClicked += delegate
		{
			EstateGridGroup estateGridGroup2 = UIManager.FindScript<EstateGridGroup>();
			estateGridGroup2.Open(OwnerType.Player, _largestSize);
		};
		_emptyClanEstatePage.DeclareButtonClicked += delegate
		{
			EstateGridGroup estateGridGroup = UIManager.FindScript<EstateGridGroup>();
			estateGridGroup.Open(OwnerType.ClanEstate, _largestSize);
		};
		_pioneerInfoWidget = _pioneerInfoLinker.Object.GetComponent<PioneerInfoWidget>();
		_pioneerInfoWidget.Clicked += delegate
		{
			PioneerGradeRewardsPopup pioneerGradeRewardsPopup = UIManager.Popup.FindTooltip<PioneerGradeRewardsPopup>();
			pioneerGradeRewardsPopup.Show();
		};
	}

	public void Show(OwnerType type, EstateLicense? info, int largestSize, bool moveToMain, EntityTile? waprholeTile = null)
	{
		_type = type;
		_license = info;
		_largestSize = largestSize;
		if (moveToMain)
		{
			ShowMainPage();
		}
		Refresh(info, largestSize, waprholeTile);
		base.gameObject.SetActive(value: true);
	}

	private void Refresh(EstateLicense? info, int largestSize, EntityTile? warpholeTile)
	{
		_license = info;
		_largestSize = largestSize;
		_pioneerInfoLinker.gameObject.SetActive(_type == OwnerType.PersonalPlayer);
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		if (!info.HasValue)
		{
			return;
		}
		_infoWidget.Set(info.Value, _largestSize, warpholeTile);
		switch (_type)
		{
		case OwnerType.PersonalPlayer:
			_menuWidget.SetTitle(T._("개인섬 메뉴"));
			_menuWidget.SetButtonText(T._("섬으로 귀환"), delegate
			{
				ReturnToEstate(OwnerType.PersonalPlayer);
			});
			_menuWidget.MenuLoadBegin(_type, _license);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Timeline, EstateTimeLine);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Expand, EstateExpand);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Licenses, EstateLicenses);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Bookmark, EstateBookmark);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Admission, PersonalRegionAdmission);
			_menuWidget.MenuLoadEnd();
			_pioneerInfoWidget.Refresh();
			break;
		case OwnerType.Player:
			_menuWidget.SetTitle(T._("사유지 메뉴"));
			_menuWidget.SetButtonText(T._("사유지로 귀환"), delegate
			{
				ReturnToEstate(OwnerType.Player);
			});
			_menuWidget.MenuLoadBegin(_type, _license);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Timeline, EstateTimeLine);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Expand, EstateExpand);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Licenses, EstateLicenses);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Remove, EstateRemove);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Extend, EstateExtend);
			_menuWidget.MenuLoadEnd();
			break;
		case OwnerType.ClanEstate:
		{
			bool flag = false;
			if (info.Value.AccessRights.HasValue && info.Value.AccessRights.Value.ForClanMembers != null)
			{
				Messages.Member clan = PlayerBehavior.LocalPlayer.Clan;
				flag = !string.IsNullOrEmpty(clan.ClanId) && clan.RoleId == 0;
			}
			_menuWidget.SetTitle(T._("부족 메뉴"));
			_menuWidget.SetButtonText(T._("부족 영토로 귀환"), ReturnToClanEstate);
			_menuWidget.MenuLoadBegin(_type, _license);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.ClanInfo, EstateClanInfo);
			_menuWidget.AddMenu(EstateMenuWidget.Menu.Timeline, EstateTimeLine);
			if (flag)
			{
				_menuWidget.AddMenu(EstateMenuWidget.Menu.Expand, EstateExpand);
				_menuWidget.AddMenu(EstateMenuWidget.Menu.Licenses, EstateLicenses);
				_menuWidget.AddMenu(EstateMenuWidget.Menu.Remove, EstateRemove);
				_menuWidget.AddMenu(EstateMenuWidget.Menu.Extend, EstateExtend);
			}
			else
			{
				_menuWidget.AddMenu(EstateMenuWidget.Menu.Expand, null);
				_menuWidget.AddMenu(EstateMenuWidget.Menu.Licenses, null);
				_menuWidget.AddMenu(EstateMenuWidget.Menu.Remove, null);
				_menuWidget.AddMenu(EstateMenuWidget.Menu.Extend, null);
			}
			_menuWidget.MenuLoadEnd();
			break;
		}
		}
	}

	private void ShowMainPage()
	{
		if (_license.HasValue)
		{
			_estatePage.gameObject.SetActive(value: true);
			_emptyPersonalEstatePage.gameObject.SetActive(value: false);
			_emptyUrbanEstatePage.gameObject.SetActive(value: false);
			_emptyClanEstatePage.gameObject.SetActive(value: false);
			_emptyBaseEstatePage.gameObject.SetActive(value: false);
			return;
		}
		_estatePage.gameObject.SetActive(value: false);
		_emptyPersonalEstatePage.gameObject.SetActive(_type == OwnerType.PersonalPlayer);
		_emptyUrbanEstatePage.gameObject.SetActive(_type == OwnerType.Player);
		_emptyClanEstatePage.gameObject.SetActive(_type == OwnerType.ClanEstate);
		_emptyBaseEstatePage.gameObject.SetActive(_type == OwnerType.ClanWarphole);
		switch (_type)
		{
		case OwnerType.PersonalPlayer:
			_emptyPersonalEstatePage.Refresh();
			break;
		case OwnerType.Player:
			_emptyUrbanEstatePage.Refresh();
			break;
		case OwnerType.ClanEstate:
			_emptyClanEstatePage.Refresh();
			break;
		case OwnerType.ClanWarphole:
			_emptyBaseEstatePage.Refresh();
			break;
		case OwnerType.System:
			break;
		}
	}

	private static void ReturnToEstate(OwnerType type)
	{
		UIBase.CloseAllUI();
		EstateSystem.ReturnToEstate(type);
	}

	private static void ReturnToClanEstate()
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan != null)
		{
			UIBase.CloseAllUI();
			EstateSystem.VisitEstate(OwnerType.ClanEstate, playerClan.Id);
		}
	}

	private void EstateTimeLine()
	{
		if (_license.HasValue)
		{
			switch (_license.Value.Type)
			{
			case OwnerType.System:
			case OwnerType.ClanWarphole:
				break;
			case OwnerType.Player:
			case OwnerType.ClanEstate:
			case OwnerType.PersonalPlayer:
				UIManager.FindScript<TimelineLogGroup>().OpenForEstate(_license.Value);
				break;
			}
		}
	}

	private void EstateExpand()
	{
		if (_license.HasValue)
		{
			EstateInfo estateInfo = EstateSystem.GetEstateInfo(PlayerBehavior.LocalPlayer.CurrentTile);
			if (estateInfo == null || estateInfo.Id != _license.Value.EstateId)
			{
				UIManager.SystemMsg(T._("사유지에 있어야 합니다"));
				return;
			}
			EstateGridGroup estateGridGroup = UIManager.FindScript<EstateGridGroup>();
			estateGridGroup.Open(estateInfo, _largestSize);
		}
	}

	private void EstateLicenses()
	{
		if (_license.HasValue)
		{
			UIManager.FindScript<AccessRightsManageGroup>().Open(_license.Value.Type, delegate(EstateLicense license)
			{
				_parent.NotifyEstateLicenseChanged(license.Type, license);
			});
		}
	}

	private void EstateRemove()
	{
		EstateLicense? license = _license;
		if (!license.HasValue)
		{
			return;
		}
		EstateLicense estate = _license.Value;
		MessageBox messageBox = UIManager.MessageBox;
		string mainText = string.Empty;
		string subText = string.Empty;
		switch (estate.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			mainText = T._("모든 사유지의 권한을 포기하시겠습니까?");
			subText = string.Format("{0}\n{1}\n{2}", T._("<alert_icon/> 사유지 포기 후 다시 확장할 때 비용이 들지 않습니다."), T._("<alert_icon/> 사유지의 예치금은 소지금으로 돌려받습니다."), T._("<alert><alert_icon/> 즉시 소유권을 잃게 되므로 건물을 미리 포장하는 것이 좋습니다.</alert>"));
			break;
		case OwnerType.ClanEstate:
			mainText = T._("모든 부족 영토의 권한을 포기하시겠습니까?");
			subText = string.Format("{0}\n{1}\n{2}", T._("<alert_icon/> 부족 영토 포기 후 다시 확장할 때 비용이 들지 않습니다."), T._("<alert_icon/> 부족 영토의 예치금은 부족 자금으로 돌려받습니다."), T._("<alert><alert_icon/> 즉시 소유권을 잃게 되므로 건물을 미리 포장하는 것이 좋습니다.</alert>"));
			break;
		}
		Action shrinkAction = delegate
		{
			EstateSystem.RemoveEstate(estate.EstateId);
			UIBase.CloseAllUI();
		};
		Action<bool> onOkCancel = null;
		switch (estate.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			onOkCancel = delegate(bool ok)
			{
				if (ok)
				{
					UIManager.MessageBox.Show(T._("모든 건물의 소유권을 잃습니다. 정말 포기하시겠습니까?"), string.Format("{0}\n{1}", T._("<alert_icon/> 사유지 포기 후 다시 확장할 때 비용이 들지 않습니다."), T._("<alert><alert_icon/> 즉시 소유권을 잃게 되므로 건물을 미리 포장하는 것이 좋습니다.</alert>")), delegate(bool ok2)
					{
						if (ok2)
						{
							shrinkAction();
						}
					});
				}
			};
			break;
		case OwnerType.ClanEstate:
			onOkCancel = delegate(bool ok)
			{
				if (ok)
				{
					UIManager.MessageBox.Show(T._("모든 건물의 소유권을 잃습니다. 정말 포기하시겠습니까?"), string.Format("{0}\n{1}", T._("<alert_icon/> 부족 영토 포기 후 다시 확장할 때 비용이 들지 않습니다."), T._("<alert><alert_icon/> 즉시 소유권을 잃게 되므로 건물을 미리 포장하는 것이 좋습니다.</alert>")), delegate(bool ok2)
					{
						if (ok2)
						{
							shrinkAction();
						}
					});
				}
			};
			break;
		}
		messageBox.Show(mainText, subText, onOkCancel);
	}

	private void EstateExtend()
	{
		EstateLicense? license2 = _license;
		if (!license2.HasValue)
		{
			return;
		}
		EstateGroup.ShowExtendEstatePopup(_license.Value, delegate
		{
			EstateLicense? license3 = _license;
			if (license3.HasValue)
			{
				long extendingCost = Singleton<CostsYaml>.Instance.Estate.GetExtendingCost(_license.Value.Type, _license.Value.Size);
				EstateSystem.ExtendEstate(_license.Value.EstateId, extendingCost * _license.Value.Size * 7, delegate(EstateLicense license)
				{
					_parent.NotifyEstateLicenseChanged(license.Type, license);
				});
			}
		});
	}

	private static void EstateClanInfo()
	{
		UIManager.FindScript<ClanGroup>().Open();
	}

	private static void PersonalRegionAdmission()
	{
		EstateSystem.GetPersonalRegionInfo(delegate(PersonalRegionInfo info)
		{
			Messages.PersonalRegion? personalRegion = info.PersonalRegion;
			if (personalRegion.HasValue)
			{
				PersonalRegionAdmissionPopup personalRegionAdmissionPopup = UIManager.Popup.Tooltip<PersonalRegionAdmissionPopup>();
				personalRegionAdmissionPopup.Set(info.PersonalRegion.Value.AdmissionCategories);
				personalRegionAdmissionPopup.Show();
			}
		});
	}

	private void EstateBookmark()
	{
		_parent.ShowFavoriteIslandsPage();
	}
}
