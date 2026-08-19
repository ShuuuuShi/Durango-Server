using System;
using UnityEngine;

public class UIEffect : MonoBehaviour
{
	public Action<UIEffect> Disabled;

	private Transform _cachedTransform;

	private bool _hasParent;

	private Transform _parent;

	private Vector3 _offset;

	public string Key { get; set; }

	private void Awake()
	{
		_cachedTransform = ((Component)this).transform;
		OnAwake();
	}

	protected virtual void OnAwake()
	{
	}

	private void Update()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (_hasParent)
		{
			if ((Object)(object)_parent == (Object)null)
			{
				Stop();
				return;
			}
			_cachedTransform.position = _parent.position + _offset;
		}
		OnUpdate();
	}

	protected virtual void OnUpdate()
	{
	}

	private void OnDisable()
	{
		if (Disabled != null)
		{
			Disabled(this);
		}
	}

	public virtual void Play()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public virtual void Stop()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void SetParent(Transform parent, Vector3 offset)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		_hasParent = true;
		_parent = parent;
		_offset = offset;
	}

	public void SetPosition(Vector3 pos)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_hasParent = false;
		_cachedTransform.position = pos;
	}
}
