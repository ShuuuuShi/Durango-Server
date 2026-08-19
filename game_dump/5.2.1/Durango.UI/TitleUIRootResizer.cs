using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.System;
using UnityEngine;

namespace Durango.UI;

public class TitleUIRootResizer : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoUpdateScreenSize_003Ed__19 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TitleUIRootResizer _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoUpdateScreenSize_003Ed__19(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TitleUIRootResizer titleUIRootResizer = _003C_003E4__this;
			int width;
			int height;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				goto IL_0042;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0042;
			case 2:
				{
					_003C_003E1__state = -1;
					titleUIRootResizer.OnScreenSizeChanged();
					return false;
				}
				IL_0042:
				if (!Platform.Instance.GetScreenResolution(IsPortrait, out width, out height))
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				titleUIRootResizer._root.manualWidth = width;
				titleUIRootResizer._root.manualHeight = height;
				titleUIRootResizer.ScreenHeight = titleUIRootResizer._root.activeHeight;
				ScreenWidth = Mathf.RoundToInt(NGUITools.screenSize.x * titleUIRootResizer._root.pixelSizeAdjustment);
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoUpdateScreenSize_003Ed__19(0)
		{
			_003C_003E4__this = this
		};
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
		if (IsPortrait)
		{
			return new Rect(safeRect.y, safeRect.x, safeRect.height, safeRect.width);
		}
		return safeRect;
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
