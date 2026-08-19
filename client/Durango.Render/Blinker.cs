using UnityEngine;

namespace Durango.Render;

public class Blinker : MonoBehaviour
{
	[SerializeField]
	private Shader _blinkShader;

	[SerializeField]
	private Texture2D _blinkGradation;

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private float _blinkPeriod;

	[SerializeField]
	private Vector2 _scrollUV;

	[SerializeField]
	private float _scrollSpeed;

	[SerializeField]
	private float _intensity;

	[SerializeField]
	private AnimationCurve _blinkCurve;

	private readonly MeshCloner _meshCloner = new MeshCloner();

	private Material _material;

	private void Awake()
	{
		_material = new Material(_blinkShader);
		_material.SetColor("_Color", _color);
		_material.SetTexture("_MainTex", _blinkGradation);
		_material.SetVector("_ScrollUV", _scrollUV.normalized);
		_meshCloner.Add(base.transform, GetComponentsInChildren<SkinnedMeshRenderer>(), _material);
	}

	private void OnDestroy()
	{
		Object.Destroy(_material);
	}

	private void Update()
	{
		SetMaterialProperty();
	}

	private void SetMaterialProperty()
	{
		_material.SetFloat("_ScrollOffset", Time.time * _scrollSpeed);
		float value = _blinkCurve.Evaluate(Mathf.Clamp01(Time.time % _blinkPeriod) / _blinkPeriod) * _intensity;
		_material.SetFloat("_Intensity", value);
	}
}
