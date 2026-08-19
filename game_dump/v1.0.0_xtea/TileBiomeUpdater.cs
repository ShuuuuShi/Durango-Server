using System;
using TerrainData;
using UnityEngine;

internal class TileBiomeUpdater : MonoBehaviour
{
	[SerializeField]
	public float IntervalTime = 0.5f;

	private float _lastCheckTime;

	private Biome _curTileBiome = Biome.Unspecified;

	public bool Running { get; set; }

	public event Action<Biome> BiomeUpdated;

	private void Update()
	{
		if (TerrainA6.IsPlayerInitialized && !(Time.time - _lastCheckTime < IntervalTime) && Running)
		{
			_lastCheckTime = Time.time;
			Biome curTileBiome = _curTileBiome;
			Biome tileBiome = GetTileBiome();
			if (tileBiome != Biome.Unspecified)
			{
				_curTileBiome = tileBiome;
			}
			bool flag = curTileBiome != tileBiome;
			if (this.BiomeUpdated != null && flag)
			{
				this.BiomeUpdated(_curTileBiome);
			}
		}
	}

	public Biome GetTileBiome()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPos = TerrainA6.ClientPositionToWorldPosition(((Component)this).gameObject.transform.position);
		return TerrainA6.GetTileBiome(worldPos);
	}
}
