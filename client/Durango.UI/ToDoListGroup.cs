using Durango.Logic.PlayGuide;
using UnityEngine;

namespace Durango.UI;

public class ToDoListGroup : ToDoListGroupBase
{
	[SerializeField]
	private UIWidget _handle;

	private float _verticalVisibleRatio;

	protected override void Start()
	{
		base.Start();
		_vertical.gameObject.SetActive(value: false);
		UIEventListener.Get(_handle.gameObject).onClick = delegate
		{
			ShowVertical(visible: true);
		};
		UIEventListener.Get(_closeBtn).onClick = delegate
		{
			ShowVertical(visible: false);
		};
	}

	protected override void LateUpdate()
	{
		UpdateVerticalTween();
		base.LateUpdate();
		UpdateDetailWidget();
	}

	private void UpdateVerticalTween()
	{
		if (!base.Visible)
		{
			return;
		}
		float num = Time.deltaTime * 4f;
		float verticalVisibleRatio = _verticalVisibleRatio;
		if (_showVertical)
		{
			if (!(verticalVisibleRatio > 0f))
			{
				return;
			}
			verticalVisibleRatio -= num;
		}
		else
		{
			if (!(verticalVisibleRatio < 1f))
			{
				return;
			}
			verticalVisibleRatio += num;
		}
		verticalVisibleRatio = (_verticalVisibleRatio = Mathf.Clamp01(verticalVisibleRatio));
		int num2 = (int)((float)ToDoListGroupBase.Width * verticalVisibleRatio);
		UIWidget parentWidget = _vertical.ParentWidget;
		parentWidget.leftAnchor.absolute = num2 - ToDoListGroupBase.Width;
		parentWidget.rightAnchor.absolute = num2;
		UIUtility.UpdateAnchors(parentWidget.transform);
		parentWidget.alpha = 1f - verticalVisibleRatio;
		_handle.transform.localScale = Vector3.one * verticalVisibleRatio;
		_handle.alpha = verticalVisibleRatio;
		_handle.gameObject.SetActive(verticalVisibleRatio > 0f);
		_vertical.gameObject.SetActive(verticalVisibleRatio < 1f);
		_detailWidget.alpha = (1f - verticalVisibleRatio) * _lastNodeAlpha;
		if (_widthRatioChanged != null)
		{
			_widthRatioChanged(verticalVisibleRatio);
		}
	}

	private void UpdateDetailWidget()
	{
		if (!base.IsVerticalVisible)
		{
			return;
		}
		ToDoListSystem toDoListSystem = GameSystem<ToDoListSystem>.Instance();
		int collectionCount = toDoListSystem.CollectionCount;
		ListObjectPool nodes = _scrollView.Nodes;
		for (int i = 0; i < collectionCount; i++)
		{
			ToDoCollection collection = toDoListSystem.GetCollection(i);
			ToDoIconNode toDoIconNode = nodes.Get<ToDoIconNode>(i);
			if (_detailWidget.Collection == collection)
			{
				_detailWidget.alpha = toDoIconNode.Alpha;
			}
		}
	}
}
