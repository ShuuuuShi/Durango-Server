using System;
using System.Collections.Generic;
using TerrainData;
using UnityEngine;

[RequireComponent(typeof(PlayerBehavior))]
public class PlayerFootStepEffect : MonoBehaviour
{
	private struct FootStepInfo
	{
		public TerrainWater.WaterDepthLevel WaterDepth;

		public FootStepCondition[] Conditions;

		public int DefaultSound;

		public string DefaultParticle;

		public float DefaultPeriod;
	}

	private struct FootStepCondition
	{
		public Biome Biome;

		public int SoundIndex;

		public string Particle;

		public float Period;
	}

	private class FootStepJson
	{
		public float step_period;

		public Dictionary<TerrainWater.WaterDepthLevel, FootStepConditionJson> conditions;

		public Dictionary<string, string[]> sounds;

		public Dictionary<string, string> particles;

		public int particle_offset;
	}

	private class FootStepBiomeJson
	{
		public string sound;

		public string particle;

		public float period;
	}

	private class FootStepConditionJson
	{
		public Dictionary<Biome, FootStepBiomeJson> biomes;

		public string sound;

		public string particle;

		public float period;
	}

	private int _footStepParticleOffset;

	private float _footStepPeriod;

	private string[][] _footSounds;

	private FootStepInfo[] _footStepInfos;

	private int _waitFrame = -1;

	private PlayerBehavior _player;

	private float _footStepTimer;

	private int _currentFootSoundIndex;

	private string _currentFootStepParticle;

	private int _footStepIndex;

	private void Awake()
	{
		_player = ((Component)this).GetComponent<PlayerBehavior>();
		_currentFootSoundIndex = -1;
		FootStepJson footStepJson = KUtility.ParseJsonFile<FootStepJson>("player_footstep_effect");
		_footStepPeriod = footStepJson.step_period;
		string[] array = new string[footStepJson.sounds.Count];
		_footSounds = new string[footStepJson.sounds.Count][];
		int num = 0;
		foreach (KeyValuePair<string, string[]> sound in footStepJson.sounds)
		{
			array[num] = sound.Key;
			_footSounds[num] = sound.Value;
			int i = 0;
			for (int num2 = sound.Value.Length; i < num2; i++)
			{
				SoundManager.Cache(sound.Value[i]);
			}
			num++;
		}
		foreach (KeyValuePair<string, string> particle in footStepJson.particles)
		{
			ParticleManager.Cache(particle.Value);
		}
		_footStepParticleOffset = footStepJson.particle_offset;
		_footStepInfos = new FootStepInfo[footStepJson.conditions.Count];
		num = 0;
		foreach (KeyValuePair<TerrainWater.WaterDepthLevel, FootStepConditionJson> condition in footStepJson.conditions)
		{
			_footStepInfos[num].WaterDepth = condition.Key;
			_footStepInfos[num].DefaultSound = Array.IndexOf(array, condition.Value.sound);
			if (condition.Value.particle != null)
			{
				footStepJson.particles.TryGetValue(condition.Value.particle, out _footStepInfos[num].DefaultParticle);
			}
			_footStepInfos[num].DefaultPeriod = ((!(condition.Value.period > 0f)) ? footStepJson.step_period : condition.Value.period);
			if (condition.Value.biomes != null)
			{
				_footStepInfos[num].Conditions = new FootStepCondition[condition.Value.biomes.Count];
				int num3 = 0;
				foreach (KeyValuePair<Biome, FootStepBiomeJson> biome in condition.Value.biomes)
				{
					int soundIndex = Array.IndexOf(array, biome.Value.sound);
					string value = null;
					if (biome.Value.particle != null)
					{
						footStepJson.particles.TryGetValue(biome.Value.particle, out value);
					}
					ref FootStepCondition reference = ref _footStepInfos[num].Conditions[num3];
					reference = new FootStepCondition
					{
						Biome = biome.Key,
						SoundIndex = soundIndex,
						Particle = value,
						Period = ((!(biome.Value.period > 0f)) ? footStepJson.step_period : biome.Value.period)
					};
					num3++;
				}
			}
			num++;
		}
	}

	private void OnEnable()
	{
		_player.TileChanged += Player_TileChanged;
		_player.WaterDepthLevelChanged += Player_WaterDepthLevelChanged;
		_player.ChangeMoveState += Player_ChangeMoveState;
	}

	private void OnDisable()
	{
		_player.TileChanged -= Player_TileChanged;
		_player.WaterDepthLevelChanged -= Player_WaterDepthLevelChanged;
		_player.ChangeMoveState -= Player_ChangeMoveState;
	}

	private void Update()
	{
		if (_waitFrame < 0)
		{
			return;
		}
		if (_waitFrame > 0)
		{
			_waitFrame--;
		}
		else if (_currentFootSoundIndex >= 0 && _currentFootSoundIndex < _footSounds.Length)
		{
			_footStepTimer -= Time.deltaTime;
			if (_footStepTimer <= 0f)
			{
				PlayFootStepEffect();
				_footStepTimer = _footStepPeriod;
			}
		}
	}

	private void Player_TileChanged(Point2 prev, Point2 current)
	{
		UpdateFootStep();
	}

	private void Player_WaterDepthLevelChanged()
	{
		UpdateFootStep();
	}

	private void Player_ChangeMoveState(bool isMoving)
	{
		_waitFrame = (isMoving ? 1 : (-1));
		_footStepTimer = 0f;
	}

	private void UpdateFootStep()
	{
		int num = -1;
		int i = 0;
		for (int num2 = _footStepInfos.Length; i < num2; i++)
		{
			if (_footStepInfos[i].WaterDepth == _player.WaterDepthLevel)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			FootStepInfo footStepInfo = _footStepInfos[num];
			_currentFootSoundIndex = footStepInfo.DefaultSound;
			_currentFootStepParticle = footStepInfo.DefaultParticle;
			_footStepPeriod = footStepInfo.DefaultPeriod;
			if (footStepInfo.Conditions == null)
			{
				return;
			}
			Biome biome = _player.GetBiome();
			int num3 = footStepInfo.Conditions.Length;
			for (int j = 0; j < num3; j++)
			{
				if (footStepInfo.Conditions[j].Biome == biome)
				{
					_currentFootSoundIndex = _footStepInfos[num].Conditions[j].SoundIndex;
					_currentFootStepParticle = _footStepInfos[num].Conditions[j].Particle;
					_footStepPeriod = _footStepInfos[num].Conditions[j].Period;
					break;
				}
			}
		}
		else
		{
			_currentFootSoundIndex = -1;
			_currentFootStepParticle = null;
		}
	}

	private void PlayFootStepEffect()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		string[] array = _footSounds[_currentFootSoundIndex];
		int num = array.Length;
		_footStepIndex = (_footStepIndex + Random.Range(1, num)) % num;
		SoundManager.Play(_footSounds[_currentFootSoundIndex][_footStepIndex], _player.CurrentPosition);
		if (!string.IsNullOrEmpty(_currentFootStepParticle))
		{
			string currentFootStepParticle = _currentFootStepParticle;
			Vector3 currentPosition = _player.CurrentPosition;
			Vector3 moveDir = _player.MoveDir;
			ParticleManager.Emit(currentFootStepParticle, currentPosition + ((Vector3)(ref moveDir)).normalized * (float)_footStepParticleOffset, Quaternion.identity);
		}
	}
}
