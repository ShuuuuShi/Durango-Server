using JetBrains.Annotations;
using UnityEngine;

public class AdditiveSpriteModifier : MonoBehaviour
{
	private float _ratio;

	private float _offset;

	private float _direction = 1f;

	private tk2dSprite _sprite;

	public void Initialize([NotNull] tk2dSprite sprite)
	{
		_offset = Random.Range(KSingleton<SpriteManager>.Instance().AdditiveFrequency, 0f - Mathf.Epsilon);
		_ratio = _offset;
		_sprite = sprite;
		UpdateAlpha();
	}

	private void Update()
	{
		UpdateAlpha();
	}

	private void UpdateAlpha()
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		SpriteManager spriteManager = KSingleton<SpriteManager>.Instance();
		bool flag = TimeGauge.CheckTime(spriteManager.AdditiveBeginTime, spriteManager.AdditiveEndTime);
		float num = ((!flag) ? spriteManager.AdditiveMinInDay : spriteManager.AdditiveMinInNight);
		float num2 = ((!flag) ? spriteManager.AdditiveMaxInDay : spriteManager.AdditiveMaxInNight);
		if (_ratio > num2)
		{
			_direction = -1f;
		}
		else if (_ratio < num)
		{
			_direction = 1f;
		}
		float num3 = num2 - num;
		_ratio += Time.deltaTime * spriteManager.AdditiveFrequency * _direction * num3;
		float num4 = Mathf.Clamp01(_ratio);
		_sprite.color = new Color(1f, 1f, 1f, num4);
	}
}
