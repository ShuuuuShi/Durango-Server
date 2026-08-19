using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class RollDecorationSprite : CustomFillSprite
{
	[SerializeField]
	private float _width = 2f;

	[SerializeField]
	private float _height = 2f;

	[SerializeField]
	private float _distance = 50f;

	[SerializeField]
	private bool _useRadialGradient;

	[SerializeField]
	private bool _useDefaultAlpha = true;

	[SerializeField]
	private Gradient _gradient;

	[SerializeField]
	private float _rotateSpeed;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying && Mathf.Abs(_rotateSpeed) > 0f)
		{
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			localEulerAngles.z += _rotateSpeed * Time.deltaTime;
			base.transform.localEulerAngles = localEulerAngles;
		}
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		UISpriteData atlasSprite = GetAtlasSprite();
		if (atlasSprite == null || _distance < 1f || _width < 1f || _height < 1f)
		{
			return;
		}
		float num = (float)Mathf.Min(base.width, base.height) * 0.5f;
		int num2 = Mathf.RoundToInt((float)Math.PI * 2f * num / _distance);
		if (num2 > 0)
		{
			BetterList<Vector3> verts = arguments.verts;
			BetterList<Vector2> uvs = arguments.uvs;
			BetterList<Color> cols = arguments.cols;
			int size = verts.size;
			for (int i = 0; i < num2; i++)
			{
				float num3 = 360f * (float)i / (float)num2;
				Vector2 position = new Vector2(Mathf.Cos(num3 * ((float)Math.PI / 180f)), Mathf.Sin(num3 * ((float)Math.PI / 180f))) * num;
				Color color = ((!_useRadialGradient) ? this.color : _gradient.Evaluate((float)i / (float)num2));
				color.a = ((!_useDefaultAlpha) ? 1f : this.color.a);
				DrawSprite(verts, uvs, cols, new DrawParam
				{
					Sprite = atlasSprite,
					Angle = num3 - 90f,
					Color = color,
					Pivot = new Vector2(0.5f, 0.5f),
					Position = position,
					Rect = new Rect(0f, 0f, 1f, 1f),
					Size = new Vector2(_width, _height)
				});
			}
			if (onPostFill != null)
			{
				onPostFill(this, size, arguments);
			}
		}
	}

	public void SetRotateSpeed(float speed)
	{
		float num = (float)Mathf.Min(base.width, base.height) * 0.5f;
		_rotateSpeed = 360f * (speed / ((float)Math.PI * 2f * num));
	}
}
