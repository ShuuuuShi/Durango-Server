using UnityEngine;

[AddComponentMenu("NGUI/Examples/Equipment")]
public class InvEquipment : MonoBehaviour
{
	private InvGameItem[] mItems;

	private InvAttachmentPoint[] mAttachments;

	public InvGameItem[] equippedItems => mItems;

	public InvGameItem Replace(InvBaseItem.Slot slot, InvGameItem item)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		InvBaseItem invBaseItem = item?.baseItem;
		if (slot != 0)
		{
			if (invBaseItem != null && invBaseItem.slot != slot)
			{
				return item;
			}
			if (mItems == null)
			{
				int num = 8;
				mItems = new InvGameItem[num];
			}
			InvGameItem result = mItems[(int)(slot - 1)];
			mItems[(int)(slot - 1)] = item;
			if (mAttachments == null)
			{
				mAttachments = ((Component)this).GetComponentsInChildren<InvAttachmentPoint>();
			}
			int i = 0;
			for (int num2 = mAttachments.Length; i < num2; i++)
			{
				InvAttachmentPoint invAttachmentPoint = mAttachments[i];
				if (invAttachmentPoint.slot != slot)
				{
					continue;
				}
				GameObject val = invAttachmentPoint.Attach(invBaseItem?.attachment);
				if (invBaseItem != null && (Object)(object)val != (Object)null)
				{
					Renderer component = val.GetComponent<Renderer>();
					if ((Object)(object)component != (Object)null)
					{
						component.material.color = invBaseItem.color;
					}
				}
			}
			return result;
		}
		if (item != null)
		{
		}
		return item;
	}

	public InvGameItem Equip(InvGameItem item)
	{
		if (item != null)
		{
			InvBaseItem baseItem = item.baseItem;
			if (baseItem != null)
			{
				return Replace(baseItem.slot, item);
			}
		}
		return item;
	}

	public InvGameItem Unequip(InvGameItem item)
	{
		if (item != null)
		{
			InvBaseItem baseItem = item.baseItem;
			if (baseItem != null)
			{
				return Replace(baseItem.slot, null);
			}
		}
		return item;
	}

	public InvGameItem Unequip(InvBaseItem.Slot slot)
	{
		return Replace(slot, null);
	}

	public bool HasEquipped(InvGameItem item)
	{
		if (mItems != null)
		{
			int i = 0;
			for (int num = mItems.Length; i < num; i++)
			{
				if (mItems[i] == item)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasEquipped(InvBaseItem.Slot slot)
	{
		if (mItems != null)
		{
			int i = 0;
			for (int num = mItems.Length; i < num; i++)
			{
				InvBaseItem baseItem = mItems[i].baseItem;
				if (baseItem != null && baseItem.slot == slot)
				{
					return true;
				}
			}
		}
		return false;
	}

	public InvGameItem GetItem(InvBaseItem.Slot slot)
	{
		if (slot != 0)
		{
			int num = (int)(slot - 1);
			if (mItems != null && num < mItems.Length)
			{
				return mItems[num];
			}
		}
		return null;
	}
}
