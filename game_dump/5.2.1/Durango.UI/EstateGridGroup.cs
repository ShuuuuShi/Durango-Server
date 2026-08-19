using System;
using System.Collections.Generic;
using Durango.Logic.Estate;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.UI.Control;
using Durango.UI.InGame;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Estate;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class EstateGridGroup : UIBase
{
	[Serializable]
	private struct EstateEffect
	{
		public ParticleType Particle;

		public SoundEventType[] Audios;

		public string Motion;
	}

	private enum EstateEffectType
	{
		PrivateDeclare,
		PrivateExpand,
		ClanDeclare,
		ClanExpand
	}

	private const float GridZoom = 0.252f;

	private static readonly Point2[] Directions = new Point2[4]
	{
		Point2.right,
		Point2.up,
		Point2.left,
		Point2.down
	};

	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private GameObject _backButton;

	[SerializeField]
	private UIWidget _sizeWidget;

	[SerializeField]
	private UILabel _sizeLabel;

	[SerializeField]
	private UISprite _currencyIcon;

	[SerializeField]
	private UILabel _currencyLabel;

	[SerializeField]
	private GameObject _touchBox;

	[SerializeField]
	private UIWidget _resetWidget;

	[SerializeField]
	private Selectable _resetButton;

	[SerializeField]
	private UILabel _resetLabel;

	[SerializeField]
	[EnumList(typeof(EstateEffectType), false, 0, -1)]
	private EstateEffect[] _estateEffects;

	private readonly HashSet<Point2> _unitSet = new HashSet<Point2>();

	private readonly List<GridAreaBase> _areaList = new List<GridAreaBase>();

	private OwnerType _declareOwnerType;

	private Point2 _selectedUnit;

	private EstateInfo _expandEstate;

	private int _largestSize;

	private bool _isZoomOut;

	private float _prevZoom;

	protected override bool IsSoundOcclusion => false;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_closeButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			UIBase.CloseAllUI();
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(_backButton);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			Close();
		});
		UIEventListener uIEventListener3 = UIEventListener.Get(_touchBox);
		uIEventListener3.onDrag = UIManager.IgnoreUIDrag;
		uIEventListener3.onClick = OnSelectGrid;
		SetChildrenActive(activated: false);
		_resetButton.Clicked = delegate
		{
			UIManager.MessageBox.Show(T._("개인섬을 초기화 합니다."), T._("[icon=icon_make_alert] 개인섬 초기화는 하루에 한번만 가능합니다.\n[icon=icon_make_alert] 선택된 영역이 초기화 됩니다. 건물은 사라지지 않습니다."), delegate(bool ok)
			{
				if (ok && _expandEstate != null)
				{
					UIBase.CloseAllUI();
					EstateSystem.RemoveEstate(_expandEstate.Id);
				}
			}, T._("초기화"));
		};
	}

	private void OnUpdateEstateGrid()
	{
		if (base.IsOpened)
		{
			Refresh();
		}
	}

	private void RefreshGrid()
	{
		if (_expandEstate == null)
		{
			RefreshDeclareGrid();
		}
		else
		{
			RefreshExpandGrid();
		}
		_touchBox.SetActive(_expandEstate == null);
	}

	private void SetZoomOutMode(bool enable)
	{
		if (_isZoomOut != enable)
		{
			_isZoomOut = enable;
			if (enable)
			{
				_prevZoom = Durango.Utils.Singleton<MainCamera>.Instance().Zoom;
				Durango.Utils.Singleton<CameraController>.Instance().ZoomRange(0.252f, 0.252f, 0.3f).Zoom(0.252f, 0.3f)
					.LockZoomControl(isLock: true);
			}
			else
			{
				Durango.Utils.Singleton<CameraController>.Instance().ZoomRange(0.42f, 2.2f, 0.3f).Zoom(_prevZoom, 0.3f)
					.LockZoomControl(isLock: false);
			}
		}
	}

	private void RefreshExpandGrid()
	{
		_unitSet.Clear();
		int i = 0;
		for (int count = _expandEstate.Units.Count; i < count; i++)
		{
			Point2 item = _expandEstate.Units[i];
			_unitSet.Add(item);
		}
		AddExpandButtons();
		if (_expandEstate.License.Type != OwnerType.PersonalPlayer)
		{
			AddShrinkButtons();
		}
		Durango.Utils.Singleton<GridAreaViewer>.Instance().Show(_areaList);
	}

	private void AddExpandButtons()
	{
		int size = _expandEstate.License.Size;
		_largestSize = Mathf.Max(size, _largestSize);
		EstateLicense license = _expandEstate.License;
		OwnerType type = license.Type;
		string text = ((type != OwnerType.ClanEstate && type != OwnerType.ClanWarphole) ? Durango.Logic.Item.Inventory.GetIcon(Currency.TStone) : "tstone_icon_clan");
		_areaList.Clear();
		string buttonText = ((size >= _largestSize) ? Durango.Logic.Item.Inventory.CurrencyFormat(Yaml.Util.Singleton<CostsYaml>.Instance.Estate.GetExpandingCost(license.Type, license.Size), text) : T._("확장 {0}무료!", "[c][ffffff][icon=" + text + "][-][/c]"));
		Color buttonColor = Color.white;
		PresetButton.Style buttonStyle = PresetButton.Style.Solid;
		if (_expandEstate.License.Type == OwnerType.PersonalPlayer)
		{
			if (size < _largestSize)
			{
				buttonText = T._("확장");
			}
			else
			{
				buttonText = T._("확장 불가");
				buttonColor = PresetColor.UILightRed;
				buttonStyle = PresetButton.Style.Border;
			}
		}
		foreach (Point2 item in _unitSet)
		{
			for (int i = 0; i < Directions.Length; i++)
			{
				Point2 point = item + Directions[i];
				Point2 point2 = point * 4;
				if (EstateSystem.GetEstateInfo(point2) != null || _unitSet.Contains(point))
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < _areaList.Count; j++)
				{
					if (_areaList[j].Tile == point2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					_areaList.Add(new RectGridArea
					{
						Tile = point2,
						Size = Point2.one * 4,
						BgColor = Color.white,
						ButtonText = buttonText,
						ButtonStyle = buttonStyle,
						ButtonColor = buttonColor,
						OnSelect = OnExpandEstateClick
					});
				}
			}
		}
	}

	private void AddShrinkButtons()
	{
		string buttonText = T._("축소");
		foreach (Point2 item2 in _unitSet)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			bool flag = false;
			do
			{
				Point2 item = item2 + Directions[num2];
				if (num2 != num3)
				{
					item += Directions[num3];
				}
				bool flag2 = _unitSet.Contains(item);
				if (flag2 != flag)
				{
					if (flag)
					{
						num++;
					}
					flag = flag2;
				}
				if (num2 == num3)
				{
					num3 = (num3 + 1) % Directions.Length;
				}
				else
				{
					num2 = (num2 + 1) % Directions.Length;
				}
			}
			while (num2 != 0 || num3 != 0);
			if (num <= 1)
			{
				_areaList.Add(new RectGridArea
				{
					Tile = item2 * 4,
					Size = Point2.one * 4,
					BgColor = Color.white,
					ButtonText = buttonText,
					ButtonStyle = PresetButton.Style.Border,
					ButtonColor = PresetColor.UILightRed,
					OnSelect = OnShrinkEstateClick
				});
			}
		}
	}

	private void RefreshDeclareGrid()
	{
		_areaList.Clear();
		Point2 tileOffset = Durango.Utils.Singleton<GridAreaViewer>.Instance().GetTileOffset();
		tileOffset /= 4;
		OwnerType declareOwnerType = _declareOwnerType;
		string icon = ((declareOwnerType != OwnerType.ClanEstate && declareOwnerType != OwnerType.ClanWarphole) ? Durango.Logic.Item.Inventory.GetIcon(Currency.TStone) : "tstone_icon_clan");
		for (int i = 0; i < 12; i++)
		{
			for (int j = 0; j < 12; j++)
			{
				Point2 point = tileOffset + new Point2(i, j);
				bool flag = EstateSystem.GetEstateInfo(point * 4) == null;
				RectGridArea rectGridArea = new RectGridArea
				{
					Tile = point * 4,
					Size = Point2.one * 4
				};
				rectGridArea.BgColor = ((!flag) ? PresetColor.UILightRed : PresetColor.UILightGreen);
				if (point == _selectedUnit)
				{
					rectGridArea.BgColor = PresetColor.UIYellow;
					rectGridArea.ButtonStyle = PresetButton.Style.Solid;
					rectGridArea.ButtonColor = Color.white;
					long num = Yaml.Util.Singleton<Constants>.Instance.Estate.ActivationPeriod.Get(_declareOwnerType, 0) * Yaml.Util.Singleton<CostsYaml>.Instance.Estate.GetExtendingCost(_declareOwnerType, 1);
					rectGridArea.ButtonText = ((num != 0L) ? T._("선언 {0}", Durango.Logic.Item.Inventory.CurrencyFormat(num, icon)) : T._("선언"));
					rectGridArea.OnSelect = OnDeclareEstateClick;
				}
				_areaList.Add(rectGridArea);
			}
		}
		Durango.Utils.Singleton<GridAreaViewer>.Instance().Show(_areaList);
	}

	private void OnSelectGrid(GameObject obj)
	{
		if (_expandEstate != null)
		{
			return;
		}
		Vector3 position = MainCamera.ScreenPosToWorldPos(UICamera.currentTouch.pos);
		Point2 point = new Point2(Durango.Terrain.Util.ClientPositionToTilePosition(position));
		EstateInfo estateInfo = EstateSystem.GetEstateInfo(point);
		if (estateInfo == null)
		{
			Point2 point2 = point / 4;
			if (_selectedUnit != point2)
			{
				_selectedUnit = point2;
				RefreshGrid();
			}
		}
		else
		{
			UIManager.SystemMsg((estateInfo.License.Type != OwnerType.System) ? T._("다른 사람의 사유지 입니다") : T._("사유지를 선언할 수 없는 곳입니다"));
		}
	}

	private void OnExpandEstateClick(Point2 pos)
	{
		if (_expandEstate.License.Type == OwnerType.PersonalPlayer)
		{
			ExpandPersonalEstate(pos);
		}
		else
		{
			ExpandEstate(pos);
		}
	}

	private void ExpandPersonalEstate(Point2 pos)
	{
		if (_expandEstate.License.Size < _largestSize)
		{
			string id = _expandEstate.Id;
			Point2 unit = pos / 4;
			EstateSystem.ExpandEstate(id, unit, delegate(EstateLicense license)
			{
				ShowPlayerEstateDeclareEffect(EstateEffectType.PrivateExpand, unit);
				OnSuccess(license);
			});
		}
	}

	private void ExpandEstate(Point2 pos)
	{
		EstateLicense license2 = _expandEstate.License;
		if (!license2.Deposit.HasValue || !license2.DepositRunsOutAt.HasValue)
		{
			return;
		}
		OwnerType type = license2.Type;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		long num = Yaml.Util.Singleton<CostsYaml>.Instance.Estate.GetExtendingCost(license2.Type, license2.Size) * license2.Size;
		double num2 = (double)license2.Deposit.Value.Item1 - (double)num * (predictedServerTime - license2.Deposit.Value.Item2) / 86400.0;
		long num3 = Yaml.Util.Singleton<CostsYaml>.Instance.Estate.GetExtendingCost(license2.Type, license2.Size + 1) * (license2.Size + 1);
		if (num3 <= 0)
		{
			return;
		}
		double num4 = 86400.0 * num2 / (double)num3;
		MessageBox messageBox = UIManager.MessageBox;
		string mainText = string.Empty;
		string text = string.Empty;
		switch (license2.Type)
		{
		case OwnerType.Player:
			mainText = T._("사유지를 {0}칸으로 확장하시겠습니까?", license2.Size + 1);
			text = T._("[icon=icon_make_alert] 확장 시 사유지 유지비가 늘어납니다.");
			break;
		case OwnerType.ClanEstate:
			mainText = T._("부족 영토를 {0}칸으로 확장하시겠습니까?", license2.Size + 1);
			text = T._("[icon=icon_make_alert] 확장 시 부족 영토 유지비가 늘어납니다.");
			break;
		}
		messageBox.AddKeyValueInfo(T._("1일 유지비"), Durango.Logic.Item.Inventory.CurrencyFormat(num, Currency.TStone) + " [preset=animation_arrow] <em>" + Durango.Logic.Item.Inventory.CurrencyFormat(num3, Currency.TStone) + "</em>");
		long expandingCost = Yaml.Util.Singleton<CostsYaml>.Instance.Estate.GetExpandingCost(license2.Type, license2.Size);
		string confirm = ((license2.Size >= _largestSize) ? Durango.Logic.Item.Inventory.ToCurrencyButtonText(T._("확장"), expandingCost, Currency.TStone) : T._("확장 무료!"));
		bool flag = false;
		if (num4 < 86400.0)
		{
			text += T._("\n<alert>[icon=icon_make_alert] 예치금이 {0} 이상이어야 확장할 수 있습니다.</alert>", Durango.Logic.Item.Inventory.CurrencyFormat(num3, Currency.TStone));
			flag = true;
		}
		else
		{
			double num5 = license2.DepositRunsOutAt.Value - (predictedServerTime + num4);
			if (num5 > 0.0)
			{
				text += T._("\n[icon=icon_make_alert] 확장 시 유지비 상승으로 유효기간이 {0} 짧아집니다.", TimedeltaFormatter.Format(num5, 2, "min"));
			}
		}
		switch (license2.Type)
		{
		case OwnerType.Player:
			messageBox.SetCurrencyInfo(Currency.TStone);
			break;
		case OwnerType.ClanEstate:
			messageBox.SetClanFund();
			break;
		}
		string id = _expandEstate.Id;
		Point2 unit = pos / 4;
		if (flag)
		{
			messageBox.Show(mainText, text, null, new MessageBox.Button[1] { T._("취소") });
			return;
		}
		messageBox.Show(mainText, text, delegate(bool ok)
		{
			if (ok)
			{
				EstateSystem.ExpandEstate(id, unit, delegate(EstateLicense license)
				{
					if (type != 0)
					{
						if (type == OwnerType.ClanEstate)
						{
							ShowPlayerEstateDeclareEffect(EstateEffectType.ClanExpand, unit);
						}
					}
					else
					{
						ShowPlayerEstateDeclareEffect(EstateEffectType.PrivateExpand, unit);
					}
					OnSuccess(license);
				});
			}
		}, confirm);
	}

	private void OnShrinkEstateClick(Point2 pos)
	{
		EstateLicense license = _expandEstate.License;
		int size = license.Size;
		OwnerType type = license.Type;
		MessageBox messageBox = UIManager.MessageBox;
		string mainText = string.Empty;
		string subText = string.Empty;
		string id = _expandEstate.Id;
		Point2 unit = pos / 4;
		Action shrinkAction = delegate
		{
			if (_expandEstate.License.Size > 1)
			{
				EstateSystem.ShrinkEstate(id, unit, OnSuccess);
				if (PlayerBehavior.LocalPlayer.CurrentTile / 4 == unit)
				{
					UIBase.CloseAllUI();
				}
			}
			else
			{
				EstateSystem.RemoveEstate(id);
				UIBase.CloseAllUI();
			}
		};
		Action<bool> onOkCancel = null;
		if (size > 1)
		{
			switch (type)
			{
			case OwnerType.Player:
				mainText = T._("사유지를 {0}칸으로 축소하시겠습니까?", size - 1);
				subText = string.Format("{0}\n{1}\n{2}", T._("[icon=icon_make_alert] 축소 후 다시 확장할 때는 비용이 들지 않습니다."), T._("[icon=icon_make_alert] 최대 유효기한을 초과하는 부분의 예치금은 소지금으로 돌려받습니다."), T._("<alert>[icon=icon_make_alert] 축소한 사유지의 소유권을 잃게 되므로, 건물을 미리 포장하는 것이 좋습니다.</alert>"));
				break;
			case OwnerType.ClanEstate:
				mainText = T._("부족 영토를 {0}칸으로 축소하시겠습니까?", size - 1);
				subText = string.Format("{0}\n{1}\n{2}", T._("[icon=icon_make_alert] 축소 후 다시 확장할 때는 비용이 들지 않습니다."), T._("[icon=icon_make_alert] 최대 유효기한을 초과하는 부분의 예치금은 부족 자금으로 돌려받습니다."), T._("<alert>[icon=icon_make_alert] 축소한 부족 영토의 소유권을 잃게 되므로, 건물을 미리 포장하는 것이 좋습니다.</alert>"));
				break;
			}
			long extendingCost = Yaml.Util.Singleton<CostsYaml>.Instance.Estate.GetExtendingCost(type, size);
			long extendingCost2 = Yaml.Util.Singleton<CostsYaml>.Instance.Estate.GetExtendingCost(type, size - 1);
			messageBox.AddKeyValueInfo(T._("1일 유지비"), Durango.Logic.Item.Inventory.CurrencyFormat(extendingCost * size, Currency.TStone) + " [preset=animation_arrow] " + Durango.Logic.Item.Inventory.CurrencyFormat(extendingCost2 * (size - 1), Currency.TStone));
			onOkCancel = delegate(bool ok)
			{
				if (ok)
				{
					shrinkAction();
				}
			};
		}
		else
		{
			switch (type)
			{
			case OwnerType.Player:
				mainText = T._("마지막 사유지 입니다. 정말 포기하시겠습니까?");
				subText = string.Format("{0}\n{1}\n{2}", T._("[icon=icon_make_alert] 축소 후 다시 확장할 때는 비용이 들지 않습니다."), T._("[icon=icon_make_alert] 최대 유효기한을 초과하는 부분의 예치금은 소지금으로 돌려받습니다."), T._("<alert>[icon=icon_make_alert] 즉시 소유권을 잃게 되므로 건물을 미리 포장하는 것이 좋습니다.</alert>"));
				break;
			case OwnerType.ClanEstate:
				mainText = T._("마지막 부족 영토 입니다. 정말 포기하시겠습니까?");
				subText = string.Format("{0}\n{1}\n{2}", T._("[icon=icon_make_alert] 축소 후 다시 확장할 때는 비용이 들지 않습니다."), T._("[icon=icon_make_alert] 최대 유효기한을 초과하는 부분의 예치금은 부족 자금으로 돌려받습니다."), T._("<alert>[icon=icon_make_alert] 즉시 소유권을 잃게 되므로 건물을 미리 포장하는 것이 좋습니다.</alert>"));
				break;
			}
			long extendingCost3 = Yaml.Util.Singleton<CostsYaml>.Instance.Estate.GetExtendingCost(type, size);
			messageBox.AddKeyValueInfo(T._("1일 유지비"), Durango.Logic.Item.Inventory.CurrencyFormat(extendingCost3 * size, Currency.TStone) + " [preset=animation_arrow] " + Durango.Logic.Item.Inventory.CurrencyFormat(0L, Currency.TStone));
			switch (type)
			{
			case OwnerType.Player:
				onOkCancel = delegate(bool ok)
				{
					if (ok)
					{
						UIManager.MessageBox.Show(T._("모든 건물의 소유권을 잃습니다. 정말 포기하시겠습니까?"), string.Format("{0}\n{1}", T._("[icon=icon_make_alert] 사유지 포기 후 다시 확장할 때 비용이 들지 않습니다."), T._("<alert>[icon=icon_make_alert] 즉시 소유권을 잃게 되므로 건물을 미리 포장하는 것이 좋습니다.</alert>")), delegate(bool ok2)
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
						UIManager.MessageBox.Show(T._("모든 건물의 소유권을 잃습니다. 정말 포기하시겠습니까?"), string.Format("{0}\n{1}", T._("[icon=icon_make_alert] 부족 영토 포기 후 다시 확장할 때 비용이 들지 않습니다."), T._("<alert>[icon=icon_make_alert] 즉시 소유권을 잃게 되므로 건물을 미리 포장하는 것이 좋습니다.</alert>")), delegate(bool ok2)
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
		}
		string confirm = T._("축소");
		string cancel = T._("취소");
		messageBox.Show(mainText, subText, onOkCancel, confirm, cancel);
	}

	private void OnDeclareEstateClick(Point2 pos)
	{
		OwnerType type = _declareOwnerType;
		Point2 unit = pos / 4;
		Durango.Utils.Singleton<PlayerController>.Instance().MoveToPosition(Durango.Terrain.Util.TilePositionToClientPosition((unit.ToVector2() + Vector2.one * 0.5f) * 4f), delegate
		{
			GameSystem<EstateSystem>.Instance().DeclareEstate(type, unit, delegate(EstateLicense license)
			{
				if (type != OwnerType.PersonalPlayer && type != 0)
				{
					if (type == OwnerType.ClanEstate)
					{
						ShowPlayerEstateDeclareEffect(EstateEffectType.ClanDeclare, unit);
					}
				}
				else
				{
					ShowPlayerEstateDeclareEffect(EstateEffectType.PrivateDeclare, unit);
				}
				OnSuccess(license);
			});
			UIBase.CloseAllUI();
		}, 200f);
	}

	private void ShowPlayerEstateDeclareEffect(EstateEffectType type, Point2 unit)
	{
		EstateEffect estateEffect = _estateEffects[(int)type];
		int i = 0;
		for (int size = KUtility.GetSize(estateEffect.Audios); i < size; i++)
		{
			SoundManager.PlayEvent(estateEffect.Audios[i]);
		}
		if (!string.IsNullOrEmpty(estateEffect.Motion))
		{
			PlayerController.MotionUpdater.Motion(estateEffect.Motion);
		}
		if (!string.IsNullOrEmpty(estateEffect.Particle))
		{
			Vector3 pos = Durango.Terrain.Util.TilePositionToClientPosition((unit.ToVector2() + Vector2.one * 0.5f) * 4f);
			ParticleManager.Emit(estateEffect.Particle, pos, Quaternion.identity);
		}
	}

	private void OnSuccess(EstateLicense license)
	{
		OwnerType owner = GameSystem<EstateSystem>.Instance().CurrentEstate?.License.Type ?? _declareOwnerType;
		UIManager.FindScript<EstateGroup>().NotifyEstateLicenseChanged(owner, license);
	}

	private void RefreshSizeLabel()
	{
		int num = ((_expandEstate != null) ? _expandEstate.License.Size : 0);
		if (_largestSize == 0)
		{
			_sizeWidget.gameObject.SetActive(value: false);
			return;
		}
		_sizeWidget.gameObject.SetActive(value: true);
		_sizeLabel.text = $"{num} [FFFFFF7F]/ {_largestSize}[-]";
	}

	private void RefreshCurrencyLabel()
	{
		_currencyLabel.text = string.Empty;
		switch ((_expandEstate != null) ? _expandEstate.License.Type : _declareOwnerType)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			_currencyIcon.spriteName = Durango.Logic.Item.Inventory.GetIcon(Currency.TStone);
			_currencyLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(InventorySystem.Wallet.GetBalance(Currency.TStone));
			break;
		case OwnerType.ClanEstate:
			_currencyIcon.spriteName = "tstone_icon_clan";
			ClanSystem.GetClanFund(delegate(Costs costs)
			{
				_currencyLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(costs._Costs.Get(Currency.TStone, 0L));
			});
			break;
		}
		UIUtility.ResizeHeight(_currencyIcon, _currencyIcon.height);
	}

	private void RefreshResetWidget()
	{
		if (_expandEstate == null || _expandEstate.License.Type != OwnerType.PersonalPlayer)
		{
			_resetWidget.gameObject.SetActive(value: false);
			return;
		}
		_resetWidget.gameObject.SetActive(value: true);
		_resetLabel.SetText(new SyncString(delegate(out string text, out float period)
		{
			double num = ((!_expandEstate.License.RemovableAt.HasValue) ? 0.0 : _expandEstate.License.RemovableAt.Value) - Connections.Frontend.GetPredictedServerTime();
			if (num < 0.0)
			{
				text = null;
				period = 0f;
				_resetButton.gameObject.SetActive(value: true);
				_resetLabel.gameObject.SetActive(value: false);
				_resetWidget.width = 134;
				UIUtility.UpdateAnchors(base.transform);
			}
			else
			{
				_resetButton.gameObject.SetActive(value: false);
				_resetLabel.gameObject.SetActive(value: true);
				text = T._("<em>{0}</em> 후 초기화 가능", TimedeltaFormatter.ColonFormat(num));
				period = 0.5f;
				_resetLabel.text = text;
				int num2 = _resetLabel.width + 20;
				if (num2 > _resetWidget.width)
				{
					_resetWidget.width = num2;
					UIUtility.UpdateAnchors(base.transform);
				}
			}
		}));
	}

	protected override bool TryOpen()
	{
		Refresh();
		GameSystem<EstateSystem>.Instance().EstateGridUpdated += OnUpdateEstateGrid;
		SetZoomOutMode(enable: true);
		return base.TryOpen();
	}

	private void Refresh()
	{
		RefreshGrid();
		RefreshSizeLabel();
		RefreshCurrencyLabel();
		RefreshResetWidget();
	}

	protected override bool TryClose()
	{
		if (Durango.Utils.Singleton<GridAreaViewer>.HasInstance())
		{
			Durango.Utils.Singleton<GridAreaViewer>.Instance().Hide();
		}
		if (GameSystem<EstateSystem>.HasInstance())
		{
			GameSystem<EstateSystem>.Instance().EstateGridUpdated -= OnUpdateEstateGrid;
		}
		SetZoomOutMode(enable: false);
		base.TryClose();
		return true;
	}

	public void Open([NotNull] EstateInfo estate, int largestSize)
	{
		if (!base.IsOpened)
		{
			_expandEstate = estate;
			_largestSize = largestSize;
			_declareOwnerType = OwnerType.Invalid;
			base.Open();
		}
	}

	public void Open(OwnerType ownerType, int largestSize)
	{
		if (base.IsOpened)
		{
			return;
		}
		switch (GameManager.Region.Role())
		{
		default:
			UIManager.SystemMsg(T._("사유지를 선언할 수 없는 곳 입니다"));
			break;
		case Role.Rural:
		case Role.Urban:
		case Role.Safehouse:
		case Role.Personal:
			_declareOwnerType = ownerType;
			_expandEstate = null;
			_largestSize = largestSize;
			if (EstateSystem.GetEstateInfo(PlayerBehavior.LocalPlayer.CurrentTile) == null)
			{
				_selectedUnit = PlayerBehavior.LocalPlayer.CurrentTile / 4;
			}
			else
			{
				_selectedUnit = -Point2.one;
			}
			base.Open();
			break;
		}
	}

	public override bool Open()
	{
		throw new NotSupportedException();
	}
}
