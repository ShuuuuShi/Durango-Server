using UnityEngine;

[ExecuteInEditMode]
public class AnimatingProp : MonoBehaviour, IAnimationEventPlayable, IMotionPlayable
{
	[SerializeField]
	private string _motionName = string.Empty;

	private AnimationClipInfo _curInfo;

	private Animation _anim;

	private AnimationState _curAnimState;

	private string _curAnimStateName;

	private Animation Anim
	{
		get
		{
			if ((Object)null == (Object)(object)_anim)
			{
				_anim = ((Component)this).gameObject.GetComponentInChildren<Animation>();
			}
			return _anim;
		}
	}

	public AnimationState CurAnimState
	{
		get
		{
			return _curAnimState;
		}
		set
		{
			_curAnimState = value;
			_curAnimStateName = ((!((TrackedReference)(object)_curAnimState == (TrackedReference)null)) ? _curAnimState.name : string.Empty);
		}
	}

	public AnimationClipInfo GetCurrentAnimationClipInfo()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Invalid comparison between Unknown and I4
		if ((TrackedReference)null == (TrackedReference)(object)CurAnimState)
		{
			return _curInfo;
		}
		if (!CurAnimState.enabled)
		{
			return AnimationEventController.InvalidAnimationClipInfo;
		}
		_curInfo.Name = _curAnimStateName;
		_curInfo.AnimTime = Mathf.Repeat(CurAnimState.time, CurAnimState.length);
		_curInfo.Length = CurAnimState.length;
		_curInfo.IsLoop = (CurAnimState.wrapMode & 2) > 0;
		_curInfo.PlaybackRate = 1f;
		_curInfo.Clip = CurAnimState.clip;
		return _curInfo;
	}

	public Vector3 GetCurrentPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public AnimationState GetCurAnimState()
	{
		return CurAnimState;
	}

	public float CrossFade(string motionName, float fadeTime, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		return Play(motionName, loop, beginTime, playbackRate);
	}

	public float Play(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f)
	{
		if ((Object)(object)Anim == (Object)null)
		{
			return 0f;
		}
		Anim.Play(motionName);
		Anim.wrapMode = (WrapMode)(loop ? 2 : 0);
		CurAnimState = Anim[motionName];
		CurAnimState.enabled = true;
		CurAnimState.weight = 1f;
		CurAnimState.wrapMode = (WrapMode)(loop ? 2 : 0);
		CurAnimState.time = beginTime;
		CurAnimState.speed = playbackRate;
		return CurAnimState.length;
	}

	public void SetDefaultMotionName(string motionName)
	{
		_motionName = motionName;
	}

	public string GetDefaultMotionName()
	{
		return _motionName;
	}

	public WrapMode GetWrapMode(string motionName)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		AnimationClip clip = Anim.GetClip(motionName);
		return (WrapMode)(Object.op_Implicit((Object)(object)clip) ? ((int)clip.wrapMode) : 0);
	}

	public GameObject GetGameObject()
	{
		return ((Component)this).gameObject;
	}

	public void SetServerSideRootMotionEnable(bool serverSideRootMotionEnabled)
	{
	}
}
