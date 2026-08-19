using UnityEngine;

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
	private int solveIteration;

	private Transform _attachmentStart;

	private Transform _attachmentEnd;

	private float _length;

	private float _timeInterval = 0.02f;

	private Vector3[] _oldPositions;

	public void Init(Transform carrierAttachment, Transform cartAttachment, float length)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (_bones.Length != 0)
		{
			_oldPositions = (Vector3[])(object)new Vector3[_bones.Length];
			_attachmentStart = carrierAttachment;
			_attachmentEnd = cartAttachment;
			Vector3 val = _attachmentEnd.position - _attachmentStart.position;
			for (int i = 0; i < _bones.Length; i++)
			{
				float num = (float)i / (float)_bones.Length;
				Vector3 val2 = _attachmentStart.position + val * num;
				((Component)_bones[i]).transform.position = val2;
				_oldPositions[i] = val2;
			}
			_length = length;
		}
	}

	public void UpdateBones()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		ref Vector3 reference = ref _oldPositions[0];
		reference = _attachmentStart.position;
		((Component)_bones[0]).transform.position = _attachmentStart.position;
		ref Vector3 reference2 = ref _oldPositions[_bones.Length - 1];
		reference2 = _attachmentEnd.position;
		((Component)_bones[_bones.Length - 1]).transform.position = _attachmentEnd.position;
		int num = (int)(Time.deltaTime / _timeInterval) + 1;
		for (int i = 0; i < num; i++)
		{
			VerletIntegrate();
			SolveConstraints();
		}
	}

	private void VerletIntegrate()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		float num = _timeInterval * _timeInterval;
		for (int i = 0; i < _bones.Length; i++)
		{
			if (!IsFixed(i))
			{
				Vector3 val = (((Component)_bones[i]).transform.position - _oldPositions[i]) * (0f - _velocityGain);
				Vector3 position = ((Component)_bones[i]).transform.position + val + num * Physics.gravity * _gravityGain;
				position.y = Mathf.Max(position.y, _groundYLimit);
				ref Vector3 reference = ref _oldPositions[i];
				reference = ((Component)_bones[i]).transform.position;
				((Component)_bones[i]).transform.position = position;
			}
		}
	}

	private void SolveConstraints()
	{
		float desiredDistance = _length / (float)(_bones.Length - 1);
		for (int i = 0; i < solveIteration; i++)
		{
			for (int j = 0; j < _bones.Length - 1; j++)
			{
				SolveDistanceConstraint(j, j + 1, desiredDistance);
			}
		}
	}

	private void SolveDistanceConstraint(int boneInd, int linkedBoneInd, float desiredDistance)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)_bones[linkedBoneInd]).transform.position - ((Component)_bones[boneInd]).transform.position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		float num = (magnitude - desiredDistance) / magnitude;
		if (!IsFixed(boneInd) && !IsFixed(linkedBoneInd))
		{
			Transform transform = ((Component)_bones[boneInd]).transform;
			transform.position += num * 0.5f * val;
			Transform transform2 = ((Component)_bones[linkedBoneInd]).transform;
			transform2.position -= num * 0.5f * val;
		}
		else if (!IsFixed(boneInd))
		{
			Transform transform3 = ((Component)_bones[boneInd]).transform;
			transform3.position += num * val;
		}
		else if (!IsFixed(linkedBoneInd))
		{
			Transform transform4 = ((Component)_bones[linkedBoneInd]).transform;
			transform4.position -= num * val;
		}
	}

	private bool IsFixed(int index)
	{
		return index == 0 || index == _bones.Length - 1;
	}
}
