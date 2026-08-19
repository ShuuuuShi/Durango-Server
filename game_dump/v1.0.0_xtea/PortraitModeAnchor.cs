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

	private void OnEnable()
	{
		OnPortraitMode(UIManager.IsPortraitMode);
	}

	private void OnPortraitMode(bool isPortrait)
	{
		if (isPortrait != _isPortrait)
		{
			_isPortrait = isPortrait;
			UIAnchor component = ((Component)this).GetComponent<UIAnchor>();
			Swap(component);
			((Behaviour)component).enabled = true;
		}
	}

	private void Swap(UIAnchor anchor)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
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
