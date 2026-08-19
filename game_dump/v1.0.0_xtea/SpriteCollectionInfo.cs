using System;
using TerrainData;
using UnityEngine;

[Serializable]
public class SpriteCollectionInfo
{
	public enum Status
	{
		NotLoaded,
		Loading,
		Loaded
	}

	[SerializeField]
	public string SpriteCollectionPath;

	[SerializeField]
	public Biome Biome;

	public tk2dSpriteCollectionData SpriteCollectionData { get; set; }

	public Material ShadowMaterial { get; private set; }

	public Material AdditiveMaterial { get; private set; }

	public Status LoadStatus { get; private set; }

	public event Action<SpriteCollectionInfo> Loaded;

	public void Initialize(Shader shadowShader, Color shadowColor, Shader additiveShader)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		LoadStatus = Status.Loading;
		if (string.IsNullOrEmpty(SpriteCollectionPath))
		{
			return;
		}
		KSingleton<AssetBundleManager>.Instance().RequestAsset(SpriteCollectionPath, typeof(GameObject), delegate(Object obj)
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = (GameObject)(object)((obj is GameObject) ? obj : null);
			if ((Object)(object)val == (Object)null)
			{
				LoadStatus = Status.NotLoaded;
			}
			else
			{
				tk2dSpriteCollectionData component = val.GetComponent<tk2dSpriteCollectionData>();
				if ((Object)(object)component == (Object)null)
				{
					LoadStatus = Status.NotLoaded;
				}
				else
				{
					SpriteCollectionData = component;
					UpdateShadowMaterial(shadowShader, shadowColor);
					UpdateAdditiveMaterial(additiveShader);
					LoadStatus = Status.Loaded;
					if (this.Loaded != null)
					{
						this.Loaded(this);
					}
				}
			}
		});
	}

	private bool IsCollectionMaterialNullOrEmpty()
	{
		return (Object)(object)SpriteCollectionData == (Object)null || SpriteCollectionData.materials == null || SpriteCollectionData.materials.Length == 0 || (Object)(object)SpriteCollectionData.materials[0] == (Object)null;
	}

	public void UpdateShadowMaterial(Shader shadowShader, Color shadowColor)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		if (IsCollectionMaterialNullOrEmpty())
		{
			ShadowMaterial = null;
			return;
		}
		if ((Object)(object)ShadowMaterial == (Object)null)
		{
			ShadowMaterial = new Material(shadowShader);
		}
		ShadowMaterial.CopyPropertiesFromMaterial(SpriteCollectionData.materials[0]);
		ShadowMaterial.shader = shadowShader;
		ShadowMaterial.SetColor("_ShadowColor", shadowColor);
	}

	public void UpdateAdditiveMaterial(Shader additiveShader)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		if (IsCollectionMaterialNullOrEmpty())
		{
			AdditiveMaterial = null;
			return;
		}
		if ((Object)(object)AdditiveMaterial == (Object)null)
		{
			AdditiveMaterial = new Material(additiveShader);
		}
		AdditiveMaterial.CopyPropertiesFromMaterial(SpriteCollectionData.materials[0]);
		AdditiveMaterial.shader = additiveShader;
	}
}
