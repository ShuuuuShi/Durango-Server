using System.Runtime.InteropServices;
using Shared.Battle;
using UnityEngine;

public class ArtifactDamageableEntity : ComponentBasedDamageableEntity<Artifact>
{
	public override float XRadius => (float)(base.OwnerComponent.Size.x * 200) * 0.5f;

	public override float YRadius => (float)(base.OwnerComponent.Size.y * 200) * 0.5f;

	public ArtifactDamageableEntity(Artifact component)
		: base(component)
	{
	}

	public override Vector3 GetCurrentPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return base.OwnerComponent.Center;
	}

	public override ulong GetEntityId()
	{
		return base.OwnerComponent.EntityId;
	}

	public override int GetEntityTypeId()
	{
		return base.OwnerComponent.EntityType;
	}

	public override Gauge GetLife()
	{
		return base.OwnerComponent.Durability;
	}

	public override string GetName()
	{
		return base.OwnerComponent.LocalizedName;
	}

	public override int GetLevel()
	{
		return base.OwnerComponent.ArtifactState.Level;
	}

	public override float GetGaugeScale()
	{
		return base.OwnerComponent.MaxHealth / base.OwnerComponent.GetLife().Max();
	}

	public override void AddLifeGaugeUpdateDelegate()
	{
		base.OwnerComponent.DurabilityGaugeUpdated += OnUpdateTargetLifeGauge;
	}

	public override void RemoveLifeGaugeUpdateDelegate()
	{
		base.OwnerComponent.DurabilityGaugeUpdated -= OnUpdateTargetLifeGauge;
	}

	private void OnUpdateTargetLifeGauge(Artifact target)
	{
		base.GaugeChanged = true;
	}

	public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, [Optional] Vector3 nearPos)
	{
		return ((Component)base.OwnerComponent).transform.GetChild(0);
	}
}
