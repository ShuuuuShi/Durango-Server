using Durango.Network;
using Messages;

public class Trap : ArtifactComponent
{
	private TrapBase _trapBase;

	private bool _isConstruct;

	private TrapBase TrapBase
	{
		get
		{
			if (_trapBase == null)
			{
				_trapBase = base.Artifact.GetComponentInChildren<TrapBase>();
			}
			return _trapBase;
		}
	}

	public override void OnCompleted()
	{
		_isConstruct = true;
		OnConstruct();
	}

	public override void ResourcesLoadCompleted()
	{
		OnConstruct();
	}

	private void OnConstruct()
	{
		if (_isConstruct)
		{
			_isConstruct = false;
			if ((bool)TrapBase)
			{
				TrapBase.OnConstruct();
			}
		}
	}

	public override bool OnUpdateState(double eventTime)
	{
		base.OnUpdateState(eventTime);
		ArtifactState artifactState = base.Artifact.ArtifactState;
		if (!artifactState.Trap.HasValue || (!artifactState.Trap.Value.Broken && !artifactState.Trap.Value.Trapped))
		{
			return false;
		}
		float delay = (float)(eventTime - Connections.Frontend.GetBufferedServerTime());
		KUtility.DelayedCall(base.Artifact, delegate
		{
			bool flag = false;
			bool flag2 = false;
			if (base.Artifact.ArtifactState.Trap.HasValue)
			{
				flag = base.Artifact.ArtifactState.Trap.Value.Broken;
				flag2 = base.Artifact.ArtifactState.Trap.Value.Trapped;
			}
			if ((bool)TrapBase)
			{
				if (flag)
				{
					TrapBase.OnBreak();
				}
				else if (flag2)
				{
					TrapBase.OnTrapped();
				}
			}
		}, delay);
		return false;
	}
}
