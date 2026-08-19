using System;
using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Render.Sprite;

public class SpriteManager : Singleton<SpriteManager>
{
	[Serializable]
	public struct NameToPath
	{
		public string Name;

		public string Path;
	}

	private const int SpriteGroupCount = 6;

	[SerializeField]
	public GameObject NaturalObject;

	[SerializeField]
	public GameObject SpriteRenderingObject;

	[SerializeField]
	public Color ShadowColor;

	[SerializeField]
	public float GrassAlpha = 0.9f;

	[SerializeField]
	public float AdditiveBeginTime = 20f;

	[SerializeField]
	public float AdditiveEndTime = 5f;

	[SerializeField]
	public float AdditiveFrequency = 1f;

	[SerializeField]
	public float AdditiveMinInDay;

	[SerializeField]
	public float AdditiveMaxInDay = 0.5f;

	[SerializeField]
	public float AdditiveMinInNight = 0.3f;

	[SerializeField]
	public float AdditiveMaxInNight = 1f;

	[SerializeField]
	public float BushWhackAmplitude = 0.17f;

	[SerializeField]
	public float BushWhackFrequency = 15f;

	[SerializeField]
	public float AspectRatio = 5f;

	[SerializeField]
	public float FullyTransparentArea = 0.11f;

	[SerializeField]
	public float TreesCoveringFactor = 17f;

	[SerializeField]
	public float ShrubsCoveringFactor = 25f;

	[SerializeField]
	public float ShrubsMinTransparency = 0.5f;

	[SerializeField]
	[HideInInspector]
	private SpriteGroup[] _kSpriteGroupArray = new SpriteGroup[6];

	[SerializeField]
	private Shader _shadowSpriteShader;

	[SerializeField]
	private Shader _additiveSpriteShader;

	[SerializeField]
	private List<NameToPath> _nameToPathList;

	private readonly Dictionary<string, SpriteCollectionInfo> _collectionMap = new Dictionary<string, SpriteCollectionInfo>();

	public event Action SpriteCollectionLoaded;

	public Sprite CreateSprite(Transform parent, bool selectable)
	{
		GameObject gameObject = ((!selectable) ? SpriteRenderingObject : NaturalObject);
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
		if (parent != null)
		{
			gameObject2.transform.parent = parent;
		}
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localRotation = gameObject.transform.localRotation;
		gameObject2.SetActive(value: false);
		return new Sprite(gameObject2);
	}

	public Sprite CreateSprite(SpriteObjectType spriteObjectType, [NotNull] string spriteName)
	{
		Sprite sprite = CreateSprite(null, selectable: false);
		sprite.SetSpriteByName(spriteObjectType, spriteName);
		return sprite;
	}

	protected override void OnAwake()
	{
		if (_kSpriteGroupArray == null || _kSpriteGroupArray.Length != 6)
		{
			SpriteGroup[] array = new SpriteGroup[6];
			if (_kSpriteGroupArray != null)
			{
				int num = Mathf.Min(_kSpriteGroupArray.Length, 6);
				for (int i = 0; i < num; i++)
				{
					array[i] = _kSpriteGroupArray[i];
				}
				for (int j = num; j < 6; j++)
				{
					array[j] = new SpriteGroup();
				}
			}
			else
			{
				for (int k = 0; k < 6; k++)
				{
					array[k] = new SpriteGroup();
				}
			}
			_kSpriteGroupArray = array;
		}
		FillCollectionMap();
		SetShaderGlobalFloats();
	}

	private void Start()
	{
		PlayerBehavior.LocalPlayer.IsIndoorChanged += LocalPlayer_IsIndoorChanged;
	}

	private static void LocalPlayer_IsIndoorChanged()
	{
		Shader.SetGlobalInt("_SpriteStencilComp", PlayerBehavior.LocalPlayer.IsIndoor ? 6 : 0);
	}

	private void FillCollectionMap()
	{
		_collectionMap.Clear();
		for (int i = 0; i < 6; i++)
		{
			foreach (SpriteCollectionInfo spriteCollectionInfo2 in _kSpriteGroupArray[i].SpriteCollectionInfoList)
			{
				spriteCollectionInfo2.SpriteObjectType = (SpriteObjectType)i;
			}
		}
		int j = 0;
		for (int count = _nameToPathList.Count; j < count; j++)
		{
			NameToPath nameToPath = _nameToPathList[j];
			SpriteCollectionInfo spriteCollectionInfo = FindSpriteCollectionInfo(nameToPath.Path);
			if (spriteCollectionInfo != null)
			{
				_collectionMap[nameToPath.Name] = spriteCollectionInfo;
			}
		}
	}

	private void SetShaderGlobalFloats()
	{
		Shader.SetGlobalFloat("_AspectRatio", AspectRatio);
		Shader.SetGlobalFloat("_FullyTransparentArea", FullyTransparentArea);
		Shader.SetGlobalFloat("_TreesCoveringFactor", TreesCoveringFactor);
		Shader.SetGlobalFloat("_ShrubsCoveringFactor", ShrubsCoveringFactor);
		Shader.SetGlobalFloat("_ShrubsMinTransparency", ShrubsMinTransparency);
	}

	private SpriteCollectionInfo FindSpriteCollectionInfo(string path)
	{
		for (int i = 0; i < 6; i++)
		{
			foreach (SpriteCollectionInfo spriteCollectionInfo in _kSpriteGroupArray[i].SpriteCollectionInfoList)
			{
				if (spriteCollectionInfo.SpriteCollectionPath == path)
				{
					return spriteCollectionInfo;
				}
			}
		}
		return null;
	}

	private void LoadSpriteCollectionInfo(SpriteCollectionInfo info)
	{
		info.Loaded -= SpriteCollectionInfo_Loaded;
		info.Loaded += SpriteCollectionInfo_Loaded;
		info.Initialize(_shadowSpriteShader, ShadowColor, _additiveSpriteShader);
	}

	private void SpriteCollectionInfo_Loaded(SpriteCollectionInfo info)
	{
		if (this.SpriteCollectionLoaded != null)
		{
			this.SpriteCollectionLoaded();
		}
	}

	public SpriteCollectionInfo GetSpriteCollectionInfo(string spriteName, bool autoLoad = true)
	{
		SpriteCollectionInfo spriteCollectionInfo = ((spriteName == null) ? null : _collectionMap.Get(spriteName));
		if (spriteCollectionInfo == null)
		{
			return null;
		}
		if (spriteCollectionInfo.LoadStatus == SpriteCollectionInfo.Status.NotLoaded && autoLoad)
		{
			LoadSpriteCollectionInfo(spriteCollectionInfo);
		}
		return spriteCollectionInfo;
	}

	public SpriteObjectType GetSpriteObjectType(string spriteName)
	{
		return _collectionMap.Get(spriteName)?.SpriteObjectType ?? SpriteObjectType.Unspecified;
	}

	public SpriteGroup GetKSpriteGroup(SpriteObjectType type)
	{
		if (_kSpriteGroupArray.Length <= (int)type)
		{
			throw new Exception("SpriteGroupArray out of index");
		}
		return (type != SpriteObjectType.Unspecified) ? _kSpriteGroupArray[(int)type] : null;
	}
}
