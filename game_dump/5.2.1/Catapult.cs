using Messages;

public class Catapult : ArtifactComponent
{
	private VehicleCatapult _vehicleCatapult;

	private float _yawAtAppear;

	public VehicleProjectileFired LastProjectile { get; private set; }

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		if (msg.Yaw.HasValue)
		{
			TurnToYaw(msg.Yaw.Value);
			_yawAtAppear = msg.Yaw.Value;
		}
		return false;
	}

	public override bool OnUpdateState(double eventTime)
	{
		if (_vehicleCatapult != null)
		{
			_vehicleCatapult.UpdateRemainedProjectiles();
		}
		return false;
	}

	public override void ResourcesLoadCompleted()
	{
		_vehicleCatapult = base.Artifact.GetComponentInChildren<VehicleCatapult>();
		TurnToYaw(_yawAtAppear);
	}

	private void TurnToYaw(float yaw)
	{
		if (_vehicleCatapult != null)
		{
			_vehicleCatapult.TurnToYaw(yaw);
		}
	}

	public void FireProjectile(VehicleProjectileFired msg)
	{
		LastProjectile = msg;
		if (!(_vehicleCatapult == null))
		{
			_vehicleCatapult.FireProjectile(msg);
		}
	}
}
