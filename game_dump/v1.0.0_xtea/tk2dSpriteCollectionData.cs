using System;
using System.Collections.Generic;
using UnityEngine;
using tk2dRuntime;

[AddComponentMenu("2D Toolkit/Backend/tk2dSpriteCollectionData")]
public class tk2dSpriteCollectionData : MonoBehaviour
{
	public const int CURRENT_VERSION = 3;

	public int version;

	public bool materialIdsValid;

	public bool needMaterialInstance;

	public tk2dSpriteDefinition[] spriteDefinitions;

	private Dictionary<string, int> spriteNameLookupDict;

	public bool premultipliedAlpha;

	public Material material;

	public Material[] materials;

	[NonSerialized]
	public Material[] materialInsts;

	[NonSerialized]
	public Texture2D[] textureInsts = (Texture2D[])(object)new Texture2D[0];

	public Texture[] textures;

	public Texture[] alpha_textures;

	public TextAsset[] pngTextures = (TextAsset[])(object)new TextAsset[0];

	public TextAsset[] pngAlphaTextures = (TextAsset[])(object)new TextAsset[0];

	public int[] materialPngTextureId = new int[0];

	public FilterMode textureFilterMode = (FilterMode)1;

	public bool textureMipMaps;

	public bool allowMultipleAtlases;

	public string spriteCollectionGUID;

	public string spriteCollectionName;

	public string assetName = string.Empty;

	public bool loadable;

	public float invOrthoSize = 1f;

	public float halfTargetHeight = 1f;

	public int buildKey;

	public string dataGuid = string.Empty;

	public bool managedSpriteCollection;

	public bool hasPlatformData;

	public string[] spriteCollectionPlatforms;

	public string[] spriteCollectionPlatformGUIDs;

	private tk2dSpriteCollectionData platformSpecificData;

	public static readonly string internalResourcePrefix = "tk2dInternal$.";

	public bool Transient { get; set; }

	public int Count => inst.spriteDefinitions.Length;

	public tk2dSpriteDefinition FirstValidDefinition
	{
		get
		{
			tk2dSpriteDefinition[] array = inst.spriteDefinitions;
			foreach (tk2dSpriteDefinition tk2dSpriteDefinition2 in array)
			{
				if (tk2dSpriteDefinition2.Valid)
				{
					return tk2dSpriteDefinition2;
				}
			}
			return null;
		}
	}

	public int FirstValidDefinitionIndex
	{
		get
		{
			tk2dSpriteCollectionData tk2dSpriteCollectionData2 = inst;
			for (int i = 0; i < tk2dSpriteCollectionData2.spriteDefinitions.Length; i++)
			{
				if (tk2dSpriteCollectionData2.spriteDefinitions[i].Valid)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public tk2dSpriteCollectionData inst
	{
		get
		{
			if ((Object)(object)platformSpecificData == (Object)null)
			{
				if (hasPlatformData)
				{
					string currentPlatform = tk2dSystem.CurrentPlatform;
					string text = string.Empty;
					for (int i = 0; i < spriteCollectionPlatforms.Length; i++)
					{
						if (spriteCollectionPlatforms[i] == currentPlatform)
						{
							text = spriteCollectionPlatformGUIDs[i];
							break;
						}
					}
					if (text.Length == 0)
					{
						text = spriteCollectionPlatformGUIDs[0];
					}
					platformSpecificData = tk2dSystem.LoadResourceByGUID<tk2dSpriteCollectionData>(text);
				}
				else
				{
					platformSpecificData = this;
				}
			}
			platformSpecificData.Init();
			return platformSpecificData;
		}
	}

	public int GetSpriteIdByName(string name)
	{
		return GetSpriteIdByName(name, 0);
	}

	public int GetSpriteIdByName(string name, int defaultValue)
	{
		inst.InitDictionary();
		int value = defaultValue;
		if (!inst.spriteNameLookupDict.TryGetValue(name, out value))
		{
			return defaultValue;
		}
		return value;
	}

	public void ClearDictionary()
	{
		spriteNameLookupDict = null;
	}

	public tk2dSpriteDefinition GetSpriteDefinition(string name)
	{
		int spriteIdByName = GetSpriteIdByName(name, -1);
		if (spriteIdByName == -1)
		{
			return null;
		}
		return spriteDefinitions[spriteIdByName];
	}

	public void InitDictionary()
	{
		if (spriteNameLookupDict == null)
		{
			spriteNameLookupDict = new Dictionary<string, int>(spriteDefinitions.Length);
			for (int i = 0; i < spriteDefinitions.Length; i++)
			{
				spriteNameLookupDict[spriteDefinitions[i].name] = i;
			}
		}
	}

	public bool IsValidSpriteId(int id)
	{
		if (id < 0 || id >= inst.spriteDefinitions.Length)
		{
			return false;
		}
		return inst.spriteDefinitions[id].Valid;
	}

	public void InitMaterialIds()
	{
		if (inst.materialIdsValid)
		{
			return;
		}
		int num = -1;
		Dictionary<Material, int> dictionary = new Dictionary<Material, int>();
		for (int i = 0; i < inst.materials.Length; i++)
		{
			if (num == -1 && (Object)(object)inst.materials[i] != (Object)null)
			{
				num = i;
			}
			dictionary[materials[i]] = i;
		}
		if (num == -1)
		{
			Debug.LogError((object)"Init material ids failed.");
			return;
		}
		tk2dSpriteDefinition[] array = inst.spriteDefinitions;
		foreach (tk2dSpriteDefinition tk2dSpriteDefinition2 in array)
		{
			if (!dictionary.TryGetValue(tk2dSpriteDefinition2.material, out tk2dSpriteDefinition2.materialId))
			{
				tk2dSpriteDefinition2.materialId = num;
			}
		}
		inst.materialIdsValid = true;
	}

	private void Init()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if (materialInsts != null)
		{
			return;
		}
		if (spriteDefinitions == null)
		{
			spriteDefinitions = new tk2dSpriteDefinition[0];
		}
		if (materials == null)
		{
			materials = (Material[])(object)new Material[0];
		}
		materialInsts = (Material[])(object)new Material[materials.Length];
		if (needMaterialInstance)
		{
			if (tk2dSystem.OverrideBuildMaterial)
			{
				for (int i = 0; i < materials.Length; i++)
				{
					materialInsts[i] = new Material(Shader.Find("tk2d/BlendVertexColor"));
				}
			}
			else
			{
				bool flag = false;
				if (pngTextures.Length > 0)
				{
					flag = true;
					textureInsts = (Texture2D[])(object)new Texture2D[pngTextures.Length];
					for (int j = 0; j < pngTextures.Length; j++)
					{
						Texture2D val = new Texture2D(4, 4, (TextureFormat)5, textureMipMaps);
						val.LoadImage(pngTextures[j].bytes);
						textureInsts[j] = val;
						((Texture)val).filterMode = textureFilterMode;
						val.Apply(textureMipMaps, true);
					}
				}
				for (int k = 0; k < materials.Length; k++)
				{
					materialInsts[k] = Object.Instantiate<Material>(materials[k]);
					if (flag)
					{
						int num = ((materialPngTextureId.Length != 0) ? materialPngTextureId[k] : 0);
						materialInsts[k].mainTexture = (Texture)(object)textureInsts[num];
					}
				}
			}
			for (int l = 0; l < spriteDefinitions.Length; l++)
			{
				tk2dSpriteDefinition tk2dSpriteDefinition2 = spriteDefinitions[l];
				tk2dSpriteDefinition2.materialInst = materialInsts[tk2dSpriteDefinition2.materialId];
			}
		}
		else
		{
			for (int m = 0; m < materials.Length; m++)
			{
				materialInsts[m] = materials[m];
			}
			for (int n = 0; n < spriteDefinitions.Length; n++)
			{
				tk2dSpriteDefinition tk2dSpriteDefinition3 = spriteDefinitions[n];
				tk2dSpriteDefinition3.materialInst = tk2dSpriteDefinition3.material;
			}
		}
		tk2dEditorSpriteDataUnloader.Register(this);
	}

	public static tk2dSpriteCollectionData CreateFromTexture(Texture texture, tk2dSpriteCollectionSize size, string[] names, Rect[] regions, Vector2[] anchors)
	{
		return SpriteCollectionGenerator.CreateFromTexture(texture, size, names, regions, anchors);
	}

	public static tk2dSpriteCollectionData CreateFromTexturePacker(tk2dSpriteCollectionSize size, string texturePackerData, Texture texture)
	{
		return SpriteCollectionGenerator.CreateFromTexturePacker(size, texturePackerData, texture);
	}

	public void ResetPlatformData()
	{
		tk2dEditorSpriteDataUnloader.Unregister(this);
		if ((Object)(object)platformSpecificData != (Object)null)
		{
			platformSpecificData.DestroyTextureInsts();
		}
		DestroyTextureInsts();
		if (Object.op_Implicit((Object)(object)platformSpecificData))
		{
			platformSpecificData = null;
		}
		materialInsts = null;
	}

	private void DestroyTextureInsts()
	{
		Texture2D[] array = textureInsts;
		foreach (Texture2D val in array)
		{
			Object.DestroyImmediate((Object)(object)val);
		}
		textureInsts = (Texture2D[])(object)new Texture2D[0];
	}

	public void UnloadTextures()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		tk2dSpriteCollectionData tk2dSpriteCollectionData2 = inst;
		Texture[] array = tk2dSpriteCollectionData2.textures;
		for (int i = 0; i < array.Length; i++)
		{
			Texture2D val = (Texture2D)array[i];
			Resources.UnloadAsset((Object)(object)val);
		}
		tk2dSpriteCollectionData2.DestroyMaterialInsts();
		tk2dSpriteCollectionData2.DestroyTextureInsts();
	}

	private void DestroyMaterialInsts()
	{
		if (needMaterialInstance)
		{
			Material[] array = materialInsts;
			foreach (Material val in array)
			{
				Object.DestroyImmediate((Object)(object)val);
			}
		}
		materialInsts = null;
	}

	private void OnDestroy()
	{
		if (Transient)
		{
			Material[] array = materials;
			foreach (Material val in array)
			{
				Object.DestroyImmediate((Object)(object)val);
			}
		}
		else if (needMaterialInstance)
		{
			Material[] array2 = materialInsts;
			foreach (Material val2 in array2)
			{
				Object.DestroyImmediate((Object)(object)val2);
			}
			materialInsts = (Material[])(object)new Material[0];
			Texture2D[] array3 = textureInsts;
			foreach (Texture2D val3 in array3)
			{
				Object.DestroyImmediate((Object)(object)val3);
			}
			textureInsts = (Texture2D[])(object)new Texture2D[0];
		}
		ResetPlatformData();
	}
}
