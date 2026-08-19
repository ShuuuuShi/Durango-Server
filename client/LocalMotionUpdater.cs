using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.MotionInfo;
using Durango.Network;
using Durango.Player.Animation;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using Shared.Region;
using UnityEngine;

public class LocalMotionUpdater
{
	public enum StandStateEnum
	{
		None,
		Stand,
		BattleStand,
		Hiding,
		Cold,
		Hot
	}

	public enum AnimRefreshStatus
	{
		None,
		Refresh,
		ForceRefresh
	}

	public enum RideMotionState
	{
		None,
		Riding,
		Mount,
		DisMount
	}

	public struct ReservedMotion
	{
		public string Motion;

		public float PlaybackRate;

		public float PlayUntil;

		public string Equip;

		public bool ForceTransition;

		public ItemColor EquipColor;
	}

	private StandStateEnum _standState;

	private bool _isWaterCarried;

	private bool _isWaterResist;

	private bool _isSleep;

	private float _playbackRate = 1f;

	private readonly Stack<ReservedMotion> _reservedMotions = new Stack<ReservedMotion>();

	private string _exitTransitionState;

	private string _exitTransitionClip;

	private float _motionRefreshTime;

	private AnimRefreshStatus _motionRefreshStatus;

	private string _currentMotionState = "Stand";

	private string _currentMotionClip = "Barehand_Stand";

	private string _currentEquip;

	private ItemColor _currentEquipColor;

	private bool _motionUpdated;

	private RideMotionState _rideState;

	private bool _movedByManual;

	public PlayerAnimationClipInfo CurrentClipInfo { get; private set; }

	public float PlaybackRate => _playbackRate;

	public Observable<bool> IsBattleStand { get; private set; }

	private static PlayerBehavior Player => PlayerBehavior.LocalPlayer;

	private static PlayerAnimationClipManager AnimManager => Singleton<PlayerAnimationClipManager>.Instance();

	public bool IsInWater => (TerrainWater.WaterDepthLevel)Player.WaterDepthLevel >= TerrainWater.WaterDepthLevel.Waist;

	public bool IsSwimming => (TerrainWater.WaterDepthLevel)Player.WaterDepthLevel >= TerrainWater.WaterDepthLevel.Swim;

	public bool IsWaterCarried
	{
		get
		{
			return _isWaterCarried;
		}
		set
		{
			if (IsWaterResist)
			{
				value = false;
			}
			if (_isWaterCarried != value)
			{
				_isWaterCarried = value;
				ConditionChanged();
			}
		}
	}

	public bool IsWaterResist
	{
		get
		{
			return _isWaterResist;
		}
		private set
		{
			_isWaterResist = value;
			if (_isWaterResist)
			{
				IsWaterCarried = false;
			}
		}
	}

	public bool IsSleep
	{
		get
		{
			return _isSleep;
		}
		set
		{
			if (_isSleep == value)
			{
				return;
			}
			_isSleep = value;
			if (_isSleep)
			{
				if (IsState("Stand"))
				{
					Motion("Rest");
				}
			}
			else
			{
				ConditionChanged();
			}
		}
	}

	public bool IsNovice => GameManager.IsPrologueMode;

	public StandStateEnum StandState
	{
		get
		{
			return _standState;
		}
		private set
		{
			if (_standState != value)
			{
				_standState = value;
				ConditionChanged();
			}
		}
	}

	public RideMotionSet RideMotionSet { get; private set; }

	public LocalMotionUpdater()
	{
		IsBattleStand = new Observable<bool>();
		Observable<bool> isBattleStand = IsBattleStand;
		isBattleStand.Changed = (Action<bool>)Delegate.Combine(isBattleStand.Changed, (Action<bool>)delegate
		{
			UpdateStandState();
		});
		Observable<string> fatigueEffect = GameSystem<FatigueSystem>.Instance().FatigueEffect;
		fatigueEffect.Changed = (Action<string>)Delegate.Combine(fatigueEffect.Changed, (Action<string>)delegate
		{
			UpdateStandState();
		});
		RideMotionSet = MotionMap.Instance().GetRideMotion(string.Empty);
	}

	public void ForceUpdate()
	{
		_motionUpdated = true;
	}

	public void UpdateRideMotionSet(string rideMotionSetName)
	{
		RideMotionSet rideMotion = MotionMap.Instance().GetRideMotion(rideMotionSetName);
		if (rideMotion != null)
		{
			RideMotionSet = MotionMap.Instance().GetRideMotion(rideMotionSetName);
		}
	}

	public void ConditionChanged()
	{
		if (!string.IsNullOrEmpty(_currentMotionState) && _motionRefreshStatus == AnimRefreshStatus.None)
		{
			_motionRefreshStatus = AnimRefreshStatus.Refresh;
		}
	}

	public void Motion(string motion, float time = 0f, float playbackRate = 1f, bool forceTransition = false, bool overrideIdleMotion = false, string equip = null, ItemColor color = default(ItemColor))
	{
		if (!string.IsNullOrEmpty(motion))
		{
			while (_reservedMotions.Count > 0 && _reservedMotions.Peek().Motion == motion)
			{
				_reservedMotions.Pop();
			}
			ReservedMotion t = default(ReservedMotion);
			t.Motion = motion;
			t.PlaybackRate = playbackRate;
			if (overrideIdleMotion)
			{
				t.PlayUntil = -1f;
			}
			else
			{
				t.PlayUntil = ((!(time <= 0f)) ? (Time.time + time) : 0f);
			}
			t.ForceTransition = forceTransition;
			t.Equip = equip;
			t.EquipColor = color;
			_reservedMotions.Push(t);
			_motionRefreshStatus = ((!forceTransition) ? AnimRefreshStatus.Refresh : AnimRefreshStatus.ForceRefresh);
		}
	}

	private bool CheckReservedMotions()
	{
		while (_reservedMotions.Count > 0)
		{
			ReservedMotion reservedMotion = _reservedMotions.Peek();
			if (reservedMotion.PlayUntil > 0f && reservedMotion.PlayUntil <= Time.time)
			{
				_reservedMotions.Pop();
				continue;
			}
			if (reservedMotion.PlayUntil < 0f && !Player.IsAnimPlaying)
			{
				_reservedMotions.Pop();
				continue;
			}
			break;
		}
		if (_reservedMotions.Count == 0)
		{
			return false;
		}
		ReservedMotion reservedMotion2 = _reservedMotions.Peek();
		if (Mathf.Approximately(reservedMotion2.PlayUntil, 0f))
		{
			_reservedMotions.Pop();
		}
		return TrySetMotionByState(reservedMotion2.Motion, reservedMotion2.PlaybackRate, reservedMotion2.ForceTransition, reservedMotion2.Equip, reservedMotion2.EquipColor) || TrySetMotionByStateClip(reservedMotion2.Motion, string.Empty, reservedMotion2.PlaybackRate, reservedMotion2.ForceTransition, reservedMotion2.Equip, reservedMotion2.EquipColor);
	}

	private bool TrySetMotionByState(string state, float playbackRate, bool forceTransition, string equip = null, ItemColor color = default(ItemColor))
	{
		PlayerAnimationStateInfo playerAnimationStateInfo = AnimManager.GetPlayerAnimationStateInfo(state);
		if (playerAnimationStateInfo == null)
		{
			return false;
		}
		PlayerAnimationConditionArguments stateConditionArguments = GetStateConditionArguments();
		PlayerAnimationStateClip playerAnimationStateClip = playerAnimationStateInfo.Get(stateConditionArguments);
		if (playerAnimationStateClip == null)
		{
			return false;
		}
		return TrySetMotionByStateClip(playerAnimationStateClip.Clip, playerAnimationStateInfo.State, playbackRate, forceTransition, equip, color);
	}

	private PlayerAnimationConditionArguments GetStateConditionArguments()
	{
		PlayerAnimationConditionArguments result = default(PlayerAnimationConditionArguments);
		result.Framework = (int)Player.CurrentWeaponFramework;
		result.StandState = (int)StandState;
		result.MoveSpeed = (int)Singleton<PlayerController>.Instance().GetCurrentMoveSpeed();
		result.IsInWater = IsInWater;
		result.IsMoving = _movedByManual;
		result.IsSwimming = IsSwimming;
		result.IsWaterCarried = IsWaterCarried;
		result.IsBushWhack = Player.IsBushWhacking;
		result.IsTired = Player.IsTired;
		result.IsRoadRunning = Player.IsRoadRunning;
		result.IsSleep = IsSleep;
		result.IsNovice = IsNovice;
		return result;
	}

	public bool IsPlayableMotion([NotNull] PlayerAnimationClipInfo clipInfo, bool forceTransition = false)
	{
		if (clipInfo.HasAnimTag(PlayerAnimationClipTag.Riding) && !Player.IsRiding)
		{
			_motionRefreshStatus = AnimRefreshStatus.None;
			return false;
		}
		if (!clipInfo.HasAnimTag(PlayerAnimationClipTag.Dead) && !Player.IsAlive)
		{
			_motionRefreshStatus = AnimRefreshStatus.None;
			return false;
		}
		if (!forceTransition && CurrentClipInfo != null)
		{
			int tagLevel = AnimManager.GetTagLevel(CurrentClipInfo);
			int num = ((CurrentClipInfo == null) ? tagLevel : AnimManager.GetTagLevel(clipInfo));
			if ((CurrentClipInfo.HasAnimTag(PlayerAnimationClipTag.Irrevocable) || tagLevel > num) && (!Player.IsAlive || Player.Anim.isPlaying))
			{
				_motionRefreshStatus = AnimRefreshStatus.None;
				return false;
			}
		}
		return true;
	}

	private bool TrySetMotionByStateClip([NotNull] string clip, string state, float playbackRate, bool forceTransition, string equip = null, ItemColor color = default(ItemColor))
	{
		if (Singleton<PlayerController>.Instance().IsProhibitAnimRefresh && !forceTransition)
		{
			_motionRefreshStatus = AnimRefreshStatus.None;
			return true;
		}
		PlayerAnimationClipInfo playerAnimationClipInfo = AnimManager.GetPlayerAnimationClipInfo(clip);
		if (playerAnimationClipInfo == null)
		{
			if (!string.IsNullOrEmpty(clip))
			{
			}
			return false;
		}
		if (CurrentClipInfo != playerAnimationClipInfo)
		{
			if (playerAnimationClipInfo.HasAnimTag(PlayerAnimationClipTag.Run) != _movedByManual)
			{
				if (_movedByManual)
				{
				}
				_motionRefreshStatus = AnimRefreshStatus.None;
				return false;
			}
			if (!IsPlayableMotion(playerAnimationClipInfo, forceTransition))
			{
				return true;
			}
			PlayerAnimationStateClip playerAnimationStateClipInfo = AnimManager.GetPlayerAnimationStateClipInfo(clip, state);
			if (playerAnimationStateClipInfo != null)
			{
				PlayerAnimationClipTrasitionInfo transitionCondition = PlayerAnimationClipManager.GetTransitionCondition(playerAnimationStateClipInfo.Transitions, TransitionCondition.OnFinished);
				if (transitionCondition != null)
				{
					_exitTransitionState = transitionCondition.State;
					_exitTransitionClip = transitionCondition.Clip;
				}
				else
				{
					_exitTransitionState = null;
					_exitTransitionClip = null;
				}
			}
			SetMotionRefreshTime(CurrentClipInfo, playerAnimationClipInfo, playbackRate);
			_currentMotionState = state;
			_currentMotionClip = clip;
			CurrentClipInfo = playerAnimationClipInfo;
			_currentEquip = equip;
			_currentEquipColor = color;
			IsWaterResist = CurrentClipInfo.HasAnimTag(PlayerAnimationClipTag.WaterFlowResist);
			_motionUpdated = true;
		}
		if (_playbackRate != playbackRate)
		{
			_playbackRate = playbackRate;
			_motionUpdated = true;
		}
		_motionRefreshStatus = AnimRefreshStatus.None;
		return true;
	}

	private void SetMotionRefreshTime(PlayerAnimationClipInfo curClipInfo, PlayerAnimationClipInfo targetClipInfo, float playbackRate)
	{
		float num = ((!(targetClipInfo.FadeOutTime < 0f)) ? targetClipInfo.FadeOutTime : 0.1f);
		float num2 = targetClipInfo.Length;
		if (playbackRate > 0f)
		{
			num /= playbackRate;
			num2 /= playbackRate;
		}
		if (targetClipInfo.IsLoop)
		{
			_motionRefreshTime = 0f;
		}
		else if (string.IsNullOrEmpty(_exitTransitionState) && string.IsNullOrEmpty(_exitTransitionClip))
		{
			if (_motionRefreshTime > Time.time && curClipInfo.Clip == targetClipInfo.Clip)
			{
				if (_motionRefreshStatus == AnimRefreshStatus.ForceRefresh)
				{
					_motionRefreshTime = Time.time + num2 - num;
				}
				else
				{
					_motionRefreshTime = Mathf.Min(Time.time + num2 - num, _motionRefreshTime);
				}
			}
			else
			{
				_motionRefreshTime = Time.time + num2 - num;
			}
		}
		else
		{
			_motionRefreshTime = Time.time + num2 - num;
		}
	}

	public void Mount()
	{
		if (_rideState != RideMotionState.Riding && _rideState != RideMotionState.Mount)
		{
			_rideState = RideMotionState.Mount;
		}
	}

	public void DisMount(bool immediately)
	{
		if (_rideState == RideMotionState.Riding || _rideState == RideMotionState.Mount)
		{
			_rideState = ((!immediately) ? RideMotionState.DisMount : RideMotionState.None);
		}
	}

	public float GetDisMountMotionLength()
	{
		return AnimManager.GetPlayerAnimationClipInfo(RideMotionSet.DisMount)?.Length ?? 1f;
	}

	public void UpdateMovingCondition(bool movedByManual)
	{
		if (movedByManual != _movedByManual)
		{
			_motionRefreshStatus = AnimRefreshStatus.Refresh;
			_movedByManual = movedByManual;
			if (_movedByManual)
			{
				ClearReservedMotions();
			}
		}
	}

	public bool GetCurrentMotionClip(out string currentMotionClip)
	{
		UpdateCurrentMotionClip();
		bool motionUpdated = _motionUpdated;
		_motionUpdated = false;
		currentMotionClip = _currentMotionClip;
		return motionUpdated;
	}

	private void UpdateCurrentMotionClip()
	{
		if (Player.IsAlive && Player.IsRiding && _rideState != 0)
		{
			UpdateRidingMotion(_movedByManual);
		}
		else
		{
			if (_motionRefreshStatus == AnimRefreshStatus.None || CheckReservedMotions())
			{
				return;
			}
			string state = _currentMotionState;
			float playbackRate = _playbackRate;
			if (Player.Life != null && Player.IsAlive)
			{
				if (_movedByManual)
				{
					if (IsInWater)
					{
						Biome biome = Player.GetBiome();
						state = ((biome != Biome.Lava) ? "Water_Swim" : "Lava_Walk");
					}
					else
					{
						state = "Run";
					}
					playbackRate = 1f;
				}
				else if (IsState("Lava_Walk") || IsState("Water_Swim") || IsState("Run") || _motionRefreshStatus >= AnimRefreshStatus.Refresh)
				{
					state = ((!IsSwimming && !IsWaterCarried) ? "Stand" : "Water_Idle");
					playbackRate = 1f;
				}
			}
			bool forceTransition = _motionRefreshStatus == AnimRefreshStatus.ForceRefresh;
			TrySetMotionByState(state, playbackRate, forceTransition);
		}
	}

	private void UpdateRidingMotion(bool move)
	{
		float num = 1f;
		string text = _rideState switch
		{
			RideMotionState.Mount => RideMotionSet.Mount, 
			RideMotionState.DisMount => RideMotionSet.DisMount, 
			_ => (!move) ? RideMotionSet.StandMount : RideMotionSet.RunMount, 
		};
		PlayerAnimationClipInfo playerAnimationClipInfo = AnimManager.GetPlayerAnimationClipInfo(text);
		if (playerAnimationClipInfo == null)
		{
			return;
		}
		if (_rideState == RideMotionState.Riding)
		{
			float vehicleMotionLength = Player.Driver.GetVehicleMotionLength();
			if (vehicleMotionLength > 0f)
			{
				num = playerAnimationClipInfo.Length / vehicleMotionLength;
			}
		}
		if (text != _currentMotionClip || _playbackRate != num)
		{
			TrySetMotionByStateClip(playerAnimationClipInfo.Clip, "Ride", num, forceTransition: true);
		}
	}

	public void RefreshMotion(string targetMotion = null, bool force = false, bool clearReservations = false)
	{
		if (string.IsNullOrEmpty(targetMotion) || _currentMotionState == targetMotion || _currentMotionClip == targetMotion)
		{
			_motionRefreshStatus = ((!force) ? AnimRefreshStatus.Refresh : AnimRefreshStatus.ForceRefresh);
			if (clearReservations)
			{
				ClearReservedMotions();
			}
		}
	}

	public void ClearReservedMotions()
	{
		_reservedMotions.Clear();
	}

	public MotionOption GetCurrentMotionOption(bool yawSnap)
	{
		MotionOption motionOption = MotionOption.ALIGN_TO_PATH | MotionOption.PC_CLIENTSIDE_MOTION;
		if (yawSnap)
		{
			motionOption |= MotionOption.SNAP_ANGLE_BEGIN;
		}
		return motionOption;
	}

	public void ReserveMotionEquipment()
	{
		if (_currentEquip != null)
		{
			Player.ReserveMotionEquipment(_currentEquip, _currentEquipColor);
		}
	}

	private void UpdateStandState()
	{
		StandStateEnum standState = (IsBattleStand ? StandStateEnum.BattleStand : ((string)GameSystem<FatigueSystem>.Instance().FatigueEffect switch
		{
			"hot" => StandStateEnum.Hot, 
			"cold" => StandStateEnum.Cold, 
			_ => StandStateEnum.Stand, 
		}));
		StandState = standState;
	}

	public void ProcessMotionTimer()
	{
		if (Singleton<PlayerController>.Instance().CutScenePlayMode || Singleton<PlayerController>.Instance().IsProhibitAnimRefresh)
		{
			return;
		}
		if (_reservedMotions.Count > 0)
		{
			ReservedMotion reservedMotion = _reservedMotions.Peek();
			if (reservedMotion.PlayUntil > 0f && reservedMotion.PlayUntil <= Time.time)
			{
				if (reservedMotion.Motion == _currentMotionState || reservedMotion.Motion == _currentMotionClip)
				{
					RefreshMotion(null, _motionRefreshStatus == AnimRefreshStatus.ForceRefresh);
				}
				_reservedMotions.Pop();
			}
		}
		if (!Player.IsAnimPlaying)
		{
			RefreshMotion(null, _motionRefreshStatus == AnimRefreshStatus.ForceRefresh);
		}
		CheckReserveAnimTimer();
	}

	public bool IsState(string state)
	{
		if (_currentMotionState == null)
		{
			return false;
		}
		return _currentMotionState == state;
	}

	private void CheckReserveAnimTimer()
	{
		if (!(_motionRefreshTime <= 0f) && !(_motionRefreshTime > Time.time))
		{
			_motionRefreshTime = 0f;
			if (_rideState == RideMotionState.Mount)
			{
				_rideState = RideMotionState.Riding;
			}
			if (_rideState == RideMotionState.DisMount)
			{
				_rideState = RideMotionState.None;
			}
			if (string.IsNullOrEmpty(_exitTransitionState) && string.IsNullOrEmpty(_exitTransitionClip))
			{
				RefreshMotion(null, Player.IsAlive);
			}
			else if (string.IsNullOrEmpty(_exitTransitionClip))
			{
				TrySetMotionByState(_exitTransitionState, 1f, forceTransition: true);
			}
			else
			{
				TrySetMotionByStateClip(_exitTransitionClip, _exitTransitionState, 1f, forceTransition: true);
			}
		}
	}
}
