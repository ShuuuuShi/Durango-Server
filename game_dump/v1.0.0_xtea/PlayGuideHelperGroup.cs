using JetBrains.Annotations;
using PlayGuide;
using UnityEngine;

public class PlayGuideHelperGroup : UIBase
{
	[SerializeField]
	private Transform _arrowTargetTrans;

	[SerializeField]
	private UISprite _clickTargetHand;

	[SerializeField]
	private bool _arrowFixHeadDirWhenClosed = true;

	[SerializeField]
	private float _arrowScreenMargin = 100f;

	[SerializeField]
	private float _arrowAngleWithInScreen = 40f;

	private Vector3 _arrowTargetPos;

	private ClickTargetLocator _clickTargetLocator;

	private float _clickTargetPrevScale;

	private TweenScale _clickTargeTweenScale;

	private TweenAlpha _clickTargetTweenAlpha;

	private bool IsArrowTargetEnabled()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _arrowTargetPos != Vector3.zero;
	}

	private void LateUpdate()
	{
		ProcessArrowTarget();
		ProcessClickTarget();
	}

	private void ProcessArrowTarget()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		if (IsArrowTargetEnabled())
		{
			Vector3 world = _arrowTargetPos;
			Vector3 val = _arrowTargetPos - PlayerBehavior.LocalPlayer.CurrentPosition;
			if (((Vector3)(ref val)).magnitude > 3200f)
			{
				((Vector3)(ref val)).Normalize();
				world = PlayerBehavior.LocalPlayer.CurrentPosition + val * 3200f;
			}
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			Vector3 val3 = MainCamera.WorldToScreenPos(world);
			Vector3 val4 = val3 - val2;
			val4.z = 0f;
			float num = (64f + _arrowScreenMargin) / MainCamera.NGUIScale();
			bool flag;
			if (val3.x < num || val3.x > (float)Screen.width - num || val3.y < num || val3.y >= (float)Screen.height - num)
			{
				flag = false;
				float num2 = (float)Screen.width - num * 2f;
				float num3 = (float)Screen.height - num * 2f;
				float num4 = Mathf.Min(num2 / Mathf.Abs(val4.x), num3 / Mathf.Abs(val4.y));
				Vector3 nguiPos = val2;
				nguiPos.x += val4.x * 0.5f * num4;
				nguiPos.y += val4.y * 0.5f * num4;
				nguiPos.z = 0f;
				_arrowTargetTrans.localPosition = MainCamera.ScreenPosToNGUIPos(nguiPos);
			}
			else
			{
				flag = true;
				_arrowTargetTrans.localPosition = MainCamera.ScreenPosToNGUIPos(val3);
			}
			if (_arrowFixHeadDirWhenClosed && flag)
			{
				_arrowTargetTrans.localRotation = Quaternion.Euler(0f, 0f, _arrowAngleWithInScreen);
				return;
			}
			((Vector3)(ref val4)).Normalize();
			float num5 = Mathf.Atan2(val4.x, val4.y) * 57.29578f;
			num5 = 0f - num5;
			_arrowTargetTrans.localRotation = Quaternion.Euler(0f, 0f, num5);
		}
	}

	private void ProcessClickTarget()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if (_clickTargetLocator != null)
		{
			_clickTargetLocator.Process();
			((Component)_clickTargetHand).transform.localRotation = Quaternion.Euler(0f, (float)(_clickTargetLocator.IsFlip() ? 180 : 0), _clickTargetLocator.Rotate());
			bool activeSelf = ((Component)_clickTargetHand).gameObject.activeSelf;
			bool flag = _clickTargetLocator.IsVisible();
			((Component)_clickTargetHand).gameObject.SetActive(flag);
			if (!activeSelf && flag)
			{
				_clickTargetTweenAlpha.ResetToBeginning();
				_clickTargetTweenAlpha.PlayForward();
			}
			((Component)_clickTargetHand).transform.localPosition = GetCurrentClickTargetPos();
			float x = ((Component)_clickTargetHand).transform.localScale.x;
			bool flag2 = _clickTargetPrevScale > x;
			_clickTargetHand.spriteName = ((!flag2) ? "guide_hand_normal" : "guide_hand_touched");
			_clickTargetPrevScale = x;
		}
	}

	public void ShowArrowTargetIfEnabled(bool bVisible)
	{
		if (IsArrowTargetEnabled())
		{
			((Component)_arrowTargetTrans).gameObject.SetActive(bVisible);
		}
	}

	public void FinishArrowTargetIf(Vector3 target)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (IsArrowTargetEnabled())
		{
			Vector3 val = _arrowTargetPos - target;
			if (((Vector3)(ref val)).magnitude < 10f)
			{
				SetArrowTarget(Vector3.zero);
			}
		}
	}

	public void SetArrowTarget(Vector3 target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_arrowTargetPos = target;
		((Component)_arrowTargetTrans).gameObject.SetActive(IsArrowTargetEnabled());
	}

	public void EnableClickTarget([NotNull] ClickTargetLocator locator)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		_clickTargetLocator = locator;
		_clickTargetLocator.Process();
		_clickTargetHand.spriteName = "guide_hand_normal";
		Vector3 localPosition = default(Vector3);
		((Vector3)(ref localPosition))._002Ector(0f, (float)(-Screen.height) * 0.5f * MainCamera.NGUIScale(), 0f);
		((Component)_clickTargetHand).transform.localPosition = localPosition;
		((Component)_clickTargetHand).transform.localRotation = Quaternion.Euler(0f, (float)(_clickTargetLocator.IsFlip() ? 180 : 0), _clickTargetLocator.Rotate());
		((Component)_clickTargetHand).gameObject.SetActive(true);
		_clickTargetTweenAlpha = ((Component)_clickTargetHand).GetComponent<TweenAlpha>();
		((Behaviour)_clickTargetTweenAlpha).enabled = false;
		_clickTargeTweenScale = ((Component)_clickTargetHand).GetComponent<TweenScale>();
		_clickTargeTweenScale.PlayForward();
		_clickTargetPrevScale = ((Component)_clickTargetHand).transform.localScale.x;
	}

	private Vector3 GetCurrentClickTargetPos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 nGUIPosition = _clickTargetLocator.GetNGUIPosition();
		nGUIPosition.y -= 32f;
		Vector2 offset = _clickTargetLocator.GetOffset();
		return CalcRelativePos(nGUIPosition, offset);
	}

	private static Vector3 CalcRelativePos(Vector3 basePos, Vector2 offset)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		offset.x = offset.x * (float)Screen.width * MainCamera.NGUIScale();
		offset.y = offset.y * (float)Screen.height * MainCamera.NGUIScale();
		basePos.x += offset.x;
		basePos.y += offset.y;
		return basePos;
	}

	public void DisableClickTarget()
	{
		_clickTargetLocator = null;
		((Component)_clickTargetHand).gameObject.SetActive(false);
	}
}
