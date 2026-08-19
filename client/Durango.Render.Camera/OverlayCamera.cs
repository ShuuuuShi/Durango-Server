using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Camera;

public class OverlayCamera : Singleton<OverlayCamera>
{
	public enum ScreenParticleEffect
	{
		None,
		Rainy
	}

	[SerializeField]
	private GameObject _rainyScreenEffectObject;

	[SerializeField]
	private ParticleSystem _rainySreennParticle;

	private UnityEngine.Camera _baseCamera;

	private UnityEngine.Camera _currentCamera;

	private GameObject _curFullscreenEffectObject;

	private ScreenParticleEffect _curFullScreenEffect;

	public static int Layer => LayerHelper.OverlayLayer;

	private void Start()
	{
		_currentCamera = GetComponent<UnityEngine.Camera>();
		MainCamera mainCamera = Singleton<MainCamera>.Instance();
		_baseCamera = mainCamera.GetComponent<UnityEngine.Camera>();
		mainCamera.CameraUpdated += UpdateCamera;
		_rainyScreenEffectObject.transform.parent = null;
	}

	private void UpdateCamera()
	{
		UnityEngine.Camera currentCamera = _currentCamera;
		CameraClearFlags clearFlags = currentCamera.clearFlags;
		int cullingMask = currentCamera.cullingMask;
		currentCamera.CopyFrom(_baseCamera);
		currentCamera.depth += 1f;
		currentCamera.clearFlags = clearFlags;
		currentCamera.cullingMask = cullingMask;
		if (_curFullscreenEffectObject != null)
		{
			_curFullscreenEffectObject.transform.localPosition = new Vector3(0f, 0f, currentCamera.nearClipPlane + 2000f);
		}
	}

	public void SetFullscreenEffect(ScreenParticleEffect fullScreenEffect, float intensity = 1f)
	{
		if (_curFullScreenEffect == fullScreenEffect)
		{
			if (fullScreenEffect == ScreenParticleEffect.Rainy)
			{
				SetRainyIntensity(intensity);
			}
			return;
		}
		_curFullScreenEffect = fullScreenEffect;
		if (_curFullscreenEffectObject != null)
		{
			_curFullscreenEffectObject.SetActive(value: false);
			_curFullscreenEffectObject.transform.parent = null;
			_curFullscreenEffectObject = null;
		}
		if (fullScreenEffect == ScreenParticleEffect.Rainy)
		{
			_curFullscreenEffectObject = _rainyScreenEffectObject;
			_curFullscreenEffectObject.transform.parent = base.transform;
			_curFullscreenEffectObject.SetActive(value: true);
			SetRainyIntensity(intensity);
		}
	}

	private void SetRainyIntensity(float intensity)
	{
		ParticleSystem.MainModule main = _rainySreennParticle.main;
		Color color = main.startColor.color;
		color.a = intensity;
		main.startColor = color;
	}
}
