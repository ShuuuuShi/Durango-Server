using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using UnityEngine;

public static class OrientationController
{
	public enum Orientation
	{
		Landscape,
		Portrait,
		AutoRotation
	}

	[Flags]
	public enum RotationLock
	{
		None = 0,
		Loading = 1,
		UI = 2,
		Battle = 4
	}

	[CompilerGenerated]
	private sealed class _003CCoSetOrientation_003Ed__11 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ScreenOrientation screenOrientation;

		public Orientation orientation;

		private RuntimePlatform _003Cplatform_003E5__2;

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
		public _003CCoSetOrientation_003Ed__11(int _003C_003E1__state)
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
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cplatform_003E5__2 = Application.platform;
				if (_003Cplatform_003E5__2 == RuntimePlatform.IPhonePlayer)
				{
					_003C_003E2__current = new WaitForEndOfFrame();
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0051;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0051;
			case 2:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_0051:
				if (screenOrientation != 0)
				{
					Screen.orientation = screenOrientation;
				}
				if (_003Cplatform_003E5__2 == RuntimePlatform.IPhonePlayer)
				{
					_003C_003E2__current = new WaitForEndOfFrame();
					_003C_003E1__state = 2;
					return true;
				}
				break;
			}
			SetAutorotateProperty(orientation);
			return false;
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

	private static RotationLock _rotationLock;

	private static Orientation _orientation;

	private static Orientation _targetOrientation;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void InstallEvent()
	{
		GameManager.Started += delegate
		{
			_rotationLock = RotationLock.None;
			SetOrientation(_targetOrientation);
		};
	}

	public static void LockRotation(RotationLock l)
	{
		_rotationLock |= l;
		OnRotationLockChanged();
	}

	public static void UnlockRotation(RotationLock l)
	{
		_rotationLock &= ~l;
		OnRotationLockChanged();
	}

	public static void SetTargetOrientation(Orientation orientation, bool update)
	{
		_targetOrientation = orientation;
		if (update)
		{
			SetOrientation(_targetOrientation);
		}
	}

	private static void OnRotationLockChanged()
	{
		if (!GameManager.IsPrologueMode && _orientation == Orientation.AutoRotation)
		{
			if (_rotationLock != 0)
			{
				SetAutorotateProperty((Screen.width <= Screen.height) ? Orientation.Portrait : Orientation.Landscape);
			}
			else
			{
				SetAutorotateProperty(Orientation.AutoRotation);
			}
		}
	}

	public static void SetOrientation(Orientation orientation, ScreenOrientation screen = ScreenOrientation.Unknown)
	{
		if (_orientation == orientation)
		{
			return;
		}
		_orientation = orientation;
		if (screen == ScreenOrientation.Unknown)
		{
			DeviceOrientation deviceOrientation = Input.deviceOrientation;
			switch (deviceOrientation)
			{
			case DeviceOrientation.Portrait:
			case DeviceOrientation.PortraitUpsideDown:
			case DeviceOrientation.LandscapeLeft:
			case DeviceOrientation.LandscapeRight:
				screen = (ScreenOrientation)deviceOrientation;
				break;
			case DeviceOrientation.FaceUp:
			case DeviceOrientation.FaceDown:
				screen = Screen.orientation;
				break;
			}
		}
		switch (orientation)
		{
		case Orientation.Portrait:
			if (screen != ScreenOrientation.Portrait && screen != ScreenOrientation.PortraitUpsideDown)
			{
				screen = ScreenOrientation.Portrait;
			}
			break;
		case Orientation.Landscape:
			if (screen != ScreenOrientation.LandscapeLeft && screen != ScreenOrientation.LandscapeRight)
			{
				screen = ScreenOrientation.LandscapeLeft;
			}
			break;
		}
		Singleton<GameManager>.Instance().StartCoroutine(CoSetOrientation(orientation, screen));
	}

	private static IEnumerator CoSetOrientation(Orientation orientation, ScreenOrientation screenOrientation)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoSetOrientation_003Ed__11(0)
		{
			orientation = orientation,
			screenOrientation = screenOrientation
		};
	}

	private static void SetAutorotateProperty(Orientation orienatation)
	{
		Screen.orientation = ScreenOrientation.AutoRotation;
		switch (orienatation)
		{
		case Orientation.AutoRotation:
			Screen.autorotateToPortrait = true;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;
			Screen.autorotateToPortraitUpsideDown = true;
			break;
		case Orientation.Landscape:
			Screen.autorotateToPortrait = false;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;
			Screen.autorotateToPortraitUpsideDown = false;
			break;
		case Orientation.Portrait:
			Screen.autorotateToPortrait = true;
			Screen.autorotateToLandscapeLeft = false;
			Screen.autorotateToLandscapeRight = false;
			Screen.autorotateToPortraitUpsideDown = true;
			break;
		}
	}
}
