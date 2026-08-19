using System;
using System.Collections.Generic;
using ClanData;
using Estate;
using InteractionData;
using ItemSystem;
using K1Network;
using L10N;
using MapData;
using Messages;
using Player;
using Shared.Chat;
using Shared.Economy;
using Shared.Region;
using Shared.System;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class WorldMapGroup : UIBase
{
	private enum ButtonStatus
	{
		NotVisible,
		Enabled,
		Disabled
	}

	private struct ButtonStruct
	{
		public string Text;

		public int Height;

		public Action Function;

		public Func<ButtonStatus> StatusFunc;
	}

	private enum WorldOpenMode
	{
		None,
		Warp,
		Revive,
		SharePos
	}

	[SerializeField]
	private Transform _attachDock;

	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private ListObjectPool _actionButtons;

	[SerializeField]
	private GameObject _worldMapBG;

	[SerializeField]
	private UIWidget _exploredWarpholeCountWidget;

	[SerializeField]
	private UILabel _exploredWarpholeCountLabel;

	[SerializeField]
	private UIWidget _infoContainer;

	[SerializeField]
	private UILabel _informationLabel;

	[SerializeField]
	private ListObjectPool _clanWarStates;

	[SerializeField]
	private WorldMapScaleInfo _scaleInfo;

	[SerializeField]
	private WorldMapEnvWidget _mapEnvironmentWidget;

	[SerializeField]
	private UISpriteLabel _indicatorLabel;

	[SerializeField]
	private SpriteData _warpCostIcon;

	[SerializeField]
	private int _buttonMinWidth;

	[SerializeField]
	private Vector2 _buttonPadding;

	[SerializeField]
	private int _buttonMargin;

	private ButtonStruct[] _buttonInfos;

	private WorldOpenMode _worldOpenMode;

	private Point2? _warpEntryTile;

	private Vector2? _openFocusPosition;

	public Transform AttackDock => _attachDock;

	private void Start()
	{
		_buttonInfos = new ButtonStruct[4]
		{
			new ButtonStruct
			{
				Text = T._("[icon=icon_house] 기반섬 집으로 귀환"),
				Height = 80,
				Function = ReturnToHome,
				StatusFunc = delegate
				{
					Role role2 = TerrainMeta.Role;
					return (role2 != Role.Tutorial && role2 != Role.Bootcamp) ? ButtonStatus.Enabled : ButtonStatus.NotVisible;
				}
			},
			new ButtonStruct
			{
				Text = T._("[icon=icon_house] 전초기지섬으로 귀환"),
				Height = 80,
				Function = ReturnToBase,
				StatusFunc = delegate
				{
					Role role = TerrainMeta.Role;
					if (role == Role.Tutorial || role == Role.Bootcamp)
					{
						return ButtonStatus.NotVisible;
					}
					EntityTile? basePoint = GameSystem<MapSystem>.Instance().Points.BasePoint;
					return (!basePoint.HasValue) ? ButtonStatus.Disabled : (IsValidRegion(basePoint.Value.Region) ? ButtonStatus.Enabled : ButtonStatus.Disabled);
				}
			},
			new ButtonStruct
			{
				Text = T._("[icon=icon_map_warphole:1.5] 모험을 계속"),
				Height = 80,
				Function = WarpBack,
				StatusFunc = WarpBackStatusFunction
			},
			new ButtonStruct
			{
				Text = T._("[E42222FF][icon=icon_map_pinpoint:1.2][-] 위치 공유"),
				Height = 80,
				Function = SharePosition,
				StatusFunc = () => CanSharePosition() ? ButtonStatus.Enabled : ButtonStatus.NotVisible
			}
		};
		UIEventListener.Get(_closeButton).onClick = delegate
		{
			ForceClose();
		};
		base.OnOpenSucceed += delegate
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			MapSystem_WarpholesUpdated();
			if (_openFocusPosition.HasValue)
			{
				FocusToPosition(_openFocusPosition.Value);
				_openFocusPosition = null;
			}
			else
			{
				Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
				FocusToPosition(TerrainA6.ClientPositionToTilePosition(currentPosition));
			}
			KSingleton<PlayerController>.Instance().IsGestureProcessed += PlayerController_IsGestureProcessed;
			GameSystem<MapSystem>.Instance().WarpholesUpdated += MapSystem_WarpholesUpdated;
			KSingleton<MapIndicators>.Instance().IndicatorClicked += OnClickIndicator;
			KSingleton<MapIndicators>.Instance().IndicatorDraged += DragWorldMap;
			UIManager.MapContext.ZoomChanged += MapContext_ZoomChanged;
			GameSystem<ClanSystem>.Instance().EnemyClansDirtied += UpdateEnemyClans;
			UpdateEnemyClans();
			RefreshScaleInfo();
		};
		base.OnCloseSucceed += delegate
		{
			MinimapGroup minimapGroup = UIManager.FindScript<MinimapGroup>();
			minimapGroup.AttachMapContext();
			SetWorldOpenMode(WorldOpenMode.None);
			KSingleton<MapIndicators>.Instance().HideToolTipLabel();
			KSingleton<PlayerController>.Instance().IsGestureProcessed -= PlayerController_IsGestureProcessed;
			GameSystem<MapSystem>.Instance().WarpholesUpdated -= MapSystem_WarpholesUpdated;
			KSingleton<MapIndicators>.Instance().IndicatorClicked -= OnClickIndicator;
			KSingleton<MapIndicators>.Instance().IndicatorDraged -= DragWorldMap;
			UIManager.MapContext.ZoomChanged -= MapContext_ZoomChanged;
			GameSystem<ClanSystem>.Instance().EnemyClansDirtied -= UpdateEnemyClans;
		};
		UIEventListener uIEventListener = UIEventListener.Get(_worldMapBG);
		uIEventListener.onDrag = OnDragWorldMap;
		uIEventListener.onClick = OnClickWorldMap;
		uIEventListener.onDoubleClick = delegate
		{
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			MapContext mapContext = UIManager.MapContext;
			UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
			if (Debug.isDebugBuild && UICamera.CountInputSources() >= 2)
			{
				Vector2 val = mapContext.ScreenPosToTilePos(currentTouch.pos);
				int x = (int)val.x;
				int y = (int)val.y;
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
				float zoomScale = mapContext.ZoomScale;
				float num = zoomScale + 0.5f;
				num = (float)(int)(num / 0.5f) * 0.5f;
				mapContext.Zoom(num - zoomScale, currentTouch.pos);
			}
		};
		((Component)_mapEnvironmentWidget).gameObject.SetActive(true);
		_indicatorLabel.text = LocalizeSystem.Get("#map_indicator_helper_label");
		SetInformationLabel(null);
		OnClose();
		AddInteractionHandler();
	}

	public bool HasOneOrMoreWarpHoles()
	{
		List<MapIndicator> indicators = MapIndicators.Indicators;
		int i = 0;
		for (int count = indicators.Count; i < count; i++)
		{
			if (indicators[i].Type == IndicatorType.Warphole)
			{
				return true;
			}
		}
		return false;
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

	public void OpenForAnnounceBalloon(AnnounceType type, Vector2 posPinPoint, ulong entityId)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(Player.PlayerInfo info)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (info.Valid)
			{
				KSingleton<MapIndicators>.Instance().AddAnnounceBalloon(type, posPinPoint, info);
				_openFocusPosition = posPinPoint;
				Open();
			}
		});
	}

	protected override bool OnOpen()
	{
		UIManager.MapContext.Attach(worldMapMode: true, AttackDock);
		UpdateButtons();
		return base.OnOpen();
	}

	private void UpdateButtons()
	{
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		_actionButtons.Clear();
		float num = 0f;
		for (int i = 0; i < _buttonInfos.Length; i++)
		{
			ButtonStruct buttonStruct = _buttonInfos[i];
			ButtonStatus buttonStatus = ((buttonStruct.StatusFunc == null) ? ButtonStatus.Enabled : buttonStruct.StatusFunc());
			if (buttonStatus != 0)
			{
				DefaultSelectableButton defaultSelectableButton = ((ListObjectPoolBase<GameObject>)_actionButtons).Add<DefaultSelectableButton>();
				defaultSelectableButton.Text = buttonStruct.Text;
				num = Mathf.Max(num, defaultSelectableButton.TextLabel.printedSize.x);
				defaultSelectableButton.Widget.height = (int)Mathf.Max((float)buttonStruct.Height, (float)defaultSelectableButton.TextLabel.fontSize + _buttonPadding.y * 2f);
				defaultSelectableButton.Disable = buttonStatus == ButtonStatus.Disabled;
				defaultSelectableButton.Clicked = ((!defaultSelectableButton.Disable) ? buttonStruct.Function : null);
			}
		}
		int width = (int)Mathf.Max((float)_buttonMinWidth, num + _buttonPadding.x * 2f);
		Vector3 position = _actionButtons.BaseObject.GetComponent<UIWidget>().GetPosition(1f, 0f);
		for (int j = 0; j < _actionButtons.Count; j++)
		{
			DefaultSelectableButton defaultSelectableButton2 = ((ListObjectPoolBase<GameObject>)_actionButtons).Get<DefaultSelectableButton>(j);
			defaultSelectableButton2.Widget.width = width;
			defaultSelectableButton2.Widget.SetPosition(position, 1f, 0f);
			position.y += (float)(defaultSelectableButton2.Widget.height + _buttonMargin);
		}
		position.y += 10f;
		_exploredWarpholeCountWidget.width = width;
		_exploredWarpholeCountWidget.SetPosition(position, 1f, 0f);
	}

	private void SetInformationLabel(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			((Component)_informationLabel).gameObject.SetActive(false);
			_infoContainer.alpha = 1f;
		}
		else
		{
			((Component)_informationLabel).gameObject.SetActive(true);
			_infoContainer.alpha = 0f;
			_informationLabel.text = text;
		}
	}

	private void MapSystem_WarpholesUpdated()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (GameSystem<MapSystem>.Instance().GetExploredWarpholeCount() > 0)
		{
			((Component)((Component)_exploredWarpholeCountLabel).transform.parent).gameObject.SetActive(true);
			int exploredWarpholeCount = GameSystem<MapSystem>.Instance().GetExploredWarpholeCount();
			_exploredWarpholeCountLabel.text = string.Format("{2}{0}[-] {3}/[-] {1}", exploredWarpholeCount, GameSystem<MapSystem>.Instance().EntireWarpholeCount, UIManager.ColorBBCode(UIManager.UIYellow), UIManager.ColorBBCode(UIManager.UILightGray));
		}
		else
		{
			((Component)((Component)_exploredWarpholeCountLabel).transform.parent).gameObject.SetActive(false);
		}
	}

	private void ReturnToHome()
	{
		EntityTile? homePoint = GameSystem<MapSystem>.Instance().Points.HomePoint;
		Region region = ((!homePoint.HasValue) ? GameSystem<MapSystem>.Instance().Points.ReturningPoint.Region : homePoint.Value.Region);
		RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(region.TemplateId);
		string comment = T._("{0}{1:lv:}\n기반섬으로 워프합니다.", region.Name, regionTemplate.level);
		UIManager.MessageBox.Show(comment, delegate(bool ok)
		{
			if (ok)
			{
				if (PlayerBehavior.LocalPlayer.IsAlive)
				{
					UIManager.SystemMsg(T._("워프 중입니다. 움직이거나 공격을 받으면 취소됩니다."), 3f);
					UIBase.CloseAllUI();
					GameSystem<MapSystem>.Instance().ReturnToHome();
				}
				else
				{
					UIManager.SystemMsg(T._("죽었을 때는 워프 할 수 없습니다."));
				}
			}
		});
	}

	private void ReturnToBase()
	{
		EntityTile? basePoint = GameSystem<MapSystem>.Instance().Points.BasePoint;
		if (!basePoint.HasValue)
		{
			return;
		}
		Region region = basePoint.Value.Region;
		RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(region.TemplateId);
		string comment = T._("{0}{1:lv:}\n전초기지섬으로 워프합니다.", region.Name, regionTemplate.level);
		UIManager.MessageBox.Show(comment, delegate(bool ok)
		{
			if (ok)
			{
				if (PlayerBehavior.LocalPlayer.IsAlive)
				{
					UIManager.SystemMsg(T._("워프 중입니다. 움직이거나 공격을 받으면 취소됩니다."), 3f);
					UIBase.CloseAllUI();
					GameSystem<MapSystem>.Instance().ReturnToBase();
				}
				else
				{
					UIManager.SystemMsg(T._("죽었을 때는 워프 할 수 없습니다."));
				}
			}
		});
	}

	private void FocusToPosition(Vector2 tilePos)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		MapContext mapContext = UIManager.MapContext;
		int num = TerrainMeta.TileCount / 2;
		tilePos.x -= (float)num;
		tilePos.y -= (float)num;
		Vector2 val = tilePos / (float)mapContext.MapSize * (float)mapContext.MapNGUISize;
		float num2 = Mathf.Sin((float)Math.PI / 4f);
		float num3 = Mathf.Cos((float)Math.PI / 4f);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(val.x * num3 - val.y * num2, val.x * num2 + val.y * num3);
		val2 *= mapContext.ZoomScale;
		mapContext.Offset = -val2;
	}

	private void OnClickWorldMap(GameObject obj)
	{
		if (_worldOpenMode == WorldOpenMode.SharePos)
		{
			ShareTouchedPositionToClan();
		}
	}

	private void ShareTouchedPositionToClan()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!CanSharePosition())
		{
			SetWorldOpenMode(WorldOpenMode.None);
			return;
		}
		UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
		if (currentTouch == null)
		{
			return;
		}
		Vector2 tilePos = UIManager.MapContext.ScreenPosToTilePos(currentTouch.pos);
		tilePos.x = Mathf.Round(tilePos.x);
		tilePos.y = Mathf.Round(tilePos.y);
		UIManager.MessageBox.Show(T._("선택한 위치를 부족에게 공유하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<SocialSystem>.Instance().Ping(ChannelType.Clan, KSingleton<GameManager>.Instance().Region.Id, new Point2((int)tilePos.x, (int)tilePos.y));
				KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(PlayerBehavior.LocalPlayer.EntityId, delegate(Player.PlayerInfo info)
				{
					//IL_0012: Unknown result type (might be due to invalid IL or missing references)
					if (info.Valid)
					{
						KSingleton<MapIndicators>.Instance().AddAnnounceBalloon(AnnounceType.SharePinPoint, tilePos, info);
					}
				});
			}
			SetWorldOpenMode(WorldOpenMode.None);
		});
	}

	private void OnDragWorldMap(GameObject obj, Vector2 delta)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		DragWorldMap(delta);
	}

	private void DragWorldMap(Vector2 delta)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		MapContext mapContext = UIManager.MapContext;
		mapContext.Offset += delta;
	}

	private void MapContext_ZoomChanged()
	{
		RefreshScaleInfo();
	}

	private void RefreshScaleInfo()
	{
		_scaleInfo.Refresh(UIManager.MapContext.ZoomScale, (float)TerrainMeta.TileCount * 200f / 100f / 1280f);
	}

	private void PlayerController_IsGestureProcessed(PlayerController.Gesture type, Vector3 pos, bool touchedUI, ref bool result)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (type == PlayerController.Gesture.Zoom)
		{
			UIManager.MapContext.Zoom(pos.z, new Vector2(pos.x, pos.y));
		}
		result = true;
	}

	private void SetWorldOpenMode(WorldOpenMode worldOpenMode)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (_worldOpenMode != worldOpenMode)
		{
			_worldOpenMode = worldOpenMode;
			switch (_worldOpenMode)
			{
			case WorldOpenMode.None:
				ClearOpenModeDetail();
				SetInformationLabel(null);
				break;
			case WorldOpenMode.Warp:
				SetMapForWarp(PresetColor.UIDarkOrange, T._("진입할 워프홀을 선택하세요."));
				break;
			case WorldOpenMode.Revive:
				SetMapForWarp(PresetColor.UILightGreen, T._("부활할 워프홀을 선택하세요."));
				break;
			case WorldOpenMode.SharePos:
				SetMapForSharePosition(T._("공유할 위치를 선택하세요."));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void ClearOpenModeDetail()
	{
		List<MapIndicator> indicators = MapIndicators.Indicators;
		int i = 0;
		for (int count = indicators.Count; i < count; i++)
		{
			if (indicators[i].Type == IndicatorType.Warphole)
			{
				KSingleton<MapIndicators>.Instance().RemoveAreaEffectIndicator(indicators[i]);
			}
		}
		KSingleton<MapIndicators>.Instance().ClearIndicatorLabels();
	}

	private void SetMapForSharePosition(string textInformation)
	{
		ClearOpenModeDetail();
		SetInformationLabel(textInformation);
	}

	private void SetMapForWarp(Color colorEffect, string warpInformation)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		MapContext mapContext = UIManager.MapContext;
		List<MapIndicator> indicators = MapIndicators.Indicators;
		int num = 0;
		int i = 0;
		for (int count = indicators.Count; i < count; i++)
		{
			if (indicators[i].Type == IndicatorType.Warphole && (!_warpEntryTile.HasValue || !(_warpEntryTile.Value == new Point2(indicators[i].GetTile()))))
			{
				KSingleton<MapIndicators>.Instance().AddAreaEffectIndicator(indicators[i], colorEffect, 32f, 0f, fixedScale: true);
				num++;
			}
		}
		mapContext.SetIslandView();
		mapContext.Offset += Vector2.down * 50f;
		SetInformationLabel(warpInformation);
		if (num == 0)
		{
			UIManager.SystemMsg(T._("워프하려면 워프홀을 두 곳 이상 찾아야 합니다."));
		}
		KSingleton<MapIndicators>.Instance().ClearIndicatorLabels();
		Connections.Frontend.Send(default(GetWarpCosts)).On(delegate(WarpCosts msg, PacketHeader _)
		{
			AddWarpCostLabels(msg.Costs);
		});
	}

	private void AddWarpCostLabels(WarpCost[] warpCosts)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<Point2, int> dictionary = new Dictionary<Point2, int>();
		for (int i = 0; i < warpCosts.Length; i++)
		{
			WarpCost warpCost = warpCosts[i];
			dictionary[warpCost.Tile] = warpCost.Cost;
		}
		List<MapIndicator> indicators = MapIndicators.Indicators;
		int j = 0;
		for (int count = indicators.Count; j < count; j++)
		{
			if (indicators[j].Type == IndicatorType.Warphole)
			{
				Point2 point = new Point2(indicators[j].GetTile());
				if ((!_warpEntryTile.HasValue || !(_warpEntryTile.Value == point)) && dictionary.TryGetValue(point, out var value))
				{
					KSingleton<MapIndicators>.Instance().AddIndicatorLabel(indicators[j], _warpCostIcon, value.ToString());
				}
			}
		}
	}

	private void OnClickIndicator(MapIndicator ind)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		switch (_worldOpenMode)
		{
		case WorldOpenMode.Warp:
			if (ind.Type == IndicatorType.Warphole)
			{
				Warp(new Point2(ind.GetTile()));
			}
			break;
		case WorldOpenMode.Revive:
			if (ind.Type == IndicatorType.Warphole)
			{
				Revive(new Point2(ind.GetTile()));
			}
			break;
		case WorldOpenMode.SharePos:
			ShareTouchedPositionToClan();
			break;
		}
	}

	private void Warp(Point2 tile)
	{
		if (_warpEntryTile.HasValue && _warpEntryTile.Value == tile)
		{
			return;
		}
		GameSystem<MapSystem>.Instance().RequestWarpCost(tile, delegate(int cost)
		{
			string comment = string.Format("{0}\n{1}", T._("[FFD85B]워프[-] 하시겠습니까?"), ItemSystem.Inventory.CurrencyFormat(cost, Currency.TStone));
			UIManager.MessageBox.Show(comment, delegate(bool ok)
			{
				if (ok)
				{
					UIBase.CloseAllUI();
					GameSystem<MapSystem>.Instance().Warp(tile);
				}
			});
		});
	}

	private void Revive(Point2 tile)
	{
		GameSystem<MapSystem>.Instance().RequestWarpCost(tile, delegate(int cost)
		{
			string comment = string.Format("{0}\n{1}\n{2}", T._("지금 부활하면 아이템을 잃을 수 있습니다."), T._("[FFD85B]부활[-] 하시겠습니까?"), ItemSystem.Inventory.CurrencyFormat(cost, Currency.TStone));
			UIManager.MessageBox.Show(comment, delegate(bool ok)
			{
				if (ok)
				{
					UIBase.CloseAllUI();
					KSingleton<PlayerController>.Instance().ResurrectionRequest(tile);
				}
			});
		});
	}

	private ButtonStatus WarpBackStatusFunction()
	{
		switch (TerrainMeta.Role)
		{
		case Role.Tutorial:
		case Role.Bootcamp:
		case Role.Risky:
			return ButtonStatus.NotVisible;
		default:
		{
			RegionTile? lastReturnPoint = GameSystem<MapSystem>.Instance().Points.LastReturnPoint;
			return (lastReturnPoint.HasValue && IsValidRegion(lastReturnPoint.Value.Region)) ? ButtonStatus.Enabled : ButtonStatus.Disabled;
		}
		}
	}

	private void WarpBack()
	{
		GameSystem<MapSystem>.Instance().RequestWarpBackCost(delegate(int cost)
		{
			RegionTile? lastReturnPoint = GameSystem<MapSystem>.Instance().Points.LastReturnPoint;
			if (lastReturnPoint.HasValue)
			{
				Region region = lastReturnPoint.Value.Region;
				RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(region.TemplateId);
				string comment = T._("{0}{1:lv:}\n귀환했던 섬으로 워프합니다\n{2}", region.Name, regionTemplate.level, ItemSystem.Inventory.CurrencyFormat(cost, Currency.TStone));
				UIManager.MessageBox.Show(comment, delegate(bool ok)
				{
					if (ok)
					{
						UIBase.CloseAllUI();
						GameSystem<MapSystem>.Instance().WarpBack();
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
		if (regionTemplate.expires_in > 0.0)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			if (region.CreatedAt + regionTemplate.expires_in < predictedServerTime)
			{
				return false;
			}
		}
		return true;
	}

	private void SharePosition()
	{
		SetWorldOpenMode(WorldOpenMode.SharePos);
	}

	private static bool CanSharePosition()
	{
		return PlayerBehavior.LocalPlayer.ClanId != 0;
	}

	private void UpdateEnemyClans()
	{
		ulong[] enemyClanIds = GameSystem<ClanSystem>.Instance().EnemyClanIds;
		_clanWarStates.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(enemyClanIds); i < size; i++)
		{
			ClanSystem.GetClanInfo(enemyClanIds[i], OnEnemyClan);
		}
	}

	private void OnEnemyClan(Clan clan)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = _clanWarStates.Add();
		UISpriteLabel component = ((Component)val.transform.FindChild("text")).GetComponent<UISpriteLabel>();
		SyncString synsString = new SyncString(delegate(out string text, out float period)
		{
			clan.GetClanWarState(out var state, out var remain);
			if (state == ClanData.ClanWarState.None)
			{
				text = clan.Name;
			}
			else
			{
				text = string.Format("[icon={0}] <em>{1}</em> {2}", IconMap.Get(state), clan.Name, T._("{0:sec:}", remain));
			}
			period = 1f;
		});
		LabelUpdater.Set(component, synsString);
		_clanWarStates.Reposition(Vector3.up, 5);
	}

	private void AddInteractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().RegisterContextActionFinder(ContextActionFinder);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Shared.System.Interaction.Warp, delegate(InteractionObject obj)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			OpenForWarp(new Point2(obj.Tile));
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.Warp, delegate
		{
			OpenForWarp(null);
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(InteractionData.Interaction.WarpBack, delegate
		{
			WarpBack();
		});
	}

	private void ContextActionFinder(ref List<InteractionData.Interaction> actions)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		TileObject tileObject = TerrainA6.GetTileObject(new Point2(TerrainA6.ClientPositionToTilePosition(localPlayer.CurrentPosition)), warning: false);
		if (tileObject == null || tileObject.EstateId == 0L)
		{
			return;
		}
		Estate.EstateInfo estateInfo = GameSystem<EstateSystem>.Instance().GetEstateInfo(tileObject.EstateId);
		if (estateInfo != null && estateInfo.IsValid() && estateInfo.Owner == localPlayer.EntityId)
		{
			actions.Add(InteractionData.Interaction.Warp);
			ButtonStatus buttonStatus = WarpBackStatusFunction();
			if (buttonStatus == ButtonStatus.Enabled)
			{
				actions.Add(InteractionData.Interaction.WarpBack);
			}
		}
	}
}
