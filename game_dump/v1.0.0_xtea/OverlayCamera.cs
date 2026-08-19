using UnityEngine;

public class OverlayCamera : KSingleton<OverlayCamera>
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

	private Camera _baseCamera;

	private Camera _currentCamera;

	private GameObject _curFullscreenEffectObject;

	private ScreenParticleEffect _curFullScreenEffect;

	public static int Layer => LayerMask.NameToLayer("Overlay Effect");

	private void Start()
	{
		_currentCamera = ((Component)this).GetComponent<Camera>();
		_baseCamera = ((Component)KSingleton<MainCamera>.Instance()).GetComponent<Camera>();
		_rainyScreenEffectObject.transform.parent = null;
	}

	private void LateUpdate()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Camera currentCamera = _currentCamera;
		CameraClearFlags clearFlags = currentCamera.clearFlags;
		int cullingMask = currentCamera.cullingMask;
		currentCamera.CopyFrom(_baseCamera);
		currentCamera.depth += 1f;
		currentCamera.clearFlags = clearFlags;
		currentCamera.cullingMask = cullingMask;
		if ((Object)(object)_curFullscreenEffectObject != (Object)null)
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
		if ((Object)(object)_curFullscreenEffectObject != (Object)null)
		{
			_curFullscreenEffectObject.SetActive(false);
			_curFullscreenEffectObject.transform.parent = null;
			_curFullscreenEffectObject = null;
		}
		if (fullScreenEffect == ScreenParticleEffect.Rainy)
		{
			_curFullscreenEffectObject = _rainyScreenEffectObject;
			_curFullscreenEffectObject.transform.parent = ((Component)this).transform;
			_curFullscreenEffectObject.SetActive(true);
			SetRainyIntensity(intensity);
		}
	}

	private void SetRainyIntensity(float intensity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Color startColor = _rainySreennParticle.startColor;
		startColor.a = intensity;
		_rainySreennParticle.startColor = startColor;
	}
}
