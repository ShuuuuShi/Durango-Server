using Durango.Render.Camera;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueClickTargetLocator
{
	private readonly Vector3 _worldPos;

	private readonly Transform _targetTransform;

	private readonly UIWidget _widget;

	public PrologueClickTargetLocator(Transform transform, Vector3 world = default(Vector3))
	{
		_targetTransform = transform;
		_worldPos = world;
		_widget = ((!(_targetTransform == null)) ? _targetTransform.GetComponent<UIWidget>() : null);
	}

	public void Process()
	{
	}

	public Vector3 GetNGUIPosition()
	{
		if (_targetTransform == null)
		{
			if (_worldPos != Vector3.zero)
			{
				return MainCamera.WorldToNGUIPos(_worldPos);
			}
			return Vector3.zero;
		}
		return UIUtility.ToRootPosition(_targetTransform.gameObject);
	}

	public Vector2 GetOffset()
	{
		return Vector2.zero;
	}

	public float Rotate()
	{
		return 0f;
	}

	public bool IsVisible()
	{
		return ((_targetTransform != null && _targetTransform.gameObject.activeInHierarchy) || _worldPos != Vector3.zero) && (_widget == null || _widget.isVisible);
	}
}
