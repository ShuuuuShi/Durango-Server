using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveMotionInfo : IEnumerable<AnimationSequenceClip>, IEnumerable, IClipEnumerator
{
	[SerializeField]
	public string motion;

	[SerializeField]
	public string rot_motion_cw;

	[SerializeField]
	public string rot_motion_ccw;

	[SerializeField]
	public string turn_reverse_motion;

	[SerializeField]
	public Condition conditions = new Condition();

	[SerializeField]
	public float base_move_speed = 100f;

	[SerializeField]
	public float rot_speed = 60f;

	[SerializeField]
	public float playback_rate = 1f;

	[SerializeField]
	public float rot_playback_rate = 1f;

	[SerializeField]
	private AnimationClip _clipObjMove;

	[SerializeField]
	private AnimationClip _clipObjRotateCW;

	[SerializeField]
	private AnimationClip _clipObjRotateCCW;

	[SerializeField]
	private AnimationClip _clipObjTurnReverse;

	public AnimationClip ClipMove
	{
		get
		{
			return _clipObjMove;
		}
		set
		{
			_clipObjMove = value;
			motion = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public AnimationClip ClipRotateCW
	{
		get
		{
			return _clipObjRotateCW;
		}
		set
		{
			_clipObjRotateCW = value;
			rot_motion_cw = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public AnimationClip ClipRotateCCW
	{
		get
		{
			return _clipObjRotateCCW;
		}
		set
		{
			_clipObjRotateCCW = value;
			rot_motion_ccw = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public AnimationClip ClipTurnReverse
	{
		get
		{
			return _clipObjTurnReverse;
		}
		set
		{
			_clipObjTurnReverse = value;
			turn_reverse_motion = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public void CollectClips(List<AnimationClip> clips)
	{
		clips.Add(ClipMove);
		clips.Add(ClipRotateCW);
		clips.Add(ClipRotateCCW);
		clips.Add(ClipTurnReverse);
	}

	public void AutoFill(List<string> animFbxFiles)
	{
		if (ClipMove == null)
		{
			ClipMove = AnimalFrameworkUtils.AutoFillInternal("move", string.Empty, animFbxFiles);
		}
		if (ClipRotateCW == null)
		{
			ClipRotateCW = AnimalFrameworkUtils.AutoFillInternal("rotate_cw", string.Empty, animFbxFiles);
		}
		if (ClipRotateCCW == null)
		{
			ClipRotateCCW = AnimalFrameworkUtils.AutoFillInternal("rotate_ccw", string.Empty, animFbxFiles);
		}
		if (ClipTurnReverse == null)
		{
			ClipTurnReverse = AnimalFrameworkUtils.AutoFillInternal("turn", string.Empty, animFbxFiles);
		}
	}

	public IEnumerator<AnimationSequenceClip> GetEnumerator()
	{
		return new AnimationSequenceClip.Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool TryMoveNext(int index, out AnimationSequenceClip clip)
	{
		if (index == 0)
		{
			clip = new AnimationSequenceClip(ClipMove);
			return true;
		}
		clip = default(AnimationSequenceClip);
		return false;
	}
}
