using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("2D Toolkit/Backend/tk2dSpriteCollection")]
public class tk2dSpriteCollection : MonoBehaviour
{
	public enum NormalGenerationMode
	{
		None,
		NormalsOnly,
		NormalsAndTangents
	}

	public enum TextureCompression
	{
		Uncompressed,
		Reduced16Bit,
		Compressed,
		Dithered16Bit_Alpha,
		Dithered16Bit_NoAlpha
	}

	public enum AtlasFormat
	{
		UnityTexture,
		Png
	}

	[Serializable]
	public class AttachPointTestSprite
	{
		public string attachPointName = string.Empty;

		public tk2dSpriteCollectionData spriteCollection;

		public int spriteId = -1;

		public bool CompareTo(AttachPointTestSprite src)
		{
			return src.attachPointName == attachPointName && (Object)(object)src.spriteCollection == (Object)(object)spriteCollection && src.spriteId == spriteId;
		}

		public void CopyFrom(AttachPointTestSprite src)
		{
			attachPointName = src.attachPointName;
			spriteCollection = src.spriteCollection;
			spriteId = src.spriteId;
		}
	}

	public const int CURRENT_VERSION = 4;

	[SerializeField]
	private tk2dSpriteCollectionDefinition[] textures;

	[SerializeField]
	private Texture2D[] textureRefs;

	public tk2dSpriteSheetSource[] spriteSheets;

	public tk2dSpriteCollectionFont[] fonts;

	public tk2dSpriteCollectionDefault defaults;

	public List<tk2dSpriteCollectionPlatform> platforms = new List<tk2dSpriteCollectionPlatform>();

	public bool managedSpriteCollection;

	public tk2dSpriteCollection linkParent;

	public bool loadable;

	public AtlasFormat atlasFormat;

	public int maxTextureSize = 2048;

	public bool forceTextureSize;

	public int forcedTextureWidth = 2048;

	public int forcedTextureHeight = 2048;

	public TextureCompression textureCompression = TextureCompression.Compressed;

	public int atlasWidth;

	public int atlasHeight;

	public bool forceSquareAtlas;

	public float atlasWastage;

	public bool allowMultipleAtlases;

	public bool removeDuplicates = true;

	public tk2dSpriteCollectionDefinition[] textureParams;

	public tk2dSpriteCollectionData spriteCollection;

	public bool premultipliedAlpha;

	public Material[] altMaterials;

	public Material[] atlasMaterials;

	public Texture2D[] atlasTextures;

	public Texture2D[] atlasAlphaTextures;

	public TextAsset[] atlasTextureFiles = (TextAsset[])(object)new TextAsset[0];

	public TextAsset[] atlasAlphaTextureFiles = (TextAsset[])(object)new TextAsset[0];

	[SerializeField]
	private bool useTk2dCamera;

	[SerializeField]
	private int targetHeight = 640;

	[SerializeField]
	private float targetOrthoSize = 10f;

	public tk2dSpriteCollectionSize sizeDef = tk2dSpriteCollectionSize.Default();

	public float globalScale = 1f;

	public float globalTextureRescale = 1f;

	public List<AttachPointTestSprite> attachPointTestSprites = new List<AttachPointTestSprite>();

	[SerializeField]
	private bool pixelPerfectPointSampled;

	public FilterMode filterMode = (FilterMode)1;

	public TextureWrapMode wrapMode = (TextureWrapMode)1;

	public bool userDefinedTextureSettings;

	public bool mipmapEnabled;

	public int anisoLevel = 1;

	public tk2dSpriteDefinition.PhysicsEngine physicsEngine;

	public float physicsDepth = 0.1f;

	public bool disableTrimming;

	public bool disableRotation;

	public NormalGenerationMode normalGenerationMode;

	public int padAmount = -1;

	public bool autoUpdate = true;

	public float editorDisplayScale = 1f;

	public int version;

	public string assetName = string.Empty;

	public List<tk2dLinkedSpriteCollection> linkedSpriteCollections = new List<tk2dLinkedSpriteCollection>();

	public Texture2D[] DoNotUse__TextureRefs
	{
		get
		{
			return textureRefs;
		}
		set
		{
			textureRefs = value;
		}
	}

	public bool HasPlatformData => platforms.Count > 1;

	public void Upgrade()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (version == 4)
		{
			return;
		}
		if (version == 0)
		{
			if (pixelPerfectPointSampled)
			{
				filterMode = (FilterMode)0;
			}
			else
			{
				filterMode = (FilterMode)1;
			}
			userDefinedTextureSettings = true;
		}
		if (version < 3 && textureRefs != null && textureParams != null && textureRefs.Length == textureParams.Length)
		{
			for (int i = 0; i < textureRefs.Length; i++)
			{
				textureParams[i].texture = textureRefs[i];
			}
			textureRefs = null;
		}
		if (version < 4)
		{
			sizeDef.CopyFromLegacy(useTk2dCamera, targetOrthoSize, targetHeight);
		}
		version = 4;
	}
}
