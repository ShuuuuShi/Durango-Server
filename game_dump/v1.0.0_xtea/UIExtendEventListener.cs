using UnityEngine;

public class UIExtendEventListener : UIEventListener
{
	public VoidDelegate onAwake;

	public VoidDelegate onStart;

	public VoidDelegate onEnable;

	public VoidDelegate onDisable;

	public VoidDelegate onDestroy;

	public VoidDelegate onWillRenderObject;

	private void Awake()
	{
		if (onAwake != null)
		{
			onAwake(((Component)this).gameObject);
		}
	}

	private void Start()
	{
		if (onStart != null)
		{
			onStart(((Component)this).gameObject);
		}
	}

	private void OnEnable()
	{
		if (onEnable != null)
		{
			onEnable(((Component)this).gameObject);
		}
	}

	private void OnDisable()
	{
		if (onDisable != null)
		{
			onDisable(((Component)this).gameObject);
		}
	}

	private void OnDestroy()
	{
		if (onDestroy != null)
		{
			onDestroy(((Component)this).gameObject);
		}
	}

	private void OnWillRenderObject()
	{
		if (onWillRenderObject != null)
		{
			onWillRenderObject(((Component)this).gameObject);
		}
	}

	public new static UIExtendEventListener Get(GameObject go)
	{
		UIExtendEventListener uIExtendEventListener = go.GetComponent<UIExtendEventListener>();
		if ((Object)(object)uIExtendEventListener == (Object)null)
		{
			uIExtendEventListener = go.AddComponent<UIExtendEventListener>();
		}
		return uIExtendEventListener;
	}
}
