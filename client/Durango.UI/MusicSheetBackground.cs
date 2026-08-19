using UnityEngine;

namespace Durango.UI;

public class MusicSheetBackground : MonoBehaviour
{
	[SerializeField]
	private UIWidget _baseLine;

	[SerializeField]
	private UIWidget _termLine;

	[SerializeField]
	private UIWidget _octaveLine;

	[SerializeField]
	private UIWidget _divisionLine;

	private ListObjectPool<UIWidget> _baseLines;

	private ListObjectPool<UIWidget> _termLines;

	private ListObjectPool<UIWidget> _octaveLines;

	private ListObjectPool<UIWidget> _divisionLines;

	private UIWidget _parentWidget;

	private UIPanel _parentPanel;

	private UIWidget _widget;

	private int _startMargin;

	private int _termWidth;

	private int _temperedHeight;

	private int _termCountPerGroup;

	private Point2 _size;

	public void Init(int startMargin, int termWidth, int temperedHeight, int termCountPerGroup)
	{
		_startMargin = startMargin;
		_termWidth = termWidth;
		_temperedHeight = temperedHeight;
		_termCountPerGroup = termCountPerGroup;
		_baseLine.height = temperedHeight * 2;
		_baseLines = new ListObjectPool<UIWidget>();
		_baseLines.BaseObject = _baseLine;
		_baseLines.UseBase = true;
		_baseLines.Clear();
		_termLines = new ListObjectPool<UIWidget>();
		_termLines.BaseObject = _termLine;
		_termLines.UseBase = true;
		_termLines.Clear();
		_octaveLines = new ListObjectPool<UIWidget>();
		_octaveLines.BaseObject = _octaveLine;
		_octaveLines.UseBase = true;
		_octaveLines.Clear();
		_divisionLines = new ListObjectPool<UIWidget>();
		_divisionLines.BaseObject = _divisionLine;
		_divisionLines.UseBase = true;
		_divisionLines.Clear();
		_parentWidget = base.transform.parent.GetComponentInParent<UIWidget>();
		_parentPanel = GetComponentInParent<UIPanel>();
		_widget = GetComponent<UIWidget>();
		_parentWidget.AddOnChange(OnChangeSize);
	}

	private void OnChangeSize()
	{
		Point2 point = new Point2(_parentWidget.width, _parentWidget.height);
		if (point == _size)
		{
			return;
		}
		_size = point;
		int num = Mathf.CeilToInt((float)_size.x / (float)_termWidth);
		num = (Mathf.CeilToInt((float)num / (float)_termCountPerGroup) + 1) * _termCountPerGroup;
		_widget.SetDimensions(_startMargin + num + _termWidth, 87 * _temperedHeight);
		Vector3[] localCorners = _widget.localCorners;
		_baseLines.BeginLoad();
		_octaveLines.BeginLoad();
		Vector3 vector = localCorners[0] + Vector3.up + Vector3.right * _startMargin;
		int width = num * _termWidth;
		for (int i = 0; i < 87; i++)
		{
			if (i % 2 == 0)
			{
				UIWidget next = _baseLines.GetNext();
				next.width = width;
				next.SetPosition(vector + Vector3.up * i * _temperedHeight, 0f, 0f);
				UIUtility.UpdateAnchors(next.transform);
			}
			if (i % 12 == 0)
			{
				UIWidget next2 = _octaveLines.GetNext();
				next2.width = width;
				next2.SetPosition(vector + Vector3.up * i * _temperedHeight, 0f, 0.5f);
			}
		}
		_baseLines.EndLoad();
		_octaveLines.EndLoad();
		Vector3 vector2 = Vector3.Lerp(localCorners[0], localCorners[1], 0.5f) + Vector3.right * _startMargin;
		_termLines.BeginLoad();
		_divisionLines.BeginLoad();
		int height = 87 * _temperedHeight;
		for (int j = 0; j < num; j++)
		{
			UIWidget next3;
			if (j % _termCountPerGroup == 0)
			{
				next3 = _divisionLines.GetNext();
			}
			else
			{
				if (j % 2 != 0)
				{
					continue;
				}
				next3 = _termLines.GetNext();
			}
			next3.height = height;
			next3.SetPosition(vector2 + Vector3.right * j * _termWidth, 0.5f, 0.5f);
		}
		_termLines.EndLoad();
		_divisionLines.EndLoad();
		_widget.SetPosition(Vector3.zero, 0f, 0.5f);
	}

	public void SetOffset(Vector2 offset)
	{
		int num = _termWidth * _termCountPerGroup;
		if (offset.x > (float)(_startMargin + num))
		{
			offset.x = (float)_startMargin + (offset.x - (float)_startMargin) % (float)num;
		}
		Vector2 vector = -offset;
		Vector2 clipOffset = offset;
		_parentPanel.transform.localPosition = vector;
		_parentPanel.clipOffset = clipOffset;
	}
}
