using System.Collections.Generic;
using UnityEngine;

public class TileLabel : KSingleton<TileLabel>
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
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		int num = Indexof(tilePos);
		if (num == -1)
		{
			Vector3 val = TerrainA6.WorldPositionToClientPosition(TerrainA6.TilePositionToWorldPosition(tilePos));
			val += new Vector3(0.5f, 0f, 0.5f) * 200f;
			UILabel uILabel = ((Component)_panel).gameObject.AddChild<UILabel>();
			uILabel.trueTypeFont = _font.dynamicFont;
			uILabel.pivot = UIWidget.Pivot.Center;
			uILabel.fontSize = 20;
			uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
			uILabel.text = str;
			((Component)uILabel).transform.position = val;
			((Component)uILabel).transform.localEulerAngles = new Vector3(angle, 45f, 0f);
			uILabel.alpha = 0f;
			TweenAlpha.Begin(((Component)uILabel).gameObject, 0.5f, 1f);
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
		TweenAlpha tweenAlpha = TweenAlpha.Begin(((Component)label).gameObject, 0.5f, 0f);
		tweenAlpha.SetOnFinished(delegate
		{
			Object.Destroy((Object)(object)((Component)label).gameObject);
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
