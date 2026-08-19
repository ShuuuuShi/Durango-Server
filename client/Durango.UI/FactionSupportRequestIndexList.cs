using System;
using Durango.Logic.Faction;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class FactionSupportRequestIndexList : KScrollViewBase
{
	public Action<int> LevelSelected;

	[SerializeField]
	private FactionSupportRequestIndexNode _baseNode;

	[SerializeField]
	private UISprite _separatorBase;

	private Vector2 _baseNodeSize;

	private float _prevNodeOffset = -1f;

	private ListObjectPool<FactionSupportRequestIndexNode> _nodes;

	private ListObjectPool<UISprite> _separators;

	public float NodeSize => GetSize(_baseNodeSize) + (float)base.Margin;

	public ListObjectPool<FactionSupportRequestIndexNode> Nodes
	{
		get
		{
			if (_nodes == null)
			{
				_baseNodeSize = _baseNode.localSize;
				_nodes = new ListObjectPool<FactionSupportRequestIndexNode>();
				_nodes.BaseObject = _baseNode;
				_nodes.Init(delegate(FactionSupportRequestIndexNode comp)
				{
					comp.Clicked = (Action<FactionSupportRequestIndexNode>)Delegate.Combine(comp.Clicked, new Action<FactionSupportRequestIndexNode>(OnClickNode));
				});
			}
			return _nodes;
		}
	}

	private ListObjectPool<UISprite> Separators
	{
		get
		{
			if (_separators == null)
			{
				_separators = new ListObjectPool<UISprite>();
				_separators.BaseObject = _separatorBase;
				_separators.UseBase = true;
			}
			return _separators;
		}
	}

	public void OnLateUpdate()
	{
		float num = base.CurrentOffset / NodeSize;
		if (_prevNodeOffset == num)
		{
			return;
		}
		_prevNodeOffset = num;
		int num2 = Mathf.FloorToInt(num);
		float num3 = num - (float)num2;
		for (int i = 0; i < Nodes.Count; i++)
		{
			FactionSupportRequestIndexNode factionSupportRequestIndexNode = Nodes[i];
			if (i == num2)
			{
				factionSupportRequestIndexNode.SetSelectRatio(1f - num3);
			}
			else if (i == num2 + 1)
			{
				factionSupportRequestIndexNode.SetSelectRatio(num3);
			}
			else
			{
				factionSupportRequestIndexNode.SetSelectRatio(0f);
			}
		}
	}

	public override UIWidget GetNode(int index)
	{
		return Nodes[index].GetComponent<UIWidget>();
	}

	public override int GetNodeCount()
	{
		return Nodes.Count;
	}

	protected override float OnUpdateLayout(bool instant)
	{
		float size = GetSize(base.ViewSize);
		float size2 = GetSize(_baseNodeSize);
		base.Padding = Mathf.RoundToInt((size - size2) * 0.5f);
		base.EndPadding = base.Padding;
		Vector3 basePosition = GetBasePosition();
		Vector3 localPosition = Separators.BaseObject.transform.localPosition;
		Vector2 vector = base.Vector;
		switch (base.Dir)
		{
		case Direction.Vertical:
			localPosition.y = basePosition.y;
			break;
		case Direction.Horizontal:
			localPosition.x = basePosition.x;
			break;
		}
		for (int i = 0; i < Separators.Count; i++)
		{
			Separators[i].transform.localPosition = localPosition + (Vector3)(vector * size2 * i);
		}
		return UIUtility.WidgetsReposition(Nodes, base.Vector, basePosition, base.Margin, 0f, instant);
	}

	private void OnClickNode(FactionSupportRequestIndexNode node)
	{
		int level = node.Level;
		if (LevelSelected != null)
		{
			LevelSelected(level);
		}
	}

	public void Set(Faction faction)
	{
		int level = faction.Level;
		int maxLevel = faction.GetMaxLevel();
		Nodes.BeginLoad();
		for (int i = 0; i < maxLevel; i++)
		{
			FactionSupportRequestIndexNode next = Nodes.GetNext();
			int num = i + 1;
			next.Set(num);
			next.Locked = num > level;
		}
		Nodes.EndLoad();
		Separators.Set((Nodes.Count != 0) ? (Nodes.Count + 1) : 0);
	}
}
