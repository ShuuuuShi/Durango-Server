using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class WidgetLayoutController : MonoBehaviour
{
	[SerializeField]
	private bool _onEnable;

	[SerializeField]
	private WidgetLayout _layout;

	[SerializeField]
	private WidgetLayout _portraitLayout;

	private UIWidget _widget;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	public bool IsRoot => _onEnable;

	private void OnEnable()
	{
		if (_onEnable)
		{
			UpdateLayout();
		}
	}

	private void OnPortraitMode(bool isPortrait)
	{
		if (_onEnable)
		{
			UpdateLayout();
		}
	}

	[ExposedInEditor(null)]
	public void UpdateLayout()
	{
		UIWidget widget = Widget;
		if (widget.isAnchored)
		{
			UpdateLayout(widget.width, widget.height);
		}
		else
		{
			UpdateLayout(0, 0);
		}
	}

	public void UpdateLayout(int width, int height)
	{
		WidgetLayout widgetLayout = ((!UIManager.IsPortraitMode) ? _layout : ((!_portraitLayout.HasItems()) ? _layout : _portraitLayout));
		widgetLayout.UpdateLayout(Widget, new Point2(width, height));
	}
}
