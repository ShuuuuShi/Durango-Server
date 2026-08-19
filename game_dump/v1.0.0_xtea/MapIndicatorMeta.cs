using System;
using System.Collections.Generic;
using MapData;
using Shared.Region;
using TerrainData;
using UnityEngine;

[ResourcePath("map_indicator_meta")]
public class MapIndicatorMeta : ResourceSingleton<MapIndicatorMeta>
{
	[Serializable]
	[EnumType(typeof(Shared.Region.Biome))]
	public class BiomeColorList : EnumKeyList
	{
		[SerializeField]
		private List<Color> _values;

		public Color Get(Shared.Region.Biome type)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			int num = IndexOf((int)type);
			return (num != -1) ? _values[num] : Color.clear;
		}
	}

	[Serializable]
	[EnumType(typeof(AnnounceType))]
	public class AnnounceBalloonMetaList : EnumKeyList
	{
		[SerializeField]
		private List<AnnounceBalloonMeta> _values;

		public bool TryGet(AnnounceType type, out AnnounceBalloonMeta meta)
		{
			int num = IndexOf((int)type);
			meta = ((num != -1) ? _values[num] : default(AnnounceBalloonMeta));
			return num != -1;
		}
	}

	[SerializeField]
	private BiomeColorList _biomeColors;

	[SerializeField]
	private AnnounceBalloonMetaList _announceBalloonMetaList;

	[SerializeField]
	private Color _landmarkColor;

	[SerializeField]
	private Color _scoopColor;

	public static Color LandMakrColor => ResourceSingleton<MapIndicatorMeta>.Instance()._landmarkColor;

	public static Color ScoopColor => ResourceSingleton<MapIndicatorMeta>.Instance()._scoopColor;

	public static Color GetBiomeColor(TerrainData.Biome biome)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return ResourceSingleton<MapIndicatorMeta>.Instance()._biomeColors.Get((Shared.Region.Biome)biome);
	}

	public static bool TryGetAnnounceBalloonMeta(AnnounceType type, out AnnounceBalloonMeta meta)
	{
		return ResourceSingleton<MapIndicatorMeta>.Instance()._announceBalloonMetaList.TryGet(type, out meta);
	}
}
