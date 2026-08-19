using System;
using System.Collections;
using System.Collections.Generic;
using Shared.Skill;
using SkillData;
using UnityEngine;

public class SkillTreeWidget : MonoBehaviour
{
	[Serializable]
	private struct LayoutOption
	{
		public float VerticalDistance;

		[Range(0f, 1f)]
		public float LineBegin;

		[Range(0f, 1f)]
		public float LineEnd;

		[Range(0f, 1f)]
		public float SplitLineBegin;

		public int GaugeHeadSize;

		public int GaugeAnimationSpeed;
	}

	private struct ResizeStruct
	{
		public bool Flag;

		public int Offset;

		public bool Instant;

		public void Set(int offset, bool instant)
		{
			Flag = true;
			Offset = offset;
			Instant |= instant;
		}

		public void Reset()
		{
			Flag = false;
			Offset = 0;
			Instant = false;
		}
	}

	[SerializeField]
	private GameObject _mainContainer;

	[SerializeField]
	private GameObject _noSelect;

	[SerializeField]
	private KScrollView _treeScroll;

	[SerializeField]
	private ListObjectPool _depthList;

	[SerializeField]
	private UIPanel _depthPanel;

	[SerializeField]
	private UIWidget _gaugeUpper;

	[SerializeField]
	private ListObjectPool _skillNodes;

	[SerializeField]
	private ListObjectPool _lines;

	[SerializeField]
	private ListObjectPool _arrows;

	[SerializeField]
	private AnimationWidget _moreVScrollArrow;

	[SerializeField]
	private AnimationWidget _moreHScrollArrow;

	[SerializeField]
	private LayoutOption _option;

	[SerializeField]
	private UIRect[] _resizeObjects;

	private int[] _resizeOriginWidths;

	private float _scrollOffset;

	private Vector3 _baseDepthPos;

	private Vector2 _baseDepthOffset;

	private Vector3 _baseGaugePos;

	private int _gaugeWidth;

	private bool _isGaugeAnimation;

	private SkillTreeItem _selected;

	private Category _category;

	private bool _isShow;

	private int _offset;

	private int _currentOffset;

	private bool _checkSelectedNodeVisble;

	private bool _isVisibleVScrollArrow;

	private bool _isVisibleHScrollArrow;

	private ResizeStruct _resizeArgument;

	private bool _isInit;

	public event Action<SkillNode> SkillSelected;

	private void Init()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_baseDepthPos = ((Component)_depthPanel).transform.localPosition;
			_baseDepthOffset = _depthPanel.clipOffset;
			_baseGaugePos = ((Component)_gaugeUpper).transform.localPosition;
			_skillNodes.Init(OnInitSkillNode);
			_resizeOriginWidths = new int[_resizeObjects.Length];
			for (int i = 0; i < _resizeObjects.Length; i++)
			{
				_resizeObjects[i].SetAnchor((Transform)null);
				Vector2 val = Vector2.op_Implicit(_resizeObjects[i].localCorners[2] - _resizeObjects[i].localCorners[0]);
				_resizeOriginWidths[i] = (int)val.x;
			}
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
		float currentOffset = _treeScroll.CurrentOffset;
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
		LateResize();
		if (_checkSelectedNodeVisble)
		{
			SelectNodeMoveToVisibleArea();
			flag = true;
		}
		if (flag)
		{
			CheckMoreScroll();
		}
	}

	private void OnPress(bool isPress)
	{
		if (isPress)
		{
			OnSelectSkill(null);
		}
	}

	private void SyncOffset()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		float scrollOffset = _scrollOffset;
		Vector3 localPosition = _baseDepthPos + Vector3.left * scrollOffset;
		((Component)_depthPanel).transform.localPosition = localPosition;
		_depthPanel.clipOffset = _baseDepthOffset + Vector2.right * scrollOffset;
		if (_depthList.Count > 0 && scrollOffset < 0f)
		{
			SkillTreeDepthNode component = _depthList[0].GetComponent<SkillTreeDepthNode>();
			component.BgOffset(scrollOffset);
		}
	}

	private void SyncGauge(int width)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		float scrollOffset = _scrollOffset;
		if (scrollOffset < 0f)
		{
			((Component)_gaugeUpper).transform.localPosition = _baseGaugePos;
			_gaugeUpper.width = width - (int)scrollOffset;
		}
		else
		{
			((Component)_gaugeUpper).transform.localPosition = _baseGaugePos + Vector3.left * scrollOffset;
			_gaugeUpper.width = width;
		}
	}

	private void BeginGaugeAnimation()
	{
		((MonoBehaviour)this).StopCoroutine("CoGaugeAnimation");
		((MonoBehaviour)this).StartCoroutine("CoGaugeAnimation");
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
				int w = (int)width - widthSum;
				SkillTreeDepthNode node = _depthList[depthIndex].GetComponent<SkillTreeDepthNode>();
				if (node.Widget.width <= w)
				{
					node.BackgroundEnable(enable: true);
					depthIndex++;
					widthSum += node.Widget.width;
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

	private void SelectNodeMoveToVisibleArea()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		_checkSelectedNodeVisble = false;
		if ((Object)(object)_selected == (Object)null)
		{
			_treeScroll.MoveTo(_treeScroll.CurrentOffset, instant: false);
			return;
		}
		_treeScroll.MoveToVisibleArea(_selected.Depth, instant: false);
		float y = _treeScroll.GetRawOffset(_skillNodes.BaseObject.GetComponent<UIWidget>().GetPosition(0f, 1f)).y;
		Vector3 position = _selected.Widget.GetPosition(0f, 1f);
		float num = 0f - _treeScroll.GetCurrentRawOffset().y;
		float num2 = num + y;
		float y2 = _treeScroll.GetRawOffset(position).y;
		if (y2 < num2)
		{
			SpringPanel component = ((Component)_treeScroll).GetComponent<SpringPanel>();
			ref Vector3 target = ref component.target;
			target.y -= num2 - y2;
			((Behaviour)component).enabled = true;
			return;
		}
		float num3 = num + _treeScroll.BoxBreadth - y;
		float num4 = y2 + (float)_selected.Widget.height;
		if (num4 > num3)
		{
			SpringPanel component2 = ((Component)_treeScroll).GetComponent<SpringPanel>();
			ref Vector3 target2 = ref component2.target;
			target2.y -= num3 - num4;
			((Behaviour)component2).enabled = true;
		}
	}

	private void CheckMoreScroll()
	{
		CheckMoreVScroll();
		CheckMoreHScroll();
	}

	private void CheckMoreVScroll()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		float num = _treeScroll.BoxBreadth - _treeScroll.GetCurrentRawOffset().y;
		Bounds bounds = _treeScroll.ScrollView.bounds;
		bool flag = (Object)(object)_selected == (Object)null && Mathf.Ceil(num) < Mathf.Floor(((Bounds)(ref bounds)).size.y);
		if (_isVisibleVScrollArrow != flag)
		{
			_isVisibleVScrollArrow = flag;
			_moreVScrollArrow.Alpha = ((!flag) ? 0f : 1f);
		}
	}

	private void CheckMoreHScroll()
	{
		float currentOffset = _treeScroll.CurrentOffset;
		bool flag = (Object)(object)_selected == (Object)null && currentOffset < _treeScroll.MaxOffset;
		if (_isVisibleHScrollArrow != flag)
		{
			_isVisibleHScrollArrow = flag;
			_moreHScrollArrow.Alpha = ((!flag) ? 0f : 1f);
		}
	}

	private void OnSkillNodeClick()
	{
		SkillTreeItem skillTreeItem = Selectable.Current as SkillTreeItem;
		if (!((Object)(object)skillTreeItem == (Object)null))
		{
			OnSelectSkill(skillTreeItem.Skill);
			_checkSelectedNodeVisble = true;
		}
	}

	public void SelectSkill(string id, string subId, int level)
	{
		for (int i = 0; i < _skillNodes.Count; i++)
		{
			SkillTreeItem component = _skillNodes[i].GetComponent<SkillTreeItem>();
			if (component.Skill.Id == id && component.Skill.Sub == subId && component.Skill.Level == level)
			{
				OnSelectSkill(component.Skill);
				return;
			}
		}
		OnSelectSkill(null);
	}

	private void OnSelectSkill(SkillNode skill)
	{
		_checkSelectedNodeVisble = skill != null;
		SkillNode obj = null;
		_selected = null;
		for (int i = 0; i < _skillNodes.Count; i++)
		{
			SkillTreeItem component = _skillNodes[i].GetComponent<SkillTreeItem>();
			if (component.Skill == skill)
			{
				if (component.Select)
				{
					component.Select = false;
					continue;
				}
				_selected = component;
				obj = skill;
				component.Select = true;
			}
			else
			{
				component.Select = false;
			}
		}
		if (this.SkillSelected != null)
		{
			this.SkillSelected(obj);
		}
	}

	private void UpdateLayout()
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = ((Component)this).GetComponent<UIWidget>();
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
		UIUtility.UpdateAnchors(((Component)_depthPanel).transform);
		_depthList.Reposition(Vector3.right);
		_treeScroll.UpdateLayout();
		_treeScroll.ScrollView.movement = UIScrollView.Movement.Unrestricted;
		_treeScroll.ScrollView.ResetPosition();
		float boxBreadth = _treeScroll.BoxBreadth;
		Bounds bounds = _treeScroll.ScrollView.bounds;
		_treeScroll.ScrollView.movement = ((Mathf.Ceil(boxBreadth) < Mathf.Floor(((Bounds)(ref bounds)).size.y)) ? UIScrollView.Movement.Unrestricted : UIScrollView.Movement.Horizontal);
		_scrollOffset = -1f;
	}

	private void MakeSkillTree(IList<SkillBundle> skills)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		Init();
		_isShow = true;
		_mainContainer.gameObject.SetActive(true);
		_noSelect.gameObject.SetActive(false);
		_category = skills[0].Category;
		_treeScroll.Nodes.Clear();
		_depthList.Clear();
		_skillNodes.Clear();
		_lines.Clear();
		_arrows.Clear();
		Vector3 localPosition = _skillNodes.BaseObject.transform.localPosition;
		Point2 size = new Point2(_depthList.BaseObject.GetComponent<UIWidget>().width, (int)_option.VerticalDistance);
		int num = -1;
		int num2 = 0;
		List<SkillNode> list = new List<SkillNode>();
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
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = list[k].Parent.Get(list[k].Parent.MaxLevel).CategoryLevel;
		}
		int[] array3 = new int[list.Count];
		for (int l = 1; l < array3.Length; l++)
		{
			array3[l] = -1;
			for (int m = array3[l - 1] + 1; m < l; m++)
			{
				if (!array2[m])
				{
					array3[l] = m;
					break;
				}
			}
			if (array3[l] == -1)
			{
				array3[l] = l;
			}
			int categoryLevel = list[l].CategoryLevel;
			int num4 = -1;
			if (list[l].Parent.Parent.Base == list[l].Parent)
			{
				for (int num5 = array3[l] - 1; num5 >= 0; num5--)
				{
					if (array[num5] < categoryLevel)
					{
						num4 = num5;
					}
				}
			}
			else
			{
				Skill @base = list[l].Parent.Parent.Base;
				if (@base.MaxLevel == 1 && @base == list[l - 1].Parent && array[l - 1] <= categoryLevel)
				{
					num4 = l - 1;
				}
			}
			if (num4 != -1)
			{
				array3[l] = num4;
				array[num4] = list[l].Parent.Get(list[l].Parent.MaxLevel).CategoryLevel;
			}
			array2[array3[l]] = true;
		}
		int[] array4 = new int[list.Count];
		int[] array5 = new int[list.Count];
		int[] array6 = new int[skills.Count];
		for (int n = 0; n < array6.Length; n++)
		{
			array6[n] = -1;
		}
		while (true)
		{
			int num6 = -1;
			int num7 = int.MaxValue;
			int num8 = int.MaxValue;
			bool flag = false;
			bool flag2 = false;
			int num9 = -1;
			for (int num10 = 0; num10 < list.Count; num10++)
			{
				if (list[num10] == null || list[num10].CategoryLevel > num7)
				{
					continue;
				}
				bool flag3 = list[num10].Level == 1;
				bool flag4 = flag3 && list[num10].Parent.Parent.Base == list[num10].Parent;
				int num11 = skills.IndexOf(list[num10].Parent.Parent);
				if (flag4 || array6[num11] != -1)
				{
					if (list[num10].CategoryLevel < num7 || array4[num10] < num8)
					{
						num8 = array4[num10];
						num6 = num10;
						flag = flag3;
						flag2 = flag4;
						num9 = num11;
					}
					num7 = list[num10].CategoryLevel;
				}
			}
			if (num6 == -1)
			{
				break;
			}
			if (num < 0)
			{
				num = list[num6].CategoryLevel;
			}
			else if (num < list[num6].CategoryLevel)
			{
				SkillTreeDepthNode skillTreeDepthNode = ((ListObjectPoolBase<GameObject>)_depthList).Add<SkillTreeDepthNode>();
				int num12 = Mathf.Max(array4);
				skillTreeDepthNode.Set(num, size.x * (num12 - num2));
				for (int num13 = num2; num13 < num12; num13++)
				{
					_treeScroll.Nodes.Add();
				}
				for (int num14 = 0; num14 < array4.Length; num14++)
				{
					array4[num14] = num12;
				}
				num2 = num12;
				num = list[num6].CategoryLevel;
			}
			AddTreeNode(list[num6], localPosition, size, array4[num6], array3[num6]);
			if (!flag)
			{
				DrawArrowLine(localPosition.x + (float)size.x * ((float)array5[num6] + _option.LineBegin), localPosition.x + (float)size.x * ((float)array4[num6] - (1f - _option.LineEnd)), localPosition.y - (float)(size.y * array3[num6]));
			}
			else if (!flag2)
			{
				DrawArrowLine(localPosition.x + (float)size.x * ((float)array6[num9] + _option.SplitLineBegin), localPosition.x + (float)size.x * ((float)array4[num6] - (1f - _option.LineEnd)), localPosition.y - (float)(size.y * array3[num6]));
			}
			else
			{
				array6[num9] = array4[num6];
				for (int num15 = num6 + 1; num15 < list.Count && list[num15] != null && list[num15].Parent.Parent == list[num6].Parent.Parent; num15++)
				{
					array4[num15] = array4[num6] + 1;
				}
				if (KUtility.GetSize(skills[num9].Sub) > 0)
				{
					DrawSplitLine(localPosition.y - (float)(size.y * array3[num6]), localPosition.y - (float)(size.y * array3[num6 + KUtility.GetSize(skills[num9].Sub)]), localPosition.x + (float)size.x * ((float)array4[num6] + _option.SplitLineBegin));
					if (list[num6].Parent.MaxLevel == 1)
					{
						DrawArrowLine(localPosition.x + (float)size.x * ((float)array4[num6] + _option.LineBegin), localPosition.x + (float)size.x * ((float)array4[num6] + _option.SplitLineBegin), localPosition.y - (float)(size.y * array3[num6]), hideArrow: true);
					}
				}
			}
			array5[num6] = array4[num6];
			array4[num6]++;
			list[num6] = list[num6].Parent.Get(list[num6].Level + 1);
		}
		int num16 = Mathf.Max(array4);
		if (num16 > num2)
		{
			SkillTreeDepthNode skillTreeDepthNode2 = ((ListObjectPoolBase<GameObject>)_depthList).Add<SkillTreeDepthNode>();
			skillTreeDepthNode2.Set(num, size.x * (num16 - num2));
			for (int num17 = num2; num17 < num16; num17++)
			{
				_treeScroll.Nodes.Add();
			}
		}
		for (int num18 = 0; num18 < _treeScroll.GetNodeCount(); num18++)
		{
			UIWidget node = _treeScroll.GetNode(num18);
			node.width = size.x;
			node.height = size.x;
		}
	}

	private void AddTreeNode(SkillNode skill, Vector3 pos, Point2 size, int x, int y)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		SkillTreeItem skillTreeItem = ((ListObjectPoolBase<GameObject>)_skillNodes).Add<SkillTreeItem>();
		skillTreeItem.Set(skill);
		skillTreeItem.Depth = x;
		((Component)skillTreeItem).transform.localPosition = pos + new Vector3((float)(size.x * x), (float)(-size.y * y));
	}

	private void DrawArrowLine(float begin, float end, float yPos, bool hideArrow = false)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		UIWidget uIWidget = ((ListObjectPoolBase<GameObject>)_lines).Add<UIWidget>();
		UIWidget uIWidget2 = null;
		if (hideArrow)
		{
			uIWidget.width = (int)(end - begin + (float)uIWidget.height * 0.5f);
		}
		else
		{
			uIWidget2 = ((ListObjectPoolBase<GameObject>)_arrows).Add<UIWidget>();
			uIWidget.width = (int)(end - begin) - uIWidget2.width;
		}
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(begin, yPos);
		((Component)uIWidget).transform.localRotation = Quaternion.identity;
		uIWidget.SetPosition(val, 0f, 0.5f);
		val += Vector3.right * (float)uIWidget.width;
		if ((Object)(object)uIWidget2 != (Object)null)
		{
			uIWidget2.SetPosition(val, 0f, 0.5f);
		}
	}

	private void DrawSplitLine(float begin, float end, float xPos)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		UIWidget uIWidget = ((ListObjectPoolBase<GameObject>)_lines).Add<UIWidget>();
		uIWidget.width = (int)Mathf.Abs(end - begin) + uIWidget.height;
		Vector3 localPosition = default(Vector3);
		((Vector3)(ref localPosition))._002Ector(xPos, Mathf.Lerp(begin, end, 0.5f));
		((Component)uIWidget).transform.localPosition = localPosition;
		((Component)uIWidget).transform.localEulerAngles = Vector3.forward * 90f;
	}

	public void Set(IList<SkillBundle> skills)
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
		_mainContainer.gameObject.SetActive(false);
		_noSelect.gameObject.SetActive(true);
		OnSelectSkill(null);
	}

	public void UpdateData()
	{
		for (int i = 0; i < _skillNodes.Count; i++)
		{
			_skillNodes[i].GetComponent<SkillTreeItem>().UpdateData();
		}
	}

	public void Resize(int offset, bool instant)
	{
		_resizeArgument.Set(offset, instant);
	}

	private void LateResize()
	{
		if (_resizeArgument.Flag)
		{
			_resizeArgument.Flag = false;
			_offset = _resizeArgument.Offset;
			bool flag = _resizeArgument.Instant;
			_resizeArgument.Reset();
			if (_offset == _currentOffset)
			{
				flag = true;
			}
			int width = _mainContainer.gameObject.GetComponent<UIWidget>().width;
			_treeScroll.ResizeBox(width - _offset);
			((MonoBehaviour)this).StopCoroutine("CoResize");
			if (flag)
			{
				SetOffset(_offset);
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine("CoResize");
			}
		}
	}

	private IEnumerator CoResize()
	{
		int goal = _offset;
		int current = _currentOffset;
		float timer = 0f;
		while (true)
		{
			float ratio = timer / 0.3f;
			SetOffset((int)Mathf.Lerp((float)current, (float)goal, Mathf.Clamp01(ratio)));
			if (ratio >= 1f)
			{
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}
	}

	private void SetOffset(int offset)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		_currentOffset = offset;
		for (int i = 0; i < _resizeObjects.Length; i++)
		{
			UIPanel uIPanel = _resizeObjects[i] as UIPanel;
			if ((Object)(object)uIPanel == (Object)null)
			{
				UIWidget uIWidget = _resizeObjects[i] as UIWidget;
				uIWidget.width = _resizeOriginWidths[i] - offset;
				continue;
			}
			int num = _resizeOriginWidths[i] - offset;
			Vector4 baseClipRegion = uIPanel.baseClipRegion;
			baseClipRegion.x = (float)num / 2f;
			baseClipRegion.z = num;
			uIPanel.baseClipRegion = baseClipRegion;
		}
	}
}
