using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class ClanInfoPopup : TooltipBase
{
	private enum ButtonType
	{
		Join,
		SuggestAlly,
		Report
	}

	[SerializeField]
	private UITexture _textureEmblem;

	[SerializeField]
	private GameObject _iconNoEmblem;

	[SerializeField]
	private UIWidget _upperPane;

	[SerializeField]
	private UILabel _textClanName;

	[SerializeField]
	private UILabel _textLevel;

	[SerializeField]
	private UILabel _textRegionName;

	[SerializeField]
	private UILabel _textMembers;

	[SerializeField]
	private UILabel _textNotice;

	[SerializeField]
	private UIWidget _lowerPane;

	[SerializeField]
	private SelectableButton _buttonBase;

	[SerializeField]
	private int _buttonsCountPerLine;

	[NotNull]
	private Clan _clan;

	private readonly ListObjectPool<SelectableButton> _buttons = new ListObjectPool<SelectableButton>();

	private readonly List<ButtonType> _buttonTypes = new List<ButtonType>();

	private int _defaultLowerPaneHeight;

	private bool _isWaitingClan;

	private bool _hideJoin;

	public void Set([NotNull] Clan clan, bool hideJoin = false)
	{
		_clan = clan;
		_hideJoin = hideJoin;
		Clan waitingClan = GameSystem<ClanSystem>.Instance().WaitingClan;
		_isWaitingClan = waitingClan != null && waitingClan.Id == _clan.Id;
	}

	protected override void OnAwake()
	{
		_buttons.BaseObject = _buttonBase;
		_buttons.Init(delegate(SelectableButton button)
		{
			SelectableButton selectableButton = button;
			selectableButton.Clicked = (Action)Delegate.Combine(selectableButton.Clicked, (Action)delegate
			{
				int num = _buttons.IndexOf(button);
				if (0 <= num && num < _buttonTypes.Count)
				{
					DoButtonClick(_buttonTypes[num]);
				}
			});
		});
		_defaultLowerPaneHeight = _lowerPane.height;
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated += OnClanInfoUpdated;
	}

	protected override void FillData()
	{
		SetEmblem(-Point2.one);
		ClanSystem.GetEmblem(_clan.Id, SetEmblem);
		_textClanName.text = _clan.Name;
		_textLevel.text = _clan.Level.ToString();
		_textRegionName.text = _clan.Mainland;
		_textMembers.text = $"{_clan.MemberCount} / {_clan.Capacity}";
		_textNotice.text = ((!string.IsNullOrEmpty(_clan.Intro)) ? _clan.Intro : T._("부족 소개글이 없습니다"));
		ClearButtons();
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			if (!_hideJoin)
			{
				AddButton(ButtonType.Join);
			}
			AddButton(ButtonType.Report);
		}
		else if (_clan.Id != playerClan.Id)
		{
			Messages.Member clan = PlayerBehavior.LocalPlayer.Clan;
			if (clan.ClanId == playerClan.Id && clan.RoleId == 0)
			{
				AddButton(ButtonType.SuggestAlly);
			}
			AddButton(ButtonType.Report);
		}
	}

	protected override void UpdateLayout()
	{
		Vector3 localPosition = _buttonBase.transform.localPosition;
		int num = 0;
		for (int i = 0; i < _buttons.Count; i++)
		{
			SelectableButton selectableButton = _buttons[i];
			int num2 = _buttonBase.Widget.width * (i % _buttonsCountPerLine);
			int num3 = _buttonBase.Widget.height * (i / _buttonsCountPerLine);
			num = Mathf.Max(num, num3);
			selectableButton.transform.localPosition = localPosition - Vector3.up * num3 + Vector3.right * num2;
		}
		_lowerPane.height = _defaultLowerPaneHeight + num;
		_lowerPane.gameObject.SetActive(_buttons.Count > 0);
		base.Widget.height = _upperPane.height + _lowerPane.height;
	}

	private void ClearButtons()
	{
		_buttons.Clear();
		_buttonTypes.Clear();
	}

	private void AddButton(ButtonType type)
	{
		SelectableButton selectableButton = _buttons.Add();
		selectableButton.Text = GetButtonText(type);
		selectableButton.SetStyle((type != 0) ? PresetButton.Style.Border : PresetButton.Style.Solid);
		selectableButton.Disabled = type == ButtonType.Join && _isWaitingClan;
		selectableButton.CanClickWhenDisabled = true;
		_buttonTypes.Add(type);
	}

	private string GetButtonText(ButtonType type)
	{
		return type switch
		{
			ButtonType.Join => (!_isWaitingClan) ? T._("가입 신청") : T._("가입 대기"), 
			ButtonType.SuggestAlly => T._("동맹 맺기"), 
			ButtonType.Report => string.Format("<alert>[icon=alarm_private] {0}</alert>", T._("신고")), 
			_ => string.Empty, 
		};
	}

	private void DoButtonClick(ButtonType type)
	{
		switch (type)
		{
		case ButtonType.Join:
			if (_isWaitingClan)
			{
				UIManager.SystemMsg(T._("가입 대기 중입니다."));
			}
			else
			{
				UIManager.FindScript<ClanGroup>().JoinClan(_clan);
			}
			break;
		case ButtonType.SuggestAlly:
			Hide();
			UIManager.FindScript<ClanGroup>().SuggestAlly(_clan.Id);
			break;
		case ButtonType.Report:
		{
			SendReportPopup sendReportPopup = UIManager.Popup.Tooltip<SendReportPopup>();
			sendReportPopup.SetForClan(_clan);
			sendReportPopup.Show();
			break;
		}
		}
	}

	private void SetEmblem(Point2 pos)
	{
		if (pos.x < 0 || pos.y < 0)
		{
			_iconNoEmblem.gameObject.SetActive(value: true);
			_textureEmblem.gameObject.SetActive(value: false);
		}
		else
		{
			_iconNoEmblem.gameObject.SetActive(value: false);
			_textureEmblem.gameObject.SetActive(value: true);
			EmblemTexture.Set(_textureEmblem, pos);
		}
	}

	private void OnClanInfoUpdated()
	{
		if (base.gameObject.activeSelf)
		{
			Set(_clan, _hideJoin);
			Refresh();
		}
	}
}
