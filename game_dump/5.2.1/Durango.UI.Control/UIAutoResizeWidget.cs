using UnityEngine;

namespace Durango.UI.Control;

public class UIAutoResizeWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget[] _components;

	[SerializeField]
	private int _leftMargin;

	[SerializeField]
	private int _rightMargin;

	[SerializeField]
	private int _topMargin;

	[SerializeField]
	private int _bottomMargin;

	private UIWidget _widget;

	private void Awake()
	{
		_widget = GetComponent<UIWidget>();
		Resize();
	}

	private void Resize()
	{
		int num = _leftMargin + _rightMargin;
		int num2 = _bottomMargin + _topMargin;
		float num3 = _components[0].transform.localPosition.x - (float)_components[0].width * _components[0].pivotOffset.x;
		float num4 = _components[0].transform.localPosition.x - (float)_components[0].width * _components[0].pivotOffset.x + (float)_components[0].width;
		float num5 = 0f - (_components[0].transform.localPosition.y + (float)_components[0].height * (1f - _components[0].pivotOffset.y));
		float num6 = 0f - (_components[0].transform.localPosition.y + (float)_components[0].height * (1f - _components[0].pivotOffset.y)) + (float)_components[0].height;
		for (int i = 1; i < _components.Length; i++)
		{
			num3 = Mathf.Min(num3, _components[i].transform.localPosition.x - (float)_components[i].width * _components[i].pivotOffset.x);
			num4 = Mathf.Max(num4, _components[i].transform.localPosition.x - (float)_components[i].width * _components[i].pivotOffset.x + (float)_components[i].width);
			num5 = Mathf.Min(num5, 0f - (_components[i].transform.localPosition.y + (float)_components[i].height * (1f - _components[i].pivotOffset.y)));
			num6 = Mathf.Max(num6, 0f - (_components[i].transform.localPosition.y + (float)_components[i].height * (1f - _components[i].pivotOffset.y)) + (float)_components[i].height);
		}
		num += (int)(num4 - num3);
		num2 += (int)(num6 - num5);
		NGUIMath.CalculateAbsoluteWidgetBounds(base.transform);
		_widget.width = num;
		_widget.height = num2;
	}
}
