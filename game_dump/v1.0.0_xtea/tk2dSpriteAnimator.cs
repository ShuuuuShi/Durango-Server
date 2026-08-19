using System;
using UnityEngine;

[AddComponentMenu("2D Toolkit/Sprite/tk2dSpriteAnimator")]
public class tk2dSpriteAnimator : MonoBehaviour
{
	private enum State
	{
		Init,
		Playing,
		Paused
	}

	[SerializeField]
	private tk2dSpriteAnimation library;

	[SerializeField]
	private int defaultClipId;

	public bool playAutomatically;

	private static State globalState;

	private tk2dSpriteAnimationClip currentClip;

	private float clipTime;

	private float clipFps = -1f;

	private int previousFrame = -1;

	public Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip> AnimationCompleted;

	public Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int> AnimationEventTriggered;

	private State state;

	protected tk2dBaseSprite _sprite;

	public static bool g_Paused
	{
		get
		{
			return (globalState & State.Paused) != 0;
		}
		set
		{
			globalState = (value ? State.Paused : State.Init);
		}
	}

	public bool Paused
	{
		get
		{
			return (state & State.Paused) != 0;
		}
		set
		{
			if (value)
			{
				state |= State.Paused;
			}
			else
			{
				state &= (State)(-3);
			}
		}
	}

	public tk2dSpriteAnimation Library
	{
		get
		{
			return library;
		}
		set
		{
			library = value;
		}
	}

	public int DefaultClipId
	{
		get
		{
			return defaultClipId;
		}
		set
		{
			defaultClipId = value;
		}
	}

	public tk2dSpriteAnimationClip DefaultClip => GetClipById(defaultClipId);

	public virtual tk2dBaseSprite Sprite
	{
		get
		{
			if ((Object)(object)_sprite == (Object)null)
			{
				_sprite = ((Component)this).GetComponent<tk2dBaseSprite>();
				if ((Object)(object)_sprite == (Object)null)
				{
					Debug.LogError((object)"Sprite not found attached to tk2dSpriteAnimator.");
				}
			}
			return _sprite;
		}
	}

	public bool Playing => (state & State.Playing) != 0;

	public tk2dSpriteAnimationClip CurrentClip => currentClip;

	public float ClipTimeSeconds => (!(clipFps > 0f)) ? (clipTime / currentClip.fps) : (clipTime / clipFps);

	public float ClipFps
	{
		get
		{
			return clipFps;
		}
		set
		{
			if (currentClip != null)
			{
				clipFps = ((!(value > 0f)) ? currentClip.fps : value);
			}
		}
	}

	public static float DefaultFps => 0f;

	public int CurrentFrame
	{
		get
		{
			switch (currentClip.wrapMode)
			{
			case tk2dSpriteAnimationClip.WrapMode.Once:
				return Mathf.Min((int)clipTime, currentClip.frames.Length);
			case tk2dSpriteAnimationClip.WrapMode.Loop:
			case tk2dSpriteAnimationClip.WrapMode.RandomLoop:
				return (int)clipTime % currentClip.frames.Length;
			case tk2dSpriteAnimationClip.WrapMode.LoopSection:
			{
				int num2 = (int)clipTime;
				int result = currentClip.loopStart + (num2 - currentClip.loopStart) % (currentClip.frames.Length - currentClip.loopStart);
				if (num2 >= currentClip.loopStart)
				{
					return result;
				}
				return num2;
			}
			case tk2dSpriteAnimationClip.WrapMode.PingPong:
			{
				int num = ((currentClip.frames.Length > 1) ? ((int)clipTime % (currentClip.frames.Length + currentClip.frames.Length - 2)) : 0);
				if (num >= currentClip.frames.Length)
				{
					num = 2 * currentClip.frames.Length - 2 - num;
				}
				return num;
			}
			case tk2dSpriteAnimationClip.WrapMode.Single:
				return 0;
			default:
				Debug.LogError((object)"Unhandled clip wrap mode");
				goto case tk2dSpriteAnimationClip.WrapMode.Loop;
			}
		}
	}

	private void OnEnable()
	{
		if ((Object)(object)Sprite == (Object)null)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void Start()
	{
		if (playAutomatically)
		{
			Play(DefaultClip);
		}
	}

	public static tk2dSpriteAnimator AddComponent(GameObject go, tk2dSpriteAnimation anim, int clipId)
	{
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = anim.clips[clipId];
		tk2dSpriteAnimator tk2dSpriteAnimator2 = go.AddComponent<tk2dSpriteAnimator>();
		tk2dSpriteAnimator2.Library = anim;
		tk2dSpriteAnimator2.SetSprite(tk2dSpriteAnimationClip2.frames[0].spriteCollection, tk2dSpriteAnimationClip2.frames[0].spriteId);
		return tk2dSpriteAnimator2;
	}

	private tk2dSpriteAnimationClip GetClipByNameVerbose(string name)
	{
		if ((Object)(object)library == (Object)null)
		{
			Debug.LogError((object)"Library not set");
			return null;
		}
		tk2dSpriteAnimationClip clipByName = library.GetClipByName(name);
		if (clipByName == null)
		{
			Debug.LogError((object)("Unable to find clip '" + name + "' in library"));
			return null;
		}
		return clipByName;
	}

	public void Play()
	{
		if (currentClip == null)
		{
			currentClip = DefaultClip;
		}
		Play(currentClip);
	}

	public void Play(string name)
	{
		Play(GetClipByNameVerbose(name));
	}

	public void Play(tk2dSpriteAnimationClip clip)
	{
		Play(clip, 0f, DefaultFps);
	}

	public void PlayFromFrame(int frame)
	{
		if (currentClip == null)
		{
			currentClip = DefaultClip;
		}
		PlayFromFrame(currentClip, frame);
	}

	public void PlayFromFrame(string name, int frame)
	{
		PlayFromFrame(GetClipByNameVerbose(name), frame);
	}

	public void PlayFromFrame(tk2dSpriteAnimationClip clip, int frame)
	{
		PlayFrom(clip, ((float)frame + 0.001f) / clip.fps);
	}

	public void PlayFrom(float clipStartTime)
	{
		if (currentClip == null)
		{
			currentClip = DefaultClip;
		}
		PlayFrom(currentClip, clipStartTime);
	}

	public void PlayFrom(string name, float clipStartTime)
	{
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = ((!Object.op_Implicit((Object)(object)library)) ? null : library.GetClipByName(name));
		if (tk2dSpriteAnimationClip2 == null)
		{
			ClipNameError(name);
		}
		else
		{
			PlayFrom(tk2dSpriteAnimationClip2, clipStartTime);
		}
	}

	public void PlayFrom(tk2dSpriteAnimationClip clip, float clipStartTime)
	{
		Play(clip, clipStartTime, DefaultFps);
	}

	public void Play(tk2dSpriteAnimationClip clip, float clipStartTime, float overrideFps)
	{
		if (clip != null)
		{
			float num = ((!(overrideFps > 0f)) ? clip.fps : overrideFps);
			if (clipStartTime == 0f && IsPlaying(clip))
			{
				clipFps = num;
				return;
			}
			state |= State.Playing;
			currentClip = clip;
			clipFps = num;
			if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Single || currentClip.frames == null)
			{
				WarpClipToLocalTime(currentClip, 0f);
				state &= (State)(-2);
			}
			else if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomFrame || currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomLoop)
			{
				int num2 = Random.Range(0, currentClip.frames.Length);
				WarpClipToLocalTime(currentClip, num2);
				if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomFrame)
				{
					previousFrame = -1;
					state &= (State)(-2);
				}
			}
			else
			{
				float num3 = clipStartTime * clipFps;
				if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Once && num3 >= clipFps * (float)currentClip.frames.Length)
				{
					WarpClipToLocalTime(currentClip, currentClip.frames.Length - 1);
					state &= (State)(-2);
				}
				else
				{
					WarpClipToLocalTime(currentClip, num3);
					clipTime = num3;
				}
			}
		}
		else
		{
			Debug.LogError((object)"Calling clip.Play() with a null clip");
			OnAnimationCompleted();
			state &= (State)(-2);
		}
	}

	public void Stop()
	{
		state &= (State)(-2);
	}

	public void StopAndResetFrame()
	{
		if (currentClip != null)
		{
			SetSprite(currentClip.frames[0].spriteCollection, currentClip.frames[0].spriteId);
		}
		Stop();
	}

	public bool IsPlaying(string name)
	{
		return Playing && CurrentClip != null && CurrentClip.name == name;
	}

	public bool IsPlaying(tk2dSpriteAnimationClip clip)
	{
		return Playing && CurrentClip != null && CurrentClip == clip;
	}

	public tk2dSpriteAnimationClip GetClipById(int id)
	{
		if ((Object)(object)library == (Object)null)
		{
			return null;
		}
		return library.GetClipById(id);
	}

	public int GetClipIdByName(string name)
	{
		return (!Object.op_Implicit((Object)(object)library)) ? (-1) : library.GetClipIdByName(name);
	}

	public tk2dSpriteAnimationClip GetClipByName(string name)
	{
		return (!Object.op_Implicit((Object)(object)library)) ? null : library.GetClipByName(name);
	}

	public void Pause()
	{
		state |= State.Paused;
	}

	public void Resume()
	{
		state &= (State)(-3);
	}

	public void SetFrame(int currFrame)
	{
		SetFrame(currFrame, triggerEvent: true);
	}

	public void SetFrame(int currFrame, bool triggerEvent)
	{
		if (currentClip == null)
		{
			currentClip = DefaultClip;
		}
		if (currentClip != null)
		{
			int num = currFrame % currentClip.frames.Length;
			SetFrameInternal(num);
			if (triggerEvent && currentClip.frames.Length > 0 && currFrame >= 0)
			{
				ProcessEvents(num - 1, num, 1);
			}
		}
	}

	public void UpdateAnimation(float deltaTime)
	{
		State state = this.state | globalState;
		if (state != State.Playing)
		{
			return;
		}
		clipTime += deltaTime * clipFps;
		int num = previousFrame;
		switch (currentClip.wrapMode)
		{
		case tk2dSpriteAnimationClip.WrapMode.Loop:
		case tk2dSpriteAnimationClip.WrapMode.RandomLoop:
		{
			int num5 = (int)clipTime % currentClip.frames.Length;
			SetFrameInternal(num5);
			if (num5 < num)
			{
				ProcessEvents(num, currentClip.frames.Length - 1, 1);
				ProcessEvents(-1, num5, 1);
			}
			else
			{
				ProcessEvents(num, num5, 1);
			}
			break;
		}
		case tk2dSpriteAnimationClip.WrapMode.LoopSection:
		{
			int num3 = (int)clipTime;
			int num4 = currentClip.loopStart + (num3 - currentClip.loopStart) % (currentClip.frames.Length - currentClip.loopStart);
			if (num3 >= currentClip.loopStart)
			{
				SetFrameInternal(num4);
				num3 = num4;
				if (num < currentClip.loopStart)
				{
					ProcessEvents(num, currentClip.loopStart - 1, 1);
					ProcessEvents(currentClip.loopStart - 1, num3, 1);
				}
				else if (num3 < num)
				{
					ProcessEvents(num, currentClip.frames.Length - 1, 1);
					ProcessEvents(currentClip.loopStart - 1, num3, 1);
				}
				else
				{
					ProcessEvents(num, num3, 1);
				}
			}
			else
			{
				SetFrameInternal(num3);
				ProcessEvents(num, num3, 1);
			}
			break;
		}
		case tk2dSpriteAnimationClip.WrapMode.PingPong:
		{
			int num6 = ((currentClip.frames.Length > 1) ? ((int)clipTime % (currentClip.frames.Length + currentClip.frames.Length - 2)) : 0);
			int direction = 1;
			if (num6 >= currentClip.frames.Length)
			{
				num6 = 2 * currentClip.frames.Length - 2 - num6;
				direction = -1;
			}
			if (num6 < num)
			{
				direction = -1;
			}
			SetFrameInternal(num6);
			ProcessEvents(num, num6, direction);
			break;
		}
		case tk2dSpriteAnimationClip.WrapMode.Once:
		{
			int num2 = (int)clipTime;
			if (num2 >= currentClip.frames.Length)
			{
				SetFrameInternal(currentClip.frames.Length - 1);
				this.state &= (State)(-2);
				ProcessEvents(num, currentClip.frames.Length - 1, 1);
				OnAnimationCompleted();
			}
			else
			{
				SetFrameInternal(num2);
				ProcessEvents(num, num2, 1);
			}
			break;
		}
		case tk2dSpriteAnimationClip.WrapMode.RandomFrame:
			break;
		}
	}

	private void ClipNameError(string name)
	{
		Debug.LogError((object)("Unable to find clip named '" + name + "' in library"));
	}

	private void ClipIdError(int id)
	{
		Debug.LogError((object)("Play - Invalid clip id '" + id + "' in library"));
	}

	private void WarpClipToLocalTime(tk2dSpriteAnimationClip clip, float time)
	{
		clipTime = time;
		int num = (int)clipTime % clip.frames.Length;
		tk2dSpriteAnimationFrame tk2dSpriteAnimationFrame2 = clip.frames[num];
		SetSprite(tk2dSpriteAnimationFrame2.spriteCollection, tk2dSpriteAnimationFrame2.spriteId);
		if (tk2dSpriteAnimationFrame2.triggerEvent && AnimationEventTriggered != null)
		{
			AnimationEventTriggered(this, clip, num);
		}
		previousFrame = num;
	}

	private void SetFrameInternal(int currFrame)
	{
		if (previousFrame != currFrame)
		{
			SetSprite(currentClip.frames[currFrame].spriteCollection, currentClip.frames[currFrame].spriteId);
			previousFrame = currFrame;
		}
	}

	private void ProcessEvents(int start, int last, int direction)
	{
		if (AnimationEventTriggered == null || start == last || Mathf.Sign((float)(last - start)) != Mathf.Sign((float)direction))
		{
			return;
		}
		int num = last + direction;
		tk2dSpriteAnimationFrame[] frames = currentClip.frames;
		for (int i = start + direction; i != num; i += direction)
		{
			if (frames[i].triggerEvent && AnimationEventTriggered != null)
			{
				AnimationEventTriggered(this, currentClip, i);
			}
		}
	}

	private void OnAnimationCompleted()
	{
		previousFrame = -1;
		if (AnimationCompleted != null)
		{
			AnimationCompleted(this, currentClip);
		}
	}

	public virtual void LateUpdate()
	{
		UpdateAnimation(Time.deltaTime);
	}

	public virtual void SetSprite(tk2dSpriteCollectionData spriteCollection, int spriteId)
	{
		Sprite.SetSprite(spriteCollection, spriteId);
	}
}
