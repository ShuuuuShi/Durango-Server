using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Battle;
using UnityEngine;

namespace Durango.UI.InGame;

public class AreaOfEffectVisualizer : Singleton<AreaOfEffectVisualizer>
{
	public enum Type
	{
		Alert,
		Player,
		Catapult
	}

	private static int _idSequence;

	[EnumList(typeof(Type), false, 0, -1)]
	[SerializeField]
	private AreaOfEffectAlert[] _areaOfEffects;

	public static float Now()
	{
		if (Application.isPlaying)
		{
			return Time.time;
		}
		return Time.realtimeSinceStartup;
	}

	public static int GetNextId()
	{
		return _idSequence++;
	}

	public static int ShowCircle(Type type, Vector3 position, float radius, float startAt, float finishAt, float showAt, float hideAt)
	{
		if (Application.isPlaying && !Singleton<AreaOfEffectVisualizer>.HasInstance())
		{
			return -1;
		}
		if (radius <= 0f)
		{
			return -1;
		}
		if (Mathf.Max(finishAt, hideAt) < Now())
		{
			return -1;
		}
		AreaOfEffectAlert[] alerts = GetAlerts();
		if (alerts == null)
		{
			return -1;
		}
		AreaOfEffectAlert areaOfEffectAlert = alerts[(int)type];
		if (areaOfEffectAlert == null)
		{
			return -1;
		}
		return areaOfEffectAlert.ShowCircle(position, radius, startAt, finishAt, showAt, hideAt);
	}

	public static int ShowArc(Type type, Vector3 position, float radius, float startAngle, float endAngle, float startAt, float finishAt, float showAt, float hideAt)
	{
		if (Application.isPlaying && !Singleton<AreaOfEffectVisualizer>.HasInstance())
		{
			return -1;
		}
		if (radius <= 0f)
		{
			return -1;
		}
		if (Mathf.Max(finishAt, hideAt) < Now())
		{
			return -1;
		}
		AreaOfEffectAlert[] alerts = GetAlerts();
		if (alerts == null)
		{
			return -1;
		}
		AreaOfEffectAlert areaOfEffectAlert = alerts[(int)type];
		if (areaOfEffectAlert == null)
		{
			return -1;
		}
		return areaOfEffectAlert.ShowArc(position, radius, startAngle, endAngle, startAt, finishAt, showAt, hideAt);
	}

	public static int ShowRect(Type type, Vector3 position, float width, float height, float angle, float startAt, float finishAt, float showAt, float hideAt)
	{
		if (Application.isPlaying && !Singleton<AreaOfEffectVisualizer>.HasInstance())
		{
			return -1;
		}
		if (width <= 0f || height <= 0f)
		{
			return -1;
		}
		if (Mathf.Max(finishAt, hideAt) < Now())
		{
			return -1;
		}
		AreaOfEffectAlert[] alerts = GetAlerts();
		if (alerts == null)
		{
			return -1;
		}
		AreaOfEffectAlert areaOfEffectAlert = alerts[(int)type];
		if (areaOfEffectAlert == null)
		{
			return -1;
		}
		return areaOfEffectAlert.ShowRect(position, width, height, angle, startAt, finishAt, showAt, hideAt);
	}

	public static void Stop(int id, float delay = 0f)
	{
		if ((!Application.isPlaying || Singleton<AreaOfEffectVisualizer>.HasInstance()) && id != -1)
		{
			AreaOfEffectAlert[] alerts = GetAlerts();
			int i = 0;
			for (int size = KUtility.GetSize(alerts); i < size; i++)
			{
				alerts[i].Stop(id, delay);
			}
		}
	}

	public static void Move(int id, Vector3 pos)
	{
		if ((!Application.isPlaying || Singleton<AreaOfEffectVisualizer>.HasInstance()) && id != -1)
		{
			AreaOfEffectAlert[] alerts = GetAlerts();
			int i = 0;
			for (int size = KUtility.GetSize(alerts); i < size; i++)
			{
				alerts[i].Move(id, pos);
			}
		}
	}

	private static AreaOfEffectAlert[] GetAlerts()
	{
		if (Singleton<AreaOfEffectVisualizer>.HasInstance())
		{
			return Singleton<AreaOfEffectVisualizer>.Instance()._areaOfEffects;
		}
		if (!Application.isPlaying)
		{
			InGameUIRoot inGameUIRoot = Object.FindObjectOfType<InGameUIRoot>();
			if (inGameUIRoot != null)
			{
				AreaOfEffectVisualizer areaOfEffectVisualizer = inGameUIRoot.MakeTempPrefabObject<AreaOfEffectVisualizer>();
				Singleton<AreaOfEffectVisualizer>.SetInstance(areaOfEffectVisualizer);
				return areaOfEffectVisualizer._areaOfEffects;
			}
		}
		return null;
	}

	private void Start()
	{
		GameSystem<CombatSystem>.Instance().AttackAlerted += delegate(AttackAlerted alerted)
		{
			if (CombatSystem.AttackAlertEnabled)
			{
				ShowAttackAlerted(alerted);
			}
		};
		GameSystem<CombatSystem>.Instance().VehicleProjectileFired += delegate(VehicleProjectileFired fired)
		{
			ShowVehicleProjectileFired(fired);
		};
	}

	public static int ShowVehicleProjectileFired(VehicleProjectileFired fired)
	{
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		bool num = localPlayer != null && localPlayer.Driver.Vehicle != null && localPlayer.Driver.Vehicle.EntityId == fired.EntityId;
		Vector3 position = fired.TargetPosition.ToClientPosition();
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		float time = Time.time;
		float showAt = ((!num) ? (time - (float)(predictedServerTime - fired.WarnSince)) : time);
		float finishAt = time - (float)(predictedServerTime - fired.DmgAt);
		float hideAt = time - (float)(predictedServerTime - fired.WarnUntil);
		return ShowCircle(Type.Catapult, position, fired.DmgRadius, time, finishAt, showAt, hideAt);
	}

	public static int ShowAttackAlerted(AttackAlerted alert)
	{
		return ShowAttackAlerted(Type.Alert, alert);
	}

	public static int ShowAttackAlerted(Type type, AttackAlerted alert)
	{
		WorldPosition? center = alert.Center;
		if (!center.HasValue)
		{
			return -1;
		}
		Vector3 vector = alert.Center.Value.ToClientPosition();
		if (Application.isPlaying && (vector - PlayerBehavior.LocalPlayer.CurrentPosition).magnitude > 2000f)
		{
			return -1;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		float num = Now();
		float num2 = num - (float)(predictedServerTime - alert.EventAt);
		float num3 = num - (float)(predictedServerTime - alert.AttackTime);
		bool num4 = alert.DamageType == DamageType.Melee || alert.DamageType == DamageType.CircularArea || alert.DamageType == DamageType.Ranged;
		bool flag = alert.DamageType == DamageType.RectangularArea;
		bool flag2 = alert.DamageType == DamageType.Melee || alert.DamageType == DamageType.CircularArea;
		float num5 = ((!alert.Yaw.HasValue) ? 0f : alert.Yaw.Value);
		if (num4 && alert.Radius.HasValue && alert.Radius.Value > 0f)
		{
			float startAngle;
			float endAngle;
			if (flag2 && alert.Angles.HasValue)
			{
				startAngle = num5 + alert.Angles.Value.x;
				endAngle = num5 + alert.Angles.Value.y;
			}
			else
			{
				startAngle = 0f;
				endAngle = 360f;
			}
			return ShowArc(type, vector, alert.Radius.Value, startAngle, endAngle, num2, num3, num2, num3);
		}
		if (flag && alert.RectSizeHalves.HasValue)
		{
			Vector2 vector2 = alert.RectSizeHalves.Value * 2f;
			if (vector2.x > 0f && vector2.y > 0f)
			{
				return ShowRect(type, vector, vector2.x, vector2.y, num5, num2, num3, num2, num3);
			}
		}
		return -1;
	}

	[ExposedInEditor(null)]
	public void TestShowRect(Type type, Vector3 position, float width, float height, float angle, float duration)
	{
		duration = ((!(duration > 0f)) ? 3f : duration);
		float num = ((!Application.isPlaying) ? Time.realtimeSinceStartup : Time.time);
		ShowRect(type, position, width, height, angle, num, num + duration, num, num + duration);
	}

	[ExposedInEditor(null)]
	public void TestShowArc(Type type, Vector3 position, float radius, float startAngle, float endAngle, float duration)
	{
		duration = ((!(duration > 0f)) ? 3f : duration);
		float num = ((!Application.isPlaying) ? Time.realtimeSinceStartup : Time.time);
		if (endAngle <= startAngle)
		{
			ShowCircle(type, position, radius, num, num + duration, num, num + duration);
		}
		else
		{
			ShowArc(type, position, radius, startAngle, endAngle, num, num + duration, num, num + duration);
		}
	}
}
