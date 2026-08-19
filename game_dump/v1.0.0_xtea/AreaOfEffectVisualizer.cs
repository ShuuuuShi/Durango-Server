using System.Collections.Generic;
using CombatData;
using UnityEngine;

public class AreaOfEffectVisualizer : MonoBehaviour
{
	[SerializeField]
	private AreaOfEffectSprite _spritePrefab;

	private Stack<AreaOfEffectSprite> _spritePool = new Stack<AreaOfEffectSprite>();

	private void OnEnable()
	{
		GameSystem<CombatSystem>.Instance().AttackAlerted += AttackAlert;
	}

	private void OnDisable()
	{
		GameSystem<CombatSystem>.Instance().AttackAlerted -= AttackAlert;
	}

	private AreaOfEffectSprite GetSprite()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (_spritePool.Count > 0)
		{
			return _spritePool.Pop();
		}
		GameObject val = ((Component)this).gameObject.AddChild(((Component)_spritePrefab).gameObject);
		val.transform.eulerAngles = Vector3.right * 90f;
		AreaOfEffectSprite component = val.GetComponent<AreaOfEffectSprite>();
		component.OnFinished = ReleaseSprite;
		return component;
	}

	private void ReleaseSprite(AreaOfEffectSprite sprite)
	{
		_spritePool.Push(sprite);
	}

	private void AttackAlert(AttackAlert alert)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = alert.Center - PlayerBehavior.LocalPlayer.CurrentPosition;
		if (!(((Vector3)(ref val)).magnitude > 2000f))
		{
			float duration = (float)(alert.At - Connections.Frontend.GetBufferedServerTime_Enhanced());
			PlaySector(alert.Center, alert.Radius, alert.RectSizeHalves, alert.Yaw, alert.Angle.x, alert.Angle.y, duration);
		}
	}

	public void PlayCircle(Vector3 pos, int radius, float yaw, float duration)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		PlaySector(pos, radius, Vector2.zero, yaw, 0, 0, duration);
	}

	public void PlaySector(Vector3 pos, int radius, Vector2 rectSizeHalves, float yaw, int startAngle, int endAngle, float duration)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((Vector2)(ref rectSizeHalves)).magnitude > Mathf.Epsilon)
		{
			GetSprite().Play(pos, (int)(rectSizeHalves.x * 2f), (int)(rectSizeHalves.y * 2f), startAngle, endAngle, UIWidget.Pivot.Center, yaw, duration, isRectangle: true);
		}
		else
		{
			GetSprite().Play(pos, radius * 2, radius * 2, startAngle, endAngle, UIWidget.Pivot.Center, yaw, duration);
		}
	}
}
