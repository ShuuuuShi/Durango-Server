using Durango.Render.Camera;
using Durango.System;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Screen;

public class BlitScreen : MonoBehaviour
{
	public static int AntiAliasingValue;

	public static bool AntiAliasingChanged;

	[ExposedInEditor(null)]
	private RenderTexture _renderTexture;

	private RenderTexture _rtRemoved;

	private void Update()
	{
		if (_renderTexture == null || _renderTexture.width != UnityEngine.Screen.width || _renderTexture.height != UnityEngine.Screen.height || AntiAliasingChanged)
		{
			_rtRemoved = _renderTexture;
			_renderTexture = new RenderTexture(UnityEngine.Screen.width, UnityEngine.Screen.height, 24, RenderTextureFormat.ARGB32);
			if (Platform.Instance.UsePCRenderer)
			{
				AntiAliasingChanged = false;
				_renderTexture.antiAliasing = GetSafeAntiAliasingValue();
			}
			Singleton<MainCamera>.Instance().TargetTexture = _renderTexture;
		}
	}

	private void OnDestroy()
	{
		Object.Destroy(_rtRemoved);
		Object.Destroy(_renderTexture);
		if (Singleton<MainCamera>.HasInstance())
		{
			Singleton<MainCamera>.Instance().TargetTexture = null;
		}
	}

	private void OnPostRender()
	{
		if (_rtRemoved != null)
		{
			Object.Destroy(_rtRemoved);
			_rtRemoved = null;
		}
	}

	private void OnPreRender()
	{
		Graphics.Blit((Texture)_renderTexture, (RenderTexture)null);
	}

	private static int GetSafeAntiAliasingValue()
	{
		int antiAliasingValue = AntiAliasingValue;
		if (antiAliasingValue == 2 || antiAliasingValue == 4 || antiAliasingValue == 8)
		{
			return AntiAliasingValue;
		}
		return 1;
	}
}
