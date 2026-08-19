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
		if (!_isInit)
		{
			_isInit = true;
			_widget = GetComponent<UIWidget>();
			if (_widget != null && !_widget.isAnchored)
			{
				_widget = null;
			}
			_anchor = GetComponent<UIAnchor>();
			if (_widget != null)
			{
				_anchors = new int[4];
				_anchors[0] = _widget.leftAnchor.absolute;
				_anchors[1] = _widget.rightAnchor.absolute;
				_anchors[2] = _widget.bottomAnchor.absolute;
				_anchors[3] = _widget.topAnchor.absolute;
			}
			else if (_anchor != null)
			{
				_origin = _anchor.pixelOffset;
			}
			else
			{
				_origin = base.transform.localPosition;
			}
		}
	}

	private void Awake()
	{
		UIManager.AddOnScreenResized(OnScreenResize);
	}

	private void OnScreenResize()
	{
		Init();
		bool isPortraitScreen = UIManager.IsPortraitScreen;
		if (_isPortrait != isPortraitScreen)
		{
			_isPortrait = isPortraitScreen;
			if (isPortraitScreen)
			{
				Set(_origin + _offset);
			}
			else
			{
				Set(_origin);
			}
		}
	}

	private void Set(Vector2 p)
	{
		if (_widget != null)
		{
			int num = Mathf.RoundToInt(p.x);
			int num2 = Mathf.RoundToInt(p.y);
			_widget.leftAnchor.absolute = _anchors[0] + num;
			_widget.rightAnchor.absolute = _anchors[1] + num;
			_widget.bottomAnchor.absolute = _anchors[2] + num2;
			_widget.topAnchor.absolute = _anchors[3] + num2;
			_widget.UpdateAnchors();
		}
		else if (_anchor != null)
		{
			_anchor.pixelOffset = p;
			_anchor.enabled = true;
		}
		else
		{
			base.transform.localPosition = p;
		}
	}
}
