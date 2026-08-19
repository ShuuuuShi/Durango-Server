using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Forward Events (Legacy)")]
public class UIForwardEvents : MonoBehaviour
{
	public GameObject target;

	public bool onHover;

	public bool onPress;

	public bool onClick;

	public bool onDoubleClick;

	public bool onSelect;

	public bool onDrag;

	public bool onDrop;

	public bool onSubmit;

	public bool onScroll;

	private void OnHover(bool isOver)
	{
		if (onHover && (Object)(object)target != (Object)null)
		{
			target.SendMessage("OnHover", (object)isOver, (SendMessageOptions)1);
		}
	}

	private void OnPress(bool pressed)
	{
		if (onPress && (Object)(object)target != (Object)null)
		{
			target.SendMessage("OnPress", (object)pressed, (SendMessageOptions)1);
		}
	}

	private void OnClick()
	{
		if (onClick && (Object)(object)target != (Object)null)
		{
			target.SendMessage("OnClick", (SendMessageOptions)1);
		}
	}

	private void OnDoubleClick()
	{
		if (onDoubleClick && (Object)(object)target != (Object)null)
		{
			target.SendMessage("OnDoubleClick", (SendMessageOptions)1);
		}
	}

	private void OnSelect(bool selected)
	{
		if (onSelect && (Object)(object)target != (Object)null)
		{
			target.SendMessage("OnSelect", (object)selected, (SendMessageOptions)1);
		}
	}

	private void OnDrag(Vector2 delta)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (onDrag && (Object)(object)target != (Object)null)
		{
			target.SendMessage("OnDrag", (object)delta, (SendMessageOptions)1);
		}
	}

	private void OnDrop(GameObject go)
	{
		if (onDrop && (Object)(object)target != (Object)null)
		{
			target.SendMessage("OnDrop", (object)go, (SendMessageOptions)1);
		}
	}

	private void OnSubmit()
	{
		if (onSubmit && (Object)(object)target != (Object)null)
		{
			target.SendMessage("OnSubmit", (SendMessageOptions)1);
		}
	}

	private void OnScroll(float delta)
	{
		if (onScroll && (Object)(object)target != (Object)null)
		{
			target.SendMessage("OnScroll", (object)delta, (SendMessageOptions)1);
		}
	}
}
