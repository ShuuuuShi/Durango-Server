using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.Model;

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
			_position = Transform.position;
			_rot = Transform.rotation;
		}

		public void CancelingOrientations()
		{
			Transform.rotation = Quaternion.Lerp(Transform.rotation, _rot, CancelingWeight);
			Transform.position = Vector3.Lerp(Transform.position, _position, CancelingWeight);
		}
	}

	private static readonly Vector2[] DefaultFlinchingLerpSample = new Vector2[8]
	{
		new Vector2(0f, 0f),
		new Vector2(0.06f, 1f),
		new Vector2(0.125f, 0.9f),
		new Vector2(0.18f, 1f),
		new Vector2(0.25f, 0.9f),
		new Vector2(0.31f, 0.95f),
		new Vector2(0.7f, 0f),
		new Vector2(1f, 0f)
	};

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
		if (!Singleton<PlayerController>.HasInstance())
		{
			base.enabled = false;
			return;
		}
		_characterOwner = base.gameObject.GetComponent<CharacterBehavior>();
		CacheLimbBones();
	}

	private static bool IsLimbBone(string boneName)
	{
		return _limbBonesNames.Contains(boneName);
	}

	private void CacheLimbBones()
	{
		_limbBones.Clear();
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			if (IsLimbBone(componentsInChildren[i].name))
			{
				_limbBones.Add(componentsInChildren[i]);
			}
		}
	}

	public void TakeBoneFlinching(Transform flinchBone)
	{
		_boneFlinchBeginTime = Time.time;
		_boneFlinchList.Clear();
		if (null == flinchBone)
		{
			return;
		}
		UpdateCancelingLimbBones(flinchBone);
		Vector3 vector = Maths.RandomSignVector(1f);
		float num = 1f;
		for (int i = 0; i < _numIter; i++)
		{
			BoneFlinchInfo item = default(BoneFlinchInfo);
			item.BoneFlinchHitBone = flinchBone;
			item.BoneFlinchInitialDisplace = vector * _flinchDeg * 0.2f * num * 57.29578f;
			_boneFlinchList.Add(item);
			if (!(null == flinchBone.parent) && !(null == flinchBone.parent.parent))
			{
				Transform parent = flinchBone.parent;
				if (!IsFlinchProhibitedBone(parent))
				{
					flinchBone = parent;
					num *= 0.7f;
					if (null == flinchBone)
					{
						break;
					}
					continue;
				}
				break;
			}
			break;
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
			if (!(flinchBone.name == _limbBones[i].name))
			{
				if ((bool)_limbBones[i].parent)
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
		if (!(PlayerBehavior.LocalPlayer == null) && _characterOwner.WillBeRendered && !((PlayerBehavior.LocalPlayer.CurrentPosition - _characterOwner.CurrentPosition).magnitude > _cullDistance))
		{
			AccumulateBoneFlinching();
		}
	}

	public void AccumulateBoneFlinching()
	{
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
				if (boneFlinchInfo.BoneFlinchHitBone != null)
				{
					float num = SampleFlinching(flPercent);
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

	public static float SampleFlinching(float flPercent, Vector2[] flinchingLerpSample = null)
	{
		if (flinchingLerpSample == null)
		{
			flinchingLerpSample = DefaultFlinchingLerpSample;
		}
		int num = flinchingLerpSample.Length;
		for (int i = 1; i < num; i++)
		{
			if (flPercent < flinchingLerpSample[i].x)
			{
				float num2 = flinchingLerpSample[i].x - flinchingLerpSample[i - 1].x;
				if (num2 > 0f)
				{
					float t = (flPercent - flinchingLerpSample[i - 1].x) / num2;
					return Mathf.Lerp(flinchingLerpSample[i - 1].y, flinchingLerpSample[i].y, t);
				}
			}
		}
		return flinchingLerpSample[flinchingLerpSample.Length - 1].y;
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
		if (null == bone)
		{
			return true;
		}
		if (null == bone.parent)
		{
			return true;
		}
		if (null == bone.parent.parent)
		{
			return true;
		}
		if (bone.name == "Bip001")
		{
			return true;
		}
		if (bone.name == "Bip001_Pelvis")
		{
			return true;
		}
		if (bone.name.Contains("Bip001_Belly"))
		{
			return true;
		}
		if (bone.name.Contains("Attachment"))
		{
			return true;
		}
		if (!bone.name.Contains("Bip001_"))
		{
			return true;
		}
		return false;
	}
}
