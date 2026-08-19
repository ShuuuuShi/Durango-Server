using System;
using Durango.Terrain;
using UnityEngine;

namespace Durango.Render.Water;

public class Lake : WaterBase
{
	[Serializable]
	public class LakeSet
	{
		public string LakeType;

		public Material Material;
	}

	private static Lake _lake;

	[SerializeField]
	private LakeSet[] _lakeSets;

	private int _lakeTilingPeriodId;

	public static Lake FindLake()
	{
		if (_lake != null)
		{
			return _lake;
		}
		GameObject gameObject = GameObject.Find("Lake");
		_lake = ((!(gameObject == null)) ? gameObject.GetComponent<Lake>() : null);
		return _lake;
	}

	private void Start()
	{
		_lakeTilingPeriodId = Shader.PropertyToID("_LakeTilingPeriod");
	}

	private void Update()
	{
		if (!(base.SharedMaterial == null))
		{
			base.SharedMaterial.SetVector(WorldLightDirId, _specularLight.forward);
			UpdateTilingPeriod(_lakeTilingPeriodId);
		}
	}

	protected override void InitMaterial()
	{
		SetMaterialType(TerrainMeta.LakeType);
	}

	public override void SetMaterialType(string lakeType)
	{
		for (int i = 0; i < _lakeSets.Length; i++)
		{
			if (!(_lakeSets[i].LakeType != lakeType))
			{
				base.SharedMaterial = _lakeSets[i].Material;
				return;
			}
		}
		Debug.LogError("Lake Type not found - " + lakeType);
	}
}
