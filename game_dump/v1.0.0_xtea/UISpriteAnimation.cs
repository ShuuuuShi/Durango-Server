using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Sprite Animation")]
[RequireComponent(typeof(UISprite))]
public class UISpriteAnimation : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	protected int mFPS = 30;

	[SerializeField]
	[HideInInspector]
	protected string mPrefix = string.Empty;

	[SerializeField]
	[HideInInspector]
	protected bool mLoop = true;

	[HideInInspector]
	[SerializeField]
	protected bool mSnap = true;

	protected UISprite mSprite;

	protected float mDelta;

	protected int mIndex;

	protected bool mActive = true;

	protected List<string> mSpriteNames = new List<string>();

	public int frames => mSpriteNames.Count;

	public int framesPerSecond
	{
		get
		{
			return mFPS;
		}
		set
		{
			mFPS = value;
		}
	}

	public string namePrefix
	{
		get
		{
			return mPrefix;
		}
		set
		{
			if (mPrefix != value)
			{
				mPrefix = value;
				RebuildSpriteList();
			}
		}
	}

	public bool loop
	{
		get
		{
			return mLoop;
		}
		set
		{
			mLoop = value;
		}
	}

	public bool isPlaying => mActive;

	protected virtual void Start()
	{
		RebuildSpriteList();
	}

	protected virtual void Update()
	{
		if (!mActive || mSpriteNames.Count <= 1 || !Application.isPlaying || mFPS <= 0)
		{
			return;
		}
		mDelta += Mathf.Min(1f, RealTime.deltaTime);
		float num = 1f / (float)mFPS;
		while (num < mDelta)
		{
			mDelta = ((!(num > 0f)) ? 0f : (mDelta - num));
			if (++mIndex >= mSpriteNames.Count)
			{
				mIndex = 0;
				mActive = mLoop;
			}
			if (mActive)
			{
				mSprite.spriteName = mSpriteNames[mIndex];
				if (mSnap)
				{
					mSprite.MakePixelPerfect();
				}
			}
		}
	}

	public void RebuildSpriteList()
	{
		if ((Object)(object)mSprite == (Object)null)
		{
			mSprite = ((Component)this).GetComponent<UISprite>();
		}
		mSpriteNames.Clear();
		if (!((Object)(object)mSprite != (Object)null) || !((Object)(object)mSprite.atlas != (Object)null))
		{
			return;
		}
		List<UISpriteData> spriteList = mSprite.atlas.spriteList;
		int i = 0;
		for (int count = spriteList.Count; i < count; i++)
		{
			UISpriteData uISpriteData = spriteList[i];
			if (string.IsNullOrEmpty(mPrefix) || uISpriteData.name.StartsWith(mPrefix))
			{
				mSpriteNames.Add(uISpriteData.name);
			}
		}
		mSpriteNames.Sort();
	}

	public void Play()
	{
		mActive = true;
	}

	public void Pause()
	{
		mActive = false;
	}

	public void ResetToBeginning()
	{
		mActive = true;
		mIndex = 0;
		if ((Object)(object)mSprite != (Object)null && mSpriteNames.Count > 0)
		{
			mSprite.spriteName = mSpriteNames[mIndex];
			if (mSnap)
			{
				mSprite.MakePixelPerfect();
			}
		}
	}
}
