using System;
using UnityEngine;

public class RopeSetter : MonoBehaviour
{
	[Serializable]
	public class RopeSet
	{
		public Transform AttachmentA;

		public Transform AttachmentB;

		public float Length;

		[NonSerialized]
		public float SqrLength;
	}

	[SerializeField]
	private GameObject _ropePrefab;

	[SerializeField]
	private RopeSet[] _ropeSets;

	private Rope[] _ropes;

	private void Start()
	{
		MakeRopes();
	}

	[ExposedInEditor(null)]
	public void MakeRopes()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		if (_ropes == null)
		{
			_ropes = new Rope[_ropeSets.Length];
			for (int i = 0; i < _ropes.Length; i++)
			{
				if ((Object)(object)_ropes[i] == (Object)null)
				{
					GameObject val = (GameObject)Object.Instantiate((Object)(object)_ropePrefab, ((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
					Rope component = val.GetComponent<Rope>();
					if (!((Object)(object)component == (Object)null))
					{
						_ropes[i] = component;
					}
				}
			}
		}
		for (int j = 0; j < _ropes.Length; j++)
		{
			_ropes[j].Init(_ropeSets[j].AttachmentB, _ropeSets[j].AttachmentA, _ropeSets[j].Length);
			_ropeSets[j].SqrLength = _ropeSets[j].Length * _ropeSets[j].Length;
		}
	}

	private void LateUpdate()
	{
		if (_ropes != null)
		{
			for (int i = 0; i < _ropes.Length; i++)
			{
				_ropes[i].UpdateBones();
			}
		}
	}
}
