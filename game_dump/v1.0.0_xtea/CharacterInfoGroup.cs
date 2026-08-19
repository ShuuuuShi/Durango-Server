using System;
using System.Collections.Generic;
using ClanData;
using Shared.Ability;
using StatisticsData;
using UnityEngine;

public class CharacterInfoGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private GameObject _touchBox;

	[SerializeField]
	private CharacterInfoPage _characterInfo;

	[SerializeField]
	private CharacterSurvivalGaugePage _characterFatigueInfo;

	[SerializeField]
	private CharacterTitlePage _characterTitleInfo;

	[SerializeField]
	private PageSwipe _pageSwipe;

	private Title _requestChangeTitle;

	private void Awake()
	{
		SetOpenCloseSound("Sound/Effect/UI/UI_Menu_Main_Open_01.wav", "Sound/Effect/UI/UI_Menu_Main_Close_01.wav");
		_pageSwipe.OnShowingPage = OnShowingPage;
		OnClose();
	}

	private void Start()
	{
		_titleWidget.OnClose += base.ForceClose;
		_characterTitleInfo.TitleSelected = OnSelectTitle;
		base.OnOpenSucceed += OnOpened;
		base.OnCloseSucceed += OnClosed;
		UIEventListener uIEventListener = UIEventListener.Get(_touchBox);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, (UIEventListener.BoolDelegate)delegate(GameObject go, bool press)
		{
			if (!press)
			{
				ForceClose();
			}
		});
	}

	private void OnEnable()
	{
		GameSystem<StatisticsSystem>.Instance().TitleUpdated += OnUpdateTitle;
		GameSystem<StatisticsSystem>.Instance().AbilitiesUpdated += OnUpdateAbilities;
		GameSystem<StatisticsSystem>.Instance().ExpChanged += OnUpdateExp;
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated += OnUpdateClan;
	}

	private void OnDisable()
	{
		GameSystem<StatisticsSystem>.Instance().TitleUpdated -= OnUpdateTitle;
		GameSystem<StatisticsSystem>.Instance().AbilitiesUpdated -= OnUpdateAbilities;
		GameSystem<StatisticsSystem>.Instance().ExpChanged -= OnUpdateExp;
		GameSystem<ClanSystem>.Instance().ClanInfoUpdated -= OnUpdateClan;
	}

	private void OnPortraitMode(bool isPortrait)
	{
		_pageSwipe.UpdateLayout();
	}

	private void OnOpened()
	{
		_requestChangeTitle = null;
		SetName(PlayerBehavior.LocalPlayer.PlayerName);
		OnUpdateTitle();
		OnUpdateAbilities();
		OnUpdateExp();
		OnUpdateClan();
	}

	private void OnClosed()
	{
		if (_requestChangeTitle != null)
		{
			GameSystem<StatisticsSystem>.Instance().RequestChangeTitle(_requestChangeTitle);
		}
	}

	private void OnUpdateTitle()
	{
		SetTitle(PlayerBehavior.LocalPlayer.Title._Title, GameSystem<StatisticsSystem>.Instance().Titles);
	}

	private void OnUpdateAbilities()
	{
		SetAbility(GameSystem<StatisticsSystem>.Instance().BasicAbilities);
	}

	private void OnUpdateExp(int prev, int exp)
	{
		OnUpdateExp();
	}

	private void OnUpdateExp()
	{
		GameSystem<StatisticsSystem>.Instance().GetLevel(out var level, out var currentExp, out var currentMaxExp);
		SetExp(level, currentExp, currentMaxExp);
	}

	private void OnUpdateClan()
	{
		SetClan(GameSystem<ClanSystem>.Instance().PlayerClan);
	}

	private void OnSelectTitle(Title title)
	{
		_requestChangeTitle = title;
		SetTitle(title);
	}

	private void SetName(string name)
	{
		_characterInfo.CharacterWidget.SetName(name);
	}

	private void SetTitle(Title title)
	{
		if (title == null || title.Enabled)
		{
			_characterInfo.CharacterWidget.SetTitle((title != null) ? title.Name : string.Empty);
			_characterInfo.UpdateLayout();
		}
		_characterTitleInfo.SetTitle(title);
	}

	private void SetTitle(string current, IList<Title> titles)
	{
		int num = -1;
		int i = 0;
		for (int count = titles.Count; i < count; i++)
		{
			if (titles[i].Name == current)
			{
				num = i;
				break;
			}
		}
		SetTitle((num != -1) ? titles[num] : null);
		_characterTitleInfo.SetTitleComboBox(num, titles);
	}

	private void SetClan(Clan clan)
	{
		_characterInfo.CharacterWidget.SetClan((clan != null) ? clan.Name : string.Empty);
		_characterInfo.UpdateLayout();
	}

	private void SetExp(int level, int current, int max)
	{
		_characterInfo.CharacterWidget.SetExp(level, current, max);
	}

	private void SetAbility(Dictionary<Basic, int> abilities)
	{
		_characterInfo.SetAbility(abilities);
	}

	private void OnShowingPage(int index)
	{
		switch (index)
		{
		case 0:
			_characterInfo.ShowAnimation();
			break;
		case 1:
			_characterFatigueInfo.ShowAnimation();
			break;
		case 2:
			_characterTitleInfo.ShowAnimation();
			break;
		}
	}
}
