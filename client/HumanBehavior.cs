using Durango.Model;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(CostumableModel))]
[RequireComponent(typeof(CostumableLoader))]
public class HumanBehavior : AnimalBehavior
{
	[SerializeField]
	private bool _loadCostumeOnStart;

	public CostumableModel CostumableModel => GetComponent<CostumableModel>();

	public override Transform WeaponTipTransform => CostumableModel.WeaponTipTransform;

	protected override ChatableBase CreateChatableBase()
	{
		return new ChatableHuman(this);
	}

	protected new void Start()
	{
		base.Start();
		int key = ((base.EntityId != null) ? base.EntityId.GetHashCode() : 0);
		if (_loadCostumeOnStart)
		{
			LoadCostume(key);
		}
	}

	public void LoadCostume(int key)
	{
		GetComponent<CostumableLoader>().LoadCostume(base.EntityTypeId, key);
		if (base.ChatableBase is ChatableHuman chatableHuman)
		{
			chatableHuman.RefreshPortrait(key);
		}
	}

	[UsedImplicitly]
	private void OnAttack()
	{
	}
}
