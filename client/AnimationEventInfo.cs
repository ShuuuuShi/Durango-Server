using System;
using Durango.Render.Effect;
using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;

[Serializable]
public class AnimationEventInfo : IComparable<AnimationEventInfo>
{
	public enum Position
	{
		Default,
		ToGround,
		ToCamera,
		ToBoneLocal
	}

	public enum Rotation
	{
		Default,
		FaceWithCharacter,
		FaceToCamera,
		FaceToBoneLocal
	}

	public enum Emission
	{
		Default,
		PulledTowardCamera,
		Attach,
		ChasePosition
	}

	public enum LandingEffectSize
	{
		Small,
		Medium,
		Large
	}

	public enum SoundEventSwitch
	{
		None,
		VoiceType,
		Footstep
	}

	public int frame;

	public float time;

	public AnimEventCmd animEventCmd;

	public bool isOnce;

	public bool isFinally;

	public bool removeEnded;

	public string paramStr = string.Empty;

	public string paramStr2 = string.Empty;

	public string paramStr3 = string.Empty;

	public Vector3 paramVector;

	public Vector3 paramVector2;

	public float paramFloat;

	public int paramInt;

	public int paramInt2;

	public int enumID;

	public bool paramBool;

	public bool paramBool2;

	public bool shared;

	public string gameObjectPath;

	public TrailBaker.TrailData TrailData { get; set; }

	public string BoneName
	{
		get
		{
			return paramStr;
		}
		set
		{
			paramStr = value;
		}
	}

	public string CustomCmd
	{
		get
		{
			return paramStr;
		}
		set
		{
			paramStr = value;
		}
	}

	public string SoundEventName
	{
		get
		{
			return paramStr;
		}
		set
		{
			paramStr = value;
		}
	}

	public string PrefabPath
	{
		get
		{
			return paramStr;
		}
		set
		{
			paramStr = value;
		}
	}

	public string TrailTipBoneName
	{
		get
		{
			return paramStr2;
		}
		set
		{
			paramStr2 = value;
		}
	}

	public string CustomCmdParams
	{
		get
		{
			return paramStr2;
		}
		set
		{
			paramStr2 = value;
		}
	}

	public string BakedTrailData
	{
		get
		{
			return paramStr3;
		}
		set
		{
			paramStr3 = value;
		}
	}

	public Vector3 EmitPosition
	{
		get
		{
			return paramVector;
		}
		set
		{
			paramVector = value;
		}
	}

	public float CameraShakeAmplitudeU
	{
		get
		{
			return paramVector.x;
		}
		set
		{
			paramVector.x = value;
		}
	}

	public float CameraShakeAmplitudeV
	{
		get
		{
			return paramVector.y;
		}
		set
		{
			paramVector.y = value;
		}
	}

	public float CameraShakeInterval
	{
		get
		{
			return paramVector.z;
		}
		set
		{
			paramVector.z = value;
		}
	}

	public float CameraZoomRatio
	{
		get
		{
			return paramVector.x;
		}
		set
		{
			paramVector.x = value;
		}
	}

	public float CameraZoomTime
	{
		get
		{
			return paramVector.y;
		}
		set
		{
			paramVector.y = value;
		}
	}

	public float SoundPitchMin
	{
		get
		{
			return paramVector.x;
		}
		set
		{
			paramVector.x = value;
		}
	}

	public float SoundPitchMax
	{
		get
		{
			return paramVector.y;
		}
		set
		{
			paramVector.y = value;
		}
	}

	public float SceneTimeScaleRatio
	{
		get
		{
			return paramVector.x;
		}
		set
		{
			paramVector.x = value;
		}
	}

	public float SceneTimeScaleDuration
	{
		get
		{
			return paramVector.y;
		}
		set
		{
			paramVector.y = value;
		}
	}

	public Vector3 EmitRotation
	{
		get
		{
			return paramVector2;
		}
		set
		{
			paramVector2 = value;
		}
	}

	public float CameraShakeMinDist
	{
		get
		{
			return paramVector2.x;
		}
		set
		{
			paramVector2.x = value;
		}
	}

	public float CameraShakeMaxDist
	{
		get
		{
			return paramVector2.y;
		}
		set
		{
			paramVector2.y = value;
		}
	}

	public float CameraZoomDuration
	{
		get
		{
			return paramVector2.x;
		}
		set
		{
			paramVector2.x = value;
		}
	}

	public float CameraZoomOutTime
	{
		get
		{
			return paramVector2.y;
		}
		set
		{
			paramVector2.y = value;
		}
	}

	public float VibrationRange
	{
		get
		{
			return paramFloat;
		}
		set
		{
			paramFloat = value;
		}
	}

	public float EmitUniformScale
	{
		get
		{
			return paramFloat;
		}
		set
		{
			paramFloat = value;
		}
	}

	public float TrailPushBase
	{
		get
		{
			return paramFloat;
		}
		set
		{
			paramFloat = value;
		}
	}

	public float FadeOutDuration
	{
		get
		{
			return paramFloat;
		}
		set
		{
			paramFloat = value;
		}
	}

	public LandingEffectSize AnimLandingEffectSize
	{
		get
		{
			return (LandingEffectSize)paramInt;
		}
		set
		{
			paramInt = (int)value;
		}
	}

	public PlaneTrail.FixOption TrailFixOption
	{
		get
		{
			return (PlaneTrail.FixOption)paramInt;
		}
		set
		{
			paramInt = (int)value;
		}
	}

	public bool IsSoundChaseTarget
	{
		get
		{
			return paramInt != 0;
		}
		set
		{
			paramInt = (value ? 1 : 0);
		}
	}

	public Position PositionBasis
	{
		get
		{
			return (Position)paramInt;
		}
		set
		{
			paramInt = (int)value;
		}
	}

	public Emission EmissionType
	{
		get
		{
			return (Emission)paramInt;
		}
		set
		{
			paramInt = (int)value;
		}
	}

	public SoundEventSwitch AnimSoundEventSwitch
	{
		get
		{
			return (SoundEventSwitch)paramInt2;
		}
		set
		{
			paramInt2 = (int)value;
		}
	}

	public Rotation RotationBasis
	{
		get
		{
			return (Rotation)paramInt2;
		}
		set
		{
			paramInt2 = (int)value;
		}
	}

	public bool Use2dSound
	{
		get
		{
			return paramBool;
		}
		set
		{
			paramBool = value;
		}
	}

	public bool IsFollowingTarget
	{
		get
		{
			return paramBool;
		}
		set
		{
			paramBool = value;
		}
	}

	public bool TrailTipOverrideRequired
	{
		get
		{
			return paramBool;
		}
		set
		{
			paramBool = value;
		}
	}

	public bool IsLocalPlayerOnlyCameraEffect
	{
		get
		{
			return paramBool;
		}
		set
		{
			paramBool = value;
		}
	}

	public bool UseAutoUnLocateProp
	{
		get
		{
			return paramBool;
		}
		set
		{
			paramBool = value;
		}
	}

	public bool AdjustHeightToGround
	{
		get
		{
			return paramBool && EmissionType != Emission.Attach;
		}
		set
		{
			paramBool = value;
		}
	}

	public bool IsLocalPlayerOnlyEffectSound
	{
		get
		{
			return paramBool2;
		}
		set
		{
			paramBool2 = value;
		}
	}

	public AnimationEventInfo(int frame)
	{
		SetFrame(frame);
		animEventCmd = AnimEventCmd.SoundEvent;
	}

	public int GetFrame()
	{
		return frame;
	}

	public void SetFrame(int newFrame)
	{
		frame = newFrame;
		UpdateTime();
	}

	public float GetEventTime()
	{
		return time;
	}

	public void UpdateTime()
	{
		time = (float)frame / 30f;
	}

	public AnimationEventInfo CopySerializable()
	{
		return (AnimationEventInfo)MemberwiseClone();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		if (!(obj is AnimationEventInfo animationEventInfo))
		{
			return false;
		}
		if (animationEventInfo.frame != frame)
		{
			return false;
		}
		if (animationEventInfo.animEventCmd != animEventCmd)
		{
			return false;
		}
		if (animationEventInfo.isOnce != isOnce)
		{
			return false;
		}
		if (animationEventInfo.removeEnded != removeEnded)
		{
			return false;
		}
		if (animationEventInfo.isFinally != isFinally)
		{
			return false;
		}
		if (animationEventInfo.paramStr != paramStr)
		{
			return false;
		}
		if (animationEventInfo.paramStr2 != paramStr2)
		{
			return false;
		}
		if (animationEventInfo.paramVector != paramVector)
		{
			return false;
		}
		if (animationEventInfo.paramVector2 != paramVector2)
		{
			return false;
		}
		if (Mathf.Abs(animationEventInfo.paramFloat - paramFloat) > Mathf.Epsilon)
		{
			return false;
		}
		if (animationEventInfo.paramInt != paramInt)
		{
			return false;
		}
		if (animationEventInfo.paramInt2 != paramInt2)
		{
			return false;
		}
		if (animationEventInfo.enumID != enumID)
		{
			return false;
		}
		if (animationEventInfo.paramBool != paramBool)
		{
			return false;
		}
		if (animationEventInfo.paramBool2 != paramBool2)
		{
			return false;
		}
		if (animationEventInfo.shared != shared)
		{
			return false;
		}
		return animationEventInfo.gameObjectPath == gameObjectPath;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public int CompareTo(AnimationEventInfo v2)
	{
		if (frame != v2.frame)
		{
			return frame - v2.frame;
		}
		if (animEventCmd != v2.animEventCmd)
		{
			return animEventCmd - v2.animEventCmd;
		}
		if (paramStr != v2.paramStr)
		{
			return string.Compare(paramStr, v2.paramStr, StringComparison.Ordinal);
		}
		if (paramStr2 != v2.paramStr2)
		{
			return string.Compare(paramStr2, v2.paramStr2, StringComparison.Ordinal);
		}
		if (gameObjectPath != v2.gameObjectPath)
		{
			return string.Compare(gameObjectPath, v2.gameObjectPath, StringComparison.Ordinal);
		}
		if (enumID != v2.enumID)
		{
			return enumID - v2.enumID;
		}
		return GetHashCode() - v2.GetHashCode();
	}

	public static void Init(AnimationEventInfo info, AnimEventCmd cmd)
	{
		info.animEventCmd = cmd;
		if (cmd == AnimEventCmd.SoundEvent)
		{
			info.gameObjectPath = null;
			info.paramInt = 0;
			info.paramInt2 = 0;
			info.paramBool2 = false;
		}
	}
}
