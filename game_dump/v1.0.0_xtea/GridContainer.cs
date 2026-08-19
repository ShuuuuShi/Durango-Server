using System;
using UnityEngine;

public class GridContainer : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _nodes;

	[SerializeField]
	protected int _margin;

	[SerializeField]
	private int _rowMargin;

	[SerializeField]
	private ListObjectPool _spliter;

	private int _rowItemCount;

	[SerializeField]
	private Vector2 _vector;

	private UIWidget _widget;

	public ListObjectPool Nodes => _nodes;

	public Vector2 Vector
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _vector;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_vector = value;
		}
	}

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public void Refresh(bool instant = true)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		float num = UIUtility.CalcGridSize(_nodes, Vector, Widget.localSize, _rowMargin, _margin, out _rowItemCount);
		if (Math.Abs(Vector.x) > 0f)
		{
			Widget.width = (int)num;
		}
		else
		{
			Widget.height = (int)num;
		}
		UpdateLayout(instant);
	}

	private void UpdateLayout(bool instant)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(Vector);
		Vector3 val2 = ((val.x != 0f) ? Vector3.down : Vector3.right);
		Vector3 val3 = val;
		Vector3 val4 = Widget.localCenter - val3 * UIUtility.GetSize(Widget, Vector2.op_Implicit(val3)) * 0.5f;
		val4 -= val2 * UIUtility.GetSize(Widget, Vector2.op_Implicit(val2)) * 0.5f;
		UIUtility.WidgetsGridReposition(_nodes, _spliter, Vector, val4, Widget.localSize, _rowMargin, _margin, instant);
	}
}
