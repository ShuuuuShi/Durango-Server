using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using StatisticsData;
using UnityEngine;

public class AutoGuideTitleSelectWidget : MonoBehaviour
{
	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private UIWidget _containerWidget;

	[SerializeField]
	private ListObjectPool _nodes;

	[SerializeField]
	private UISprite _lineVertical;

	[SerializeField]
	private UISprite _lineVerticalCompleted;

	[SerializeField]
	private UIWidget _selector;

	[SerializeField]
	private Transform _seperatorCompleted;

	public Title SelectedTitle { get; private set; }

	public event Action Selected;

	public void Setup([NotNull] IList<Title> titles)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		_nodes.Init(null);
		_nodes.Set(titles.Count);
		UIWidget component = _nodes.BaseObject.GetComponent<UIWidget>();
		int yOffset = -60;
		Vector3 localPosition = ((Component)_lineVertical).transform.localPosition;
		localPosition.y = yOffset;
		((Component)_lineVertical).transform.localPosition = localPosition;
		int num = SetupNodes(titles, component, 0, forCompleted: false, ref yOffset);
		if (num == 0)
		{
			((Component)_lineVertical).gameObject.SetActive(false);
		}
		else
		{
			((Component)_lineVertical).gameObject.SetActive(true);
			float num2 = 0f - ((float)yOffset - localPosition.y);
			_lineVertical.height = (int)num2;
		}
		Vector3 localPosition2 = _seperatorCompleted.localPosition;
		localPosition2.y = yOffset;
		_seperatorCompleted.localPosition = localPosition2;
		yOffset -= 60;
		localPosition = ((Component)_lineVerticalCompleted).transform.localPosition;
		localPosition.y = yOffset;
		((Component)_lineVerticalCompleted).transform.localPosition = localPosition;
		if (SetupNodes(titles, component, num, forCompleted: true, ref yOffset) == 0)
		{
			((Component)_lineVerticalCompleted).gameObject.SetActive(false);
		}
		else
		{
			((Component)_lineVerticalCompleted).gameObject.SetActive(true);
			float num3 = 0f - ((float)yOffset - localPosition.y);
			float num4 = (float)_containerWidget.height + localPosition.y;
			_lineVerticalCompleted.height = (int)Mathf.Max(num4, num3);
		}
		_scrollView.ResetPosition();
		Node_OnClick(_nodes.BaseObject);
	}

	private int SetupNodes(IList<Title> titles, UIWidget widget, int nodeOffset, bool forCompleted, ref int yOffset)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < titles.Count; i++)
		{
			Title title = titles[i];
			float achievementRatio = GameSystem<AutoGuideSystem>.Instance().GetAchievementRatio(title.Id);
			bool flag = achievementRatio >= 1f;
			if (flag == forCompleted)
			{
				AutoGuideTitleNode autoGuideTitleNode = ((ListObjectPoolBase<GameObject>)_nodes).Get<AutoGuideTitleNode>(num + nodeOffset);
				autoGuideTitleNode.Set(title);
				int num2 = num % 2 * widget.width;
				((Component)autoGuideTitleNode).transform.localPosition = new Vector3((float)num2, (float)yOffset, 0f);
				UIEventListener.Get(((Component)autoGuideTitleNode).gameObject).onClick = Node_OnClick;
				num++;
				if (num % 2 == 0)
				{
					yOffset -= widget.height;
				}
			}
		}
		if (num % 2 == 1)
		{
			yOffset -= widget.height;
		}
		return num;
	}

	private void Node_OnClick(GameObject go)
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			AutoGuideTitleNode autoGuideTitleNode = ((ListObjectPoolBase<GameObject>)_nodes).Get<AutoGuideTitleNode>(i);
			autoGuideTitleNode.Selected = false;
		}
		int num = _nodes.IndexOf(go);
		if (num != -1)
		{
			AutoGuideTitleNode autoGuideTitleNode2 = ((ListObjectPoolBase<GameObject>)_nodes).Get<AutoGuideTitleNode>(num);
			autoGuideTitleNode2.Selected = true;
			_selector.SetAnchor(((Component)autoGuideTitleNode2).gameObject);
			SelectedTitle = autoGuideTitleNode2.Title;
			if (this.Selected != null)
			{
				this.Selected();
			}
		}
	}
}
