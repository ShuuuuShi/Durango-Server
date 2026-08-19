using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag Camera")]
[ExecuteInEditMode]
public class UIDragCamera : MonoBehaviour
{
	public UIDraggableCamera draggableCamera;

	private void Awake()
	{
		if ((Object)(object)draggableCamera == (Object)null)
		{
			draggableCamera = NGUITools.FindInParents<UIDraggableCamera>(((Component)this).gameObject);
		}
	}

	private void OnPress(bool isPressed)
	{
		if (((Behaviour)this).enabled && NGUITools.GetActive(((Component)this).gameObject) && (Object)(object)draggableCamera != (Object)null)
		{
			draggableCamera.Press(isPressed);
		}
	}

	private void OnDrag(Vector2 delta)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled && NGUITools.GetActive(((Component)this).gameObject) && (Object)(object)draggableCamera != (Object)null)
		{
			draggableCamera.Drag(delta);
		}
	}

	private void OnScroll(float delta)
	{
		if (((Behaviour)this).enabled && NGUITools.GetActive(((Component)this).gameObject) && (Object)(object)draggableCamera != (Object)null)
		{
			draggableCamera.Scroll(delta);
		}
	}
}
