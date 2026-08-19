using System;
using UnityEngine;

public class Ocean : KWater
{
	private static Ocean _ocean;

	[SerializeField]
	private Material _warmOceanMaterial;

	[SerializeField]
	private Material _coldOceanMaterial;

	[SerializeField]
	private Vector3 _eyeOffset;

	[SerializeField]
	private float _foamSpeed;

	[SerializeField]
	private float _foamLength;

	[SerializeField]
	private float _foamMaxIntensity;

	[SerializeField]
	private float _foamMinIntensity;

	[SerializeField]
	private float _foamMaxDepth;

	[SerializeField]
	private float _foamMinDepth;

	[SerializeField]
	private float _minBoundaryPow;

	[SerializeField]
	private float _maxBoundaryPow;

	[SerializeField]
	private float _minWetDarkness;

	[SerializeField]
	private float _maxWetDarkness;

	private float _foamTime;

	private int _eyePosId;

	private int _oceanTilingPeriod;

	private int _foamTimeId;

	private int _foamIntensityId;

	private int _foamDepthId;

	private int _boundaryPowId;

	private int _wetDarknessId;

	public static Ocean FindOcean()
	{
		if ((Object)(object)_ocean != (Object)null)
		{
			return _ocean;
		}
		GameObject val = GameObject.Find("Ocean");
		_ocean = ((!((Object)(object)val == (Object)null)) ? val.GetComponent<Ocean>() : null);
		return _ocean;
	}

	private void Start()
	{
		_eyePosId = Shader.PropertyToID("_EyePos");
		_oceanTilingPeriod = Shader.PropertyToID("_OceanTilingPeriod");
		_foamTimeId = Shader.PropertyToID("_FoamTime");
		_foamIntensityId = Shader.PropertyToID("_FoamIntensity");
		_foamDepthId = Shader.PropertyToID("_FoamDepth");
		_boundaryPowId = Shader.PropertyToID("_BoundaryPow");
		_wetDarknessId = Shader.PropertyToID("_WetDarkness");
	}

	private void Update()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)base.SharedMaterial == (Object)null))
		{
			Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
			Shader.SetGlobalVector(_eyePosId, Vector4.op_Implicit(currentPosition + _eyeOffset));
			base.SharedMaterial.SetVector(WorldLightDirId, Vector4.op_Implicit(_specularLight.forward));
			UpdateTilingPeriod(_oceanTilingPeriod);
			UpdateFoamFactor();
		}
	}

	private void UpdateFoamFactor()
	{
		Material sharedMaterial = base.SharedMaterial;
		_foamTime = (_foamTime + Time.deltaTime * _foamSpeed) % 360f;
		double num = Math.Sin(_foamTime * ((float)Math.PI / 180f));
		float num2 = (float)((double)_foamLength * num);
		sharedMaterial.SetFloat(_foamTimeId, num2);
		float num3 = (_foamMaxIntensity - _foamMinIntensity) / 2f;
		sharedMaterial.SetFloat(_foamIntensityId, (float)((double)num3 * num + (double)num3 + (double)_foamMinIntensity));
		float num4 = (_foamMaxDepth - _foamMinDepth) / 2f;
		sharedMaterial.SetFloat(_foamDepthId, (float)((double)(0f - num4) * num + (double)num4 + (double)_foamMinDepth));
		float num5 = _maxBoundaryPow - _minBoundaryPow;
		sharedMaterial.SetFloat(_boundaryPowId, (float)((double)num5 * num + (double)num5 + (double)_minBoundaryPow));
		float num6 = (_maxWetDarkness - _minWetDarkness) / 2f;
		double num7 = Math.Sin((_foamTime - 90f) * ((float)Math.PI / 180f));
		sharedMaterial.SetFloat(_wetDarknessId, (float)((double)num6 * num7 + (double)num6 + (double)_minWetDarkness));
	}

	protected override void InitMaterial()
	{
		base.SharedMaterial = ((!TerrainMeta.IsColdOcean) ? _warmOceanMaterial : _coldOceanMaterial);
	}
}
