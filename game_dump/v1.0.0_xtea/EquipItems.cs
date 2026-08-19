using UnityEngine;

[AddComponentMenu("NGUI/Examples/Equip Items")]
public class EquipItems : MonoBehaviour
{
	public int[] itemIDs;

	private void Start()
	{
		if (itemIDs != null && itemIDs.Length > 0)
		{
			InvEquipment invEquipment = ((Component)this).GetComponent<InvEquipment>();
			if ((Object)(object)invEquipment == (Object)null)
			{
				invEquipment = ((Component)this).gameObject.AddComponent<InvEquipment>();
			}
			int num = 12;
			int i = 0;
			for (int num2 = itemIDs.Length; i < num2; i++)
			{
				int num3 = itemIDs[i];
				InvBaseItem invBaseItem = InvDatabase.FindByID(num3);
				if (invBaseItem != null)
				{
					InvGameItem invGameItem = new InvGameItem(num3, invBaseItem);
					invGameItem.quality = (InvGameItem.Quality)Random.Range(0, num);
					invGameItem.itemLevel = NGUITools.RandomRange(invBaseItem.minItemLevel, invBaseItem.maxItemLevel);
					invEquipment.Equip(invGameItem);
				}
			}
		}
		Object.Destroy((Object)(object)this);
	}
}
