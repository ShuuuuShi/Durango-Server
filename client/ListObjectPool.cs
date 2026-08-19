using System;
using UnityEngine;

[Serializable]
public class ListObjectPool : ListObjectPoolBase<GameObject>
{
	[SerializeField]
	private GameObject _baseObject;

	[SerializeField]
	private bool _useBase;

	public override GameObject BaseObject
	{
		get
		{
			return _baseObject;
		}
		set
		{
			_baseObject = value;
		}
	}

	public override bool UseBase
	{
		get
		{
			return _useBase;
		}
		set
		{
			_useBase = value;
		}
	}

	protected override void SetActive(GameObject obj, bool active)
	{
		if (obj != null)
		{
			obj.SetActive(active);
		}
	}

	protected override void MakeNew(out GameObject obj, out GameObject comp)
	{
		if (BaseObject == null)
		{
			obj = null;
			comp = null;
			return;
		}
		obj = UnityEngine.Object.Instantiate(BaseObject.gameObject, Parent);
		obj.transform.localPosition = BaseObject.transform.localPosition;
		obj.transform.localScale = BaseObject.transform.localScale;
		obj.transform.localRotation = BaseObject.transform.localRotation;
		comp = obj;
	}

	protected override TK GetComponent<TK>(GameObject obj)
	{
		return obj.GetComponent<TK>();
	}
}
public class ListObjectPool<T> : ListObjectPoolBase<T> where T : Component
{
	protected override void MakeNew(out GameObject obj, out T comp)
	{
		if (BaseObject == null)
		{
			obj = null;
			comp = (T)null;
			return;
		}
		T baseObject = BaseObject;
		obj = UnityEngine.Object.Instantiate(baseObject.gameObject, Parent);
		Transform transform = obj.transform;
		T baseObject2 = BaseObject;
		transform.localPosition = baseObject2.transform.localPosition;
		Transform transform2 = obj.transform;
		T baseObject3 = BaseObject;
		transform2.localScale = baseObject3.transform.localScale;
		Transform transform3 = obj.transform;
		T baseObject4 = BaseObject;
		transform3.localRotation = baseObject4.transform.localRotation;
		comp = obj.GetComponent<T>();
	}

	protected override TK GetComponent<TK>(T obj)
	{
		return obj.GetComponent<TK>();
	}

	protected override void SetActive(T obj, bool active)
	{
		if (obj != null)
		{
			obj.gameObject.SetActive(active);
		}
	}
}
