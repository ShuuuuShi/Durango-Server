using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Logic.Clan;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Clan;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI;

public class ClanInfoPage : ClanMenuPage, IUIInitializable
{
	[CompilerGenerated]
	private sealed class _003CCoRefreshPlayerClan_003Ed__26 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float totalSeconds;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoRefreshPlayerClan_003Ed__26(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(totalSeconds);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				ClanSystem.RefreshPlayerClan();
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[SerializeField]
	private UIWidget _emblemEditButton;

	[SerializeField]
	private GameObject _noEmblem;

	[SerializeField]
	private UITexture _emblemSprite;

	[SerializeField]
	private GameObject _emblemEditableMark;

	[SerializeField]
	private UILabel _emblemWarningLabel;

	[SerializeField]
	private UILabel _lvLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _nameWarningLabel;

	[SerializeField]
	private UIWidget _clanRenamingButton;

	[SerializeField]
	private UILabel _numberLabel;

	[SerializeField]
	private UILabel _regionLabel;

	[SerializeField]
	private UILabel _textClanFund;

	[SerializeField]
	private SelectableButton _donateButton;

	[SerializeField]
	private GameObject[] _editableMarks;

	[SerializeField]
	private ClanBulletinBoard _noticeBulletinBoard;

	[SerializeField]
	private ClanBulletinBoard _introBulletinBoard;

	private Clan _clan;

	private bool _hasEditPermission;

	private bool _isEditableEmblem;

	private bool _canClickRegionLabel;

	private ICoroutineBinder _binder;

	private const string EmblemDrawingAsArtifactKey = "emblem_draw_pixel";

	void IUIInitializable.Init()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_emblemEditButton.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnEditEmblem));
		UIEventListener uIEventListener2 = UIEventListener.Get(_clanRenamingButton.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(ClanRenamingButton_Clicked));
		UIEventListener uIEventListener3 = UIEventListener.Get(_regionLabel.gameObject);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnClickRegionLabel));
		SelectableButton donateButton = _donateButton;
		donateButton.Clicked = (Action)Delegate.Combine(donateButton.Clicked, new Action(OnClickDonateButton));
		string defaultText = T._("공지사항을 입력하세요.");
		_noticeBulletinBoard.Init(defaultText, 250, NoticeBulletinBoard_SendButtonClicked);
		string defaultText2 = T._("부족 소개글을 입력하세요.");
		_introBulletinBoard.Init(defaultText2, 55, IntroBulletinBoard_SendButtonClicked, isLineBreakable: false);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated += UpdateData;
		SetEmblem(-Point2.one);
		SetEmblemEditable(editable: false);
		_textClanFund.alpha = 0f;
		UpdateData();
	}

	private void OnDisable()
	{
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated -= UpdateData;
	}

	public override bool Back()
	{
		if (_introBulletinBoard.Back())
		{
			return _noticeBulletinBoard.Back();
		}
		return false;
	}

	private IEnumerator CoRefreshPlayerClan(float totalSeconds)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoRefreshPlayerClan_003Ed__26(0)
		{
			totalSeconds = totalSeconds
		};
	}

	private void UpdateData()
	{
		_clan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (_clan == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		Messages.Member clan = PlayerBehavior.LocalPlayer.Clan;
		if (clan.ClanId == _clan.Id && _clan.TryGetRole(clan.RoleId, out var role))
		{
			_hasEditPermission = role.HasPermission(Permissions.EditClanInfo);
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double val = ((!(_clan.RenameableUntil > predictedServerTime)) ? double.MaxValue : _clan.RenameableUntil);
		double val2 = ((!(_clan.EmblemChangeableUntil > predictedServerTime)) ? double.MaxValue : _clan.EmblemChangeableUntil);
		double num = Math.Min(val, val2);
		if (num < double.MaxValue)
		{
			double num2 = num - predictedServerTime + 1.0;
			GameSystem<ClanSystem>.Instance().StartCoroutine(ref _binder, CoRefreshPlayerClan((float)num2));
		}
		else
		{
			GameSystem<ClanSystem>.Instance().StopCoroutine(_binder);
		}
		SetInfos();
		_introBulletinBoard.Text = _clan.Intro;
		_noticeBulletinBoard.Text = _clan.Notice;
		ClanSystem.GetEmblem(_clan.Id, SetEmblem);
		ClanSystem.GetClanFund(SetClanCosts);
	}

	private void IntroBulletinBoard_SendButtonClicked()
	{
		if (!_hasEditPermission)
		{
			return;
		}
		ClanSystem.SetClanIntro(_introBulletinBoard.Text, delegate(bool success)
		{
			if (success)
			{
				_introBulletinBoard.SetEditMode(editMode: false);
			}
		});
	}

	private void NoticeBulletinBoard_SendButtonClicked()
	{
		if (!_hasEditPermission)
		{
			return;
		}
		ClanSystem.SetClanNotice(_noticeBulletinBoard.Text, delegate(bool success)
		{
			if (success)
			{
				_noticeBulletinBoard.SetEditMode(editMode: false);
			}
		});
	}

	private void SetInfos()
	{
		if (_clan != null)
		{
			_lvLabel.text = $"[AAA297][icon=icon_align_lv:1.2][-] {_clan.Level}";
			_numberLabel.text = $"[AAA297][icon=icon_person_big][-] [FFD85B]{_clan.MemberCount}[-] [FFFFFF7F]/[-] {_clan.Capacity}";
			bool flag = _clan.RenameableUntil > Connections.Frontend.GetPredictedServerTime();
			_nameLabel.text = ((!flag) ? ("[ffd85b]<em>" + _clan.Name + "</em>[-]") : ("[ffd85b]<em>" + _clan.Name + "</em>[-] [FFFFFF96][icon=icon_chat_edit][-]"));
			_nameWarningLabel.gameObject.SetActive(flag);
			if (flag)
			{
				_nameWarningLabel.SetText(new SyncString(delegate(out string text, out float period)
				{
					double num = _clan.RenameableUntil - Connections.Frontend.GetPredictedServerTime();
					string text2 = TimedeltaFormatter.Format(num);
					text = T._("[icon=icon_make_alert]\n{0} 동안 수정 가능", text2);
					period = (float)num % 1f;
				}));
			}
		}
		_canClickRegionLabel = false;
		RefreshRegionLabel();
		EstateSystem.GetEstateLicenses(delegate(EstateLicenses licenses)
		{
			if (licenses.ClanEstate.HasValue)
			{
				_canClickRegionLabel = true;
				RefreshRegionLabel();
			}
		});
		if (_editableMarks != null)
		{
			for (int i = 0; i < _editableMarks.Length; i++)
			{
				_editableMarks[i].gameObject.SetActive(_hasEditPermission);
			}
		}
	}

	private void SetEmblemEditable(bool editable)
	{
		_isEditableEmblem = _hasEditPermission && editable;
		_emblemEditableMark.SetActive(_isEditableEmblem);
	}

	private void SetEmblemWarning(bool shows)
	{
		_emblemWarningLabel.gameObject.SetActive(_hasEditPermission && shows);
		if (shows)
		{
			_emblemWarningLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				double num = _clan.EmblemChangeableUntil - Connections.Frontend.GetPredictedServerTime();
				string text2 = TimedeltaFormatter.Format(num, 1);
				text = T._("[icon=icon_make_alert]\n{0} 동안 수정 가능", text2);
				period = (float)num % 1f;
			}));
		}
	}

	private void RefreshRegionLabel()
	{
		if (_clan != null)
		{
			string text = "[AAA297][icon=icon_social_location][-] " + _clan.Mainland;
			if (_canClickRegionLabel)
			{
				text += " [icon=img_market_arrowright]";
			}
			_regionLabel.text = text;
		}
	}

	private void SetEmblem(Point2 pos)
	{
		if (pos.x < 0 || pos.y < 0)
		{
			_noEmblem.gameObject.SetActive(value: true);
			_emblemSprite.gameObject.SetActive(value: false);
			SetEmblemEditable(editable: true);
			SetEmblemWarning(shows: false);
		}
		else if (_clan.EmblemChangeableUntil > Connections.Frontend.GetPredictedServerTime())
		{
			_noEmblem.gameObject.SetActive(value: false);
			_emblemSprite.gameObject.SetActive(value: true);
			EmblemTexture.Set(_emblemSprite, pos);
			SetEmblemEditable(editable: true);
			SetEmblemWarning(shows: true);
		}
		else
		{
			_noEmblem.gameObject.SetActive(value: false);
			_emblemSprite.gameObject.SetActive(value: true);
			EmblemTexture.Set(_emblemSprite, pos);
			SetEmblemEditable(editable: false);
			SetEmblemWarning(shows: false);
		}
	}

	private void SetClanCosts(Costs costs)
	{
		long amount = ((costs._Costs == null) ? 0 : costs._Costs.Get(Currency.TStone, 0L));
		_textClanFund.alpha = 1f;
		_textClanFund.text = Durango.Logic.Item.Inventory.CurrencyFormat(amount, "tstone_icon_clan", 1.5f);
	}

	private static string CurrencyToString(float currency)
	{
		return Durango.Logic.Item.Inventory.CurrencyFormat((long)currency, Currency.TStone);
	}

	private void OnEditEmblem(GameObject obj)
	{
		if (!_isEditableEmblem)
		{
			return;
		}
		DrawPixelGroup canvasUI = UIManager.FindScript<DrawPixelGroup>();
		canvasUI.Set(32, 32, "emblem_draw_pixel", 1, "color_board_L05.raw", T._("부족 문양 편집"), T._("정말 그리기를 종료하시겠습니까?\n완성하지 않은 부족 문양은 임시 저장됩니다."), delegate(List<Texture2D> list, Action<bool> confirmed)
		{
			string mainText = string.Format("[size=32]{0}[/size]\n[size=25]<alert>[icon=icon_make_alert]{1}</alert>", T._("완성하시겠습니까?"), T._("부족 문양은 한번 정하면 바꿀 수 없습니다."));
			UIManager.MessageBox.Show(mainText, delegate(bool ok)
			{
				if (confirmed != null)
				{
					confirmed(ok);
				}
				if (ok)
				{
					ClanSystem.SetClanEmblem(list[0].EncodeToPNG(), delegate
					{
						ClanSystem.GetEmblem(_clan.Id, SetEmblem);
					});
					canvasUI.Close();
				}
			});
		});
		canvasUI.SetWarningText(string.Format("<alert>[icon=icon_make_alert]{0}</alert>", T._("부족 문양은 한번 정하면 바꿀 수 없습니다.")));
		ClanSystem.GetEmblem(_clan.Id, delegate(Point2 pos)
		{
			if (pos.x >= 0 && pos.y >= 0)
			{
				EmblemAtlas emblemAtlas = ClanSystem.EmblemAtlas;
				canvasUI.SetTexture(emblemAtlas.Texture, emblemAtlas.GetUvRect(pos));
			}
		});
	}

	private void ClanRenamingButton_Clicked(GameObject obj)
	{
		bool flag = _clan.RenameableUntil > Connections.Frontend.GetPredictedServerTime();
		if (_hasEditPermission && flag)
		{
			string text = T._("부족의 이름을 입력해주세요");
			double num = _clan.RenameableUntil - Connections.Frontend.GetPredictedServerTime();
			if (num > 0.0)
			{
				string text2 = TimedeltaFormatter.Format(num);
				string text3 = T._("{0} 이내에 변경하지 않으면, 이대로 고정됩니다.", text2);
				text = text + "\n[size=10] \n[/size][size=22]<alert>[icon=icon_make_alert] " + text3 + "</alert>[/size]";
			}
			UIManager.Popup.Tooltip<TextInputPopup>().Show(SubmitClanName, text, _clan.Name);
		}
	}

	private void SubmitClanName(string newClanName)
	{
		if (string.IsNullOrEmpty(newClanName) || newClanName.Trim().Length == 0 || newClanName == _clan.Name)
		{
			return;
		}
		string mainText = T._("부족 이름을 <em>{0}</em>{0:-으로} 변경하시겠습니까?", newClanName);
		UIManager.MessageBox.Show(mainText, delegate(bool ok)
		{
			if (ok)
			{
				ClanSystem.RenameClan(newClanName);
			}
		});
	}

	private void OnClickRegionLabel(GameObject obj)
	{
		if (_canClickRegionLabel)
		{
			UIManager.FindScript<EstateGroup>().Open(EstateTabWidget.Menu.Clan);
		}
	}

	private void OnClickDonateButton()
	{
		string title = T._("기부할 금액 입력");
		UIManager.Popup.Tooltip<NumberInputPopup>().Show(0L, Currency.TStone, title, DonateToClanFund, 99999999L);
	}

	private void DonateToClanFund(long value)
	{
		int num = (int)value;
		long balance = InventorySystem.Wallet.GetBalance(Currency.TStone);
		if (num > balance)
		{
			UIManager.SystemMsg(T._("돈이 없습니다."));
			return;
		}
		Dictionary<Currency, long> dictionary = new Dictionary<Currency, long>();
		dictionary[Currency.TStone] = num;
		Connections.Frontend.Send(new DonateToClanFund
		{
			Costs = dictionary
		}).On(delegate(Costs costs, PacketHeader _)
		{
			SetClanCosts(costs);
		});
	}
}
