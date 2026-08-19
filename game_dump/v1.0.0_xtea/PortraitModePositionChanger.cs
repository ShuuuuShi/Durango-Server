using UnityEngine;

public class PortraitModePositionChanger : MonoBehaviour
{
	[SerializeField]
	private Vector2 _offset;

	private Vector2 _origin;

	private UIAnchor _anchor;

	private int[] _anchors;

	private UIWidget _widget;

	private bool _isPortrait;

	private bool _isInit;

	private void Init()
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_widget = ((Component)this).GetComponent<UIWidget>();
			if ((Object)(object)_widget != (Object)null && !_widget.isAnchored)
			{
				_widget = null;
			}
			_anchor = ((Component)this).GetComponent<UIAnchor>();
			if ((Object)(object)_widget != (Object)null)
			{
				_anchors = new int[4];
				_anchors[0] = _widget.leftAnchor.absolute;
				_anchors[1] = _widget.rightAnchor.absolute;
				_anchors[2] = _widget.bottomAnchor.absolute;
				_anchors[3] = _widget.topAnchor.absolute;
			}
			else if ((Object)(object)_anchor != (Object)null)
			{
				_origin = _anchor.pixelOffset;
			}
			else
			{
				_origin = Vector2.op_Implicit(((Component)this).transform.localPosition);
			}
		}
	}

	private void OnEnable()
	{
		if (_isPortrait != UIManager.IsPortraitMode)
		{
			OnPortraitMode(UIManager.IsPortraitMode);
		}
	}

	private void OnPortraitMode(bool isPortrait)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Init();
		_isPortrait = isPortrait;
		if (isPortrait)
		{
			Set(_origin + _offset);
		}
		else
		{
			Set(_origin);
		}
	}

	private void Set(Vector2 p)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_widget != (Object)null)
		{
			int num = Mathf.RoundToInt(p.x);
			int num2 = Mathf.RoundToInt(p.y);
			_widget.leftAnchor.absolute = _anchors[0] + num;
			_widget.rightAnchor.absolute = _anchors[1] + num;
			_widget.bottomAnchor.absolute = _anchors[2] + num2;
			_widget.topAnchor.absolute = _anchors[3] + num2;
			_widget.UpdateAnchors();
		}
		else if ((Object)(object)_anchor != (Object)null)
		{
			_anchor.pixelOffset = p;
			((Behaviour)_anchor).enabled = true;
		}
		else
		{
			((Component)this).transform.localPosition = Vector2.op_Implicit(p);
		}
	}
}
