using System;
using L10N;
using Messages;
using Shared.Region;
using UnityEngine;

namespace Durango.UI;

public class WorldRoutesBeginnerStableArea : MonoBehaviour
{
	[Serializable]
	private class Node
	{
		public Vector2 Position;

		public GameObject NodeObject;

		public UILabel RegionNameLabel;
	}

	[SerializeField]
	private Node _tutorial;

	[SerializeField]
	private Node _safehouse;

	[SerializeField]
	private ExplorePersonalNode _personal;

	private WorldRoutesViewer _parent;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_parent = UIUtility.FindComponentInParent<WorldRoutesViewer>(base.gameObject);
			_tutorial.RegionNameLabel.text = Role.Tutorial.GetName() + "\n" + LocalizeUtil.FormatLevel(1);
			_safehouse.RegionNameLabel.text = Role.Safehouse.GetName() + "\n" + LocalizeUtil.FormatLevel(5);
		}
	}

	public void Set()
	{
		Init();
	}

	public void SetPersonal(PersonalRegion? personalRegion)
	{
		if (!personalRegion.HasValue)
		{
			_personal.SetEmpty();
			return;
		}
		_personal.Set(personalRegion.Value.Region);
		if (GameManager.Region.Id == personalRegion.Value.Region.Id)
		{
			_parent.SetCurrentCursor(_personal.transform);
		}
	}

	public void ProcessRegionNode(Action<Transform, string> func)
	{
		func(_personal.transform, _personal.ActivatedRegionId);
	}

	public void UpdateLayout(global::System.Random rand)
	{
		if (_isInit)
		{
			UIWidget component = GetComponent<UIWidget>();
			Vector2 pos = component.localCorners[0];
			Vector2 localSize = component.localSize;
			UpdateRegionNodePosition(rand, pos, localSize, _tutorial.NodeObject.transform, _tutorial.Position);
			UpdateRegionNodePosition(rand, pos, localSize, _safehouse.NodeObject.transform, _safehouse.Position);
			UpdateRegionNodePosition(rand, pos, localSize, _personal.transform, _personal.Position);
		}
	}

	private void UpdateRegionNodePosition(global::System.Random rand, Vector2 pos, Vector2 size, Transform node, Vector2 p)
	{
		pos += new Vector2(size.x * p.x, size.y * p.y);
		Vector2 randomPositionOffset = WorldRoutesViewer.GetRandomPositionOffset(rand);
		node.localPosition = pos + randomPositionOffset;
	}

	public Transform FindRegionNode(Role role)
	{
		if (role == Role.Personal)
		{
			return _personal.transform;
		}
		return null;
	}
}
