using UnityEngine;

public class UI2DSpriteAnimation : MonoBehaviour
{
	[SerializeField]
	protected int framerate = 20;

	public bool ignoreTimeScale = true;

	public bool loop = true;

	public Sprite[] frames;

	private SpriteRenderer mUnitySprite;

	private UI2DSprite mNguiSprite;

	private int mIndex;

	private float mUpdate;

	public bool isPlaying => ((Behaviour)this).enabled;

	public int framesPerSecond
	{
		get
		{
			return framerate;
		}
		set
		{
			framerate = value;
		}
	}

	public void Play()
	{
		if (frames == null || frames.Length <= 0)
		{
			return;
		}
		if (!((Behaviour)this).enabled && !loop)
		{
			int num = ((framerate <= 0) ? (mIndex - 1) : (mIndex + 1));
			if (num < 0 || num >= frames.Length)
			{
				mIndex = ((framerate < 0) ? (frames.Length - 1) : 0);
			}
		}
		((Behaviour)this).enabled = true;
		UpdateSprite();
	}

	public void Pause()
	{
		((Behaviour)this).enabled = false;
	}

	public void ResetToBeginning()
	{
		mIndex = ((framerate < 0) ? (frames.Length - 1) : 0);
		UpdateSprite();
	}

	private void Start()
	{
		Play();
	}

	private void Update()
	{
		if (frames == null || frames.Length == 0)
		{
			((Behaviour)this).enabled = false;
		}
		else
		{
			if (framerate == 0)
			{
				return;
			}
			float num = ((!ignoreTimeScale) ? Time.time : RealTime.time);
			if (mUpdate < num)
			{
				mUpdate = num;
				int num2 = ((framerate <= 0) ? (mIndex - 1) : (mIndex + 1));
				if (!loop && (num2 < 0 || num2 >= frames.Length))
				{
					((Behaviour)this).enabled = false;
					return;
				}
				mIndex = NGUIMath.RepeatIndex(num2, frames.Length);
				UpdateSprite();
			}
		}
	}

	private void UpdateSprite()
	{
		if ((Object)(object)mUnitySprite == (Object)null && (Object)(object)mNguiSprite == (Object)null)
		{
			mUnitySprite = ((Component)this).GetComponent<SpriteRenderer>();
			mNguiSprite = ((Component)this).GetComponent<UI2DSprite>();
			if ((Object)(object)mUnitySprite == (Object)null && (Object)(object)mNguiSprite == (Object)null)
			{
				((Behaviour)this).enabled = false;
				return;
			}
		}
		float num = ((!ignoreTimeScale) ? Time.time : RealTime.time);
		if (framerate != 0)
		{
			mUpdate = num + Mathf.Abs(1f / (float)framerate);
		}
		if ((Object)(object)mUnitySprite != (Object)null)
		{
			mUnitySprite.sprite = frames[mIndex];
		}
		else if ((Object)(object)mNguiSprite != (Object)null)
		{
			mNguiSprite.nextSprite = frames[mIndex];
		}
	}
}
