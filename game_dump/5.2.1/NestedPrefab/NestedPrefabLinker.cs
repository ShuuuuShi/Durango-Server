using System;
using System.Collections.Generic;
using Durango.System;
using Durango.Utils;
using UnityEngine;

namespace NestedPrefab;

[ExecuteInEditMode]
public class NestedPrefabLinker : NestedPrefabLinker<Transform>
{
}
[ExecuteInEditMode]
public class NestedPrefabLinker<T> : MonoBehaviour, IUIInitializable, RectLayout.ICompatible where T : Component
{
	[SerializeField]
	private GameObject _prefab;

	[SerializeField]
	private GameObject _prefabForPC;

	[SerializeField]
	private int _panelDepthOffset;

	[HideInInspector]
	[SerializeField]
	private T _object;

	private bool _isInitializedLink;

	public T Object
	{
		get
		{
			InitializeLink();
			return _object;
		}
	}

	private GameObject Prefab
	{
		get
		{
			if (_prefabForPC == null || !Platform.Instance.UsePCUI)
			{
				return _prefab;
			}
			return _prefabForPC;
		}
	}

	private void InitializeLink()
	{
		if (!Application.isPlaying || _isInitializedLink)
		{
			return;
		}
		_isInitializedLink = true;
		Unlink();
		Link();
		if (UIManager.UIInitializing)
		{
			return;
		}
		IUIInitializable[] componentsInChildren = _object.GetComponentsInChildren<IUIInitializable>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			try
			{
				componentsInChildren[i].Init();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	protected virtual void Awake()
	{
		InitializeLink();
	}

	protected virtual void OnEnable()
	{
		if (!Application.isPlaying)
		{
			Link();
		}
	}

	protected virtual void OnDisable()
	{
		if (!Application.isPlaying)
		{
			Unlink();
		}
	}

	private void OnValidate()
	{
	}

	void IUIInitializable.Init()
	{
		InitializeLink();
	}

	protected virtual void Link()
	{
		if (Prefab == null)
		{
			Unlink();
		}
		else
		{
			if (_object != null)
			{
				return;
			}
			_object = base.gameObject.AddChild(Prefab).GetComponent<T>();
			UIRect component = GetComponent<UIRect>();
			if (component != null)
			{
				UIRect component2 = _object.GetComponent<UIRect>();
				if (component2 != null)
				{
					component2.SetAnchor(base.gameObject, 0, 0, 0, 0);
				}
			}
			UIWidget uIWidget = component as UIWidget;
			int num = 0;
			int num2 = 0;
			if (uIWidget != null)
			{
				UIPanel uIPanel = UIUtility.FindComponentInParent<UIPanel>(base.gameObject);
				num = ((!(uIPanel == null)) ? uIPanel.depth : 0);
				num2 = uIWidget.depth;
			}
			else
			{
				UIPanel uIPanel2 = component as UIPanel;
				if (uIPanel2 != null)
				{
					num = uIPanel2.depth;
				}
			}
			num += _panelDepthOffset;
			using (Reusable<Stack<Transform>> reusable = ReusableStack<Transform>.Pop())
			{
				Stack<Transform> value = reusable.Value;
				value.Push(_object.transform);
				while (value.Count > 0)
				{
					Transform transform = value.Pop();
					UIRect component3 = transform.GetComponent<UIRect>();
					UIWidget uIWidget2 = component3 as UIWidget;
					if (uIWidget2 != null)
					{
						if (uIWidget2.DrawPanel == null)
						{
							uIWidget2.depth += num2;
						}
					}
					else
					{
						UIPanel uIPanel3 = component3 as UIPanel;
						if (uIPanel3 != null)
						{
							uIPanel3.depth += num;
							continue;
						}
					}
					int i = 0;
					for (int childCount = transform.childCount; i < childCount; i++)
					{
						value.Push(transform.GetChild(i));
					}
				}
				if (!Application.isPlaying)
				{
					value.Push(_object.transform);
					while (value.Count > 0)
					{
						Transform transform2 = value.Pop();
						transform2.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
						int j = 0;
						for (int childCount2 = transform2.childCount; j < childCount2; j++)
						{
							value.Push(transform2.GetChild(j));
						}
					}
				}
			}
			OnLinked();
		}
	}

	protected virtual void OnLinked()
	{
	}

	protected virtual void Unlink()
	{
		if (!(_object == null))
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_object.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_object.gameObject);
			}
			_object = null;
		}
	}

	Vector2 RectLayout.ICompatible.UpdateLayout(float? x, float? y)
	{
		T @object;
		if (Application.isPlaying)
		{
			@object = Object;
		}
		else
		{
			if (_object == null)
			{
				return new Vector2(x.HasValue ? x.Value : 0f, y.HasValue ? y.Value : 0f);
			}
			@object = _object;
		}
		if (!@object.gameObject.activeSelf)
		{
			return new Vector2(0f, 0f);
		}
		RectLayout.ICompatible component = @object.GetComponent<RectLayout.ICompatible>();
		if (component == null)
		{
			UIRect component2 = @object.GetComponent<UIRect>();
			if (component2 != null)
			{
				if (!x.HasValue)
				{
					x = component2.GetWidth();
				}
				if (!y.HasValue)
				{
					y = component2.GetHeight();
				}
			}
			return new Vector2(x.HasValue ? x.Value : 0f, y.HasValue ? y.Value : 0f);
		}
		return component.UpdateLayout(x, y);
	}
}
