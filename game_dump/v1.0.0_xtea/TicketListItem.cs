using System;
using System.Text;
using L10N;
using Messages;
using Player;
using UnityEngine;

public class TicketListItem : MonoBehaviour
{
	private static StringBuilder _codeBuilder = new StringBuilder();

	[SerializeField]
	private UIWidget _keyWidget;

	[SerializeField]
	private UISpriteLabel _keyLabel;

	[SerializeField]
	private UIWidget _userWidget;

	[SerializeField]
	private UILabel _userLabel;

	[SerializeField]
	private UISprite _tierSprite;

	[SerializeField]
	private GameObject _lineSprite;

	private Messages.Ticket _ticket;

	private string _dashCode;

	private Player.PlayerInfo _venderInfo;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_keyWidget).gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickKeyWidget));
		UIEventListener uIEventListener2 = UIEventListener.Get(((Component)_userWidget).gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickUserWidget));
	}

	private void OnClickKeyWidget(GameObject obj)
	{
		string clipBoardString = _ticket.Code.Replace("-", string.Empty);
		UniPasteBoard.SetClipBoardString(clipBoardString);
		UIManager.SystemMsg(T._("{0} 을 복사했습니다", _dashCode));
	}

	private void OnClickUserWidget(GameObject obj)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (_venderInfo != null)
		{
			ProfileTooltip profileTooltip = UIManager.Popup.Tooltip<ProfileTooltip>();
			profileTooltip.Set(_venderInfo);
			profileTooltip.Direction = TooltipBase.TooltipDirection.Horizontal;
			profileTooltip.Show(_userWidget, Vector2.zero, 3600f);
		}
	}

	public void Set(Messages.Ticket ticket)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		_ticket = ticket;
		_dashCode = CodeToString(ticket.Code);
		_keyLabel.text = string.Format("[{1}]{0}[-] [{2}][market_check_icon_big][-]", _dashCode, NGUIText.EncodeColor((!ticket.Activated) ? Color.white : PresetColor.UIMoreLightGray), NGUIText.EncodeColor((!ticket.Activated) ? PresetColor.UIMoreLightGray : PresetColor.UIYellow));
		_venderInfo = null;
		if (ticket.Activated)
		{
			if (ticket.Vendor.HasValue)
			{
				_userLabel.text = string.Empty;
				TicketVendor value = ticket.Vendor.Value;
				TicketGroup.SetTierIcon(_tierSprite, value.Tier);
				KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(value.EntityId, OnResponsePlayerInfo, useOldCache: true);
			}
			else
			{
				_userLabel.text = T._("미생성");
				_userLabel.color = PresetColor.UIMoreLightGray;
				_tierSprite.alpha = 0f;
			}
		}
		else
		{
			_userLabel.text = T._("미사용");
			_userLabel.color = PresetColor.UIMoreLightGray;
			_tierSprite.alpha = 0f;
		}
	}

	public void SetActiveSplitLine(bool active)
	{
		_lineSprite.SetActive(active);
	}

	private void OnResponsePlayerInfo(Player.PlayerInfo playerInfo)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (_ticket.Vendor.HasValue)
		{
			TicketVendor value = _ticket.Vendor.Value;
			if (playerInfo.EntityId == value.EntityId)
			{
				_venderInfo = playerInfo;
				_userLabel.text = $"{playerInfo.Name}#{playerInfo.Freq}";
				_userLabel.color = Color.white;
			}
		}
	}

	private static string CodeToString(string code)
	{
		_codeBuilder.Length = 0;
		for (int i = 0; i < 4; i++)
		{
			if (i > 0)
			{
				_codeBuilder.Append('-');
			}
			_codeBuilder.Append(code, i * 4, 4);
		}
		return _codeBuilder.ToString();
	}
}
