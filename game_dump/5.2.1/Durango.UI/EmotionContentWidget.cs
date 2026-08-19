using System;
using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class EmotionContentWidget : UIWidget
{
	[SerializeField]
	private KScrollView _parentScrollView;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _contentWidget;

	[SerializeField]
	private Vector2 _padding;

	[SerializeField]
	private UIWidget _prefab;

	[SerializeField]
	private UILabel _blankText;

	[SerializeField]
	private SelectableButton _button;

	private ListObjectPool<UIWidget> _subjectItems;

	private bool _isinit;

	private void Init()
	{
		if (!_isinit)
		{
			_isinit = true;
			_subjectItems = new ListObjectPool<UIWidget>();
			_subjectItems.BaseObject = _prefab;
		}
	}

	public void SetGrids<TData, TObj>(string categoryTitle, List<TData> dataToOrganizeGrid, Action<TObj, TData> initialize) where TObj : UIWidget
	{
		Init();
		if (_titleLabel != null)
		{
			_titleLabel.text = categoryTitle;
		}
		_subjectItems.BeginLoad();
		foreach (TData item in dataToOrganizeGrid)
		{
			initialize(_subjectItems.GetNext() as TObj, item);
		}
		_subjectItems.EndLoad();
		if (_blankText != null)
		{
			_blankText.gameObject.SetActive(value: false);
		}
		if (_button != null)
		{
			_button.gameObject.SetActive(value: false);
		}
	}

	public Vector2 UpdateLayoutItems()
	{
		Vector2 viewSize = _parentScrollView.ViewSize;
		Point2 point = default(Point2);
		point.x = (int)viewSize.x;
		Vector2 vector = (Vector2)_contentWidget.localCorners[1] + new Vector2(_padding.x, 0f - _padding.y);
		int rowItemCount;
		float rowSize;
		float colSize;
		Vector2 vector2 = UIUtility.WidgetsGridReposition(_subjectItems, null, Vector2.down, vector, viewSize.x - _padding.x * 2f, _subjectItems.BaseObject.localSize, _padding.x, _padding.y, out rowItemCount, out rowSize, out colSize);
		_contentWidget.height = (int)(vector2.y + _padding.y * 2f);
		point.y = _contentWidget.height + ((_titleWidget != null) ? _titleWidget.height : 0);
		SetDimensions(point.x, point.y);
		UIUtility.UpdateAnchors(base.transform);
		if (rowItemCount == 0)
		{
			return _prefab.localSize + _padding * 2f;
		}
		return new Vector2(rowSize, colSize) + _padding * 2f;
	}

	public void SetBlank(string blnakText, float targetContentHeight)
	{
		base.height = (int)targetContentHeight + _titleWidget.height;
		if (!(_blankText == null))
		{
			_blankText.gameObject.SetActive(value: true);
			_blankText.text = blnakText;
		}
	}

	public void ActivateButton(string text, Action clicked)
	{
		if (!(_button == null))
		{
			_button.gameObject.SetActive(value: true);
			_button.Text = text;
			_button.Clicked = clicked;
		}
	}
}
