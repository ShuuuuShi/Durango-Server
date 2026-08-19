using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public class VerticalLayoutWidget : UIWidget, RectLayout.ICompatible
{
	private enum ChildAlignment
	{
		LeftTop,
		CenterTop,
		RightTop
	}

	[Serializable]
	public struct Padding
	{
		[SerializeField]
		public float Left;

		[SerializeField]
		public float Right;

		[SerializeField]
		public float Up;

		[SerializeField]
		public float Down;
	}

	public class Subject
	{
		public ListObjectPool<UIWidget> Pool;

		private UIWidget _prefab;

		private Transform _transform;

		private readonly List<UIWidget> _childs = new List<UIWidget>();

		private bool HasPool => _prefab != null;

		public UIWidget this[int i] => (!HasPool) ? _childs[i] : Pool[i];

		public int Count => (!HasPool) ? _childs.Count : Pool.Count;

		public IEnumerable<UIWidget> Collection => (!HasPool) ? ((IEnumerable<UIWidget>)_childs) : ((IEnumerable<UIWidget>)Pool);

		public Vector2 RepresentativeSize => (!HasPool) ? new Vector2(_childs.Max((UIWidget elem) => elem.width), _childs.Max((UIWidget elem) => elem.height)) : Pool.BaseObject.localSize;

		public Subject(UIWidget prefab, Transform trf)
		{
			_prefab = prefab;
			_transform = trf;
			if (_prefab == null)
			{
				SetChildrenWidgets(_childs, trf);
				return;
			}
			Pool = new ListObjectPool<UIWidget>();
			Pool.BaseObject = _prefab;
		}

		public void Update(UIWidget prefab)
		{
			if (prefab != null)
			{
				_prefab = prefab;
			}
			if (!HasPool)
			{
				SetChildrenWidgets(_childs, _transform);
			}
		}

		private void SetChildrenWidgets(List<UIWidget> targetWidget, Transform trf)
		{
			targetWidget.Clear();
			foreach (Transform item in trf)
			{
				UIWidget uIWidget = null;
				if ((uIWidget = item.GetComponent<UIWidget>()) != null && uIWidget.gameObject.activeSelf)
				{
					targetWidget.Add(uIWidget);
				}
			}
		}
	}

	[FormerlySerializedAs("_anchoredPanel")]
	[SerializeField]
	private UIRect _contentSizeFittee;

	[SerializeField]
	private UIWidget _prefab;

	[SerializeField]
	private Padding _padding;

	[SerializeField]
	private Vector2 _spacing;

	[SerializeField]
	private ChildAlignment _childAlignment;

	[SerializeField]
	private bool _childForceExpandWidth;

	[SerializeField]
	private bool _resizeWidgetWidth;

	[SerializeField]
	private int _maxWidthWhenResize;

	[SerializeField]
	private bool _resizeWidgetHeight;

	private Subject _subject;

	private float _originWidth;

	private Subject Items
	{
		get
		{
			if (_subject == null)
			{
				_subject = new Subject(_prefab, base.transform);
			}
			return _subject;
		}
	}

	private float ProperWidth
	{
		get
		{
			float num = 0f;
			num = ((!(_contentSizeFittee == null) && !(_contentSizeFittee == this)) ? _contentSizeFittee.GetWidth() : ((float)base.width));
			return (!_resizeWidgetWidth || (float)_maxWidthWhenResize == 0f) ? num : Mathf.Min(_maxWidthWhenResize, num);
		}
	}

	public Vector2 UpdateLayout(float? x, float? y)
	{
		Vector2 result = ((!_childForceExpandWidth) ? UpdateAsAligned((!x.HasValue) ? ProperWidth : x.Value) : UpdateAsStretched((!x.HasValue) ? ProperWidth : x.Value));
		ResizeAnchoredPanel(_contentSizeFittee, new Point2(Mathf.FloorToInt((!x.HasValue) ? result.x : x.Value), Mathf.FloorToInt((!y.HasValue) ? result.y : y.Value)));
		return result;
	}

	[ContextMenu("Execute")]
	public Vector2 UpdateLayout()
	{
		Items.Update(_prefab);
		Vector2 result = ((!_childForceExpandWidth) ? UpdateAsAligned(ProperWidth) : UpdateAsStretched(ProperWidth));
		if (_resizeWidgetWidth || _resizeWidgetHeight)
		{
			int x = ((!_resizeWidgetWidth) ? base.width : Mathf.RoundToInt(result.x));
			int y = ((!_resizeWidgetHeight) ? base.height : Mathf.RoundToInt(result.y));
			ResizeAnchoredPanel((!(_contentSizeFittee == null)) ? _contentSizeFittee : this, new Point2(x, y));
		}
		return result;
	}

	public void UpdateWidget<TObj>(Action<TObj, int> update) where TObj : UIWidget
	{
		int num = 0;
		int i = 0;
		for (int count = Items.Count; i < count; i++)
		{
			TObj arg = Items[i] as TObj;
			update(arg, num);
			num++;
		}
	}

	public void SetGrids<TData, TObj>([CanBeNull] IList<TData> dataToOrganizeGrid, Action<TData, TObj, int> initialize) where TObj : UIWidget
	{
		if (Items.Pool == null)
		{
			throw new NullReferenceException(typeof(ListObjectPool<UIWidget>).ToString());
		}
		Items.Pool.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(dataToOrganizeGrid); i < size; i++)
		{
			TData arg = dataToOrganizeGrid[i];
			initialize(arg, Items.Pool.GetNext() as TObj, i);
		}
		Items.Pool.EndLoad();
	}

	private Vector2 UpdateAsStretched(float w)
	{
		Vector2 representativeSize = Items.RepresentativeSize;
		Vector3[] array = localCorners;
		Vector2 vector = (Vector2)array[1] + new Vector2(_padding.Left, 0f - _padding.Up);
		float y = UIUtility.WidgetsGridReposition(Items.Collection, null, Vector2.down, vector, w - _padding.Left - _padding.Right, representativeSize, _spacing.x, _spacing.y).y + _padding.Up + _padding.Down;
		return new Vector2(w, y);
	}

	private Vector2 UpdateAsAligned(float w)
	{
		float num = 0f;
		float num2 = 0f - _padding.Up;
		float num3 = float.MinValue;
		float num4 = _padding.Left;
		int lineStartIndex = 0;
		int i = 0;
		for (int count = Items.Count; i < count; i++)
		{
			UIWidget uIWidget = Items[i];
			if (!UIUtility.IsVisibleWidget(uIWidget))
			{
				continue;
			}
			num4 += (float)uIWidget.width;
			if (i != Items.Count - 1 && num4 + (float)Items[i + 1].width + _spacing.x < w - _padding.Right + 1f)
			{
				num4 += _spacing.x;
				continue;
			}
			float curContentWidth = num4 - _padding.Left;
			float maxContentWidth = w - _padding.Left - _padding.Right;
			SetAlignment(Items, lineStartIndex, i, curContentWidth, maxContentWidth, num2);
			lineStartIndex = i + 1;
			float num5 = num4 + _padding.Right;
			if (num5 > num3)
			{
				num3 = num5;
				if (num3 > num)
				{
					num = num3;
				}
			}
			num4 = _padding.Left;
			num2 -= (float)uIWidget.height + ((i != Items.Count - 1) ? _spacing.y : 0f);
		}
		return new Vector2((!_resizeWidgetWidth) ? w : num, 0f - (num2 - _padding.Down));
	}

	private void SetAlignment(Subject subjects, int lineStartIndex, int lineEndIndex, float curContentWidth, float maxContentWidth, float heightInverseSum)
	{
		float num = 0f;
		switch (_childAlignment)
		{
		case ChildAlignment.CenterTop:
			num = (maxContentWidth - curContentWidth) / 2f;
			break;
		case ChildAlignment.RightTop:
			num = maxContentWidth - curContentWidth;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ChildAlignment.LeftTop:
			break;
		}
		float num2 = _padding.Left;
		int i = lineStartIndex;
		for (int num3 = lineEndIndex + 1; i < num3; i++)
		{
			UIWidget uIWidget = subjects[i];
			if (UIUtility.IsVisibleWidget(uIWidget))
			{
				Vector3 zero = Vector3.zero;
				zero.x += num + num2 + uIWidget.pivotOffset.x * (float)uIWidget.width - (float)base.width * base.pivotOffset.x;
				num2 += (float)uIWidget.width + _spacing.x;
				zero.y += heightInverseSum + (uIWidget.pivotOffset.y - 1f) * (float)uIWidget.height - (float)base.height * (base.pivotOffset.y - 1f);
				uIWidget.transform.localPosition = zero;
			}
		}
	}

	private void ResizeAnchoredPanel(UIRect rect, Point2 size)
	{
		Point2 point = new Point2(base.width, base.height);
		UIWidget uIWidget = rect as UIWidget;
		if (uIWidget != null)
		{
			uIWidget.SetDimensions(size.x, size.y);
			Vector3 vector = new Vector3(uIWidget.pivotOffset.x * (float)(point.x - uIWidget.width), (0f - (1f - uIWidget.pivotOffset.y)) * (float)(point.y - uIWidget.height));
			Subject items = Items;
			int i = 0;
			for (int count = items.Count; i < count; i++)
			{
				UIWidget uIWidget2 = items[i];
				if (UIUtility.IsVisibleWidget(uIWidget2))
				{
					uIWidget2.transform.localPosition += vector;
				}
			}
		}
		UIPanel uIPanel = rect as UIPanel;
		if (uIPanel != null)
		{
			uIPanel.SetRect(0f, 0f, size.x, size.y);
		}
	}
}
