using UnityEngine;

public class Lake : KWater
{
	private static Lake _lake;

	[SerializeField]
	private Material _snowfieldLakeMaterial;

	[SerializeField]
	private Material _tundraLakeMaterial;

	[SerializeField]
	private Material _temperateLakeMaterial;

	[SerializeField]
	private Material _tropicalLakeMaterial;

	[SerializeField]
	private Material _grasslandLakeMaterial;

	[SerializeField]
	private Material _desertLakeMaterial;

	[SerializeField]
	private float _decoMinDepth;

	[SerializeField]
	private float _decoMaxDepth;

	[SerializeField]
	private float _decoMaxIntensity;

	private int _lakeTilingPeriodId;

	private int _decoMinDepthId;

	private int _decoMaxDepthId;

	private int _decoFactorId;

	public static Lake FindLake()
	{
		if ((Object)(object)_lake != (Object)null)
		{
			return _lake;
		}
		GameObject val = GameObject.Find("Lake");
		_lake = ((!((Object)(object)val == (Object)null)) ? val.GetComponent<Lake>() : null);
		return _lake;
	}

	private void Start()
	{
		_lakeTilingPeriodId = Shader.PropertyToID("_LakeTilingPeriod");
		_decoMinDepthId = Shader.PropertyToID("_DecoMinDepth");
		_decoMaxDepthId = Shader.PropertyToID("_DecoMaxDepth");
		_decoFactorId = Shader.PropertyToID("_DecoFactor");
	}

	private void Update()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)base.SharedMaterial == (Object)null))
		{
			base.SharedMaterial.SetVector(WorldLightDirId, Vector4.op_Implicit(_specularLight.forward));
			UpdateTilingPeriod(_lakeTilingPeriodId);
		}
	}

	protected override void InitMaterial()
	{
		SetMaterialIndex(TerrainMeta.LakeType);
	}

	private void UpdateDecoFactor()
	{
		Material sharedMaterial = base.SharedMaterial;
		sharedMaterial.SetFloat(_decoMinDepthId, _decoMinDepth);
		sharedMaterial.SetFloat(_decoMaxDepthId, _decoMaxDepth);
		float num = (_decoMinDepth + _decoMaxDepth) / 2f;
		float num2 = num - _decoMinDepth;
		float num3 = num - _decoMaxDepth;
		sharedMaterial.SetFloat(_decoFactorId, _decoMaxIntensity / (num2 * num3));
	}

	public int GetMaterialIndex()
	{
		for (int i = 0; i < 6; i++)
		{
			if ((Object)(object)FindMaterial(i) == (Object)(object)base.SharedMaterial)
			{
				return i;
			}
		}
		return 0;
	}

	public void SetMaterialIndex(int index)
	{
		base.SharedMaterial = FindMaterial(index);
		UpdateDecoFactor();
	}

	private Material FindMaterial(int index)
	{
		return (Material)(index switch
		{
			0 => _snowfieldLakeMaterial, 
			1 => _tundraLakeMaterial, 
			2 => _temperateLakeMaterial, 
			3 => _tropicalLakeMaterial, 
			4 => _grasslandLakeMaterial, 
			5 => _desertLakeMaterial, 
			_ => _temperateLakeMaterial, 
		});
	}
}
