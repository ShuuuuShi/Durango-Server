using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Clan;
using Durango.Logic.Clusters;
using Durango.Logic.Estate;
using Durango.Logic.Map;
using Durango.Logic.Social;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.Player;
using Durango.Terrain;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using L10N;
using Messages;
using Shared.Chat;
using Shared.Economy;
using Shared.Estate;
using Shared.Region;
using Shared.System;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Map")]
public class WorldMapGroup : UIBase
{
	public enum ButtonType
	{
		[T.EnumName("모험 복귀")]
		WarpBack,
		[T.EnumName("항구로")]
		WarpToPort,
		[T.EnumName("귀환 지점으로")]
		ReturnToHome,
		[T.EnumName("도시섬 사유지로")]
		ReturnToUrbanEstate,
		[T.EnumName("개인섬 사유지로")]
		ReturnToPersonalEstate,
		[T.EnumName("캠프로")]
		ReturnToCamp,
		[T.EnumName("부족영토로")]
		ReturnToClanEstate,
		[T.EnumName("위치 공유")]
		SharePosToClan,
		[T.EnumName("지도 열기")]
		PurchaseMap,
		[T.EnumName("워프")]
		Teleport
	}

	private struct ButtonInfo
	{
		public ButtonType Type;

		public string Icon;

		public Action Function;

		public Func<bool> ValidFunc;

		public string Text => Type.GetName();
	}

	private enum WorldOpenMode
	{
		None,
		Warp,
		Revive,
		MapDownloading,
		SharePos,
		SharePosToClan
	}

	private enum InfoType
	{
		None,
		[T.EnumName("공유할 위치를 선택하세요.")]
		PickForSharePos,
		[T.EnumName("선택한 위치를 공유하시겠습니까?")]
		ConfirmToSharePos,
		[T.EnumName("진입할 워프홀을 선택하세요.")]
		SelectWarpHoleForEntry,
		[T.EnumName("부활할 워프홀을 선택하세요.")]
		SelectWarpHoleForRevive
	}

	[SerializeField]
	private Transform _attachDock;

	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private UIWidget _buttonContainer;

	[SerializeField]
	private SelectableButton _actionButtonBase;

	[SerializeField]
	private MapActionButtonSelector _actionButtonSelector;

	[SerializeField]
	private GameObject _worldMapBG;

	[SerializeField]
	private RectLayoutComponent _exploredWarpholeLayout;

	[SerializeField]
	private UIGaugeWithPercentage _exploreGaugeElement;

	[SerializeField]
	private ExploreReward _exploreReward;

	[SerializeField]
	private UIWidget _infoContainer;

	[SerializeField]
	private UILabel _informationLabel;

	[SerializeField]
	private WorldMapScaleInfo _scaleInfo;

	[SerializeField]
	private WorldMapEnvWidget _mapEnvironmentWidget;

	[SerializeField]
	private TweenerPlayer _downloadTweenerPlayer;

	[SerializeField]
	private float _downloadTweenDuration;

	[SerializeField]
	private UILabel _indicatorLabel;

	[SerializeField]
	private SpriteData _warpCostIcon;

	[SerializeField]
	private SpriteData _defendingWarphole;

	[SerializeField]
	private GameObject _voucherWidget;

	private ButtonInfo[][] _buttonInfos;

	private int[] _buttonIndexes;

	private int _buttonSelectorIndex;

	private WorldOpenMode _worldOpenMode;

	private Point2? _warpEntryTile;

	private ChannelType? _sharePosChannel;

	private string _sharePosConvId;

	private EstateLicenses _estateLicenses;

	private readonly ListObjectPool<SelectableButton> _actionButtons = new ListObjectPool<SelectableButton>();

	private Vector2? _openFocusPosition;

	private readonly Dictionary<Point2, WarpCost> _latestWarpholeCosts = new Dictionary<Point2, WarpCost>();

	private bool _isTeleportMode;

	private void Start()
	{
		_openCloseSound = UISound.GroupType.WorldMap;
		GameSystem<MapSystem>.Instance().WarpTimer.Started += delegate(PredictTimer predictTimer)
		{
			Durango.Logic.Timer.Timer.Play<DefaultProgressGauge>(predictTimer.Timer);
		};
		GameSystem<MapSystem>.Instance().MapPurchased += delegate
		{
			Open();
			_warpEntryTile = null;
			SetWorldOpenMode(WorldOpenMode.MapDownloading);
			MapSystemExploredPoIsUpdated();
			UpdateButtons();
		};
		ButtonInfo buttonInfo = default(ButtonInfo);
		buttonInfo.Type = ButtonType.WarpBack;
		buttonInfo.Icon = "icon_map_warphole";
		buttonInfo.ValidFunc = delegate
		{
			RegionTile? lastReturnPoint = GameSystem<MapSystem>.Instance().Points.LastReturnPoint;
			return lastReturnPoint.HasValue && IsValidRegion(lastReturnPoint.Value.Region);
		};
		ButtonInfo buttonInfo2 = buttonInfo;
		buttonInfo = default(ButtonInfo);
		buttonInfo.Type = ButtonType.ReturnToCamp;
		buttonInfo.Icon = "icon_map_base";
		buttonInfo.ValidFunc = delegate
		{
			RegionTile? campPoint = GameSystem<MapSystem>.Instance().Points.CampPoint;
			return campPoint.HasValue && IsValidRegion(campPoint.Value.Region);
		};
		ButtonInfo buttonInfo3 = buttonInfo;
		buttonInfo = default(ButtonInfo);
		buttonInfo.Type = ButtonType.WarpToPort;
		buttonInfo.Icon = "icon_map_port";
		buttonInfo.ValidFunc = () => GameSystem<MapSystem>.Instance().GetPOICount(Shared.System.PointOfInterest.Port) > 0;
		ButtonInfo buttonInfo4 = buttonInfo;
		ButtonInfo buttonInfo5 = default(ButtonInfo);
		buttonInfo5.Type = ButtonType.ReturnToClanEstate;
		buttonInfo5.Icon = "icon_map_clan_building";
		buttonInfo5.ValidFunc = () => _estateLicenses.ClanEstate.HasValue;
		buttonInfo = buttonInfo5;
		ButtonInfo buttonInfo6 = buttonInfo;
		buttonInfo = default(ButtonInfo);
		buttonInfo.Type = ButtonType.ReturnToUrbanEstate;
		buttonInfo.Icon = "icon_map_civilizedisland";
		buttonInfo.Function = delegate
		{
			ReturnToEstate(OwnerType.Player);
		};
		buttonInfo.ValidFunc = () => _estateLicenses.UrbanEstate.HasValue;
		ButtonInfo buttonInfo7 = buttonInfo;
		buttonInfo = default(ButtonInfo);
		buttonInfo.Type = ButtonType.ReturnToPersonalEstate;
		buttonInfo.Icon = "icon_map_privateisland";
		buttonInfo.Function = delegate
		{
			ReturnToEstate(OwnerType.PersonalPlayer);
		};
		buttonInfo.ValidFunc = () => _estateLicenses.PersonalEstate.HasValue;
		ButtonInfo buttonInfo8 = buttonInfo;
		if (GameManager.ClusterMode == Mode.SingleMode)
		{
			ButtonInfo[][] array = new ButtonInfo[4][];
			int num = 0;
			ButtonInfo[] array2 = new ButtonInfo[1];
			int num2 = 0;
			buttonInfo = default(ButtonInfo);
			buttonInfo.Type = ButtonType.ReturnToHome;
			buttonInfo.Icon = "icon_house";
			buttonInfo.Function = delegate
			{
				ReturnToHome();
			};
			array2[num2] = buttonInfo;
			array[num] = array2;
			int num3 = 2;
			ButtonInfo[] array3 = new ButtonInfo[1];
			int num4 = 0;
			ButtonInfo buttonInfo9 = default(ButtonInfo);
			buttonInfo9.Type = ButtonType.SharePosToClan;
			buttonInfo9.Icon = "icon_map_pinpoint";
			buttonInfo9.Function = delegate
			{
				SetWorldOpenMode(WorldOpenMode.SharePosToClan);
			};
			buttonInfo9.ValidFunc = () => PlayerBehavior.LocalPlayer.HasClan;
			array3[num4] = buttonInfo9;
			array[num3] = array3;
			int num5 = 3;
			ButtonInfo[] array4 = new ButtonInfo[1];
			int num6 = 0;
			ButtonInfo buttonInfo10 = default(ButtonInfo);
			buttonInfo10.Type = ButtonType.PurchaseMap;
			buttonInfo10.Icon = null;
			buttonInfo10.Function = delegate
			{
				PurchaseMap();
			};
			buttonInfo10.ValidFunc = () => GameSystem<MapSystem>.Instance().CanPurchaseMap && GameSystem<MenuSystem>.Instance().IsEnabled(Durango.Logic.MenuType.Shop);
			array4[num6] = buttonInfo10;
			array[num5] = array4;
			_buttonInfos = array;
			switch (GameManager.Region.Role())
			{
			case Role.Rural:
				_buttonInfos[1] = new ButtonInfo[8] { buttonInfo, buttonInfo2, buttonInfo4, buttonInfo6, buttonInfo7, buttonInfo8, buttonInfo9, buttonInfo10 };
				break;
			case Role.Risky:
				_buttonInfos[1] = new ButtonInfo[4] { buttonInfo3, buttonInfo8, buttonInfo7, buttonInfo6 };
				break;
			case Role.Outpost:
				_buttonInfos[1] = new ButtonInfo[4] { buttonInfo4, buttonInfo8, buttonInfo7, buttonInfo6 };
				break;
			case Role.Urban:
				_buttonInfos[1] = new ButtonInfo[5] { buttonInfo2, buttonInfo4, buttonInfo8, buttonInfo7, buttonInfo6 };
				break;
			case Role.Safehouse:
				_buttonInfos[1] = new ButtonInfo[1] { buttonInfo3 };
				break;
			case Role.Personal:
				_buttonInfos[1] = new ButtonInfo[5] { buttonInfo2, buttonInfo4, buttonInfo8, buttonInfo7, buttonInfo6 };
				break;
			}
		}
		else
		{
			_buttonInfos = new ButtonInfo[1][] { new ButtonInfo[1]
			{
				new ButtonInfo
				{
					Type = ButtonType.Teleport,
					Icon = "act_warphole",
					Function = ToggleTeleport,
					ValidFunc = () => _worldOpenMode == WorldOpenMode.None
				}
			} };
		}
		_buttonIndexes = new int[_buttonInfos.Length];
		_actionButtons.BaseObject = _actionButtonBase;
		_actionButtons.UseBase = true;
		_actionButtons.Init(delegate(SelectableButton btn)
		{
			btn.Clicked = OnClickActionButton;
			btn.SubClicked = OnSubClickActionButton;
		});
		_actionButtons.Set(_buttonInfos.Length);
		UIEventListener.Get(_closeButton).onClick = delegate
		{
			switch (_worldOpenMode)
			{
			case WorldOpenMode.SharePos:
				Close();
				break;
			default:
				UIBase.CloseAllUI();
				break;
			case WorldOpenMode.SharePosToClan:
				SetWorldOpenMode(WorldOpenMode.None);
				break;
			}
		};
		base.OnOpenSucceed += delegate
		{
			_isTeleportMode = false;
			_actionButtonSelector.Hide();
			MapSystemExploredPoIsUpdated();
			if (_openFocusPosition.HasValue)
			{
				Durango.Utils.Singleton<MapContext>.Instance().FocusToTilePostion(_openFocusPosition.Value);
				_openFocusPosition = null;
			}
			else
			{
				Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
				Durango.Utils.Singleton<MapContext>.Instance().FocusToTilePostion(Durango.Terrain.Util.ClientPositionToTilePosition(currentPosition));
			}
			GameSystem<InputSystem>.Instance().On(InputCommand.GestureZoom, OnGestureZoomProcess);
			GameSystem<MapSystem>.Instance().ExploredPOIsUpdated += MapSystemExploredPoIsUpdated;
			GameSystem<InventorySystem>.Instance().WalletUpdated += InventorySystem_WalletUpdated;
			Durango.Utils.Singleton<MapIndicators>.Instance().IndicatorClicked += OnClickIndicator;
			Durango.Utils.Singleton<MapContext>.Instance().ScaleChanged += MapContext_ScaleChanged;
			RefreshScaleInfo();
			RefreshVoucherWidget();
		};
		base.OnCloseSucceed += delegate
		{
			SetWorldOpenMode(WorldOpenMode.None);
			Durango.Utils.Singleton<MapIndicators>.Instance().HideToolTipLabel();
			GameSystem<InputSystem>.Instance().Off(InputCommand.GestureZoom, OnGestureZoomProcess);
			if (GameSystem<MapSystem>.HasInstance())
			{
				GameSystem<MapSystem>.Instance().ExploredPOIsUpdated -= MapSystemExploredPoIsUpdated;
			}
			if (GameSystem<InventorySystem>.HasInstance())
			{
				GameSystem<InventorySystem>.Instance().WalletUpdated -= InventorySystem_WalletUpdated;
			}
			if (Durango.Utils.Singleton<MapIndicators>.HasInstance())
			{
				Durango.Utils.Singleton<MapIndicators>.Instance().IndicatorClicked -= OnClickIndicator;
			}
			if (Durango.Utils.Singleton<MapContext>.HasInstance())
			{
				Durango.Utils.Singleton<MapContext>.Instance().ScaleChanged -= MapContext_ScaleChanged;
			}
			UIManager.Popup.Tooltip<SharePointConfirmPopup>().Hide();
		};
		UIEventListener uIEventListener = UIEventListener.Get(_worldMapBG);
		uIEventListener.onClick = OnClickWorldMap;
		uIEventListener.onDrag = OnDragWorldMap;
		uIEventListener.onDoubleClick = delegate
		{
			MapContext mapContext = Durango.Utils.Singleton<MapContext>.Instance();
			UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
			if (Debug.isDebugBuild && UICamera.CountInputSources() >= 2)
			{
				Vector2 vector = mapContext.ScreenPosToTilePos(currentTouch.pos);
				int x = (int)vector.x;
				int y = (int)vector.y;
				UIManager.MessageBox.Show($"Do you want to teleport to ({x},{y})", delegate(bool ok)
				{
					if (ok)
					{
						Connections.Frontend.Send(new Cheat
						{
							_Cheat = $"m {x} {y}"
						});
					}
				});
			}
			else
			{
				float currentZoomScale = mapContext.CurrentZoomScale;
				float num7 = currentZoomScale + 0.5f;
				num7 = (float)(int)(num7 / 0.5f) * 0.5f;
				mapContext.Zoom(num7 - currentZoomScale, currentTouch.pos);
			}
		};
		_mapEnvironmentWidget.gameObject.SetActive(value: true);
		_indicatorLabel.text = string.Format("[icon=icon_map_myplayer_dir] {0}  [icon=icon_map_otherplayer] {1}  [icon=icon_map_house] {2}  [icon=icon_map_port] {3}  [FF6A00][icon=icon_map_bonfire][-] {4}", T._("내 위치"), T._("다른 유저"), T._("숙소"), T._("항구"), T._("모닥불"));
		SetInformationLabel();
		ShowDownloadTweener(show: false);
		TryClose();
		AddInteractionHandler();
	}

	public void OpenForWarp(Point2? tile)
	{
		Open();
		_warpEntryTile = tile;
		SetWorldOpenMode(WorldOpenMode.Warp);
	}

	public void OpenForRevive()
	{
		Open();
		_warpEntryTile = null;
		SetWorldOpenMode(WorldOpenMode.Revive);
	}

	public void OpenForSharePos(ChannelType? channelType = null, string conversationId = null)
	{
		Open();
		_warpEntryTile = null;
		_sharePosChannel = channelType;
		_sharePosConvId = conversationId;
		SetWorldOpenMode(WorldOpenMode.SharePos);
	}

	public void OpenForAnnounceBalloon(AnnounceType type, Vector2 posPinPoint, string entityId)
	{
		if (string.IsNullOrEmpty(entityId))
		{
			Durango.Utils.Singleton<MapIndicators>.Instance().AddAnnounceBalloon(type, posPinPoint, PlayerInfoManager.EmptyPlayer);
			_openFocusPosition = posPinPoint;
			Open();
			return;
		}
		Durango.Utils.Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(Durango.Player.PlayerInfo info)
		{
			if (info.Valid)
			{
				Durango.Utils.Singleton<MapIndicators>.Instance().AddAnnounceBalloon(type, posPinPoint, info);
				_openFocusPosition = posPinPoint;
				Open();
			}
		});
	}

	public Transform GetButtonTransform(ButtonType type)
	{
		SelectableButton selectableButton = null;
		for (int i = 0; i < _buttonInfos.Length; i++)
		{
			ButtonInfo[] array = _buttonInfos[i];
			if (array == null)
			{
				continue;
			}
			if (i == _buttonSelectorIndex && _actionButtonSelector.IsVisible)
			{
				UIWidget uIWidget = FindButtonFromSelector(type, array);
				if (uIWidget != null)
				{
					return uIWidget.transform;
				}
			}
			SelectableButton selectableButton2 = _actionButtons[i];
			if (!(selectableButton2 == null))
			{
				if (selectableButton2.GetStyle() == PresetButton.Style.BorderPopup)
				{
					selectableButton = selectableButton2;
				}
				int num = _buttonIndexes[i];
				if (num >= 0 && num < array.Length && array[num].Type == type)
				{
					return selectableButton2.transform;
				}
			}
		}
		if (selectableButton != null && selectableButton.SubButton != null)
		{
			return selectableButton.SubButton.transform;
		}
		return null;
	}

	private UIWidget FindButtonFromSelector(ButtonType type, ButtonInfo[] infos)
	{
		int i = 0;
		for (int count = _actionButtonSelector.Count; i < count; i++)
		{
			int index = _actionButtonSelector.GetIndex(i);
			if (index >= 0 && index < infos.Length && infos[index].Type == type)
			{
				return _actionButtonSelector.Get(i);
			}
		}
		return null;
	}

	protected override bool TryOpen()
	{
		Durango.Utils.Singleton<MapContext>.Instance().Attach(worldMapMode: true, _attachDock);
		_buttonContainer.alpha = 0f;
		EstateSystem.GetEstateLicenses(OnLicenses);
		VisibleController.Hide(VisibleType.HideOnWorldMap, hide: true, "WorldMap");
		return base.TryOpen();
	}

	protected override bool TryClose()
	{
		VisibleController.Hide(VisibleType.HideOnWorldMap, hide: false, "WorldMap");
		return base.TryClose();
	}

	private void OnLicenses(EstateLicenses licenses)
	{
		_estateLicenses = licenses;
		_buttonContainer.alpha = 1f;
		UpdateButtons();
	}

	private void UpdateButtons()
	{
		for (int i = 0; i < _buttonInfos.Length; i++)
		{
			SelectableButton selectableButton = _actionButtons[i];
			ButtonInfo[] array = _buttonInfos[i];
			int num = 0;
			int num2 = -1;
			int j = 0;
			for (int size = KUtility.GetSize(array); j < size; j++)
			{
				if (array[j].ValidFunc == null || array[j].ValidFunc())
				{
					if (num2 < 0)
					{
						num2 = j;
					}
					num++;
				}
			}
			if (num == 0)
			{
				selectableButton.gameObject.SetActive(value: false);
				continue;
			}
			selectableButton.gameObject.SetActive(value: true);
			ButtonInfo buttonInfo = array[num2];
			_buttonIndexes[i] = num2;
			if (buttonInfo.Type == ButtonType.PurchaseMap && num == 1)
			{
				selectableButton.SetStyle(PresetButton.Style.Solid);
				selectableButton.SetEffect(Yaml.Util.Singleton<CostsYaml>.Instance.OpenMap.HasVoucherFromCommodity() ? PresetButton.Effect.Emphasis : PresetButton.Effect.None);
				selectableButton.Text = GetPurchaseMapButtonText();
			}
			else
			{
				selectableButton.SetStyle((num <= 1) ? PresetButton.Style.Border : PresetButton.Style.BorderPopup);
				selectableButton.Text = $"[icon={buttonInfo.Icon}] {buttonInfo.Text}";
			}
			selectableButton.Selected = false;
		}
		Vector3 position = _actionButtonBase.Widget.GetPosition(0.5f, 0f);
		float num3 = UIUtility.WidgetsReposition(_actionButtons, Vector3.up, position, 20f);
		num3 += 20f;
		_exploreGaugeElement.Widget.SetPosition(position + Vector3.up * num3, 0.5f, 0f);
	}

	private string GetPurchaseMapButtonText()
	{
		OpenMapCost openMap = Yaml.Util.Singleton<CostsYaml>.Instance.OpenMap;
		bool flag = openMap.HasVoucherFromCommodity();
		if (openMap.HasVoucher() || flag)
		{
			string arg;
			if (flag)
			{
				string voucherId = openMap.GetVoucherFromCommodity().VoucherId;
				arg = T._("무료 ({0}/{1})", InventorySystem.Wallet.GetVoucherCount(voucherId), SingletonDict<string, Voucher>.Instance.Get(voucherId).CountMax);
			}
			else
			{
				arg = $"[icon={openMap.GetVoucher().VoucherId}]  1";
			}
			return $"{ButtonType.PurchaseMap.GetName()}  [preset=round_box? {arg} ]";
		}
		return T._("지도 열기 이용권 구입");
	}

	private void InventorySystem_WalletUpdated()
	{
		UpdateButtons();
	}

	private static bool CanAction(ButtonType type)
	{
		if (type == ButtonType.SharePosToClan)
		{
			return true;
		}
		if (PlayerBehavior.LocalPlayer.Driver.IsWarpAllowed)
		{
			return true;
		}
		UIManager.SystemMsg(T._("탑승 중에는 사용할 수 없습니다"));
		return false;
	}

	private void OnClickActionButton()
	{
		int num = _actionButtons.IndexOf((SelectableButton)Selectable.Current);
		ButtonInfo buttonInfo = _buttonInfos[num][_buttonIndexes[num]];
		if (CanAction(buttonInfo.Type))
		{
			buttonInfo.Function();
		}
	}

	private void OnSubClickActionButton()
	{
		SelectableButton selectableButton = (SelectableButton)Selectable.Current;
		int index = _actionButtons.IndexOf(selectableButton);
		_buttonSelectorIndex = index;
		ButtonInfo[] array = _buttonInfos[index];
		int num = 0;
		int num2 = _buttonIndexes[index];
		_actionButtonSelector.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(array); i < size; i++)
		{
			if (i != num2 && (array[i].ValidFunc == null || array[i].ValidFunc()))
			{
				_actionButtonSelector.Add(i, array[i].Icon, array[i].Text);
				num++;
			}
		}
		_actionButtonSelector.EndLoad();
		if (num == 0)
		{
			return;
		}
		_actionButtonSelector.Show();
		_actionButtonSelector.Widget.SetPosition(selectableButton.Widget.GetPosition(1f, 1f), 1f, 0f);
		_actionButtonSelector.Selected = delegate(int idx)
		{
			ButtonInfo buttonInfo = _buttonInfos[index][idx];
			if (CanAction(buttonInfo.Type))
			{
				buttonInfo.Function();
			}
		};
	}

	private void SetInformationLabel(InfoType? type = null)
	{
		if (type.HasValue && type.Value != 0)
		{
			_informationLabel.gameObject.SetActive(value: true);
			_informationLabel.text = type.Value.GetName();
		}
		else
		{
			_informationLabel.gameObject.SetActive(value: false);
		}
		_infoContainer.alpha = ((!type.HasValue) ? 1f : 0f);
	}

	private void MapSystemExploredPoIsUpdated()
	{
		MapSystem mapSystem = GameSystem<MapSystem>.Instance();
		int pOICount = mapSystem.GetPOICount(Shared.System.PointOfInterest.Warphole);
		int pOICount2 = mapSystem.GetPOICount(Shared.System.PointOfInterest.CargoWarphole);
		int pOICount3 = mapSystem.GetPOICount(Shared.System.PointOfInterest.Crater);
		int pOICount4 = mapSystem.GetPOICount(Shared.System.PointOfInterest.Rift);
		int num = pOICount + pOICount2 + pOICount3 + pOICount4;
		int num2 = mapSystem.EntireWarpholeCount + mapSystem.EntireCraterCount + mapSystem.EntireRiftCount;
		if (num2 > 0)
		{
			_exploredWarpholeLayout.gameObject.SetActive(value: true);
			_exploreGaugeElement.SetTitle(T._("발견한 탐험지점")).SetGauge(num, num2);
			Messages.Cost? rewardtCost = mapSystem.GetRewardtCost();
			if (rewardtCost.HasValue && rewardtCost.Value.Amount > 0)
			{
				ExploreReward.RewardState state = ((num >= num2) ? ((!mapSystem.HasRewarded()) ? ExploreReward.RewardState.Available : ExploreReward.RewardState.Received) : ExploreReward.RewardState.Locked);
				_exploreReward.gameObject.SetActive(value: true);
				_exploreReward.Set(state, rewardtCost.Value);
			}
			else
			{
				_exploreReward.gameObject.SetActive(value: false);
			}
			_exploredWarpholeLayout.UpdateLayout();
			UIUtility.UpdateAnchors(_exploredWarpholeLayout.transform);
		}
		else
		{
			_exploredWarpholeLayout.gameObject.SetActive(value: false);
		}
	}

	private static void ReturnToHome()
	{
		UIBase.CloseAllUI();
		GameSystem<MapSystem>.Instance().ReturnToHome();
	}

	private static void WarpToPort()
	{
		UIBase.CloseAllUI();
		GameSystem<MapSystem>.Instance().WarpToPort();
	}

	private static void ReturnToEstate(OwnerType type)
	{
		UIBase.CloseAllUI();
		EstateSystem.ReturnToEstate(type);
	}

	private static void ReturnToClanEstate()
	{
		UIBase.CloseAllUI();
		EstateSystem.VisitEstate(OwnerType.ClanEstate, PlayerBehavior.LocalPlayer.ClanId);
	}

	private static void ReturnToCamp()
	{
		UIBase.CloseAllUI();
		GameSystem<MapSystem>.Instance().ReturnToCamp();
	}

	private void OnClickWorldMap(GameObject obj)
	{
		if (_worldOpenMode == WorldOpenMode.SharePos || _worldOpenMode == WorldOpenMode.SharePosToClan)
		{
			ShareTouchedPosition();
		}
		else
		{
			if (!_isTeleportMode)
			{
				return;
			}
			UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
			if (currentTouch == null)
			{
				return;
			}
			Vector2 vector = Durango.Utils.Singleton<MapContext>.Instance().ScreenPosToTilePos(currentTouch.pos);
			int num = (int)vector.x;
			int num2 = (int)vector.y;
			MessageBox messageBox = UIManager.MessageBox;
			if (!(messageBox != null))
			{
				return;
			}
			messageBox.Show(T._("선택한 위치로 워프하시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					Close();
					Connections.Frontend.Send(new Cheat
					{
						_Cheat = $"m {num} {num2}"
					});
				}
			});
		}
	}

	private void OnDragWorldMap(GameObject obj, Vector2 delta)
	{
		Durango.Utils.Singleton<MapContext>.Instance().Focus(Durango.Utils.Singleton<MapContext>.Instance().Offset + new Vector2(delta.x, delta.y));
	}

	private void ShareTouchedPosition()
	{
		UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
		if (currentTouch == null)
		{
			return;
		}
		SetInformationLabel(InfoType.ConfirmToSharePos);
		Vector2 tilePos = Durango.Utils.Singleton<MapContext>.Instance().ScreenPosToTilePos(currentTouch.pos);
		tilePos.x = Mathf.Round(tilePos.x);
		tilePos.y = Mathf.Round(tilePos.y);
		SharePointConfirmPopup sharePointConfirmPopup = UIManager.Popup.Tooltip<SharePointConfirmPopup>();
		switch (_worldOpenMode)
		{
		case WorldOpenMode.SharePos:
			sharePointConfirmPopup.Set(tilePos, delegate
			{
				Close();
			}, delegate
			{
				SetInformationLabel(InfoType.PickForSharePos);
			}, delegate
			{
				Close();
			}, _sharePosChannel, _sharePosConvId);
			sharePointConfirmPopup.Show();
			break;
		case WorldOpenMode.SharePosToClan:
			sharePointConfirmPopup.Set(tilePos, delegate
			{
				ShowSharePosPopup();
			}, delegate
			{
				SetInformationLabel(InfoType.PickForSharePos);
			}, delegate
			{
				SetWorldOpenMode(WorldOpenMode.None);
			}, ChannelType.Clan);
			sharePointConfirmPopup.Show();
			break;
		}
	}

	private void ShowSharePosPopup()
	{
		SetWorldOpenMode(WorldOpenMode.None);
		ConfirmPopup confirmPopup = UIManager.Popup.Tooltip<ConfirmPopup>();
		confirmPopup.AddButton(new MessageBox.Button(T._("부족 채팅 이동")), delegate
		{
			Close();
			UIManager.FindScript<ChattingGroupBase>().Open(ChatFilterType.Clan, string.Empty);
		});
		confirmPopup.Show(T._("부족 채팅에 위치가 공유 되었습니다."), 600f);
	}

	private void MapContext_ScaleChanged()
	{
		RefreshScaleInfo();
	}

	private void RefreshScaleInfo()
	{
		_scaleInfo.Refresh(Durango.Utils.Singleton<MapContext>.Instance().CurrentZoomScale, (float)TerrainMeta.TileCount * 200f / 100f / 1280f);
	}

	private void RefreshVoucherWidget()
	{
		_voucherWidget.SetActive(_worldOpenMode == WorldOpenMode.None && GameSystem<MapSystem>.Instance().CanPurchaseMap);
	}

	private void ShowDownloadTweener(bool show)
	{
		if (show)
		{
			_indicatorLabel.gameObject.SetActive(value: false);
			_downloadTweenerPlayer.gameObject.SetActive(value: true);
			_downloadTweenerPlayer.Play(delegate
			{
				SetWorldOpenMode(WorldOpenMode.None);
			});
		}
		else
		{
			_indicatorLabel.gameObject.SetActive(value: true);
			_downloadTweenerPlayer.gameObject.SetActive(value: false);
		}
	}

	private void OnGestureZoomProcess(InputCommandMessage message)
	{
		Vector3 gestureVector = message.GestureVector;
		Durango.Utils.Singleton<MapContext>.Instance().Zoom(gestureVector.z, new Vector2(gestureVector.x, gestureVector.y));
		GameSystem<InputSystem>.Instance().Gesture.NotifyGestureProcessed();
		GameSystem<InputSystem>.Instance().StopPropagation();
	}

	private void SetWorldOpenMode(WorldOpenMode worldOpenMode)
	{
		if (_worldOpenMode != worldOpenMode)
		{
			_worldOpenMode = worldOpenMode;
			RefreshVoucherWidget();
			MapIndicators mapIndicators = Durango.Utils.Singleton<MapIndicators>.Instance();
			MapContext mapContext = Durango.Utils.Singleton<MapContext>.Instance();
			switch (_worldOpenMode)
			{
			case WorldOpenMode.None:
				ClearOpenModeDetail();
				SetInformationLabel();
				mapIndicators.HideTypesClear();
				mapContext.ShowPosLabel();
				ShowDownloadTweener(show: false);
				break;
			case WorldOpenMode.Warp:
				SetMapForWarp(PresetColor.UIDarkOrange, InfoType.SelectWarpHoleForEntry);
				HideIndicatorsWithoutWarpTargets(mapIndicators);
				mapContext.HidePosLabel();
				ShowDownloadTweener(show: false);
				break;
			case WorldOpenMode.Revive:
				SetMapForWarp(PresetColor.UILightGreen, InfoType.SelectWarpHoleForRevive);
				HideIndicatorsWithoutWarpTargets(mapIndicators);
				mapContext.HidePosLabel();
				ShowDownloadTweener(show: false);
				break;
			case WorldOpenMode.MapDownloading:
				ClearOpenModeDetail();
				Durango.Utils.Singleton<MapContext>.Instance().ZoomOut(toPlayer: false);
				SetInformationLabel(InfoType.None);
				mapIndicators.HideTypesClear();
				mapContext.HidePosLabel();
				ShowDownloadTweener(show: true);
				break;
			case WorldOpenMode.SharePos:
			case WorldOpenMode.SharePosToClan:
				ClearOpenModeDetail();
				SetInformationLabel(InfoType.PickForSharePos);
				mapIndicators.HideTypesClear();
				mapContext.ShowPosLabel();
				ShowDownloadTweener(show: false);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void ClearOpenModeDetail()
	{
		List<MapIndicator> indicators = Durango.Utils.Singleton<MapIndicators>.Instance().Indicators;
		int i = 0;
		for (int count = indicators.Count; i < count; i++)
		{
			if (indicators[i].Type == IndicatorType.Warphole)
			{
				Durango.Utils.Singleton<MapIndicators>.Instance().RemoveAreaEffectIndicator(indicators[i]);
			}
		}
		Durango.Utils.Singleton<MapIndicators>.Instance().ClearIndicatorLabels();
	}

	private void SetMapForWarp(Color colorEffect, InfoType infoType)
	{
		Durango.Utils.Singleton<MapContext>.Instance().ZoomOut(toPlayer: true);
		SetInformationLabel(infoType);
		Durango.Utils.Singleton<MapIndicators>.Instance().ClearIndicatorLabels();
		Connections.Frontend.Send(default(GetWarpCosts)).On(delegate(WarpCosts msg, PacketHeader _)
		{
			if (msg.Costs.Length <= 1 && _worldOpenMode != WorldOpenMode.Revive)
			{
				UIManager.SystemMsg(T._("워프하려면 워프홀을 두 곳 이상 찾아야 합니다."));
			}
			UpdateLatestWarpholeCosts(msg.Costs);
			UpdateWarpholeIndicatorLabelsAndEffects(colorEffect);
		});
	}

	private static void HideIndicatorsWithoutWarpTargets(MapIndicators inds)
	{
		IndicatorType[] array = Enums<IndicatorType>.All();
		for (int i = 0; i < array.Length; i++)
		{
			IndicatorType indicatorType = array[i];
			if (indicatorType != IndicatorType.Dock && indicatorType - 9 > IndicatorType.Returning && indicatorType != IndicatorType.LocalPlayer)
			{
				inds.Hide(array[i], hide: true);
			}
			else
			{
				inds.Hide(array[i], hide: false);
			}
		}
	}

	private void UpdateLatestWarpholeCosts(WarpCost[] warpCosts)
	{
		_latestWarpholeCosts.Clear();
		for (int i = 0; i < warpCosts.Length; i++)
		{
			WarpCost value = warpCosts[i];
			_latestWarpholeCosts[value.Tile] = value;
		}
	}

	private void UpdateWarpholeIndicatorLabelsAndEffects(Color availableWarpholeEffectColor)
	{
		List<MapIndicator> indicators = Durango.Utils.Singleton<MapIndicators>.Instance().Indicators;
		int i = 0;
		for (int count = indicators.Count; i < count; i++)
		{
			if (indicators[i].Type != IndicatorType.Warphole)
			{
				continue;
			}
			Point2 point = new Point2(indicators[i].GetTile());
			if ((!_warpEntryTile.HasValue || !(_warpEntryTile.Value == point)) && _latestWarpholeCosts.TryGetValue(point, out var value))
			{
				if (!value.Prohibited)
				{
					Durango.Utils.Singleton<MapIndicators>.Instance().AddIndicatorLabel(indicators[i], _warpCostIcon, value.Cost.ToString());
					Durango.Utils.Singleton<MapIndicators>.Instance().AddAreaEffectIndicator(indicators[i], availableWarpholeEffectColor, 32f, 0f, fixedScale: true);
				}
				else
				{
					Durango.Utils.Singleton<MapIndicators>.Instance().AddIndicatorLabel(indicators[i], _defendingWarphole, T._("사용 불가"));
				}
			}
		}
	}

	private void OnClickIndicator(MapIndicator ind)
	{
		switch (_worldOpenMode)
		{
		case WorldOpenMode.Warp:
			if (ind.Type == IndicatorType.Warphole)
			{
				Warp(new Point2(ind.GetTile()));
			}
			return;
		case WorldOpenMode.Revive:
			if (ind.Type == IndicatorType.Warphole)
			{
				Revive(new Point2(ind.GetTile()));
			}
			return;
		case WorldOpenMode.SharePos:
		case WorldOpenMode.SharePosToClan:
			ShareTouchedPosition();
			return;
		}
		MapFlagIndicator mapFlagIndicator = ind as MapFlagIndicator;
		if (mapFlagIndicator != null && !string.IsNullOrEmpty(mapFlagIndicator.ClanId))
		{
			ClanSystem.GetClanInfo(mapFlagIndicator.ClanId, delegate(Clan clan)
			{
				ClanInfoPopup clanInfoPopup = UIManager.Popup.Tooltip<ClanInfoPopup>();
				clanInfoPopup.Set(clan);
				clanInfoPopup.Show(ind.Widget, Vector2.zero, 3600f);
			});
		}
	}

	private void Warp(Point2 tile)
	{
		if ((_warpEntryTile.HasValue && _warpEntryTile.Value == tile) || !_latestWarpholeCosts.ContainsKey(tile))
		{
			return;
		}
		WarpCost warpCost = _latestWarpholeCosts[tile];
		if (warpCost.Prohibited)
		{
			UIManager.SystemMsg(T._("전쟁 기간이거나 점령되지 않은 화물 워프홀은 이용할 수 없습니다"));
			return;
		}
		UIManager.MessageBox.ShowPayConfirm(warpCost.Cost, Currency.TStone, T._("<em>워프</em> 하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				UIBase.CloseAllUI();
				GameSystem<MapSystem>.Instance().Warp(tile);
			}
		});
	}

	private void Revive(Point2 tile)
	{
		if (!_latestWarpholeCosts.ContainsKey(tile))
		{
			return;
		}
		WarpCost warpCost = _latestWarpholeCosts[tile];
		string comment = T._("지금 부활하면 아이템을 잃을 수 있습니다.\n(상점에서 구매한 아이템 포함)\n<em>부활</em> 하시겠습니까?");
		UIManager.MessageBox.ShowPayConfirm(warpCost.Cost, Currency.TStone, comment, delegate(bool ok)
		{
			if (ok)
			{
				UIBase.CloseAllUI();
				Durango.Utils.Singleton<PlayerController>.Instance().ResurrectionRequest(tile);
			}
		});
	}

	private void PurchaseMap()
	{
		OpenMapCost openMap = Yaml.Util.Singleton<CostsYaml>.Instance.OpenMap;
		bool free = openMap.HasVoucherFromCommodity();
		if (openMap.HasVoucher() || free)
		{
			string arg = T._("<em>{0}</em>의 <em>지도</em>를 구입하시겠습니까?", GameManager.Region.Name);
			string text = T._("[size=22][icon=icon_timer2] 섬 수명 {0}[/size]", GetIslandLifeTimeText());
			string subText = T._("[icon=icon_make_alert] 지형 정보가 공개되고, 크레이터와 군락의 위치가 표시되며, 모든 워프홀을 발견합니다.\n[icon=icon_make_alert] 크레이터는 닫힌 상태로 표시됩니다.\n[icon=icon_make_alert] 섬 수명이 끝날 때까지 적용됩니다.");
			if (GameManager.Region.RemaingTime < 86400.0)
			{
				text = $"<alert>{text}</alert>";
			}
			MessageBox messageBox = UIManager.MessageBox;
			if (free)
			{
				VoucherWithCommodity voucherFromCommodity = openMap.GetVoucherFromCommodity();
				string textFormat = T._("<em>{0}</em> 구매 기간 동안 하루에 {1}번 무료로 <em>지도 구입</em>을 할 수 있습니다.  <bar/>  <em>{2} 남음</em>");
				string packageEffectText = Vouchers.GetPackageEffectText(voucherFromCommodity.VoucherId, voucherFromCommodity.IncludingCommodityId, textFormat);
				messageBox.SetLowerText(packageEffectText);
			}
			string voucherId = openMap.GetVoucher().VoucherId;
			messageBox.SetVoucherInfo(voucherId);
			messageBox.Show($"{arg}\n{text}", subText, delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<MapSystem>.Instance().PurchaseMap((!free) ? voucherId : voucherId, _downloadTweenDuration);
				}
			}, GetPurchaseMapButtonText());
		}
		else
		{
			Durango.Utils.Singleton<UIManager>.Instance().OpenUri("Shop/Category/character");
		}
	}

	private void ToggleTeleport()
	{
		_isTeleportMode = !_isTeleportMode;
		for (int i = 0; i < _buttonInfos.Length; i++)
		{
			if (KUtility.GetSize(_buttonInfos[i]) != 0 && _buttonInfos[i][0].Type == ButtonType.Teleport)
			{
				_actionButtons[i].Selected = _isTeleportMode;
				break;
			}
		}
		if (_isTeleportMode)
		{
			UIManager.SystemMsg(T._("<em>화면을 터치</em>하면 해당 위치로 이동합니다."));
		}
	}

	private static void WarpBack()
	{
		MapSystem.RequestWarpBackCost(delegate(long cost)
		{
			RegionTile? lastReturnPoint = GameSystem<MapSystem>.Instance().Points.LastReturnPoint;
			if (lastReturnPoint.HasValue)
			{
				Region region = lastReturnPoint.Value.Region;
				RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(region.TemplateId);
				string comment = T._("{0} {1:lv:}\n탐험하던 섬으로 이동합니다.", region.Name, regionTemplate?.Level ?? 0);
				UIManager.MessageBox.ShowPayConfirm(cost, Currency.TStone, comment, delegate(bool ok)
				{
					if (ok)
					{
						UIBase.CloseAllUI();
						GameSystem<MapSystem>.Instance().WarpBack(region);
					}
				});
			}
		});
	}

	private static bool IsValidRegion(Region region)
	{
		RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(region.TemplateId);
		if (regionTemplate == null)
		{
			return false;
		}
		if (regionTemplate.ExpiresIn > 0.0)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			if (region.CreatedAt + regionTemplate.ExpiresIn < predictedServerTime)
			{
				return false;
			}
		}
		return true;
	}

	private void AddInteractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().RegisterContextActionFinder(ContextActionFinder);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.Warp, delegate(InteractionObject interactionObject)
		{
			if (interactionObject == null)
			{
				OpenForWarp(null);
			}
			else
			{
				ImmovableBase targetComponent = interactionObject.GetTargetComponent<ImmovableBase>();
				if (!(targetComponent == null))
				{
					string entityId = targetComponent.EntityId;
					Point2 worldTile = targetComponent.WorldTile;
					Connections.Frontend.Send(new IsWarpholeAvailable
					{
						EntityId = entityId,
						Tile = worldTile
					}).On<OK>(delegate
					{
						OpenForWarp(null);
					});
				}
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.WarpToPort, delegate
		{
			WarpToPort();
		});
	}

	private void ContextActionFinder(List<InteractionMenuData> actions)
	{
		switch (GameManager.Region.Role())
		{
		case Role.Rural:
		case Role.Outpost:
		case Role.Urban:
			if (!PlayerBehavior.LocalPlayer.IsMoving)
			{
				actions.Add(InteractionData.Interaction.WarpToPort);
			}
			break;
		case Role.Personal:
			if (!PlayerBehavior.LocalPlayer.IsMoving)
			{
				actions.Add(InteractionData.Interaction.WarpToPort);
			}
			return;
		}
		EstateInfo estateInfo = EstateSystem.GetEstateInfo(PlayerBehavior.LocalPlayer.CurrentTile);
		if (estateInfo != null && !(estateInfo.License.OwnerId != PlayerBehavior.LocalPlayer.EntityId))
		{
			actions.Add(InteractionData.Interaction.Warp);
		}
	}

	public static void ShareCurrentPos(ChannelType? channelType = null, string conversationId = null)
	{
		Durango.Utils.Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(PlayerBehavior.LocalPlayer.EntityId, delegate(Durango.Player.PlayerInfo info)
		{
			if (info.Valid)
			{
				Point2 currentTile = PlayerBehavior.LocalPlayer.CurrentTile;
				Durango.Utils.Singleton<MapIndicators>.Instance().AddAnnounceBalloon(AnnounceType.SharePinPoint, new Vector2(currentTile.x, currentTile.y), info);
				GameSystem<SocialSystem>.Instance().SystemSay(new RadioPin
				{
					RegionId = GameManager.Region.Id,
					Tile = currentTile
				}, channelType, conversationId);
			}
		});
	}

	public static string GetIslandLifeTimeText()
	{
		double remaingTime = GameManager.Region.RemaingTime;
		if (remaingTime <= 0.0)
		{
			return "-";
		}
		if (remaingTime < 60.0)
		{
			return T._("곧 파괴됨");
		}
		return TimedeltaFormatter.Format(remaingTime, 2, "min");
	}
}
