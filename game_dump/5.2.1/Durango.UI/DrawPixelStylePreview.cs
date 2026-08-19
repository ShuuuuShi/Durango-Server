using System.Collections.Generic;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class DrawPixelStylePreview : UIWidget
{
	[SerializeField]
	private UISprite _boxPrefab;

	[SerializeField]
	private UISprite _previewAsIcon;

	private ListObjectPool<UISprite> _boxPool;

	private HashSet<Point2> _nodeInstance = new HashSet<Point2>(default(CordinateComparer));

	[SerializeField]
	private Color _eraserPreviewColor = Color.white;

	public ListObjectPool<UISprite> BoxPool
	{
		get
		{
			if (_boxPool == null)
			{
				_boxPool = new ListObjectPool<UISprite>();
				_boxPool.BaseObject = _boxPrefab;
			}
			return _boxPool;
		}
	}

	public void SetColor(ToolDatum data, Color col)
	{
		Color color = ((data.Tool != ToolType.Eraser) ? col : Color.white);
		_previewAsIcon.color = color;
		foreach (UISprite item in BoxPool)
		{
			item.color = color;
		}
	}

	public void ShowPreview([NotNull] ToolDatum data, Color targetColor)
	{
		if (!data.HasNodeStylePreview)
		{
			_previewAsIcon.gameObject.SetActive(value: true);
			_previewAsIcon.spriteName = data.IconKey;
			_previewAsIcon.color = targetColor;
			BoxPool.Clear();
			return;
		}
		_previewAsIcon.gameObject.SetActive(value: false);
		_nodeInstance.Clear();
		_nodeInstance.AddRange(DrawExtension.GetNode(data));
		Point2 contourSquareSize = DrawExtension.GetContourSquareSize(_nodeInstance);
		BoxPool.BeginLoad();
		int num = _boxPool.BaseObject.width;
		int num2 = _boxPool.BaseObject.height;
		for (int i = 0; i < contourSquareSize.y; i++)
		{
			for (int j = 0; j < contourSquareSize.x; j++)
			{
				Point2 item = new Point2(j - contourSquareSize.x / 2, i - contourSquareSize.y / 2);
				bool flag = _nodeInstance.Contains(item);
				if (flag || data.Tool == ToolType.Brush)
				{
					UISprite next = _boxPool.GetNext();
					if (flag && data.Tool == ToolType.Eraser)
					{
						next.spriteName = "img_square_dotted";
						next.color = _eraserPreviewColor;
					}
					else
					{
						next.spriteName = ((!flag) ? "img_square_dotted" : "bg_white_small");
						next.color = targetColor;
					}
					next.transform.localPosition = new Vector3((2 * j - contourSquareSize.x + 1) * num / 2 + item.x, (2 * i - contourSquareSize.y + 1) * num2 / 2 + item.y);
				}
			}
		}
		BoxPool.EndLoad();
	}
}
