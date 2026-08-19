using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Terrain;
using Durango.Utils;
using Durango.Utils.Extensions;
using Shared.Region;
using UnityEngine;

public class AmbientSoundManager : Singleton<AmbientSoundManager>
{
	[Serializable]
	private class AmbientSound
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public SoundEventType SoundEvent;
	}

	[Serializable]
	private class WaterSound
	{
		[SerializeField]
		public string OceanType;

		[SerializeField]
		public SoundEventType OceanSound;

		[SerializeField]
		public SoundEventType FlowSound;

		[SerializeField]
		public Biome FlowBiome;
	}

	[Serializable]
	private class AmbientSoundSet
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public AmbientSound[] AmbientSounds;
	}

	private const string _defaultSoundSetName = "_defaultSoundSet";

	[SerializeField]
	[EnumList(typeof(Biome), true, 0, -1)]
	private SoundEventType[] _ambientSoundEvents;

	[SerializeField]
	private WaterSound[] _waterSounds;

	[SerializeField]
	private AmbientSoundSet[] _forOverrideTileSet;

	[SerializeField]
	private float _fadeOutDuration = 2f;

	[SerializeField]
	private float _checkBiomePeriod = 2f;

	private Biome _currentBiome;

	private SoundEventType _currentAmbientSound;

	private Dictionary<string, SoundEventType[]> _ambientSoundSets = new Dictionary<string, SoundEventType[]>();

	private SoundEventType _oceanSoundEvent;

	private SoundEventType _flowSoundEvent;

	private Biome _flowBiome;

	private int _currentHour = -1;

	private float _timeToCheckBiome;

	private uint _ambientSoundId;

	private uint _flowSoundId;

	private void Update()
	{
		int currentHour = GetCurrentHour();
		if (_currentHour != currentHour)
		{
			_currentHour = currentHour;
			SetAmbientParameter(_currentHour);
		}
		if (_timeToCheckBiome <= Time.time)
		{
			RefreshAmbientSound();
			_timeToCheckBiome = Time.time + _checkBiomePeriod;
		}
	}

	public void SetBiome(Biome biome)
	{
		_currentBiome = biome;
	}

	public void SetFlowAudio(Vector3 clientPos)
	{
		Vector3 worldPos = Util.ClientPositionToWorldPosition(clientPos);
		float f = MinSquaredDistance(15, worldPos, _flowBiome);
		f = Mathf.Sqrt(f);
		if (f < 3000f)
		{
			PlayFlowSound(f);
		}
		else
		{
			StopFlowSound();
		}
	}

	protected override void OnAwake()
	{
		for (int i = 0; i < _forOverrideTileSet.Length; i++)
		{
			SoundEventType[] array = CreateBiomeSoundSet();
			AmbientSoundSet ambientSoundSet = _forOverrideTileSet[i];
			for (int j = 0; j < ambientSoundSet.AmbientSounds.Length; j++)
			{
				AmbientSound ambientSound = ambientSoundSet.AmbientSounds[j];
				if (ambientSound.Name.TryEnum<Biome>(out var value, showError: true))
				{
					ref SoundEventType reference = ref array[(int)value];
					reference = ambientSound.SoundEvent;
				}
			}
			_ambientSoundSets[ambientSoundSet.Name] = array;
		}
		SoundEventType[] array2 = CreateBiomeSoundSet();
		for (int k = 0; k < _ambientSoundEvents.Length; k++)
		{
			ref SoundEventType reference2 = ref array2[k];
			reference2 = _ambientSoundEvents[k];
		}
		_ambientSoundSets["_defaultSoundSet"] = array2;
		WaterSound waterSound = _waterSounds.FirstOrDefault((WaterSound ws) => ws.OceanType == TerrainMeta.OceanType);
		if (waterSound != null)
		{
			_oceanSoundEvent = waterSound.OceanSound;
			_flowSoundEvent = waterSound.FlowSound;
			_flowBiome = waterSound.FlowBiome;
		}
	}

	private static SoundEventType[] CreateBiomeSoundSet()
	{
		return new SoundEventType[(int)(Enums<Biome>.Max() + 1)];
	}

	private void RefreshAmbientSound()
	{
		SoundEventType ambientSoundEvent = GetAmbientSoundEvent(_currentBiome);
		if (!string.IsNullOrEmpty(ambientSoundEvent.Path) && (string)_currentAmbientSound != (string)ambientSoundEvent)
		{
			_currentAmbientSound = ambientSoundEvent;
			SoundManager.StopEvent(_ambientSoundId, _fadeOutDuration);
			_ambientSoundId = 0u;
			if (!string.IsNullOrEmpty(_currentAmbientSound.Path))
			{
				_ambientSoundId = SoundManager.PlayEvent(_currentAmbientSound, SoundPosition.Empty, exclusive: true);
			}
		}
	}

	private SoundEventType GetAmbientSoundEvent(Biome biome)
	{
		if (biome == Biome.WarmOcean || biome == Biome.ColdOcean)
		{
			return _oceanSoundEvent;
		}
		if (!string.IsNullOrEmpty(TerrainMeta.TileSet) && TryGetAmbientSoundEvent(TerrainMeta.TileSet, biome, out var soundEvent))
		{
			return soundEvent;
		}
		if (TryGetAmbientSoundEvent("_defaultSoundSet", biome, out soundEvent))
		{
			return soundEvent;
		}
		return default(SoundEventType);
	}

	private bool TryGetAmbientSoundEvent(string tileSet, Biome biome, out SoundEventType soundEvent)
	{
		if (_ambientSoundSets.TryGetValue(tileSet, out var value) && Biome.TemperateForest <= biome && (int)biome < value.Length)
		{
			soundEvent = value[(int)biome];
			return !string.IsNullOrEmpty(soundEvent.Path);
		}
		soundEvent = default(SoundEventType);
		return false;
	}

	private void PlayFlowSound(float distance)
	{
		if (_flowSoundId == 0)
		{
			_flowSoundId = SoundManager.PlayEvent(_flowSoundEvent, SoundPosition.Empty, exclusive: true);
		}
		SoundManager.SetRTPC(new SoundParameters("amb_river_distance", distance));
	}

	private void StopFlowSound()
	{
		if (_flowSoundId != 0)
		{
			SoundManager.StopEvent(_flowSoundId);
			_flowSoundId = 0u;
		}
	}

	private static int GetCurrentHour()
	{
		return (int)(TimeGauge.GetNormalizedTime() * 24f);
	}

	private static void SetAmbientParameter(float value)
	{
		SoundManager.SetRTPC(new SoundParameters("time_od_day", value));
	}

	private static float MinSquaredDistance(int radiusInTile, Vector3 worldPos, Biome currentBiome)
	{
		float num = float.MaxValue;
		for (int i = -radiusInTile; i <= radiusInTile; i++)
		{
			for (int j = -radiusInTile; j <= radiusInTile; j++)
			{
				Vector3 vector = worldPos + new Vector3(j * 200, 0f, i * 200);
				float sqrMagnitude = (worldPos - vector).sqrMagnitude;
				if (sqrMagnitude >= num)
				{
					continue;
				}
				TerrainChunkBase chunkFromWorldPosition = Singleton<TerrainBase>.Instance().GetChunkFromWorldPosition(vector);
				if (chunkFromWorldPosition == null || !chunkFromWorldPosition.HasRiver)
				{
					Vector3 vector2 = Util.ChunkCoordsToWorldPosition(Util.WorldPositionToChunkCoords(vector));
					int num2 = (int)((vector - vector2).x / 200f);
					int num3 = 16 - num2 - 1;
					j += num3;
				}
				else if (chunkFromWorldPosition.GetTileBiome(vector) == currentBiome)
				{
					num = sqrMagnitude;
					if (num <= 40000f)
					{
						return num;
					}
				}
			}
		}
		return num;
	}
}
