using System;
using System.Collections.Generic;
using Durango.Logic.Estate;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using InteractionData;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Estate;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Estate")]
public class EstateGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private EstateTabWidget _tabWidget;

	[SerializeField]
	private UIWidget _mainContainer;

	[SerializeField]
	private EstatePage _estatePage;

	[SerializeField]
	private ClanBasePage _basePage;

	[SerializeField]
	private RecommendRegionPage _recommendRegionPage;

	[SerializeField]
	private FavoriteIslandsPage _favoriteIslandsPage;

	[SerializeField]
	private SelectPersonalRegion _selectPersonalRegion;

	private bool _loading;

	private EstateLicenses _licenses;

	private EstateTabWidget.Menu? _forceSelectMenu;

	private float _nextUpdateAt;

	private GameObject[] _pages;

	private Artifact _airBalloon;

	public Transform DelcareEstateButton => _estatePage.DelcareEstateButton;

	public EstateTabWidget.Menu SelectedMenu { get; private set; }

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Default;
		_titleWidget.Object.SetTitle(T._("사유지"));
		_tabWidget.TabClicked += OnTabClick;
		GameSystem<InteractionSystem>.Instance().RegisterContextActionFinder(OnContextAction);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.EstateMenu, delegate
		{
			Open();
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.DepartTutorialAirballoon, delegate(InteractionObject target)
		{
			Artifact artifact = target.GetTargetComponent<Artifact>();
			if (!(artifact == null))
			{
				EstateSystem.GetPersonalRegionInfo(delegate(PersonalRegionInfo info)
				{
					Messages.PersonalRegion? personalRegion = info.PersonalRegion;
					if (personalRegion.HasValue)
					{
						AirBalloonLeaving(artifact);
					}
					else
					{
						_airBalloon = artifact;
						Open(EstateTabWidget.Menu.Personal);
					}
				});
			}
		});
		_selectPersonalRegion.Selected += delegate(string regionId)
		{
			UIManager.MessageBox.Show(T._("선택한 개인섬으로 이동합니다."), T._("<alert_icon/> 한 번 선택한 지형은 변경할 수 없습니다.\n\n새로 만들어진 개인섬에는 외부인의 방문이 허용됩니다.\n그러나 외부인에게는 어떤 권한도 허락되지 않으므로 구경만 가능합니다."), delegate(bool ok)
			{
				if (ok)
				{
					Connections.Frontend.Send(new RecommendPersonalRegion
					{
						TemplateId = regionId
					}).On<Messages.PersonalRegion>(delegate
					{
						UIBase.CloseAllUI();
						if (_airBalloon != null)
						{
							AirBalloonLeaving(_airBalloon);
							_airBalloon = null;
						}
						else
						{
							EstateSystem.ReturnToEstate(OwnerType.PersonalPlayer);
						}
					});
				}
			});
		};
		UIManager.OnLoadingCurtainHidden(OnLoadingCurtainHidden);
		_pages = new GameObject[4] { _estatePage.gameObject, _basePage.gameObject, _recommendRegionPage.gameObject, _favoriteIslandsPage.gameObject };
		SetChildrenActive(activated: false);
	}

	private void AirBalloonLeaving(Artifact artifact)
	{
		GameSystem<InputSystem>.Instance().MoveLock = true;
		VehicleBase vehicleBase = ((!(artifact != null)) ? null : artifact.GetComponentInChildren<VehicleBase>());
		if ((bool)vehicleBase)
		{
			PlayerBehavior.LocalPlayer.Driver.Mount(vehicleBase, delegate
			{
				Connections.Frontend.Send(default(MountAirBalloon));
			});
		}
		KUtility.DelayedCall(this, delegate
		{
			GameSystem<TutorialIslandSystem>.Instance().SendDepartTutorial(artifact);
		}, 5f);
	}

	private void Update()
	{
		if (base.IsOpened && _nextUpdateAt > 0f && _nextUpdateAt < Time.time)
		{
			_nextUpdateAt = 0f;
			EstateSystem.GetEstateLicenses(OnEstateLicenses);
		}
	}

	public void Open(EstateTabWidget.Menu menu)
	{
		if (base.IsOpened)
		{
			SelectMenu(menu);
			return;
		}
		_forceSelectMenu = menu;
		Open();
	}

	private void HidePages(GameObject except = null)
	{
		GameObject[] pages = _pages;
		foreach (GameObject gameObject in pages)
		{
			if (except != gameObject)
			{
				gameObject.SetActive(value: false);
			}
		}
		HasBack.Value = _favoriteIslandsPage.gameObject == except || _recommendRegionPage.gameObject == except;
	}

	private static void OnLoadingCurtainHidden()
	{
		EstateSystem.GetEstateLicenses(CheckWarningEstate);
	}

	private static void CheckWarningEstate(EstateLicenses licenses)
	{
		if (!CheckWarningEstate(licenses.ClanEstate))
		{
			CheckWarningEstate(licenses.UrbanEstate);
		}
	}

	private static bool CheckWarningEstate(EstateLicense? license)
	{
		if (!license.HasValue)
		{
			return false;
		}
		EstateLicense e = license.Value;
		string text = null;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (e.DepositRunsOutAt.HasValue && predictedServerTime < e.DepositRunsOutAt.Value)
		{
			double num = e.DepositRunsOutAt.Value - predictedServerTime;
			if (num > 86400.0)
			{
				return false;
			}
			switch (e.Type)
			{
			case OwnerType.Player:
				text = T._("<em>사유지 유효기간</em>이 <em>{0}</em> 뒤 끝납니다. 권리를 잃지 않도록 영토 선언을 연장해주세요.", TimedeltaFormatter.Format(num, 2, "min"));
				break;
			case OwnerType.ClanEstate:
				text = T._("<em>부족 영토 유효기간</em>이 <em>{0}</em> 뒤 끝납니다. 권리를 잃지 않도록 영토 선언을 연장해주세요.", TimedeltaFormatter.Format(num, 2, "min"));
				break;
			}
		}
		if (e.ExpiresAt.HasValue && predictedServerTime < e.ExpiresAt.Value)
		{
			double num2 = e.ExpiresAt.Value - predictedServerTime;
			if (num2 < 7200.0)
			{
				switch (e.Type)
				{
				case OwnerType.Player:
					text = T._("다른 개척자들이 <em>당신의 땅과 재산</em>을 차지할 수 있게 됩니다! <em>사유지 유효기간</em>을 지금 연장하세요!", TimedeltaFormatter.Format(num2, 2, "min"));
					break;
				case OwnerType.ClanEstate:
					text = T._("다른 개척자들이 <em>부족의 땅과 재산</em>을 차지할 수 있게 됩니다! <em>부족 영토 유효기간</em>을 지금 연장하세요!");
					break;
				}
			}
			else
			{
				switch (e.Type)
				{
				case OwnerType.Player:
					text = T._("<em>사유지 유효기간</em>이 끝나서 임시 보호중입니다! <em>땅과 재산</em>을 잃지 않도록 연장하세요.", TimedeltaFormatter.Format(num2, 2, "min"));
					break;
				case OwnerType.ClanEstate:
					text = T._("<em>부족 영토 유효기간</em>이 끝나서 임시 보호중입니다! <em>땅과 재산</em>을 잃지 않도록 연장하세요.");
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		UIManager.MessageBox.Show(text, delegate(bool ok)
		{
			if (ok)
			{
				ShowExtendEstatePopup(e, delegate
				{
					long extendingCost = Yaml.Util.Singleton<CostsYaml>.Instance.Estate.GetExtendingCost(e.Type, e.Size);
					EstateSystem.ExtendEstate(e.EstateId, extendingCost * e.Size * 7, null);
				});
			}
		}, T._("연장"), T._("나중에"));
		return true;
	}

	private static void OnContextAction(List<InteractionMenuData> actions)
	{
		Point2 currentTile = PlayerBehavior.LocalPlayer.CurrentTile;
		EstateInfo estateInfo = EstateSystem.GetEstateInfo(currentTile);
		if (estateInfo != null && estateInfo.IsLocalPlayers())
		{
			OwnerType type = estateInfo.License.Type;
			if (type == OwnerType.PersonalPlayer || type == OwnerType.Player || type == OwnerType.ClanEstate)
			{
				actions.Add(Interaction.EstateMenu);
			}
		}
	}

	protected override bool TryOpen()
	{
		EstateTabWidget.Menu menu = SelectedMenu;
		if (_forceSelectMenu.HasValue)
		{
			menu = _forceSelectMenu.Value;
			_forceSelectMenu = null;
		}
		else
		{
			Point2 currentTile = PlayerBehavior.LocalPlayer.CurrentTile;
			EstateInfo estateInfo = EstateSystem.GetEstateInfo(currentTile);
			if (estateInfo != null && estateInfo.IsLocalPlayers())
			{
				switch (estateInfo.License.Type)
				{
				case OwnerType.PersonalPlayer:
					menu = EstateTabWidget.Menu.Personal;
					break;
				case OwnerType.Player:
					menu = EstateTabWidget.Menu.Urban;
					break;
				case OwnerType.ClanEstate:
					menu = EstateTabWidget.Menu.Clan;
					break;
				}
			}
		}
		_loading = true;
		HidePages();
		UIManager.Popup.LoadingRing.AttachToWidget(_mainContainer.gameObject);
		if (!SelectMenu(menu))
		{
			SelectMenu(EstateTabWidget.Menu.Urban);
		}
		EstateSystem.GetEstateLicenses(OnEstateLicenses);
		if (_airBalloon != null || MenuHelper.IsEnabledMenu(this))
		{
			SetChildrenActive(activated: true);
			return true;
		}
		return false;
	}

	protected override bool TryClose()
	{
		if (_favoriteIslandsPage.gameObject.activeSelf || _recommendRegionPage.gameObject.activeSelf)
		{
			SelectMenu(SelectedMenu);
			return false;
		}
		return base.TryClose();
	}

	public void NotifyEstateLicenseChanged(OwnerType owner, EstateLicense? license)
	{
		if (base.IsOpened && !_loading)
		{
			switch (owner)
			{
			default:
				return;
			case OwnerType.PersonalPlayer:
				_licenses.PersonalEstate = license;
				break;
			case OwnerType.Player:
				_licenses.UrbanEstate = license;
				break;
			case OwnerType.ClanEstate:
				_licenses.ClanEstate = license;
				break;
			case OwnerType.System:
			case OwnerType.ClanWarphole:
				return;
			}
			OnEstateLicenses(_licenses);
		}
	}

	public void OnClanCargoWarpholeChange(ClanCargoWarphole? warphole)
	{
		if (base.IsOpened && !_loading)
		{
			_licenses.ClanCargoWarphole = warphole;
			OnEstateLicenses(_licenses);
		}
	}

	private void OnEstateLicenses(EstateLicenses licenses)
	{
		if (base.IsOpened)
		{
			_licenses = licenses;
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			double nextUpdateAt = 0.0;
			nextUpdateAt = GetNextUpdateAt(predictedServerTime, nextUpdateAt, _licenses.PersonalEstate);
			nextUpdateAt = GetNextUpdateAt(predictedServerTime, nextUpdateAt, _licenses.UrbanEstate);
			nextUpdateAt = GetNextUpdateAt(predictedServerTime, nextUpdateAt, _licenses.ClanEstate);
			nextUpdateAt = GetNextUpdateAt(predictedServerTime, nextUpdateAt, _licenses.ClanCargoWarphole);
			if (nextUpdateAt > 0.0)
			{
				_nextUpdateAt = Time.time + (float)(nextUpdateAt - predictedServerTime);
			}
			else
			{
				_nextUpdateAt = 0f;
			}
			_licenses.LargestPersonalEstateSize = Mathf.Max(_licenses.LargestPersonalEstateSize, _licenses.PersonalEstate.HasValue ? _licenses.PersonalEstate.Value.Size : 0);
			_licenses.LargestUrbanEstateSize = Mathf.Max(_licenses.LargestUrbanEstateSize, _licenses.UrbanEstate.HasValue ? _licenses.UrbanEstate.Value.Size : 0);
			_licenses.LargestClanEstateSize = Mathf.Max(_licenses.LargestClanEstateSize, _licenses.ClanEstate.HasValue ? _licenses.ClanEstate.Value.Size : 0);
			bool moveTomain = false;
			if (_loading)
			{
				_loading = false;
				UIManager.Popup.LoadingRing.DetachFromWidget(_mainContainer.gameObject);
				moveTomain = true;
			}
			SelectMenu(SelectedMenu, moveTomain);
		}
	}

	private static double GetNextUpdateAt(double now, double nextUpdateAt, EstateLicense? license)
	{
		if (!license.HasValue)
		{
			return nextUpdateAt;
		}
		EstateLicense value = license.Value;
		if (value.ActivatedAt > now)
		{
			nextUpdateAt = ((!(nextUpdateAt > 0.0)) ? value.ActivatedAt : Math.Min(nextUpdateAt, value.ActivatedAt));
		}
		if (value.ExpiresAt.HasValue && value.ExpiresAt.Value > now)
		{
			nextUpdateAt = ((!(nextUpdateAt > 0.0)) ? value.ExpiresAt.Value : Math.Min(nextUpdateAt, value.ExpiresAt.Value));
		}
		if (value.ProtectedUntil.HasValue && value.ProtectedUntil.Value > now)
		{
			nextUpdateAt = ((!(nextUpdateAt > 0.0)) ? value.ProtectedUntil.Value : Math.Min(nextUpdateAt, value.ProtectedUntil.Value));
		}
		return nextUpdateAt;
	}

	private static double GetNextUpdateAt(double now, double nextUpdateAt, ClanCargoWarphole? warphole)
	{
		if (!warphole.HasValue)
		{
			return nextUpdateAt;
		}
		ClanCargoWarphole value = warphole.Value;
		if (value.BattleInfo.HasValue)
		{
			if (value.BattleInfo.Value.CycleProtectionUntil > now)
			{
				nextUpdateAt = ((!(nextUpdateAt > 0.0)) ? value.BattleInfo.Value.CycleProtectionUntil : Math.Min(nextUpdateAt, value.BattleInfo.Value.CycleProtectionUntil));
			}
			if (value.BattleInfo.Value.CycleUntil > now)
			{
				nextUpdateAt = ((!(nextUpdateAt > 0.0)) ? value.BattleInfo.Value.CycleUntil : Math.Min(nextUpdateAt, value.BattleInfo.Value.CycleUntil));
			}
		}
		return nextUpdateAt;
	}

	private void OnTabClick(EstateTabWidget.Menu menu)
	{
		SelectMenu(menu);
	}

	private bool SelectMenu(EstateTabWidget.Menu menu, bool moveTomain = true)
	{
		if (menu == EstateTabWidget.Menu.Base && !PlayerBehavior.LocalPlayer.HasClan)
		{
			UIManager.SystemMsg(null, T._("부족에 가입하면 이용 가능합니다."), 4f);
			return false;
		}
		SelectedMenu = menu;
		_tabWidget.SelectTab(menu);
		switch (menu)
		{
		case EstateTabWidget.Menu.Personal:
			ShowPersonalEstatePage(moveTomain);
			break;
		case EstateTabWidget.Menu.Urban:
			ShowUrbanEstatePage(moveTomain);
			break;
		case EstateTabWidget.Menu.Clan:
			ShowClanEstatePage(moveTomain);
			break;
		case EstateTabWidget.Menu.Base:
			ShowBasePage(moveTomain);
			break;
		}
		return true;
	}

	private void ShowPersonalEstatePage(bool moveTomain)
	{
		if (!_loading)
		{
			_estatePage.Show(OwnerType.PersonalPlayer, _licenses.PersonalEstate, _licenses.LargestPersonalEstateSize, moveTomain, _licenses.UrbanWarpholeTile);
			HidePages(_estatePage.gameObject);
		}
	}

	private void ShowUrbanEstatePage(bool moveTomain)
	{
		if (!_loading)
		{
			_estatePage.Show(OwnerType.Player, _licenses.UrbanEstate, _licenses.LargestUrbanEstateSize, moveTomain);
			HidePages(_estatePage.gameObject);
		}
	}

	private void ShowClanEstatePage(bool moveTomain)
	{
		if (!_loading)
		{
			_estatePage.Show(OwnerType.ClanEstate, _licenses.ClanEstate, _licenses.LargestClanEstateSize, moveTomain);
			HidePages(_estatePage.gameObject);
		}
	}

	private void ShowBasePage(bool moveTomain)
	{
		if (!_loading)
		{
			_basePage.Show(_licenses, moveTomain);
			HidePages(_basePage.gameObject);
		}
	}

	private void ShowRecommendedRegionPage()
	{
		_recommendRegionPage.Show(!_loading);
		HidePages(_recommendRegionPage.gameObject);
	}

	public void ShowFavoriteIslandsPage()
	{
		_favoriteIslandsPage.Show();
		HidePages(_favoriteIslandsPage.gameObject);
	}

	public Transform GetRecommendedRegionNodeTransform(Role role = Role.Rural)
	{
		return _recommendRegionPage.GetRegionNodeTransform(role);
	}

	public static void ShowExtendEstatePopup(EstateLicense estate, Action onOk)
	{
		global::System.DateTime dateTime = ((!estate.DepositRunsOutAt.HasValue) ? global::System.DateTime.Now.AddDays(7.0) : Times.UnixTimeToDateTimeLocal(estate.DepositRunsOutAt.Value).AddDays(7.0));
		MessageBox messageBox = UIManager.MessageBox;
		string mainText = string.Empty;
		string subText = string.Empty;
		switch (estate.Type)
		{
		case OwnerType.Player:
			mainText = T._("사유지 유효 기간을 일주일 연장합니다.");
			subText = T._("사유지 유효 기간은 사유지를 확장하거나 축소하면 달라집니다.");
			break;
		case OwnerType.ClanEstate:
			mainText = T._("부족 영토 유효 기간을 일주일 연장합니다.");
			subText = T._("부족 영토 유효 기간은 부족 영토를 확장하거나 축소하면 달라집니다.");
			break;
		}
		long extendingCost = Yaml.Util.Singleton<CostsYaml>.Instance.Estate.GetExtendingCost(estate.Type, estate.Size);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		Pair<int, double>? deposit = estate.Deposit;
		long num = (deposit.HasValue ? (estate.Deposit.Value.Item1 - (int)((predictedServerTime - estate.Deposit.Value.Item2) * (double)extendingCost * (double)estate.Size / 86400.0)) : 0);
		long num2 = extendingCost * estate.Size * 7;
		messageBox.AddKeyValueInfo(T._("예치금"), $"{Durango.Logic.Item.Inventory.CurrencyFormat(num, Currency.TStone)} [preset=animation_arrow] <em>{Durango.Logic.Item.Inventory.CurrencyFormat(num + num2, Currency.TStone)}</em>");
		messageBox.AddKeyValueInfo(T._("연장 시 만료일"), dateTime.ToString("g", T.Culture));
		string confirm = Durango.Logic.Item.Inventory.ToCurrencyButtonText(T._("연장"), num2, Currency.TStone);
		string cancel = T._("취소");
		switch (estate.Type)
		{
		case OwnerType.Player:
			messageBox.SetCurrencyInfo(Currency.TStone);
			break;
		case OwnerType.ClanEstate:
			messageBox.SetClanFund();
			break;
		}
		messageBox.Show(mainText, subText, delegate(bool ok)
		{
			if (ok && onOk != null)
			{
				onOk();
			}
		}, confirm, cancel);
	}

	[Uri]
	private void SelectMenuUri(string menu)
	{
		if (menu.TryEnum<EstateTabWidget.Menu>(out var value))
		{
			Open(value);
		}
	}

	[Uri("RecommendedRegion")]
	private void RecommendRegion()
	{
		ShowRecommendedRegionPage();
		Open();
	}
}
