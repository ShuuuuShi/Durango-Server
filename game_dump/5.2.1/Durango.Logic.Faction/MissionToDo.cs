using Durango.Logic.PlayGuide;
using Durango.Network;
using L10N;
using Shared.Faction;
using UnityEngine;

namespace Durango.Logic.Faction;

public class MissionToDo : ToDoBase
{
	public double? ExpiresAt;

	public string Id;

	public string DisplayText;

	public Vector2 TargetTile = Vector2.zero;

	public FactionType FactionType;

	public MissionTodoType MissionType;

	public MissionTodoOrder MissionOrder;

	private float _lastUpdatedAt;

	public override void Process()
	{
		if (!ExpiresAt.HasValue)
		{
			base.LocalText = DisplayText;
			return;
		}
		float time = Time.time;
		if (!(time - _lastUpdatedAt < 1f))
		{
			_lastUpdatedAt = time;
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			if (predictedServerTime > ExpiresAt.Value)
			{
				ExpiresAt = null;
				return;
			}
			base.LocalText = T._("{0}\n남은 시간 {1}", DisplayText, TimedeltaFormatter.ColonFormat(ExpiresAt.Value - predictedServerTime));
			GameSystem<ToDoListSystem>.Instance().SetUpdated(this, textOnly: true);
		}
	}

	public bool HasTile()
	{
		return TargetTile != Vector2.zero;
	}
}
