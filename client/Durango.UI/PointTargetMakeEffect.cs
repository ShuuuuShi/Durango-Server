using UnityEngine;

namespace Durango.UI;

public class PointTargetMakeEffect : UISprite
{
	[SerializeField]
	private float _duraiton = 4f;

	[SerializeField]
	private int _effectCount = 6;

	[SerializeField]
	private float _effectPeriod = 0.2f;

	[SerializeField]
	private float _fromScale = 0.5f;

	private float _delay;

	private float _animationRatio = 1f;

	protected override void OnDisable()
	{
		base.OnDisable();
		_animationRatio = 1f;
	}

	[ExposedInEditor(null)]
	public void Play(float delay = 0f)
	{
		_delay = delay;
		_animationRatio = 0f;
		base.enabled = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (_animationRatio >= 1f)
		{
			base.enabled = false;
			return;
		}
		if (_delay > 0f)
		{
			_delay -= Time.deltaTime;
			return;
		}
		_animationRatio += Time.deltaTime / _duraiton;
		mChanged = true;
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		if (!Application.isPlaying || _animationRatio >= 1f || _delay > 0f)
		{
			return;
		}
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Vector2> uvs = arguments.uvs;
		BetterList<Color> cols = arguments.cols;
		int size = verts.size;
		base.OnFill(arguments);
		Vector3 a = base.localCenter;
		int size2 = verts.size;
		float num = (1f - _effectPeriod) / (float)_effectCount;
		for (int i = 0; i < _effectCount; i++)
		{
			float num2 = num * (float)i;
			float num3 = (_animationRatio - num2) / _effectPeriod;
			if (!(num3 < 0f) && !(num3 >= 1f))
			{
				num3 = 1f - num3;
				num3 *= num3;
				num3 = 1f - num3;
				float t = Mathf.Lerp(_fromScale, 1f, num3);
				float num4 = Mathf.Abs(num3 - 0.5f);
				num4 /= 0.5f;
				num4 *= num4;
				num4 = 1f - num4;
				for (int j = 0; j < size2; j++)
				{
					Vector3 b = verts[j];
					b = Vector3.Lerp(a, b, t);
					Color item = cols[j];
					item.a *= num4;
					verts.Add(b);
					uvs.Add(uvs[j]);
					cols.Add(item);
				}
			}
		}
		verts.RemoveRange(0, size2);
		uvs.RemoveRange(0, size2);
		cols.RemoveRange(0, size2);
		if (onPostFill != null)
		{
			onPostFill(this, size, arguments);
		}
	}
}
