using System;
using UnityEngine;

[Serializable]
public class AnimationEventInfo : IComparable<AnimationEventInfo>
{
	public enum Position
	{
		Default,
		ToGround,
		ToCamera
	}

	public enum Rotation
	{
		Default,
		FaceWithCharacter,
		FaceToCamera
	}

	public int frame;

	public float time;

	public AnimEventCmd animEventCmd;

	public bool isOnce;

	public bool isFinally;

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

	public AnimationEventInfo(int frame)
	{
		SetFrame(frame);
		animEventCmd = AnimEventCmd.Sound;
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
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (obj == null || (object)GetType() != obj.GetType())
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
		if (cmd == AnimEventCmd.Voice)
		{
			info.enumID = 0;
			info.paramInt = 0;
			info.paramFloat = 1f;
			info.paramBool = true;
		}
	}
}
