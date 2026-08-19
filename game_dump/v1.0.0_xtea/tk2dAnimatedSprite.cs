using System;
using UnityEngine;

[AddComponentMenu("2D Toolkit/Sprite/tk2dAnimatedSprite (Obsolete)")]
public class tk2dAnimatedSprite : tk2dSprite
{
	public delegate void AnimationCompleteDelegate(tk2dAnimatedSprite sprite, int clipId);

	public delegate void AnimationEventDelegate(tk2dAnimatedSprite sprite, tk2dSpriteAnimationClip clip, tk2dSpriteAnimationFrame frame, int frameNum);

	[SerializeField]
	private tk2dSpriteAnimator _animator;

	[SerializeField]
	private tk2dSpriteAnimation anim;

	[SerializeField]
	private int clipId;

	public bool playAutomatically;

	public bool createCollider;

	public AnimationCompleteDelegate animationCompleteDelegate;

	public AnimationEventDelegate animationEventDelegate;

	public tk2dSpriteAnimator Animator
	{
		get
		{
			CheckAddAnimatorInternal();
			return _animator;
		}
	}

	public tk2dSpriteAnimation Library
	{
		get
		{
			return Animator.Library;
		}
		set
		{
			Animator.Library = value;
		}
	}

	public int DefaultClipId
	{
		get
		{
			return Animator.DefaultClipId;
		}
		set
		{
			Animator.DefaultClipId = value;
		}
	}

	public static bool g_paused
	{
		get
		{
			return tk2dSpriteAnimator.g_Paused;
		}
		set
		{
			tk2dSpriteAnimator.g_Paused = value;
		}
	}

	public bool Paused
	{
		get
		{
			return Animator.Paused;
		}
		set
		{
			Animator.Paused = value;
		}
	}

	public tk2dSpriteAnimationClip CurrentClip => Animator.CurrentClip;

	public float ClipTimeSeconds => Animator.ClipTimeSeconds;

	public float ClipFps
	{
		get
		{
			return Animator.ClipFps;
		}
		set
		{
			Animator.ClipFps = value;
		}
	}

	public bool Playing => Animator.Playing;

	public static float DefaultFps => tk2dSpriteAnimator.DefaultFps;

	private void CheckAddAnimatorInternal()
	{
		if ((Object)(object)_animator == (Object)null)
		{
			_animator = ((Component)this).gameObject.GetComponent<tk2dSpriteAnimator>();
			if ((Object)(object)_animator == (Object)null)
			{
				_animator = ((Component)this).gameObject.AddComponent<tk2dSpriteAnimator>();
				_animator.Library = anim;
				_animator.DefaultClipId = clipId;
				_animator.playAutomatically = playAutomatically;
			}
		}
	}

	protected override bool NeedBoxCollider()
	{
		return createCollider;
	}

	private void ProxyCompletedHandler(tk2dSpriteAnimator anim, tk2dSpriteAnimationClip clip)
	{
		if (animationCompleteDelegate == null)
		{
			return;
		}
		int num = -1;
		tk2dSpriteAnimationClip[] array = ((!((Object)(object)anim.Library != (Object)null)) ? null : anim.Library.clips);
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == clip)
				{
					num = i;
					break;
				}
			}
		}
		animationCompleteDelegate(this, num);
	}

	private void ProxyEventTriggeredHandler(tk2dSpriteAnimator anim, tk2dSpriteAnimationClip clip, int frame)
	{
		if (animationEventDelegate != null)
		{
			animationEventDelegate(this, clip, clip.frames[frame], frame);
		}
	}

	private void OnEnable()
	{
		Animator.AnimationCompleted = ProxyCompletedHandler;
		Animator.AnimationEventTriggered = ProxyEventTriggeredHandler;
	}

	private void OnDisable()
	{
		Animator.AnimationCompleted = null;
		Animator.AnimationEventTriggered = null;
	}

	private void Start()
	{
		CheckAddAnimatorInternal();
	}

	public static tk2dAnimatedSprite AddComponent(GameObject go, tk2dSpriteAnimation anim, int clipId)
	{
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = anim.clips[clipId];
		tk2dAnimatedSprite tk2dAnimatedSprite2 = go.AddComponent<tk2dAnimatedSprite>();
		tk2dAnimatedSprite2.SetSprite(tk2dSpriteAnimationClip2.frames[0].spriteCollection, tk2dSpriteAnimationClip2.frames[0].spriteId);
		tk2dAnimatedSprite2.anim = anim;
		return tk2dAnimatedSprite2;
	}

	public void Play()
	{
		if (Animator.DefaultClip != null)
		{
			Animator.Play(Animator.DefaultClip);
		}
	}

	public void Play(float clipStartTime)
	{
		if (Animator.DefaultClip != null)
		{
			Animator.PlayFrom(Animator.DefaultClip, clipStartTime);
		}
	}

	public void PlayFromFrame(int frame)
	{
		if (Animator.DefaultClip != null)
		{
			Animator.PlayFromFrame(Animator.DefaultClip, frame);
		}
	}

	public void Play(string name)
	{
		Animator.Play(name);
	}

	public void PlayFromFrame(string name, int frame)
	{
		Animator.PlayFromFrame(name, frame);
	}

	public void Play(string name, float clipStartTime)
	{
		Animator.PlayFrom(name, clipStartTime);
	}

	public void Play(tk2dSpriteAnimationClip clip, float clipStartTime)
	{
		Animator.PlayFrom(clip, clipStartTime);
	}

	public void Play(tk2dSpriteAnimationClip clip, float clipStartTime, float overrideFps)
	{
		Animator.Play(clip, clipStartTime, overrideFps);
	}

	public void Stop()
	{
		Animator.Stop();
	}

	public void StopAndResetFrame()
	{
		Animator.StopAndResetFrame();
	}

	[Obsolete]
	public bool isPlaying()
	{
		return Animator.Playing;
	}

	public bool IsPlaying(string name)
	{
		return Animator.IsPlaying(name);
	}

	public bool IsPlaying(tk2dSpriteAnimationClip clip)
	{
		return Animator.IsPlaying(clip);
	}

	public int GetClipIdByName(string name)
	{
		return Animator.GetClipIdByName(name);
	}

	public tk2dSpriteAnimationClip GetClipByName(string name)
	{
		return Animator.GetClipByName(name);
	}

	public void Pause()
	{
		Animator.Pause();
	}

	public void Resume()
	{
		Animator.Resume();
	}

	public void SetFrame(int currFrame)
	{
		Animator.SetFrame(currFrame);
	}

	public void SetFrame(int currFrame, bool triggerEvent)
	{
		Animator.SetFrame(currFrame, triggerEvent);
	}

	public void UpdateAnimation(float deltaTime)
	{
		Animator.UpdateAnimation(deltaTime);
	}
}
