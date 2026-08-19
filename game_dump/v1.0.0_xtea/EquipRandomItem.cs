using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Equip Random Item")]
public class EquipRandomItem : MonoBehaviour
{
	public InvEquipment equipment;

	private void OnClick()
	{
		if (!((Object)(object)equipment == (Object)null))
		{
			List<InvBaseItem> items = InvDatabase.list[0].items;
			if (items.Count != 0)
			{
				int num = 12;
				int num2 = Random.Range(0, items.Count);
				InvBaseItem invBaseItem = items[num2];
				InvGameItem invGameItem = new InvGameItem(num2, invBaseItem);
				invGameItem.quality = (InvGameItem.Quality)Random.Range(0, num);
				invGameItem.itemLevel = NGUITools.RandomRange(invBaseItem.minItemLevel, invBaseItem.maxItemLevel);
				equipment.Equip(invGameItem);
			}
		}
	}
}
