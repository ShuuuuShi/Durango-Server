using UnityEngine;

public class Blinker : MeshCloner
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

	private Material _material;

	protected override Material GetSourceMaterial()
	{
		return _material;
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		_material = new Material(_blinkShader);
		_material.SetColor("_Color", _color);
		_material.SetTexture("_MainTex", (Texture)(object)_blinkGradation);
		_material.SetVector("_ScrollUV", Vector4.op_Implicit(((Vector2)(ref _scrollUV)).normalized));
		Add(((Component)this).GetComponentsInChildren<SkinnedMeshRenderer>());
		base.Show = true;
	}

	private void OnDestroy()
	{
		Object.Destroy((Object)(object)_material);
	}

	private void Update()
	{
		if (base.Show)
		{
			SetMaterialProperty();
		}
	}

	private void SetMaterialProperty()
	{
		_material.SetFloat("_ScrollOffset", Time.time * _scrollSpeed);
		float num = _blinkCurve.Evaluate(Mathf.Clamp01(Time.time % _blinkPeriod) / _blinkPeriod) * _intensity;
		_material.SetFloat("_Intensity", num);
	}
}
