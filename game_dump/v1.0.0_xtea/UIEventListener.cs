using UnityEngine;

[AddComponentMenu("NGUI/Internal/Event Listener")]
public class UIEventListener : MonoBehaviour
{
	public delegate void VoidDelegate(GameObject go);

	public delegate void BoolDelegate(GameObject go, bool state);

	public delegate void FloatDelegate(GameObject go, float delta);

	public delegate void VectorDelegate(GameObject go, Vector2 delta);

	public delegate void ObjectDelegate(GameObject go, GameObject obj);

	public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

	public object parameter;

	public VoidDelegate onSubmit;

	public VoidDelegate onClick;

	public VoidDelegate onDoubleClick;

	public BoolDelegate onHover;

	public BoolDelegate onPress;

	public BoolDelegate onSelect;

	public FloatDelegate onScroll;

	public VoidDelegate onDragStart;

	public VectorDelegate onDrag;

	public VoidDelegate onDragOver;

	public VoidDelegate onDragOut;

	public VoidDelegate onDragEnd;

	public ObjectDelegate onDrop;

	public KeyCodeDelegate onKey;

	public BoolDelegate onTooltip;

	private bool isColliderEnabled
	{
		get
		{
			Collider component = ((Component)this).GetComponent<Collider>();
			if ((Object)(object)component != (Object)null)
			{
				return component.enabled;
			}
			Collider2D component2 = ((Component)this).GetComponent<Collider2D>();
			return (Object)(object)component2 != (Object)null && ((Behaviour)component2).enabled;
		}
	}

	private void OnSubmit()
	{
		if (isColliderEnabled && onSubmit != null)
		{
			onSubmit(((Component)this).gameObject);
		}
	}

	private void OnClick()
	{
		if (isColliderEnabled && onClick != null)
		{
			onClick(((Component)this).gameObject);
		}
	}

	private void OnDoubleClick()
	{
		if (isColliderEnabled && onDoubleClick != null)
		{
			onDoubleClick(((Component)this).gameObject);
		}
	}

	private void OnHover(bool isOver)
	{
		if (isColliderEnabled && onHover != null)
		{
			onHover(((Component)this).gameObject, isOver);
		}
	}

	private void OnPress(bool isPressed)
	{
		if (isColliderEnabled && onPress != null)
		{
			onPress(((Component)this).gameObject, isPressed);
		}
	}

	private void OnSelect(bool selected)
	{
		if (isColliderEnabled && onSelect != null)
		{
			onSelect(((Component)this).gameObject, selected);
		}
	}

	private void OnScroll(float delta)
	{
		if (isColliderEnabled && onScroll != null)
		{
			onScroll(((Component)this).gameObject, delta);
		}
	}

	private void OnDragStart()
	{
		if (onDragStart != null)
		{
			onDragStart(((Component)this).gameObject);
		}
	}

	private void OnDrag(Vector2 delta)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (onDrag != null)
		{
			onDrag(((Component)this).gameObject, delta);
		}
	}

	private void OnDragOver()
	{
		if (isColliderEnabled && onDragOver != null)
		{
			onDragOver(((Component)this).gameObject);
		}
	}

	private void OnDragOut()
	{
		if (isColliderEnabled && onDragOut != null)
		{
			onDragOut(((Component)this).gameObject);
		}
	}

	private void OnDragEnd()
	{
		if (onDragEnd != null)
		{
			onDragEnd(((Component)this).gameObject);
		}
	}

	private void OnDrop(GameObject go)
	{
		if (isColliderEnabled && onDrop != null)
		{
			onDrop(((Component)this).gameObject, go);
		}
	}

	private void OnKey(KeyCode key)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (isColliderEnabled && onKey != null)
		{
			onKey(((Component)this).gameObject, key);
		}
	}

	private void OnTooltip(bool show)
	{
		if (isColliderEnabled && onTooltip != null)
		{
			onTooltip(((Component)this).gameObject, show);
		}
	}

	public static UIEventListener Get(GameObject go)
	{
		UIEventListener uIEventListener = go.GetComponent<UIEventListener>();
		if ((Object)(object)uIEventListener == (Object)null)
		{
			uIEventListener = go.AddComponent<UIEventListener>();
		}
		return uIEventListener;
	}
}
