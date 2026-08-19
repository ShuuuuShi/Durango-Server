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
		obj.SetActive(active);
	}

	protected override void MakeNew(out GameObject obj, out GameObject comp)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Object obj2 = Object.Instantiate((Object)(object)BaseObject.gameObject, BaseObject.transform.parent);
		obj = (GameObject)(object)((obj2 is GameObject) ? obj2 : null);
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
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		Object obj2 = Object.Instantiate((Object)(object)((Component)BaseObject).gameObject, ((Component)BaseObject).transform.parent);
		obj = (GameObject)(object)((obj2 is GameObject) ? obj2 : null);
		obj.transform.localPosition = ((Component)BaseObject).transform.localPosition;
		obj.transform.localScale = ((Component)BaseObject).transform.localScale;
		obj.transform.localRotation = ((Component)BaseObject).transform.localRotation;
		comp = obj.GetComponent<T>();
	}

	protected override TK GetComponent<TK>(T obj)
	{
		return ((Component)obj).GetComponent<TK>();
	}

	protected override void SetActive(T obj, bool active)
	{
		((Component)obj).gameObject.SetActive(active);
	}
}
