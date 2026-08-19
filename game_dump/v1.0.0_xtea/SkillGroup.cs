using System;
using System.Collections.Generic;
using L10N;
using Shared.Skill;
using SkillData;
using UnityEngine;

public class SkillGroup : UIBase, INewCheckerable
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private KWidgetScrollView _mainScroll;

	[SerializeField]
	private SkillCategoryWidget _skillCategory;

	[SerializeField]
	private SkillCategoryInfoWidget _skillCategoryInfo;

	[SerializeField]
	private SkillListWidget _skillList;

	[SerializeField]
	private SkillInfoWidget _skillInfo;

	private float _scrollOffset;

	private int _scrollIndex;

	private NewCheckerCountableNode _newChecker;

	public NewChecker NewChecker
	{
		get
		{
			if (_newChecker == null)
			{
				_newChecker = new NewCheckerCountableNode();
			}
			return _newChecker;
		}
	}

	private void Awake()
	{
		SetOpenCloseSound("Sound/Effect/UI/UI_Menu_Skill_Open_01.wav", "Sound/Effect/UI/UI_Menu_Skill_Close_01.wav");
		int i = 0;
		for (int num = _mainScroll.Widgets.Length; i < num; i++)
		{
			((Component)_mainScroll.Widgets[i]).gameObject.SetActive(true);
		}
		base.OnClose();
	}

	private void Start()
	{
		_titleWidget.OnClose += base.ForceClose;
		_titleWidget.OnBack += Close;
		SkillCategoryWidget skillCategory = _skillCategory;
		skillCategory.CategorySelected = (Action<Category>)Delegate.Combine(skillCategory.CategorySelected, new Action<Category>(OnSelectCategory));
		SkillCategoryInfoWidget skillCategoryInfo = _skillCategoryInfo;
		skillCategoryInfo.InfoButtonClicked = (Action)Delegate.Combine(skillCategoryInfo.InfoButtonClicked, new Action(OnClickInfoButton));
		_skillList.SkillSelected += OnSelectSkill;
		_skillList.SkillGroupSelected += OnSelectSkillGroup;
		_skillInfo.OnLearnSkill += OnLearnSkill;
		_skillInfo.OnUntrainSkill += OnUntrainSkill;
	}

	private void OnEnable()
	{
		GameSystem<SkillSystem>.Instance().CategoryExpChanged += OnChangeCategoryExp;
		GameSystem<SkillSystem>.Instance().SkillListUpdated += OnSkillListUpdate;
	}

	private void OnDisable()
	{
		GameSystem<SkillSystem>.Instance().CategoryExpChanged -= OnChangeCategoryExp;
		GameSystem<SkillSystem>.Instance().SkillListUpdated -= OnSkillListUpdate;
	}

	private void Update()
	{
		if (base.IsOpen)
		{
			float currentOffset = _mainScroll.CurrentOffset;
			if (_scrollOffset != currentOffset)
			{
				_scrollOffset = currentOffset;
				RefreshInfoOffset();
			}
		}
	}

	protected override bool OnOpen()
	{
		base.OnOpen();
		((Component)_skillInfo).gameObject.SetActive(true);
		_mainScroll.Reposition(resetPosition: true, tween: false);
		_skillCategory.Reset();
		_skillCategoryInfo.Set(Category.Invalid);
		_skillCategoryInfo.ButtonVisible(isVisible: true);
		_titleWidget.ShowBackButton(isShow: false, instant: true);
		_scrollOffset = -1f;
		_scrollIndex = -1;
		return true;
	}

	protected override bool OnClose()
	{
		int goalNodeIndex = _mainScroll.GoalNodeIndex;
		if (goalNodeIndex > 0)
		{
			_skillCategory.UpdateData();
			_mainScroll.MoveToNode(0, instant: false);
			return false;
		}
		return base.OnClose();
	}

	public void Open(Category cat, string id, int level)
	{
		bool instant = true;
		if (base.IsOpen)
		{
			instant = false;
		}
		else
		{
			Open();
		}
		_skillCategory.SelectCategory(cat);
		MoveToSkillListPage(instant: true);
		_skillList.SelectSkillGroup(id);
		_skillInfo.SelectSkill(id, "__base__", level, instant);
	}

	private void OnSelectCategory(Category cat)
	{
		_skillCategoryInfo.Set(cat);
	}

	private void OnClickInfoButton()
	{
		MoveToSkillListPage(instant: false);
	}

	private void MoveToSkillListPage(bool instant)
	{
		Category selectedCategory = _skillCategory.SelectedCategory;
		if (selectedCategory != Category.Invalid)
		{
			_skillList.Set(selectedCategory);
			_skillCategoryInfo.ButtonVisible(isVisible: false);
			_mainScroll.MoveToNode(1, instant);
			_titleWidget.SetTitle(T._("{0} [sub]{1:lv:}[/sub]", SkillUtil.CategoryLocalizeName(selectedCategory), GameSystem<SkillSystem>.Instance().GetCategoryLevel(selectedCategory)));
		}
	}

	private void OnLearnSkill(SkillNode skill)
	{
		UIManager.MessageBox.Show(T._("{0:을} 배우시겠습니까?", skill.Name), delegate(bool ok)
		{
			if (ok)
			{
				_skillInfo.SelectSkill(skill.Id, skill.Sub, skill.Level + 1, instant: false);
				GameSystem<SkillSystem>.Instance().LearnSkill(skill.Parent);
			}
		});
	}

	private void OnUntrainSkill(SkillNode skill)
	{
		UIManager.MessageBox.Show(T._("{0:을} 습득 취소 하시겠습니까?", skill.Name), delegate(bool ok)
		{
			if (ok)
			{
				_skillInfo.SelectSkill(skill.Id, skill.Sub, skill.Level - 1, instant: false);
				GameSystem<SkillSystem>.Instance().UntrainSkill(skill.Parent);
			}
		});
	}

	private void OnSelectSkill(SkillBundle skill)
	{
		_skillInfo.Show((skill == null) ? null : new SkillBundle[1] { skill });
	}

	private void OnSelectSkillGroup(IList<SkillBundle> skills)
	{
		_skillInfo.Show(skills);
	}

	private void RefreshInfoOffset()
	{
		int currentNodeIndex = _mainScroll.CurrentNodeIndex;
		if (currentNodeIndex != _scrollIndex)
		{
			_scrollIndex = currentNodeIndex;
			_titleWidget.ShowBackButton(currentNodeIndex > 0);
			_skillCategoryInfo.ButtonVisible(currentNodeIndex == 0);
			if (currentNodeIndex == 0)
			{
				_titleWidget.ResetTitle();
			}
		}
	}

	private void OnSkillListUpdate()
	{
		int num = 0;
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		Array values = Enum.GetValues(typeof(Category));
		for (int i = 0; i < values.Length; i++)
		{
			Category category = (Category)(int)values.GetValue(i);
			if (category == Category.Invalid)
			{
				continue;
			}
			GameSystem<SkillSystem>.Instance().GetCategoryExp(category, out var current, out var max);
			if (current == max)
			{
				SkillCategory skillCategory = GameSystem<SkillSystem>.Instance().GetSkillCategory(category);
				if (!skillCategory.IsResearching() && skillCategory.Level < level)
				{
					num++;
				}
			}
		}
		NewChecker.Count = num;
	}

	private static void OnChangeCategoryExp(Category category, int exp)
	{
		string text = ((exp != 0) ? string.Format("[5695A1][{1}_small:1.5] +{0}[-]", exp, SkillUtil.CategoryIcon(category)) : string.Format("<em>[{1}_small:1.5]{0}</em>", T._("FULL"), SkillUtil.CategoryIcon(category)));
		UIManager.IndicatorMsg(text);
	}
}
