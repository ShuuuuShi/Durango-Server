using System;
using System.Collections.Generic;
using Shared.Skill;
using UnityEngine;

public class SkillCategoryWidget : MonoBehaviour
{
	public Action<Category> CategorySelected;

	[SerializeField]
	private KGridScrollView _categories;

	[SerializeField]
	private Transform _background;

	[SerializeField]
	private ListObjectPool _dotLines;

	private float _cellHeight;

	private List<Category> _categoryList;

	private bool _isInit;

	public Category SelectedCategory { get; private set; }

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_categoryList = new List<Category>();
		Array values = Enum.GetValues(typeof(Category));
		for (int i = 0; i < values.Length; i++)
		{
			Category category = (Category)(int)values.GetValue(i);
			if (category != Category.Invalid)
			{
				_categoryList.Add(category);
			}
		}
		ListObjectPool nodes = _categories.Nodes;
		nodes.Init(OnInitCategoryNode);
		nodes.Set(_categoryList.Count);
		_categories.Reposition(resetPosition: true, tween: false);
	}

	private void OnEnable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated += OnSkillListUpdate;
		MakeBackground();
	}

	private void OnDisable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated -= OnSkillListUpdate;
	}

	private void LateUpdate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = _categories.CurrentOffset % _cellHeight * Vector3.up;
		_background.localPosition = localPosition;
	}

	private void OnInitCategoryNode(GameObject obj)
	{
		SkillCategoryNode component = obj.GetComponent<SkillCategoryNode>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickSkillCategory));
	}

	public void SelectCategory(Category category)
	{
		if ((Object)(object)Selectable.Current != (Object)null)
		{
			Selectable.Current.Select = false;
		}
		for (int i = 0; i < _categories.Nodes.Count; i++)
		{
			SkillCategoryNode component = _categories.Nodes[i].GetComponent<SkillCategoryNode>();
			if (component.Category.Category == category)
			{
				Selectable.Current = component;
				break;
			}
		}
		OnClickSkillCategory();
	}

	private void OnClickSkillCategory()
	{
		if (!Selectable.Current.Disable)
		{
			SkillCategoryNode skillCategoryNode = Selectable.Current as SkillCategoryNode;
			if ((Object)(object)skillCategoryNode != (Object)null && skillCategoryNode.Select)
			{
				skillCategoryNode = null;
			}
			int num = ((!((Object)(object)skillCategoryNode == (Object)null)) ? _categories.Nodes.IndexOf(((Component)skillCategoryNode).gameObject) : (-1));
			for (int i = 0; i < _categories.Nodes.Count; i++)
			{
				SkillCategoryNode component = _categories.Nodes[i].GetComponent<SkillCategoryNode>();
				component.Select = i == num;
			}
			Category obj = (SelectedCategory = ((!((Object)(object)skillCategoryNode == (Object)null)) ? skillCategoryNode.Category.Category : Category.Invalid));
			if (CategorySelected != null)
			{
				CategorySelected(obj);
			}
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
			component.Select = SelectedCategory == _categoryList[i];
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

	public void Reset()
	{
		Init();
		SelectedCategory = Category.Invalid;
		OnSkillListUpdate();
	}

	private void MakeBackground()
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		UIPanel component = ((Component)_categories.ScrollView).GetComponent<UIPanel>();
		float width = component.width;
		float height = component.height;
		UIWidget component2 = _categories.Nodes.BaseObject.GetComponent<UIWidget>();
		Point2 point = new Point2(_categories.RowMargin, _categories.Margin);
		_cellHeight = component2.height + point.y;
		_dotLines.Clear();
		int i = 0;
		Vector2 val = default(Vector2);
		for (int num = Mathf.CeilToInt((width + (float)point.x) / (float)(component2.width + point.x)) - 1; i < num; i++)
		{
			val.x = (float)((i + 1) * component2.width) + (float)point.x * 0.5f - width * 0.5f;
			val.y = (0f - height) * 0.5f;
			UIWidget uIWidget = ((ListObjectPoolBase<GameObject>)_dotLines).Add<UIWidget>();
			uIWidget.width = (int)(height + _cellHeight * 2f);
			((Component)uIWidget).transform.localEulerAngles = Vector3.forward * 90f;
			((Component)uIWidget).transform.localPosition = Vector2.op_Implicit(val);
		}
		int j = 0;
		Vector2 val2 = default(Vector2);
		for (int num2 = Mathf.CeilToInt((height + (float)point.y) / (float)(component2.height + point.y)) + 1; j < num2; j++)
		{
			val2.x = 0f;
			val2.y = 0f - ((float)(j * component2.height) - (float)point.y * 0.5f);
			UIWidget uIWidget2 = ((ListObjectPoolBase<GameObject>)_dotLines).Add<UIWidget>();
			uIWidget2.width = (int)width;
			((Component)uIWidget2).transform.localEulerAngles = Vector3.zero;
			((Component)uIWidget2).transform.localPosition = Vector2.op_Implicit(val2);
		}
	}
}
