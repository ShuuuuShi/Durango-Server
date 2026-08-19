using Shared.Etc;
using UnityEngine;

public class Trap : ArtifactComponent
{
	private TrapBase _trapBase;

	private bool _isConstruct;

	private TrapBase TrapBase
	{
		get
		{
			if ((Object)(object)_trapBase == (Object)null)
			{
				_trapBase = ((Component)base.Artifact).GetComponentInChildren<TrapBase>();
			}
			return _trapBase;
		}
	}

	protected override bool HasShadow => false;

	public override void PostInit(string artifactId, int worldTileX, int worldTileY, Rotation rotation, Point2 size)
	{
		((Component)base.Artifact).gameObject.tag = "Trap";
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
			if (Object.op_Implicit((Object)(object)TrapBase))
			{
				TrapBase.OnConstruct();
			}
		}
	}

	public override bool OnUpdateState(double eventTime)
	{
		base.OnUpdateState(eventTime);
		float delay = (float)(eventTime - Connections.Frontend.GetBufferedServerTime_Enhanced());
		KUtility.DelayedCall((MonoBehaviour)(object)base.Artifact, delegate
		{
			bool flag = false;
			bool flag2 = false;
			if (base.Artifact.ArtifactState.Trap.HasValue)
			{
				flag = base.Artifact.ArtifactState.Trap.Value.Broken;
				flag2 = base.Artifact.ArtifactState.Trap.Value.Trapped;
			}
			if (Object.op_Implicit((Object)(object)TrapBase))
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
