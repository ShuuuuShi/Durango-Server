using System;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.UI;
using Messages;
using Shared.Accelerator;
using UnityEngine;

public class WarpAccelerator : ArtifactComponent
{
	private Durango.Logic.Timer.Timer _waitTimer;

	public override bool OnUpdateState(double eventTime)
	{
		UpdateTimer();
		return base.OnUpdateState(eventTime);
	}

	private void SetWaitTimer(double since, double until)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (since <= 0.0)
		{
			since = predictedServerTime;
		}
		if (until <= predictedServerTime || since >= until)
		{
			if (_waitTimer != null)
			{
				_waitTimer.Stop();
			}
			return;
		}
		float num = (float)(until - since);
		float ratio = (float)(predictedServerTime - since) / num;
		if (_waitTimer == null || _waitTimer.IsStop)
		{
			_waitTimer = new Durango.Logic.Timer.Timer(base.EntityId, "warp_accelerator_wait", num, ratio);
			Durango.Logic.Timer.Timer.Play<TimerProgressGauge>(_waitTimer).SetTarget(base.Artifact.gameObject, new Vector3(base.Size.x, 2f, base.Size.y) * 200f * 0.5f);
		}
		else
		{
			_waitTimer.SetDuration(base.EntityId, "warp_accelerator_wait", num, ratio);
		}
	}

	private void UpdateTimer()
	{
		Messages.WarpAccelerator? warpaccelerator = base.Artifact.ArtifactState.Warpaccelerator;
		if (!warpaccelerator.HasValue || (warpaccelerator.Value.Participants != null && Array.IndexOf(warpaccelerator.Value.Participants, GameManager.PlayerId) == -1))
		{
			SetWaitTimer(0.0, 0.0);
		}
		else if (warpaccelerator.Value.Status == AcceleratorStatus.Waiting)
		{
			SetWaitTimer(warpaccelerator.Value.StatusSince.GetValueOrDefault(), warpaccelerator.Value.StatusUntil.GetValueOrDefault());
		}
		else
		{
			SetWaitTimer(0.0, 0.0);
		}
	}
}
