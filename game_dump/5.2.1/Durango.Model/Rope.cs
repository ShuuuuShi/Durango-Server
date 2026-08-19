using UnityEngine;

namespace Durango.Model;

public class Rope : MonoBehaviour
{
	[SerializeField]
	private Transform[] _bones;

	[SerializeField]
	private float _velocityGain;

	[SerializeField]
	private float _gravityGain;

	[SerializeField]
	private float _groundYLimit;

	[SerializeField]
	private int _solveIteration = 3;

	private Transform _attachmentStart;

	private Transform _attachmentEnd;

	private float _length;

	private const float TimeInterval = 0.03f;

	private const float TimeIntervalSqr = 0.0009f;

	private Vector3[] _oldPositions;

	private SkinnedMeshRenderer _renderer;

	public void Init(Transform carrierAttachment, Transform cartAttachment, float length, float thickness)
	{
		if (_bones.Length != 0)
		{
			_oldPositions = new Vector3[_bones.Length];
			_attachmentStart = carrierAttachment;
			_attachmentEnd = cartAttachment;
			Vector3 vector = _attachmentEnd.position - _attachmentStart.position;
			for (int i = 0; i < _bones.Length; i++)
			{
				float num = (float)i / (float)_bones.Length;
				Vector3 vector2 = _attachmentStart.position + vector * num;
				_bones[i].transform.position = vector2;
				_oldPositions[i] = vector2;
			}
			_length = length;
			if (thickness > 0f)
			{
				base.transform.localScale = new Vector3(thickness, thickness, thickness);
			}
			_solveIteration = Mathf.Min(3, _solveIteration);
			_renderer = GetComponentInChildren<SkinnedMeshRenderer>();
		}
	}

	public void UpdateBones()
	{
		if (_renderer != null)
		{
			ref Vector3 reference = ref _oldPositions[0];
			reference = _attachmentStart.position;
			_bones[0].transform.position = _attachmentStart.position;
			ref Vector3 reference2 = ref _oldPositions[_bones.Length - 1];
			reference2 = _attachmentEnd.position;
			_bones[_bones.Length - 1].transform.position = _attachmentEnd.position;
			int num = ((!_renderer.isVisible) ? 1 : ((int)(Time.deltaTime / 0.03f) + 1));
			for (int i = 0; i < num; i++)
			{
				VerletIntegrate();
				SolveConstraints();
			}
		}
	}

	private void VerletIntegrate()
	{
		for (int i = 1; i < _bones.Length - 1; i++)
		{
			Vector3 vector = (_bones[i].transform.position - _oldPositions[i]) * (0f - _velocityGain);
			Vector3 position = _bones[i].transform.position + vector + 0.0009f * Physics.gravity * _gravityGain;
			position.y = Mathf.Max(position.y, _groundYLimit);
			ref Vector3 reference = ref _oldPositions[i];
			reference = _bones[i].transform.position;
			_bones[i].transform.position = position;
		}
	}

	private void SolveConstraints()
	{
		float desiredDistance = _length / (float)(_bones.Length - 1);
		for (int i = 0; i < _solveIteration; i++)
		{
			for (int j = 0; j < _bones.Length - 1; j++)
			{
				SolveDistanceConstraint(j, j + 1, desiredDistance);
			}
		}
	}

	private void SolveDistanceConstraint(int boneInd, int linkedBoneInd, float desiredDistance)
	{
		Vector3 vector = _bones[linkedBoneInd].transform.position - _bones[boneInd].transform.position;
		float magnitude = vector.magnitude;
		float num = (magnitude - desiredDistance) / magnitude;
		bool flag = IsFixed(boneInd);
		bool flag2 = IsFixed(linkedBoneInd);
		if (!flag && !flag2)
		{
			_bones[boneInd].transform.position += num * 0.5f * vector;
			_bones[linkedBoneInd].transform.position -= num * 0.5f * vector;
		}
		else if (!flag)
		{
			_bones[boneInd].transform.position += num * vector;
		}
		else if (!flag2)
		{
			_bones[linkedBoneInd].transform.position -= num * vector;
		}
	}

	private bool IsFixed(int index)
	{
		if (index != 0)
		{
			return index == _bones.Length - 1;
		}
		return true;
	}
}
