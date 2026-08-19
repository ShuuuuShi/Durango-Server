using System;
using System.Collections.Generic;
using ClanData;
using L10N;
using Messages;
using Shared.Clan;
using UnityEngine;

public class ClanInfoPage : MonoBehaviour
{
	[SerializeField]
	private UIWidget _emblemEditButton;

	[SerializeField]
	private GameObject _noEmblem;

	[SerializeField]
	private UITexture _emblemSprite;

	[SerializeField]
	private UILabel _lvLabel;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _subTitleLabel;

	[SerializeField]
	private UIWidget _subTitleEditButton;

	[SerializeField]
	private UISpriteLabel _numberLabel;

	[SerializeField]
	private UISpriteLabel _regionLabel;

	[SerializeField]
	private UIWidget _noticeEditButton;

	[SerializeField]
	private UIWidget _noticeContainer;

	[SerializeField]
	private UILabel _noticeLabel;

	[SerializeField]
	private GameObject _noNotices;

	[SerializeField]
	private GameObject[] _editibleMarks;

	private Clan _clan;

	private bool _validRole;

	private MemberRole _myRole;

	private bool _hasEditPermission;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_emblemEditButton).gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnEditEmblem));
		UIEventListener uIEventListener2 = UIEventListener.Get(((Component)_subTitleEditButton).gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnEditSubTitle));
		UIEventListener uIEventListener3 = UIEventListener.Get(((Component)_noticeEditButton).gameObject);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnEditNotice));
	}

	private void OnEnable()
	{
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated += UpdateData;
		SetEmblem(null);
		UpdateData();
	}

	private void OnDisable()
	{
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated -= UpdateData;
	}

	private void UpdateData()
	{
		_clan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (_clan == null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		ClanData.Member member = _clan.GetMember(GameManager.PlayerId);
		_validRole = false;
		if (member != null && _clan.TryGetRole(member.RoleId, out var role))
		{
			_validRole = true;
			_myRole = role;
		}
		SetInfos(_clan);
		SetNotices(_clan.Notice);
	}

	private void SetInfos(Clan clan)
	{
		_titleLabel.text = clan.Name;
		_subTitleLabel.text = clan.GetIntro();
		_lvLabel.text = T._("{0:lv:}", clan.Level);
		_numberLabel.text = T._("[AAA297][icon=icon_mainhud_social:1.5][-] [ffd85b]{0}[-] [dddddd]/[-] {1}", clan.MemberCount, clan.Capacity);
		_regionLabel.text = T._("[AAA297][icon=icon_social_location][-] {0}", clan.Mainland);
		clan.GetEmblem(SetEmblem);
		_hasEditPermission = _validRole && (_myRole.Permissions & Permissions.EditClanInfo) != 0;
		int i = 0;
		for (int size = KUtility.GetSize(_editibleMarks); i < size; i++)
		{
			_editibleMarks[i].gameObject.SetActive(_hasEditPermission);
		}
	}

	private void SetEmblem(Texture2D emblem)
	{
		if ((Object)(object)emblem == (Object)null)
		{
			_noEmblem.gameObject.SetActive(true);
			((Component)_emblemSprite).gameObject.SetActive(false);
		}
		else
		{
			_noEmblem.gameObject.SetActive(false);
			((Component)_emblemSprite).gameObject.SetActive(true);
			_emblemSprite.mainTexture = (Texture)(object)emblem;
		}
	}

	private void SetNotices(string notice)
	{
		bool flag = !string.IsNullOrEmpty(notice);
		_noticeLabel.text = notice;
		((Component)_noticeContainer).gameObject.SetActive(flag);
		_noNotices.gameObject.SetActive(!flag);
	}

	private void OnEditEmblem(GameObject obj)
	{
		if (!_hasEditPermission)
		{
			return;
		}
		UIManager.MessageBox.Show(T._("수정하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				DrawPixelGroup canvasUI = UIManager.FindScript<DrawPixelGroup>();
				canvasUI.Open(32, 32, 1, delegate(List<Texture2D> list)
				{
					ClanSystem.SetClanEmblem(list[0].EncodeToPNG());
				}, ColorTable.ReadColorTable("color_board_L05.raw"));
				_clan.GetEmblem(delegate(Texture2D emblem)
				{
					if (!((Object)(object)emblem == (Object)null))
					{
						canvasUI.SetTexture(emblem, removeSpace: false);
					}
				});
			}
		});
	}

	private void OnEditSubTitle(GameObject obj)
	{
		if (_hasEditPermission)
		{
			UIManager.Popup.TextInput.Show(ClanSystem.SetClanIntro, T._("부족의 소개글을 적어주세요"), _clan.Intro);
		}
	}

	private void OnEditNotice(GameObject obj)
	{
		if (_hasEditPermission)
		{
			UIManager.Popup.TextInput.Show(ClanSystem.SetClanNotice, T._("부족의 공지사항을 적어주세요"), _clan.Notice);
		}
	}
}
