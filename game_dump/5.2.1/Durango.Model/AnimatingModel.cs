using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Model;

public class AnimatingModel : MonoBehaviour, IMotionPlayable, IAnimationEventPlayable
{
	private AnimationClipInfo _curInfo;

	private Animation _anim;

	private AnimationState _curAnimState;

	private string _curAnimStateName;

	private Animation Anim
	{
		get
		{
			if (null == _anim)
			{
				_anim = base.gameObject.GetComponentInChildren<Animation>();
			}
			return _anim;
		}
	}

	[CanBeNull]
	public AnimationState CurAnimState
	{
		get
		{
			return _curAnimState;
		}
		set
		{
			_curAnimState = value;
			_curAnimStateName = ((!(_curAnimState == null)) ? _curAnimState.name : string.Empty);
		}
	}

	public bool AnimationEventProhibited => false;

	public AnimationClipInfo GetCurrentAnimationClipInfo()
	{
		if (null == CurAnimState)
		{
			return _curInfo;
		}
		if (!CurAnimState.enabled)
		{
			return AnimationEventController.InvalidAnimationClipInfo;
		}
		_curInfo.Name = _curAnimStateName;
		_curInfo.State = CurAnimState;
		return _curInfo;
	}

	public Vector3 GetCurrentPosition()
	{
		return base.transform.position;
	}

	public AnimationState GetCurAnimState()
	{
		return CurAnimState;
	}

	public float CrossFade(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		if (Anim == null)
		{
			return 0f;
		}
		if (fadeTime > 0f)
		{
			Anim.CrossFade(motionName, fadeTime);
		}
		else
		{
			Anim.Play(motionName);
		}
		Anim.wrapMode = (loop ? WrapMode.Loop : WrapMode.Default);
		CurAnimState = Anim[motionName];
		if (CurAnimState == null)
		{
			return 0f;
		}
		CurAnimState.enabled = true;
		CurAnimState.weight = 1f;
		CurAnimState.wrapMode = (loop ? WrapMode.Loop : WrapMode.Default);
		CurAnimState.time = beginTime;
		CurAnimState.speed = playbackRate;
		return CurAnimState.length;
	}

	public float Play(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		return CrossFade(motionName, 0f, loop, beginTime, playbackRate);
	}

	WrapMode IMotionPlayable.GetWrapMode(string motionName)
	{
		if (Anim == null)
		{
			return WrapMode.Default;
		}
		AnimationClip clip = Anim.GetClip(motionName);
		if (!clip)
		{
			return WrapMode.Default;
		}
		return clip.wrapMode;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public void SetActivateRootMotion(bool active)
	{
	}
}
