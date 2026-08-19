using System;
using System.Diagnostics;
using Durango.Terrain;
using UnityEngine;

namespace Durango.Render.Water;

public class Ocean : WaterBase
{
	[Serializable]
	public class OceanSet
	{
		public string OceanType;

		public Material Material;

		public float FoamSpeed;

		public float FoamLength;

		public float FoamMaxIntensity;

		public float FoamMinIntensity;

		public float FoamMaxDepth;

		public float FoamMinDepth;

		public float MinBoundaryPow;

		public float MaxBoundaryPow;

		public float MinWetDarkness;

		public float MaxWetDarkness;
	}

	private static Ocean _ocean;

	[SerializeField]
	private OceanSet[] _oceanSets;

	[SerializeField]
	private Vector3 _eyeOffset;

	private float _foamTime;

	private OceanSet _curOceanSet;

	private int _eyePosId;

	private int _oceanTilingPeriod;

	private int _foamTimeId;

	private int _foamIntensityId;

	private int _foamDepthId;

	private int _boundaryPowId;

	private int _wetDarknessId;

	public static Ocean FindOcean()
	{
		if (_ocean != null)
		{
			return _ocean;
		}
		GameObject gameObject = GameObject.Find("Ocean");
		_ocean = ((!(gameObject == null)) ? gameObject.GetComponent<Ocean>() : null);
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
		if (!(base.SharedMaterial == null))
		{
			Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
			Shader.SetGlobalVector(_eyePosId, currentPosition + _eyeOffset);
			base.SharedMaterial.SetVector(WorldLightDirId, _specularLight.forward);
			UpdateTilingPeriod(_oceanTilingPeriod);
			UpdateFoamFactor();
		}
	}

	private void UpdateFoamFactor()
	{
		Material sharedMaterial = base.SharedMaterial;
		_foamTime = (_foamTime + Time.deltaTime * _curOceanSet.FoamSpeed) % 360f;
		double num = Math.Sin(_foamTime * ((float)Math.PI / 180f));
		float value = (float)((double)_curOceanSet.FoamLength * num);
		sharedMaterial.SetFloat(_foamTimeId, value);
		float num2 = (_curOceanSet.FoamMaxIntensity - _curOceanSet.FoamMinIntensity) / 2f;
		sharedMaterial.SetFloat(_foamIntensityId, (float)((double)num2 * num + (double)num2 + (double)_curOceanSet.FoamMinIntensity));
		float num3 = (_curOceanSet.FoamMaxDepth - _curOceanSet.FoamMinDepth) / 2f;
		sharedMaterial.SetFloat(_foamDepthId, (float)((double)(0f - num3) * num + (double)num3 + (double)_curOceanSet.FoamMinDepth));
		float num4 = _curOceanSet.MaxBoundaryPow - _curOceanSet.MinBoundaryPow;
		sharedMaterial.SetFloat(_boundaryPowId, (float)((double)num4 * num + (double)num4 + (double)_curOceanSet.MinBoundaryPow));
		float num5 = (_curOceanSet.MaxWetDarkness - _curOceanSet.MinWetDarkness) / 2f;
		double num6 = Math.Sin((_foamTime - 90f) * ((float)Math.PI / 180f));
		sharedMaterial.SetFloat(_wetDarknessId, (float)((double)num5 * num6 + (double)num5 + (double)_curOceanSet.MinWetDarkness));
	}

	protected override void InitMaterial()
	{
		SetMaterialType(TerrainMeta.OceanType);
	}

	public override void SetMaterialType(string oceanType)
	{
		for (int i = 0; i < _oceanSets.Length; i++)
		{
			if (!(_oceanSets[i].OceanType != oceanType))
			{
				_curOceanSet = _oceanSets[i];
				base.SharedMaterial = _curOceanSet.Material;
				return;
			}
		}
		Debug.LogError("Ocean Type not found - " + oceanType);
	}

	[Conditional("MAP_EXPORTER_BUILD")]
	public void ClearOcean()
	{
		Vector4 vector = base.SharedMaterial.GetVector("_DistortParams");
		vector.x = 0f;
		base.SharedMaterial.SetVector("_DistortParams", vector);
		Shader.SetGlobalVector(_eyePosId, new Vector2(-1111111f, -1111111f));
	}
}
