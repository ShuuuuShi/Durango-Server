using System.Collections.Generic;
using Estate;
using ItemSystem;
using Shared.Economy;
using UnityEngine;

public class EstateManagerGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private EstateManageContainer _estateManager;

	private bool _isOpenEstateExtendUI;

	private ulong _prevTileEstate;

	private void Awake()
	{
		base.OnClose();
	}

	private void Start()
	{
		_titleWidget.OnBack += Close;
		_titleWidget.OnClose += base.ForceClose;
		_estateManager.Closed += base.ForceClose;
		PlayerBehavior.LocalPlayer.TileChanged += Localplayer_TileChanged;
	}

	public void OpenPermissionManager(ulong estateId)
	{
		_estateManager.Set(GameSystem<EstateSystem>.Instance().GetEstateInfo(estateId));
		Open();
	}

	private void Localplayer_TileChanged(Point2 prev, Point2 current)
	{
		TileObject currentTileObject = PlayerBehavior.LocalPlayer.CurrentTileObject;
		if (currentTileObject == null)
		{
			_prevTileEstate = 0uL;
		}
		else if (_prevTileEstate != currentTileObject.EstateId)
		{
			if (_prevTileEstate != 0L)
			{
				OnLeaveEstate(_prevTileEstate);
			}
			_prevTileEstate = currentTileObject.EstateId;
			if (currentTileObject.EstateId != 0L)
			{
				OnEnterEstate(currentTileObject.EstateId);
			}
		}
	}

	private void OnEnterEstate(ulong estateId)
	{
		EstateInfo estateInfo = GameSystem<EstateSystem>.Instance().GetEstateInfo(estateId);
		if (estateInfo != null && !estateInfo.RestrictedArea)
		{
			estateInfo.ShowEstateLines();
		}
	}

	private void OnLeaveEstate(ulong estateId)
	{
		EstateInfo estateInfo = GameSystem<EstateSystem>.Instance().GetEstateInfo(estateId);
		if (estateInfo != null && !estateInfo.RestrictedArea)
		{
			estateInfo.HideEstateLines();
		}
		HideEstateExtendUI();
	}

	public void OpenEstateExtendUI(ulong id)
	{
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		if (_isOpenEstateExtendUI)
		{
			return;
		}
		EstateInfo estateInfo = GameSystem<EstateSystem>.Instance().GetEstateInfo(id);
		if (estateInfo == null)
		{
			return;
		}
		_isOpenEstateExtendUI = true;
		List<Point2> list = new List<Point2>();
		int i = 0;
		for (int count = estateInfo.Units.Count; i < count; i++)
		{
			Point2 point = estateInfo.Units[i];
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					if ((j != 0 || k != 0) && j * k == 0)
					{
						Point2 item = point + new Point2(j, k);
						if (!estateInfo.Units.Contains(item) && !list.Contains(item))
						{
							list.Add(item);
						}
					}
				}
			}
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			bool flag = false;
			Point2 point2 = list[num];
			for (int l = 0; l < 4; l++)
			{
				for (int m = 0; m < 4; m++)
				{
					TileObject tileObject = TerrainA6.GetTileObject(point2 * 4 + new Point2(l, m));
					if (tileObject == null || tileObject.EstateId != 0L)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				list.RemoveAt(num);
			}
		}
		SelectAreaUI.AreaStruct[] array = new SelectAreaUI.AreaStruct[list.Count];
		string comment = Inventory.CurrencyFormat(estateInfo.ExtendCost, Currency.TStone);
		int n = 0;
		for (int num2 = array.Length; n < num2; n++)
		{
			Point2 unit = list[n];
			ref SelectAreaUI.AreaStruct reference = ref array[n];
			reference = new SelectAreaUI.AreaStruct
			{
				Pos = unit * 4,
				Size = Point2.one * 4,
				Color = Color.white,
				Comment = comment,
				OnSelect = delegate
				{
					GameSystem<EstateSystem>.Instance().RequestAddEstateUnit(id, unit);
					HideEstateExtendUI();
				}
			};
		}
		UIBase.HideUI(UIFlag.Base, hide: true, "Estate");
		KSingleton<SelectAreaUI>.Instance().Show(array);
	}

	private void HideEstateExtendUI()
	{
		if (_isOpenEstateExtendUI)
		{
			_isOpenEstateExtendUI = false;
			KSingleton<SelectAreaUI>.Instance().Hide();
			UIBase.HideUI(UIFlag.Base, hide: false, "Estate");
		}
	}
}
