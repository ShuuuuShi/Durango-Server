using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Skill;
using Durango.UI.Control;
using Durango.Utils;
using Shared.Skill;
using UnityEngine;

namespace Durango.UI;

public class SkillTreeWidget : EmptyBoxScrollView
{
	[Serializable]
	private struct LayoutOption
	{
		public float VerticalDistance;

		public int GaugeHeadSize;

		public int GaugeAnimationSpeed;
	}

	[SerializeField]
	private GameObject _mainContainer;

	[SerializeField]
	private GameObject _noSelect;

	[SerializeField]
	private ListObjectPool _depthList;

	[SerializeField]
	private UIPanel _depthPanel;

	[SerializeField]
	private UIWidget _gaugeUpper;

	[SerializeField]
	private ListObjectPool _skillNodes;

	[SerializeField]
	private SkillTreeDirectionSprite _directionSprite;

	[SerializeField]
	private AnimationWidget _moreVScrollArrow;

	[SerializeField]
	private AnimationWidget _moreHScrollArrow;

	[SerializeField]
	private LayoutOption _option;

	private float _scrollOffset;

	private Vector3 _baseIconPos;

	private float _contentsPadding;

	private Vector3 _baseDepthPos;

	private Vector2 _baseDepthOffset;

	private Vector3 _baseGaugePos;

	private int _gaugeWidth;

	private bool _isGaugeAnimation;

	private SkillTreeItem _selected;

	private Shared.Skill.Category _category;

	private bool _isShow;

	private bool _checkScollTo;

	private SkillTreeItem _scrollToNode;

	private bool _isVisibleVScrollArrow;

	private bool _isVisibleHScrollArrow;

	private Point2 _nodesSize;

	private Point2 _nodeCount;

	private ICoroutineBinder _gaugeAnimation;

	private bool _isInit;

	protected override float Size => _nodesSize.x;

	public event Action<Node> SkillSelected;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_baseIconPos = _skillNodes.BaseObject.transform.localPosition;
			_baseDepthPos = _depthPanel.transform.localPosition;
			_baseDepthOffset = _depthPanel.clipOffset;
			_baseGaugePos = _gaugeUpper.transform.localPosition;
			_skillNodes.Init(OnInitSkillNode);
			_nodesSize = new Point2(_depthList.BaseObject.GetComponent<UIWidget>().width, (int)_option.VerticalDistance);
			_contentsPadding = Mathf.Abs(_baseIconPos.y) - (float)_nodesSize.y * 0.5f;
			_moreVScrollArrow.SetAlpha(0f, useTween: false);
			_moreHScrollArrow.SetAlpha(0f, useTween: false);
		}
	}

	private void LateUpdate()
	{
		if (!_isShow)
		{
			return;
		}
		float currentOffset = base.CurrentOffset;
		bool flag = false;
		if (_scrollOffset != currentOffset)
		{
			_scrollOffset = currentOffset;
			SyncOffset();
			if (!_isGaugeAnimation)
			{
				SyncGauge(_gaugeWidth);
			}
			flag = true;
		}
		if (_checkScollTo)
		{
			ScrollToNode(_scrollToNode);
			_checkScollTo = false;
			_scrollToNode = null;
			flag = true;
		}
		if (flag)
		{
			CheckMoreScroll();
		}
	}

	public override int GetNodeCount()
	{
		return _nodeCount.x;
	}

	private void OnClick()
	{
		OnSelectSkill(null);
	}

	private void SyncOffset()
	{
		float scrollOffset = _scrollOffset;
		Vector3 localPosition = _baseDepthPos + Vector3.left * scrollOffset;
		_depthPanel.transform.localPosition = localPosition;
		_depthPanel.clipOffset = _baseDepthOffset + Vector2.right * scrollOffset;
		if (_depthList.Count > 0 && scrollOffset < 0f)
		{
			SkillTreeDepthNode component = _depthList[0].GetComponent<SkillTreeDepthNode>();
			component.BgOffset(scrollOffset);
		}
	}

	private void SyncGauge(int width)
	{
		float scrollOffset = _scrollOffset;
		if (scrollOffset < 0f)
		{
			_gaugeUpper.transform.localPosition = _baseGaugePos;
			_gaugeUpper.width = width - (int)scrollOffset;
		}
		else
		{
			_gaugeUpper.transform.localPosition = _baseGaugePos + Vector3.left * scrollOffset;
			_gaugeUpper.width = width;
		}
	}

	private void BeginGaugeAnimation()
	{
		this.StartCoroutine(ref _gaugeAnimation, CoGaugeAnimation());
	}

	private IEnumerator CoGaugeAnimation()
	{
		float width = _option.GaugeHeadSize;
		_isGaugeAnimation = true;
		int depthIndex = 0;
		int widthSum = 0;
		while (true)
		{
			width += (float)_option.GaugeAnimationSpeed * Time.deltaTime;
			if (depthIndex < _depthList.Count)
			{
				int num = (int)width - widthSum;
				SkillTreeDepthNode component = _depthList[depthIndex].GetComponent<SkillTreeDepthNode>();
				if (component.Widget.width <= num)
				{
					component.BackgroundEnable(enable: true);
					depthIndex++;
					widthSum += component.Widget.width;
				}
			}
			if (width > (float)_gaugeWidth)
			{
				break;
			}
			SyncGauge((int)width);
			yield return null;
		}
		SyncGauge(_gaugeWidth);
		_isGaugeAnimation = false;
	}

	private void OnInitSkillNode(GameObject obj)
	{
		SkillTreeItem component = obj.GetComponent<SkillTreeItem>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnSkillNodeClick));
	}

	private void ScrollToNode(SkillTreeItem node)
	{
		if (node == null)
		{
			Reposition();
			if (base.ScrollView.transform.localPosition.y > 0f)
			{
				float num = base.ViewBreadth + base.ScrollView.transform.localPosition.y;
				Bounds bounds = base.ScrollView.bounds;
				if (num > bounds.size.y)
				{
					base.ScrollView.Press(pressed: false);
					SpringPanel springPanel = SpringPanel.Begin(base.ScrollView.gameObject, base.ScrollView.transform.localPosition, 8f);
					springPanel.target.y -= num - bounds.size.y;
				}
			}
			return;
		}
		MoveToVisibleArea(node.Depth, instant: false);
		float contentsPadding = _contentsPadding;
		Vector3 position = node.Widget.GetPosition(0f, 1f);
		float y = base.ScrollView.transform.localPosition.y;
		float num2 = y + contentsPadding;
		float num3 = 0f - position.y;
		if (num3 < num2)
		{
			SpringPanel springPanel2 = base.ScrollView.gameObject.AddMissingComponent<SpringPanel>();
			Vector3 localPosition = base.ScrollView.transform.localPosition;
			localPosition.y -= num2 - num3;
			if (springPanel2.enabled)
			{
				localPosition.x = springPanel2.target.x;
				springPanel2.target = localPosition;
				return;
			}
			springPanel2.strength = 8f;
			springPanel2.onFinished = null;
			springPanel2.target = localPosition;
			springPanel2.enabled = true;
			return;
		}
		float num4 = y + base.ViewBreadth - contentsPadding;
		float num5 = num3 + (float)node.Widget.height;
		if (num5 > num4)
		{
			SpringPanel springPanel3 = base.ScrollView.gameObject.AddMissingComponent<SpringPanel>();
			Vector3 localPosition2 = base.ScrollView.transform.localPosition;
			localPosition2.y -= num4 - num5;
			if (springPanel3.enabled)
			{
				localPosition2.x = springPanel3.target.x;
				springPanel3.target = localPosition2;
				return;
			}
			springPanel3.strength = 8f;
			springPanel3.onFinished = null;
			springPanel3.target = localPosition2;
			springPanel3.enabled = true;
		}
	}

	private void CheckMoreScroll()
	{
		CheckMoreVScroll();
		CheckMoreHScroll();
	}

	private void CheckMoreVScroll()
	{
		float num = base.ViewBreadth + base.ScrollView.transform.localPosition.y + _contentsPadding;
		bool flag = num < base.ScrollView.bounds.size.y - (float)_nodesSize.y * 0.5f;
		if (_isVisibleVScrollArrow != flag)
		{
			_isVisibleVScrollArrow = flag;
			_moreVScrollArrow.Alpha = ((!flag) ? 0f : 1f);
		}
	}

	private void CheckMoreHScroll()
	{
		float currentOffset = base.CurrentOffset;
		bool flag = currentOffset < base.MaxOffset - (float)_nodesSize.x * 0.5f;
		if (_isVisibleHScrollArrow != flag)
		{
			_isVisibleHScrollArrow = flag;
			_moreHScrollArrow.Alpha = ((!flag) ? 0f : 1f);
		}
	}

	private void OnSkillNodeClick()
	{
		SkillTreeItem skillTreeItem = Selectable.Current as SkillTreeItem;
		if (!(skillTreeItem == null))
		{
			OnSelectSkill(skillTreeItem.Skill);
		}
	}

	public void SelectSkill(string id, string subId, int level)
	{
		SkillTreeItem skillTreeItem = FindSkill(id, subId, level);
		OnSelectSkill((!(skillTreeItem != null)) ? null : skillTreeItem.Skill);
	}

	public void ScrollTo(string id, string subId, int level)
	{
		SkillTreeItem skillTreeItem = FindSkill(id, subId, level);
		if (skillTreeItem != null)
		{
			_checkScollTo = true;
			_scrollToNode = skillTreeItem;
		}
	}

	public SkillTreeItem FindSkill(string id, string subId, int level)
	{
		for (int i = 0; i < _skillNodes.Count; i++)
		{
			SkillTreeItem component = _skillNodes[i].GetComponent<SkillTreeItem>();
			if (component.Skill.Id == id && component.Skill.Sub == subId && component.Skill.Level == level)
			{
				return component;
			}
		}
		return null;
	}

	private void OnSelectSkill(Node skill)
	{
		Node obj = null;
		_selected = null;
		for (int i = 0; i < _skillNodes.Count; i++)
		{
			SkillTreeItem component = _skillNodes[i].GetComponent<SkillTreeItem>();
			if (component.Skill == skill)
			{
				if (component.Selected)
				{
					component.Selected = false;
					continue;
				}
				_selected = component;
				obj = skill;
				component.Selected = true;
			}
			else
			{
				component.Selected = false;
			}
		}
		_checkScollTo = true;
		if (_selected != null)
		{
			_scrollToNode = _selected;
		}
		if (this.SkillSelected != null)
		{
			this.SkillSelected(obj);
		}
	}

	private void UpdateLayout()
	{
		UIWidget component = GetComponent<UIWidget>();
		_gaugeWidth = 0;
		int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(_category);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < _depthList.Count; i++)
		{
			SkillTreeDepthNode component2 = _depthList[i].GetComponent<SkillTreeDepthNode>();
			component2.Widget.height = component.height;
			if (_gaugeWidth == 0)
			{
				if (categoryLevel >= component2.Lv)
				{
					num += component2.Widget.width;
				}
				else
				{
					_gaugeWidth = num;
					float num3 = (float)(categoryLevel - num2) / (float)(component2.Lv - num2);
					_gaugeWidth += (int)((float)component2.Widget.width * num3);
				}
			}
			num2 = component2.Lv;
		}
		_gaugeWidth = ((_gaugeWidth != 0) ? _gaugeWidth : num) + _option.GaugeHeadSize;
		UIUtility.WidgetsReposition(_depthList, Vector3.right, Vector3.down * component.height * 0.5f);
		base.ScrollView.movement = UIScrollView.Movement.Unrestricted;
		base.ScrollView.ResetPosition();
		_scrollOffset = -1f;
	}

	protected override Bounds GetScrollBounds()
	{
		Bounds bounds = base.GetScrollBounds();
		float num = (float)(_nodeCount.y * _nodesSize.y) + _contentsPadding;
		if (num > bounds.size.y)
		{
			Vector3 center = bounds.center;
			Vector3 size = bounds.size;
			center.y -= (num - bounds.size.y) * 0.5f;
			size.y = num;
			bounds = new Bounds(center, size);
		}
		UpdateScrollMovement(bounds);
		return bounds;
	}

	private void MakeSkillTree(IList<Bundle> skills)
	{
		Init();
		_isShow = true;
		_mainContainer.gameObject.SetActive(value: true);
		_noSelect.gameObject.SetActive(value: false);
		_category = skills[0].Category;
		Vector3 baseIconPos = _baseIconPos;
		Point2 nodesSize = _nodesSize;
		int num = -1;
		int num2 = 0;
		List<Node> list = new List<Node>();
		int i = 0;
		for (int count = skills.Count; i < count; i++)
		{
			int j = 0;
			for (int num3 = KUtility.GetSize(skills[i].Sub) + 1; j < num3; j++)
			{
				list.Add((j != 0) ? skills[i].Sub[j - 1].Get(1) : skills[i].Base.Get(1));
			}
		}
		int[] array = new int[list.Count];
		bool[] array2 = new bool[list.Count];
		int[] array3 = new int[list.Count];
		array[0] = list[0].Parent.Get(list[0].Parent.MaxLevel).CategoryLevel;
		for (int k = 1; k < list.Count; k++)
		{
			array3[k] = -1;
			for (int l = array3[k - 1] + 1; l < k; l++)
			{
				if (!array2[l])
				{
					array3[k] = l;
					break;
				}
			}
			if (array3[k] == -1)
			{
				array3[k] = k;
			}
			int categoryLevel = list[k].CategoryLevel;
			int num4 = -1;
			if (list[k].Parent.Bundle.Base == list[k].Parent)
			{
				for (int num5 = array3[k] - 1; num5 >= 0; num5--)
				{
					if (array[num5] < categoryLevel)
					{
						num4 = num5;
					}
				}
			}
			else
			{
				Skill @base = list[k].Parent.Bundle.Base;
				if (@base.MaxLevel == 1 && @base == list[k - 1].Parent && array[k - 1] <= categoryLevel)
				{
					num4 = array3[k - 1];
				}
			}
			if (num4 != -1)
			{
				array3[k] = num4;
				array[num4] = list[k].Parent.Get(list[k].Parent.MaxLevel).CategoryLevel;
			}
			int num6 = array3[k];
			array2[num6] = true;
			array[num6] = Mathf.Max(array[num6], list[k].Parent.Get(list[k].Parent.MaxLevel).CategoryLevel);
		}
		int[] array4 = new int[list.Count];
		int[] array5 = new int[list.Count];
		Point2[] array6 = new Point2[skills.Count];
		for (int m = 0; m < array6.Length; m++)
		{
			ref Point2 reference = ref array6[m];
			reference = -Point2.one;
		}
		_depthList.BeginLoad();
		_skillNodes.BeginLoad();
		_directionSprite.Clear();
		_nodeCount = Point2.zero;
		while (true)
		{
			int num7 = -1;
			int num8 = int.MaxValue;
			int num9 = int.MaxValue;
			bool flag = false;
			bool flag2 = false;
			int num10 = -1;
			for (int n = 0; n < list.Count; n++)
			{
				if (list[n] == null || list[n].CategoryLevel > num8)
				{
					continue;
				}
				bool flag3 = list[n].Level == 1;
				bool flag4 = flag3 && list[n].Parent.Bundle.Base == list[n].Parent;
				int num11 = skills.IndexOf(list[n].Parent.Bundle);
				if (flag4 || !(array6[num11] == -Point2.one))
				{
					if (list[n].CategoryLevel < num8 || array4[n] < num9)
					{
						num9 = array4[n];
						num7 = n;
						flag = flag3;
						flag2 = flag4;
						num10 = num11;
					}
					num8 = list[n].CategoryLevel;
				}
			}
			if (num7 == -1)
			{
				break;
			}
			if (num < 0)
			{
				num = list[num7].CategoryLevel;
			}
			else if (num < list[num7].CategoryLevel)
			{
				SkillTreeDepthNode component = _depthList.GetNext().GetComponent<SkillTreeDepthNode>();
				int num12 = MaxDepth(array4);
				component.Set(num, nodesSize.x * (num12 - num2));
				for (int num13 = num2; num13 < num12; num13++)
				{
					_nodeCount.x++;
				}
				for (int num14 = 0; num14 < array4.Length; num14++)
				{
					array4[num14] = num12;
				}
				num2 = num12;
				num = list[num7].CategoryLevel;
			}
			AddTreeNode(list[num7], baseIconPos, nodesSize, array4[num7], array3[num7]);
			_nodeCount.y = Mathf.Max(_nodeCount.y, array3[num7] + 1);
			if (flag2)
			{
				ref Point2 reference2 = ref array6[num10];
				reference2 = new Point2(array4[num7], array3[num7]);
				for (int num15 = num7 + 1; num15 < list.Count && list[num15] != null && list[num15].Parent.Bundle == list[num7].Parent.Bundle; num15++)
				{
					array4[num15] = array4[num7] + 1;
				}
			}
			else if (flag)
			{
				_directionSprite.Add(array6[num10], new Point2(array4[num7], array3[num7]), list[num7]);
			}
			else
			{
				_directionSprite.Add(new Point2(array5[num7], array3[num7]), new Point2(array4[num7], array3[num7]), list[num7]);
			}
			array5[num7] = array4[num7];
			array4[num7]++;
			list[num7] = list[num7].Parent.Get(list[num7].Level + 1);
		}
		int num16 = MaxDepth(array4);
		if (num16 > num2)
		{
			SkillTreeDepthNode component2 = _depthList.GetNext().GetComponent<SkillTreeDepthNode>();
			component2.Set(num, nodesSize.x * (num16 - num2));
			for (int num17 = num2; num17 < num16; num17++)
			{
				_nodeCount.x++;
			}
		}
		_depthList.EndLoad();
		_skillNodes.EndLoad();
		ResetPosition();
	}

	protected override float OnUpdateLayout(bool instant)
	{
		float result = base.OnUpdateLayout(instant);
		Vector3 size = base.ScrollView.bounds.size;
		_directionSprite.SetDimensions((int)size.x, (int)size.y);
		_directionSprite.pivot = UIWidget.Pivot.TopLeft;
		_directionSprite.transform.localPosition = Vector3.zero;
		return result;
	}

	private int MaxDepth(int[] list)
	{
		int num = 0;
		for (int i = 0; i < list.Length; i++)
		{
			num = Mathf.Max(list[i], num);
		}
		return num;
	}

	private void AddTreeNode(Node skill, Vector3 pos, Point2 size, int x, int y)
	{
		SkillTreeItem component = _skillNodes.GetNext().GetComponent<SkillTreeItem>();
		component.Set(skill);
		component.Depth = x;
		component.transform.localPosition = pos + new Vector3(size.x * x, -size.y * y);
	}

	public void Set(IList<Bundle> skills)
	{
		MakeSkillTree(skills);
		UpdateLayout();
		BeginGaugeAnimation();
		UpdateData();
		OnSelectSkill(null);
	}

	public void Hide()
	{
		_isShow = false;
		_mainContainer.gameObject.SetActive(value: false);
		_noSelect.gameObject.SetActive(value: true);
		OnSelectSkill(null);
	}

	public void UpdateData()
	{
		for (int i = 0; i < _skillNodes.Count; i++)
		{
			_skillNodes[i].GetComponent<SkillTreeItem>().UpdateData();
		}
		_directionSprite.Draw(_baseIconPos, _nodesSize);
	}

	public void UpdateScrollMovement()
	{
		UpdateScrollMovement(base.ScrollView.bounds);
	}

	private void UpdateScrollMovement(Bounds bounds)
	{
		float viewBreadth = base.ViewBreadth;
		base.ScrollView.movement = ((Mathf.Ceil(viewBreadth) < Mathf.Floor(bounds.size.y)) ? UIScrollView.Movement.Unrestricted : UIScrollView.Movement.Horizontal);
	}
}
