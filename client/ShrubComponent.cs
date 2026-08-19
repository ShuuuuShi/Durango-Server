using System.Collections;
using Durango.Environment;
using Durango.Render.Sprite;
using Durango.Utils;
using UnityEngine;

public class ShrubComponent : NaturalComponent
{
	private Vector3[] _shakenVertices;

	private bool _isWindy;

	private float _curWindTime;

	private float _curOffset;

	private bool _isShaking;

	public ShrubComponent(NaturalSpriteObject natural)
		: base(natural)
	{
	}

	public void RefreshShakenVertices()
	{
		_shakenVertices = null;
	}

	public void Shake(bool shake)
	{
		if (shake)
		{
			PrepareVertices();
		}
		if (KUtility.GetSize(_shakenVertices) == 0)
		{
			return;
		}
		if (!shake && _isShaking)
		{
			_isShaking = false;
			Vector3[] baseVertices = base.Sprite.GetBaseVertices();
			if (baseVertices != null)
			{
				base.Sprite.SetMeshVertices(baseVertices);
				int num = baseVertices.Length;
				for (int i = 0; i < num; i++)
				{
					ref Vector3 reference = ref _shakenVertices[i];
					reference = baseVertices[i];
				}
			}
		}
		else if (shake)
		{
			_isShaking = true;
			float f = Time.time * Singleton<SpriteManager>.Instance().BushWhackFrequency;
			float num2 = Singleton<SpriteManager>.Instance().BushWhackAmplitude * Mathf.Sin(f);
			for (int j = 0; j < _shakenVertices.Length; j++)
			{
				_shakenVertices[j].x = _shakenVertices[j].x + num2 * Mathf.Max(0f, _shakenVertices[j].y);
			}
			base.Sprite.SetMeshVertices(_shakenVertices);
		}
	}

	private void PrepareVertices()
	{
		if (KUtility.GetSize(_shakenVertices) == 0)
		{
			_shakenVertices = base.Sprite.GetMeshVertices();
		}
	}

	public void Sway(float windTime)
	{
		if (!_isWindy && !_isShaking && base.GameObject.activeSelf)
		{
			PrepareVertices();
			base.Natural.StartCoroutine(CoSway(windTime));
		}
	}

	private IEnumerator CoSway(float windTime)
	{
		_isWindy = true;
		_curWindTime = (0f - (base.GameObject.transform.position.x - PlayerBehavior.LocalPlayer.CurrentPosition.x)) * 0.001f - 1f;
		while (_isWindy)
		{
			if (_curWindTime >= windTime)
			{
				_isWindy = false;
			}
			SwayVertices();
			SetWindFactor(windTime);
			yield return null;
		}
	}

	private void SwayVertices()
	{
		if (_curWindTime < 0f || _isShaking)
		{
			return;
		}
		Vector3[] baseVertices = base.Sprite.GetBaseVertices();
		if (baseVertices != null && KUtility.GetSize(_shakenVertices) != 0)
		{
			for (int i = 0; i < _shakenVertices.Length; i++)
			{
				float windValue = Singleton<WindManager>.Instance().GetWindValue(_curOffset);
				_shakenVertices[i].x = baseVertices[i].x + windValue * Mathf.Max(0f, _shakenVertices[i].y);
			}
			base.Sprite.SetMeshVertices(_shakenVertices);
		}
	}

	private void SetWindFactor(float windTime)
	{
		_curWindTime += Time.deltaTime;
		_curOffset = _curWindTime / windTime;
	}
}
