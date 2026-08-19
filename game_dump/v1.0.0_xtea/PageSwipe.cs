using System;
using UnityEngine;

public class PageSwipe : MonoBehaviour
{
	[Serializable]
	public class WidgetWithTitle
	{
		public UIWidget Widget;

		[LocalizableString]
		public string Title;
	}

	public Action<int> OnShowPage;

	public Action<int> OnShowingPage;

	[SerializeField]
	private UILabel _category;

	[SerializeField]
	private GameObject _prevArrow;

	[SerializeField]
	private GameObject _nextArrow;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private WidgetWithTitle[] _widgets;

	[SerializeField]
	private ListObjectPool _indexCircle;

	[SerializeField]
	private float _circleMargin = 5f;

	private UIWidget _frameWidget;

	private int _width;

	private int _height;

	private Vector2 _defaultOffset;

	private int _dragStartIndex;

	private int _currentIndex;

	private float _floatingIndex;

	private bool _isInit;

	public int CurrentIndex => _currentIndex;

	public WidgetWithTitle[] Widgets => _widgets;

	private void Start()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		UpdateLayout();
		_scrollView.ResetPosition();
		_defaultOffset = _scrollView.panel.clipOffset;
		_scrollView.onDragStarted = OnScrollViewDragStarted;
		_scrollView.onDragFinished = OnScrollViewDragFinished;
		_scrollView.onStoppedMoving = OnScrollViewStopMoving;
		if ((Object)(object)_prevArrow != (Object)null)
		{
			UIEventListener.Get(_prevArrow).onClick = OnClick_ContentArrow;
		}
		if ((Object)(object)_nextArrow != (Object)null)
		{
			UIEventListener.Get(_nextArrow).onClick = OnClick_ContentArrow;
		}
		ShowPage(_currentIndex, instant: true);
	}

	private void OnEnable()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		_floatingIndex = -1f;
		if (_indexCircle != null && !((Object)(object)_indexCircle.BaseObject == (Object)null))
		{
			Vector3 localPosition = _indexCircle.BaseObject.transform.localPosition;
			int i = 0;
			for (int count = _indexCircle.Count; i < count; i++)
			{
				Vector3 localPosition2 = _indexCircle[i].transform.localPosition;
				localPosition2.y = localPosition.y;
				_indexCircle[i].transform.localPosition = localPosition2;
			}
		}
	}

	public void UpdateLayout()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		UIPanel uIPanel = _scrollView.panel;
		if ((Object)(object)uIPanel == (Object)null)
		{
			uIPanel = ((Component)_scrollView).GetComponent<UIPanel>();
		}
		_width = (int)uIPanel.width;
		_height = (int)uIPanel.height;
		Vector4 finalClipRegion = uIPanel.finalClipRegion;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(finalClipRegion.x, finalClipRegion.y);
		int num = ((_widgets != null) ? _widgets.Length : 0);
		for (int i = 0; i < num; i++)
		{
			UIWidget widget = _widgets[i].Widget;
			((Component)widget).gameObject.SetActive(true);
			Vector3 val2 = val + Vector3.right * (float)_width * (float)i;
			Vector2 pivotOffset = widget.pivotOffset;
			((Component)widget).transform.localPosition = val2 + Vector3.right * (pivotOffset.x - 0.5f) * (float)widget.width + Vector3.up * (pivotOffset.y - 0.5f) * (float)widget.height;
		}
		MakeFrameWidget();
		MakeIndexCircle();
	}

	public void SetDefaultIndex(int index)
	{
		_currentIndex = index;
	}

	private void MakeFrameWidget()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_frameWidget == (Object)null)
		{
			_frameWidget = ((Component)_scrollView).gameObject.AddChild<UIWidget>();
		}
		UIWidget frameWidget = _frameWidget;
		int num = ((_widgets != null) ? _widgets.Length : 0);
		frameWidget.width = num * _width;
		frameWidget.height = _height;
		frameWidget.depth = 0;
		((Component)frameWidget).transform.localPosition = Vector3.right * ((float)_width / 2f) * (float)(num - 1);
		BoxCollider box = ((Component)frameWidget).gameObject.AddComponent<BoxCollider>();
		NGUITools.UpdateWidgetCollider(box, considerInactive: false);
		UIDragScrollView uIDragScrollView = ((Component)frameWidget).gameObject.AddComponent<UIDragScrollView>();
		uIDragScrollView.scrollView = _scrollView;
	}

	private void MakeIndexCircle()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		if (_indexCircle == null || (Object)(object)_indexCircle.BaseObject == (Object)null)
		{
			return;
		}
		_indexCircle.Set((_widgets != null) ? _widgets.Length : 0);
		UIWidget component = _indexCircle.BaseObject.GetComponent<UIWidget>();
		component.UpdateAnchors();
		int width = component.width;
		float num = 0f;
		int count = _indexCircle.Count;
		for (int i = 0; i < count; i++)
		{
			UIWidget component2 = _indexCircle[i].GetComponent<UIWidget>();
			component2.SetAnchor((GameObject)null);
			_indexCircle[i].transform.localPosition = Vector3.right * num;
			if (i < count - 1)
			{
				num += (float)width + _circleMargin;
			}
		}
		Vector3 val = ((Component)component).transform.localPosition + Vector3.left * num / 2f;
		for (int j = 0; j < count; j++)
		{
			Transform transform = _indexCircle[j].transform;
			transform.localPosition += val;
		}
	}

	private void IndexCircleResize(float floatIndex)
	{
		int count = _indexCircle.Count;
		floatIndex = Mathf.Clamp(floatIndex, 0f, (float)(count - 1));
		for (int i = 0; i < count; i++)
		{
			UIWidget component = _indexCircle[i].GetComponent<UIWidget>();
			float num = Mathf.Abs(floatIndex - (float)i);
			if (num < 1f)
			{
				component.alpha = 0.5f + (1f - num) * 0.5f;
			}
			else
			{
				component.alpha = 0.5f;
			}
		}
	}

	private void Update()
	{
		OnUpdate(CurrentFloatIndex());
	}

	private void OnUpdate(float floatIndex)
	{
		if (_indexCircle != null && (Object)(object)_indexCircle.BaseObject != (Object)null)
		{
			IndexCircleResize(floatIndex);
		}
		if (OnShowingPage != null)
		{
			int num = Mathf.RoundToInt(_floatingIndex);
			int num2 = Mathf.RoundToInt(floatIndex);
			if (num != num2)
			{
				OnShowingPage(num2);
			}
		}
		_floatingIndex = floatIndex;
	}

	private float CurrentFloatIndex()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		float num = _scrollView.panel.clipOffset.x - _defaultOffset.x;
		return num / (float)_width;
	}

	public void ShowPage(int index, bool instant = false)
	{
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		int num = ((_widgets != null) ? _widgets.Length : 0);
		index = Mathf.Clamp(index, 0, num - 1);
		_currentIndex = index;
		if ((Object)(object)_category != (Object)null)
		{
			string title = _widgets[index].Title;
			_category.text = (string.IsNullOrEmpty(title) ? ((Object)_widgets[index].Widget).name : LocalizeSystem.Get(title));
		}
		float num2 = (float)(_currentIndex * _width) + _defaultOffset.x;
		if (instant)
		{
			_scrollView.DisableSpring();
			UIPanel uIPanel = _scrollView.panel;
			if ((Object)(object)uIPanel == (Object)null)
			{
				uIPanel = ((Component)_scrollView).GetComponent<UIPanel>();
			}
			((Component)uIPanel).transform.localPosition = Vector3.left * num2;
			uIPanel.clipOffset = _defaultOffset + Vector2.right * num2;
			_scrollView.UpdateScrollbars(recalculateBounds: false);
		}
		else
		{
			SpringPanel.Begin(((Component)_scrollView).gameObject, Vector3.left * num2, 8f);
		}
		DrawArrow();
		if (OnShowPage != null)
		{
			OnShowPage(index);
		}
	}

	private void DrawArrow()
	{
		int num = ((_widgets != null) ? _widgets.Length : 0);
		if ((Object)(object)_prevArrow != (Object)null)
		{
			_prevArrow.SetActive(_currentIndex > 0);
		}
		if ((Object)(object)_nextArrow != (Object)null)
		{
			_nextArrow.SetActive(_currentIndex < num - 1);
		}
	}

	private void OnScrollViewDragStarted()
	{
		_dragStartIndex = _currentIndex;
	}

	private void OnScrollViewDragFinished()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = Mathf.RoundToInt(_floatingIndex);
		if (_dragStartIndex == num2)
		{
			float x = _scrollView.currentMomentum.x;
			if (Mathf.Abs(x) > 0.01f)
			{
				num = ((!(x > 0f)) ? 1 : (-1));
			}
		}
		ShowPage(num2 + num);
	}

	private void OnScrollViewStopMoving()
	{
		int num = Mathf.RoundToInt(_floatingIndex);
		if (Mathf.Abs((float)num - _floatingIndex) > 0.01f)
		{
			ShowPage(num);
		}
	}

	private void OnClick_ContentArrow(GameObject go)
	{
		if ((Object)(object)_prevArrow == (Object)(object)go)
		{
			ShowPage(_currentIndex - 1);
		}
		else if ((Object)(object)_nextArrow == (Object)(object)go)
		{
			ShowPage(_currentIndex + 1);
		}
	}
}
