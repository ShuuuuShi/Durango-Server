using System.Runtime.InteropServices;
using Shared.Battle;
using UnityEngine;

public class CharacterDamageableEntity : ComponentBasedDamageableEntity<CharacterBehavior>
{
	public override float XRadius => base.OwnerComponent.XRadius;

	public override float YRadius => base.OwnerComponent.YRadius;

	public CharacterDamageableEntity(CharacterBehavior component)
		: base(component)
	{
	}

	public override Vector3 GetCurrentPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return base.OwnerComponent.CurrentPosition;
	}

	public override ulong GetEntityId()
	{
		return base.OwnerComponent.EntityId;
	}

	public override int GetEntityTypeId()
	{
		return base.OwnerComponent.EntityTypeId;
	}

	public override Gauge GetLife()
	{
		return base.OwnerComponent.Life;
	}

	public override string GetName()
	{
		return base.OwnerComponent.GetName();
	}

	public override int GetLevel()
	{
		return base.OwnerComponent.Level;
	}

	public override void AddLifeGaugeUpdateDelegate()
	{
		base.OwnerComponent.SurvivalGaugeUpdated += OnUpdateTargetLifeGauge;
	}

	public override void RemoveLifeGaugeUpdateDelegate()
	{
		base.OwnerComponent.SurvivalGaugeUpdated -= OnUpdateTargetLifeGauge;
	}

	private void OnUpdateTargetLifeGauge(CharacterBehavior target)
	{
		base.GaugeChanged = true;
	}

	public override Transform GetBodyPartTransform(BodyPart part, bool bAllowNull = false, [Optional] Vector3 nearPos)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return base.OwnerComponent.GetBodyPartTransform(part, bAllowNull, nearPos);
	}
}
