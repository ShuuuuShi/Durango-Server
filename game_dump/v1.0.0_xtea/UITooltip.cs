using System;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Tooltip")]
public class UITooltip : MonoBehaviour
{
	protected static UITooltip mInstance;

	public Camera uiCamera;

	public UILabel text;

	public GameObject tooltipRoot;

	public UISprite background;

	public float appearSpeed = 10f;

	public bool scalingTransitions = true;

	protected GameObject mTooltip;

	protected Transform mTrans;

	protected float mTarget;

	protected float mCurrent;

	protected Vector3 mPos;

	protected Vector3 mSize = Vector3.zero;

	protected UIWidget[] mWidgets;

	public static bool isVisible => (Object)(object)mInstance != (Object)null && mInstance.mTarget == 1f;

	private void Awake()
	{
		mInstance = this;
	}

	private void OnDestroy()
	{
		mInstance = null;
	}

	protected virtual void Start()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		mTrans = ((Component)this).transform;
		mWidgets = ((Component)this).GetComponentsInChildren<UIWidget>();
		mPos = mTrans.localPosition;
		if ((Object)(object)uiCamera == (Object)null)
		{
			uiCamera = NGUITools.FindCameraForLayer(((Component)this).gameObject.layer);
		}
		SetAlpha(0f);
	}

	protected virtual void Update()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mTooltip != (Object)(object)UICamera.tooltipObject)
		{
			mTooltip = null;
			mTarget = 0f;
		}
		if (mCurrent != mTarget)
		{
			mCurrent = Mathf.Lerp(mCurrent, mTarget, RealTime.deltaTime * appearSpeed);
			if (Mathf.Abs(mCurrent - mTarget) < 0.001f)
			{
				mCurrent = mTarget;
			}
			SetAlpha(mCurrent * mCurrent);
			if (scalingTransitions)
			{
				Vector3 val = mSize * 0.25f;
				val.y = 0f - val.y;
				Vector3 localScale = Vector3.one * (1.5f - mCurrent * 0.5f);
				Vector3 localPosition = Vector3.Lerp(mPos - val, mPos, mCurrent);
				mTrans.localPosition = localPosition;
				mTrans.localScale = localScale;
			}
		}
	}

	protected virtual void SetAlpha(float val)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int num = mWidgets.Length; i < num; i++)
		{
			UIWidget uIWidget = mWidgets[i];
			Color color = uIWidget.color;
			color.a = val;
			uIWidget.color = color;
		}
	}

	protected virtual void SetText(string tooltipText)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)text != (Object)null && !string.IsNullOrEmpty(tooltipText))
		{
			mTarget = 1f;
			mTooltip = UICamera.tooltipObject;
			text.text = tooltipText;
			mPos = Vector2.op_Implicit(UICamera.lastEventPosition);
			Transform transform = ((Component)text).transform;
			Vector3 localPosition = transform.localPosition;
			Vector3 localScale = transform.localScale;
			mSize = Vector2.op_Implicit(text.printedSize);
			ref Vector3 reference = ref mSize;
			reference.x *= localScale.x;
			ref Vector3 reference2 = ref mSize;
			reference2.y *= localScale.y;
			if ((Object)(object)background != (Object)null)
			{
				Vector4 border = background.border;
				ref Vector3 reference3 = ref mSize;
				reference3.x += border.x + border.z + (localPosition.x - border.x) * 2f;
				ref Vector3 reference4 = ref mSize;
				reference4.y += border.y + border.w + (0f - localPosition.y - border.y) * 2f;
				background.width = Mathf.RoundToInt(mSize.x);
				background.height = Mathf.RoundToInt(mSize.y);
			}
			if ((Object)(object)uiCamera != (Object)null)
			{
				mPos.x = Mathf.Clamp01(mPos.x / (float)Screen.width);
				mPos.y = Mathf.Clamp01(mPos.y / (float)Screen.height);
				float num = uiCamera.orthographicSize / mTrans.parent.lossyScale.y;
				float num2 = (float)Screen.height * 0.5f / num;
				Vector2 val = default(Vector2);
				((Vector2)(ref val))._002Ector(num2 * mSize.x / (float)Screen.width, num2 * mSize.y / (float)Screen.height);
				mPos.x = Mathf.Min(mPos.x, 1f - val.x);
				mPos.y = Mathf.Max(mPos.y, val.y);
				mTrans.position = uiCamera.ViewportToWorldPoint(mPos);
				mPos = mTrans.localPosition;
				mPos.x = Mathf.Round(mPos.x);
				mPos.y = Mathf.Round(mPos.y);
			}
			else
			{
				if (mPos.x + mSize.x > (float)Screen.width)
				{
					mPos.x = (float)Screen.width - mSize.x;
				}
				if (mPos.y - mSize.y < 0f)
				{
					mPos.y = mSize.y;
				}
				ref Vector3 reference5 = ref mPos;
				reference5.x -= (float)Screen.width * 0.5f;
				ref Vector3 reference6 = ref mPos;
				reference6.y -= (float)Screen.height * 0.5f;
			}
			mTrans.localPosition = mPos;
			if ((Object)(object)tooltipRoot != (Object)null)
			{
				tooltipRoot.BroadcastMessage("UpdateAnchors");
			}
			else
			{
				((Component)text).BroadcastMessage("UpdateAnchors");
			}
		}
		else
		{
			mTooltip = null;
			mTarget = 0f;
		}
	}

	[Obsolete("Use UITooltip.Show instead")]
	public static void ShowText(string text)
	{
		if ((Object)(object)mInstance != (Object)null)
		{
			mInstance.SetText(text);
		}
	}

	public static void Show(string text)
	{
		if ((Object)(object)mInstance != (Object)null)
		{
			mInstance.SetText(text);
		}
	}

	public static void Hide()
	{
		if ((Object)(object)mInstance != (Object)null)
		{
			mInstance.mTooltip = null;
			mInstance.mTarget = 0f;
		}
	}
}
