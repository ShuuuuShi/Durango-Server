using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render;

public class LightSetter : MonoBehaviour
{
	public enum LightPreset
	{
		Any,
		CharacterCreation
	}

	[SerializeField]
	private float _directionalLightConeAngle = 45f;

	[SerializeField]
	private float _directionalLightRotation;

	private Vector3 _midNightLight;

	[ExposedInEditor(null)]
	private Transform _debugDirectionalLight;

	[SerializeField]
	private LightPreset _currentPreset;

	private Transform _camTransform;

	[SerializeField]
	private float _prologueLightConeAngle = 60f;

	[SerializeField]
	private float _prologueLightRotation = -342.2f;

	public Transform CamTransform
	{
		get
		{
			if (_camTransform == null)
			{
				_camTransform = Singleton<MainCamera>.Instance().transform;
			}
			return _camTransform;
		}
	}

	private void Start()
	{
		RefreshMidNightLight();
	}

	private void OnValidate()
	{
		if (_currentPreset == LightPreset.CharacterCreation)
		{
			_directionalLightConeAngle = _prologueLightConeAngle;
			_directionalLightRotation = _prologueLightRotation;
		}
	}

	private void LateUpdate()
	{
		float num = TimeGauge.GetNormalizedTime();
		if (GameManager.IsPrologueMode)
		{
			num = 0f;
		}
		Shader.SetGlobalVector("_WorldLightVector", (Quaternion.AngleAxis(num * 360f + _directionalLightRotation, CamTransform.forward) * _midNightLight).normalized);
	}

	private void RefreshMidNightLight()
	{
		_midNightLight = Quaternion.AngleAxis(_directionalLightConeAngle, CamTransform.up) * CamTransform.forward;
	}

	public void ChangeCamTransform(Transform tf)
	{
		_camTransform = tf;
		RefreshMidNightLight();
	}

	public void TransposePreset(LightPreset from, LightPreset to)
	{
		if (_currentPreset == LightPreset.Any || _currentPreset == from)
		{
			_currentPreset = to;
			OnValidate();
		}
	}
}
