using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.Model;

public class JiggleBonesController : MonoBehaviour
{
	[Serializable]
	public class JiggleBoneData
	{
		public Transform Bone;

		public float Length;

		public float TipMass;

		public bool UseForwardGravity;

		public float YawStiffness;

		public float YawDamping;

		public float PitchStiffness;

		public float PitchDamping;

		public float AngleLimit;

		public JiggleBoneData Clone()
		{
			JiggleBoneData jiggleBoneData = new JiggleBoneData();
			jiggleBoneData.CopyFrom(this);
			jiggleBoneData.Bone = Bone;
			return jiggleBoneData;
		}

		public void CopyFrom(JiggleBoneData data)
		{
			Length = data.Length;
			TipMass = data.TipMass;
			UseForwardGravity = data.UseForwardGravity;
			YawStiffness = data.YawStiffness;
			YawDamping = data.YawDamping;
			PitchStiffness = data.PitchStiffness;
			PitchDamping = data.PitchDamping;
			AngleLimit = data.AngleLimit;
		}
	}

	[Serializable]
	public class SphereConstraint
	{
		[SerializeField]
		private TransformResolver _bone;

		[SerializeField]
		private Vector3 _center;

		[SerializeField]
		private float _radius;

		public Transform Bone => _bone;

		public string BoneName => _bone.Name;

		public Vector3 WorldCenter { get; set; }

		public float Radius { get; private set; }

		public float RadiusSquared { get; private set; }

		public void Update()
		{
			if (_bone.IsValid)
			{
				WorldCenter = Bone.TransformPoint(_center);
				float x = Bone.lossyScale.x;
				Radius = _radius * x;
				RadiusSquared = Radius * Radius;
			}
		}

		public bool UpdateBone(IDictionary<string, Transform> childTransforms)
		{
			return _bone.Resolve(childTransforms);
		}

		public bool UpdateBone(IList<Transform> childTransforms)
		{
			return _bone.Resolve(childTransforms);
		}
	}

	public class JiggleBone
	{
		private readonly JiggleBoneData _data;

		private readonly Transform _parent;

		private readonly Vector3 _basePos;

		private readonly Vector3 _baseForward;

		private readonly Vector3 _baseUp;

		private readonly Vector3 _baseLeft;

		private float _lastUpdateTime = -1f;

		private Vector3 _tipPos;

		private Vector3 _tipVel;

		private Vector3 _tipAccel;

		public CharacterCostume.CostumeType Type { get; set; }

		public JiggleBone(JiggleBoneData data, JiggleBoneData original = null, JiggleBonesController originalObj = null)
		{
			_data = data;
			_parent = data.Bone.parent;
			_basePos = data.Bone.localPosition;
			Quaternion localRotation = data.Bone.localRotation;
			_baseForward = -(localRotation * Vector3.right);
			_baseUp = localRotation * Vector3.up;
			_baseLeft = localRotation * Vector3.forward;
		}

		public void Update(List<SphereConstraint> sphereConstraints)
		{
			if (!(_data.Bone == null))
			{
				float x = _data.Bone.lossyScale.x;
				float num = x * _data.Length;
				Vector3 vector = _parent.TransformPoint(_basePos);
				Vector3 vector2 = _parent.TransformDirection(_baseForward);
				Vector3 goalUp = _parent.TransformDirection(_baseUp);
				Vector3 vector3 = _parent.TransformDirection(_baseLeft);
				Vector3 vector4 = vector + num * vector2;
				float time = Time.time;
				if (_lastUpdateTime < 0f || time - _lastUpdateTime > 0.5f)
				{
					Init(time, vector4);
				}
				float delta = Mathf.Min(time - _lastUpdateTime, 0.066f);
				_lastUpdateTime = time;
				ApplyEuler(delta, vector2, goalUp, vector3, vector4, x);
				ApplySphereConstraint(sphereConstraints);
				Vector3 vector5 = ApplyLimit(vector2, vector4, vector, num);
				Vector3 vector6 = Vector3.Cross(vector5, vector3);
				vector6.Normalize();
				Vector3 vector7 = Vector3.Cross(vector6, vector5);
				Quaternion rotation = Quaternion.LookRotation(vector7, vector6);
				_data.Bone.rotation = rotation;
				_data.Bone.position = vector;
				if (_showDebugLine)
				{
					float num2 = x * 50f;
					Debug.DrawLine(vector, vector + vector6 * num2, Color.green);
					Debug.DrawLine(vector, vector + vector7 * num2, Color.blue);
					Debug.DrawLine(vector, _tipPos, Color.red);
					float num3 = x * 5f;
					Debug.DrawLine(_tipPos - vector7 * num3, _tipPos + vector7 * num3, Color.red);
					Debug.DrawLine(_tipPos - vector6 * num3, _tipPos + vector6 * num3, Color.red);
					Debug.DrawLine(vector, _tipVel * 100f, Color.magenta);
				}
			}
		}

		public void Destroy()
		{
			if (Application.isPlaying)
			{
				_data.Bone.gameObject.SetActive(value: false);
				UnityEngine.Object.Destroy(_data.Bone.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_data.Bone.gameObject);
			}
		}

		private void Init(float time, Vector3 tipPos)
		{
			_lastUpdateTime = time;
			_tipPos = tipPos;
			_tipVel = Vector3.zero;
			_tipAccel = Vector3.zero;
		}

		private void ApplyEuler(float delta, Vector3 goalForward, Vector3 goalUp, Vector3 goalLeft, Vector3 goalTip, float scale)
		{
			if (_data.UseForwardGravity)
			{
				_tipAccel += _data.TipMass * goalForward * scale;
			}
			else
			{
				_tipAccel.y -= _data.TipMass * scale;
			}
			Vector3 rhs = goalTip - _tipPos;
			Vector3 vector = default(Vector3);
			vector.x = Vector3.Dot(goalForward, rhs);
			vector.y = Vector3.Dot(goalUp, rhs);
			vector.z = Vector3.Dot(goalLeft, rhs);
			Vector3 vector2 = default(Vector3);
			vector2.y = Vector3.Dot(goalUp, _tipVel);
			vector2.z = Vector3.Dot(goalLeft, _tipVel);
			float num = _data.YawStiffness * vector.z - _data.YawDamping * vector2.z;
			float num2 = _data.PitchStiffness * vector.y - _data.PitchDamping * vector2.y;
			_tipAccel += num * goalLeft + num2 * goalUp;
			_tipVel += _tipAccel * delta;
			_tipPos += _tipVel * delta;
			_tipAccel = Vector3.zero;
		}

		private void ApplySphereConstraint(List<SphereConstraint> sphereConstraints)
		{
			foreach (SphereConstraint sphereConstraint in sphereConstraints)
			{
				Vector3 vector = _tipPos - sphereConstraint.WorldCenter;
				float sqrMagnitude = vector.sqrMagnitude;
				if (!(sqrMagnitude >= sphereConstraint.RadiusSquared))
				{
					vector.Normalize();
					_tipPos = sphereConstraint.WorldCenter + vector * sphereConstraint.Radius;
					_tipVel = Vector3.zero;
				}
			}
		}

		private Vector3 ApplyLimit(Vector3 goalForward, Vector3 goalTip, Vector3 goalBasePosition, float length)
		{
			Vector3 vector = _tipPos - goalBasePosition;
			vector.Normalize();
			float f = Vector3.Dot(vector, goalForward);
			float num = Mathf.Acos(f) * 57.29578f;
			if (num > _data.AngleLimit)
			{
				float num2 = length * Mathf.Sin(_data.AngleLimit * ((float)Math.PI / 180f));
				Vector3 vector2 = goalTip - _tipPos;
				vector2.Normalize();
				_tipPos = goalTip - num2 * vector2;
				vector = _tipPos - goalBasePosition;
				vector.Normalize();
			}
			_tipPos = goalBasePosition + length * vector;
			_tipVel -= Vector3.Dot(_tipVel, vector) * vector;
			return vector;
		}
	}

	[ExposedInEditor(null)]
	private static bool _showDebugLine;

	[SerializeField]
	private List<JiggleBoneData> _jiggleBonesData = new List<JiggleBoneData>();

	[SerializeField]
	private List<SphereConstraint> _sphereConstraints = new List<SphereConstraint>();

	private readonly List<JiggleBone> _jiggleBones = new List<JiggleBone>();

	private CharacterBehavior _owner;

	private void Awake()
	{
		Reset();
		_owner = GetComponent<CharacterBehavior>();
		UpdateFramework();
	}

	public void UpdateFramework(Dictionary<string, Transform> childTransformCache = null)
	{
		if (_owner == null)
		{
			return;
		}
		if (childTransformCache == null)
		{
			childTransformCache = new Dictionary<string, Transform>();
			Transform child = _owner.transform.GetChild(0);
			Transform[] componentsInChildren = child.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				childTransformCache[transform.name] = transform;
			}
		}
		foreach (SphereConstraint sphereConstraint in _sphereConstraints)
		{
			if (sphereConstraint.UpdateBone(childTransformCache))
			{
			}
		}
	}

	[ExposedInEditor(null)]
	private void Reset()
	{
		_jiggleBones.Clear();
		foreach (JiggleBoneData jiggleBonesDatum in _jiggleBonesData)
		{
			if (!(jiggleBonesDatum.Bone == null))
			{
				JiggleBone item = new JiggleBone(jiggleBonesDatum);
				_jiggleBones.Add(item);
			}
		}
	}

	private void LateUpdate()
	{
		if ((!(_owner != null) || _owner.WillBeRendered) && _jiggleBones.Count > 0)
		{
			int i = 0;
			for (int count = _sphereConstraints.Count; i < count; i++)
			{
				SphereConstraint sphereConstraint = _sphereConstraints[i];
				sphereConstraint.Update();
			}
			int j = 0;
			for (int count2 = _jiggleBones.Count; j < count2; j++)
			{
				JiggleBone jiggleBone = _jiggleBones[j];
				jiggleBone.Update(_sphereConstraints);
			}
		}
	}

	public void Remove(CharacterCostume.CostumeType type)
	{
		for (int num = _jiggleBones.Count - 1; num >= 0; num--)
		{
			JiggleBone jiggleBone = _jiggleBones[num];
			if (jiggleBone.Type == type)
			{
				jiggleBone.Destroy();
				_jiggleBones.RemoveAt(num);
				_jiggleBonesData.RemoveAt(num);
			}
		}
	}

	public void Add(JiggleBonesController srcController, Transform[] dstBones, CharacterCostume.CostumeType type)
	{
		List<JiggleBoneData> jiggleBonesData = srcController._jiggleBonesData;
		for (int num = jiggleBonesData.Count - 1; num >= 0; num--)
		{
			JiggleBoneData jiggleBoneData = jiggleBonesData[num];
			if (jiggleBoneData.Bone == null)
			{
				jiggleBonesData.RemoveAt(num);
			}
		}
		int size = KUtility.GetSize(jiggleBonesData);
		Transform[] array = new Transform[size];
		for (int i = 0; i < size; i++)
		{
			Transform bone = jiggleBonesData[i].Bone;
			GameObject gameObject = new GameObject(bone.name);
			array[i] = gameObject.transform;
		}
		for (int j = 0; j < size; j++)
		{
			JiggleBoneData jiggleBoneData2 = jiggleBonesData[j];
			JiggleBoneData jiggleBoneData3 = jiggleBoneData2.Clone();
			string parentName = jiggleBoneData3.Bone.parent.name;
			Transform transform = FindParent(dstBones, parentName);
			if (transform == null)
			{
				transform = FindParent(array, parentName);
			}
			jiggleBoneData3.Bone = array[j];
			jiggleBoneData3.Bone.parent = transform;
			jiggleBoneData3.Bone.localPosition = jiggleBoneData2.Bone.localPosition;
			jiggleBoneData3.Bone.localRotation = jiggleBoneData2.Bone.localRotation;
			jiggleBoneData3.Bone.localScale = jiggleBoneData2.Bone.localScale;
			JiggleBone jiggleBone = new JiggleBone(jiggleBoneData3, jiggleBoneData2, srcController);
			jiggleBone.Type = type;
			_jiggleBones.Add(jiggleBone);
			_jiggleBonesData.Add(jiggleBoneData3);
		}
	}

	private static Transform FindParent(Transform[] dstBones, string parentName)
	{
		int size = KUtility.GetSize(dstBones);
		for (int i = 0; i < size; i++)
		{
			if (dstBones[i].name == parentName)
			{
				return dstBones[i];
			}
		}
		return null;
	}
}
