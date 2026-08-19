using System.Collections.Generic;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.Development;

public class TileLabel : Singleton<TileLabel>
{
	public struct TileLabelStruct
	{
		public Point2 Tile;

		public UILabel Label;

		public float Until;
	}

	[SerializeField]
	private UIPanel _panel;

	[SerializeField]
	private UIFont _font;

	private List<TileLabelStruct> _labels = new List<TileLabelStruct>();

	public void Show(Point2 tilePos, string str, float duration = 5f, float angle = 45f)
	{
		int num = Indexof(tilePos);
		if (num == -1)
		{
			Vector3 position = Util.WorldPositionToClientPosition(Util.TilePositionToWorldPosition(tilePos));
			position += new Vector3(0.5f, 0f, 0.5f) * 200f;
			UILabel uILabel = _panel.gameObject.AddChild<UILabel>();
			uILabel.trueTypeFont = _font.dynamicFont;
			uILabel.pivot = UIWidget.Pivot.Center;
			uILabel.fontSize = 20;
			uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
			uILabel.text = str;
			uILabel.transform.position = position;
			uILabel.transform.localEulerAngles = new Vector3(angle, 45f, 0f);
			uILabel.alpha = 0f;
			TweenAlpha.Begin(uILabel.gameObject, 0.5f, 1f);
			_labels.Add(new TileLabelStruct
			{
				Tile = tilePos,
				Label = uILabel,
				Until = Time.time + duration
			});
		}
		else
		{
			TileLabelStruct value = _labels[num];
			value.Label.text = str;
			value.Until = Time.time + duration;
			_labels[num] = value;
		}
	}

	private void Update()
	{
		float time = Time.time;
		int i = 0;
		for (int count = _labels.Count; i < count; i++)
		{
			if (_labels[i].Until < time)
			{
				DestoryLabel(_labels[i].Label);
				_labels.RemoveAt(i);
				break;
			}
		}
	}

	private void DestoryLabel(UILabel label)
	{
		TweenAlpha tweenAlpha = TweenAlpha.Begin(label.gameObject, 0.5f, 0f);
		tweenAlpha.SetOnFinished(delegate
		{
			Object.Destroy(label.gameObject);
		});
	}

	private int Indexof(Point2 tile)
	{
		int i = 0;
		for (int count = _labels.Count; i < count; i++)
		{
			if (_labels[i].Tile == tile)
			{
				return i;
			}
		}
		return -1;
	}
}
