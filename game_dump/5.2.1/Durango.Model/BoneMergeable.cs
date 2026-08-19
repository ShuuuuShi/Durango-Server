using System;
using System.Collections.Generic;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Model;

public class BoneMergeable
{
	private class BoneMergeSet
	{
		public GameObject GameObj;

		public readonly List<Transform> Sources = new List<Transform>();

		public readonly List<Transform> Followers = new List<Transform>();
	}

	private readonly GameObject _gameObject;

	private readonly Transform _meshObjectTransform;

	private readonly Transform _rootBone;

	private readonly List<BoneMergeSet> _boneMergeSets = new List<BoneMergeSet>();

	public event Action PreUpdate;

	public BoneMergeable(GameObject gameObject, Transform meshObjectTransform, Transform rootBone)
	{
		_gameObject = gameObject;
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
			Debug.LogError("Needs to define secondaryAttachmentNames!");
			return;
		}
		BoneMergeSet boneMergeSet = _boneMergeSets[_boneMergeSets.Count - 1];
		for (int num = boneMergeSet.Sources.Count - 1; num >= 0; num--)
		{
			if (secondaryAttachmentNames.IndexOf(boneMergeSet.Sources[num].name) != -1)
			{
				boneMergeSet.Sources.RemoveAt(num);
				boneMergeSet.Followers.RemoveAt(num);
			}
		}
		BoneMerge(secondaryParent, obj, secondaryAttachmentNames);
	}

	public void AttachBoneMerge([NotNull] GameObject obj)
	{
		int count = _boneMergeSets.Count;
		for (int i = 0; i < count; i++)
		{
			if (_boneMergeSets[i].GameObj == obj)
			{
				return;
			}
		}
		if (_meshObjectTransform == null)
		{
			Debug.LogError("MeshObjectTransform is null");
			return;
		}
		Transform transform = _meshObjectTransform.Find("Equipment");
		if (transform == null)
		{
			transform = new GameObject("Equipment").transform;
			transform.parent = _meshObjectTransform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
		}
		obj.transform.parent = transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		if (_rootBone == null || !BoneMerge(_rootBone, obj) || !Application.isPlaying)
		{
			return;
		}
		using Reusable<List<IBoneMergedObserver>> reusable = ReusableList<IBoneMergedObserver>.Pop();
		SkinnedMeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		List<IBoneMergedObserver> value = reusable.Value;
		_gameObject.GetComponents(value);
		foreach (IBoneMergedObserver item in value)
		{
			item.OnAttached(componentsInChildren);
		}
	}

	private bool BoneMerge(Transform sourceTrans, GameObject obj, string[] attachmentNames = null)
	{
		BoneMergeSet boneMergeSet = new BoneMergeSet();
		boneMergeSet.GameObj = obj;
		Transform[] componentsInChildren = sourceTrans.GetComponentsInChildren<Transform>(includeInactive: true);
		Transform[] componentsInChildren2 = obj.transform.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren2)
		{
			Transform[] array = componentsInChildren;
			foreach (Transform transform2 in array)
			{
				if ((attachmentNames == null || attachmentNames.IndexOf(transform2.name) != -1) && transform2.name == transform.name)
				{
					boneMergeSet.Sources.Add(transform2);
					boneMergeSet.Followers.Add(transform);
					transform.position = transform2.position;
					transform.rotation = transform2.rotation;
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
		using (Reusable<List<IBoneMergedObserver>> reusable = ReusableList<IBoneMergedObserver>.Pop())
		{
			SkinnedMeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
			List<IBoneMergedObserver> value = reusable.Value;
			_gameObject.GetComponents(value);
			foreach (IBoneMergedObserver item in value)
			{
				item.OnDetached(componentsInChildren);
			}
		}
		int count = _boneMergeSets.Count;
		for (int i = 0; i < count; i++)
		{
			if (_boneMergeSets[i].GameObj == obj)
			{
				_boneMergeSets.RemoveAt(i);
				break;
			}
		}
	}

	public void UpdateBoneMergeSet()
	{
		if (this.PreUpdate != null)
		{
			this.PreUpdate();
		}
		int count = _boneMergeSets.Count;
		for (int i = 0; i < count; i++)
		{
			BoneMergeSet boneMergeSet = _boneMergeSets[i];
			if (boneMergeSet.GameObj == null || !boneMergeSet.GameObj.activeSelf)
			{
				continue;
			}
			for (int num = boneMergeSet.Sources.Count - 1; num >= 0; num--)
			{
				Transform transform = boneMergeSet.Sources[num];
				Transform transform2 = boneMergeSet.Followers[num];
				if (!(transform == null) && !(transform2 == null))
				{
					transform2.position = transform.position;
					transform2.rotation = transform.rotation;
				}
			}
		}
	}
}
