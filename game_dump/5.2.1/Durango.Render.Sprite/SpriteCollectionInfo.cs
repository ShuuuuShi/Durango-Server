using System;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Sprite;

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

	public SpriteObjectType SpriteObjectType { get; set; }

	public tk2dSpriteCollectionData SpriteCollectionData { get; set; }

	public Material ShadowMaterial { get; private set; }

	public Material AdditiveMaterial { get; private set; }

	public Status LoadStatus { get; private set; }

	public event Action<SpriteCollectionInfo> Loaded;

	public void Initialize(Shader shadowShader, Color shadowColor, Shader additiveShader)
	{
		LoadStatus = Status.Loading;
		if (string.IsNullOrEmpty(SpriteCollectionPath))
		{
			return;
		}
		Singleton<AssetBundleManager>.Instance().RequestAsset(SpriteCollectionPath, typeof(GameObject), delegate(UnityEngine.Object obj)
		{
			GameObject gameObject = obj as GameObject;
			if (gameObject == null)
			{
				LoadStatus = Status.NotLoaded;
			}
			else
			{
				tk2dSpriteCollectionData component = gameObject.GetComponent<tk2dSpriteCollectionData>();
				if (component == null)
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
		if (!(SpriteCollectionData == null) && SpriteCollectionData.materials != null && SpriteCollectionData.materials.Length != 0)
		{
			return SpriteCollectionData.materials[0] == null;
		}
		return true;
	}

	public void UpdateShadowMaterial(Shader shadowShader, Color shadowColor)
	{
		if (IsCollectionMaterialNullOrEmpty())
		{
			ShadowMaterial = null;
			return;
		}
		if (ShadowMaterial == null)
		{
			ShadowMaterial = new Material(shadowShader);
		}
		ShadowMaterial.CopyPropertiesFromMaterial(SpriteCollectionData.materials[0]);
		ShadowMaterial.shader = shadowShader;
		ShadowMaterial.SetColor("_ShadowColor", shadowColor);
	}

	public void UpdateAdditiveMaterial(Shader additiveShader)
	{
		if (IsCollectionMaterialNullOrEmpty())
		{
			AdditiveMaterial = null;
			return;
		}
		if (AdditiveMaterial == null)
		{
			AdditiveMaterial = new Material(additiveShader);
		}
		AdditiveMaterial.CopyPropertiesFromMaterial(SpriteCollectionData.materials[0]);
		AdditiveMaterial.shader = additiveShader;
	}
}
