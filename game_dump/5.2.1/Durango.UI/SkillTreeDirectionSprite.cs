using System.Collections.Generic;
using Durango.Logic.Skill;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class SkillTreeDirectionSprite : CustomFillSprite
{
	private struct Node
	{
		public int Depth;

		public int Row;

		public Durango.Logic.Skill.Node Skill;

		public int Parent;

		public int Sibling;

		public int Child;
	}

	[SerializeField]
	private SpriteData _arrowSprite;

	[SerializeField]
	private SpriteData _lineSprite;

	[Range(0f, 1f)]
	[SerializeField]
	private float _beginMargin;

	[Range(0f, 1f)]
	[SerializeField]
	private float _endMargin;

	[Range(0f, 1f)]
	[SerializeField]
	private float _splitPosition;

	[SerializeField]
	private float _lineSize;

	[SerializeField]
	private Vector2 _arrowSize;

	[SerializeField]
	private Color _emphasisColor;

	[SerializeField]
	private Color _normalColor;

	private UISpriteData _arrow;

	private UISpriteData _line;

	private readonly List<Node> _nodes = new List<Node>();

	private readonly Stack<int> _stack = new Stack<int>();

	private Vector2 _basePosition;

	private Point2 _nodeSize;

	public void Clear()
	{
		_nodes.Clear();
	}

	public void Add(Point2 begin, Point2 end, Durango.Logic.Skill.Node targetSkill)
	{
		int num = -1;
		for (int i = 0; i < _nodes.Count; i++)
		{
			Node node = _nodes[i];
			if (node.Depth == begin.x && node.Row == begin.y)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			num = _nodes.Count;
			_nodes.Add(new Node
			{
				Depth = begin.x,
				Row = begin.y,
				Parent = -1,
				Child = -1,
				Sibling = -1
			});
		}
		int count = _nodes.Count;
		_nodes.Add(new Node
		{
			Depth = end.x,
			Row = end.y,
			Skill = targetSkill,
			Parent = num,
			Child = -1,
			Sibling = -1
		});
		Node value = _nodes[num];
		if (value.Child == -1)
		{
			value.Child = count;
		}
		else
		{
			int num2 = value.Child;
			while (num2 != -1)
			{
				Node value2 = _nodes[num2];
				if (value2.Sibling == -1)
				{
					value2.Sibling = count;
					_nodes[num2] = value2;
					break;
				}
				num2 = value2.Sibling;
			}
		}
		_nodes[num] = value;
	}

	public void Draw(Vector2 basePosition, Point2 nodeSize)
	{
		_basePosition = basePosition;
		_nodeSize = nodeSize;
		_arrow = ResourceSingleton<UISpriteManager>.Instance().GetSprite(_arrowSprite.sprite);
		_line = ResourceSingleton<UISpriteManager>.Instance().GetSprite(_lineSprite.sprite);
		mChanged = true;
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		int size = verts.size;
		for (int i = 0; i < _nodes.Count; i++)
		{
			if (_nodes[i].Parent != -1)
			{
				continue;
			}
			_stack.Clear();
			_stack.Push(i);
			while (_stack.Count > 0)
			{
				int index = _stack.Pop();
				Node node = _nodes[index];
				if (node.Sibling != -1)
				{
					_stack.Push(node.Sibling);
				}
				if (node.Child == -1)
				{
					continue;
				}
				_stack.Push(node.Child);
				Node node2 = _nodes[node.Child];
				Vector2 position = GetPosition(_basePosition, _nodeSize, (float)node.Depth + _beginMargin, node.Row);
				if (node2.Sibling == -1)
				{
					Color color = ((node2.Skill.State != Durango.Logic.Skill.State.Learned) ? _normalColor : _emphasisColor);
					Vector2 position2 = GetPosition(_basePosition, _nodeSize, (float)node2.Depth - (1f - _endMargin), node2.Row);
					if (node.Row == node2.Row)
					{
						DrawSprite(verts, uvs, cols, new DrawParam
						{
							Sprite = _line,
							Color = color,
							Position = position,
							Rect = new Rect(0f, 0f, 1f, 1f),
							Pivot = new Vector2(0f, 0.5f),
							Size = new Vector2(position2.x - position.x - _arrowSize.x, _lineSize)
						});
					}
					else
					{
						Vector2 position3 = GetPosition(_basePosition, _nodeSize, (float)node.Depth + _splitPosition, node.Row);
						DrawSprite(verts, uvs, cols, new DrawParam
						{
							Sprite = _line,
							Color = color,
							Position = position,
							Rect = new Rect(0f, 0f, 1f, 1f),
							Pivot = new Vector2(0f, 0.5f),
							Size = new Vector2(position3.x - position.x - _lineSize * 0.5f, _lineSize)
						});
						DrawSprite(verts, uvs, cols, new DrawParam
						{
							Sprite = _line,
							Color = color,
							Position = position3 + new Vector2(0f, _lineSize * 0.5f),
							Rect = new Rect(0f, 0f, 1f, 1f),
							Pivot = new Vector2(0.5f, 1f),
							Size = new Vector2(_lineSize, position.y - position2.y + _lineSize)
						});
						DrawSprite(verts, uvs, cols, new DrawParam
						{
							Sprite = _line,
							Color = color,
							Position = position2 - new Vector2(_arrowSize.x, 0f),
							Rect = new Rect(0f, 0f, 1f, 1f),
							Pivot = new Vector2(1f, 0.5f),
							Size = new Vector2(position2.x - position3.x - _arrowSize.x - _lineSize * 0.5f, _lineSize)
						});
					}
					DrawSprite(verts, uvs, cols, new DrawParam
					{
						Sprite = _arrow,
						Color = color,
						Position = position2,
						Rect = new Rect(0f, 0f, 1f, 1f),
						Pivot = new Vector2(1f, 0.5f),
						Size = _arrowSize
					});
					continue;
				}
				int num = -1;
				int num2 = node.Row;
				Node node3 = node2;
				Vector2 position4 = GetPosition(_basePosition, _nodeSize, (float)node.Depth + _splitPosition, node.Row);
				while (true)
				{
					num2 = Mathf.Max(num2, node3.Row);
					Color color2;
					if (node3.Skill.State == Durango.Logic.Skill.State.Learned)
					{
						color2 = _emphasisColor;
						num = Mathf.Max(num, node3.Row);
					}
					else
					{
						color2 = _normalColor;
					}
					Vector2 position5 = GetPosition(_basePosition, _nodeSize, (float)node3.Depth - (1f - _endMargin), node3.Row);
					DrawSprite(verts, uvs, cols, new DrawParam
					{
						Sprite = _line,
						Color = color2,
						Position = position5 - new Vector2(_arrowSize.x, 0f),
						Rect = new Rect(0f, 0f, 1f, 1f),
						Pivot = new Vector2(1f, 0.5f),
						Size = new Vector2(position5.x - position4.x - _arrowSize.x - _lineSize * 0.5f, _lineSize)
					});
					DrawSprite(verts, uvs, cols, new DrawParam
					{
						Sprite = _arrow,
						Color = color2,
						Position = position5,
						Rect = new Rect(0f, 0f, 1f, 1f),
						Pivot = new Vector2(1f, 0.5f),
						Size = _arrowSize
					});
					if (node3.Sibling == -1)
					{
						break;
					}
					node3 = _nodes[node3.Sibling];
				}
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = _line,
					Color = ((num != -1) ? _emphasisColor : _normalColor),
					Position = position,
					Rect = new Rect(0f, 0f, 1f, 1f),
					Pivot = new Vector2(0f, 0.5f),
					Size = new Vector2(position4.x - position.x - _lineSize * 0.5f, _lineSize)
				});
				if (num == -1)
				{
					DrawSprite(verts, uvs, cols, new DrawParam
					{
						Sprite = _line,
						Color = _normalColor,
						Position = position4 + new Vector2(0f, _lineSize * 0.5f),
						Rect = new Rect(0f, 0f, 1f, 1f),
						Pivot = new Vector2(0.5f, 1f),
						Size = new Vector2(_lineSize, (float)((num2 - node.Row) * _nodeSize.y) + _lineSize)
					});
					continue;
				}
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = _line,
					Color = _emphasisColor,
					Position = position4 + new Vector2(0f, _lineSize * 0.5f),
					Rect = new Rect(0f, 0f, 1f, 1f),
					Pivot = new Vector2(0.5f, 1f),
					Size = new Vector2(_lineSize, (float)((num - node.Row) * _nodeSize.y) + _lineSize)
				});
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = _line,
					Color = _normalColor,
					Position = new Vector2(position4.x, _basePosition.y - (float)(_nodeSize.y * num2) - _lineSize * 0.5f),
					Rect = new Rect(0f, 0f, 1f, 1f),
					Pivot = new Vector2(0.5f, 0f),
					Size = new Vector2(_lineSize, (float)((num2 - num) * _nodeSize.y) + _lineSize)
				});
			}
		}
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}

	private Vector2 GetPosition(Vector2 basePos, Point2 nodeSize, float x, float y)
	{
		return new Vector2(basePos.x + (float)nodeSize.x * x, basePos.y - (float)nodeSize.y * y);
	}
}
