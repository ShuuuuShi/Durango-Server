using System.Collections.Generic;
using BuildData;
using UnityEngine;

public class RepairGroup : UIBase
{
	[SerializeField]
	private RepairWidget _repairWidget;

	private void Awake()
	{
		_repairWidget.Init();
		OnClose();
	}

	private void Start()
	{
		_repairWidget.ReadyToRepair = ReadyToRepair;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void ReadyToRepair(Artifact artifact, IList<RepairSlot> repairSlots)
	{
		Dictionary<string, ulong[]> dictionary = new Dictionary<string, ulong[]>();
		int i = 0;
		for (int count = repairSlots.Count; i < count; i++)
		{
			if (repairSlots[i].selectItems.Count != 0)
			{
				ulong[] array = new ulong[repairSlots[i].selectItems.Count];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = repairSlots[i].selectItems[j].Id;
				}
				dictionary.Add(repairSlots[i].key, array);
			}
		}
		ArtifactRepair(artifact, dictionary);
	}

	private void ArtifactRepair(Artifact artifact, Dictionary<string, ulong[]> materials)
	{
		PutRepairMaterials(artifact, materials);
		StartRepair(artifact);
	}

	private void PutRepairMaterials(Artifact artifact, Dictionary<string, ulong[]> materials)
	{
		if (materials != null && materials.Count != 0)
		{
		}
	}

	private void StartRepair(Artifact artifact)
	{
	}
}
