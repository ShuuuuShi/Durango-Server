using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Durango.Model;
using Durango.Render.Camera;
using Durango.Render.Effect;
using Durango.Render.Particle;
using Durango.Utils;
using JetBrains.Annotations;
using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;

public class AnimationEventController : MonoBehaviour
{
	private struct LocatedPropInfo
	{
		public string Path;

		public GameObject PropObject;

		public bool AutoRemoveAtMotionChanged;
	}

	[CompilerGenerated]
	private sealed class _003CCoModifyPlaybackRatio_003Ed__54 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public AnimationEventController _003C_003E4__this;

		public float destRatio;

		private float _003CbeginTime_003E5__2;

		private float _003CendTime_003E5__3;

		private float _003Cfrom_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoModifyPlaybackRatio_003Ed__54(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			AnimationEventController animationEventController = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CbeginTime_003E5__2 = Time.realtimeSinceStartup;
				_003CendTime_003E5__3 = Time.realtimeSinceStartup + duration;
				_003Cfrom_003E5__4 = Time.timeScale;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.realtimeSinceStartup < _003CendTime_003E5__3)
			{
				float t = Time.realtimeSinceStartup - _003CbeginTime_003E5__2;
				Time.timeScale = animationEventController.EaseOutQuad(t, _003Cfrom_003E5__4, destRatio, duration);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			Time.timeScale = destRatio;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public static AnimationClipInfo InvalidAnimationClipInfo = new AnimationClipInfo
	{
		Name = null,
		State = null
	};

	public static readonly string NoBoneName = "_None_";

	[SerializeField]
	public AnimationEventResource AnimationEventResource;

	[SerializeField]
	public AnimationEventResource AnimationEventResourceShared;

	private Dictionary<string, List<AnimationEventInfo>> _animationEvents;

	private IAnimationEventPlayable _targetPlayable;

	private float _lastProcessTime = -1f;

	private string _prevAnimKey;

	private float _prevAnimTime;

	private readonly List<AnimationEventInfo> _reservedFinallyEvents = new List<AnimationEventInfo>();

	private readonly List<int> _removeEndedParticleIds = new List<int>();

	private readonly List<KeyValuePair<uint, float>> _removeEndedSoundIds = new List<KeyValuePair<uint, float>>();

	private readonly List<AnimationEventInfo> _onceExcutedEvents = new List<AnimationEventInfo>();

	private readonly Dictionary<string, int> _activeTrails = new Dictionary<string, int>();

	private readonly List<LocatedPropInfo> _attachedProps = new List<LocatedPropInfo>();

	public Dictionary<string, List<AnimationEventInfo>> AnimationEvents
	{
		get
		{
			if (_animationEvents == null)
			{
				Load();
			}
			return _animationEvents;
		}
	}

	public event Action AnimEventMotionChanged;

	private void OnDestroy()
	{
		UnlocateAllProps();
		StopRemoveEndedEvents();
	}

	private void Update()
	{
		float time = Time.time;
		if (time - _lastProcessTime >= 0.033f)
		{
			ProcessAnimationEvent();
			_lastProcessTime = time;
		}
	}

	public void Load()
	{
		_animationEvents = AnimationEventContainer.LoadAnimationEvent(AnimationEventResource, AnimationEventResourceShared);
	}

	public void Reload()
	{
		AnimationEventContainer.Remove(AnimationEventResource, AnimationEventResourceShared);
		_animationEvents = AnimationEventContainer.LoadAnimationEvent(AnimationEventResource, AnimationEventResourceShared);
	}

	public void ForceApply(string motionName, List<AnimationEventInfo> animationEvents)
	{
		if (string.IsNullOrEmpty(motionName) || _animationEvents == null || !_animationEvents.ContainsKey(motionName))
		{
			return;
		}
		_animationEvents[motionName].Clear();
		foreach (AnimationEventInfo animationEvent in animationEvents)
		{
			AnimationEventInfo animationEventInfo = animationEvent.CopySerializable();
			animationEventInfo.enumID = animationEvent.enumID;
			_animationEvents[motionName].Add(animationEventInfo);
		}
	}

	private void ProcessAnimationEvent()
	{
		if (AnimationEvents == null)
		{
			return;
		}
		if (_targetPlayable == null)
		{
			_targetPlayable = base.gameObject.GetComponent<IAnimationEventPlayable>();
		}
		if (_targetPlayable == null || _targetPlayable.AnimationEventProhibited)
		{
			return;
		}
		AnimationClipInfo currentAnimationClipInfo = _targetPlayable.GetCurrentAnimationClipInfo();
		if (currentAnimationClipInfo.Name == null)
		{
			return;
		}
		float num = _prevAnimTime;
		float time = currentAnimationClipInfo.Time;
		if (_prevAnimKey == currentAnimationClipInfo.Name)
		{
			if (num > time)
			{
				if (currentAnimationClipInfo.IsLoop)
				{
					EmitAnimEvents(currentAnimationClipInfo.Name, num, currentAnimationClipInfo.Length);
					num = -0.1f;
				}
				else
				{
					OnEndMotion(num);
					OnBeginMotion(currentAnimationClipInfo.Name);
					num = -0.1f;
				}
			}
			EmitAnimEvents(currentAnimationClipInfo.Name, num, time);
		}
		else
		{
			OnEndMotion(num);
			AutoUnLocatePropsAtMotionChanged();
			OnBeginMotion(currentAnimationClipInfo.Name);
			if (this.AnimEventMotionChanged != null)
			{
				this.AnimEventMotionChanged();
			}
			num = -0.1f;
			EmitAnimEvents(currentAnimationClipInfo.Name, num, time);
		}
		_prevAnimKey = currentAnimationClipInfo.Name;
		_prevAnimTime = currentAnimationClipInfo.Time;
	}

	private void OnBeginMotion(string motionName)
	{
		_onceExcutedEvents.Clear();
		if (!_animationEvents.TryGetValue(motionName, out var value))
		{
			return;
		}
		int count = value.Count;
		for (int i = 0; i < count; i++)
		{
			AnimationEventInfo animationEventInfo = value[i];
			if (animationEventInfo.isFinally)
			{
				_reservedFinallyEvents.Add(animationEventInfo);
			}
		}
	}

	private void OnEndMotion(float endTime)
	{
		int count = _reservedFinallyEvents.Count;
		for (int i = 0; i < count; i++)
		{
			AnimationEventInfo animationEventInfo = _reservedFinallyEvents[i];
			if (!animationEventInfo.removeEnded)
			{
				EmitSingleAnimEvent(animationEventInfo, cancelFinally: false, endTime - animationEventInfo.time);
			}
		}
		_reservedFinallyEvents.Clear();
		_onceExcutedEvents.Clear();
		StopRemoveEndedEvents();
	}

	private void StopRemoveEndedEvents()
	{
		foreach (int removeEndedParticleId in _removeEndedParticleIds)
		{
			ParticleManager.Stop(removeEndedParticleId);
		}
		_removeEndedParticleIds.Clear();
		foreach (KeyValuePair<uint, float> removeEndedSoundId in _removeEndedSoundIds)
		{
			SoundManager.StopEvent(removeEndedSoundId.Key, removeEndedSoundId.Value);
		}
		_removeEndedSoundIds.Clear();
	}

	private void EmitAnimEvents(string motionName, float fromTime, float toTime)
	{
		if (fromTime > toTime)
		{
			float num = fromTime;
			fromTime = toTime;
			toTime = num;
		}
		if (!_animationEvents.TryGetValue(motionName, out var value))
		{
			return;
		}
		int count = value.Count;
		for (int i = 0; i < count; i++)
		{
			AnimationEventInfo animationEventInfo = value[i];
			float eventTime = animationEventInfo.GetEventTime();
			if (eventTime >= 0f && fromTime < eventTime && eventTime <= toTime)
			{
				EmitSingleAnimEvent(animationEventInfo, cancelFinally: true, toTime - eventTime);
			}
			else if (eventTime > toTime)
			{
				break;
			}
		}
	}

	private void EmitSingleAnimEvent(AnimationEventInfo animEventInfo, bool cancelFinally, float timePassed)
	{
		if (animEventInfo.isOnce)
		{
			if (_onceExcutedEvents.FindIndex((AnimationEventInfo info) => info.Equals(animEventInfo)) != -1)
			{
				return;
			}
			_onceExcutedEvents.Add(animEventInfo);
		}
		switch (animEventInfo.animEventCmd)
		{
		case AnimEventCmd.LegacyParticle:
			EmitParticle(animEventInfo, animEventInfo.gameObjectPath);
			break;
		case AnimEventCmd.NewParticle:
			EmitParticleNew(animEventInfo, animEventInfo.gameObjectPath);
			break;
		case AnimEventCmd.Sound:
			EmitSound(animEventInfo);
			break;
		case AnimEventCmd.Vibrate:
			EmitVibrate(animEventInfo);
			break;
		case AnimEventCmd.CustomCmd:
			EmitCustomCmd(animEventInfo);
			break;
		case AnimEventCmd.TrailOn:
			TurnOnTrail(animEventInfo, timePassed);
			break;
		case AnimEventCmd.TrailOff:
			TurnOffTrail(animEventInfo);
			break;
		case AnimEventCmd.WeaponVisible:
			ReEquipCurrentWeapon();
			break;
		case AnimEventCmd.WeaponInvisible:
			animEventInfo.paramStr = string.Empty;
			SetMotionEquip(animEventInfo);
			break;
		case AnimEventCmd.SceneTimeScale:
			ScenePlaybackRatio(animEventInfo);
			break;
		case AnimEventCmd.CameraShake:
			CameraShake(animEventInfo);
			break;
		case AnimEventCmd.CameraZoom:
			CameraZoom(animEventInfo);
			break;
		case AnimEventCmd.ChangeEquip:
			SetMotionEquip(animEventInfo);
			break;
		case AnimEventCmd.LocateProp:
			LocateProp(animEventInfo);
			break;
		case AnimEventCmd.UnlocateProp:
			UnLocateProp(animEventInfo);
			break;
		case AnimEventCmd.LandingEffect:
			EmitLandingEffect(animEventInfo);
			break;
		case AnimEventCmd.IntegratedEffect:
			EmitIntegratedEffect(animEventInfo);
			break;
		case AnimEventCmd.SoundEvent:
			EmitSoundEvent(animEventInfo);
			break;
		case AnimEventCmd.SoundEventString:
			EmitSoundEventString(animEventInfo);
			break;
		}
		if (animEventInfo.isFinally && cancelFinally)
		{
			_reservedFinallyEvents.Remove(animEventInfo);
		}
	}

	public Vector3 GetEmitPosition(AnimationEventInfo.Position positionType, Vector3 position, Transform target, bool isTrail)
	{
		if (target == null)
		{
			return _targetPlayable.GetCurrentPosition();
		}
		Vector3 vector = ((positionType != AnimationEventInfo.Position.ToBoneLocal) ? base.gameObject.transform : target).TransformVector(position);
		if (isTrail)
		{
			return vector;
		}
		return target.position + vector;
	}

	public Transform FindTargetBone(string boneName)
	{
		Transform transform = null;
		if (boneName != NoBoneName)
		{
			transform = KUtility.FindTransformByName(_targetPlayable.GetGameObject(), boneName);
			_ = transform == null;
		}
		if (transform == null)
		{
			return base.transform;
		}
		return transform;
	}

	private bool ValidateParticle(string effectPath, bool localPlayerOnly)
	{
		if (string.IsNullOrEmpty(effectPath))
		{
			return false;
		}
		if (localPlayerOnly && !IsLocalPlayer())
		{
			return false;
		}
		return true;
	}

	private void EmitParticle(AnimationEventInfo animEventInfo, string effectPath)
	{
		if (ValidateParticle(effectPath, animEventInfo.IsLocalPlayerOnlyEffectSound))
		{
			Transform transform = ((!string.IsNullOrEmpty(animEventInfo.BoneName)) ? FindTargetBone(animEventInfo.BoneName) : null);
			Vector3 emitPosition = GetEmitPosition(animEventInfo.PositionBasis, animEventInfo.EmitPosition, transform, isTrail: false);
			bool comeForwardToCamera = animEventInfo.PositionBasis == AnimationEventInfo.Position.ToCamera;
			bool groundDecal = animEventInfo.PositionBasis == AnimationEventInfo.Position.ToGround;
			Quaternion emitRotation = GetEmitRotation(animEventInfo.RotationBasis, animEventInfo.IsFollowingTarget, animEventInfo.EmitRotation, transform);
			Vector3 scale = ((!(animEventInfo.EmitUniformScale <= 0f)) ? (Vector3.one * animEventInfo.EmitUniformScale) : default(Vector3));
			int particleId = ((!animEventInfo.IsFollowingTarget || !(transform != null)) ? ParticleManager.Emit(effectPath, emitPosition, emitRotation, comeForwardToCamera, groundDecal, scale) : ParticleManager.EmitFollow(effectPath, emitPosition, emitRotation, transform, useLocalPosition: false, comeForwardToCamera, groundDecal, scale));
			OnParticleEmitted(particleId, animEventInfo.removeEnded);
		}
	}

	private void EmitParticleNew(AnimationEventInfo animEventInfo, string effectPath)
	{
		if (!ValidateParticle(effectPath, animEventInfo.IsLocalPlayerOnlyEffectSound))
		{
			return;
		}
		Transform transform = FindTargetBone(animEventInfo.BoneName);
		int particleId = 0;
		Vector3 vector = ((!(animEventInfo.EmitUniformScale <= 0f)) ? (Vector3.one * animEventInfo.EmitUniformScale) : default(Vector3));
		switch (animEventInfo.EmissionType)
		{
		case AnimationEventInfo.Emission.Default:
		case AnimationEventInfo.Emission.PulledTowardCamera:
		{
			Vector3 vector2 = transform.position + base.transform.TransformDirection(animEventInfo.EmitPosition);
			if (animEventInfo.AdjustHeightToGround)
			{
				vector2.y = 5f;
			}
			Quaternion emitRotation = GetEmitRotation(animEventInfo.RotationBasis, followTarget: false, animEventInfo.EmitRotation, transform);
			bool num = animEventInfo.EmissionType == AnimationEventInfo.Emission.PulledTowardCamera;
			Vector3 pos = vector2;
			Quaternion rotation3 = emitRotation;
			bool comeForwardToCamera = num;
			Vector3 scale3 = vector;
			particleId = ParticleManager.Emit(effectPath, pos, rotation3, comeForwardToCamera, groundDecal: false, scale3);
			break;
		}
		case AnimationEventInfo.Emission.Attach:
		{
			Vector3 emitPosition2 = animEventInfo.EmitPosition;
			Quaternion rotation2 = Quaternion.Euler(animEventInfo.EmitRotation);
			Transform followingParent2 = transform;
			Vector3 scale2 = vector;
			particleId = ParticleManager.EmitFollow(effectPath, emitPosition2, rotation2, followingParent2, useLocalPosition: true, comeForwardToCamera: false, groundDecal: false, scale2);
			break;
		}
		case AnimationEventInfo.Emission.ChasePosition:
		{
			Vector3 emitPosition = animEventInfo.EmitPosition;
			Quaternion rotation = Quaternion.Euler(animEventInfo.EmitRotation);
			Transform followingParent = base.transform;
			Vector3 scale = vector;
			Transform chasingTarget = transform;
			particleId = ParticleManager.EmitFollow(effectPath, emitPosition, rotation, followingParent, useLocalPosition: true, comeForwardToCamera: false, animEventInfo.AdjustHeightToGround, scale, chasingTarget);
			break;
		}
		}
		OnParticleEmitted(particleId, animEventInfo.removeEnded);
	}

	private void OnParticleEmitted(int particleId, bool removeEnded)
	{
		if (removeEnded && particleId != 0)
		{
			_removeEndedParticleIds.Add(particleId);
		}
	}

	public Quaternion GetEmitRotation(AnimationEventInfo.Rotation rotationBasis, bool followTarget, Vector3 rotationalVector, [CanBeNull] Transform targetBoneTransform)
	{
		Quaternion quaternion = Quaternion.identity;
		switch (rotationBasis)
		{
		case AnimationEventInfo.Rotation.FaceWithCharacter:
			if (!followTarget)
			{
				quaternion = base.gameObject.transform.rotation;
			}
			break;
		case AnimationEventInfo.Rotation.FaceToCamera:
		{
			Vector3 vector = ((!Application.isPlaying) ? Quaternion.identity.eulerAngles : Singleton<MainCamera>.Instance().transform.rotation.eulerAngles);
			quaternion = Quaternion.Euler(180f + vector.x, 180f + vector.y, vector.z);
			break;
		}
		case AnimationEventInfo.Rotation.FaceToBoneLocal:
			if (targetBoneTransform != null)
			{
				quaternion = targetBoneTransform.rotation;
			}
			break;
		}
		return Quaternion.Euler(rotationalVector) * quaternion;
	}

	private void EmitSound(AnimationEventInfo animEventInfo)
	{
		if (!string.IsNullOrEmpty(animEventInfo.gameObjectPath))
		{
			uint num = SoundManager.PlayEvent(Path.GetFileNameWithoutExtension(animEventInfo.gameObjectPath), (!animEventInfo.Use2dSound) ? SoundPosition.Fix(_targetPlayable.GetCurrentPosition()) : SoundPosition.Empty);
			if (animEventInfo.removeEnded && num != 0)
			{
				_removeEndedSoundIds.Add(new KeyValuePair<uint, float>(num, 0f));
			}
		}
	}

	private void EmitSoundEventString(AnimationEventInfo animEventInfo)
	{
		SoundManager.PlayEvent(animEventInfo.SoundEventName);
	}

	private void EmitSoundEvent(AnimationEventInfo animEventInfo)
	{
		PlayerBehavior playerBehavior = null;
		if (animEventInfo.IsLocalPlayerOnlyEffectSound)
		{
			playerBehavior = GetTargetPlayer();
			if (playerBehavior == null || !playerBehavior.IsLocalPlayer)
			{
				return;
			}
		}
		SoundSwitch soundSwitch = SoundSwitch.Empty;
		switch (animEventInfo.AnimSoundEventSwitch)
		{
		case AnimationEventInfo.SoundEventSwitch.VoiceType:
			soundSwitch = GetVoiceTypeSwitch(playerBehavior);
			break;
		case AnimationEventInfo.SoundEventSwitch.Footstep:
			soundSwitch = GetFootstepSwitch();
			break;
		}
		SoundPosition soundPosition = ((!animEventInfo.IsSoundChaseTarget) ? SoundPosition.Fix(_targetPlayable.GetCurrentPosition()) : SoundPosition.Chase(_targetPlayable.GetGameObject()));
		uint num = SoundManager.PlayEvent(animEventInfo.gameObjectPath, soundPosition, soundSwitch);
		if (animEventInfo.removeEnded && num != 0)
		{
			_removeEndedSoundIds.Add(new KeyValuePair<uint, float>(num, animEventInfo.FadeOutDuration));
		}
	}

	private PlayerBehavior GetTargetPlayer()
	{
		GameObject gameObject = _targetPlayable.GetGameObject();
		if (gameObject != null)
		{
			return gameObject.GetComponent<PlayerBehavior>();
		}
		return null;
	}

	private CharacterBehavior GetTargetCharacter()
	{
		GameObject gameObject = _targetPlayable.GetGameObject();
		if (gameObject != null)
		{
			return gameObject.GetComponent<CharacterBehavior>();
		}
		return null;
	}

	private SoundSwitch GetVoiceTypeSwitch(PlayerBehavior player)
	{
		if (player == null)
		{
			player = GetTargetPlayer();
		}
		if (player != null)
		{
			return player.VoiceSoundSwitch;
		}
		return SoundSwitch.Empty;
	}

	private SoundSwitch GetFootstepSwitch()
	{
		CharacterBehavior targetCharacter = GetTargetCharacter();
		if (targetCharacter != null)
		{
			return SoundEventMaterialSwitch.Get(targetCharacter.GetBiome(), targetCharacter.WaterDepthLevel);
		}
		return SoundSwitch.Empty;
	}

	private void EmitVibrate(AnimationEventInfo animEventInfo)
	{
		PlayerBehavior targetPlayer = GetTargetPlayer();
		bool flag = targetPlayer != null && targetPlayer.IsLocalPlayer;
		if (!animEventInfo.IsLocalPlayerOnlyEffectSound || flag)
		{
			float num = ((!flag) ? (PlayerBehavior.LocalPlayer.CurrentPosition - _targetPlayable.GetCurrentPosition()).magnitude : 0f);
			if (animEventInfo.VibrationRange <= 0f || animEventInfo.VibrationRange > num)
			{
				Vibration.Vibrate(animEventInfo.enumID);
			}
		}
	}

	private void EmitCustomCmd(AnimationEventInfo animEventInfo)
	{
		if (animEventInfo.CustomCmd != string.Empty)
		{
			_targetPlayable.GetGameObject().SendMessage(animEventInfo.CustomCmd, animEventInfo.CustomCmdParams);
		}
	}

	private void TurnOnTrail(AnimationEventInfo animEventInfo, float timePassed)
	{
		string gameObjectPath = animEventInfo.gameObjectPath;
		if (string.IsNullOrEmpty(gameObjectPath) || string.IsNullOrEmpty(animEventInfo.BoneName))
		{
			return;
		}
		Transform transform = FindTargetBone(animEventInfo.BoneName);
		Vector3 emitPosition = GetEmitPosition(animEventInfo.PositionBasis, animEventInfo.EmitPosition, transform, isTrail: true);
		int num = _activeTrails.Get(gameObjectPath, 0);
		if (num == 0)
		{
			Quaternion rotation = Quaternion.Euler(animEventInfo.EmitRotation);
			num = ParticleManager.EmitFollow(gameObjectPath, emitPosition, rotation, transform, useLocalPosition: true, comeForwardToCamera: false, groundDecal: false, default(Vector3), null, reusable: false);
			_activeTrails[gameObjectPath] = num;
		}
		Singleton<ParticleManager>.Instance().RegisterAction(num, delegate(GameObject trail)
		{
			if (!(this == null) && trail != null)
			{
				OnTrailEmitted(animEventInfo, trail, timePassed);
			}
		});
	}

	private void OnTrailEmitted(AnimationEventInfo animEventInfo, [NotNull] GameObject trailObject, float timePassed)
	{
		if (!animEventInfo.TrailTipOverrideRequired && animEventInfo.TrailTipBoneName == string.Empty)
		{
			return;
		}
		PlaneTrail component = trailObject.GetComponent<PlaneTrail>();
		if (component == null)
		{
			return;
		}
		if (base.gameObject == null)
		{
			component.Emit = false;
			return;
		}
		CharacterBehavior targetCharacter = GetTargetCharacter();
		if (animEventInfo.TrailTipOverrideRequired)
		{
			if (targetCharacter != null)
			{
				component.TipTransform = targetCharacter.WeaponTipTransform;
			}
		}
		else if (animEventInfo.TrailTipBoneName != string.Empty)
		{
			component.TipTransform = KUtility.FindTransformByName(base.gameObject, animEventInfo.TrailTipBoneName);
		}
		if (component.TipTransform == null)
		{
			component.TipTransform = base.gameObject.transform;
		}
		SetTrailOption(animEventInfo, timePassed, component, targetCharacter);
	}

	private void SetTrailOption(AnimationEventInfo animEventInfo, float timePassed, PlaneTrail planeTrail, CharacterBehavior character)
	{
		planeTrail.Option = animEventInfo.TrailFixOption;
		if (planeTrail.Option == PlaneTrail.FixOption.Baked)
		{
			TrailBaker.TrailData trailData = animEventInfo.TrailData;
			if (trailData == null || KUtility.GetSize(trailData.BasePoints) == 0)
			{
				TrailBaker.TrailData trailData3 = (animEventInfo.TrailData = Json.Read<TrailBaker.TrailData>(animEventInfo.BakedTrailData));
				trailData = trailData3;
			}
			if (trailData == null || KUtility.GetSize(trailData.BasePoints) == 0)
			{
				planeTrail.Option = PlaneTrail.FixOption.None;
			}
			else
			{
				if (!(character != null))
				{
					planeTrail.Emit = false;
					return;
				}
				planeTrail.SetBaked(trailData, character, animEventInfo.TrailPushBase, timePassed);
			}
		}
		planeTrail.Emit = true;
	}

	private void TurnOffTrail(AnimationEventInfo animEventInfo)
	{
		if (string.IsNullOrEmpty(animEventInfo.gameObjectPath))
		{
			foreach (string key in _activeTrails.Keys)
			{
				TurnOffTrail(key);
			}
			return;
		}
		TurnOffTrail(animEventInfo.gameObjectPath);
	}

	private void TurnOffTrail(string key)
	{
		_activeTrails.TryGetValue(key, out var value);
		if (value == 0)
		{
			return;
		}
		Singleton<ParticleManager>.Instance().RegisterAction(value, delegate(GameObject go)
		{
			if (!(this == null) && go != null)
			{
				TrailRenderer_Base component = go.GetComponent<TrailRenderer_Base>();
				if ((bool)component)
				{
					component.Emit = false;
				}
			}
		});
	}

	private void ScenePlaybackRatio(AnimationEventInfo animEventInfo)
	{
		float sceneTimeScaleRatio = animEventInfo.SceneTimeScaleRatio;
		float sceneTimeScaleDuration = animEventInfo.SceneTimeScaleDuration;
		StartCoroutine(CoModifyPlaybackRatio(sceneTimeScaleRatio, sceneTimeScaleDuration));
	}

	private IEnumerator CoModifyPlaybackRatio(float destRatio, float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoModifyPlaybackRatio_003Ed__54(0)
		{
			_003C_003E4__this = this,
			destRatio = destRatio,
			duration = duration
		};
	}

	private float EaseOutQuad(float t, float from, float to, float duration)
	{
		t /= duration;
		return (0f - to) * t * (t - 2f) + from;
	}

	private bool IsLocalPlayer()
	{
		return PlayerBehavior.LocalPlayer.gameObject == _targetPlayable.GetGameObject();
	}

	private void CameraShake(AnimationEventInfo animEventInfo)
	{
		if (animEventInfo.IsLocalPlayerOnlyCameraEffect && !IsLocalPlayer())
		{
			return;
		}
		float magnitude = (PlayerBehavior.LocalPlayer.CurrentPosition - _targetPlayable.GetCurrentPosition()).magnitude;
		float num = animEventInfo.CameraShakeAmplitudeU;
		float num2 = animEventInfo.CameraShakeAmplitudeV;
		float cameraShakeInterval = animEventInfo.CameraShakeInterval;
		if (magnitude > 0f)
		{
			float x = animEventInfo.paramVector2.x;
			float y = animEventInfo.paramVector2.y;
			if (!(Math.Abs(y - x) < float.Epsilon))
			{
				float num3 = Mathf.Clamp01(1f - (magnitude - x) / (y - x));
				num *= num3;
				num2 *= num3;
			}
		}
		Singleton<CameraShaker>.Instance().Shake(num, num2, cameraShakeInterval);
	}

	private void CameraZoom(AnimationEventInfo animEventInfo)
	{
		if (!animEventInfo.IsLocalPlayerOnlyCameraEffect || IsLocalPlayer())
		{
			float cameraZoomRatio = animEventInfo.CameraZoomRatio;
			float cameraZoomTime = animEventInfo.CameraZoomTime;
			float x = animEventInfo.paramVector2.x;
			float y = animEventInfo.paramVector2.y;
			Singleton<CameraController>.Instance().ZoomRatio(Mathf.Sqrt(cameraZoomRatio), cameraZoomTime, NgInterpolate.EaseType.EaseInCirc).Delay(x)
				.ZoomRatio(1f, y, NgInterpolate.EaseType.EaseOutCirc);
		}
	}

	private void SetMotionEquip(AnimationEventInfo animEventInfo)
	{
		PlayerBehavior component = _targetPlayable.GetGameObject().GetComponent<PlayerBehavior>();
		if ((bool)component)
		{
			component.ChangeEquipmentWhileCurrentAnimation(animEventInfo.PrefabPath);
		}
		CostumeActorBehavior component2 = _targetPlayable.GetGameObject().GetComponent<CostumeActorBehavior>();
		if ((bool)component2)
		{
			component2.ChangeEquipment(animEventInfo.PrefabPath);
		}
	}

	private void ReEquipCurrentWeapon()
	{
		PlayerBehavior component = _targetPlayable.GetGameObject().GetComponent<PlayerBehavior>();
		if ((bool)component)
		{
			component.ReEquipCurrentWeapon();
		}
	}

	private void LocateProp(AnimationEventInfo animEventInfo)
	{
		if (string.IsNullOrEmpty(animEventInfo.gameObjectPath))
		{
			return;
		}
		string path = animEventInfo.gameObjectPath;
		Singleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!((GameObject)asset == null) && !(this == null))
			{
				_targetPlayable.GetGameObject().GetComponentInChildren<Animation>().Sample();
				Transform transform = _targetPlayable.GetGameObject().transform;
				if (!string.IsNullOrEmpty(animEventInfo.BoneName) && !(animEventInfo.BoneName == NoBoneName))
				{
					Transform transform2 = KUtility.FindTransformByName(_targetPlayable.GetGameObject(), animEventInfo.BoneName);
					if (!(transform2 == null))
					{
						transform = transform2;
					}
				}
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(asset);
				gameObject.transform.position = Vector3.zero;
				gameObject.transform.rotation = Quaternion.identity;
				Transform transform3 = KUtility.FindTransformByName(gameObject, animEventInfo.BoneName);
				if (transform3 != null)
				{
					Matrix4x4 localToWorldMatrix = transform.transform.localToWorldMatrix;
					Matrix4x4 m = Matrix4x4.TRS(transform3.transform.localPosition, transform3.transform.localRotation, transform3.transform.localScale);
					Maths.DecomposeMatrix(localToWorldMatrix * Matrix4x4.Inverse(m), out var position, out var rotation, out var _);
					gameObject.transform.rotation = rotation;
					gameObject.transform.position = position;
				}
				else
				{
					gameObject.transform.rotation = transform.rotation;
					gameObject.transform.position = transform.position;
				}
				if (animEventInfo.EmitPosition != Vector3.zero)
				{
					gameObject.transform.Translate(animEventInfo.EmitPosition);
				}
				if (animEventInfo.paramVector2 != Vector3.zero)
				{
					gameObject.transform.Rotate(animEventInfo.paramVector2);
				}
				_attachedProps.Add(new LocatedPropInfo
				{
					Path = path,
					PropObject = gameObject,
					AutoRemoveAtMotionChanged = animEventInfo.UseAutoUnLocateProp
				});
			}
		});
	}

	private void UnLocateProp(AnimationEventInfo animEventInfo)
	{
		if (string.IsNullOrEmpty(animEventInfo.gameObjectPath))
		{
			return;
		}
		string gameObjectPath = animEventInfo.gameObjectPath;
		for (int num = _attachedProps.Count - 1; num >= 0; num--)
		{
			if (_attachedProps[num].Path == gameObjectPath)
			{
				UnityEngine.Object.Destroy(_attachedProps[num].PropObject);
				_attachedProps.RemoveAt(num);
			}
		}
	}

	private void AutoUnLocatePropsAtMotionChanged()
	{
		for (int num = _attachedProps.Count - 1; num >= 0; num--)
		{
			if (_attachedProps[num].AutoRemoveAtMotionChanged)
			{
				UnityEngine.Object.Destroy(_attachedProps[num].PropObject);
				_attachedProps.RemoveAt(num);
			}
		}
	}

	private void UnlocateAllProps()
	{
		for (int num = _attachedProps.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(_attachedProps[num].PropObject);
			_attachedProps.RemoveAt(num);
		}
	}

	private void EmitLandingEffect(AnimationEventInfo animEventInfo)
	{
		LandingEffectManager landingEffectManager = Singleton<LandingEffectManager>.Instance();
		if (_targetPlayable.GetGameObject() == null)
		{
			return;
		}
		CharacterBehavior targetCharacter = GetTargetCharacter();
		if (!(targetCharacter == null))
		{
			EffectSet effectSet = landingEffectManager.GetEffectSet(targetCharacter.GetBiome(), animEventInfo.AnimLandingEffectSize);
			if (effectSet != null)
			{
				EmitParticle(animEventInfo, effectSet.Particle.Path);
				SoundManager.PlayEvent(effectSet.Sound, SoundPosition.Fix(targetCharacter.CurrentPosition));
			}
		}
	}

	private void EmitIntegratedEffect(AnimationEventInfo animEventInfo)
	{
		if (!ValidateParticle(animEventInfo.gameObjectPath, animEventInfo.IsLocalPlayerOnlyEffectSound))
		{
			return;
		}
		CharacterBehavior targetCharacter = GetTargetCharacter();
		if (!(targetCharacter == null))
		{
			IntegratedEffect.RequestProperEffectSet(animEventInfo.gameObjectPath, targetCharacter.GetBiome(), targetCharacter.WaterDepthLevel, delegate(EffectSet dataSet)
			{
				EmitParticleNew(animEventInfo, dataSet.Particle);
				SoundManager.PlayEvent(dataSet.Sound, SoundPosition.Fix(_targetPlayable.GetCurrentPosition()));
			});
		}
	}
}
