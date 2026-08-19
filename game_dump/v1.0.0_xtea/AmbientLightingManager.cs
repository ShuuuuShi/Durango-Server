using System;
using System.Collections.Generic;
using TerrainData;
using UnityEngine;

public class AmbientLightingManager : KSingleton<AmbientLightingManager>
{
	[Serializable]
	public class ColorSet
	{
		public string Name;

		public Color[] AmbientColors;

		public Color SkyAmbient;
	}

	[SerializeField]
	public ColorSet DefaultColorSet;

	[SerializeField]
	public List<ColorSet> OverrideColorSets;

	[SerializeField]
	public float TransitionTime = 10f;

	private readonly Color _noneColor = new Color(0f, 0f, 0f, 0f);

	private ColorSet _currentColorSet;

	private int _skyAmbient;

	private void Start()
	{
		_skyAmbient = Shader.PropertyToID("_SkyAmbient");
		ApplyTileSet();
		KSingleton<GameManager>.Instance().PostReconnect += ApplyTileSet;
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
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 15; i++)
		{
			Color val = colorset.AmbientColors[i];
			if (val != _noneColor)
			{
				_currentColorSet.AmbientColors[i] = val;
			}
		}
	}

	public Color GetAmbientColor(Biome biome)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return (biome != Biome.Unspecified) ? _currentColorSet.AmbientColors[(int)biome] : _noneColor;
	}

	private void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalColor(_skyAmbient, _currentColorSet.SkyAmbient);
	}
}
