using System;
using System.Collections;
using Durango.System;
using UnityEngine;

namespace Durango.UI;

public class TitleUIRootResizer : MonoBehaviour
{
	[SerializeField]
	private UIRoot _root;

	public static bool IsPortrait { get; private set; }

	public static int ScreenWidth { get; private set; }

	public int ScreenHeight { get; private set; }

	public static event Action ScreenResized;

	private void Awake()
	{
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(OnScreenResize));
		OnScreenResize();
	}

	private void OnDestroy()
	{
		TitleUIRootResizer.ScreenResized = null;
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Remove(UICamera.onScreenResize, new UICamera.OnScreenResize(OnScreenResize));
	}

	private void OnScreenResize()
	{
		IsPortrait = Platform.Instance.SupportPortrait && Screen.width < Screen.height;
		StartCoroutine(CoUpdateScreenSize());
	}

	private IEnumerator CoUpdateScreenSize()
	{
		int w;
		int h;
		while (!Platform.Instance.GetScreenResolution(IsPortrait, out w, out h))
		{
			yield return null;
		}
		_root.manualWidth = w;
		_root.manualHeight = h;
		ScreenHeight = _root.activeHeight;
		ScreenWidth = Mathf.RoundToInt(NGUITools.screenSize.x * _root.pixelSizeAdjustment);
		yield return null;
		OnScreenSizeChanged();
	}

	private void OnScreenSizeChanged()
	{
		UIUtility.ResetAndUpdateAnchors(_root.transform);
		if (TitleUIRootResizer.ScreenResized != null)
		{
			TitleUIRootResizer.ScreenResized();
		}
	}

	public static Rect GetSafeRect()
	{
		Rect safeRect = DeviceInfo.SafeRect;
		return (!IsPortrait) ? safeRect : new Rect(safeRect.y, safeRect.x, safeRect.height, safeRect.width);
	}

	public static void AddOnScreenResized(Action func)
	{
		if (func != null)
		{
			ScreenResized += func;
			func();
		}
	}
}
