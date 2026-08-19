using System.Collections;
using UnityEngine;

public class ShrubComponent : NaturalComponent
{
	private Vector3[] _shakenVertices;

	private bool _isWindy;

	private float _curWindTime;

	private float _curOffset;

	private bool _isShaking;

	public ShrubComponent(NaturalObject natural)
		: base(natural)
	{
	}

	public void Shake(bool shake)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
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
			Vector3[] baseVertices = base.KSprite.GetBaseVertices();
			if (baseVertices != null)
			{
				base.KSprite.SetMeshVertices(baseVertices);
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
			float num2 = Time.time * KSingleton<SpriteManager>.Instance().BushWhackFrequency;
			float num3 = KSingleton<SpriteManager>.Instance().BushWhackAmplitude * Mathf.Sin(num2);
			for (int j = 0; j < _shakenVertices.Length; j++)
			{
				_shakenVertices[j].x = _shakenVertices[j].x + num3 * Mathf.Max(0f, _shakenVertices[j].y);
			}
			base.KSprite.SetMeshVertices(_shakenVertices);
		}
	}

	private void PrepareVertices()
	{
		if (KUtility.GetSize(_shakenVertices) == 0)
		{
			_shakenVertices = base.KSprite.GetMeshVertices();
		}
	}

	public void Sway(float windTime)
	{
		if (!_isWindy && !_isShaking && base.GameObject.activeSelf)
		{
			PrepareVertices();
			((MonoBehaviour)base.Natural).StartCoroutine(CoSway(windTime));
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
		Vector3[] baseVertices = base.KSprite.GetBaseVertices();
		if (baseVertices != null && KUtility.GetSize(_shakenVertices) != 0)
		{
			for (int i = 0; i < _shakenVertices.Length; i++)
			{
				float windValue = KSingleton<WindManager>.Instance().GetWindValue(_curOffset);
				_shakenVertices[i].x = baseVertices[i].x + windValue * Mathf.Max(0f, _shakenVertices[i].y);
			}
			base.KSprite.SetMeshVertices(_shakenVertices);
		}
	}

	private void SetWindFactor(float windTime)
	{
		_curWindTime += Time.deltaTime;
		_curOffset = _curWindTime / windTime;
	}
}
