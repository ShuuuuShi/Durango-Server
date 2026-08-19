using System;
using System.Collections;
using System.Collections.Generic;
using CameraEffects;
using EffectData;
using PigeonCoopToolkit.Effects.Trails;
using Player;
using UnityEngine;

public class AnimationEventController : MonoBehaviour
{
	private struct LocatedPropInfo
	{
		public string Path;

		public GameObject PropObject;

		public bool AutoRemoveAtMotionChanged;
	}

	public static AnimationClipInfo InvalidAnimationClipInfo = new AnimationClipInfo
	{
		Name = null,
		Clip = null
	};

	public static readonly string NoBoneName = "_None_";

	[SerializeField]
	public AnimationEventResource AnimationEventResource;

	[SerializeField]
	public AnimationEventResource AnimationEventResourceShared;

	private Dictionary<string, List<AnimationEventInfo>> _animationEvents;

	private IAnimationEventPlayable _targetPlayable;

	private float _lastProcessTime = -1f;

	private AnimationClipInfo _prevAnimInfo;

	private readonly List<AnimationEventInfo> _reservedFinallyEvents = new List<AnimationEventInfo>();

	private readonly List<AnimationEventInfo> _onceExcutedEvents = new List<AnimationEventInfo>();

	private readonly Dictionary<string, GameObject> _activeTrails = new Dictionary<string, GameObject>();

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
	}

	private void Update()
	{
		float time = Time.time;
		float num = time - _lastProcessTime;
		if (num >= 0.033f)
		{
			ProcessAnimationEvent();
			_lastProcessTime = time;
		}
	}

	public void Load()
	{
		_animationEvents = KSingleton<AnimationEventData>.Instance().LoadAnimationEvent(AnimationEventResource, AnimationEventResourceShared);
	}

	public void Reload()
	{
		KSingleton<AnimationEventData>.Instance().Remove(AnimationEventResource, AnimationEventResourceShared);
		_animationEvents = KSingleton<AnimationEventData>.Instance().LoadAnimationEvent(AnimationEventResource, AnimationEventResourceShared);
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

	private static IAnimationEventPlayable FindAnimEventPlayable(GameObject obj)
	{
		MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
		int num = components.Length;
		for (int i = 0; i < num; i++)
		{
			if (components[i] is IAnimationEventPlayable result)
			{
				return result;
			}
		}
		return null;
	}

	private void ProcessAnimationEvent()
	{
		if (AnimationEvents == null)
		{
			return;
		}
		if (_targetPlayable == null)
		{
			_targetPlayable = FindAnimEventPlayable(((Component)this).gameObject);
		}
		AnimationClipInfo currentAnimationClipInfo = _targetPlayable.GetCurrentAnimationClipInfo();
		if (currentAnimationClipInfo.Name == null)
		{
			return;
		}
		float num = _prevAnimInfo.OriginalTime;
		float originalTime = currentAnimationClipInfo.OriginalTime;
		if (_prevAnimInfo.Name == currentAnimationClipInfo.Name)
		{
			if (num > originalTime)
			{
				if (currentAnimationClipInfo.IsLoop)
				{
					EmitAnimEvents(currentAnimationClipInfo.Name, _prevAnimInfo.OriginalTime, _prevAnimInfo.Length);
					num = -0.1f;
				}
				else
				{
					OnEndMotion(num);
					OnBeginMotion(currentAnimationClipInfo.Name);
					num = -0.1f;
				}
			}
			EmitAnimEvents(currentAnimationClipInfo.Name, num, originalTime);
		}
		else
		{
			OnEndMotion(num);
			AutoUnLocatePropsAtMotionChanged();
			OnBeginMotion(currentAnimationClipInfo.Name);
			num = -0.1f;
			EmitAnimEvents(currentAnimationClipInfo.Name, num, originalTime);
			if (this.AnimEventMotionChanged != null)
			{
				this.AnimEventMotionChanged();
			}
		}
		_prevAnimInfo = currentAnimationClipInfo;
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
			EmitSingleAnimEvent(animationEventInfo, removeFinally: false, endTime - animationEventInfo.time);
		}
		_reservedFinallyEvents.Clear();
		_onceExcutedEvents.Clear();
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
				EmitSingleAnimEvent(animationEventInfo, removeFinally: true, toTime - eventTime);
			}
			else if (eventTime > toTime)
			{
				break;
			}
		}
	}

	private void EmitSingleAnimEvent(AnimationEventInfo animEventInfo, bool removeFinally, float timePassed)
	{
		if (animEventInfo.isOnce)
		{
			int num = _onceExcutedEvents.FindIndex((AnimationEventInfo info) => info.Equals(animEventInfo));
			if (num != -1)
			{
				return;
			}
			_onceExcutedEvents.Add(animEventInfo);
		}
		switch (animEventInfo.animEventCmd)
		{
		case AnimEventCmd.Particle:
			EmitParticle(animEventInfo, animEventInfo.gameObjectPath);
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
			ChangeEquip(animEventInfo);
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
			ChangeEquip(animEventInfo);
			break;
		case AnimEventCmd.LocateProp:
			LocateProp(animEventInfo);
			break;
		case AnimEventCmd.UnlocateProp:
			UnLocateProp(animEventInfo);
			break;
		case AnimEventCmd.Voice:
			EmitVoice(animEventInfo);
			break;
		case AnimEventCmd.LandingEffect:
			EmitLandingEffect(animEventInfo);
			break;
		case AnimEventCmd.IntegratedEffect:
			EmitIntegratedEffect(animEventInfo);
			break;
		}
		if (animEventInfo.isFinally && removeFinally)
		{
			_reservedFinallyEvents.Remove(animEventInfo);
		}
	}

	private Vector3 GetEmitPosition(AnimationEventInfo animEventInfo, bool shouldFollow, out Transform parentTrans)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		parentTrans = null;
		Vector3 currentPosition = _targetPlayable.GetCurrentPosition();
		if (string.IsNullOrEmpty(animEventInfo.paramStr))
		{
			return currentPosition;
		}
		Transform val = null;
		if (animEventInfo.paramStr == NoBoneName)
		{
			parentTrans = ((Component)this).transform;
		}
		else
		{
			parentTrans = KUtility.FindTransformByName(_targetPlayable.GetGameObject(), animEventInfo.paramStr);
			val = parentTrans;
			if ((Object)null == (Object)(object)parentTrans)
			{
				parentTrans = ((Component)this).transform;
			}
		}
		Transform transform = ((Component)this).gameObject.transform;
		currentPosition = parentTrans.position + transform.forward * animEventInfo.paramVector.z + transform.right * animEventInfo.paramVector.x + transform.up * animEventInfo.paramVector.y;
		if (shouldFollow)
		{
			parentTrans = val;
		}
		else if (!animEventInfo.paramBool)
		{
			parentTrans = null;
		}
		return currentPosition;
	}

	private void EmitParticle(AnimationEventInfo animEventInfo, string effectPath)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (effectPath != null)
		{
			Transform parentTrans;
			Vector3 emitPosition = GetEmitPosition(animEventInfo, shouldFollow: false, out parentTrans);
			bool comeForwardToCamera = animEventInfo.paramInt == 2;
			bool groundDecal = animEventInfo.paramBool2 || animEventInfo.paramInt == 1;
			Quaternion emitRotation = GetEmitRotation(animEventInfo.paramInt2, animEventInfo.paramBool, animEventInfo.paramVector2);
			ParticleManager.Emit(effectPath, emitPosition, emitRotation, parentTrans, useLocalPosition: false, comeForwardToCamera, groundDecal);
		}
	}

	private Quaternion GetEmitRotation(int rotationType, bool followTarget, Vector3 rotationalVector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.identity;
		switch ((AnimationEventInfo.Rotation)rotationType)
		{
		case AnimationEventInfo.Rotation.FaceWithCharacter:
			if (!followTarget)
			{
				val = ((Component)this).gameObject.transform.rotation;
			}
			break;
		case AnimationEventInfo.Rotation.FaceToCamera:
		{
			Quaternion rotation = ((Component)KSingleton<MainCamera>.Instance()).transform.rotation;
			Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
			val = Quaternion.Euler(180f + eulerAngles.x, 180f + eulerAngles.y, eulerAngles.z);
			break;
		}
		}
		return Quaternion.Euler(rotationalVector) * val;
	}

	private void EmitSound(AnimationEventInfo animEventInfo)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (animEventInfo.paramBool)
		{
			SoundManager.Play(animEventInfo.gameObjectPath, loop: false, new SoundManager.PitchRange(animEventInfo.paramVector.x, animEventInfo.paramVector.y));
		}
		else
		{
			SoundManager.Play(animEventInfo.gameObjectPath, _targetPlayable.GetCurrentPosition(), null, loop: false, new SoundManager.PitchRange(animEventInfo.paramVector.x, animEventInfo.paramVector.y));
		}
	}

	private void EmitVibrate(AnimationEventInfo animEventInfo)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = PlayerBehavior.LocalPlayer.CurrentPosition - _targetPlayable.GetCurrentPosition();
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (animEventInfo.paramFloat <= 0f || animEventInfo.paramFloat > magnitude)
		{
			Vibration.Vibrate(animEventInfo.enumID);
		}
	}

	private void EmitCustomCmd(AnimationEventInfo animEventInfo)
	{
		if (animEventInfo.paramStr != string.Empty)
		{
			_targetPlayable.GetGameObject().SendMessage(animEventInfo.paramStr, (object)animEventInfo.paramStr2);
		}
	}

	private void TurnOnTrail(AnimationEventInfo animEventInfo, float timePassed)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(animEventInfo.gameObjectPath))
		{
			return;
		}
		Transform parentTrans;
		Vector3 emitPosition = GetEmitPosition(animEventInfo, shouldFollow: true, out parentTrans);
		if (!((Object)(object)parentTrans == (Object)null))
		{
			_activeTrails.TryGetValue(animEventInfo.gameObjectPath, out var value);
			if ((Object)(object)value == (Object)null)
			{
				Quaternion rotation = Quaternion.Euler(animEventInfo.paramVector2);
				value = ParticleManager.EmitSync(animEventInfo.gameObjectPath, emitPosition, rotation, parentTrans, useLocalPosition: false, comeForwardToCamera: false, groundDecal: false, reusable: false);
				_activeTrails[animEventInfo.gameObjectPath] = value;
			}
			TrailEmitted(animEventInfo, value, timePassed);
		}
	}

	private void TrailEmitted(AnimationEventInfo animEventInfo, GameObject trailObject, float timePassed)
	{
		if (!animEventInfo.paramBool && animEventInfo.paramStr2 == string.Empty)
		{
			return;
		}
		PlaneTrail component = trailObject.GetComponent<PlaneTrail>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		if (animEventInfo.paramBool)
		{
			CharacterBehavior component2 = ((Component)this).gameObject.GetComponent<CharacterBehavior>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component.TipTransform = component2.WeaponTipTransform;
			}
		}
		else if (animEventInfo.paramStr2 != string.Empty)
		{
			component.TipTransform = KUtility.FindTransformByName(((Component)this).gameObject, animEventInfo.paramStr2);
		}
		if ((Object)null == (Object)(object)component.TipTransform)
		{
			component.TipTransform = ((Component)this).gameObject.transform;
		}
		component.Option = (PlaneTrail.FixOption)animEventInfo.paramInt;
		if (component.Option == PlaneTrail.FixOption.Baked)
		{
			if (animEventInfo.TrailData == null)
			{
				animEventInfo.TrailData = KUtility.ParseJson<TrailBaker.TrailData>(animEventInfo.paramStr3);
			}
			if (animEventInfo.TrailData == null)
			{
				component.Option = PlaneTrail.FixOption.None;
			}
			else
			{
				component.SetBaked(animEventInfo.TrailData, ((Component)this).gameObject.transform, animEventInfo.paramFloat, timePassed);
			}
		}
		component.Emit = true;
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
		if (!((Object)(object)value == (Object)null))
		{
			TrailRenderer_Base component = value.GetComponent<TrailRenderer_Base>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.Emit = false;
			}
		}
	}

	private void ScenePlaybackRatio(AnimationEventInfo animEventInfo)
	{
		float x = animEventInfo.paramVector.x;
		float y = animEventInfo.paramVector.y;
		((MonoBehaviour)this).StartCoroutine(CoModifyPlaybackRatio(x, y));
	}

	private IEnumerator CoModifyPlaybackRatio(float destRatio, float duration)
	{
		float beginTime = Time.realtimeSinceStartup;
		float endTime = Time.realtimeSinceStartup + duration;
		float from = Time.timeScale;
		while (Time.realtimeSinceStartup < endTime)
		{
			float dt = Time.realtimeSinceStartup - beginTime;
			Time.timeScale = EaseOutQuad(dt, from, destRatio, duration);
			yield return null;
		}
		Time.timeScale = destRatio;
	}

	private float EaseOutQuad(float t, float from, float to, float duration)
	{
		t /= duration;
		return (0f - to) * t * (t - 2f) + from;
	}

	private bool IsLocalPlayer()
	{
		return (Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject == (Object)(object)_targetPlayable.GetGameObject();
	}

	private void CameraShake(AnimationEventInfo animEventInfo)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (animEventInfo.paramBool && !IsLocalPlayer())
		{
			return;
		}
		Vector3 val = PlayerBehavior.LocalPlayer.CurrentPosition - _targetPlayable.GetCurrentPosition();
		float magnitude = ((Vector3)(ref val)).magnitude;
		float num = animEventInfo.paramVector.x;
		float num2 = animEventInfo.paramVector.y;
		float z = animEventInfo.paramVector.z;
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
		KSingleton<CameraShaker>.Instance().Shake(num, num2, z);
	}

	private void CameraZoom(AnimationEventInfo animEventInfo)
	{
		if (!animEventInfo.paramBool || IsLocalPlayer())
		{
			float x = animEventInfo.paramVector.x;
			float y = animEventInfo.paramVector.y;
			float x2 = animEventInfo.paramVector2.x;
			float y2 = animEventInfo.paramVector2.y;
			KSingleton<CameraController>.Instance().AddCameraEffect(new DollyCameraEffect(x, y, x2, y2));
		}
	}

	private void ChangeEquip(AnimationEventInfo animEventInfo)
	{
		PlayerBehavior component = _targetPlayable.GetGameObject().GetComponent<PlayerBehavior>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.ChangeEquipmentWhileCurrentAnimation(animEventInfo.paramStr);
		}
		NPCActorBehavior component2 = _targetPlayable.GetGameObject().GetComponent<NPCActorBehavior>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.ChangeEquipment(animEventInfo.paramStr);
		}
	}

	private void ReEquipCurrentWeapon()
	{
		PlayerBehavior component = _targetPlayable.GetGameObject().GetComponent<PlayerBehavior>();
		if (Object.op_Implicit((Object)(object)component))
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
		KSingleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(GameObject), delegate(Object asset)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Expected O, but got Unknown
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = (GameObject)asset;
			if (!((Object)(object)val == (Object)null) && !((Object)(object)this == (Object)null))
			{
				Animation componentInChildren = _targetPlayable.GetGameObject().GetComponentInChildren<Animation>();
				componentInChildren.Sample();
				Transform val2 = _targetPlayable.GetGameObject().transform;
				if (!string.IsNullOrEmpty(animEventInfo.paramStr) && !(animEventInfo.paramStr == NoBoneName))
				{
					Transform val3 = KUtility.FindTransformByName(_targetPlayable.GetGameObject(), animEventInfo.paramStr);
					if (!((Object)(object)val3 == (Object)null))
					{
						val2 = val3;
					}
				}
				GameObject val4 = (GameObject)Object.Instantiate(asset);
				val4.transform.position = Vector3.zero;
				val4.transform.rotation = Quaternion.identity;
				Transform val5 = KUtility.FindTransformByName(val4, animEventInfo.paramStr);
				if ((Object)(object)val5 != (Object)null)
				{
					Matrix4x4 localToWorldMatrix = ((Component)val2).transform.localToWorldMatrix;
					Matrix4x4 val6 = Matrix4x4.TRS(((Component)val5).transform.localPosition, ((Component)val5).transform.localRotation, ((Component)val5).transform.localScale);
					Matrix4x4 m = localToWorldMatrix * Matrix4x4.Inverse(val6);
					KMathUtil.DecomposeMatrix(m, out var position, out var rotation, out var _);
					val4.transform.rotation = rotation;
					val4.transform.position = position;
				}
				else
				{
					val4.transform.rotation = val2.rotation;
					val4.transform.position = val2.position;
				}
				if (animEventInfo.paramVector != Vector3.zero)
				{
					val4.transform.Translate(animEventInfo.paramVector);
				}
				if (animEventInfo.paramVector2 != Vector3.zero)
				{
					val4.transform.Rotate(animEventInfo.paramVector2);
				}
				_attachedProps.Add(new LocatedPropInfo
				{
					Path = path,
					PropObject = val4,
					AutoRemoveAtMotionChanged = animEventInfo.paramBool
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
		int count = _attachedProps.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			if (_attachedProps[num].Path == gameObjectPath)
			{
				Object.Destroy((Object)(object)_attachedProps[num].PropObject);
				_attachedProps.RemoveAt(num);
			}
		}
	}

	private void AutoUnLocatePropsAtMotionChanged()
	{
		int count = _attachedProps.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			if (_attachedProps[num].AutoRemoveAtMotionChanged)
			{
				Object.Destroy((Object)(object)_attachedProps[num].PropObject);
				_attachedProps.RemoveAt(num);
			}
		}
	}

	private void UnlocateAllProps()
	{
		int count = _attachedProps.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			Object.Destroy((Object)(object)_attachedProps[num].PropObject);
			_attachedProps.RemoveAt(num);
		}
	}

	private void EmitVoice(AnimationEventInfo animEventInfo)
	{
		GameObject gameObject = _targetPlayable.GetGameObject();
		if ((Object)(object)gameObject == (Object)null)
		{
			return;
		}
		PlayerBehavior component = gameObject.GetComponent<PlayerBehavior>();
		if ((Object)(object)component == (Object)null || (animEventInfo.paramBool && !component.IsLocalPlayer))
		{
			return;
		}
		float paramFloat = animEventInfo.paramFloat;
		if (!(paramFloat > 0f) || !(paramFloat < Random.value))
		{
			PlayerVoice.Type enumID = (PlayerVoice.Type)animEventInfo.enumID;
			int paramInt = animEventInfo.paramInt;
			if (paramInt == 0)
			{
				component.Voice.Play(enumID);
			}
			else
			{
				component.Voice.Play(enumID, paramInt - 1);
			}
		}
	}

	private void EmitLandingEffect(AnimationEventInfo animEventInfo)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		LandingEffectManager landingEffectManager = KSingleton<LandingEffectManager>.Instance();
		GameObject gameObject = _targetPlayable.GetGameObject();
		if ((Object)(object)gameObject == (Object)null)
		{
			return;
		}
		CharacterBehavior component = gameObject.GetComponent<CharacterBehavior>();
		if (!((Object)(object)component == (Object)null))
		{
			EffectSet effectSet = landingEffectManager.GetEffectSet(component.GetBiome(), animEventInfo.paramInt);
			if (effectSet != null)
			{
				EmitParticle(animEventInfo, effectSet.Particle.Path);
				SoundManager.Play(effectSet.Sound.Path, component.CurrentPosition);
			}
		}
	}

	private void EmitIntegratedEffect(AnimationEventInfo animEventInfo)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (animEventInfo.gameObjectPath == null)
		{
			return;
		}
		string gameObjectPath = animEventInfo.gameObjectPath;
		if (gameObjectPath == null)
		{
			return;
		}
		Transform parentTrans;
		Vector3 emitPosition = GetEmitPosition(animEventInfo, shouldFollow: false, out parentTrans);
		bool useLocalPosition = animEventInfo.paramInt == 2;
		bool comeForwardToCamera = animEventInfo.paramBool2 || animEventInfo.paramInt == 1;
		Quaternion emitRotation = GetEmitRotation(animEventInfo.paramInt2, animEventInfo.paramBool, animEventInfo.paramVector2);
		GameObject gameObject = _targetPlayable.GetGameObject();
		if (!((Object)(object)gameObject == (Object)null))
		{
			CharacterBehavior component = gameObject.GetComponent<CharacterBehavior>();
			if (!((Object)(object)component == (Object)null))
			{
				IntegratedEffect.Emit(gameObjectPath, component.GetBiome(), emitPosition, emitRotation, parentTrans, useLocalPosition, comeForwardToCamera);
			}
		}
	}
}
