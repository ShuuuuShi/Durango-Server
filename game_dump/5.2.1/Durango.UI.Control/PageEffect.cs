using System;
using UnityEngine;

namespace Durango.UI.Control;

public class PageEffect : MonoBehaviour
{
	private readonly UIWidget[] _origins = new UIWidget[2];

	private readonly UIWidget[] _nexts = new UIWidget[2];

	private float _ratio;

	private UIPanel[] _pagePanel = new UIPanel[2];

	private Transform[] _pageContainer = new Transform[2];

	private Transform[] _originParent = new Transform[2];

	private Vector3[] _originPos = new Vector3[2];

	private UIWidget[] _pageChild = new UIWidget[2];

	private bool _leftToRight;

	private void Awake()
	{
		UIPanel componentInParent = GetComponentInParent<UIPanel>();
		int num = ((!(componentInParent == null)) ? componentInParent.depth : 0);
		for (int i = 0; i < _pagePanel.Length; i++)
		{
			UIPanel uIPanel = base.gameObject.AddChild<UIPanel>();
			uIPanel.clipping = UIDrawCall.Clipping.SoftClip;
			uIPanel.clipSoftness = Vector2.zero;
			uIPanel.depth = num + 1;
			_pageContainer[i] = uIPanel.gameObject.AddChild().transform;
			_pagePanel[i] = uIPanel;
		}
	}

	private void Update()
	{
		Drag(NGUIMath.ScreenToPixels(UICamera.lastEventPosition, base.transform));
	}

	private void FindCornerPos(Vector2 touchPos, out Vector2 pos1, out Vector2 pos2, out bool bottom)
	{
		UIWidget uIWidget = ((!_leftToRight) ? _origins[1] : _origins[0]);
		pos1 = touchPos;
		pos2 = Vector3.Lerp(uIWidget.localCorners[(!_leftToRight) ? 3 : 0], uIWidget.localCorners[_leftToRight ? 1 : 2], _ratio) + uIWidget.transform.localPosition;
		Vector2 vector = pos2 - pos1;
		Vector2 vector2 = new Vector2(0f - vector.y, vector.x);
		float num = Mathf.Atan2(vector2.y, vector2.x);
		float f = 2f * num - (float)Math.PI / 2f;
		Vector2 vector3 = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
		pos1 -= vector3 * _pageChild[0].height * _ratio;
		pos2 = uIWidget.localCorners[(!_leftToRight) ? 3 : 0] + uIWidget.transform.localPosition;
		UIWidget component = GetComponent<UIWidget>();
		Rect rect = new Rect(component.localCorners[0], component.localSize);
		bottom = ((!_leftToRight) ? (pos1.x > rect.xMax || pos1.y < rect.yMin) : (pos1.x < rect.xMin || pos1.y < rect.yMin));
		if (bottom)
		{
			pos1 += vector3 * _pageChild[0].height;
			pos2 = uIWidget.localCorners[_leftToRight ? 1 : 2] + uIWidget.transform.localPosition;
		}
	}

	private void Drag(Vector2 p)
	{
		FindCornerPos(p, out var pos, out var pos2, out var bottom);
		Vector2 vector = new Vector2((!_leftToRight) ? 0.5f : (-0.5f), 0.5f);
		if (bottom)
		{
			_pageChild[0].transform.localPosition = -_pageChild[0].localCorners[(!_leftToRight) ? 1 : 2];
			_pageChild[1].transform.localPosition = -_pageChild[1].localCorners[_leftToRight ? 1 : 2];
			vector.y = -0.5f;
		}
		else
		{
			_pageChild[0].transform.localPosition = -_pageChild[0].localCorners[_leftToRight ? 3 : 0];
			_pageChild[1].transform.localPosition = -_pageChild[1].localCorners[(!_leftToRight) ? 3 : 0];
			vector.y = 0.5f;
		}
		Vector2 vector2 = pos2 - pos;
		Vector2 vector3 = new Vector2(0f - vector2.y, vector2.x);
		float num = Mathf.Atan2(vector3.y, vector3.x);
		float num2 = vector2.magnitude * 0.5f;
		if (Mathf.Abs(num2 / Mathf.Sin(num)) > (float)_pageChild[0].width)
		{
			pos = pos2 - vector2.normalized * Mathf.Abs((float)_pageChild[0].width * 2f * Mathf.Sin(num));
			vector2 = pos2 - pos;
			vector3 = new Vector2(0f - vector2.y, vector2.x);
			num2 = vector2.magnitude * 0.5f;
		}
		Vector2 vector4 = vector3 / (2f * Mathf.Tan(num));
		float magnitude = vector4.magnitude;
		Vector2 vector5 = Vector2.Lerp(pos, pos2, 0.5f) - vector4;
		float num3 = num + (float)Math.PI * ((!_leftToRight) ? (-0.5f) : 0.5f);
		float num4 = 0f - ((float)Math.PI - 2f * num);
		_pagePanel[0].transform.localPosition = vector5;
		_pagePanel[0].transform.localEulerAngles = Vector3.forward * 57.29578f * num3;
		_pagePanel[0].baseClipRegion = new Vector4((0f - num2) * vector.x, _pagePanel[0].height * vector.y, num2, _pagePanel[0].height);
		_pageContainer[0].transform.localPosition = new Vector3((0f - num2) * Mathf.Sign(vector.x), magnitude * Mathf.Sign(vector.y));
		_pageContainer[0].transform.localEulerAngles = Vector3.forward * 57.29578f * (num4 - num3);
		_pagePanel[1].transform.localPosition = vector5;
		_pagePanel[1].transform.localEulerAngles = Vector3.forward * 57.29578f * num3;
		_pagePanel[1].baseClipRegion = new Vector4(num2 * vector.x, _pagePanel[1].height * vector.y, num2, _pagePanel[1].height);
		_pageContainer[1].transform.localPosition = new Vector3(num2 * Mathf.Sign(vector.x), magnitude * Mathf.Sign(vector.y));
		_pageContainer[1].transform.localEulerAngles = Vector3.forward * 57.29578f * (0f - num3);
	}

	public static void Begin(GameObject parent, UIWidget left, UIWidget right, UIWidget nextLeft, UIWidget nextRight, float ratio, bool leftToRight)
	{
		parent.AddMissingComponent<PageEffect>().Begin(left, right, nextLeft, nextRight, ratio, leftToRight);
	}

	public void Begin(UIWidget left, UIWidget right, UIWidget nextLeft, UIWidget nextRight, float ratio, bool leftToRight)
	{
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
			_originParent[i] = uIWidget.transform.parent;
			ref Vector3 reference = ref _originPos[i];
			reference = uIWidget.transform.localPosition;
			ChangeParent(uIWidget, _pageContainer[i]);
			uIWidget.transform.localRotation = Quaternion.identity;
			uIWidget.ParentHasChanged();
			float z = uIWidget.width;
			float w = Mathf.Sqrt(Mathf.Pow(uIWidget.width, 2f) + Mathf.Pow(uIWidget.height, 2f));
			_pagePanel[i].baseClipRegion = new Vector4(0f, 0f, z, w);
		}
		Update();
		base.enabled = true;
	}

	public static void End(GameObject parent)
	{
		PageEffect component = parent.GetComponent<PageEffect>();
		if (component != null)
		{
			component.End();
		}
	}

	public void End()
	{
		for (int i = 0; i < _pageChild.Length; i++)
		{
			UIWidget uIWidget = _pageChild[i];
			ChangeParent(uIWidget, _originParent[i]);
			uIWidget.transform.localPosition = _originPos[i];
			uIWidget.transform.localRotation = Quaternion.identity;
		}
		base.enabled = false;
	}

	private void ChangeParent(UIWidget widget, Transform parent)
	{
		widget.transform.parent = parent;
		widget.BroadcastMessage("ParentHasChanged", SendMessageOptions.DontRequireReceiver);
	}
}
