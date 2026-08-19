using UnityEngine;

namespace Durango.UI.Control;

public class KGridScrollView : NodesScrollView
{
	[SerializeField]
	private int _rowMargin;

	[SerializeField]
	private ListObjectPool _spliter;

	[SerializeField]
	private ScrollViewGridBackground _gridBackground;

	[SerializeField]
	private float _rowPivot;

	private float _rowSize;

	private float _colSize;

	private int _rowItemCount;

	private Vector2 GridSize => (base.Vector.x != 0f) ? new Vector2(_colSize, _rowSize) : new Vector2(_rowSize, _colSize);

	private void RefreshGridBackground()
	{
		if (_gridBackground != null && _colSize > 0f && _rowSize > 0f)
		{
			ScrollViewGridBackground.Horizontal horizontal = ((base.Vector.x < 0f) ? ScrollViewGridBackground.Horizontal.Right : ScrollViewGridBackground.Horizontal.Left);
			ScrollViewGridBackground.Vertical vertical = ((!(base.Vector.y > 0f)) ? ScrollViewGridBackground.Vertical.Top : ScrollViewGridBackground.Vertical.Bottom);
			_gridBackground.ResetGrid(GridSize, base.Vector * base.CurrentOffset, horizontal, vertical);
		}
	}

	protected override float OnUpdateLayout(bool instant)
	{
		Vector3 vector = base.Vector;
		Vector3 vector2 = ((vector.x != 0f) ? Vector3.down : Vector3.right);
		Vector3 basePosition = GetBasePosition();
		basePosition -= vector2 * UIUtility.GetSize(base.ViewSize, vector2) * 0.5f;
		Vector2 localSize = base.Nodes.BaseObject.GetComponent<UIWidget>().localSize;
		Vector2 size = UIUtility.WidgetsGridReposition(base.Nodes, _spliter, vector, basePosition, UIUtility.GetBreadth(base.ViewSize, vector), localSize, _rowMargin, base.Margin, out _rowItemCount, out _rowSize, out _colSize, _rowPivot, null, instant);
		RefreshGridBackground();
		return UIUtility.GetSize(size, vector);
	}

	public override float GetNodeOffset(int index)
	{
		if (_rowItemCount > 0)
		{
			int num = Mathf.Max(Mathf.FloorToInt((float)index / (float)_rowItemCount), 0);
			return (_colSize + (float)base.Margin) * (float)num;
		}
		return 0f;
	}

	protected override int CalcNodeIndex(float offset)
	{
		if (_rowItemCount > 0)
		{
			int num = Mathf.Max(0, Mathf.FloorToInt(offset / (_colSize + (float)base.Margin)));
			return num * _rowItemCount;
		}
		return 0;
	}

	protected override int ToIntOffset(int currentIndex, int sign)
	{
		if (_rowItemCount > 0)
		{
			int num = Mathf.Max(Mathf.FloorToInt((float)currentIndex / (float)_rowItemCount), 0);
			num = Mathf.Max(num, num + sign);
			return num * _rowItemCount;
		}
		return base.ToIntOffset(currentIndex, sign);
	}
}
