using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Durango.Model;

[Serializable]
public class TransformResolver
{
	[SerializeField]
	private string _name;

	private Transform _transform;

	public string Name => _name;

	public bool IsValid => _transform != null;

	public TransformResolver(string defaultName)
	{
		_name = defaultName;
	}

	public static implicit operator Transform(TransformResolver resolver)
	{
		return resolver._transform;
	}

	public bool Resolve(IList<Transform> candidates)
	{
		_transform = null;
		for (int i = 0; i < KUtility.GetSize(candidates); i++)
		{
			Transform transform = candidates[i];
			if (transform.name == _name)
			{
				_transform = transform;
				break;
			}
		}
		return CheckAndReturn();
	}

	public bool Resolve(IDictionary<string, Transform> candidates)
	{
		_transform = null;
		if (candidates == null)
		{
			return false;
		}
		_transform = candidates.Get(_name);
		return CheckAndReturn();
	}

	public bool Resolve(Transform parentTransform)
	{
		_transform = null;
		if (parentTransform == null)
		{
			return false;
		}
		if (parentTransform.name == _name)
		{
			_transform = parentTransform;
			return true;
		}
		Transform[] componentsInChildren = parentTransform.GetComponentsInChildren<Transform>();
		_transform = componentsInChildren.FirstOrDefault((Transform x) => x.name == _name);
		return CheckAndReturn();
	}

	private bool CheckAndReturn()
	{
		return _transform != null;
	}
}
