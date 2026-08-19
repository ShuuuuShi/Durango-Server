using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

[ExecuteInEditMode]
public class RectLayoutComponent : MonoBehaviour, IScreenResizeReceiver, RectLayout.ICompatible
{
	[SerializeField]
	private bool _onEnable;

	[SerializeField]
	private bool _isRoot;

	[SerializeField]
	private RectLayout _landspace;

	[SerializeField]
	private RectLayout _portrait;

	private Point2 _prevSize;

	private bool _isAddedChangedEvent;

	public UIWidget ParentWidget => GetCurrentLayout().GetParentWidget();

	private void OnEnable()
	{
		if (_onEnable)
		{
			UpdateLayout();
		}
	}

	private void OnParentChanged()
	{
		UIWidget parentWidget = ParentWidget;
		Point2 point = new Point2(parentWidget.width, parentWidget.height);
		if (!(point == _prevSize))
		{
			_prevSize = point;
			UpdateLayout();
			UIUtility.UpdateAnchors(parentWidget.transform);
		}
	}

	public Vector2 UpdateLayout()
	{
		return GetCurrentLayout().UpdateLayout();
	}

	public Vector2 UpdateLayout(float? width, float? height)
	{
		return GetCurrentLayout().UpdateLayout(width, height);
	}

	private RectLayout GetCurrentLayout()
	{
		if (UIManager.IsPortraitWidget(base.gameObject))
		{
			if (_portrait.HasItems())
			{
				return _portrait;
			}
			return _landspace;
		}
		return _landspace;
	}

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		if (_isRoot)
		{
			UpdateLayout();
		}
	}

	public void UpdateOnSizeChange()
	{
		if (!_isAddedChangedEvent)
		{
			_isAddedChangedEvent = true;
			UIWidget parentWidget = ParentWidget;
			if (parentWidget != null)
			{
				parentWidget.AddOnChange(OnParentChanged);
			}
		}
	}

	public void AddCompatible([NotNull] UIWidget widget, RectLayout.CompatibleDelegate func)
	{
		if (_landspace != null)
		{
			_landspace.AddCompatible(widget, func);
		}
		if (_portrait != null)
		{
			_portrait.AddCompatible(widget, func);
		}
	}

	public void AddCompatible(int index, RectLayout.CompatibleDelegate func)
	{
		if (_landspace != null)
		{
			_landspace.AddCompatible(index, func);
		}
		if (_portrait != null)
		{
			_portrait.AddCompatible(index, func);
		}
	}
}
