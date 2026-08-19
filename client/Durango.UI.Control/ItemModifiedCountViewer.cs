using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI.Control;

public class ItemModifiedCountViewer : CustomFillSprite
{
	private enum Direction
	{
		Left,
		Right,
		Down,
		Up
	}

	[SerializeField]
	private Direction _direction = Direction.Down;

	[SerializeField]
	private float _pivotOffset;

	[SerializeField]
	private float _margin = 2f;

	private int _count;

	public void Set(int count)
	{
		if (_count != count)
		{
			_count = count;
			mChanged = true;
		}
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		if (!Application.isPlaying)
		{
			base.OnFill(arguments);
		}
		else
		{
			if (_count == 0)
			{
				return;
			}
			UISpriteData atlasSprite = GetAtlasSprite();
			if (atlasSprite != null)
			{
				BetterList<Vector3> verts = arguments.verts;
				BetterList<Vector2> uvs = arguments.uvs;
				BetterList<Color> cols = arguments.cols;
				int size = verts.size;
				Vector3 vector = GetDirection(_direction);
				float size2 = UIUtility.GetSize(localSize, vector);
				float num = size2 * (float)_count + _margin * (float)(_count - 1);
				Vector3 vector2 = base.localCenter - vector * (num - size2) * _pivotOffset;
				Color color = this.color.WithA(1f);
				for (int i = 0; i < _count; i++)
				{
					DrawSprite(verts, uvs, cols, new DrawParam
					{
						Sprite = atlasSprite,
						Color = color,
						Size = localSize,
						Rect = new Rect(0f, 0f, 1f, 1f),
						Pivot = new Vector2(0.5f, 0.5f),
						Position = vector2 + vector * i * (size2 + _margin)
					});
				}
				if (onPostFill != null)
				{
					onPostFill(this, size, arguments);
				}
			}
		}
	}

	private static Vector2 GetDirection(Direction dir)
	{
		return dir switch
		{
			Direction.Left => Vector2.left, 
			Direction.Right => Vector2.right, 
			Direction.Down => Vector2.down, 
			Direction.Up => Vector2.up, 
			_ => Vector2.zero, 
		};
	}
}
