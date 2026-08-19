using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Control;

public class ScreenAreaMask : MonoBehaviour
{
	private UIWidget _widget;

	private bool _isDirty;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public bool IsVisible { get; private set; }

	private void Start()
	{
		Widget.AddOnChange(OnChange);
	}

	private void OnEnable()
	{
		Singleton<ScreenAreaManager>.Instance().Add(this);
	}

	private void OnDisable()
	{
		if (Singleton<ScreenAreaManager>.HasInstance())
		{
			Singleton<ScreenAreaManager>.Instance().Remove(this);
		}
	}

	private void Update()
	{
		bool isVisible = _widget.isVisible;
		if (isVisible != IsVisible)
		{
			IsVisible = isVisible;
			_isDirty = true;
		}
		if (_isDirty)
		{
			_isDirty = false;
			Singleton<ScreenAreaManager>.Instance().SetDirty();
		}
	}

	private void OnChange()
	{
		_isDirty = true;
	}
}
