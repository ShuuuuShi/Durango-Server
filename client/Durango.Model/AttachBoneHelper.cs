using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Model;

public class AttachBoneHelper
{
	private class BoneAttach
	{
		private readonly AttachBoneHelper _parent;

		private readonly Transform _root;

		private BoneInfo _original;

		private BoneInfo _current;

		private GameObject _attachModel;

		private Transform _modelLink;

		private string _path;

		private bool _isDirty;

		private bool _visible = true;

		private bool _prevVisible;

		public BoneAttach(AttachBoneHelper parent, Transform root, Transform modelLink)
		{
			_parent = parent;
			_root = root;
			_modelLink = modelLink;
		}

		public void SetVisible(bool visible)
		{
			if (visible != _visible)
			{
				_visible = visible;
				UpdateVisible();
			}
		}

		private void UpdateVisible()
		{
			if (_attachModel == null)
			{
				return;
			}
			bool flag = _visible && Get().Parent != null;
			_attachModel.gameObject.SetActive(flag);
			if (_prevVisible != flag)
			{
				_prevVisible = flag;
				if (_parent.VisibleChanged != null)
				{
					_parent.VisibleChanged(_attachModel, flag);
				}
			}
		}

		public void SetDirty(Transform link)
		{
			if (!(_modelLink == link))
			{
				_modelLink = link;
				_isDirty = true;
				UpdateAttachModel();
			}
		}

		public void Create(GameObject obj, string path)
		{
			Destory();
			SetPath(path);
			if (!(obj == null))
			{
				_attachModel = UnityEngine.Object.Instantiate(obj);
				UpdateAttachModel();
			}
		}

		public void Destory()
		{
			if (_attachModel == null)
			{
				_prevVisible = false;
				return;
			}
			if (_prevVisible)
			{
				_prevVisible = false;
				if (_parent.VisibleChanged != null)
				{
					_parent.VisibleChanged(_attachModel, arg2: false);
				}
			}
			UnityEngine.Object.Destroy(_attachModel);
			_attachModel = null;
		}

		private void SetPath(string path)
		{
			_path = path;
			_isDirty = true;
			Transform transform = ((!(_root == null)) ? KUtility.FindTransformByName(_root.gameObject, path) : null);
			if (!(transform == null))
			{
				_original = new BoneInfo(transform);
				_original.Parent = transform.parent;
			}
		}

		private void UpdateAttachModel()
		{
			if (!(_attachModel == null))
			{
				BoneInfo boneInfo = Get();
				Transform transform = _attachModel.transform;
				transform.parent = boneInfo.Parent;
				if (transform.parent == null)
				{
					transform.parent = _root;
				}
				transform.localPosition = boneInfo.Position;
				transform.localRotation = boneInfo.Quaterion;
				transform.localScale = boneInfo.Scale;
				for (int i = 0; i < transform.childCount; i++)
				{
					Transform child = transform.GetChild(i);
					child.localPosition = Vector3.zero;
					child.localRotation = Quaternion.identity;
					child.localScale = Vector3.one;
				}
				UpdateVisible();
			}
		}

		private BoneInfo Get()
		{
			if (!_isDirty)
			{
				return _current;
			}
			_isDirty = false;
			_current = _original;
			if (_modelLink != null)
			{
				Transform transform = KUtility.FindTransformByName(_modelLink.gameObject, _path);
				if (transform != null)
				{
					_current = new BoneInfo(transform);
					if (transform.parent != null)
					{
						Transform parent = ((!(_root == null)) ? KUtility.FindTransformByName(_root.gameObject, transform.parent.name) : null);
						_current.Parent = parent;
					}
				}
			}
			return _current;
		}
	}

	private struct BoneInfo
	{
		public Transform Parent;

		public readonly Vector3 Position;

		public readonly Vector3 Scale;

		public readonly Quaternion Quaterion;

		public BoneInfo(Transform t)
		{
			Parent = null;
			Position = t.localPosition;
			Scale = t.localScale;
			Quaterion = t.localRotation;
		}
	}

	private readonly Dictionary<string, BoneAttach> _boneInfos = new Dictionary<string, BoneAttach>();

	private readonly Transform _root;

	private Transform _modelLink;

	public event Action<GameObject, bool> VisibleChanged;

	public AttachBoneHelper(Transform root)
	{
		_root = root;
	}

	public void SetModelLink(Transform t)
	{
		_modelLink = t;
		foreach (KeyValuePair<string, BoneAttach> boneInfo in _boneInfos)
		{
			boneInfo.Value.SetDirty(t);
		}
	}

	public void AddAttach(string key, string bonePath, GameObject obj)
	{
		BoneAttach orAdd = GetOrAdd(key);
		orAdd.Create(obj, bonePath);
	}

	public void RemoveAttach(string key)
	{
		_boneInfos.Get(key)?.Destory();
	}

	public void SetVisible(string key, bool visible)
	{
		BoneAttach orAdd = GetOrAdd(key);
		orAdd.SetVisible(visible);
	}

	[NotNull]
	private BoneAttach GetOrAdd(string key)
	{
		BoneAttach boneAttach = _boneInfos.Get(key);
		if (boneAttach != null)
		{
			return boneAttach;
		}
		boneAttach = new BoneAttach(this, _root, _modelLink);
		_boneInfos.Add(key, boneAttach);
		return boneAttach;
	}
}
