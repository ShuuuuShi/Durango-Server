using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Typewriter Effect")]
[RequireComponent(typeof(UILabel))]
public class TypewriterEffect : MonoBehaviour
{
	private struct FadeEntry
	{
		public int index;

		public string text;

		public float alpha;
	}

	public static TypewriterEffect current;

	public int charsPerSecond = 20;

	public float fadeInTime;

	public float delayOnPeriod;

	public float delayOnNewLine;

	public UIScrollView scrollView;

	public bool keepFullDimensions;

	public List<EventDelegate> onFinished = new List<EventDelegate>();

	private UILabel mLabel;

	private string mFullText = string.Empty;

	private int mCurrentOffset;

	private float mNextChar;

	private bool mReset = true;

	private bool mActive;

	private BetterList<FadeEntry> mFade = new BetterList<FadeEntry>();

	public bool isActive => mActive;

	public void ResetToBeginning()
	{
		Finish();
		mReset = true;
		mActive = true;
		mNextChar = 0f;
		mCurrentOffset = 0;
		Update();
	}

	public void Finish()
	{
		if (mActive)
		{
			mActive = false;
			if (!mReset)
			{
				mCurrentOffset = mFullText.Length;
				mFade.Clear();
				mLabel.text = mFullText;
			}
			if (keepFullDimensions && (Object)(object)scrollView != (Object)null)
			{
				scrollView.UpdatePosition();
			}
			current = this;
			EventDelegate.Execute(onFinished);
			current = null;
		}
	}

	private void OnEnable()
	{
		mReset = true;
		mActive = true;
	}

	private void OnDisable()
	{
		Finish();
	}

	private void Update()
	{
		if (!mActive)
		{
			return;
		}
		if (mReset)
		{
			mCurrentOffset = 0;
			mReset = false;
			mLabel = ((Component)this).GetComponent<UILabel>();
			mFullText = mLabel.processedText;
			mFade.Clear();
			if (keepFullDimensions && (Object)(object)scrollView != (Object)null)
			{
				scrollView.UpdatePosition();
			}
		}
		if (string.IsNullOrEmpty(mFullText))
		{
			return;
		}
		while (mCurrentOffset < mFullText.Length && mNextChar <= RealTime.time)
		{
			int num = mCurrentOffset;
			charsPerSecond = Mathf.Max(1, charsPerSecond);
			if (mLabel.supportEncoding)
			{
				while (NGUIText.ParseSymbol(mFullText, ref mCurrentOffset))
				{
				}
			}
			mCurrentOffset++;
			if (mCurrentOffset > mFullText.Length)
			{
				break;
			}
			float num2 = 1f / (float)charsPerSecond;
			char c = ((num >= mFullText.Length) ? '\n' : mFullText[num]);
			if (c == '\n')
			{
				num2 += delayOnNewLine;
			}
			else if (num + 1 == mFullText.Length || mFullText[num + 1] <= ' ')
			{
				switch (c)
				{
				case '.':
					if (num + 2 < mFullText.Length && mFullText[num + 1] == '.' && mFullText[num + 2] == '.')
					{
						num2 += delayOnPeriod * 3f;
						num += 2;
					}
					else
					{
						num2 += delayOnPeriod;
					}
					break;
				case '!':
				case '?':
					num2 += delayOnPeriod;
					break;
				}
			}
			if (mNextChar == 0f)
			{
				mNextChar = RealTime.time + num2;
			}
			else
			{
				mNextChar += num2;
			}
			if (fadeInTime != 0f)
			{
				FadeEntry item = default(FadeEntry);
				item.index = num;
				item.alpha = 0f;
				item.text = mFullText.Substring(num, mCurrentOffset - num);
				mFade.Add(item);
			}
			else
			{
				mLabel.text = ((!keepFullDimensions) ? mFullText.Substring(0, mCurrentOffset) : (mFullText.Substring(0, mCurrentOffset) + "[00]" + mFullText.Substring(mCurrentOffset)));
				if (!keepFullDimensions && (Object)(object)scrollView != (Object)null)
				{
					scrollView.UpdatePosition();
				}
			}
		}
		if (mCurrentOffset >= mFullText.Length)
		{
			mLabel.text = mFullText;
			current = this;
			EventDelegate.Execute(onFinished);
			current = null;
			mActive = false;
		}
		else
		{
			if (mFade.size == 0)
			{
				return;
			}
			int num3 = 0;
			while (num3 < mFade.size)
			{
				FadeEntry value = mFade[num3];
				value.alpha += RealTime.deltaTime / fadeInTime;
				if (value.alpha < 1f)
				{
					mFade[num3] = value;
					num3++;
				}
				else
				{
					mFade.RemoveAt(num3);
				}
			}
			if (mFade.size == 0)
			{
				if (keepFullDimensions)
				{
					mLabel.text = mFullText.Substring(0, mCurrentOffset) + "[00]" + mFullText.Substring(mCurrentOffset);
				}
				else
				{
					mLabel.text = mFullText.Substring(0, mCurrentOffset);
				}
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < mFade.size; i++)
			{
				FadeEntry fadeEntry = mFade[i];
				if (i == 0)
				{
					stringBuilder.Append(mFullText.Substring(0, fadeEntry.index));
				}
				stringBuilder.Append('[');
				stringBuilder.Append(NGUIText.EncodeAlpha(fadeEntry.alpha));
				stringBuilder.Append(']');
				stringBuilder.Append(fadeEntry.text);
			}
			if (keepFullDimensions)
			{
				stringBuilder.Append("[00]");
				stringBuilder.Append(mFullText.Substring(mCurrentOffset));
			}
			mLabel.text = stringBuilder.ToString();
		}
	}
}
[RequireComponent(typeof(UILabel))]
public class TypeWriterEffect : MonoBehaviour
{
	private UILabel _label;

	private UISpriteLabel _spriteLabel;

	private bool _volatility;

	private int _commentTypingCount;

	private int _maxTypingCount;

	private float _typeAt;

	private bool _enabled;

	public float TypingSpeed { get; set; }

	public event Action Finished;

	public void Reset()
	{
		_commentTypingCount = 0;
		_maxTypingCount = -1;
		_typeAt = Time.time;
		if (_enabled)
		{
			_label.MarkAsChanged();
		}
	}

	private void OnEnable()
	{
		_enabled = true;
		_label = ((Component)this).GetComponent<UILabel>();
		_spriteLabel = ((Component)this).GetComponent<UISpriteLabel>();
		if ((Object)(object)_spriteLabel != (Object)null)
		{
			_spriteLabel.onFill = OnPostFill;
		}
		else
		{
			_label.onPostFill = OnPostFill;
		}
		Reset();
	}

	private void OnDisable()
	{
		_enabled = false;
		if ((Object)(object)_spriteLabel != (Object)null)
		{
			_spriteLabel.onFill = null;
		}
		else
		{
			_label.onPostFill = null;
		}
		_label.MarkAsChanged();
		if (this.Finished != null)
		{
			this.Finished();
		}
		if (_volatility)
		{
			Object.Destroy((Object)(object)this);
		}
	}

	private void Update()
	{
		if (_maxTypingCount < 0)
		{
			return;
		}
		if (_commentTypingCount < _maxTypingCount)
		{
			float time = Time.time;
			float num = time - _typeAt;
			if (num > TypingSpeed)
			{
				if (TypingSpeed > 0f)
				{
					_commentTypingCount += (int)(num / TypingSpeed);
					_typeAt = time - num % TypingSpeed;
				}
				else
				{
					_commentTypingCount = _maxTypingCount;
				}
				_label.MarkAsChanged();
			}
		}
		else
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void OnPostFill(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		_maxTypingCount = verts.size / 4;
		switch (_label.effectStyle)
		{
		case UILabel.Effect.Shadow:
			_maxTypingCount /= 2;
			break;
		case UILabel.Effect.Outline:
			_maxTypingCount /= 5;
			break;
		case UILabel.Effect.Outline8:
		case UILabel.Effect.OutlineShadow:
			_maxTypingCount /= 9;
			break;
		}
		for (int i = _commentTypingCount; i < _maxTypingCount; i++)
		{
			for (int j = i * 4; j < cols.size; j += _maxTypingCount * 4)
			{
				for (int k = 0; k < 4; k++)
				{
					Color value = cols[j + k];
					value.a = 0f;
					cols[j + k] = value;
				}
			}
		}
	}

	private void OnPostFill(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, ref bool isOverrideAlpha)
	{
		OnPostFill(widget, bufferOffset, verts, uvs, cols);
		isOverrideAlpha = true;
	}

	public static TypeWriterEffect Begin(UILabel label, float typingSpeed = 0.1f, Action onFinish = null)
	{
		((Component)(object)label).SetEnable<TypeWriterEffect>(enable: false);
		TypeWriterEffect typeWriterEffect = ((Component)label).gameObject.AddComponent<TypeWriterEffect>();
		typeWriterEffect.TypingSpeed = typingSpeed;
		typeWriterEffect.Finished = onFinish;
		typeWriterEffect._volatility = true;
		return typeWriterEffect;
	}
}
