using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Table")]
public class UITable : UIWidgetContainer
{
	public enum Direction
	{
		Down,
		Up
	}

	public enum Sorting
	{
		None,
		Alphabetic,
		Horizontal,
		Vertical,
		Custom
	}

	public delegate void OnReposition();

	public int columns;

	public Direction direction;

	public Sorting sorting;

	public UIWidget.Pivot pivot;

	public UIWidget.Pivot cellAlignment;

	public bool hideInactive = true;

	public bool keepWithinPanel;

	public Vector2 padding = Vector2.zero;

	public OnReposition onReposition;

	public Comparison<Transform> onCustomSort;

	protected UIPanel mPanel;

	protected bool mInitDone;

	protected bool mReposition;

	public bool repositionNow
	{
		set
		{
			if (value)
			{
				mReposition = true;
				((Behaviour)this).enabled = true;
			}
		}
	}

	public List<Transform> GetChildList()
	{
		Transform transform = ((Component)this).transform;
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!hideInactive || (Object.op_Implicit((Object)(object)child) && NGUITools.GetActive(((Component)child).gameObject)))
			{
				list.Add(child);
			}
		}
		if (sorting != 0)
		{
			if (sorting == Sorting.Alphabetic)
			{
				list.Sort(UIGrid.SortByName);
			}
			else if (sorting == Sorting.Horizontal)
			{
				list.Sort(UIGrid.SortHorizontal);
			}
			else if (sorting == Sorting.Vertical)
			{
				list.Sort(UIGrid.SortVertical);
			}
			else if (onCustomSort != null)
			{
				list.Sort(onCustomSort);
			}
			else
			{
				Sort(list);
			}
		}
		return list;
	}

	protected virtual void Sort(List<Transform> list)
	{
		list.Sort(UIGrid.SortByName);
	}

	protected virtual void Start()
	{
		Init();
		Reposition();
		((Behaviour)this).enabled = false;
	}

	protected virtual void Init()
	{
		mInitDone = true;
		mPanel = NGUITools.FindInParents<UIPanel>(((Component)this).gameObject);
	}

	protected virtual void LateUpdate()
	{
		if (mReposition)
		{
			Reposition();
		}
		((Behaviour)this).enabled = false;
	}

	private void OnValidate()
	{
		if (!Application.isPlaying && NGUITools.GetActive((Behaviour)(object)this))
		{
			Reposition();
		}
	}

	protected void RepositionVariableSize(List<Transform> children)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		int num3 = ((columns <= 0) ? 1 : (children.Count / columns + 1));
		int num4 = ((columns <= 0) ? children.Count : columns);
		Bounds[,] array = new Bounds[num3, num4];
		Bounds[] array2 = (Bounds[])(object)new Bounds[num4];
		Bounds[] array3 = (Bounds[])(object)new Bounds[num3];
		int num5 = 0;
		int num6 = 0;
		int i = 0;
		for (int count = children.Count; i < count; i++)
		{
			Transform val = children[i];
			Bounds val2 = NGUIMath.CalculateRelativeWidgetBounds(val, !hideInactive);
			Vector3 localScale = val.localScale;
			((Bounds)(ref val2)).min = Vector3.Scale(((Bounds)(ref val2)).min, localScale);
			((Bounds)(ref val2)).max = Vector3.Scale(((Bounds)(ref val2)).max, localScale);
			array[num6, num5] = val2;
			((Bounds)(ref array2[num5])).Encapsulate(val2);
			((Bounds)(ref array3[num6])).Encapsulate(val2);
			if (++num5 >= columns && columns > 0)
			{
				num5 = 0;
				num6++;
			}
		}
		num5 = 0;
		num6 = 0;
		Vector2 pivotOffset = NGUIMath.GetPivotOffset(cellAlignment);
		int j = 0;
		for (int count2 = children.Count; j < count2; j++)
		{
			Transform val3 = children[j];
			Bounds val4 = array[num6, num5];
			Bounds val5 = array2[num5];
			Bounds val6 = array3[num6];
			Vector3 localPosition = val3.localPosition;
			localPosition.x = num + ((Bounds)(ref val4)).extents.x - ((Bounds)(ref val4)).center.x;
			localPosition.x -= Mathf.Lerp(0f, ((Bounds)(ref val4)).max.x - ((Bounds)(ref val4)).min.x - ((Bounds)(ref val5)).max.x + ((Bounds)(ref val5)).min.x, pivotOffset.x) - padding.x;
			if (direction == Direction.Down)
			{
				localPosition.y = 0f - num2 - ((Bounds)(ref val4)).extents.y - ((Bounds)(ref val4)).center.y;
				localPosition.y += Mathf.Lerp(((Bounds)(ref val4)).max.y - ((Bounds)(ref val4)).min.y - ((Bounds)(ref val6)).max.y + ((Bounds)(ref val6)).min.y, 0f, pivotOffset.y) - padding.y;
			}
			else
			{
				localPosition.y = num2 + ((Bounds)(ref val4)).extents.y - ((Bounds)(ref val4)).center.y;
				localPosition.y -= Mathf.Lerp(0f, ((Bounds)(ref val4)).max.y - ((Bounds)(ref val4)).min.y - ((Bounds)(ref val6)).max.y + ((Bounds)(ref val6)).min.y, pivotOffset.y) - padding.y;
			}
			num += ((Bounds)(ref val5)).size.x + padding.x * 2f;
			val3.localPosition = localPosition;
			if (++num5 >= columns && columns > 0)
			{
				num5 = 0;
				num6++;
				num = 0f;
				num2 += ((Bounds)(ref val6)).size.y + padding.y * 2f;
			}
		}
		if (pivot == UIWidget.Pivot.TopLeft)
		{
			return;
		}
		pivotOffset = NGUIMath.GetPivotOffset(pivot);
		Bounds val7 = NGUIMath.CalculateRelativeWidgetBounds(((Component)this).transform);
		float num7 = Mathf.Lerp(0f, ((Bounds)(ref val7)).size.x, pivotOffset.x);
		float num8 = Mathf.Lerp(0f - ((Bounds)(ref val7)).size.y, 0f, pivotOffset.y);
		Transform transform = ((Component)this).transform;
		for (int k = 0; k < transform.childCount; k++)
		{
			Transform child = transform.GetChild(k);
			SpringPosition component = ((Component)child).GetComponent<SpringPosition>();
			if ((Object)(object)component != (Object)null)
			{
				ref Vector3 target = ref component.target;
				target.x -= num7;
				ref Vector3 target2 = ref component.target;
				target2.y -= num8;
			}
			else
			{
				Vector3 localPosition2 = child.localPosition;
				localPosition2.x -= num7;
				localPosition2.y -= num8;
				child.localPosition = localPosition2;
			}
		}
	}

	[ContextMenu("Execute")]
	public virtual void Reposition()
	{
		if (Application.isPlaying && !mInitDone && NGUITools.GetActive((Behaviour)(object)this))
		{
			Init();
		}
		mReposition = false;
		Transform transform = ((Component)this).transform;
		List<Transform> childList = GetChildList();
		if (childList.Count > 0)
		{
			RepositionVariableSize(childList);
		}
		if (keepWithinPanel && (Object)(object)mPanel != (Object)null)
		{
			mPanel.ConstrainTargetToBounds(transform, immediate: true);
			UIScrollView component = ((Component)mPanel).GetComponent<UIScrollView>();
			if ((Object)(object)component != (Object)null)
			{
				component.UpdateScrollbars(recalculateBounds: true);
			}
		}
		if (onReposition != null)
		{
			onReposition();
		}
	}
}
