using System;
using ClanData;
using UnityEngine;

public class ClanTooltip : TooltipBase
{
	[SerializeField]
	private GameObject _mainContainer;

	[SerializeField]
	private UITexture _emblemTexture;

	[SerializeField]
	private GameObject _noEmblem;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _memberLabel;

	[SerializeField]
	private UILabel _regionLabel;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private DefaultSelectableButton _joinButton;

	[SerializeField]
	private DefaultSelectableButton _reportButton;

	private bool _isLoaded;

	private Clan _clan;

	public void Set(Clan clan)
	{
		_isLoaded = false;
		_clan = clan;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		DefaultSelectableButton joinButton = _joinButton;
		joinButton.Clicked = (Action)Delegate.Combine(joinButton.Clicked, new Action(OnClickJoinButton));
		DefaultSelectableButton reportButton = _reportButton;
		reportButton.Clicked = (Action)Delegate.Combine(reportButton.Clicked, new Action(OnClickReportButton));
	}

	protected override void FillData()
	{
		if (_clan == null)
		{
			Hide(instant: true);
			return;
		}
		if (!_isLoaded)
		{
			_mainContainer.gameObject.SetActive(false);
			((Component)_reportButton).gameObject.SetActive(false);
			ClanSystem.GetClanInfo(_clan, OnClan);
			return;
		}
		_mainContainer.gameObject.SetActive(true);
		((Component)_reportButton).gameObject.SetActive(true);
		SetEmblem(null);
		_clan.GetEmblem(SetEmblem);
		_nameLabel.text = _clan.Name;
		_levelLabel.text = _clan.Level.ToString();
		_memberLabel.text = $"{_clan.MemberCount} [dddddd]/[-] {_clan.Capacity}";
		_regionLabel.text = _clan.Mainland;
		_commentLabel.text = _clan.GetIntro();
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		((Component)_joinButton).gameObject.SetActive(playerClan == null);
	}

	private void SetEmblem(Texture2D texture)
	{
		if ((Object)(object)texture == (Object)null)
		{
			_noEmblem.gameObject.SetActive(true);
			((Component)_emblemTexture).gameObject.SetActive(false);
		}
		else
		{
			_noEmblem.gameObject.SetActive(false);
			((Component)_emblemTexture).gameObject.SetActive(true);
			_emblemTexture.mainTexture = (Texture)(object)texture;
		}
	}

	private void OnClan(Clan clan)
	{
		if (_clan != null && clan != null && _clan.Id == clan.Id)
		{
			_clan = clan;
			_isLoaded = true;
			Refresh();
		}
	}

	protected override void UpdateLayout()
	{
		UIWidget component = ((Component)((Component)_joinButton).transform.parent).gameObject.GetComponent<UIWidget>();
		UIWidget component2 = ((Component)((Component)_commentLabel).transform.parent).gameObject.GetComponent<UIWidget>();
		component2.bottomAnchor.absolute = (((Component)_joinButton).gameObject.activeSelf ? component.height : 0);
		UIUtility.UpdateAnchors(((Component)component2).transform);
	}

	private void OnClickJoinButton()
	{
		ClanSystem.JoinClan(_clan);
	}

	private void OnClickReportButton()
	{
		if (!_reportButton.Disable)
		{
			Hide(instant: true);
			UIBase.CloseAllUI();
			UIManager.FindScript<SendReportGroup>().OpenForClan(_clan);
		}
	}
}
