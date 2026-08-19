using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Label")]
public class UILabel : UIWidget, RectLayout.ICompatible
{
	public enum Effect
	{
		None,
		Shadow,
		Outline,
		OutlineShadow
	}

	public enum Overflow
	{
		ShrinkContent,
		ClampContent,
		ResizeFreely,
		ResizeHeight
	}

	public enum Modifier
	{
		None = 0,
		ToUppercase = 1,
		ToLowercase = 2,
		Custom = 255
	}

	public delegate string ModifierFunc(string s);

	[HideInInspector]
	[SerializeField]
	private Font mTrueTypeFont;

	[HideInInspector]
	[SerializeField]
	private UIFont mFont;

	[Multiline(6)]
	[HideInInspector]
	[SerializeField]
	private string mText = string.Empty;

	[HideInInspector]
	[SerializeField]
	private int mFontSize = 16;

	[HideInInspector]
	[SerializeField]
	private FontStyle mFontStyle;

	[HideInInspector]
	[SerializeField]
	private NGUIText.Alignment mAlignment;

	[HideInInspector]
	[SerializeField]
	private bool mEncoding = true;

	[HideInInspector]
	[SerializeField]
	private int mMaxLineCount;

	[HideInInspector]
	[SerializeField]
	private Effect mEffectStyle;

	[HideInInspector]
	[SerializeField]
	private Color mEffectColor = Color.black;

	[HideInInspector]
	[SerializeField]
	private Vector2 mEffectDistance = Vector2.one;

	[HideInInspector]
	[SerializeField]
	private Overflow mOverflow;

	[HideInInspector]
	[SerializeField]
	private int mMinFontSize = 1;

	[HideInInspector]
	[SerializeField]
	private Material mMaterial;

	[HideInInspector]
	[SerializeField]
	private bool mApplyGradient;

	[HideInInspector]
	[SerializeField]
	private Color mGradientTop = Color.white;

	[HideInInspector]
	[SerializeField]
	private Color mGradientBottom = new Color(0.7f, 0.7f, 0.7f);

	[HideInInspector]
	[SerializeField]
	private int mSpacingX;

	[HideInInspector]
	[SerializeField]
	private int mSpacingY = 6;

	[HideInInspector]
	[SerializeField]
	private bool mUseFloatSpacing;

	[HideInInspector]
	[SerializeField]
	private float mFloatSpacingX;

	[HideInInspector]
	[SerializeField]
	private float mFloatSpacingY;

	[HideInInspector]
	[SerializeField]
	private bool mOverflowEllipsis;

	[HideInInspector]
	[SerializeField]
	private int mOverflowWidth;

	[HideInInspector]
	[SerializeField]
	private Modifier mModifier;

	[HideInInspector]
	[SerializeField]
	private bool mWrapAlways;

	[HideInInspector]
	[SerializeField]
	private bool mGettext;

	[HideInInspector]
	[SerializeField]
	private string mContext = string.Empty;

	[HideInInspector]
	[SerializeField]
	private string mComment = string.Empty;

	[NonSerialized]
	private bool mShouldBeProcessed = true;

	[NonSerialized]
	private bool mPremultiply;

	[NonSerialized]
	private Vector2 mCalculatedSize = Vector2.zero;

	[NonSerialized]
	private float mScale = 1f;

	[NonSerialized]
	private int mLastWidth;

	[NonSerialized]
	private int mLastHeight;

	[NonSerialized]
	private int mFontVersion;

	private readonly TextBuilder.TextTokens _processedTokens = new TextBuilder.TextTokens();

	private readonly TextBuilder.TextTokens _tokens = new TextBuilder.TextTokens();

	[NonSerialized]
	private SyncString _syncString;

	[NonSerialized]
	private float _updateAt;

	public ModifierFunc customModifier;

	private static BetterList<UILabel> mList = new BetterList<UILabel>();

	private static bool mTexRebuildAdded = false;

	private static int fontVersion = 0;

	private static BetterList<Vector3> mTempVerts = new BetterList<Vector3>();

	private static BetterList<int> mTempIndices = new BetterList<int>();

	public int finalFontSize
	{
		get
		{
			if ((bool)trueTypeFont)
			{
				return Mathf.RoundToInt(mScale * (float)fontSize);
			}
			return Mathf.RoundToInt((float)fontSize * mScale);
		}
	}

	private bool shouldBeProcessed
	{
		get
		{
			return mShouldBeProcessed;
		}
		set
		{
			if (value)
			{
				mChanged = true;
				mShouldBeProcessed = true;
			}
			else
			{
				mShouldBeProcessed = false;
			}
		}
	}

	public override bool isAnchoredHorizontally => base.isAnchoredHorizontally || mOverflow == Overflow.ResizeFreely;

	public override bool isAnchoredVertically => base.isAnchoredVertically || mOverflow == Overflow.ResizeFreely || mOverflow == Overflow.ResizeHeight;

	public override Material material
	{
		get
		{
			if (mMaterial != null)
			{
				return mMaterial;
			}
			if (mFont != null)
			{
				return mFont.material;
			}
			if (mTrueTypeFont != null)
			{
				return mTrueTypeFont.material;
			}
			return null;
		}
		set
		{
			if (mMaterial != value)
			{
				RemoveFromPanel();
				mMaterial = value;
				MarkAsChanged();
			}
		}
	}

	public UIFont bitmapFont
	{
		get
		{
			return mFont;
		}
		set
		{
			if (mFont != value)
			{
				RemoveFromPanel();
				mFont = value;
				mTrueTypeFont = null;
				MarkAsChanged();
			}
		}
	}

	public Font trueTypeFont
	{
		get
		{
			if (mTrueTypeFont != null)
			{
				return mTrueTypeFont;
			}
			return (!(mFont != null)) ? null : mFont.dynamicFont;
		}
		set
		{
			if (mTrueTypeFont != value)
			{
				RemoveFromPanel();
				mTrueTypeFont = value;
				shouldBeProcessed = true;
				mFont = null;
				ProcessAndRequest();
			}
		}
	}

	public UnityEngine.Object ambigiousFont
	{
		get
		{
			return (UnityEngine.Object)(((object)mFont) ?? ((object)mTrueTypeFont));
		}
		set
		{
			UIFont uIFont = value as UIFont;
			if (uIFont != null)
			{
				bitmapFont = uIFont;
			}
			else
			{
				trueTypeFont = value as Font;
			}
		}
	}

	public string text
	{
		get
		{
			return mText;
		}
		set
		{
			_updateAt = 0f;
			if (mText == value)
			{
				return;
			}
			_tokens.Clear();
			if (string.IsNullOrEmpty(value))
			{
				if (!string.IsNullOrEmpty(mText))
				{
					mText = string.Empty;
					MarkAsChanged();
					ProcessAndRequest();
				}
			}
			else if (mText != value)
			{
				mText = value;
				MarkAsChanged();
				ProcessAndRequest();
			}
			if (autoResizeBoxCollider)
			{
				ResizeCollider();
			}
		}
	}

	public bool useGettext
	{
		get
		{
			return mGettext;
		}
		set
		{
			mGettext = value;
		}
	}

	public string comment
	{
		get
		{
			return mComment;
		}
		set
		{
			mComment = value;
		}
	}

	public int defaultFontSize => (trueTypeFont != null) ? mFontSize : ((!(mFont != null)) ? 16 : mFont.defaultSize);

	public int fontSize
	{
		get
		{
			return mFontSize;
		}
		set
		{
			value = Mathf.Clamp(value, 0, 256);
			if (mFontSize != value)
			{
				mFontSize = value;
				_tokens.Clear();
				shouldBeProcessed = true;
				ProcessAndRequest();
			}
		}
	}

	public int minFontSize
	{
		get
		{
			return mMinFontSize;
		}
		set
		{
			if (mMinFontSize != value)
			{
				mMinFontSize = value;
				shouldBeProcessed = true;
				ProcessAndRequest();
			}
		}
	}

	public FontStyle fontStyle
	{
		get
		{
			return mFontStyle;
		}
		set
		{
			if (mFontStyle != value)
			{
				mFontStyle = value;
				shouldBeProcessed = true;
				ProcessAndRequest();
			}
		}
	}

	public NGUIText.Alignment alignment
	{
		get
		{
			return mAlignment;
		}
		set
		{
			if (mAlignment != value)
			{
				mAlignment = value;
				shouldBeProcessed = true;
				ProcessAndRequest();
			}
		}
	}

	public bool applyGradient
	{
		get
		{
			return mApplyGradient;
		}
		set
		{
			if (mApplyGradient != value)
			{
				mApplyGradient = value;
				MarkAsChanged();
			}
		}
	}

	public Color gradientTop
	{
		get
		{
			return mGradientTop;
		}
		set
		{
			if (mGradientTop != value)
			{
				mGradientTop = value;
				if (mApplyGradient)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public Color gradientBottom
	{
		get
		{
			return mGradientBottom;
		}
		set
		{
			if (mGradientBottom != value)
			{
				mGradientBottom = value;
				if (mApplyGradient)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public int spacingX
	{
		get
		{
			return mSpacingX;
		}
		set
		{
			if (mSpacingX != value)
			{
				mSpacingX = value;
				MarkAsChanged();
			}
		}
	}

	public int spacingY
	{
		get
		{
			return mSpacingY;
		}
		set
		{
			if (mSpacingY != value)
			{
				mSpacingY = value;
				MarkAsChanged();
			}
		}
	}

	public bool useFloatSpacing
	{
		get
		{
			return mUseFloatSpacing;
		}
		set
		{
			if (mUseFloatSpacing != value)
			{
				mUseFloatSpacing = value;
				shouldBeProcessed = true;
			}
		}
	}

	public float floatSpacingX
	{
		get
		{
			return mFloatSpacingX;
		}
		set
		{
			if (!Mathf.Approximately(mFloatSpacingX, value))
			{
				mFloatSpacingX = value;
				MarkAsChanged();
			}
		}
	}

	public float floatSpacingY
	{
		get
		{
			return mFloatSpacingY;
		}
		set
		{
			if (!Mathf.Approximately(mFloatSpacingY, value))
			{
				mFloatSpacingY = value;
				MarkAsChanged();
			}
		}
	}

	public float effectiveSpacingY => (!mUseFloatSpacing) ? ((float)mSpacingY) : mFloatSpacingY;

	public float effectiveSpacingX => (!mUseFloatSpacing) ? ((float)mSpacingX) : mFloatSpacingX;

	public bool overflowEllipsis
	{
		get
		{
			return mOverflowEllipsis;
		}
		set
		{
			if (mOverflowEllipsis != value)
			{
				mOverflowEllipsis = value;
				MarkAsChanged();
			}
		}
	}

	public int overflowWidth
	{
		get
		{
			return mOverflowWidth;
		}
		set
		{
			if (mOverflowWidth != value)
			{
				mOverflowWidth = value;
				MarkAsChanged();
			}
		}
	}

	public bool supportEncoding
	{
		get
		{
			return mEncoding;
		}
		set
		{
			if (mEncoding != value)
			{
				mEncoding = value;
				shouldBeProcessed = true;
			}
		}
	}

	public Overflow overflowMethod
	{
		get
		{
			return mOverflow;
		}
		set
		{
			if (mOverflow != value)
			{
				mOverflow = value;
				shouldBeProcessed = true;
			}
		}
	}

	public bool multiLine
	{
		get
		{
			return mMaxLineCount != 1;
		}
		set
		{
			if (mMaxLineCount != 1 != value)
			{
				mMaxLineCount = ((!value) ? 1 : 0);
				shouldBeProcessed = true;
			}
		}
	}

	public override Vector3[] localCorners
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return base.localCorners;
		}
	}

	public override Vector3[] worldCorners
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return base.worldCorners;
		}
	}

	public override Vector4 drawingDimensions
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return base.drawingDimensions;
		}
	}

	public int maxLineCount
	{
		get
		{
			return mMaxLineCount;
		}
		set
		{
			if (mMaxLineCount != value)
			{
				mMaxLineCount = Mathf.Max(value, 0);
				shouldBeProcessed = true;
				if (overflowMethod == Overflow.ShrinkContent)
				{
					MakePixelPerfect();
				}
			}
		}
	}

	public Effect effectStyle
	{
		get
		{
			return mEffectStyle;
		}
		set
		{
			if (mEffectStyle != value)
			{
				mEffectStyle = value;
				shouldBeProcessed = true;
			}
		}
	}

	public Color effectColor
	{
		get
		{
			return mEffectColor;
		}
		set
		{
			if (mEffectColor != value)
			{
				mEffectColor = value;
				if (mEffectStyle != 0)
				{
					shouldBeProcessed = true;
				}
			}
		}
	}

	public Vector2 effectDistance
	{
		get
		{
			return mEffectDistance;
		}
		set
		{
			if (mEffectDistance != value)
			{
				mEffectDistance = value;
				shouldBeProcessed = true;
			}
		}
	}

	public TextBuilder.TextTokens ProcessedTokens
	{
		get
		{
			if (mLastWidth != mWidth || mLastHeight != mHeight)
			{
				mShouldBeProcessed = true;
			}
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return _processedTokens;
		}
	}

	public TextBuilder.TextTokens Tokens
	{
		get
		{
			if (_tokens.IsEmpty())
			{
				mFontVersion = fontVersion;
				mScale = 1f;
				string text = UILabelPreProcesser.PreProcessText(this, mText);
				switch (mModifier)
				{
				case Modifier.ToLowercase:
					text = text.ToLower();
					break;
				case Modifier.ToUppercase:
					text = text.ToUpper();
					break;
				case Modifier.Custom:
					if (customModifier != null)
					{
						text = customModifier(text);
					}
					break;
				}
				OnTextParseStart();
				using (TextBuilder textBuilder = GetTextBuilder())
				{
					if (mEncoding)
					{
						textBuilder.ParseText(text, _tokens, TryTextParse);
					}
					else
					{
						textBuilder.ParseText(text, _tokens, null);
					}
				}
				OnTextParseFinish();
			}
			return _tokens;
		}
	}

	public Vector2 printedSize
	{
		get
		{
			if (mLastWidth != mWidth || mLastHeight != mHeight)
			{
				mShouldBeProcessed = true;
			}
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return mCalculatedSize;
		}
	}

	public Vector2 FontOffset { get; private set; }

	public override Vector2 localSize
	{
		get
		{
			if (shouldBeProcessed)
			{
				ProcessText();
			}
			return base.localSize;
		}
	}

	private bool isValid => mFont != null || mTrueTypeFont != null;

	public Modifier modifier
	{
		get
		{
			return mModifier;
		}
		set
		{
			if (mModifier != value)
			{
				mModifier = value;
				MarkAsChanged();
				ProcessAndRequest();
			}
		}
	}

	public bool wrapAlways
	{
		get
		{
			return mWrapAlways;
		}
		set
		{
			if (mWrapAlways != value)
			{
				mWrapAlways = value;
				shouldBeProcessed = true;
				ProcessAndRequest();
			}
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		mList.Add(this);
		if (mFontVersion != fontVersion)
		{
			MarkAsChanged();
		}
	}

	protected override void OnDisable()
	{
		mList.Remove(this);
		base.OnDisable();
	}

	protected virtual void OnTextParseStart()
	{
	}

	protected virtual bool TryTextParse(string str, ref int index, TextBuilder builder, TextBuilder.TextTokens tokens)
	{
		return false;
	}

	protected virtual void OnTextParseFinish()
	{
	}

	private static void OnFontChanged(Font font)
	{
		fontVersion++;
		for (int i = 0; i < mList.size; i++)
		{
			UILabel uILabel = mList[i];
			if (!(uILabel == null))
			{
				uILabel.MarkAsChanged();
			}
		}
	}

	public override Vector3[] GetSides(Transform relativeTo)
	{
		if (shouldBeProcessed)
		{
			ProcessText();
		}
		return base.GetSides(relativeTo);
	}

	protected override void OnAnchor()
	{
		if (mOverflow == Overflow.ResizeFreely)
		{
			if (base.isFullyAnchored)
			{
				mOverflow = Overflow.ShrinkContent;
			}
		}
		else if (mOverflow == Overflow.ResizeHeight && topAnchor.target != null && bottomAnchor.target != null)
		{
			mOverflow = Overflow.ShrinkContent;
		}
		base.OnAnchor();
	}

	private void ProcessAndRequest()
	{
		if (ambigiousFont != null)
		{
			ProcessText();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!mTexRebuildAdded)
		{
			mTexRebuildAdded = true;
			Font.textureRebuilt += OnFontChanged;
		}
		CheckLocalized();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (_updateAt > 0f && _updateAt < Time.time)
		{
			UpdateSyncString();
		}
	}

	public void SetText(SyncString str)
	{
		_syncString = str;
		UpdateSyncString();
	}

	private void UpdateSyncString()
	{
		text = _syncString.Get(out var period);
		_updateAt = ((!(period > 0f)) ? 0f : (Time.time + period));
	}

	private void CheckLocalized()
	{
		if (Application.isPlaying && mGettext && !string.IsNullOrEmpty(mText))
		{
			text = LocalizeSystem.Get(mText);
			mGettext = false;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		mPremultiply = material != null && material.shader != null && material.shader.name.Contains("Premultiplied");
		ProcessAndRequest();
	}

	public override void MarkAsChanged()
	{
		_tokens.Clear();
		shouldBeProcessed = true;
		base.MarkAsChanged();
	}

	protected override void OnPivotChanged()
	{
		_tokens.Clear();
		shouldBeProcessed = true;
		base.OnPivotChanged();
	}

	public void ProcessText(TextBuilder builder = null)
	{
		_processedTokens.Clear();
		if (!isValid)
		{
			return;
		}
		TextBuilder.TextTokens tokens = Tokens;
		IDisposable disposable = null;
		if (builder == null)
		{
			builder = TextBuilder.Pop();
			disposable = builder;
		}
		using (disposable)
		{
			mChanged = true;
			shouldBeProcessed = false;
			float num = mDrawRegion.z - mDrawRegion.x;
			float num2 = mDrawRegion.w - mDrawRegion.y;
			builder.Width = ((num == 1f) ? base.width : Mathf.RoundToInt((float)base.width * num));
			builder.Height = ((num2 == 1f) ? base.height : Mathf.RoundToInt((float)base.height * num2));
			mScale = 1f;
			if (builder.Width < 1 || builder.Height < 0)
			{
				return;
			}
			if (disposable != null)
			{
				GetTextBuilder(builder);
			}
			if (fontSize > 0)
			{
				builder.Update(request: false);
				int minSize;
				switch (mOverflow)
				{
				case Overflow.ShrinkContent:
				case Overflow.ResizeFreely:
				case Overflow.ResizeHeight:
					minSize = Mathf.Max(2, mMinFontSize);
					break;
				default:
					minSize = fontSize;
					break;
				}
				int num3 = builder.ProcessText(tokens, _processedTokens, out mCalculatedSize, minSize, mOverflowEllipsis, mWrapAlways);
				mScale = (float)num3 / (float)fontSize;
				switch (mOverflow)
				{
				case Overflow.ResizeFreely:
					mWidth = Mathf.Max(minWidth, Mathf.RoundToInt(mCalculatedSize.x));
					if (num != 1f)
					{
						mWidth = Mathf.RoundToInt((float)mWidth / num);
					}
					mHeight = Mathf.Max(minHeight, Mathf.RoundToInt(mCalculatedSize.y));
					if (num2 != 1f)
					{
						mHeight = Mathf.RoundToInt((float)mHeight / num2);
					}
					if ((mWidth & 1) == 1)
					{
						mWidth++;
					}
					if ((mHeight & 1) == 1)
					{
						mHeight++;
					}
					break;
				case Overflow.ResizeHeight:
					mHeight = Mathf.Max(minHeight, Mathf.RoundToInt(mCalculatedSize.y));
					if (num2 != 1f)
					{
						mHeight = Mathf.RoundToInt((float)mHeight / num2);
					}
					if ((mHeight & 1) == 1)
					{
						mHeight++;
					}
					break;
				}
			}
			else
			{
				base.cachedTransform.localScale = Vector3.one;
				mScale = 1f;
			}
		}
		if (mLastWidth != mWidth || mLastHeight != mHeight)
		{
			_movedEventFlag = true;
		}
		Vector2 vector = base.pivotOffset;
		float x = Mathf.Lerp(0f, -mWidth, vector.x);
		float y = Mathf.Lerp(mHeight, 0f, vector.y) + Mathf.Lerp(mCalculatedSize.y - (float)mHeight, 0f, vector.y);
		FontOffset = new Vector3(x, y);
		mLastWidth = mWidth;
		mLastHeight = mHeight;
		OnProcessedText(_processedTokens);
	}

	protected virtual void OnProcessedText(TextBuilder.TextTokens tokens)
	{
	}

	public override void MakePixelPerfect()
	{
		if (ambigiousFont != null)
		{
			Vector3 localPosition = base.cachedTransform.localPosition;
			localPosition.x = Mathf.RoundToInt(localPosition.x);
			localPosition.y = Mathf.RoundToInt(localPosition.y);
			localPosition.z = Mathf.RoundToInt(localPosition.z);
			base.cachedTransform.localPosition = localPosition;
			base.cachedTransform.localScale = Vector3.one;
			if (mOverflow == Overflow.ResizeFreely)
			{
				AssumeNaturalSize();
				return;
			}
			int a = base.width;
			int a2 = base.height;
			Overflow overflow = mOverflow;
			if (overflow != Overflow.ResizeHeight)
			{
				mWidth = 100000;
			}
			mHeight = 100000;
			mOverflow = Overflow.ShrinkContent;
			ProcessText();
			mOverflow = overflow;
			int a3 = Mathf.RoundToInt(mCalculatedSize.x);
			int a4 = Mathf.RoundToInt(mCalculatedSize.y);
			a3 = Mathf.Max(a3, base.minWidth);
			a4 = Mathf.Max(a4, base.minHeight);
			if ((a3 & 1) == 1)
			{
				a3++;
			}
			if ((a4 & 1) == 1)
			{
				a4++;
			}
			mWidth = Mathf.Max(a, a3);
			mHeight = Mathf.Max(a2, a4);
			MarkAsChanged();
		}
		else
		{
			base.MakePixelPerfect();
		}
	}

	public void AssumeNaturalSize()
	{
		if (ambigiousFont != null)
		{
			mWidth = 100000;
			mHeight = 100000;
			ProcessText();
			mWidth = Mathf.RoundToInt(mCalculatedSize.x);
			mHeight = Mathf.RoundToInt(mCalculatedSize.y);
			if ((mWidth & 1) == 1)
			{
				mWidth++;
			}
			if ((mHeight & 1) == 1)
			{
				mHeight++;
			}
			MarkAsChanged();
		}
	}

	public int GetCharacterIndexAtPosition(Vector3 worldPos)
	{
		Vector2 localPos = base.cachedTransform.InverseTransformPoint(worldPos);
		return GetCharacterIndexAtPosition(localPos);
	}

	public int GetCharacterIndexAtPosition(Vector2 localPos)
	{
		if (isValid)
		{
			TextBuilder.TextTokens processedTokens = ProcessedTokens;
			if (processedTokens.Count == 0)
			{
				return 0;
			}
			using TextBuilder textBuilder = GetTextBuilder();
			textBuilder.PrintApproximateCharacterPositions(base.width, processedTokens, mTempVerts, mTempIndices);
			if (mTempVerts.size > 0)
			{
				ApplyOffset(mTempVerts, 0);
				int approximateCharacterIndex = NGUIText.GetApproximateCharacterIndex(mTempVerts, mTempIndices, localPos);
				mTempVerts.Clear();
				mTempIndices.Clear();
				return approximateCharacterIndex;
			}
		}
		return 0;
	}

	public int GetCharacterIndex(int currentIndex, KeyCode key)
	{
		if (isValid)
		{
			TextBuilder.TextTokens processedTokens = ProcessedTokens;
			if (processedTokens.Count == 0)
			{
				return 0;
			}
			int num = defaultFontSize;
			using (TextBuilder textBuilder = GetTextBuilder())
			{
				textBuilder.PrintApproximateCharacterPositions(base.width, processedTokens, mTempVerts, mTempIndices);
				if (mTempVerts.size > 0)
				{
					ApplyOffset(mTempVerts, 0);
					for (int i = 0; i < mTempIndices.size; i++)
					{
						if (mTempIndices[i] == currentIndex)
						{
							Vector2 pos = mTempVerts[i];
							switch (key)
							{
							case KeyCode.UpArrow:
								pos.y += (float)num + effectiveSpacingY;
								break;
							case KeyCode.DownArrow:
								pos.y -= (float)num + effectiveSpacingY;
								break;
							case KeyCode.Home:
								pos.x -= 1000f;
								break;
							case KeyCode.End:
								pos.x += 1000f;
								break;
							}
							int approximateCharacterIndex = NGUIText.GetApproximateCharacterIndex(mTempVerts, mTempIndices, pos);
							if (approximateCharacterIndex == currentIndex)
							{
								break;
							}
							mTempVerts.Clear();
							mTempIndices.Clear();
							return approximateCharacterIndex;
						}
					}
					mTempVerts.Clear();
					mTempIndices.Clear();
				}
			}
			switch (key)
			{
			case KeyCode.UpArrow:
			case KeyCode.Home:
				return 0;
			case KeyCode.DownArrow:
			case KeyCode.End:
				return text.Length;
			}
		}
		return currentIndex;
	}

	public void PrintOverlay(int start, int end, UIGeometry caret, UIGeometry highlight, Color caretColor, Color highlightColor)
	{
		caret?.Clear();
		highlight?.Clear();
		if (!isValid)
		{
			return;
		}
		TextBuilder.TextTokens processedTokens = ProcessedTokens;
		using TextBuilder textBuilder = GetTextBuilder();
		int size = caret.verts.size;
		Vector2 item = new Vector2(0.5f, 0.5f);
		float num = finalAlpha;
		if (highlight != null && start != end)
		{
			int size2 = highlight.verts.size;
			textBuilder.PrintCaretAndSelection(base.width, processedTokens, start, end, caret.verts, highlight.verts);
			if (highlight.verts.size > size2)
			{
				ApplyOffset(highlight.verts, size2);
				Color item2 = new Color(highlightColor.r, highlightColor.g, highlightColor.b, highlightColor.a * num);
				for (int i = size2; i < highlight.verts.size; i++)
				{
					highlight.uvs.Add(item);
					highlight.cols.Add(item2);
				}
			}
		}
		else
		{
			textBuilder.PrintCaretAndSelection(base.width, processedTokens, start, end, caret.verts, null);
		}
		ApplyOffset(caret.verts, size);
		Color item3 = new Color(caretColor.r, caretColor.g, caretColor.b, caretColor.a * num);
		for (int j = size; j < caret.verts.size; j++)
		{
			caret.uvs.Add(item);
			caret.cols.Add(item3);
		}
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		if (!isValid)
		{
			return;
		}
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		int num = verts.size;
		Color c = color;
		c.a = finalAlpha;
		if (mFont != null && mFont.premultipliedAlphaShader)
		{
			c = NGUITools.ApplyPMA(c);
		}
		int size = verts.size;
		TextBuilder.TextTokens processedTokens = ProcessedTokens;
		using (TextBuilder textBuilder = GetTextBuilder())
		{
			textBuilder.Build(processedTokens, c, base.width, verts, uvs, cols);
		}
		ApplyOffset(verts, size);
		if (mFont != null && mFont.packedFontShader)
		{
			return;
		}
		if (effectStyle != 0)
		{
			int size2 = verts.size;
			Vector2 vector = default(Vector2);
			vector.x = mEffectDistance.x;
			vector.y = mEffectDistance.y;
			float num2 = 1f;
			if (effectStyle == Effect.OutlineShadow)
			{
				num2 = 2f;
			}
			ApplyShadow(verts, uvs, cols, num, size2, vector.x * num2, (0f - vector.y) * num2);
			num = size2;
			if (effectStyle == Effect.Outline || effectStyle == Effect.OutlineShadow)
			{
				size2 = verts.size;
				ApplyShadow(verts, uvs, cols, num, size2, 0f - vector.x, vector.y);
				num = size2;
				size2 = verts.size;
				ApplyShadow(verts, uvs, cols, num, size2, vector.x, vector.y);
				num = size2;
				size2 = verts.size;
				ApplyShadow(verts, uvs, cols, num, size2, 0f - vector.x, 0f - vector.y);
				num = size2;
			}
		}
		onFillOffset = num;
		if (onPostFill != null)
		{
			onPostFill(this, num, arguments);
		}
	}

	public void ApplyOffset(BetterList<Vector3> verts, int start)
	{
		for (int i = start; i < verts.size; i++)
		{
			verts.buffer[i] += (Vector3)FontOffset;
		}
	}

	public void ApplyShadow(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, int start, int end, float x, float y)
	{
		Color color = mEffectColor;
		color.a *= finalAlpha;
		if (bitmapFont != null && bitmapFont.premultipliedAlphaShader)
		{
			color = NGUITools.ApplyPMA(color);
		}
		Color color2 = color.GammaToLinearSpace();
		for (int i = start; i < end; i++)
		{
			verts.Add(verts.buffer[i]);
			uvs.Add(uvs.buffer[i]);
			cols.Add(cols.buffer[i]);
			Vector3 vector = verts.buffer[i];
			vector.x += x;
			vector.y += y;
			verts.buffer[i] = vector;
			Color color3 = cols.buffer[i];
			if (color3.a == 1f)
			{
				cols.buffer[i] = color2;
				continue;
			}
			Color c = color;
			c.a = color3.a * color.a;
			ref Color reference = ref cols.buffer[i];
			reference = c.GammaToLinearSpace();
		}
	}

	public int CalculateOffsetToFit(string text)
	{
		using TextBuilder textBuilder = GetTextBuilder();
		textBuilder.Encoding = false;
		return textBuilder.CalculateOffsetToFit(text);
	}

	public void SetCurrentProgress()
	{
		if (UIProgressBar.current != null)
		{
			text = UIProgressBar.current.value.ToString("F");
		}
	}

	public void SetCurrentPercent()
	{
		if (UIProgressBar.current != null)
		{
			text = Mathf.RoundToInt(UIProgressBar.current.value * 100f) + "%";
		}
	}

	public void SetCurrentSelection()
	{
		if (UIPopupList.current != null)
		{
			text = ((!UIPopupList.current.isLocalized) ? UIPopupList.current.value : Localization.Get(UIPopupList.current.value));
		}
	}

	protected TextBuilder GetTextBuilder(TextBuilder builder = null)
	{
		if (builder == null)
		{
			builder = TextBuilder.Pop();
		}
		Font font = trueTypeFont;
		builder.FontSize = fontSize;
		builder.FontStyle = mFontStyle;
		builder.Width = Mathf.RoundToInt((float)mWidth * (mDrawRegion.z - mDrawRegion.x));
		builder.Height = Mathf.RoundToInt((float)mHeight * (mDrawRegion.w - mDrawRegion.y));
		builder.Gradient = mApplyGradient && (mFont == null || !mFont.packedFontShader);
		builder.GradientTop = mGradientTop;
		builder.GradientBottom = mGradientBottom;
		builder.Encoding = mEncoding;
		builder.Premultiply = mPremultiply;
		builder.MaxLines = mMaxLineCount;
		builder.SpacingX = effectiveSpacingX;
		builder.SpacingY = effectiveSpacingY;
		builder.FontScale = mScale;
		builder.Font = font;
		if (mOverflow == Overflow.ResizeFreely)
		{
			builder.Width = 1000000;
			if (mOverflowWidth > 0)
			{
				builder.Width = Mathf.Min(builder.Width, mOverflowWidth);
			}
		}
		if (mOverflow == Overflow.ResizeFreely || mOverflow == Overflow.ResizeHeight)
		{
			builder.Height = 1000000;
		}
		switch (alignment)
		{
		case NGUIText.Alignment.Automatic:
			switch (base.pivot)
			{
			case Pivot.TopLeft:
			case Pivot.Left:
			case Pivot.BottomLeft:
				builder.Alignment = 0f;
				break;
			case Pivot.TopRight:
			case Pivot.Right:
			case Pivot.BottomRight:
				builder.Alignment = 1f;
				break;
			default:
				builder.Alignment = 0.5f;
				break;
			}
			break;
		case NGUIText.Alignment.Center:
			builder.Alignment = 0.5f;
			break;
		case NGUIText.Alignment.Right:
			builder.Alignment = 1f;
			break;
		default:
			builder.Alignment = 0f;
			break;
		}
		builder.Update(request: true);
		return builder;
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused && mTrueTypeFont != null)
		{
			Invalidate(includeChildren: false);
		}
	}

	Vector2 RectLayout.ICompatible.UpdateLayout(float? w, float? h)
	{
		switch (overflowMethod)
		{
		case Overflow.ResizeHeight:
			if (w.HasValue)
			{
				base.width = (int)w.Value;
				if (shouldBeProcessed)
				{
					ProcessText();
				}
			}
			break;
		}
		return new Vector2((!w.HasValue) ? ((float)base.width) : w.Value, (!h.HasValue) ? ((float)base.height) : h.Value);
	}
}
