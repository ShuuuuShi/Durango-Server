using System;
using UnityEngine;

namespace Durango.Model;

public class RopeSetter : MonoBehaviour
{
	[Serializable]
	public class RopeSet
	{
		public Transform AttachmentA;

		public Transform AttachmentB;

		public float Length;

		public float Thickness;

		[NonSerialized]
		public float SqrLength;
	}

	[SerializeField]
	private GameObject _ropePrefab;

	[SerializeField]
	private RopeSet[] _ropeSets;

	private Rope[] _ropes;

	[ExposedInEditor(null)]
	public void InitRopes()
	{
		if (_ropePrefab == null || _ropes != null)
		{
			return;
		}
		if (_ropes == null)
		{
			_ropes = new Rope[_ropeSets.Length];
			for (int i = 0; i < _ropes.Length; i++)
			{
				if (_ropes[i] == null)
				{
					Rope component = UnityEngine.Object.Instantiate(_ropePrefab, base.transform.position, base.transform.rotation, base.transform).GetComponent<Rope>();
					if (!(component == null))
					{
						_ropes[i] = component;
					}
				}
			}
		}
		for (int j = 0; j < _ropes.Length; j++)
		{
			_ropes[j].Init(_ropeSets[j].AttachmentB, _ropeSets[j].AttachmentA, _ropeSets[j].Length, _ropeSets[j].Thickness);
			_ropeSets[j].SqrLength = _ropeSets[j].Length * _ropeSets[j].Length;
		}
		if (_ropes != null)
		{
			base.enabled = true;
		}
	}

	private void Awake()
	{
		base.enabled = false;
	}

	private void LateUpdate()
	{
		for (int i = 0; i < _ropes.Length; i++)
		{
			if (!(_ropes[i] == null))
			{
				_ropes[i].UpdateBones();
			}
		}
	}
}
