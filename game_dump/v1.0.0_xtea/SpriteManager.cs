using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : KSingleton<SpriteManager>
{
	[Serializable]
	public struct NameToPath
	{
		public string Name;

		public string Path;
	}

	private const int SpriteGroupCount = 5;

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
	[HideInInspector]
	private KSpriteGroup[] _kSpriteGroupArray = new KSpriteGroup[5];

	[SerializeField]
	private Shader _shadowSpriteShader;

	[SerializeField]
	private Shader _additiveSpriteShader;

	[SerializeField]
	private List<NameToPath> _nameToPathList;

	private readonly Dictionary<string, SpriteCollectionInfo> _collectionMap = new Dictionary<string, SpriteCollectionInfo>();

	public event Action SpriteCollectionLoaded;

	public KSprite CreateSprite(GameObject parent, bool selectable)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = ((!selectable) ? SpriteRenderingObject : NaturalObject);
		GameObject val2 = Object.Instantiate<GameObject>(val);
		if ((Object)(object)parent != (Object)null)
		{
			val2.transform.parent = parent.transform;
		}
		val2.transform.localPosition = Vector3.zero;
		val2.transform.localRotation = val.transform.localRotation;
		val2.SetActive(false);
		return new KSprite(val2);
	}

	public KSprite CreateSprite(SpriteObjectType spriteObjectType, string spriteName)
	{
		KSprite kSprite = CreateSprite(null, selectable: false);
		kSprite.SetSpriteByName(spriteObjectType, spriteName);
		return kSprite;
	}

	private void OnEnable()
	{
		if (_kSpriteGroupArray == null || _kSpriteGroupArray.Length != 5)
		{
			KSpriteGroup[] array = new KSpriteGroup[5];
			if (_kSpriteGroupArray != null)
			{
				int num = Mathf.Min(_kSpriteGroupArray.Length, 5);
				for (int i = 0; i < num; i++)
				{
					array[i] = _kSpriteGroupArray[i];
				}
				for (int j = num; j < 5; j++)
				{
					array[j] = new KSpriteGroup();
				}
			}
			else
			{
				for (int k = 0; k < 5; k++)
				{
					array[k] = new KSpriteGroup();
				}
			}
			_kSpriteGroupArray = array;
		}
		FillCollectionMap();
	}

	private void FillCollectionMap()
	{
		_collectionMap.Clear();
		int i = 0;
		for (int count = _nameToPathList.Count; i < count; i++)
		{
			NameToPath nameToPath = _nameToPathList[i];
			SpriteCollectionInfo spriteCollectionInfo = FindSpriteCollectionInfo(nameToPath.Path);
			if (spriteCollectionInfo != null)
			{
				_collectionMap[nameToPath.Name] = spriteCollectionInfo;
			}
		}
	}

	private SpriteCollectionInfo FindSpriteCollectionInfo(string path)
	{
		for (int i = 0; i < 5; i++)
		{
			List<SpriteCollectionInfo> spriteCollectionInfoList = _kSpriteGroupArray[i].SpriteCollectionInfoList;
			for (int j = 0; j < spriteCollectionInfoList.Count; j++)
			{
				if (spriteCollectionInfoList[j].SpriteCollectionPath == path)
				{
					return spriteCollectionInfoList[j];
				}
			}
		}
		return null;
	}

	private void LoadSpriteCollectionInfo(SpriteCollectionInfo info)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
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

	public SpriteCollectionInfo GetSpriteCollectionInfo(string spriteName)
	{
		SpriteCollectionInfo spriteCollectionInfo = _collectionMap.Get(spriteName);
		if (spriteCollectionInfo == null)
		{
			return null;
		}
		if (spriteCollectionInfo.LoadStatus == SpriteCollectionInfo.Status.NotLoaded)
		{
			LoadSpriteCollectionInfo(spriteCollectionInfo);
		}
		return spriteCollectionInfo;
	}

	public KSpriteGroup GetKSpriteGroup(SpriteObjectType type)
	{
		return (type != SpriteObjectType.Unspecified) ? _kSpriteGroupArray[(int)type] : null;
	}
}
