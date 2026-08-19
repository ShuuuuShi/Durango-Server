using System.Collections.Generic;
using UnityEngine;

public class BoneFlinchingController : MonoBehaviour
{
	public struct BoneFlinchInfo
	{
		public Transform BoneFlinchHitBone;

		public Vector3 BoneFlinchInitialDisplace;
	}

	private class LimbBoneInfo
	{
		public Transform Transform;

		public float CancelingWeight;

		private Vector3 _position;

		private Quaternion _rot;

		public void StoreCurrentOrientations()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			_position = Transform.position;
			_rot = Transform.rotation;
		}

		public void CancelingOrientations()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			Transform.rotation = Quaternion.Lerp(Transform.rotation, _rot, CancelingWeight);
			Transform.position = Vector3.Lerp(Transform.position, _position, CancelingWeight);
		}
	}

	private static HashSet<string> _limbBonesNames = new HashSet<string> { "Bip001_L_Foot", "Bip001_R_Foot", "Bip001_L_Hand", "Bip001_R_Hand" };

	private List<Transform> _limbBones = new List<Transform>();

	private List<LimbBoneInfo> _cancelingLimbBones = new List<LimbBoneInfo>();

	[SerializeField]
	private bool _isLimbCanceling = true;

	[SerializeField]
	private int _numIter = 3;

	[SerializeField]
	private float _cullDistance = 1000f;

	[SerializeField]
	private float _flinchDeg = 2f;

	[SerializeField]
	private float _flinchDuration = 1f;

	private CharacterBehavior _characterOwner;

	private float _boneFlinchBeginTime = -1f;

	private List<BoneFlinchInfo> _boneFlinchList = new List<BoneFlinchInfo>();

	private void Start()
	{
		if (!KSingleton<PlayerController>.HasInstance())
		{
			((Behaviour)this).enabled = false;
			return;
		}
		_characterOwner = ((Component)this).gameObject.GetComponent<CharacterBehavior>();
		CacheLimbBones();
	}

	private static bool IsLimbBone(string boneName)
	{
		return _limbBonesNames.Contains(boneName);
	}

	private void CacheLimbBones()
	{
		_limbBones.Clear();
		Transform[] componentsInChildren = ((Component)this).gameObject.GetComponentsInChildren<Transform>(true);
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			if (IsLimbBone(((Object)componentsInChildren[i]).name))
			{
				_limbBones.Add(componentsInChildren[i]);
			}
		}
	}

	public void TakeBoneFlinching(Transform flinchBone)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		_boneFlinchBeginTime = Time.time;
		_boneFlinchList.Clear();
		if ((Object)null == (Object)(object)flinchBone)
		{
			return;
		}
		UpdateCancelingLimbBones(flinchBone);
		Vector3 val = KMathUtil.RandomSignVector(1f);
		float num = 1f;
		for (int i = 0; i < _numIter; i++)
		{
			BoneFlinchInfo item = default(BoneFlinchInfo);
			item.BoneFlinchHitBone = flinchBone;
			item.BoneFlinchInitialDisplace = val * _flinchDeg * 0.2f * num * 57.29578f;
			_boneFlinchList.Add(item);
			if ((Object)null == (Object)(object)flinchBone.parent || (Object)null == (Object)(object)flinchBone.parent.parent)
			{
				break;
			}
			Transform parent = flinchBone.parent;
			if (IsFlinchProhibitedBone(parent))
			{
				break;
			}
			Transform val2 = parent;
			flinchBone = val2;
			num *= 0.7f;
			if ((Object)null == (Object)(object)flinchBone)
			{
				break;
			}
		}
	}

	private void UpdateCancelingLimbBones(Transform flinchBone)
	{
		if (!_isLimbCanceling)
		{
			return;
		}
		_cancelingLimbBones.Clear();
		int count = _limbBones.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(((Object)flinchBone).name == ((Object)_limbBones[i]).name))
			{
				if (Object.op_Implicit((Object)(object)_limbBones[i].parent))
				{
					_cancelingLimbBones.Add(new LimbBoneInfo
					{
						Transform = _limbBones[i].parent,
						CancelingWeight = 0.5f
					});
				}
				_cancelingLimbBones.Add(new LimbBoneInfo
				{
					Transform = _limbBones[i],
					CancelingWeight = 1f
				});
			}
		}
	}

	public void ForceUpdateFirst()
	{
		LateUpdate();
	}

	private void LateUpdate()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (_characterOwner.IsAnimPlaying && !((Object)(object)PlayerBehavior.LocalPlayer == (Object)null) && _characterOwner.IsVisible)
		{
			Vector3 val = PlayerBehavior.LocalPlayer.CurrentPosition - _characterOwner.CurrentPosition;
			if (!(((Vector3)(ref val)).magnitude > _cullDistance))
			{
				AccumulateBoneFlinching();
			}
		}
	}

	public void AccumulateBoneFlinching()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (_boneFlinchBeginTime <= 0f)
		{
			return;
		}
		if (Time.time - _boneFlinchBeginTime < _flinchDuration)
		{
			PrepareLimbCanceling();
			int count = _boneFlinchList.Count;
			float flPercent = (Time.time - _boneFlinchBeginTime) / _flinchDuration;
			for (int i = 0; i < count; i++)
			{
				BoneFlinchInfo boneFlinchInfo = _boneFlinchList[i];
				if ((Object)(object)boneFlinchInfo.BoneFlinchHitBone != (Object)null)
				{
					float num = KUtility.FlinchingFunc(flPercent);
					boneFlinchInfo.BoneFlinchHitBone.Rotate(num * boneFlinchInfo.BoneFlinchInitialDisplace);
				}
			}
			ProcessLimbCanceling();
		}
		else
		{
			_boneFlinchBeginTime = -1f;
			_boneFlinchList.Clear();
		}
	}

	private void PrepareLimbCanceling()
	{
		int count = _cancelingLimbBones.Count;
		for (int i = 0; i < count; i++)
		{
			_cancelingLimbBones[i].StoreCurrentOrientations();
		}
	}

	private void ProcessLimbCanceling()
	{
		int count = _cancelingLimbBones.Count;
		for (int i = 0; i < count; i++)
		{
			_cancelingLimbBones[i].CancelingOrientations();
		}
	}

	private bool IsFlinchProhibitedBone(Transform bone)
	{
		if ((Object)null == (Object)(object)bone)
		{
			return true;
		}
		if ((Object)null == (Object)(object)bone.parent)
		{
			return true;
		}
		if ((Object)null == (Object)(object)bone.parent.parent)
		{
			return true;
		}
		if (((Object)bone).name == "Bip001")
		{
			return true;
		}
		if (((Object)bone).name == "Bip001_Pelvis")
		{
			return true;
		}
		if (((Object)bone).name.Contains("Bip001_Belly"))
		{
			return true;
		}
		if (((Object)bone).name.Contains("Attachment"))
		{
			return true;
		}
		if (!((Object)bone).name.Contains("Bip001_"))
		{
			return true;
		}
		return false;
	}
}
