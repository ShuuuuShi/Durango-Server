using System;
using System.Collections.Generic;
using Durango.Terrain;
using Durango.Utils;
using Shared.Region;
using UnityEngine;

namespace Durango.Environment;

public class AmbientLightingManager : Singleton<AmbientLightingManager>
{
	[Serializable]
	public class ColorSet
	{
		public string Name;

		public Color[] AmbientColors;

		public Color SkyAmbient;
	}

	private static readonly int SkyAmbientId = Shader.PropertyToID("_SkyAmbient");

	[SerializeField]
	public ColorSet DefaultColorSet;

	[SerializeField]
	public List<ColorSet> OverrideColorSets;

	public float TransitionTime = 1f;

	private readonly Color _noneColor = new Color(0f, 0f, 0f, 0f);

	private ColorSet _currentColorSet;

	private void Start()
	{
		ApplyTileSet();
		Singleton<GameManager>.Instance().PostReconnect += ApplyTileSet;
	}

	private void ApplyTileSet()
	{
		_currentColorSet = DefaultColorSet;
		for (int i = 0; i < OverrideColorSets.Count; i++)
		{
			if (OverrideColorSets[i].Name == TerrainMeta.TileSet)
			{
				ReplaceColorSet(OverrideColorSets[i]);
				break;
			}
		}
	}

	private void ReplaceColorSet(ColorSet colorset)
	{
		int i = 0;
		for (int num = (int)(Enums<Biome>.Max() + 1); i < num; i++)
		{
			Color color = colorset.AmbientColors[i];
			if (color != _noneColor)
			{
				_currentColorSet.AmbientColors[i] = color;
			}
		}
	}

	public Color GetAmbientColor(Biome biome)
	{
		if (biome < Biome.TemperateForest || (int)biome >= _currentColorSet.AmbientColors.Length)
		{
			return _noneColor;
		}
		return _currentColorSet.AmbientColors[(int)biome];
	}

	private void Update()
	{
		Shader.SetGlobalColor(SkyAmbientId, _currentColorSet.SkyAmbient);
	}
}
