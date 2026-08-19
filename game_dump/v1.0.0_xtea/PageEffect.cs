using System;
using UnityEngine;

public class PageEffect : MonoBehaviour
{
	private readonly UIWidget[] _origins = new UIWidget[2];

	private readonly UIWidget[] _nexts = new UIWidget[2];

	private float _ratio;

	private UIPanel[] _pagePanel = new UIPanel[2];

	private Transform[] _pageContainer = (Transform[])(object)new Transform[2];

	private Transform[] _originParent = (Transform[])(object)new Transform[2];

	private Vector3[] _originPos = (Vector3[])(object)new Vector3[2];

	private UIWidget[] _pageChild = new UIWidget[2];

	private bool _leftToRight;

	private void Awake()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		UIPanel componentInParent = ((Component)this).GetComponentInParent<UIPanel>();
		int num = ((!((Object)(object)componentInParent == (Object)null)) ? componentInParent.depth : 0);
		for (int i = 0; i < _pagePanel.Length; i++)
		{
			UIPanel uIPanel = ((Component)this).gameObject.AddChild<UIPanel>();
			uIPanel.clipping = UIDrawCall.Clipping.SoftClip;
			uIPanel.clipSoftness = Vector2.zero;
			uIPanel.depth = num + 1;
			_pageContainer[i] = ((Component)uIPanel).gameObject.AddChild().transform;
			_pagePanel[i] = uIPanel;
		}
	}

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Drag(NGUIMath.ScreenToPixels(UICamera.lastEventPosition, ((Component)this).transform));
	}

	private void FindCornerPos(Vector2 touchPos, out Vector2 pos1, out Vector2 pos2, out bool bottom)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		UIWidget uIWidget = ((!_leftToRight) ? _origins[1] : _origins[0]);
		pos1 = touchPos;
		pos2 = Vector2.op_Implicit(Vector3.Lerp(uIWidget.localCorners[(!_leftToRight) ? 3 : 0], uIWidget.localCorners[_leftToRight ? 1 : 2], _ratio) + ((Component)uIWidget).transform.localPosition);
		Vector2 val = pos2 - pos1;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(0f - val.y, val.x);
		float num = Mathf.Atan2(val2.y, val2.x);
		float num2 = 2f * num - (float)Math.PI / 2f;
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(Mathf.Cos(num2), Mathf.Sin(num2));
		pos1 -= val3 * (float)_pageChild[0].height * _ratio;
		pos2 = Vector2.op_Implicit(uIWidget.localCorners[(!_leftToRight) ? 3 : 0] + ((Component)uIWidget).transform.localPosition);
		UIWidget component = ((Component)this).GetComponent<UIWidget>();
		Rect val4 = default(Rect);
		((Rect)(ref val4))._002Ector(Vector2.op_Implicit(component.localCorners[0]), component.localSize);
		bottom = (_leftToRight ? (pos1.x < ((Rect)(ref val4)).xMin || pos1.y < ((Rect)(ref val4)).yMin) : (pos1.x > ((Rect)(ref val4)).xMax || pos1.y < ((Rect)(ref val4)).yMin));
		if (bottom)
		{
			pos1 += val3 * (float)_pageChild[0].height;
			pos2 = Vector2.op_Implicit(uIWidget.localCorners[_leftToRight ? 1 : 2] + ((Component)uIWidget).transform.localPosition);
		}
	}

	private void Drag(Vector2 p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		FindCornerPos(p, out var pos, out var pos2, out var bottom);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((!_leftToRight) ? 0.5f : (-0.5f), 0.5f);
		if (bottom)
		{
			((Component)_pageChild[0]).transform.localPosition = -_pageChild[0].localCorners[(!_leftToRight) ? 1 : 2];
			((Component)_pageChild[1]).transform.localPosition = -_pageChild[1].localCorners[_leftToRight ? 1 : 2];
			val.y = -0.5f;
		}
		else
		{
			((Component)_pageChild[0]).transform.localPosition = -_pageChild[0].localCorners[_leftToRight ? 3 : 0];
			((Component)_pageChild[1]).transform.localPosition = -_pageChild[1].localCorners[(!_leftToRight) ? 3 : 0];
			val.y = 0.5f;
		}
		Vector2 val2 = pos2 - pos;
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(0f - val2.y, val2.x);
		float num = Mathf.Atan2(val3.y, val3.x);
		float num2 = ((Vector2)(ref val2)).magnitude * 0.5f;
		if (Mathf.Abs(num2 / Mathf.Sin(num)) > (float)_pageChild[0].width)
		{
			pos = pos2 - ((Vector2)(ref val2)).normalized * Mathf.Abs((float)_pageChild[0].width * 2f * Mathf.Sin(num));
			val2 = pos2 - pos;
			((Vector2)(ref val3))._002Ector(0f - val2.y, val2.x);
			num2 = ((Vector2)(ref val2)).magnitude * 0.5f;
		}
		Vector2 val4 = val3 / (2f * Mathf.Tan(num));
		float magnitude = ((Vector2)(ref val4)).magnitude;
		Vector2 val5 = Vector2.Lerp(pos, pos2, 0.5f) - val4;
		float num3 = num + (float)Math.PI * ((!_leftToRight) ? (-0.5f) : 0.5f);
		float num4 = 0f - ((float)Math.PI - 2f * num);
		((Component)_pagePanel[0]).transform.localPosition = Vector2.op_Implicit(val5);
		((Component)_pagePanel[0]).transform.localEulerAngles = Vector3.forward * 57.29578f * num3;
		_pagePanel[0].baseClipRegion = new Vector4((0f - num2) * val.x, _pagePanel[0].height * val.y, num2, _pagePanel[0].height);
		((Component)_pageContainer[0]).transform.localPosition = new Vector3((0f - num2) * Mathf.Sign(val.x), magnitude * Mathf.Sign(val.y));
		((Component)_pageContainer[0]).transform.localEulerAngles = Vector3.forward * 57.29578f * (num4 - num3);
		((Component)_pagePanel[1]).transform.localPosition = Vector2.op_Implicit(val5);
		((Component)_pagePanel[1]).transform.localEulerAngles = Vector3.forward * 57.29578f * num3;
		_pagePanel[1].baseClipRegion = new Vector4(num2 * val.x, _pagePanel[1].height * val.y, num2, _pagePanel[1].height);
		((Component)_pageContainer[1]).transform.localPosition = new Vector3(num2 * Mathf.Sign(val.x), magnitude * Mathf.Sign(val.y));
		((Component)_pageContainer[1]).transform.localEulerAngles = Vector3.forward * 57.29578f * (0f - num3);
	}

	public static void Begin(GameObject parent, UIWidget left, UIWidget right, UIWidget nextLeft, UIWidget nextRight, float ratio, bool leftToRight)
	{
		PageEffect pageEffect = parent.AddMissingComponent<PageEffect>();
		pageEffect.Begin(left, right, nextLeft, nextRight, ratio, leftToRight);
	}

	public void Begin(UIWidget left, UIWidget right, UIWidget nextLeft, UIWidget nextRight, float ratio, bool leftToRight)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		_origins[0] = left;
		_origins[1] = right;
		_nexts[0] = ((!leftToRight) ? nextLeft : nextRight);
		_nexts[1] = ((!leftToRight) ? nextRight : nextLeft);
		_ratio = ratio;
		_leftToRight = leftToRight;
		_pageChild[0] = _nexts[0];
		_pageChild[1] = _nexts[1];
		for (int i = 0; i < _pageChild.Length; i++)
		{
			UIWidget uIWidget = _pageChild[i];
			_originParent[i] = ((Component)uIWidget).transform.parent;
			ref Vector3 reference = ref _originPos[i];
			reference = ((Component)uIWidget).transform.localPosition;
			ChangeParent(uIWidget, _pageContainer[i]);
			((Component)uIWidget).transform.localRotation = Quaternion.identity;
			uIWidget.ParentHasChanged();
			float num = uIWidget.width;
			float num2 = Mathf.Sqrt(Mathf.Pow((float)uIWidget.width, 2f) + Mathf.Pow((float)uIWidget.height, 2f));
			_pagePanel[i].baseClipRegion = new Vector4(0f, 0f, num, num2);
		}
		Update();
		((Behaviour)this).enabled = true;
	}

	public static void End(GameObject parent)
	{
		PageEffect component = parent.GetComponent<PageEffect>();
		if ((Object)(object)component != (Object)null)
		{
			component.End();
		}
	}

	public void End()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _pageChild.Length; i++)
		{
			UIWidget uIWidget = _pageChild[i];
			ChangeParent(uIWidget, _originParent[i]);
			((Component)uIWidget).transform.localPosition = _originPos[i];
			((Component)uIWidget).transform.localRotation = Quaternion.identity;
		}
		((Behaviour)this).enabled = false;
	}

	private void ChangeParent(UIWidget widget, Transform parent)
	{
		((Component)widget).transform.parent = parent;
		((Component)widget).BroadcastMessage("ParentHasChanged", (SendMessageOptions)1);
	}
}
