using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class BoneMergeable
{
	private class BoneMergeSet
	{
		public GameObject GameObj;

		public readonly List<Transform> Sources = new List<Transform>();

		public readonly List<Transform> Followers = new List<Transform>();
	}

	private GameObject _gameObject;

	private IMeshCloner _meshCloner;

	private Transform _meshObjectTransform;

	private Transform _rootBone;

	private readonly List<BoneMergeSet> _boneMergeSets = new List<BoneMergeSet>();

	public BoneMergeable(GameObject gameObject, IMeshCloner meshCloner, Transform meshObjectTransform, Transform rootBone)
	{
		_gameObject = gameObject;
		_meshCloner = meshCloner;
		_meshObjectTransform = meshObjectTransform;
		_rootBone = rootBone;
	}

	public void AttachBoneMergeTwoObjects([NotNull] GameObject obj, [NotNull] Transform secondaryParent, string[] secondaryAttachmentNames)
	{
		AttachBoneMerge(obj);
		if (_boneMergeSets.Count == 0)
		{
			return;
		}
		if (secondaryAttachmentNames == null)
		{
			Debug.LogError((object)"Needs to define secondaryAttachmentNames!");
			return;
		}
		BoneMergeSet boneMergeSet = _boneMergeSets[_boneMergeSets.Count - 1];
		int count = boneMergeSet.Sources.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			if (secondaryAttachmentNames.IndexOf(((Object)boneMergeSet.Sources[num]).name) != -1)
			{
				boneMergeSet.Sources.RemoveAt(num);
				boneMergeSet.Followers.RemoveAt(num);
			}
		}
		BoneMerge(secondaryParent, obj, secondaryAttachmentNames);
	}

	public void AttachBoneMerge([NotNull] GameObject obj)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		int count = _boneMergeSets.Count;
		for (int i = 0; i < count; i++)
		{
			if ((Object)(object)_boneMergeSets[i].GameObj == (Object)(object)obj)
			{
				return;
			}
		}
		if ((Object)(object)_meshObjectTransform == (Object)null)
		{
			Debug.LogError((object)"MeshObjectTransform is null");
			return;
		}
		string partName = CharacterCostume.GetPartName(CharacterCostume.CostumeType.Equipment);
		Transform val = _meshObjectTransform.FindChild(partName);
		if ((Object)(object)val == (Object)null)
		{
			GameObject val2 = new GameObject(partName);
			val = val2.transform;
			val.parent = _meshObjectTransform;
			val.localPosition = Vector3.zero;
			val.localRotation = Quaternion.identity;
			val.localScale = Vector3.one;
		}
		obj.transform.parent = val;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		if (BoneMerge(_rootBone, obj) && Application.isPlaying)
		{
			_meshCloner.AddMeshCloners(obj.GetComponentsInChildren<SkinnedMeshRenderer>());
		}
	}

	private bool BoneMerge(Transform sourceTrans, GameObject obj, string[] attachmentNames = null)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		BoneMergeSet boneMergeSet = new BoneMergeSet();
		boneMergeSet.GameObj = obj;
		Transform[] componentsInChildren = ((Component)sourceTrans).GetComponentsInChildren<Transform>(true);
		Transform[] componentsInChildren2 = ((Component)obj.transform).GetComponentsInChildren<Transform>(true);
		foreach (Transform val in componentsInChildren2)
		{
			foreach (Transform val2 in componentsInChildren)
			{
				if ((attachmentNames == null || attachmentNames.IndexOf(((Object)val2).name) != -1) && ((Object)val2).name == ((Object)val).name)
				{
					boneMergeSet.Sources.Add(val2);
					boneMergeSet.Followers.Add(val);
					val.position = val2.position;
					val.rotation = val2.rotation;
				}
			}
		}
		if (boneMergeSet.Sources.Count == 0)
		{
			return false;
		}
		_boneMergeSets.Add(boneMergeSet);
		return true;
	}

	public void DetachBoneMerge([NotNull] GameObject obj)
	{
		_meshCloner.RemoveMeshCloners(obj.GetComponentsInChildren<SkinnedMeshRenderer>());
		int count = _boneMergeSets.Count;
		for (int i = 0; i < count; i++)
		{
			if ((Object)(object)_boneMergeSets[i].GameObj == (Object)(object)obj)
			{
				_boneMergeSets.RemoveAt(i);
				break;
			}
		}
	}

	public void UpdateBoneMergeSet()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		int count = _boneMergeSets.Count;
		for (int i = 0; i < count; i++)
		{
			BoneMergeSet boneMergeSet = _boneMergeSets[i];
			if (!((Object)(object)boneMergeSet.GameObj == (Object)null) && boneMergeSet.GameObj.activeSelf)
			{
				for (int num = boneMergeSet.Sources.Count - 1; num >= 0; num--)
				{
					Transform val = boneMergeSet.Sources[num];
					Transform val2 = boneMergeSet.Followers[num];
					val2.position = val.position;
					val2.rotation = val.rotation;
				}
			}
		}
	}
}
