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
			onAwake(base.gameObject);
		}
	}

	private void Start()
	{
		if (onStart != null)
		{
			onStart(base.gameObject);
		}
	}

	private void OnEnable()
	{
		if (onEnable != null)
		{
			onEnable(base.gameObject);
		}
	}

	private void OnDisable()
	{
		if (onDisable != null)
		{
			onDisable(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (onDestroy != null)
		{
			onDestroy(base.gameObject);
		}
	}

	private void OnWillRenderObject()
	{
		if (onWillRenderObject != null)
		{
			onWillRenderObject(base.gameObject);
		}
	}

	public new static UIExtendEventListener Get(GameObject go)
	{
		UIExtendEventListener uIExtendEventListener = go.GetComponent<UIExtendEventListener>();
		if (uIExtendEventListener == null)
		{
			uIExtendEventListener = go.AddComponent<UIExtendEventListener>();
		}
		return uIExtendEventListener;
	}
}
