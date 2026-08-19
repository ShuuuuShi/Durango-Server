public abstract class VehicleProp : VehicleBase
{
	private bool _isFoundArtifact;

	private Artifact _artifact;

	public override string EntityId
	{
		get
		{
			Artifact artifact = GetArtifact();
			if (artifact != null)
			{
				return artifact.EntityId;
			}
			return base.EntityId;
		}
	}

	public abstract bool IsInteractionMenuVisible { get; }

	public Artifact GetArtifact()
	{
		if (!_isFoundArtifact)
		{
			_isFoundArtifact = true;
			_artifact = GetComponentInParent<Artifact>();
		}
		return _artifact;
	}

	private void OnDestroy()
	{
		if (!GameManager.IsSceneClosing && base.Driver != null && base.Driver.Vehicle == this)
		{
			base.Driver.Unmount(null, immediately: true);
		}
	}
}
