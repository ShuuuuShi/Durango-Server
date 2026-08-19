using UnityEngine;

public class KGridScrollView : KScrollViewBase
{
	[SerializeField]
	private int _rowMargin;

	[SerializeField]
	private ListObjectPool _nodes;

	[SerializeField]
	private ListObjectPool _spliter;

	[SerializeField]
	private ScrollViewGridBackground _gridBackground;

	private int _rowItemCount;

	public ListObjectPool Nodes => _nodes;

	public int RowMargin => _rowMargin;

	protected override void OnEnable()
	{
		base.OnEnable();
		if ((Object)(object)_gridBackground != (Object)null)
		{
			_gridBackground.Reset();
		}
	}

	public override UIWidget GetNode(int index)
	{
		return _nodes[index].GetComponent<UIWidget>();
	}

	public override int GetNodeCount()
	{
		return _nodes.Count;
	}

	protected override float OnUpdateLayout(bool instant)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_boxWidget == (Object)null)
		{
			return 0f;
		}
		Vector3 vector = base.Vector;
		Vector3 val = ((vector.x != 0f) ? Vector3.down : Vector3.right);
		Vector3 val2 = vector;
		Vector3 val3 = ((Component)_boxWidget).transform.localPosition - val2 * UIUtility.GetSize(_boxWidget, Vector2.op_Implicit(val2)) * 0.5f;
		val3 -= val * UIUtility.GetSize(_boxWidget, Vector2.op_Implicit(val)) * 0.5f;
		float result = UIUtility.CalcGridSize(_nodes, Vector2.op_Implicit(base.Vector), _boxWidget.localSize, _rowMargin, _margin, out _rowItemCount);
		UIUtility.WidgetsGridReposition(_nodes, _spliter, Vector2.op_Implicit(base.Vector), val3, _boxWidget.localSize, _rowMargin, _margin, instant);
		return result;
	}

	public override float GetNodeOffset(int index)
	{
		UIWidget component = _nodes.BaseObject.GetComponent<UIWidget>();
		float size = GetSize(component);
		if (_rowItemCount > 0)
		{
			int num = Mathf.Max(Mathf.CeilToInt((float)index / (float)_rowItemCount) - 1, 0);
			return size * (float)num + (float)(_margin * (num - 1));
		}
		return 0f;
	}
}
