using System.Collections.Generic;
using Durango.System;
using Durango.Utils;
using Durango.Utils.Extensions;
using UnityEngine;

[ResourcePath("sprite_manager")]
public class UISpriteManager : ResourceSingleton<UISpriteManager>
{
	public enum Status
	{
		None,
		Loading,
		Ready,
		Failed
	}

	public struct Item
	{
		public UIAtlas Atlas;

		public UISpriteData Data;
	}

	[SerializeField]
	private UIAtlas[] _defaultAtlases;

	[SerializeField]
	private GameObjectType[] _assetAtlases;

	private Dictionary<string, Item> _spriteDictionary;

	private readonly Dictionary<string, UIWidget> _presetSpriteDictionary = new Dictionary<string, UIWidget>();

	private int _loadingCount;

	private int _version;

	public Status LoadingStatus { get; private set; }

	private static bool IsUnnecessaryAtlas(UIPrefabMap.Type type, string atlasPath)
	{
		if (!Application.isPlaying)
		{
			return false;
		}
		return type != UIPrefabMap.Type.PC && atlasPath.ContainsIgnoreCase("_PC_Atlas.");
	}

	public void Load()
	{
		if (LoadingStatus == Status.Loading)
		{
			return;
		}
		LoadingStatus = Status.Loading;
		_loadingCount = KUtility.GetSize(_defaultAtlases) + KUtility.GetSize(_assetAtlases);
		_spriteDictionary = new Dictionary<string, Item>();
		// ENV-01: resources.assets corrupt -> SerializeField atlases null -> count 0
		// without this, LoadingStatus stays Loading forever (title stuck)
		if (_loadingCount <= 0)
		{
			LoadingStatus = Status.Ready;
			return;
		}
		int i = 0;
		for (int size = KUtility.GetSize(_defaultAtlases); i < size; i++)
		{
			LoadAtlas(_defaultAtlases[i]);
		}
		_version++;
		int requested = _version;
		int j = 0;
		for (int size2 = KUtility.GetSize(_assetAtlases); j < size2; j++)
		{
			GameObjectType entry = _assetAtlases[j];
			if (entry == null || string.IsNullOrEmpty(entry.Path) || IsUnnecessaryAtlas(Platform.Instance.UIType, entry.Path))
			{
				_loadingCount--;
				continue;
			}
			Singleton<AssetBundleManager>.Instance().RequestAsset(entry.Path, typeof(GameObject), delegate(Object asset)
			{
				if (requested == _version && LoadingStatus == Status.Loading)
				{
					GameObject gameObject = asset as GameObject;
					LoadAtlas((!(gameObject != null)) ? null : gameObject.GetComponent<UIAtlas>());
				}
			});
		}
		if (_loadingCount <= 0 && LoadingStatus != Status.Failed)
		{
			LoadingStatus = Status.Ready;
		}
	}

	private void LoadAtlas(UIAtlas atlas)
	{
		if (atlas != null)
		{
			List<UISpriteData> spriteList = atlas.spriteList;
			for (int i = 0; i < spriteList.Count; i++)
			{
				UISpriteData uISpriteData = spriteList[i];
				if (_spriteDictionary.ContainsKey(uISpriteData.name))
				{
				}
				_spriteDictionary[uISpriteData.name] = new Item
				{
					Atlas = atlas,
					Data = uISpriteData
				};
			}
		}
		else
		{
			LoadingStatus = Status.Failed;
		}
		_loadingCount--;
		if (_loadingCount <= 0 && LoadingStatus != Status.Failed)
		{
			LoadingStatus = Status.Ready;
		}
	}

	/// <summary>
	/// [แก้เอง] รายชื่อสไปรต์ทั้งหมดที่โหลดไว้ — ใช้ตอนประกอบ UI ใหม่จากโค้ด
	/// (ต้องรู้ว่ามีชิ้นส่วนอะไรให้หยิบ ถึงจะทำให้หน้าใหม่ดูเป็นเนื้อเดียวกับเกม)
	/// </summary>
	public System.Collections.Generic.IEnumerable<string> AllSpriteNames()
	{
		if (_spriteDictionary == null)
		{
			yield break;
		}
		foreach (System.Collections.Generic.KeyValuePair<string, Item> pair in _spriteDictionary)
		{
			yield return pair.Key;
		}
	}

	public bool TryGet(string sprite, out UIAtlas atlas, out UISpriteData spriteData)
	{
		if (_spriteDictionary == null)
		{
			Load();
		}
		if (sprite != null && _spriteDictionary != null && _spriteDictionary.TryGetValue(sprite, out var value))
		{
			atlas = value.Atlas;
			spriteData = value.Data;
			return true;
		}
		atlas = null;
		spriteData = null;
		return false;
	}

	public UISpriteData GetSprite(string sprite)
	{
		UIAtlas atlas;
		UISpriteData spriteData;
		return (!TryGet(sprite, out atlas, out spriteData)) ? null : spriteData;
	}

	public bool TryGetPreset(string key, out UIWidget result)
	{
		if (!_presetSpriteDictionary.TryGetValue(key, out result))
		{
			string path = $"PresetSprites/{key}";
			GameObject gameObject = Resources.Load<GameObject>(path);
			result = ((!(gameObject == null)) ? gameObject.GetComponent<UIWidget>() : null);
			_presetSpriteDictionary[key] = result;
		}
		return !object.ReferenceEquals(result, null);
	}
}
