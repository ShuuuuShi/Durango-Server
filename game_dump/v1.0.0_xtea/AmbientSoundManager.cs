using System;
using System.Collections.Generic;
using TerrainData;
using UnityEngine;

public class AmbientSoundManager : KSingleton<AmbientSoundManager>
{
	[Serializable]
	[EnumType(typeof(Biome))]
	private class AmbientSoundList : EnumKeyList
	{
		[SerializeField]
		private List<AudioClip> _values;

		public AudioClip Get(Biome biome)
		{
			int index = IndexOf((int)biome);
			return _values[index];
		}
	}

	[SerializeField]
	private GameObject _ambientTemplete;

	[SerializeField]
	private float _transitionTime = 10f;

	[SerializeField]
	private AmbientSoundList _ambientSounds;

	private float _startTransitionTime;

	private AudioSource _curAudioSource;

	private AudioSource _nextAudioSource;

	private AudioSource _riverAudioSource;

	private Biome _curBiome;

	private Biome _nextBiome = Biome.Unspecified;

	private void Start()
	{
		_curAudioSource = ((Component)this).gameObject.AddChild(_ambientTemplete).GetComponent<AudioSource>();
		_nextAudioSource = ((Component)this).gameObject.AddChild(_ambientTemplete).GetComponent<AudioSource>();
		((Behaviour)_nextAudioSource).enabled = false;
		_riverAudioSource = ((Component)this).gameObject.AddChild(_ambientTemplete).GetComponent<AudioSource>();
		_riverAudioSource.clip = GetAudioClip(Biome.River);
		((Behaviour)_riverAudioSource).enabled = false;
	}

	private void Update()
	{
		if (((Behaviour)_nextAudioSource).enabled)
		{
			float num = Time.realtimeSinceStartup - _startTransitionTime;
			if (num > _transitionTime)
			{
				AudioSource curAudioSource = _curAudioSource;
				_curAudioSource = _nextAudioSource;
				_nextAudioSource = curAudioSource;
				((Behaviour)_nextAudioSource).enabled = false;
				_nextAudioSource.volume = 0f;
				_curAudioSource.volume = 1f;
				_curBiome = _nextBiome;
				_nextBiome = Biome.Unspecified;
			}
			else
			{
				float num2 = num / _transitionTime;
				_curAudioSource.volume = (1f - num2) * (1f - num2);
				_nextAudioSource.volume = num2 * num2;
			}
		}
	}

	private Biome ToMajorBiome(Biome biome)
	{
		switch (biome)
		{
		case Biome.TemperateForest:
		case Biome.TropicalForest:
		case Biome.Desert:
		case Biome.Tundra:
		case Biome.SnowField:
		case Biome.Grassland:
			return biome;
		case Biome.Taiga:
			return _nextBiome switch
			{
				Biome.TemperateForest => Biome.TemperateForest, 
				Biome.Tundra => Biome.Tundra, 
				_ => _curBiome switch
				{
					Biome.TemperateForest => Biome.TemperateForest, 
					Biome.Tundra => Biome.Tundra, 
					_ => Biome.TemperateForest, 
				}, 
			};
		case Biome.Savanna:
			return _nextBiome switch
			{
				Biome.TropicalForest => Biome.TropicalForest, 
				Biome.Grassland => Biome.Grassland, 
				_ => _curBiome switch
				{
					Biome.TropicalForest => Biome.TropicalForest, 
					Biome.Grassland => Biome.Grassland, 
					_ => Biome.Grassland, 
				}, 
			};
		case Biome.ShrubDesert:
			return _nextBiome switch
			{
				Biome.TropicalForest => Biome.TropicalForest, 
				Biome.Desert => Biome.Desert, 
				_ => _curBiome switch
				{
					Biome.TropicalForest => Biome.TropicalForest, 
					Biome.Desert => Biome.Desert, 
					_ => Biome.Desert, 
				}, 
			};
		case Biome.PebbleBeach:
		case Biome.SandBeach:
		case Biome.ColdOcean:
		case Biome.WarmOcean:
			return Biome.SandBeach;
		case Biome.River:
		case Biome.Lake:
			return Biome.Unspecified;
		default:
			return Biome.Unspecified;
		}
	}

	public void SetAmbientForced(Biome biome)
	{
		biome = ToMajorBiome(biome);
		AudioClip audioClip = GetAudioClip(biome);
		if (!((Object)(object)audioClip == (Object)null))
		{
			_curAudioSource.clip = audioClip;
			_curBiome = biome;
			_nextBiome = Biome.Unspecified;
		}
	}

	public void SetBiome(Biome biome)
	{
		biome = ToMajorBiome(biome);
		if (biome == Biome.Unspecified)
		{
			return;
		}
		if (_curBiome == Biome.Unspecified)
		{
			_curAudioSource.volume = 0f;
			_curAudioSource.clip = GetAudioClip(biome);
			((Behaviour)_curAudioSource).enabled = true;
			_curAudioSource.Play();
			_curBiome = biome;
		}
		else
		{
			if (_nextBiome == biome)
			{
				return;
			}
			if (_nextBiome != Biome.Unspecified)
			{
				if (_curBiome == biome)
				{
					AudioSource curAudioSource = _curAudioSource;
					_curAudioSource = _nextAudioSource;
					_nextAudioSource = curAudioSource;
					Biome curBiome = _curBiome;
					_curBiome = _nextBiome;
					_nextBiome = curBiome;
					_startTransitionTime = Time.realtimeSinceStartup - _startTransitionTime + Time.realtimeSinceStartup - _transitionTime;
				}
			}
			else if (_curBiome != biome)
			{
				AudioClip audioClip = GetAudioClip(biome);
				if (!((Object)(object)audioClip == (Object)null))
				{
					_startTransitionTime = Time.realtimeSinceStartup;
					_nextAudioSource.volume = 0f;
					_nextAudioSource.clip = audioClip;
					((Behaviour)_nextAudioSource).enabled = true;
					_nextAudioSource.Play();
					_nextBiome = biome;
				}
			}
		}
	}

	public void SetRiverAudio(Vector3 clientPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = TerrainA6.ClientPositionToWorldPosition(clientPos);
		float num = float.MaxValue;
		for (int i = -15; i <= 15; i++)
		{
			for (int j = -15; j <= 15; j++)
			{
				Vector3 val2 = val + new Vector3((float)(j * 200), 0f, (float)(i * 200));
				Biome tileBiome = TerrainA6.GetTileBiome(val2);
				if (tileBiome == Biome.River)
				{
					Vector3 val3 = val - val2;
					float sqrMagnitude = ((Vector3)(ref val3)).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
					}
					if (num <= 40000f)
					{
						break;
					}
				}
			}
		}
		num = Mathf.Sqrt(num);
		if (num >= 3000f)
		{
			((Behaviour)_riverAudioSource).enabled = false;
			return;
		}
		num = Mathf.Max(0f, num - 200f);
		float num2 = 1f - num / 3000f;
		num2 *= num2;
		_riverAudioSource.volume = num2;
		((Behaviour)_riverAudioSource).enabled = true;
		if (!_riverAudioSource.isPlaying)
		{
			_riverAudioSource.Play();
		}
	}

	private AudioClip GetAudioClip(Biome biome)
	{
		return _ambientSounds.Get(biome);
	}
}
