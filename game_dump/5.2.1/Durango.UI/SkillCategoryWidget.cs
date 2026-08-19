using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.UI.Control;
using Shared.Skill;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class SkillCategoryWidget : MonoBehaviour, IUIInitializable
{
	public Action<Category> CategorySelected;

	[SerializeField]
	private KGridScrollView _categories;

	private readonly List<Category> _categoryList = new List<Category>();

	public Category SelectedCategory { get; private set; }

	void IUIInitializable.Init()
	{
		RegionTemplate regionTemplate = SingletonDict<string, RegionTemplate>.Get(GameManager.Region.TemplateId);
		if (regionTemplate == null)
		{
			return;
		}
		SkillSystem skillSystem = GameSystem<SkillSystem>.Instance();
		int skillCategoryCount = skillSystem.SkillCategoryCount;
		for (int i = 0; i < skillCategoryCount; i++)
		{
			Category type = skillSystem.GetSkillCategory(i).Type;
			SkillCategory skillCategory = SingletonDict<Category, SkillCategory>.Instance.Get(type);
			if (skillCategory != null && skillCategory.Season != regionTemplate.Season)
			{
				bool flag = string.IsNullOrEmpty(skillCategory.Season);
				if (regionTemplate.UnmatchedSkillAndRecipeHided || !flag)
				{
					continue;
				}
			}
			_categoryList.Add(type);
		}
		ListObjectPool nodes = _categories.Nodes;
		nodes.Init(OnInitCategoryNode);
		nodes.Set(_categoryList.Count);
		UIManager.AddOnScreenResized(delegate
		{
			_categories.ResetPosition();
		});
	}

	private void OnEnable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated += OnSkillListUpdate;
	}

	private void OnDisable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated -= OnSkillListUpdate;
	}

	private void OnInitCategoryNode(GameObject obj)
	{
		SkillCategoryNode component = obj.GetComponent<SkillCategoryNode>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickSkillCategory));
	}

	public void SelectCategory(Category category)
	{
		if (Selectable.Current != null)
		{
			Selectable.Current.Selected = false;
		}
		for (int i = 0; i < _categories.Nodes.Count; i++)
		{
			SkillCategoryNode component = _categories.Nodes[i].GetComponent<SkillCategoryNode>();
			if (component.Category.Type == category)
			{
				Selectable.Current = component;
				break;
			}
		}
		OnClickSkillCategory();
	}

	public SkillCategoryNode GetCategoryNode(Category category)
	{
		for (int i = 0; i < _categories.Nodes.Count; i++)
		{
			SkillCategoryNode component = _categories.Nodes[i].GetComponent<SkillCategoryNode>();
			if (component.Category.Type == category)
			{
				return component;
			}
		}
		return null;
	}

	private void OnClickSkillCategory()
	{
		SkillCategoryNode skillCategoryNode = Selectable.Current as SkillCategoryNode;
		if (skillCategoryNode != null && skillCategoryNode.Selected)
		{
			skillCategoryNode = null;
		}
		int num = ((!(skillCategoryNode == null)) ? _categories.Nodes.IndexOf(skillCategoryNode.gameObject) : (-1));
		for (int i = 0; i < _categories.Nodes.Count; i++)
		{
			_categories.Nodes[i].GetComponent<SkillCategoryNode>().Selected = i == num;
		}
		Category category2 = (SelectedCategory = ((!(skillCategoryNode == null)) ? skillCategoryNode.Category.Type : Category.Invalid));
		Category obj = category2;
		if (CategorySelected != null)
		{
			CategorySelected(obj);
		}
	}

	private void OnSkillListUpdate()
	{
		_categoryList.Sort(CategoryComparison);
		UpdateData();
	}

	public void UpdateData()
	{
		for (int i = 0; i < _categoryList.Count; i++)
		{
			SkillCategoryNode component = _categories.Nodes[i].GetComponent<SkillCategoryNode>();
			component.Set(_categoryList[i]);
			component.Selected = SelectedCategory == _categoryList[i];
		}
	}

	private int CategoryComparison(Category c1, Category c2)
	{
		SkillSystem skillSystem = GameSystem<SkillSystem>.Instance();
		int num = skillSystem.GetCategoryLevel(c2) - skillSystem.GetCategoryLevel(c1);
		if (num == 0)
		{
			num = skillSystem.GetCategoryUsedSp(c2) - skillSystem.GetCategoryUsedSp(c1);
		}
		if (num == 0)
		{
			num = c1 - c2;
		}
		return num;
	}

	public void Unselect()
	{
		SelectedCategory = Category.Invalid;
		OnSkillListUpdate();
	}
}
