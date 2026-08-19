using System;
using System.Collections.Generic;
using UnityEngine;

[ResourcePath("portrait_map")]
public class PortraitMap : ResourceSingleton<PortraitMap>
{
	[Serializable]
	private class PortraitMaterial
	{
		public string Key;

		public Material Material;
	}

	[Serializable]
	private class Portrait
	{
		public string Id;

		public string Material;

		public Rect Rect = new Rect(0f, 0f, 1f, 1f);
	}

	[SerializeField]
	private PortraitMaterial[] _materials;

	[SerializeField]
	private Portrait[] _portraits;

	private Dictionary<string, PortraitMaterial> _materialDict;

	private Dictionary<string, Portrait> _portraitDict;

	public static bool TryGet(string id, out Material mat, out Rect rect)
	{
		PortraitMap portraitMap = ResourceSingleton<PortraitMap>.Instance();
		Portrait portrait = portraitMap.GetPortrait(id);
		if (portrait != null && !string.IsNullOrEmpty(portrait.Material))
		{
			PortraitMaterial material = portraitMap.GetMaterial(portrait.Material);
			if (material != null && material.Material != null)
			{
				mat = material.Material;
				rect = portrait.Rect;
				return true;
			}
		}
		mat = null;
		rect = default(Rect);
		return false;
	}

	private Portrait GetPortrait(string id)
	{
		if (_portraitDict == null)
		{
			_portraitDict = new Dictionary<string, Portrait>();
			for (int i = 0; i < _portraits.Length; i++)
			{
				Portrait portrait = _portraits[i];
				_portraitDict[portrait.Id] = portrait;
			}
		}
		return _portraitDict.Get(id);
	}

	private PortraitMaterial GetMaterial(string id)
	{
		if (_materialDict == null)
		{
			_materialDict = new Dictionary<string, PortraitMaterial>();
			for (int i = 0; i < _materials.Length; i++)
			{
				PortraitMaterial portraitMaterial = _materials[i];
				_materialDict[portraitMaterial.Key] = portraitMaterial;
			}
		}
		return _materialDict.Get(id);
	}
}
