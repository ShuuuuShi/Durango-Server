using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Region;
using Shared.Voucher;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class ShopVouchersPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private KScrollView _voucherList;

	[SerializeField]
	private GameObject _noVoucherWidget;

	private string[] _targetVouchers;

	public override bool DragLock => true;

	protected override void Start()
	{
		base.Start();
		_titleLabel.text = T._("이용권 보기");
	}

	protected override void FillData()
	{
		Wallet wallet = InventorySystem.Wallet;
		_voucherList.Nodes.BeginLoad();
		IEnumerable<string> enumerable;
		if (_targetVouchers != null)
		{
			IEnumerable<string> targetVouchers = _targetVouchers;
			enumerable = targetVouchers;
		}
		else
		{
			enumerable = wallet.Vouchers.Select((VoucherInfo x) => x.VoucherId);
		}
		foreach (string item in enumerable)
		{
			if (SingletonDict<string, Voucher>.TryGetValue(item, out var value) && value.Visible)
			{
				Color iconColor = NGUIText.ParseColor24(value.GetHexColor());
				bool flag = string.IsNullOrEmpty(value.ExpiresOn);
				string title = string.Concat(value.Name, flag ? string.Empty : " [icon=icon_question_big]");
				string expiry = string.Empty;
				if (!flag && Times.TryParse(value.ExpiresOn, out var result))
				{
					double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
					double seconds = result.ToUnixTime() - predictedServerTime;
					expiry = T._("{0} 남음", TimedeltaFormatter.Format(seconds));
				}
				string arg = string.Empty;
				Action clicked = null;
				if (value.GuideType == GuideType.WarpToPort)
				{
					clicked = VoucherWidgetButton_Clicked;
					arg = "  [icon=icon_arrow_right]";
				}
				int voucherCount = InventorySystem.Wallet.GetVoucherCount(item);
				string count = $"[ffd85b]{voucherCount}[-]/{value.CountMax}{arg}";
				_voucherList.Nodes.GetNext().GetComponent<VoucherWidget>().Set(value.Icon, iconColor, title, value.Description, expiry, count, clicked);
			}
		}
		_voucherList.Nodes.EndLoad();
		_noVoucherWidget.SetActive(_voucherList.Nodes.Count <= 0);
		_voucherList.Reposition();
	}

	public void Show(string[] targetVouchers = null)
	{
		_targetVouchers = targetVouchers;
		base.Show();
	}

	protected override void UpdateLayout()
	{
		UIUtility.UpdateAnchors(_voucherList.transform);
	}

	private static void VoucherWidgetButton_Clicked()
	{
		Role role = GameManager.Region.Role();
		if (role != Role.Rural && role != Role.Urban && role != Role.Personal)
		{
			UIManager.SystemMsg(T._("열기구는 개인섬과 도시섬에서만 탈 수 있습니다."));
			return;
		}
		string mainText = T._("열기구는 항구에서 탑승할 수 있습니다.\n 가까운 항구로 이동하시겠습니까?");
		UIManager.MessageBox.Show(mainText, delegate(bool ok)
		{
			if (ok)
			{
				UIBase.CloseAllUI();
				GameSystem<MapSystem>.Instance().WarpToPort();
			}
		});
	}
}
