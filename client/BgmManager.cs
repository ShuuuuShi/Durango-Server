using System;
using Durango.Logic.Estate;
using Durango.Logic.Explore;
using Durango.Terrain;
using Durango.Utils;
using Shared.Estate;
using Shared.Region;
using UnityEngine;

public class BgmManager : Singleton<BgmManager>
{
	private enum State
	{
		Ready,
		Region,
		Combat,
		ClanWarphole,
		AirBalloon
	}

	private enum CombatBgmType
	{
		Normal,
		Savage,
		Intense,
		ClanWar
	}

	[Serializable]
	private class BgmData
	{
		public SoundEventType Start;

		public SoundEventType End;

		public float PostDelay;
	}

	[Serializable]
	private class TemplateBgm
	{
		[SerializeField]
		public string Id;

		[SerializeField]
		public BgmData Bgm;
	}

	[Serializable]
	private class TileSetBgm
	{
		[SerializeField]
		public string TileSet;

		[SerializeField]
		public BgmData Bgm;
	}

	[EnumList(typeof(Role), true, 0, -1)]
	[SerializeField]
	private BgmData[] _regionBgm;

	[EnumList(typeof(CombatBgmType), true, 0, -1)]
	[SerializeField]
	private BgmData[] _combatBgm;

	[SerializeField]
	private TemplateBgm[] _templateBgm;

	[SerializeField]
	private TileSetBgm[] _tileSetBgm;

	[SerializeField]
	private BgmData _clanWarpholeBgm;

	[SerializeField]
	private BgmData _airBalloonBgm;

	[SerializeField]
	private float _preDelay = 15f;

	[SerializeField]
	private float _checkPlayingPeriod = 0.1f;

	[SerializeField]
	private float _bgmFadeOutDuration = 2f;

	[SerializeField]
	private int _intenseLevel = 60;

	[SerializeField]
	private float _clanWarpholeBgmMaintainTime = 60f;

	[SerializeField]
	private SoundEventType _cprSound;

	private State _currentState;

	private BgmData _currentBgm;

	private float _timeToCheckPlaying;

	private bool _paused;

	private bool _muted;

	private bool _combatMode;

	private float _readyFinishedAt;

	private float _clanWarpholeBgmFinishedAt;

	private uint _bgmInstanceId;

	private BgmData _currentRegionBgm;

	private SoundSwitch _currentRegionSoundSwitch;

	private bool _enterPlayerEstate;

	private bool _enterClanEstate;

	private bool _enterClanWarphole;

	public SoundEventType CprSound => _cprSound;

	private void Update()
	{
		if (_timeToCheckPlaying <= Time.time)
		{
			UpdateBgm();
			_timeToCheckPlaying = Time.time + _checkPlayingPeriod;
		}
	}

	public void SetMute(bool mute)
	{
		if (_muted != mute)
		{
			StopAndReadyBgm(forceStop: true);
			_muted = mute;
		}
	}

	public void SetPause(bool pause)
	{
		if (_paused != pause)
		{
			StopAndReadyBgm(forceStop: true);
			_paused = pause;
		}
	}

	public void SetSwitch(SoundSwitch soundSwitch)
	{
		SoundManager.SetSwitch(_bgmInstanceId, soundSwitch);
	}

	public void LandingAirBalloon()
	{
		if (_currentState == State.AirBalloon)
		{
			StopAndReadyBgm();
		}
	}

	protected override void OnAwake()
	{
		GetRegionBgm(_regionBgm, _templateBgm, _tileSetBgm, out _currentRegionBgm, out _currentRegionSoundSwitch);
		CombatSystem combatSystem = GameSystem<CombatSystem>.Instance();
		combatSystem.ChangedCombatMode += delegate
		{
			RefreshCombatBgmMode();
		};
		EstateSystem estateSystem = GameSystem<EstateSystem>.Instance();
		estateSystem.EstateGridUpdated += EstateSystem_EstateGridUpdated;
		estateSystem.CurrentEstateChanged += EstateSystem_CurrentEstateChanged;
		_readyFinishedAt = Time.time + _preDelay;
	}

	private void UpdateBgm()
	{
		if (_paused || _muted)
		{
			return;
		}
		if (_currentState == State.Ready)
		{
			if (_readyFinishedAt <= Time.time)
			{
				PlayBgm();
			}
		}
		else if (SoundManager.IsPlaying(_bgmInstanceId))
		{
			UpdateClanWarpholeBgm();
		}
		else
		{
			StopAndReadyBgm();
		}
	}

	private void PlayBgm()
	{
		StopAndReadyBgm(forceStop: true);
		SoundSwitch soundSwitch = SoundSwitch.Empty;
		if (_combatMode)
		{
			_currentState = State.Combat;
			_currentBgm = _combatBgm[(int)GetCombatBgmType(_intenseLevel)];
		}
		else if (PlayerBehavior.LocalPlayer.Driver.IsHovering)
		{
			_currentState = State.AirBalloon;
			_currentBgm = _airBalloonBgm;
		}
		else if (_enterClanWarphole || _clanWarpholeBgmFinishedAt > Time.time)
		{
			_currentState = State.ClanWarphole;
			_currentBgm = _clanWarpholeBgm;
		}
		else
		{
			_currentState = State.Region;
			_currentBgm = _currentRegionBgm;
			soundSwitch = _currentRegionSoundSwitch;
		}
		if (!string.IsNullOrEmpty(_currentBgm.Start))
		{
			_bgmInstanceId = SoundManager.PlayEvent(_currentBgm.Start, SoundPosition.Empty, soundSwitch, exclusive: true);
		}
	}

	private void StopAndReadyBgm(bool forceStop = false)
	{
		if (forceStop)
		{
			if (_bgmInstanceId != 0)
			{
			}
			SoundManager.StopEvent(_bgmInstanceId, _bgmFadeOutDuration);
			_readyFinishedAt = 0f;
		}
		else
		{
			if (_currentBgm != null && !string.IsNullOrEmpty(_currentBgm.End))
			{
				SoundManager.PlayEvent(_bgmInstanceId, _currentBgm.End);
			}
			else
			{
				if (_bgmInstanceId != 0)
				{
				}
				SoundManager.StopEvent(_bgmInstanceId, _bgmFadeOutDuration);
			}
			_readyFinishedAt = Time.time + _preDelay + ((_currentBgm == null) ? 0f : _currentBgm.PostDelay);
		}
		_bgmInstanceId = 0u;
		_currentState = State.Ready;
		_currentBgm = null;
	}

	private void UpdateClanWarpholeBgm()
	{
		if (_currentState == State.ClanWarphole && !_enterClanWarphole && _clanWarpholeBgmFinishedAt <= Time.time)
		{
			StopAndReadyBgm();
		}
	}

	private void RefreshClanWarpholeBgm()
	{
		if ((_currentState == State.Ready || _currentState == State.Region) && _enterClanWarphole)
		{
			_clanWarpholeBgmFinishedAt = Time.time + _clanWarpholeBgmMaintainTime;
			PlayBgm();
		}
	}

	private void RefreshCombatBgmMode()
	{
		CombatSystem combatSystem = GameSystem<CombatSystem>.Instance();
		bool combatMode = combatSystem.CombatMode;
		if (_combatMode != combatMode && !GameManager.Region.IsPvpIsland())
		{
			_combatMode = combatMode;
			if (_combatMode)
			{
				PlayBgm();
				SoundManager.SetState(new SoundStates("battle", "on"));
			}
			else
			{
				StopAndReadyBgm();
				SoundManager.SetState(new SoundStates("battle", "off"));
			}
		}
	}

	private void RefreshEstates(EstateInfo currentEstate)
	{
		OwnerType ownerType = currentEstate?.License.Type ?? OwnerType.Invalid;
		ChangePlayerEstateState(ownerType);
		ChangeClanEstateState(ownerType);
		ChangeClanWarpholeInWarState(ownerType, ClanWarpholeIsInWar(currentEstate));
	}

	private void ChangePlayerEstateState(OwnerType ownerType)
	{
		bool flag = ownerType == OwnerType.Player || ownerType == OwnerType.PersonalPlayer;
		if (_enterPlayerEstate != flag)
		{
			_enterPlayerEstate = flag;
			SoundManager.SetState(new SoundStates("bgm_private_land", (!flag) ? "out_side" : "in_side"));
		}
	}

	private void ChangeClanEstateState(OwnerType ownerType)
	{
		bool flag = ownerType == OwnerType.ClanEstate;
		if (_enterClanEstate != flag)
		{
			_enterClanEstate = flag;
			SoundManager.SetState(new SoundStates("bgm_village", (!flag) ? "out_side" : "in_side"));
		}
	}

	private void ChangeClanWarpholeInWarState(OwnerType ownerType, bool inWar)
	{
		bool flag = ownerType == OwnerType.ClanWarphole && inWar;
		if (_enterClanWarphole != flag)
		{
			_enterClanWarphole = flag;
			RefreshClanWarpholeBgm();
		}
	}

	private static void GetRegionBgm(BgmData[] regionBgm, TemplateBgm[] templateBgm, TileSetBgm[] tileSetBgm, out BgmData currentBgm, out SoundSwitch currentSoundSwitch)
	{
		for (int i = 0; i < templateBgm.Length; i++)
		{
			if (templateBgm[i].Id == GameManager.Region.TemplateId)
			{
				currentSoundSwitch = SoundSwitch.Empty;
				currentBgm = templateBgm[i].Bgm;
				return;
			}
		}
		for (int j = 0; j < tileSetBgm.Length; j++)
		{
			if (tileSetBgm[j].TileSet == TerrainMeta.TileSet)
			{
				currentSoundSwitch = SoundSwitch.Empty;
				currentBgm = tileSetBgm[j].Bgm;
				return;
			}
		}
		Region region = GameManager.Region;
		int num = Mathf.Clamp((int)region.Role(), 0, regionBgm.Length - 1);
		if (region.Role() == Role.Risky)
		{
			currentSoundSwitch = SoundSwitch.Set("map_level_for_bgm", (region.Level >= 30) ? "more_than_30" : "under_30");
		}
		else
		{
			currentSoundSwitch = SoundSwitch.Empty;
		}
		currentBgm = regionBgm[num];
	}

	private static CombatBgmType GetCombatBgmType(int intenseLevel)
	{
		bool flag = GameManager.Region.Role() == Role.Outpost;
		if (flag)
		{
			foreach (EstateInfo estate in GameSystem<EstateSystem>.Instance().GetEstates())
			{
				if (ClanWarpholeIsInWar(estate))
				{
					return CombatBgmType.ClanWar;
				}
			}
		}
		DamageableEntity damageableEntity = GameSystem<CombatSystem>.Instance().Target;
		if (damageableEntity != null && damageableEntity.GetLevel() >= intenseLevel)
		{
			return CombatBgmType.Intense;
		}
		return flag ? CombatBgmType.Savage : CombatBgmType.Normal;
	}

	private static bool ClanWarpholeIsInWar(EstateInfo estate)
	{
		return estate != null && estate.License.Type == OwnerType.ClanWarphole && !estate.License.IsProtected();
	}

	private void EstateSystem_EstateGridUpdated()
	{
		RefreshEstates(GameSystem<EstateSystem>.Instance().CurrentEstate);
	}

	private void EstateSystem_CurrentEstateChanged(EstateInfo currentEstate)
	{
		RefreshEstates(currentEstate);
	}
}
