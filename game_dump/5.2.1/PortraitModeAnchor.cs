using UnityEngine;

[RequireComponent(typeof(UIAnchor))]
public class PortraitModeAnchor : MonoBehaviour
{
	[SerializeField]
	private Camera _uiCamera;

	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private UIAnchor.Side _side = UIAnchor.Side.Center;

	[SerializeField]
	private bool _runOnlyOnce = true;

	[SerializeField]
	private Vector2 _relativeOffset = Vector2.zero;

	[SerializeField]
	private Vector2 _pixelOffset = Vector2.zero;

	private bool _isPortrait;

	private void Awake()
	{
		UIManager.AddOnScreenResized(OnScreenResize);
	}

	private void OnScreenResize()
	{
		if (!(this == null))
		{
			bool flag = UIManager.IsPortraitWidget(base.gameObject);
			if (_isPortrait != flag)
			{
				_isPortrait = flag;
				UIAnchor component = GetComponent<UIAnchor>();
				Swap(component);
				component.enabled = true;
			}
		}
	}

	private void Swap(UIAnchor anchor)
	{
		Camera uiCamera = _uiCamera;
		_uiCamera = anchor.uiCamera;
		anchor.uiCamera = uiCamera;
		GameObject container = _container;
		_container = anchor.container;
		anchor.container = container;
		UIAnchor.Side side = _side;
		_side = anchor.side;
		anchor.side = side;
		bool runOnlyOnce = _runOnlyOnce;
		_runOnlyOnce = anchor.runOnlyOnce;
		anchor.runOnlyOnce = runOnlyOnce;
		Vector2 relativeOffset = _relativeOffset;
		_relativeOffset = anchor.relativeOffset;
		anchor.relativeOffset = relativeOffset;
		Vector2 pixelOffset = _pixelOffset;
		_pixelOffset = anchor.pixelOffset;
		anchor.pixelOffset = pixelOffset;
	}
}
